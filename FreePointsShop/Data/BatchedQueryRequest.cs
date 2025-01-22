using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace FreePointsShop.Data;
internal sealed record BatchedQueryRequest {
	[JsonPropertyName("requests"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public List<BatchedQueryRequestData>? Requests { get; set; }
	public BatchedQueryRequest(List<BatchedQueryRequestData>? requests = null) => Requests = requests;
}
internal sealed record BatchedQueryRequestData {
	[JsonPropertyName("appids"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public List<uint>? AppIds { get; set; }
	[JsonPropertyName("time_available"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public uint? TimeAvailable { get; set; }
	[JsonPropertyName("community_item_classes"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public List<int>? CommunityItemClasses { get; set; }
	[JsonPropertyName("language"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public string? Language { get; set; }
	[JsonPropertyName("count"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public int? Count { get; set; }
	[JsonPropertyName("cursor"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public string? Cursor { get; set; }
	[JsonPropertyName("sort"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public int? Sort { get; set; }
	[JsonPropertyName("sort_descending"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public bool? SortDescending { get; set; }
	[JsonPropertyName("reward_types"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public List<int>? RewardTypes { get; set; }
	[JsonPropertyName("excluded_community_item_classes"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public List<int>? ExcludedCommunityItemClasses { get; set; }
	[JsonPropertyName("definitionids"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public List<uint>? DefinitionIds { get; set; }
	[JsonPropertyName("filters"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public List<int>? Filters { get; set; }
	[JsonPropertyName("filter_match_all_category_tags"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public List<string>? FilterMatchAllCategoryTags { get; set; }
	[JsonPropertyName("filter_match_any_category_tags"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public List<string>? FilterMatchAnyCategoryTags { get; set; }
	[JsonPropertyName("contains_definitionids"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public List<uint>? ContainsDefinitionIds { get; set; }
	[JsonPropertyName("include_direct_purchase_disabled"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public bool? IncludeDirectPurchaseDisabled { get; set; }
	[JsonPropertyName("excluded_content_descriptors"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public List<uint>? ExcludedContentDescriptors { get; set; }
	[JsonPropertyName("excluded_appids"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public List<uint>? ExcludedAppIds { get; set; }
	[JsonPropertyName("store_tagids"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public List<uint>? StoreTagIds { get; set; }
	[JsonPropertyName("excluded_store_tagids"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public List<uint>? ExcludedStoreTagIds { get; set; }
	[JsonPropertyName("search_term"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public string? SearchTerm { get; set; }
}
