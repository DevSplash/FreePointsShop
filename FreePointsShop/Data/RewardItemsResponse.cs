using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace FreePointsShop.Data;
internal sealed record RewardItemsResponse {
	[JsonPropertyName("response"), JsonRequired]
	public RewardItemsData? Response { get; init; }
}
internal sealed record RewardItemsData {
	[JsonPropertyName("count")]
	public uint? Count { get; init; }
	[JsonPropertyName("definitions")]
	public List<RewardItem>? Definitions { get; init; }
	[JsonPropertyName("next_cursor")]
	public string? NextCursor { get; init; }
	[JsonPropertyName("total_count")]
	public uint? TotalCount { get; init; }
}
internal sealed record RewardItem {
	[JsonPropertyName("appid")]
	public uint? AppId { get; init; }
	[JsonPropertyName("defid")]
	public uint? DefId { get; init; }
	[JsonPropertyName("type")]
	public int? Type { get; init; }
	[JsonPropertyName("community_item_class")]
	public int? CommunityItemClass { get; init; }
	[JsonPropertyName("community_item_type")]
	public int? CommunityItemType { get; init; }
	[JsonPropertyName("point_cost")]
	public string? PointCost { get; init; }
	[JsonPropertyName("timestamp_created")]
	public uint? TimestampCreated { get; init; }
	[JsonPropertyName("timestamp_updated")]
	public uint? TimestampUpdated { get; init; }
	[JsonPropertyName("timestamp_available")]
	public uint? TimestampAvailable { get; init; }
	[JsonPropertyName("timestamp_available_end")]
	public uint? TimestampAvailableEnd { get; init; }
	[JsonPropertyName("quantity")]
	public string? Quantity { get; init; }
	[JsonPropertyName("internal_description")]
	public string? InternalDescription { get; init; }
	[JsonPropertyName("active")]
	public bool? Active { get; init; }
	[JsonPropertyName("community_item_data")]
	public CommunityItemData? CommunityItemData { get; init; }
	[JsonPropertyName("bundle_defids")]
	public List<uint>? BundleDefIds { get; init; }
	[JsonPropertyName("usable_duration")]
	public int? UsableDuration { get; init; }
	[JsonPropertyName("bundle_discount")]
	public int? BundleDiscount { get; init; }
}
internal sealed record CommunityItemData {
	[JsonPropertyName("item_name")]
	public string? ItemName { get; init; }
	[JsonPropertyName("item_title")]
	public string? ItemTitle { get; init; }
	[JsonPropertyName("item_image_large")]
	public string? ItemImageLarge { get; init; }
	[JsonPropertyName("animated")]
	public bool? Animated { get; init; }
	[JsonPropertyName("tiled")]
	public bool? Tiled { get; init; }
	[JsonPropertyName("item_image_small")]
	public string? ItemImageSmall { get; init; }
	[JsonPropertyName("item_description")]
	public string? ItemDescription { get; init; }
	[JsonPropertyName("item_movie_webm")]
	public string? ItemMovieWebm { get; init; }
	[JsonPropertyName("item_movie_mp4")]
	public string? ItemMovieMp4 { get; init; }
	[JsonPropertyName("item_movie_webm_small")]
	public string? ItemMovieWebmSmall { get; init; }
	[JsonPropertyName("item_movie_mp4_small")]
	public string? ItemMovieMp4Small { get; init; }
	[JsonPropertyName("profile_theme_id")]
	public string? ProfileThemeId { get; init; }
}
