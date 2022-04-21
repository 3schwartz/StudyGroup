using Common.Models;

namespace GraphQLServer
{
    public class Mutation
    {
        public async Task<DroidPayload> DroidPrimaryFunctionChange(
            DroidPrimaryFunctionChangeInput input,
            [Service]StarWarsContext context)
        {
            var droid = await context.Droids.FindAsync(input.Id);

            droid.PrimaryFunction = input.PrimaryFunction;

            await context.SaveChangesAsync();

            return new DroidPayload(droid);
        }

        public async Task<DroidPayload> AddDroidAsync(
            AddDroidInput input,
            [Service]StarWarsContext context)
        {
            var episodes = context.Episodes
                .Where(e => input.episodes.ids.Contains(e.Id))
                .ToList();
            var droid = new Droid
            {
                Name = input.Name,
                PrimaryFunction = input.PrimaryFunction,
                Episodes = episodes
            };

            context.Droids.Add(droid);
            await context.SaveChangesAsync();

            return new DroidPayload(droid);
        }
    }

    public record DroidPrimaryFunctionChangeInput(int Id, string PrimaryFunction);

    public record AddDroidInput(string Name, string PrimaryFunction, EpisodeInput episodes);

    public record EpisodeInput(IList<int> ids);

    public class DroidPayload
    {
        public Droid Droid { get; set; }
        public DroidPayload(Droid droid)
        {
            Droid = droid;
        }
    }
}
