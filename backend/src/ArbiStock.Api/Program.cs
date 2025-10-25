using Asp.Versioning;
using ArbiStock.Application.Cs2;
using ArbiStock.Domain.Cs2;
using ArbiStock.Infrastructure.Cs2;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;

var b = WebApplication.CreateBuilder(args);
b.Services.AddEndpointsApiExplorer();
b.Services.AddSwaggerGen();
b.Services.AddCors(o => o.AddDefaultPolicy(p => p.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()));
b.Services.AddApiVersioning(o =>
    {
        o.DefaultApiVersion = new ApiVersion(1, 0);
        o.AssumeDefaultVersionWhenUnspecified = true;
        o.ReportApiVersions = true;
    }
);

b.Services.AddSingleton<ItemCatalog>();
b.Services.AddSingleton<MockPriceProvider>();
b.Services.AddSingleton<IPriceProvider>(sp => sp.GetRequiredService<MockPriceProvider>());
b.Services.AddSingleton<PriceService>();


var app = b.Build();
app.UseSwagger();
app.UseSwaggerUI();
app.UseCors();

app.MapGet("/healthz", () => Results.Ok(new { ok = true, ts = DateTime.UtcNow }));

app.MapGet("/api/v1/cs2/items", ([FromQuery] string? q, [FromServices] ItemCatalog cat) =>
    Results.Ok(new { items = cat.Search(q) }));

app.MapGet("/api/v1/cs2/items/{id}", ([FromRoute] string id, [FromServices] ItemCatalog cat) =>
{
    var item = cat.Search(null).FirstOrDefault(i => i.Id == id);
    return item is null ? Results.NotFound() : Results.Ok(item);
});

app.MapGet("/api/v1/cs2/items/{id}/latest",
    async ([FromRoute] string id, [FromServices] PriceService svc, CancellationToken ct) =>
{
    var p = await svc.Latest(id, ct);
    return p is null ? Results.NotFound() : Results.Ok(p);
});

app.MapGet("/api/v1/cs2/items/{id}/history",
    async ([FromRoute] string id, [FromQuery] int days, [FromServices] PriceService svc, CancellationToken ct) =>
{
    Results.Ok(new { itemId = id, points = await svc.History(id, days, ct) });
});

app.Run();
