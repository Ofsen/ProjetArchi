using ArchiLibrary.Models.User;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace ArchiLog.Models
{
    public class User : UserModel
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
    }
}
