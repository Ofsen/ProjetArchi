using ArchiLibrary.Data;
using ArchiLibrary.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace ArchiLibrary.Controllers.V2
{
    [ApiVersion("2.0")]
    [Route("/catalog/v{version:apiVersion}/[controller]")]
    [ApiController]
    [Authorize(Roles = "Administrator")]
    public abstract class BaseController<TContext, TModel, TController> : ControllerBase where TContext : BaseDbContext where TModel : BaseModel
    {
        protected readonly TContext _context;
        protected readonly ILogger<TController> _logger;

        public BaseController(TContext context, ILogger<TController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        public string GetAll()
        {
            return "hello";
        }
    }
}
