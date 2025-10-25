namespace ArbiStock.Domain.Cs2
{
    public record Cs2Item(string Id, string SkinName, string Weapon, string Rarity, string IconUrl);
    public record PricePoint(DateTime TsUtc, string Platform, decimal Price);

    public interface IPriceProvider
    {
        Task<PricePoint?> GetLatestAsync(string itemId, CancellationToken ct = default);
        Task<IReadOnlyList<PricePoint>> GetHistoryAsync(string itemId, TimeSpan window, CancellationToken ct = default);
    } 
}