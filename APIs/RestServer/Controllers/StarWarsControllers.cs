using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestServer.Models;

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
            return await context.Droids.ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Droid>> GetDroid(int id)
        {
            var droid = await context.Droids.FindAsync(id);

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
