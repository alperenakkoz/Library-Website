using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace Library.Models;

// Add profile data for application users by adding properties to the LibraryUser class
public class LibraryUser : IdentityUser
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public List<WaitList> WaitList { get; set; }
    public List<Rent> Rent { get; set; }
    public List<Hold> Hold { get; set; }
    public List<Notification> Notification { get; set; }
    public List<Comments> Comments { get; set; }
}

