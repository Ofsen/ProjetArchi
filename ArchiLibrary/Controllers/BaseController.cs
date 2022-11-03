using ArchiLibrary.Data;
using ArchiLibrary.Extensions;
using ArchiLibrary.Extensions.Models;
using ArchiLibrary.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using System.Linq.Expressions;
using System.Net;

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
    public abstract class BaseController<TContext, TModel, TController> : ControllerBase where TContext : BaseDbContext where TModel : BaseModel
    {
        protected readonly TContext _context;
        protected readonly ILogger<TController> _logger;

        public BaseController(TContext context, ILogger<TController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Get all data
        /// </summary>
        /// <response code="200">An array of objects</response>
        /// <returns>An array of objects</returns>
        [HttpGet]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<IEnumerable<TModel>>> GetAll([FromQuery] ParamsModel myParams)
        {
            _logger.LogInformation("LOG : Get all starting");

            IQueryable<TModel> queryable = _context.Set<TModel>().Where(x => x.Active);

            // check if desc query exists
            Boolean desc = HttpContext.Request.Query.ContainsKey("desc");

            // Sorting
            queryable = queryable.Sort(myParams, desc);

            if(!string.IsNullOrWhiteSpace(myParams.Range))
            {
                _logger.LogInformation("LOG : Get all starting - Pagination");
                string[] pagination = myParams.Range.Split("-");
                int perPage = int.Parse(pagination[1]) - int.Parse(pagination[0]) + 1;

                if (perPage > 50)
                    return BadRequest();

                int count = queryable.Count();
                queryable = queryable.Skip(int.Parse(pagination[0])).Take(perPage);

                //                                                              count=10    -> 10/3 = 3.333 -> 3 * 3 = 9    -> 9 + 1 = 10-12
                //                                                              count=9     -> 9/3 = 3      -> 3 * 3 = 9    -> 9 + 1 = 10
                //                                                              count=11    -> 11/3 = 3.66  -> 3 * 3 = 9    -> 9 + 1 = 10-12
                //                                                              count=13    -> 13/3 = 4.33  -> 4 * 3 = 12   -> 12 + 1 = 13-15
                // https://localhost:7157/catalog/v1/Brands?Range=0-2 current   0-2 perPage=3       0-5 perPage=6
                // https://localhost:7157/catalog/v1/Brands?Range=2-4 suiv      3-5 -> 6-9 -> 10-12                 6-11 -> 12-18
                // https://localhost:7157/catalog/v1/Brands?Range=0-2 first     0-2
                // https://localhost:7157/catalog/v1/Brands?Range=4-6 last      -10
                // https://localhost:7157/catalog/v1/Brands?Range=1-3 prev      -10

                string headerLinks = GenerateHeaderLinks(pagination, perPage, count);

                HttpContext.Response.Headers.Add("Links", headerLinks);

                HttpContext.Response.Headers.Add("Content-Range", myParams.Range + "/" + perPage);
                HttpContext.Response.Headers.Add("Accept-Range", "element 50");
            }

            _logger.LogInformation("LOG : Get all finished");
            return await queryable.ToListAsync();
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
            _logger.LogInformation("LOG : Get by ID starting");
            var item = await _context.Set<TModel>().SingleOrDefaultAsync(x => x.ID == id);
            if (item == null || !item.Active)
            {
                _logger.LogInformation("LOG : Get by ID - ERROR Not Found");
                return NotFound();
            }

            _logger.LogInformation("LOG : Get by ID finished");
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
            _logger.LogInformation("LOG : Add new Element starting");
            await _context.AddAsync(item);
            await _context.SaveChangesAsync();

            _logger.LogInformation("LOG : Add new Element finished");
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
            _logger.LogInformation("LOG : Update Element starting");
            if (id != item.ID)
            {
                _logger.LogInformation("LOG : Update Element - ERROR Bad Request");
                return BadRequest();
            }

            if (!ItemExists(id))
            {
                _logger.LogInformation("LOG : Update Element - ERROR Not Found");
                return NotFound();
            }

            //_context.Entry(item).State = EntityState.Modified;
            _logger.LogInformation("LOG : Update Element finished");
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
            _logger.LogInformation("LOG : Delete an element starting");
            var item = await _context.Set<TModel>().FindAsync(id);
            if (item == null)
            {
                _logger.LogInformation("LOG : Delete an element - ERROR Bad Request");
                return BadRequest();
            }
            //_context.Entry(item).State = EntityState.Deleted;
            _context.Remove(item);
            await _context.SaveChangesAsync();
            _logger.LogInformation("LOG : Delete an element finished");
            return item;
        }

        /// <summary>
        /// Checks if the object exists in the database
        /// </summary>
        /// <param name="id">ID of the object</param>
        /// <returns>Boolean</returns>
        private bool ItemExists(int id)
        {
            _logger.LogInformation("LOG : checks if element exists");
            return _context.Set<TModel>().Any(x => x.ID == id);
        }

        /// <summary>
        /// Generates the header urls for pagination
        /// </summary>
        /// <param name="pagination"></param>
        /// <param name="perPage"></param>
        /// <param name="count"></param>
        /// <returns>String</returns>
        private string GenerateHeaderLinks(string[] pagination, int perPage, int count)
        {
            string baseUrl = HttpContext.Request.Host + HttpContext.Request.Path + "?range=";

            // building first url
            string first = baseUrl + "0-" + (perPage - 1) + "; rel=\"first\"";

            // bulding last url
            string last = baseUrl;
            if (count % perPage == 0)
            {
                last += (count - perPage) + "-" + (count - 1);
            }
            else
            {
                last += (Math.Floor((decimal)count / perPage) * perPage) + "-" + (Math.Ceiling((decimal)count / perPage) * perPage - 1);
            }
            last += "; rel=\"last\"";

            // building next url
            string next = baseUrl;
            if ((int.Parse(pagination[1]) + 1) >= count)
            {
                next = last;
            }
            else
            {
                next += (int.Parse(pagination[1]) + 1) + "-" + (perPage + int.Parse(pagination[1])) + "; rel=\"next\"";
            }

            // building prev url
            string prev = baseUrl;
            if (pagination[0] == "0")
            {
                prev = HttpContext.Request.Host + HttpContext.Request.Path + HttpContext.Request.QueryString.Value;
            }
            else
            {
                prev += +(int.Parse(pagination[0]) - perPage) + "-" + (int.Parse(pagination[0]) - 1);
            }
            prev += "; rel=\"prev\"";

            return first + ", " + prev + ", " + next + ", " + last;
        }
    }
}
