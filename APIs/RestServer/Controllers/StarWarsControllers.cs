using Common.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace RestServer.Controllers
{
    [Route("[controller]")]
    public class StarWarsControllers : Controller
    {
        private readonly StarWarsContext context;

        public StarWarsControllers(StarWarsContext context)
        {
            this.context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Droid>>> GetDroids()
        {
            var droidsQuerable = context.Droids.Include(d => d.Episodes);

            var droids = new List<Droid>();
            foreach (var droid in droidsQuerable)
            {
                droids.Add(new Droid
                {
                    Id = droid.Id,
                    Name = droid.Name,
                    PrimaryFunction = droid.PrimaryFunction,
                    Episodes = droid.Episodes.Select(e => new Episode { Id = e.Id, Title = e.Title}).ToList(),
                });
            }

            return droids;
        }

        private class DroidsViewModel : Droid
        {}

        [HttpGet("{id}")]
        public async Task<ActionResult<Droid>> GetDroid(int id)
        {
            var droid = await context
                .Droids
                .FirstAsync(d => d.Id == id);

            if (droid == null)
            {
                return NotFound();
            }

            return droid;
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutDroid(int id, Droid droid)
        {
            if (id != droid.Id)
            {
                return BadRequest();
            }

            context.Entry(droid).State = EntityState.Modified;

            try
            {
                await context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!DroidExists(id))
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

        [HttpPost]
        public async Task<ActionResult<Droid>> PostDroid(Droid droid)
        {
            context.Droids.Add(droid);
            await context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetDroid), new { id = droid.Id }, droid);
        }

        private bool DroidExists(long id)
        {
            return context.Droids.Any(e => e.Id == id);
        }
    }
}
