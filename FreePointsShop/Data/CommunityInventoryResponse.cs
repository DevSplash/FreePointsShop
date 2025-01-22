using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace FreePointsShop.Data;
internal sealed record CommunityInventoryResponse {
	[JsonPropertyName("response"), JsonRequired]
	public CommunityInventoryItems? Response { get; init; }
}
internal sealed record CommunityInventoryItems {
	[JsonPropertyName("items")]
	public List<CommunityInventoryData>? Items { get; init; }
}
internal sealed record CommunityInventoryData {
	[JsonPropertyName("communityitemid")]
	public string? CommunityItemId { get; init; }
	[JsonPropertyName("item_type")]
	public int? ItemType { get; init; }
	[JsonPropertyName("appid")]
	public uint? AppId { get; init; }
	[JsonPropertyName("owner")]
	public int? Owner { get; init; }
	[JsonPropertyName("attributes")]
	public List<CommunityInventoryAttribute>? Attributes { get; init; }
	[JsonPropertyName("used")]
	public bool? Used { get; init; }
	[JsonPropertyName("owner_origin")]
	public int? OwnerOrigin { get; init; }
	[JsonPropertyName("amount")]
	public string? Amount { get; init; }
}
internal sealed record CommunityInventoryAttribute {
	[JsonPropertyName("attributeid")]
	public int? AttributeId { get; init; }
	[JsonPropertyName("value")]
	public string? Value { get; init; }
}
