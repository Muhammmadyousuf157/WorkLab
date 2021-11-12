using System;

namespace WorkLabLibrary.Models
{
    public class User
    {
        public int UserId { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string UserName { get; set; }

        public string EmailAddress { get; set; }

        public string PasswordText { get; set; }

        public Guid ConfirmationToken { get; set; }

        public bool EmailConfirmed { get; set; }

        public string FullName => $"{FirstName} {LastName}";
    }
}