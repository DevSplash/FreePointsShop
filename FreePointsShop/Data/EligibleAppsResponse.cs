using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace FreePointsShop.Data;
internal sealed record EligibleAppsResponse {
	[JsonPropertyName("response"), JsonRequired]
	public EligibleAppsData? Response { get; init; }
}
internal sealed record EligibleAppsData {
	[JsonPropertyName("apps")]
	public List<EligibleApp>? Apps { get; init; }
}
internal sealed record EligibleApp {
	[JsonPropertyName("appid")]
	public uint? AppId { get; init; }
	[JsonPropertyName("has_items_anyone_can_purchase")]
	public bool? HasItemsAnyoneCanPurchase { get; init; }
	[JsonPropertyName("event_app")]
	public bool? EventApp { get; init; }
	[JsonPropertyName("hero_carousel_image")]
	public string? HeroCarouselImage { get; init; }
}

