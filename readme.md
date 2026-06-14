### Whitelist
Plugin for restricting access to the server

### Installation
1. Install [CounterStrikeSharp](https://github.com/roflmuffin/CounterStrikeSharp/releases) and [Metamod](https://www.sourcemm.net/downloads.php/?branch=master)
2. Download [Whitelist](https://github.com/YesSeir/Whitelist/releases)
3. Unzip the archive and upload it into `game/csgo`
4. Configuration path `game/csgo/cfg/plugins/Whitelist/config.json`

### ⚙️ Configuration

```json
{
  "UseSteamGroup": 1, // 1 = use steam group, 0 = use manual steamid list
  "SteamGroupName": "Name", // u can get it from url steamcommunity.com/groups/Name
  "SteamIdList": ["76561199389731907", "00000000000000000"], // list of steamid64
  "UseBlacklist": 0 // 1 = use as Blacklist, 0 = use as Whitelist
}
```

### ✅ Example for Steam group

```json
{
  "UseSteamGroup": 1,
  "SteamGroupName": "Name",
  "SteamIdList": [],
  "UseBlacklist": 0
}
```

### ✅ Example for List

```json
{
  "UseSteamGroup": 0,
  "SteamGroupName": "",
  "SteamIdList": ["76561199389731907", "00000000000000000"],
  "UseBlacklist": 0
}
```