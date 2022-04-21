using Common.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Deltas;
using Microsoft.AspNetCore.OData.Formatter;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Microsoft.EntityFrameworkCore;

namespace ODataServer.Controllers
{
    public class DroidsController : ODataController
    {
        private readonly StarWarsContext context;

        public DroidsController(StarWarsContext context)
        {
            this.context = context;
        }

        [EnableQuery]
        public IActionResult Get()
        {
            return Ok(context.Droids);
        }

        [EnableQuery]
        public async Task<IActionResult> GetAsync(int key)
        {
            return Ok(await context.Droids.FirstAsync(d => d.Id == key));
        }

        [EnableQuery]
        public async Task<IActionResult> Patch([FromODataUri] int key, Delta<Droid> droid)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var existingDroid = await context.Droids.FindAsync(key);
            if (existingDroid == null)
            {
                return NotFound();
            }

            droid.Patch(existingDroid);
            try
            {
                await context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!DroidExists(key))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            return Updated(existingDroid);
        }

        [EnableQuery]
        public IActionResult Post([FromBody]Droid droid)
        {
            context.Droids.Add(droid);
            context.SaveChanges();
            return Created(droid);
        }

        private bool DroidExists(int id)
        {
            return context.Droids.Any(e => e.Id == id);
        }


    }
}
