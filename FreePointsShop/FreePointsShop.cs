using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using ArchiSteamFarm.Core;
using ArchiSteamFarm.Helpers.Json;
using ArchiSteamFarm.Localization;
using ArchiSteamFarm.Plugins.Interfaces;
using ArchiSteamFarm.Steam;
using ArchiSteamFarm.Web.GitHub;
using ArchiSteamFarm.Web.GitHub.Data;
using ArchiSteamFarm.Web.Responses;
using FreePointsShop.Data;
using JetBrains.Annotations;
using static ArchiSteamFarm.Steam.Integration.ArchiWebHandler;

namespace FreePointsShop;

[UsedImplicitly]
internal sealed partial class FreePointsShop : IASF, IGitHubPluginUpdates, IDisposable {
	public string Name => nameof(FreePointsShop);
	public string RepositoryName => "DevSplash/FreePointsShop";
	public Version Version => typeof(FreePointsShop).Assembly.GetName().Version ?? throw new InvalidOperationException(nameof(Version));
	private static Uri SteamApiURL => new("https://api.steampowered.com");
	private static Uri RefererURL => new(SteamStoreURL, "/points/shop");
	private FreePointsShopConfig Config = new();
	private static Timer? AutoRunTimer;
	private static readonly SemaphoreSlim AutoRunSemaphore = new(1, 1);
	private static readonly SemaphoreSlim BotSemaphore = new(1, 1);
	[GeneratedRegex(@"\[ASFMinimumVersion\]:(\d+\.\d+\.\d+\.\d+)")]
	private static partial Regex ASFMinimumVersionRegex();
	[GeneratedRegex(@"\[ASFMaximumVersion\]:(\d+\.\d+\.\d+\.\d+)")]
	private static partial Regex ASFMaximumVersionRegex();
	public Task OnLoaded() {
		AutoRunTimer = new(OnAutoRunTimer);
		return Task.CompletedTask;
	}
	public Task OnASFInit(IReadOnlyDictionary<string, JsonElement>? additionalConfigProperties = null) {
		ArgumentNullException.ThrowIfNull(AutoRunTimer);
		if (additionalConfigProperties != null) {
			foreach ((string configProperty, JsonElement configValue) in additionalConfigProperties) {
				if (configProperty == nameof(FreePointsShop) && configValue.ValueKind == JsonValueKind.Object) {
					try {
						Config = configValue.ToJsonObject<FreePointsShopConfig>() ?? Config;
					} catch (Exception e) {
						ASF.ArchiLogger.LogGenericWarning($"Invalid config property: {configProperty}\n{e}");
					}
				}
			}
		}
		lock (AutoRunSemaphore) {
			if (Config.Interval != 0) {
				AutoRunTimer.Change(TimeSpan.FromMinutes(2), TimeSpan.FromMinutes(Config.Interval));
			} else {
				AutoRunTimer.Change(Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);
			}
		}
		return Task.CompletedTask;
	}
	private static async Task<List<RewardItem>> QueryRewardItems(List<uint>? appids = null, List<uint>? definitionids = null, List<uint>? excludedAppids = null, List<int>? rewardTypes = null, List<int>? communityItemClasses = null, List<int>? excludedCommunityItemClasses = null, ushort? count = null, string? cursor = null, uint maxPageNum = 1) {
		Dictionary<string, string> parameters = [];
		for (int i = 0; i < appids?.Count; i++) {
			parameters[$"appids[{i}]"] = $"{appids[i]}";
		}
		for (int i = 0; i < definitionids?.Count; i++) {
			parameters[$"definitionids[{i}]"] = $"{definitionids[i]}";
		}
		for (int i = 0; i < excludedAppids?.Count; i++) {
			parameters[$"excluded_appids[{i}]"] = $"{excludedAppids[i]}";
		}
		for (int i = 0; i < rewardTypes?.Count; i++) {
			parameters[$"reward_types[{i}]"] = $"{rewardTypes[i]}";
		}
		for (int i = 0; i < communityItemClasses?.Count; i++) {
			parameters[$"community_item_classes[{i}]"] = $"{communityItemClasses[i]}";
		}
		for (int i = 0; i < excludedCommunityItemClasses?.Count; i++) {
			parameters[$"excluded_community_item_classes[{i}]"] = $"{excludedCommunityItemClasses[i]}";
		}
		if (count.HasValue) {
			parameters["count"] = $"{count}";
		}
		List<RewardItem> rewardItems = [];
		uint page = 0;
		uint itemCount = 0;
		while (true) {
			if (!string.IsNullOrWhiteSpace(cursor)) {
				parameters["cursor"] = $"{cursor}";
			} else {
				parameters.Remove("cursor");
			}
			string queryString = string.Join('&', parameters.Select(kv => $"{Uri.EscapeDataString(kv.Key ?? string.Empty)}={Uri.EscapeDataString(kv.Value ?? string.Empty)}"));
			Uri uri = new(SteamApiURL, $"/ILoyaltyRewardsService/QueryRewardItems/v1/{(string.IsNullOrWhiteSpace(queryString) ? string.Empty : $"?{queryString}")}");
			ObjectResponse<RewardItemsResponse>? response = await ASF.WebBrowser!.UrlGetToJsonObject<RewardItemsResponse>(uri, referer: RefererURL).ConfigureAwait(false);
			RewardItemsData? data = response?.Content?.Response;
			if (data == null || data.Definitions == null || data.Definitions.Count == 0) {
				break;
			}
			foreach (RewardItem item in data.Definitions) {
				if (item.DefId != null && !rewardItems.Any(rewardItem => rewardItem.DefId == item.DefId)) {
					rewardItems.Add(item);
				}
			}
			if (maxPageNum != 0 && ++page >= maxPageNum) {
				break;
			}
			itemCount += data.Count ?? 0;
			if (itemCount >= data.TotalCount) {
				break;
			}
			if (string.IsNullOrWhiteSpace(data.NextCursor) || cursor == data.NextCursor || data.NextCursor == "*") {
				break;
			}
			cursor = data.NextCursor;
		}
		return rewardItems;
	}
	private static async Task<CommunityInventoryResponse?> GetCommunityInventory(Bot bot, List<uint>? appids = null) {
		ArgumentNullException.ThrowIfNull(bot.AccessToken);
		Dictionary<string, string> parameters = [];
		parameters["access_token"] = $"{bot.AccessToken}";
		for (int i = 0; i < appids?.Count; i++) {
			parameters[$"filter_appids[{i}]"] = $"{appids[i]}";
		}
		string queryString = string.Join('&', parameters.Select(kv => $"{Uri.EscapeDataString(kv.Key ?? string.Empty)}={Uri.EscapeDataString(kv.Value ?? string.Empty)}"));
		Uri uri = new(SteamApiURL, $"/IQuestService/GetCommunityInventory/v1/?{queryString}");
		ObjectResponse<CommunityInventoryResponse>? response = await bot.ArchiWebHandler.UrlGetToJsonObjectWithSession<CommunityInventoryResponse>(uri, referer: RefererURL).ConfigureAwait(false);
		return response != null && response.StatusCode.IsSuccessCode() ? response.Content : null;
	}
	private static async Task<bool> RedeemPoints(Bot bot, uint defid, long expectedCost = 0) {
		ArgumentNullException.ThrowIfNull(bot.AccessToken);

		Uri uri = new(SteamApiURL, $"/ILoyaltyRewardsService/RedeemPoints/v1/");
		Dictionary<string, string> data = new() {
			{ "access_token", bot.AccessToken },
			{ "defid", $"{defid}" },
			{ "expected_points_cost",  $"{expectedCost}"}
		};
		return await bot.ArchiWebHandler.UrlPostWithSession(uri, data: data, referer: RefererURL, session: ESession.None).ConfigureAwait(false);
	}
	private static async Task<List<RewardItem>> QueryBundleItems(List<RewardItem> bundleItems, List<RewardItem>? rewardItems = null) {
		List<RewardItem> items = [];
		HashSet<uint> processedDefIds = [];
		HashSet<uint> unknownDefIds = [];
		HashSet<uint> defIds = [.. bundleItems.Where(item => item.BundleDefIds?.Count != 0).SelectMany(item => item.BundleDefIds ?? [])];
		await QueryBundleDefIds(defIds, rewardItems ?? bundleItems, processedDefIds, unknownDefIds, items).ConfigureAwait(false);
		if (unknownDefIds.Count > 0) {
			ASF.ArchiLogger.LogGenericWarning($"[{nameof(FreePointsShop)}] Definitionids could not be found: ${string.Join(", ", unknownDefIds)}");
		}
		return items;
	}
	private static async Task QueryBundleDefIds(HashSet<uint> defIds, List<RewardItem> rewardItems, HashSet<uint> processedDefIds, HashSet<uint> unknownDefIds, List<RewardItem> result, bool fetchRemote = true) {
		List<RewardItem> items = [];
		HashSet<uint> defIdsToQuery = [];
		foreach (uint defId in defIds) {
			if (!processedDefIds.Contains(defId)) {
				RewardItem? rewardItem = rewardItems.FirstOrDefault(o => o.DefId == defId);
				if (rewardItem != null) {
					items.Add(rewardItem);
					processedDefIds.Add(defId);
				} else {
					defIdsToQuery.Add(defId);
				}
			}
		}
		if (defIdsToQuery.Count != 0) {
			HashSet<uint> fetchedDefIds = [];
			if (fetchRemote) {
				List<RewardItem> fetchedItems = await QueryRewardItems(definitionids: [.. defIdsToQuery], count: 1000, maxPageNum: 0).ConfigureAwait(false);
				foreach (RewardItem item in fetchedItems) {
					if (item.DefId != null) {
						fetchedDefIds.Add(item.DefId.Value);
						if (!processedDefIds.Contains(item.DefId.Value)) {
							items.Add(item);
							processedDefIds.Add(item.DefId.Value);
						}
					}
				}
			}
			foreach (uint defId in defIdsToQuery.Except(fetchedDefIds)) {
				processedDefIds.Add(defId);
				unknownDefIds.Add(defId);
			}
		}
		defIds.Clear();
		foreach (RewardItem item in items) {
			if (item.Type == 6) {
				if (item.BundleDefIds != null && item.BundleDefIds.Count != 0) {
					foreach (uint defId in item.BundleDefIds) {
						if (!processedDefIds.Contains(defId)) {
							defIds.Add(defId);
						}
					}
				}
			} else {
				result.Add(item);
			}
		}
		if (defIds.Count != 0) {
			await QueryBundleDefIds(defIds, rewardItems, processedDefIds, unknownDefIds, result, fetchRemote).ConfigureAwait(false);
		}
	}
	private static async Task ClaimItemTask(Bot bot, List<RewardItem> bundleItems, List<RewardItem> singleItems, List<RewardItem> bundleDefs) {
		ArgumentNullException.ThrowIfNull(bot.AccessToken);
		List<RewardItem> itemsNotFound;
		HashSet<uint> ownedDefIds = [];
		foreach (RewardItem bundleItem in bundleItems) {
			List<RewardItem> items = [];
			await QueryBundleDefIds([.. bundleItem.BundleDefIds ?? []], bundleDefs, [], [], result: items, fetchRemote: false).ConfigureAwait(false);
			if (items.Count != 0) {
				itemsNotFound = [.. items.Where(item => !ownedDefIds.Any(defId => defId == item.DefId))];
				if (itemsNotFound.Count == 0) {
					continue;
				}
				List<CommunityInventoryData>? communityInventory = (await GetCommunityInventory(bot, [.. itemsNotFound.Where(o => o.AppId != null).Select(o => o.AppId!.Value).Distinct()]).ConfigureAwait(false))?.Response?.Items;
				if (communityInventory?.Count > 0) {
					foreach (CommunityInventoryData inventoryData in communityInventory) {
						RewardItem? ownedItem = itemsNotFound.FirstOrDefault(item => item.AppId == inventoryData.AppId && item.CommunityItemType == inventoryData.ItemType);
						if (ownedItem != null) {
							ownedDefIds.Add(ownedItem.DefId!.Value);
						}
					}
					if (!itemsNotFound.Any(item => !ownedDefIds.Any(defId => defId == item.DefId))) {
						continue;
					}
				}
			}
			if (await RedeemPoints(bot, bundleItem.DefId!.Value).ConfigureAwait(false)) {
				ASF.ArchiLogger.LogGenericInfo($"[{bot.BotName}] Item {bundleItem.DefId!.Value} redeemed successfully!");
				foreach (uint defId in bundleItem.BundleDefIds!) {
					ownedDefIds.Add(defId);
				}
			} else {
				ASF.ArchiLogger.LogGenericWarning($"[{bot.BotName}] Item {bundleItem.DefId!.Value} could not be redeemed! ({bundleItem.ToJsonText()})");
			}
		}
		itemsNotFound = [.. singleItems.Where(item => !ownedDefIds.Any(defId => defId == item.DefId))];
		if (itemsNotFound.Count > 0) {
			List<CommunityInventoryData>? communityInventory = (await GetCommunityInventory(bot, [.. itemsNotFound.Where(o => o.AppId != null).Select(o => o.AppId!.Value).Distinct()]).ConfigureAwait(false))?.Response?.Items;
			if (communityInventory?.Count > 0) {
				foreach (CommunityInventoryData inventoryData in communityInventory) {
					RewardItem? ownedItem = itemsNotFound.FirstOrDefault(item => item.AppId == inventoryData.AppId && item.CommunityItemType == inventoryData.ItemType);
					if (ownedItem != null) {
						itemsNotFound.Remove(ownedItem);
					}
				}
			}
			foreach (RewardItem item in itemsNotFound) {
				if (await RedeemPoints(bot, item.DefId!.Value).ConfigureAwait(false)) {
					ASF.ArchiLogger.LogGenericInfo($"[{bot.BotName}] Item {item.DefId!.Value} redeemed successfully!");
				} else {
					ASF.ArchiLogger.LogGenericWarning($"[{bot.BotName}] Item {item.DefId!.Value} could not be redeemed! ({item.ToJsonText()})");
				}
			}
		}
	}
	private async void OnAutoRunTimer(object? state = null) {
		if (!await AutoRunSemaphore.WaitAsync(0).ConfigureAwait(false)) {
			ASF.ArchiLogger.LogGenericWarning($"[{nameof(FreePointsShop)}] Task is already running!");
			return;
		}
		try {
			ASF.ArchiLogger.LogGenericInfo($"[{nameof(FreePointsShop)}] Query reward items...");
			List<RewardItem> rewardItems = await QueryRewardItems(count: Config.PageSize, excludedAppids: [.. Config.AppBlacklist], maxPageNum: Config.MaxPageNum).ConfigureAwait(false);
			if (rewardItems.Count == 0) {
				return;
			}
			List<RewardItem> bundleItems = [],
							 singleItems = [];
			foreach (RewardItem item in rewardItems) {
				if (item.Active != true) {
					continue;
				}
				lock (Config.AppBlacklist) {
					if (item.AppId.HasValue && Config.AppBlacklist.Contains(item.AppId.Value)) {
						continue;
					}
				}
				if (item.Type == 6) {
					if (item.BundleDiscount == 100) {
						bundleItems.Add(item);
					}
				} else if (item.PointCost == "0") {
					singleItems.Add(item);
				}
			}
			if (bundleItems.Count == 0 && singleItems.Count == 0) {
				return;
			}
			ASF.ArchiLogger.LogGenericInfo($"[{nameof(FreePointsShop)}] {bundleItems.Count} free bundles and {singleItems.Count} free items found!");
			List<RewardItem> bundleDefs = await QueryBundleItems(bundleItems, rewardItems).ConfigureAwait(false);
			HashSet<Bot>? bots = Bot.GetBots("ASF");
			if (bots == null || bots.Count == 0) {
				ASF.ArchiLogger.LogGenericWarning($"[{nameof(FreePointsShop)}] Couldn't find any bot!");
				return;
			}
			List<Task> tasks = [];
			foreach (Bot bot in bots) {
				if (bot.IsConnectedAndLoggedOn) {
					lock (Config.BotBlacklist) {
						if (Config.BotBlacklist.Any(item => item.Equals(bot.BotName, StringComparison.OrdinalIgnoreCase))) {
							continue;
						}
					}
					tasks.Add(Task.Run(async () => {
						await BotSemaphore.WaitAsync().ConfigureAwait(false);
						try {
							await ClaimItemTask(bot, bundleItems, singleItems, bundleDefs).ConfigureAwait(false);
						} finally {
							BotSemaphore.Release();
						}
					}));
				} else {
					ASF.ArchiLogger.LogGenericWarning($"[{bot.BotName}] {Strings.BotNotConnected}");
				}
			}
			await Task.WhenAll([.. tasks]).ConfigureAwait(false);
		} finally {
			AutoRunSemaphore.Release();
		}
	}
	public async Task<Uri?> GetTargetReleaseURL(Version asfVersion, string asfVariant, bool asfUpdate, bool stable, bool forced) {
		ArgumentNullException.ThrowIfNull(asfVersion);
		ArgumentException.ThrowIfNullOrEmpty(asfVariant);
		if (string.IsNullOrEmpty(RepositoryName)) {
			ASF.ArchiLogger.LogGenericError(string.Format(CultureInfo.CurrentCulture, Strings.WarningFailedWithError, nameof(RepositoryName)));
			return null;
		}
		ImmutableList<ReleaseResponse>? releases = await GitHubService.GetReleases(RepositoryName, 100).ConfigureAwait(false);
		if (releases == null) {
			return null;
		}
		foreach (ReleaseResponse release in releases) {
			if (!stable || !release.IsPreRelease) {
				Version newVersion = new(release.Tag);
				if (!forced) {
					if (Version >= newVersion) {
						continue;
					}
					Match match = ASFMinimumVersionRegex().Match(release.MarkdownBody);
					if (!match.Success || match.Groups.Count != 2) {
						continue;
					}
					Version minimumVersion = new(match.Groups[1].Value);
					if (asfVersion < minimumVersion) {
						continue;
					}
					match = ASFMaximumVersionRegex().Match(release.MarkdownBody);
					if (match.Success && match.Groups.Count == 2) {
						Version maximumVersion = new(match.Groups[1].Value);
						if (asfVersion > maximumVersion) {
							continue;
						}
					}
				}
				if (release.Assets.Count == 0) {
					continue;
				}
				ReleaseAsset? asset = await ((IGitHubPluginUpdates) this).GetTargetReleaseAsset(asfVersion, asfVariant, newVersion, release.Assets).ConfigureAwait(false);
				if ((asset == null) || !release.Assets.Contains(asset)) {
					continue;
				}
				ASF.ArchiLogger.LogGenericInfo(string.Format(CultureInfo.CurrentCulture, Strings.PluginUpdateFound, Name, Version, newVersion));
				return asset.DownloadURL;
			}
		}
		ASF.ArchiLogger.LogGenericInfo($"No update available for {Name} plugin");
		return null;
	}
	public void Dispose() => AutoRunTimer?.Dispose();
}
