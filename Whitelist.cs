using System.Net.Http;
using System.Text.Json;
using System.Collections.Generic;
using System.IO;
using System;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Linq;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Entities;
using static CounterStrikeSharp.API.Core.Listeners;

namespace Whitelist;

public class PluginConfig
{
    public int UseSteamGroup { get; set; } = 0;
    public string SteamGroupName { get; set; } = "";
    public List<string> SteamIdList { get; set; } = new();
    public int UseBlacklist { get; set; } = 0;
}

public class WhitelistPlugin : BasePlugin
{
    public override string ModuleName => "Whitelist";
    public override string ModuleVersion => "1.0.0";
    public override string ModuleAuthor => "Yomo3S";

    private PluginConfig Config = null!;
    private HashSet<string> _allowedSteamIds = new();
    private readonly HttpClient _httpClient = new();
    private readonly string _configPath = "csgo/cfg/plugins/Whitelist/config.json";

    public override void Load(bool hotReload)
    {
        LoadCustomConfig();

        if (Config.UseSteamGroup == 1)
            LoadWhitelistFromSteamGroup();
        else
            LoadWhitelistFromList();

        RegisterListener<OnClientAuthorized>(OnClientAuthorized);
    }

    private void LoadCustomConfig()
    {
        string fullPath = Path.Combine(Server.GameDirectory, _configPath);
        if (!File.Exists(fullPath)) return;

        try
        {
            string json = File.ReadAllText(fullPath);
            Config = JsonSerializer.Deserialize<PluginConfig>(json) ?? new PluginConfig();
        }
        catch
        {
            Config = new PluginConfig();
        }
    }

    private void LoadWhitelistFromSteamGroup()
    {
        _allowedSteamIds.Clear();
        if (string.IsNullOrEmpty(Config.SteamGroupName)) return;

        string groupName = Config.SteamGroupName;

        Task.Run(async () =>
        {
            try
            {
                var allMembers = new HashSet<string>();
                string? nextPageUrl = null;

                do
                {
                    string url = nextPageUrl ?? $"https://steamcommunity.com/groups/{groupName}/memberslistxml/?xml=1";
                    using var request = new HttpRequestMessage(HttpMethod.Get, url);
                    request.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36");
                    var response = await _httpClient.SendAsync(request);
                    response.EnsureSuccessStatusCode();

                    string xmlContent = await response.Content.ReadAsStringAsync();
                    var doc = XDocument.Parse(xmlContent);

                    var members = doc.Descendants("steamID64")
                                     .Select(el => el.Value)
                                     .Where(id => !string.IsNullOrEmpty(id));
                    foreach (var id in members)
                        allMembers.Add(id);

                    var nextLink = doc.Descendants("nextPageLink").FirstOrDefault();
                    nextPageUrl = nextLink?.Value;

                } while (!string.IsNullOrEmpty(nextPageUrl));

                Server.NextFrame(() =>
                {
                    _allowedSteamIds = allMembers;
                });
            }
            catch { }
        });
    }

    private void LoadWhitelistFromList()
    {
        _allowedSteamIds.Clear();
        foreach (var id in Config.SteamIdList)
        {
            if (!string.IsNullOrWhiteSpace(id))
                _allowedSteamIds.Add(id);
        }
    }

    private void OnClientAuthorized(int playerSlot, SteamID steamId)
    {
        var player = Utilities.GetPlayerFromSlot(playerSlot);
        if (player == null || !player.IsValid || player.IsBot) return;

        string steamId64 = steamId.SteamId64.ToString();
        if (string.IsNullOrEmpty(steamId64)) return;

        bool isInList = _allowedSteamIds.Contains(steamId64);
        bool shouldKick = (Config.UseBlacklist == 1) ? isInList : !isInList;

        if (shouldKick)
        {
            KickPlayer(player);
        }
    }

    private void KickPlayer(CCSPlayerController player)
    {
        if (player?.IsValid == true && player.UserId != null)
        {
            Server.NextFrame(() =>
            {
                if (player.IsValid && player.UserId != null)
                    Server.ExecuteCommand($"kickid {player.UserId}");
            });
        }
    }
}