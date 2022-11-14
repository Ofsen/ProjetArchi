using ArchiLibrary.Controllers.V2.Auth;
using ArchiLog.Data;
using ArchiLog.Models;
using Microsoft.AspNetCore.Mvc;

namespace ArchiLog.Controllers.V2
{
    [ApiVersion("2.0")]
    public class LoginController : LoginController<ArchiLogDbContext, User>
    {
        public LoginController(ArchiLogDbContext context, IConfiguration config) : base(context, config)
        {
        }
    }
}
