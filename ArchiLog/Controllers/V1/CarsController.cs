using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ArchiLog.Data;
using ArchiLog.Models;
using ArchiLibrary.Controllers.V1;

namespace ArchiLog.Controllers.V1
{
    public class CarsController : BaseController<ArchiLogDbContext, Car, CarsController>
    {
        public CarsController(ArchiLogDbContext context, ILogger<CarsController> logger) : base(context, logger)
        {
        }
    }
}
