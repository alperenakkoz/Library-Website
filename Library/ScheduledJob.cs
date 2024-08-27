
using Library.Models;
using Microsoft.EntityFrameworkCore;

namespace Library
{
    public class ScheduledJob : BackgroundService
    {
        private readonly IServiceProvider _services;

        public ScheduledJob(IServiceProvider services)
        {
            _services = services; //needed for creating scope
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                //the code you write here probably won't work. Make a function
                await DeletePastReserves();
                await SentNotification(); //Sends notification if books are overdue
                await Task.Delay(TimeSpan.FromDays(1), stoppingToken); // Run daily
            }
        }

        private async Task DeletePastReserves()
        {
            using (var scope = _services.CreateScope()) //Scoped service means one instance per HTTP request
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>(); //get db

                var today = DateTime.Today;

                var expiredHolds = await dbContext.Hold.Where(h => h.EndTime.Date < today.Date).ToListAsync();
                foreach(var hold in expiredHolds)
                {
                    WaitList waitListUser = await dbContext.WaitList.OrderBy(x => x.CreateDate).FirstOrDefaultAsync(x => x.BooksId == hold.BooksId);
                    if (waitListUser != null) 
                    {
                        //if there is a record in waitlist, it will take the reserve
                        Hold holdUser = new ()
                        {
                            LibraryUserId = waitListUser.LibraryUserId,
                            BooksId = waitListUser.BooksId,
                            StartTime = DateTime.Now,
                            EndTime = DateTime.Now.AddDays(2)
                        };
                        dbContext.Add(holdUser);
                        dbContext.Remove(waitListUser);
                    }
                    else
                    {
                        (await dbContext.Books.FirstOrDefaultAsync(x => x.Id == hold.BooksId)).numberOfBooks += 1;
                    }
                }
                dbContext.Hold.RemoveRange(expiredHolds);

                await dbContext.SaveChangesAsync();
            }
        }

        private async Task SentNotification()
        {
            using (var scope = _services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                var emailSender = scope.ServiceProvider.GetRequiredService<ISenderEmail>(); // Inject ISenderEmail service

                var today = DateTime.Today;
                
                var expiredRents =  dbContext.Rent
                    .Where(h => h.EndTime.Date < today.Date
                    && !h.IsReturned);
                var expiredRentUserIds = await expiredRents.Select(rent => rent.LibraryUserId)
                    .Distinct()
                    .ToListAsync();
                foreach (var userId in expiredRentUserIds)
                {
                    var latestNotification = await dbContext.Notification.OrderByDescending(x => x.SentAt).FirstOrDefaultAsync(x => x.LibraryUserId == userId);
                    if (latestNotification != null && latestNotification.SentAt.Date.AddDays(2) >= today.Date)
                        continue;
                    List<string> bookNames = await dbContext.Books.Where(b => expiredRents.Any(x => x.LibraryUserId == userId && x.BooksId == b.Id))
                                                                  .Select(b => b.Title).ToListAsync();
                    var notification = new Notification
                    {
                        SentAt = DateTime.Now,
                        Message = "You passed the return dates of the following books : " + string.Join(", ", bookNames),
                        LibraryUserId = userId,
                    };
                    var user = await dbContext.Users.FirstOrDefaultAsync(u => u.Id == userId);
                    if (user != null)
                    {
                        string emailBody = notification.Message; // Use notification message as email body
                        await emailSender.SendEmailAsync(user.Email, "Overdue Book Notification", emailBody, true); // Send email
                    }
                    dbContext.Add(notification);
                }
                await dbContext.SaveChangesAsync();
            }
        }
    }
}
