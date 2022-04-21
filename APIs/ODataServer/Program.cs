using Common.Models;
using Microsoft.AspNetCore.OData;
using Microsoft.EntityFrameworkCore;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;

static IEdmModel GetEdmModel()
{
    var builder = new ODataConventionModelBuilder();
    builder.EntitySet<Droid>("Droids");
    builder.EntitySet<Episode>("Episodes");
    return builder.GetEdmModel();
}

var builder = WebApplication.CreateBuilder(args);

var services = builder.Services;

services
    .AddDbContext<StarWarsContext>(o => o.UseInMemoryDatabase("OData"));

services
    .AddControllers()
    .AddOData(opt => opt
        .AddRouteComponents("v1", GetEdmModel())
        .Filter().Select().Expand().OrderBy());

services
    .AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new() { Title = "OdataTurorial", Version = "v1" });
    });

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "ODataTutorial v1"));

app.UseRouting();

app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    using var context = scope.ServiceProvider.GetRequiredService<StarWarsContext>();

    context.Seed();
}

app.Run();
