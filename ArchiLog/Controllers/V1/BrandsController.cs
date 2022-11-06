using ArchiLibrary.Controllers.V1;
using ArchiLog.Data;
using ArchiLog.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ArchiLog.Controllers.V1
{
    public class BrandsController : BaseController<ArchiLogDbContext, Brand, BrandsController>
    {
        public BrandsController(ArchiLogDbContext context, ILogger<BrandsController> logger) : base(context, logger)
        {
        }
    }
}
