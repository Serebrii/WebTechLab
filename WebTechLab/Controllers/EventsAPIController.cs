using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebTechLab.Models;
using Microsoft.Extensions.Caching.Memory;


namespace WebTechLab.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EventsAPIController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IMemoryCache _cache;

        public EventsAPIController(ApplicationDbContext context, IMemoryCache cache)
        {
            _context = context;
            _cache = cache;
        }

        // GET: api/EventsAPI
        [HttpGet]
        public async Task<ActionResult<object>> GetEvents([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            var totalCount = await _context.Events.CountAsync();

            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            var events = await _context.Events
                .Include(e => e.Category)
                .Include(e => e.Venue)
                .OrderBy(e => e.Id)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var nextLink = (pageNumber < totalPages)
                ? Url.Action("GetEvents", null, new { pageNumber = pageNumber + 1, pageSize = pageSize }, Request.Scheme)
                : null;

            var previousLink = (pageNumber > 1)
                ? Url.Action("GetEvents", null, new { pageNumber = pageNumber - 1, pageSize = pageSize }, Request.Scheme)
                : null;

            var response = new
            {
                totalCount,
                totalPages,
                currentPage = pageNumber,
                currentPageSize = pageSize,
                nextPageLink = nextLink,
                previousPageLink = previousLink,
                data = events
            };

            return Ok(response);
        }

        // GET: api/EventsAPI/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Event>> GetEvent(int id)
        {
            var @event = await _context.Events.FindAsync(id);

            if (@event == null)
            {
                return NotFound();
            }

            return @event;
        }

        // PUT: api/EventsAPI/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutEvent(int id, Event @event)
        {
            if (id != @event.Id)
            {
                return BadRequest();
            }

            _context.Entry(@event).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!EventExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/EventsAPI
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Event>> PostEvent(Event @event)
        {
            _context.Events.Add(@event);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetEvent", new { id = @event.Id }, @event);
        }

        // DELETE: api/EventsAPI/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEvent(int id)
        {
            var @event = await _context.Events.FindAsync(id);
            if (@event == null)
            {
                return NotFound();
            }

            _context.Events.Remove(@event);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // GET: api/EventsAPI/Statistics
        [HttpGet("Statistics")]
        public async Task<IActionResult> GetStatistics()
        {
            const string cacheKey = "StatisticsData";

            if (!_cache.TryGetValue(cacheKey, out object statisticsData))
            {
                statisticsData = await _context.Events
                    .GroupBy(e => e.Category)
                    .Select(g => new
                    {
                        categoryName = g.Key.Name,
                        count = g.Count()
                    })
                    .ToListAsync();

                var cacheEntryOptions = new MemoryCacheEntryOptions()
                    .SetAbsoluteExpiration(TimeSpan.FromMinutes(5));

                _cache.Set(cacheKey, statisticsData, cacheEntryOptions);
            }

            return Ok(statisticsData);
        }

        private bool EventExists(int id)
        {
            return _context.Events.Any(e => e.Id == id);
        }
    }
}
