using ArchiLibrary.Controllers.V1;
using ArchiLog.Data;
using ArchiLog.Models;
using Microsoft.AspNetCore.Mvc;

namespace ArchiLog.Controllers.V2
{
    [ApiVersion("2.0")]
    public class UsersController : BaseController<ArchiLogDbContext, User, UsersController>
    {
        public UsersController(ArchiLogDbContext context, ILogger<UsersController> logger) : base(context, logger)
        {
        }
    }
}
