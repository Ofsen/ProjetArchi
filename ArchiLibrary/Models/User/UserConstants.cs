using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace ArchiLibrary.Models.User
{
    public class UserConstants
    {
        public static List<UserModel> Users = new List<UserModel>()
        {
            new UserModel() { Username = "admin", Password = "admin", EmailAddress = "admin@admin.com", Active = true, CreatedAt = DateTime.Now, Role = "Administrator" },
            new UserModel() { Username = "user", Password = "user", EmailAddress = "user@user.com", Active = true, CreatedAt = DateTime.Now, Role = "User" },
        };
    }
}
