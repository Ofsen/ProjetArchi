using ArchiLibrary.Controllers.V2;
using ArchiLog.Data;
using ArchiLog.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ArchiLog.Controllers.V2
{
    [ApiVersion("2.0")]
    public class BrandsController : BaseController<ArchiLogDbContext, Brand, BrandsController>
    {
        public BrandsController(ArchiLogDbContext context, ILogger<BrandsController> logger) : base(context, logger)
        {
        }
    }
}
