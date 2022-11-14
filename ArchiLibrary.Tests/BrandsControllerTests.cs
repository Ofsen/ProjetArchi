using ArchiLibrary.Controllers.V1;
using Microsoft.EntityFrameworkCore;
using ArchiLibrary.Models;
using Microsoft.Extensions.Logging;
using ArchiLibrary.Data;
using ArchiLog.Data;
using ArchiLog.Controllers.V1;
using ArchiLog.Models;
using System;

namespace ArchiLibrary.Tests
{
    //public class Modal : BaseModel {}
    //public class Context : BaseDbContext
    //{
    //    public Context(DbContextOptions<Context> options) { }
    //}
    //public class Controller : BaseController<BaseDbContext, Modal, Controller>
    //{
    //    public Controller(BaseDbContext context) : base(context)
    //    {
    //    }
    //}

    public class BrandsControllerTests
    {
        private readonly ArchiLogDbContext _context;
        private readonly BrandsController _brandsController;

        public BrandsControllerTests()
        {
            _context = ContextGenerator.Generate();
            _brandsController = new BrandsController(_context);
        }

        [Fact]
        public async void GetAll()
        {
            _context.Database.EnsureDeleted();
            _context.Database.EnsureCreated();

            await _brandsController.PostItem(new Brand { Name = "newName" });

            Assert.Single(_context.Brands);
        }

        [Fact]
        public async void GetById()
        {
            _context.Database.EnsureDeleted();
            _context.Database.EnsureCreated();

            var id = _context.Brands.Add(new Brand { Name = "newName" }).Entity.ID;
            _context.SaveChanges();

            var result = await _brandsController.GetById(id);
            Assert.NotNull(result);
        }

        [Fact]
        public async void PostItem()
        {
            _context.Database.EnsureDeleted();
            _context.Database.EnsureCreated();

            await _brandsController.PostItem(new Brand { Name = "newName" });

            Assert.Single(_context.Brands);
        }
    }

    public static class ContextGenerator
    {
        public static ArchiLogDbContext Generate()
        {
            var options = new DbContextOptionsBuilder<ArchiLogDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
            return new ArchiLogDbContext(options);
        }
    }
}