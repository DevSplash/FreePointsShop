# FreePointsShop
![GitHub Downloads](https://img.shields.io/github/downloads/DevSplash/FreePointsShop/total)
![GitHub last commit](https://img.shields.io/github/last-commit/DevSplash/FreePointsShop)
![GitHub stars](https://img.shields.io/github/stars/DevSplash/FreePointsShop)

ArchiSteamFarm plugin to automatically redeem free items in the Steam points shop.
## Config
Plugin config is located in ASF.json: 
```json
{
    //ASF global config
    ...
    "FreePointsShop": {
        "Interval": 360,
        "PageSize": 1000,
        "MaxPageNum": 1,
        "BotBlacklist": [
            "bot1",
            "bot2"
        ],
        "AppBlacklist": [
            10000,
            20000
        ]
    }
}
```
### 1. **Interval** (`ushort`)
- **Description**: The interval (in minutes) between each check for free items.
- **Default**: `360` (6 hours)
- **Note**: The plugin will not check for free items when the value is set to 0.

### 2. **PageSize** (`ushort`)
- **Description**: The number of items to retrieve per page when checking for free items.
- **Default**: `1000`
- **Note**: It is recommended to keep the default value.

### 3. **MaxPageNum** (`uint`)
- **Description**: The maximum number of pages to scan for free items.
- **Default**: `1`
- **Note**: It is recommended to keep the default value.

### 4. **BotBlacklist** (`ImmutableHashSet<string>`)
- **Description**: A list of bots to exclude when redeeming items.
- **Default**: `[]`

### 5. **AppBlacklist** (`ImmutableHashSet<uint>`)
- **Description**: A list of App IDs to exclude from the search when looking for free items.
- **Default**: `[]`

## Disclaimer

By using this plugin, you acknowledge and accept the following:

- The plugin is provided "as-is," without warranties or guarantees.
- The creator is not responsible for any loss or damage resulting from the use of this plugin, including but not limited to accidentally redeeming paid items, account issues, or violations of Steam's Terms of Service.
- Use this plugin at your own risk.

## License

This project is licensed under the [GPL-3.0 License](LICENSE).