using System.ComponentModel;
using System.Globalization;
using ArbiStock.Domain.Cs2;

namespace ArbiStock.Application.Cs2;

public class ItemCatalog
{
    private static readonly List<Cs2Item> Items =
    [
        new("ak-redline", "AK-47 | Redline (FT)", "AK-47", "Classified", ""),
        new("awp-dragon-lore", "AWP | Dragon Lore (FN)", "AWP", "Covert", "")
    ];

    public IEnumerable<Cs2Item> Search(string? q)
        => string.IsNullOrWhiteSpace(q) ? Items :
        Items.Where(i => i.SkinName.Contains(q, StringComparison.OrdinalIgnoreCase));
}

public class PriceService(IPriceProvider provider)
{
    public Task<PricePoint?> Latest(string id, CancellationToken ct) => provider.GetLatestAsync(id, ct);
    public Task<IReadOnlyList<PricePoint>> History(string id, int days, CancellationToken ct)
        => provider.GetHistoryAsync(id, TimeSpan.FromDays(days <= 0 ? 7 : days), ct);
}