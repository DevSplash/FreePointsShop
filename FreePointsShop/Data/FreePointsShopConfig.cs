using System.Collections.Immutable;

namespace FreePointsShop.Data;
internal sealed record FreePointsShopConfig {
	public ushort Interval { get; set; } = 60 * 6;
	public ushort PageSize { get; set; } = 1000;
	public uint MaxPageNum { get; set; } = 1;
	public ImmutableHashSet<string> BotBlacklist { get; set; } = [];
	public ImmutableHashSet<uint> AppBlacklist { get; set; } = [];
}
