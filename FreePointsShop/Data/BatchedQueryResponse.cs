using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace FreePointsShop.Data;
internal sealed record BatchedQueryResponse {
	[JsonPropertyName("response"), JsonRequired]
	public BatchedQueryResponses? Response { get; init; }
}
internal sealed record BatchedQueryResponses {
	[JsonPropertyName("responses")]
	public List<BatchedQueryResponseData>? Responses { get; init; }
}
internal sealed record BatchedQueryResponseData {
	[JsonPropertyName("eresult")]
	public int? EResult { get; init; }
	[JsonPropertyName("response")]
	public RewardItemsData? Response { get; init; }
}
