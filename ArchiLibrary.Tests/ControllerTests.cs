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
    public class Modal : BaseModel { }
    public class Context : BaseDbContext
    {
        public Context(DbContextOptions<Context> options) : base(options) { }
        public DbSet<Modal> Modals { get; set; }
    }
    public class Controller : BaseController<BaseDbContext, Modal, Controller>
    {
        public Controller(BaseDbContext context) : base(context)
        {
        }
    }

    public class ControllerTests
    {
        private readonly Context _context;
        private readonly Controller _controller;

        public ControllerTests()
        {
            _context = ContextGenerator.Generate();
            _controller = new Controller(_context);
        }

        //[Fact]
        //public async void GetAll()
        //{
        //    _context.Database.EnsureDeleted();
        //    _context.Database.EnsureCreated();

        //    _context.Brands.AddRange(new Brand { Name = "newName" }, new Brand { Name = "newName2" });
        //    _context.SaveChanges();

        //    await _controller.GetAll();

        //    Assert.Collection();
        //}

        [Fact]
        public async void GetById()
        {
            _context.Database.EnsureDeleted();
            _context.Database.EnsureCreated();

            var id = _context.Modals.Add(new Modal()).Entity.ID;
            _context.SaveChanges();

            var result = await _controller.GetById(id);
            Assert.NotNull(result);
        }

        [Fact]
        public async void PostItem()
        {
            _context.Database.EnsureDeleted();
            _context.Database.EnsureCreated();

            await _controller.PostItem(new Modal());

            Assert.Single(_context.Modals);
        }
    }

    public static class ContextGenerator
    {
        public static Context Generate()
        {
            var options = new DbContextOptionsBuilder<Context>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
            return new Context(options);
        }
    }
}