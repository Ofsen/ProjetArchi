using ArchiLibrary.Data;
using ArchiLibrary.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArchiLibrary.controllers
{
    [ApiController]
    [Route("catalog/v{version:apiVersion}/[controller]")]
    [ApiVersion("1.0")]
    // adding new version
    // [ApiVersion("2.0")]

    // set a version to deprecated
    // [ApiVersion("1.0", Deprecated = true)]

    // set specific methode to a specific version
    // [MapToApiVersion("2.0")]
    public abstract class BaseController<TContext, TModel> : ControllerBase where TContext : BaseDbContext where TModel : BaseModel
    {
        protected readonly TContext _context;

        public BaseController(TContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Get all data
        /// </summary>
        /// <response code="200">An array of objects</response>
        /// <returns>An array of objects</returns>
        [HttpGet]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IEnumerable<TModel>> GetAll()
        {
            return await _context.Set<TModel>().Where(x => x.Active).ToListAsync();
        }

        /// <summary>
        /// Get one (1) object by ID
        /// </summary>
        /// <param name="id"></param>
        /// <response code="200">The object requested</response>
        /// <response code="404">Not Found, object doesn't exist in the database</response>
        /// <returns>The object requested</returns>
        [HttpGet("{id}")]// /api/{item}/3
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<TModel>> GetById([FromRoute] int id)
        {
            var item = await _context.Set<TModel>().SingleOrDefaultAsync(x => x.ID == id);
            if (item == null || !item.Active)
                return NotFound();
            return item;
        }

        /// <summary>
        /// Add a new object to the database
        /// </summary>
        /// <param name="item">The object thats going to be added to the database</param>
        /// <response code="200">The inserted object</response>
        /// <returns>The inserted object</returns>
        [HttpPost]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> PostItem([FromBody] TModel item)
        {
            await _context.AddAsync(item);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetById", new { id = item.ID }, item);
        }

        /// <summary>
        /// Modify an existing object in the database
        /// </summary>
        /// <param name="item">The new object thats going to replace the old one in the database</param>
        /// <param name="id">The ID of the object thats going to be replaced</param>
        /// <response code="200">The modified object</response>
        /// <response code="400">Bad request, item.ID doesn't match with param ID</response>
        /// <response code="404">Not Found, object doesn't exist in the database</response>
        /// <returns>The modified object</returns>
        [HttpPut("{id}")]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<TModel>> PutItem([FromRoute] int id, [FromBody] TModel item)
        {
            if (id != item.ID)
                return BadRequest();
            if (!ItemExists(id))
                return NotFound();

            //_context.Entry(item).State = EntityState.Modified;
            _context.Update(item);
            await _context.SaveChangesAsync();

            return item;
        }

        /// <summary>
        /// Delete an existing object in the database
        /// </summary>
        /// <param name="id">The ID of the object thats going to be deleted</param>
        /// <response code="200">The deleted object</response>
        /// <response code="400">Bad request, there is no object with that ID doesn't in the database</response>
        /// <returns>The deleted object</returns>
        [HttpDelete("{id}")]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<TModel>> DeleteItem([FromRoute] int id)
        {
            var item = await _context.Set<TModel>().FindAsync(id);
            if (item == null)
                return BadRequest();
            //_context.Entry(item).State = EntityState.Deleted;
            _context.Remove(item);
            await _context.SaveChangesAsync();
            return item;
        }

        /// <summary>
        /// Checks if the object exists in the database
        /// </summary>
        /// <param name="id">ID of the object</param>
        /// <returns>Boolean</returns>
        private bool ItemExists(int id)
        {
            return _context.Set<TModel>().Any(x => x.ID == id);
        }
    }
}
