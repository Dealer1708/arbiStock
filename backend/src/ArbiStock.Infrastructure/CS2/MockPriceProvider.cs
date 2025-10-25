using ArbiStock.Domain.Cs2;

namespace ArbiStock.Infrastructure.Cs2;

public class MockPriceProvider : IPriceProvider
{
    // market simulation with random data. To be changed to real analysis later
    private static readonly Random random = new();
    private readonly Dictionary<string, List<PricePoint>> _data = new();

    public MockPriceProvider()
    {
        Seed("ak-redline", 48m);
        Seed("awp-dragon-lore", 1800m);
    }

    void Seed(string id, decimal basePrice)
    {
        var now = DateTime.UtcNow;
        var list = new List<PricePoint>();

        for (int i = 144; i >= 0; i--)
        {
            var p = basePrice * (decimal)(0.95 * random.NextDouble() * 0.1);
            list.Add(new PricePoint(now.AddMinutes(-10 * i), "mock", Math.Round(p, 2)));
        }
        _data[id] = list;
    }

    public Task<PricePoint?> GetLatestAsync(string itemId, CancellationToken ct = default)
        => Task.FromResult(_data.TryGetValue(itemId, out var l) && l.Count > 0 ? l[^1] : null);

    public Task<IReadOnlyList<PricePoint>> GetHistoryAsync(string itemId, TimeSpan w, CancellationToken ct = default)
    {
        var from = DateTime.UtcNow - w;
        var list = _data.TryGetValue(itemId, out var l) ? l.Where(x => x.TsUtc >= from).ToList() : new();

        return Task.FromResult<IReadOnlyList<PricePoint>>(list);
    }
    
    public void TickAll()
    {
        foreach (var kv in _data)
        {
            var last = kv.Value[^1];
            var drift = (decimal)(random.NextDouble() * 2 - 1) * last.Price * 0.005m;
            kv.Value.Add(new PricePoint(DateTime.UtcNow, "mock", Math.Max(0.01m, Math.Round(last.Price + drift, 2))));
        }
    }
}