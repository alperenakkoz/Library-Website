<h1>Used Technologies and Languages</h1>
<ul>
  <li>Visual Studio 2022</li>
  <li>Microsoft SQL Server</li>
  <li>Asp.NET MVC 8 (Multi language, Identity, Hosted Service, E-mail)</li>
  <li>JavaScript (AJAX, JQuery)</li>
  <li>HTML</li>
  <li>Boostrap 5</li>
</ul>

<h2>What can a user do?</h2>
<ul>
  <li>Register, Login, Logout</li>
  <li>Choose the site's language</li>
  <li>Change credentials</li>
  <li>Favorite a book</li>
  <li>Reserve a book</li>
  <li>Comment</li>
</ul>

<h2>What can an admin/editor do additionally?</h2>
<ul>
  <li>Assign roles (only admin)</li>
  <li>CRUD</li>
  <li>Rent books and retrieve them</li>
</ul>

<h2>How can you send an e-mail?</h2>
You need to add this, based on your credentials;

```json
{
  "EmailSettings": {
    "MailServer": "smtp.gmail.com",
    "MailPort": 587,
    "SenderName": "Library",
    "FromEmail": "your email",
    "Password": "your password"
  }
}
```
https://dotnettutorials.net/lesson/configuring-email-service-in-asp-net-core-identity/
