using Models;

public class Query
{
    public Task<Droid> Benchmark() => 
        Task.FromResult(new Droid { Name = "R5-D4", PrimaryFunction = "Astromech" });
}