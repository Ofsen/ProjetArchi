using ArchiLibrary.controllers;
using ArchiLog.Data;
using ArchiLog.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ArchiLog.Controllers
{
    public class BrandsController : BaseController<ArchiLogDbContext, Brand, BrandsController>
    {
        public BrandsController(ArchiLogDbContext context, ILogger<BrandsController> logger):base(context, logger)
        {
        }
    }
}
