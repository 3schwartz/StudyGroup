using Chaos.Models;
using Microsoft.AspNetCore.Mvc;
using Polly.Contrib.Simmy;
using Polly.Contrib.Simmy.Outcomes;
using System.Text.Json;

namespace Chaos.Controllers
{
    [Route("[controller]/[action]")]
    public class ChaosController : Controller
    {
        private readonly ILogger<ChaosController> logger;
        private readonly ChaosContext chaosContext;
        private readonly HttpClient client;

        public ChaosController(ILogger<ChaosController> logger, ChaosContext chaosContext, HttpClient client)
        {
            this.logger = logger;
            this.chaosContext = chaosContext;
            this.client = client;
        }
        
        [HttpGet("{id}")]
        public ActionResult<IList<ChaosLink>> Linked(int id)
        {
            var links = chaosContext.ChaosLinks.Where(c => c.Linked == id).ToList();

            return links;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ChaosView>> Something(int id)
        {
            logger.LogInformation("Got a message");

            await SomeThingAsync();

            var chaosItem = await GetChaosItemFromDatabase(id);

            var links = await GetLinksFromClient(chaosItem.Id);

            AnotherThing();

            var view = CreateView(chaosItem, links);

            return Ok(view);
        }

        private async Task SomeThingAsync()
        {
            await Chaos.Chaos.GetSomeChaos.ExecuteAsync(async () =>
            await Task.Run(() => logger.LogInformation("This can be anything")));
        }

        private void AnotherThing()
        {
            logger.LogInformation("This can be something else");
        }

        private async Task<ChaosItem> GetChaosItemFromDatabase(int id)
        {
            var chaosItem = await chaosContext.ChaosItem.FindAsync(id);

            return chaosItem;
        }

        private async Task<IList<ChaosLink>> GetLinksFromClient(int id)
        {
            var response = await client.GetAsync($"http://localhost:5201/chaos/linked/{id}");
            var links = await response.Content.ReadAsAsync<List<ChaosLink>>();

            return links;
        }

        private ChaosView CreateView(ChaosItem chaosItem, IList<ChaosLink> links)
        {
            return new ChaosView
            {
                ChaosItemId = chaosItem.Id,
                Name = chaosItem.Name,
                Links = links
            };
        }
    }
}
