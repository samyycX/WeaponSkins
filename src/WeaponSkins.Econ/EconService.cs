using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.RegularExpressions;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using SwiftlyS2.Shared;

using ValveKeyValue;

using WeaponSkins.Configuration;
using WeaponSkins.Shared;

using AgentDefinition = WeaponSkins.Shared.AgentDefinition;

namespace WeaponSkins.Econ;

public class EconService
{
    private ISwiftlyCore Core { get; init; }
    private KVObject Root { get; set; } = null!;
    private ILogger<EconService> Logger { get; init; }
    private MainConfigModel Config { get; init; }
    private string _PrimaryLanguage { get; set; } = "english";
    private List<string> _RequiredLanguages { get; set; } = [];

    public Dictionary<string /* Name */, ItemDefinition> Items { get; set; } = new(StringComparer.OrdinalIgnoreCase);

    public Dictionary<string /* Name */, ClientLootListDefinition> ClientLootLists { get; set; } = new(StringComparer.OrdinalIgnoreCase);

    public List<string> NamedWeapons { get; set; } = new();
    public Dictionary<string /* Name */, RarityDefinition> Rarities { get; } = new(StringComparer.OrdinalIgnoreCase);
    public Dictionary<string /* Name */, ColorDefinition> Colors { get; } = new(StringComparer.OrdinalIgnoreCase);
    public Dictionary<string /* Name */, PaintkitDefinition> Paintkits { get; } = new(StringComparer.OrdinalIgnoreCase);

    public Dictionary<string /* Name */, List<PaintkitDefinition>> WeaponToPaintkits { get; } =
        new(StringComparer.OrdinalIgnoreCase);

    public Dictionary<string /* Name */, StickerDefinition> Stickers { get; } =
        new(StringComparer.OrdinalIgnoreCase);

    public Dictionary<string /* Name */, StickerCollectionDefinition> StickerCollections { get; } =
        new(StringComparer.OrdinalIgnoreCase);

    public Dictionary<string /* Name */, KeychainDefinition> Keychains { get; } = new(StringComparer.OrdinalIgnoreCase);

    public Dictionary<string /* Loot list name, or base charm name for highlights */, KeychainCollectionDefinition> KeychainCollections { get; set; } = new(StringComparer.OrdinalIgnoreCase);

    public Dictionary<string /* Name */, AgentDefinition> Agents { get; set; } = new(StringComparer.OrdinalIgnoreCase);

    public Dictionary<string /* Name */, MusicKitDefinition> MusicKits { get; set; } = new(StringComparer.OrdinalIgnoreCase);

    public Dictionary<string /* Name */, GraffitiDefinition> Graffiti { get; set; } = new(StringComparer.OrdinalIgnoreCase);

    public Dictionary<string /* Name */, GraffitiTintDefinition> GraffitiTints { get; set; } = new(StringComparer.OrdinalIgnoreCase);

    public Dictionary<int /* tournament_event_id */, GraffitiTournamentDefinition> GraffitiTournaments { get; set; } = new();

    public Dictionary<string /* Name */, PatchDefinition> Patches { get; set; } = new(StringComparer.OrdinalIgnoreCase);

    public Dictionary<string /* Name */, PatchCollectionDefinition> PatchCollections { get; set; } = new(StringComparer.OrdinalIgnoreCase);

    // Kept off the WeaponSkins.Shared records; IWeaponSkinAPI.GetImageKey serves them.
    private Dictionary<string /* kind + ":" + Name (event id for graffititournament), kind = agent/music/keychain/keychaincollection/sticker/capsule/graffiti/graffititournament/patch/patchcapsule */, string /* panorama/images/econ-relative key */> ImageKeys { get; set; } = new();

    // Loot lists name charms by their item name, not the definition index Keychains is keyed by.
    private Dictionary<string, KeychainDefinition> KeychainsByItemName { get; } = new(StringComparer.OrdinalIgnoreCase);

    private Dictionary<string /* Definition name */, string /* Base charm item name */> KeychainBases { get; } = new(StringComparer.OrdinalIgnoreCase);

    private Dictionary<string /* Language */, Dictionary<string /* Key */, string /* Value */>> Languages { get; } =
        new(StringComparer.OrdinalIgnoreCase);

    private Dictionary<string, string> RevolvingLootLists { get; } = new(StringComparer.OrdinalIgnoreCase);

    private const int SchemaVersion = 31;

    public EconService(ISwiftlyCore core,
        ILogger<EconService> logger,
        IOptions<MainConfigModel> config)
    {
        Core = core;
        Logger = logger;
        Config = config.Value;

        var itemLanguages = Config.ItemLanguages;
        if (itemLanguages.Count == 0)
        {
            Logger.LogInformation("No item languages specified, using default languages setting...");
            _PrimaryLanguage = "english";
            _RequiredLanguages = [];
        }
        else
        {
            _PrimaryLanguage = LanguageCodeToTranslationKey[itemLanguages[0]];
            _RequiredLanguages = itemLanguages.Select(l => LanguageCodeToTranslationKey[l]).ToList();
            Logger.LogInformation($"Item languages specified, primary language is {_PrimaryLanguage} and required languages are {string.Join(", ", _RequiredLanguages)}...");
        }

        var items = Core.GameFileSystem.ReadFile("scripts/items/items_game.txt", "GAME");
        var version = GetVersion(items);
        if (File.Exists(Path.Combine(Core.PluginDataDirectory, "version.lock")))
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            var startMemory = GC.GetTotalMemory(true);
            var lockVersion =
                JsonSerializer.Deserialize<EconVersion>(
                    File.ReadAllText(Path.Combine(Core.PluginDataDirectory, "version.lock")));
            if (new HashSet<string>(itemLanguages).SetEquals(lockVersion?.ItemLanguages ?? []) && lockVersion?.EconDataVersion == version && lockVersion.SchemaVersion == SchemaVersion)
            {
                var files = new[] { "items.json", "agents.json", "weapon_to_paintkits.json", "sticker_collections.json", "keychains.json", "keychain_collections.json", "musickits.json", "graffiti.json", "graffiti_tints.json", "graffiti_tournaments.json", "patches.json", "patch_collections.json", "image_keys.json" };
                var allFilesExist = files.All(f => File.Exists(Path.Combine(Core.PluginDataDirectory, f)));

                if (allFilesExist)
                {
                    Logger.LogInformation("Econ data is up to date, skipping parsing...");
                    Items = JsonSerializer.Deserialize<Dictionary<string, ItemDefinition>>(
                        File.ReadAllText(Path.Combine(Core.PluginDataDirectory, "items.json")))!;
                    Agents = JsonSerializer.Deserialize<Dictionary<string, AgentDefinition>>(
                        File.ReadAllText(Path.Combine(Core.PluginDataDirectory, "agents.json")))!;
                    WeaponToPaintkits =
                        JsonSerializer.Deserialize<Dictionary<string, List<PaintkitDefinition>>>(
                            File.ReadAllText(Path.Combine(Core.PluginDataDirectory, "weapon_to_paintkits.json")))!;
                    StickerCollections =
                        JsonSerializer.Deserialize<Dictionary<string, StickerCollectionDefinition>>(
                            File.ReadAllText(Path.Combine(Core.PluginDataDirectory, "sticker_collections.json")))!;
                    Keychains = JsonSerializer.Deserialize<Dictionary<string, KeychainDefinition>>(
                        File.ReadAllText(Path.Combine(Core.PluginDataDirectory, "keychains.json")))!;
                    KeychainCollections = JsonSerializer.Deserialize<Dictionary<string, KeychainCollectionDefinition>>(
                        File.ReadAllText(Path.Combine(Core.PluginDataDirectory, "keychain_collections.json")))!;
                    MusicKits = JsonSerializer.Deserialize<Dictionary<string, MusicKitDefinition>>(
                        File.ReadAllText(Path.Combine(Core.PluginDataDirectory, "musickits.json")))!;
                    Graffiti = JsonSerializer.Deserialize<Dictionary<string, GraffitiDefinition>>(
                        File.ReadAllText(Path.Combine(Core.PluginDataDirectory, "graffiti.json")))!;
                    GraffitiTints = JsonSerializer.Deserialize<Dictionary<string, GraffitiTintDefinition>>(
                        File.ReadAllText(Path.Combine(Core.PluginDataDirectory, "graffiti_tints.json")))!;
                    GraffitiTournaments = JsonSerializer.Deserialize<Dictionary<int, GraffitiTournamentDefinition>>(
                        File.ReadAllText(Path.Combine(Core.PluginDataDirectory, "graffiti_tournaments.json")))!;
                    Patches = JsonSerializer.Deserialize<Dictionary<string, PatchDefinition>>(
                        File.ReadAllText(Path.Combine(Core.PluginDataDirectory, "patches.json")))!;
                    PatchCollections = JsonSerializer.Deserialize<Dictionary<string, PatchCollectionDefinition>>(
                        File.ReadAllText(Path.Combine(Core.PluginDataDirectory, "patch_collections.json")))!;
                    ImageKeys = JsonSerializer.Deserialize<Dictionary<string, string>>(
                        File.ReadAllText(Path.Combine(Core.PluginDataDirectory, "image_keys.json")))!;

                    // TrimLanguages(allowedLanguages);
                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                    GC.Collect();
                    var endMemory = GC.GetTotalMemory(true);
                    Logger.LogInformation($"Memory usage: {endMemory - startMemory} bytes");
                    return;
                }

                Logger.LogWarning("Some econ data files are missing, re-parsing...");
            }
        }

        Dump(items);
        // TrimLanguages(allowedLanguages);
    }

    private void Dump(string items)
    {
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
        var startMemory = GC.GetTotalMemory(true);
        var stream = new MemoryStream(items.Select(c => (byte)c).ToArray());
        var kv = KVSerializer.Create(KVSerializationFormat.KeyValues1Text);
        Root = kv.Deserialize(stream);

        Stopwatch watch = new();
        Stopwatch totalWatch = new();
        watch.Start();
        totalWatch.Start();
        Logger.LogInformation("Started parsing data...");

        ParseLanguages();
        Logger.LogInformation($"Parsed {Languages.Count} languages in {watch.ElapsedMilliseconds}ms.");
        watch.Restart();

        ParseColors();
        Logger.LogInformation($"Parsed {Colors.Count} colors in {watch.ElapsedMilliseconds}ms.");
        watch.Restart();

        ParseRarities();
        Logger.LogInformation($"Parsed {Rarities.Count} rarities in {watch.ElapsedMilliseconds}ms.");
        watch.Restart();

        ParseRevolvingLootLists();
        Logger.LogInformation(
            $"Parsed {RevolvingLootLists.Count} revolving loot lists in {watch.ElapsedMilliseconds}ms.");
        watch.Restart();

        ParseClientLootLists();
        Logger.LogInformation($"Parsed {ClientLootLists.Count} client loot lists in {watch.ElapsedMilliseconds}ms.");
        watch.Restart();

        ParseWeapons();
        Logger.LogInformation($"Parsed {Items.Count} items in {watch.ElapsedMilliseconds}ms.");
        watch.Restart();

        ParseAgents();
        Logger.LogInformation($"Parsed {Agents.Count} agents in {watch.ElapsedMilliseconds}ms.");
        watch.Restart();

        ParsePaintkits();
        Logger.LogInformation($"Parsed {Paintkits.Count} paintkits in {watch.ElapsedMilliseconds}ms.");
        watch.Restart();

        ParseWeaponToPaintkits();
        Logger.LogInformation(
            $"Parsed {WeaponToPaintkits.Count} weapon to paintkits in {watch.ElapsedMilliseconds}ms.");
        watch.Restart();

        ParseStickers();
        Logger.LogInformation(
            $"Parsed {Stickers.Count} stickers in {watch.ElapsedMilliseconds}ms.");
        watch.Restart();

        ParseStickerCollections();
        Logger.LogInformation(
            $"Parsed {StickerCollections.Count} sticker collections in {watch.ElapsedMilliseconds}ms.");
        watch.Restart();

        ParseKeychains();
        Logger.LogInformation($"Parsed {Keychains.Count} keychains in {watch.ElapsedMilliseconds}ms.");
        watch.Restart();

        ParseKeychainCollections();
        Logger.LogInformation($"Parsed {KeychainCollections.Count} keychain collections in {watch.ElapsedMilliseconds}ms.");
        watch.Restart();

        ParseMusicKits();
        Logger.LogInformation($"Parsed {MusicKits.Count} music kits in {watch.ElapsedMilliseconds}ms.");
        watch.Restart();

        ParseGraffiti();
        Logger.LogInformation($"Parsed {Graffiti.Count} graffiti and {GraffitiTints.Count} tints in {watch.ElapsedMilliseconds}ms.");
        watch.Restart();

        ParsePatches();
        ParsePatchCollections();
        Logger.LogInformation($"Parsed {Patches.Count} patches and {PatchCollections.Count} patch packs in {watch.ElapsedMilliseconds}ms.");
        watch.Restart();

        var version = new EconVersion { ItemLanguages = Config.ItemLanguages, EconDataVersion = GetVersion(items), SchemaVersion = SchemaVersion };

        File.WriteAllText(Path.Combine(Core.PluginDataDirectory, "version.lock"), JsonSerializer.Serialize(version));

        Logger.LogInformation($"Finished parsing data in {totalWatch.ElapsedMilliseconds}ms.");

        Core.Profiler.RecordTime("ParseEcon", totalWatch.ElapsedMilliseconds);

        var dataDirectory = Core.PluginDataDirectory;

        var options = new JsonSerializerOptions
        {
            WriteIndented = true,
            Encoder =
                JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };

        File.WriteAllText(Path.Combine(dataDirectory, "weapon_to_paintkits.json"),
            JsonSerializer.Serialize(WeaponToPaintkits, options));

        File.WriteAllText(Path.Combine(dataDirectory, "items.json"),
            JsonSerializer.Serialize(Items, options));

        File.WriteAllText(Path.Combine(dataDirectory, "agents.json"),
            JsonSerializer.Serialize(Agents, options));

        File.WriteAllText(Path.Combine(dataDirectory, "sticker_collections.json"),
            JsonSerializer.Serialize(StickerCollections, options));

        File.WriteAllText(Path.Combine(dataDirectory, "keychains.json"),
            JsonSerializer.Serialize(Keychains, options));

        File.WriteAllText(Path.Combine(dataDirectory, "keychain_collections.json"),
            JsonSerializer.Serialize(KeychainCollections, options));

        File.WriteAllText(Path.Combine(dataDirectory, "musickits.json"),
            JsonSerializer.Serialize(MusicKits, options));

        File.WriteAllText(Path.Combine(dataDirectory, "graffiti.json"),
            JsonSerializer.Serialize(Graffiti, options));

        File.WriteAllText(Path.Combine(dataDirectory, "graffiti_tints.json"),
            JsonSerializer.Serialize(GraffitiTints, options));

        File.WriteAllText(Path.Combine(dataDirectory, "graffiti_tournaments.json"),
            JsonSerializer.Serialize(GraffitiTournaments, options));

        File.WriteAllText(Path.Combine(dataDirectory, "patches.json"),
            JsonSerializer.Serialize(Patches, options));

        File.WriteAllText(Path.Combine(dataDirectory, "patch_collections.json"),
            JsonSerializer.Serialize(PatchCollections, options));

        File.WriteAllText(Path.Combine(dataDirectory, "image_keys.json"),
            JsonSerializer.Serialize(ImageKeys, options));

        Stickers.Clear();
        KeychainsByItemName.Clear();
        KeychainBases.Clear();
        ClientLootLists.Clear();
        Rarities.Clear();
        Colors.Clear();
        Paintkits.Clear();
        Languages.Clear();
        RevolvingLootLists.Clear();
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
        var endMemory = GC.GetTotalMemory(true);
        Logger.LogInformation($"Memory usage: {endMemory - startMemory} bytes");
    }

    private RarityDefinition RemapItemRarity(string rarityName)
    {
        var original = Rarities[rarityName].Id;
        if (original >= 6)
        {
            return Rarities[rarityName];
        }

        return Rarities.FirstOrDefault(r => r.Value.Id == original + 1).Value;
    }

    private static readonly Dictionary<string, string> LanguageCodeToTranslationKey = new Dictionary<string, string>
    {
        { "ar", "arabic" },
        { "bg", "bulgarian" },
        { "zh-CN", "schinese" },
        { "zh-TW", "tchinese" },
        { "cs", "czech" },
        { "da", "danish" },
        { "nl", "dutch" },
        { "en", "english" },
        { "fi", "finnish" },
        { "fr", "french" },
        { "de", "german" },
        { "el", "greek" },
        { "hu", "hungarian" },
        { "id", "indonesian" },
        { "it", "italian" },
        { "ja", "japanese" },
        { "ko", "koreana" },
        { "no", "norwegian" },
        { "pl", "polish" },
        { "pt", "portuguese" },
        { "pt-BR", "brazilian" },
        { "ro", "romanian" },
        { "ru", "russian" },
        { "es", "spanish" },
        { "es-419", "latam" },
        { "sv", "swedish" },
        { "th", "thai" },
        { "tr", "turkish" },
        { "uk", "ukrainian" },
        { "vn", "vietnamese" }
    };

    public string GetLocalizedName(Dictionary<string, string> localizedNames, string key)
    {
        if (!LanguageCodeToTranslationKey.TryGetValue(key, out string? translationKey))
        {
            Logger.LogWarning($"Language code {key} not found in LanguageCodeToTranslationKey, using primary language {_PrimaryLanguage}...");
        }
        else if (localizedNames.TryGetValue(translationKey, out string? value))
        {
            return value;
        }

        if (localizedNames.TryGetValue(_PrimaryLanguage, out string? primary))
        {
            return primary;
        }
        // hard-coded english fallback, then whichever language does name it
        if (localizedNames.TryGetValue("english", out string? english))
        {
            return english;
        }
        return localizedNames.Values.FirstOrDefault() ?? string.Empty;
    }

    // Never empty: Valve drops tokens whose items stay in items_game (music kit 2 ships art and tracks but no name).
    private Dictionary<string, string> GetLocalizedNames(string key, string? fallback = null)
    {
        if (!TryGetLocalizedNames(key, out var localizedNames))
        {
            localizedNames[string.Intern(_PrimaryLanguage)] = fallback ?? key.TrimStart('#');
        }
        return localizedNames;
    }

    private bool TryGetLocalizedNames(string key, out Dictionary<string, string> localizedNames)
    {
        if (key.StartsWith("#"))
        {
            key = key[1..];
        }

        localizedNames = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var notFoundLanguages = new List<string>();

        string? fallbackSource1 = null;
        string? fallbackSource2 = null;

        foreach (var (languageName, tokens) in Languages)
        {
            if (_RequiredLanguages.Count > 0 && !_RequiredLanguages.Contains(languageName)
                && languageName != _PrimaryLanguage && languageName != "english")
            {
                continue;
            }

            if (tokens.TryGetValue(key, out var value))
            {
                if (_RequiredLanguages.Count == 0 || _RequiredLanguages.Contains(languageName))
                {
                    localizedNames[string.Intern(languageName)] = value;
                }
            }
            else
            {
                notFoundLanguages.Add(string.Intern(languageName));
            }

            if (fallbackSource1 == null && languageName == _PrimaryLanguage && tokens.TryGetValue(key, out var fb1))
            {
                fallbackSource1 = fb1;
            }
            if (fallbackSource2 == null && languageName == "english" && tokens.TryGetValue(key, out var fb2))
            {
                fallbackSource2 = fb2;
            }
        }

        foreach (var notfoundLanguage in notFoundLanguages)
        {
            if (fallbackSource1 != null)
            {
                localizedNames[string.Intern(notfoundLanguage)] = fallbackSource1;
            }
            else if (fallbackSource2 != null)
            {
                // hard-coded english fallback
                localizedNames[string.Intern(_PrimaryLanguage)] = fallbackSource2;
                localizedNames[string.Intern(notfoundLanguage)] = fallbackSource2;
            }
        }
        return localizedNames.Count > 0;
    }

    private string GetVersion(string content)
    {
        using var sha256 = SHA256.Create();
        var hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(content));
        return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
    }

    private void TrimLanguages(string[] allowedLanguages)
    {
        var allLanguages = Items.First().Value.LocalizedNames.Keys.ToList();
        var languagesToRemove = allLanguages.Where(language => !allowedLanguages.Contains(language)).ToList();

        foreach (var language in languagesToRemove)
        {
            foreach (var (key, value) in WeaponToPaintkits)
            {
                value.ForEach(paintkit => paintkit.LocalizedNames.Remove(language));
            }
            foreach (var (key, value) in StickerCollections)
            {
                value.LocalizedNames.Remove(language);
                value.Stickers.ForEach(sticker => sticker.LocalizedNames.Remove(language));
            }
            foreach (var (key, value) in Items)
            {
                value.LocalizedNames.Remove(language);
            }
            foreach (var (key, value) in Keychains)
            {
                value.LocalizedNames.Remove(language);
            }
        }
    }

    public void ParseWeapons()
    {
        KVObject? FindPrefab(string prefabName)
        {
            foreach (var keys in Root.Children)
            {
                if (keys.Name == "prefabs")
                {
                    foreach (var prefab in keys.Children)
                    {
                        if (prefab.Name == prefabName)
                        {
                            return prefab;
                        }
                    }
                }
            }

            return null;
        }

        foreach (var keys in Root.Children)
        {
            if (keys.Name == "items")
            {
                foreach (var item in keys.Children)
                {
                    bool needParse = false;
                    foreach (var child in item.Children)
                    {
                        if (child.Name == "baseitem")
                        {
                            needParse = true;
                        }

                        if (child.Name == "prefab")
                        {
                            if (child.Value.EToString() == "melee_unusual")
                            {
                                needParse = true;
                            }
                            else if (child.Value.EToString() == "hands_paintable")
                            {
                                needParse = true;
                            }
                        }
                    }

                    if (!needParse)
                    {
                        continue;
                    }

                    string itemName;

                    if (item.HasSubKey("item_name"))
                    {
                        itemName = item.Value["item_name"].EToString();
                    }
                    else
                    {
                        if (!item.HasSubKey("prefab"))
                        {
                            continue;
                        }

                        var prefabName = item.Value["prefab"].EToString();
                        var prefab = FindPrefab(prefabName);
                        if (prefab == null)
                        {
                            continue;
                        }

                        if (!prefab.HasSubKey("item_name"))
                        {
                            continue;
                        }

                        itemName = prefab["item_name"].EToString();
                    }

                    var definition = new ItemDefinition
                    {
                        Name = item.Value["name"].EToString(),
                        Index = int.Parse(item.Name),
                        LocalizedNames = GetLocalizedNames(itemName)
                    };
                    Items[definition.Name] = definition;
                }
            }
        }

        NamedWeapons = Items.Keys.OrderByDescending(i => i.Length).ToList();
    }

    public void ParseAgents()
    {
        KVObject? FindPrefab(string prefabName)
        {
            foreach (var keys in Root.Children)
            {
                if (keys.Name == "prefabs")
                {
                    foreach (var prefab in keys.Children)
                    {
                        if (prefab.Name == prefabName)
                        {
                            return prefab;
                        }
                    }
                }
            }
            return null;
        }

        static string? FindAgentModelInChildren(KVObject obj)
        {
            foreach (var child in obj.Children)
            {
                var value = child.Value.EToString();

                if (!string.IsNullOrWhiteSpace(value))
                {
                    var s = value.Replace('\\', '/');
                    if (s.Contains("/tm_", StringComparison.OrdinalIgnoreCase) ||
                        s.Contains("/ctm_", StringComparison.OrdinalIgnoreCase) ||
                        s.StartsWith("tm_", StringComparison.OrdinalIgnoreCase) ||
                        s.StartsWith("ctm_", StringComparison.OrdinalIgnoreCase))
                    {
                        return s;
                    }
                }

                if (child.Children.Any())
                {
                    var nested = FindAgentModelInChildren(child);
                    if (!string.IsNullOrWhiteSpace(nested))
                    {
                        return nested;
                    }
                }
            }
            return null;
        }

        foreach (var section in Root.Children)
        {
            if (section.Name != "items")
            {
                continue;
            }

            foreach (var item in section.Children)
            {
                var prefabName = item.HasSubKey("prefab") ? item.Value["prefab"].EToString() : string.Empty;

                // Only include customplayertradable items (the actual selectable agents)
                if (!prefabName.Equals("customplayertradable", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                var prefab = FindPrefab(prefabName);

                // Try to find the agent model path
                string? modelPath = FindAgentModelInChildren(item);
                if (string.IsNullOrWhiteSpace(modelPath) && prefab != null)
                {
                    modelPath = FindAgentModelInChildren(prefab);
                }

                if (string.IsNullOrWhiteSpace(modelPath))
                {
                    continue;
                }

                modelPath = modelPath.Replace('\\', '/');

                // Ensure the model path starts with characters/models/
                string fullModelPath = modelPath;
                if (!fullModelPath.StartsWith("characters/models/", StringComparison.OrdinalIgnoreCase))
                {
                    if (fullModelPath.Contains("characters/models/", StringComparison.OrdinalIgnoreCase))
                    {
                        var idx = fullModelPath.IndexOf("characters/models/", StringComparison.OrdinalIgnoreCase);
                        fullModelPath = fullModelPath.Substring(idx);
                    }
                }

                // Ensure .vmdl extension
                if (!fullModelPath.EndsWith(".vmdl", StringComparison.OrdinalIgnoreCase))
                {
                    fullModelPath += ".vmdl";
                }

                // Extract the agent name from the path (remove characters/models/ prefix and .vmdl extension)
                string normalizedPath = fullModelPath;
                if (normalizedPath.StartsWith("characters/models/", StringComparison.OrdinalIgnoreCase))
                {
                    normalizedPath = normalizedPath.Substring("characters/models/".Length);
                }
                if (normalizedPath.EndsWith(".vmdl", StringComparison.OrdinalIgnoreCase))
                {
                    normalizedPath = normalizedPath.Substring(0, normalizedPath.Length - 5);
                }

                string? itemName = null;
                if (item.HasSubKey("item_name"))
                {
                    itemName = item.Value["item_name"].EToString();
                }
                else if (prefab != null && prefab.HasSubKey("item_name"))
                {
                    itemName = prefab["item_name"].EToString();
                }

                // Get rarity information (same as keychains)
                var rarityName = item.Value["item_rarity"];
                RarityDefinition rarity = rarityName == null ? Rarities["default"] : Rarities[rarityName.EToString()];

                // Use the normalized path as the internal name
                var internalName = normalizedPath;

                var definition = new AgentDefinition
                {
                    Name = internalName,
                    Index = int.Parse(item.Name),
                    ModelPath = fullModelPath,
                    LocalizedNames = !string.IsNullOrWhiteSpace(itemName) ? GetLocalizedNames(itemName) : new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase),
                    Rarity = rarity
                };

                Agents[definition.Name] = definition;
                AddImageKey("agent:" + definition.Name, item.HasSubKey("image_inventory") ? item.Value["image_inventory"].EToString() : null);
            }
        }

        Logger.LogInformation($"ParseAgents completed. Total agents found: {Agents.Count}");
    }

    public void ParseMusicKits()
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        foreach (var section in Root.Children)
        {
            if (section.Name != "music_definitions")
            {
                continue;
            }

            foreach (var musicKit in section.Children)
            {
                var internalName = musicKit.Name;
                // The entry key is the kit's id; its readable code name ("valve_02") is the name field.
                var codeName = musicKit.HasSubKey("name") ? musicKit.Value["name"].EToString() : internalName;

                string? itemName = null;
                if (musicKit.HasSubKey("loc_name"))
                {
                    itemName = musicKit.Value["loc_name"].EToString();
                }
                else if (musicKit.HasSubKey("name"))
                {
                    itemName = musicKit.Value["name"].EToString();
                }

                var index = musicKit.HasSubKey("id") ? musicKit.Value["id"].EToInt32() : 0;
                if (index == 0 && int.TryParse(musicKit.Name, out var parsedIndex))
                {
                    index = parsedIndex;
                }

                var definition = new MusicKitDefinition
                {
                    Name = internalName,
                    Index = index,
                    LocalizedNames = GetLocalizedNames(itemName ?? string.Empty, codeName),
                    Rarity = Rarities.ContainsKey("default") ? Rarities["default"] : new RarityDefinition { Name = "default", Id = 0, Color = new ColorDefinition { Name = "default", HexColor = "FFFFFF" } }
                };

                MusicKits[definition.Name] = definition;
                AddImageKey("music:" + definition.Name, musicKit.HasSubKey("image_inventory") ? musicKit.Value["image_inventory"].EToString() : null);
            }
        }

        stopwatch.Stop();
        Logger.LogInformation($"Parsed {MusicKits.Count} music kits in {stopwatch.ElapsedMilliseconds}ms.");
    }

    // Graffiti are sticker kits, picked out by cs2-lib's test; "default" ones are plain shapes sprayed in a tint.
    public void ParseGraffiti()
    {
        foreach (var section in Root.Children)
        {
            if (section.Name == "graffiti_tints")
            {
                foreach (var tint in section.Children)
                {
                    var index = tint.Value["id"].EToInt32();
                    GraffitiTints[tint.Name] = new GraffitiTintDefinition
                    {
                        Name = tint.Name,
                        Index = index,
                        HexColor = tint.Value["hex_color"].EToString(),
                        LocalizedNames = GetLocalizedNames($"#Attrib_SprayTintValue_{index}", tint.Name)
                    };
                }
            }
            else if (section.Name == "sticker_kits")
            {
                foreach (var kit in section.Children)
                {
                    var name = kit.Value["name"]?.EToString() ?? string.Empty;
                    var itemName = kit.Value["item_name"]?.EToString() ?? string.Empty;
                    var material = kit.Value["sticker_material"]?.EToString() ?? string.Empty;
                    if (!name.StartsWith("spray_", StringComparison.OrdinalIgnoreCase)
                        && !itemName.Contains("#SprayKit", StringComparison.OrdinalIgnoreCase)
                        && !material.Contains("_graffiti", StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    var rarityName = kit.Value["item_rarity"];
                    var definition = new GraffitiDefinition
                    {
                        Name = name,
                        Index = int.Parse(kit.Name),
                        LocalizedNames = GetLocalizedNames(itemName, name),
                        Rarity = rarityName == null ? Rarities["default"] : Rarities[rarityName.EToString()],
                        Tintable = material.StartsWith("default", StringComparison.OrdinalIgnoreCase)
                    };
                    Graffiti[definition.Name] = definition;
                    AddImageKey("graffiti:" + definition.Name, "stickers/" + material);

                    if (kit.Value["tournament_event_id"] is not { } eventValue)
                    {
                        continue;
                    }

                    var eventId = eventValue.EToInt32();
                    if (!GraffitiTournaments.TryGetValue(eventId, out var tournament))
                    {
                        tournament = new GraffitiTournamentDefinition
                        {
                            EventId = eventId,
                            LocalizedNames = GetLocalizedNames($"#CSGO_Tournament_Event_NameShort_{eventId}"),
                            Graffiti = []
                        };
                        GraffitiTournaments[eventId] = tournament;
                    }

                    tournament.Graffiti.Add(definition.Name);

                    // Team 0 is the organiser, so its logo graffiti pictures the tournament.
                    if (kit.Value["tournament_team_id"]?.EToInt32() == 0)
                    {
                        AddImageKey("graffititournament:" + eventId, "stickers/" + material);
                    }
                }
            }
        }
    }

    // Patches are the sticker kits with a patch_material instead of a sticker_material.
    public void ParsePatches()
    {
        foreach (var section in Root.Children)
        {
            if (section.Name != "sticker_kits")
            {
                continue;
            }

            foreach (var kit in section.Children)
            {
                var material = kit.Value["patch_material"]?.EToString();
                if (string.IsNullOrEmpty(material))
                {
                    continue;
                }

                var name = kit.Value["name"].EToString();
                var rarityName = kit.Value["item_rarity"];
                var definition = new PatchDefinition
                {
                    Name = name,
                    Index = int.Parse(kit.Name),
                    LocalizedNames = GetLocalizedNames(kit.Value["item_name"]?.EToString() ?? string.Empty, name),
                    Rarity = rarityName == null ? Rarities["default"] : Rarities[rarityName.EToString()]
                };
                Patches[definition.Name] = definition;
                AddImageKey("patch:" + definition.Name, "patches/" + material);
            }
        }
    }

    // Patch packs are the items tagged PatchCapsule; their loot lists name patches as "[name]patch".
    public void ParsePatchCollections()
    {
        foreach (var section in Root.Children)
        {
            if (section.Name != "items")
            {
                continue;
            }

            foreach (var item in section.Children)
            {
                if (item.GetSubKey("tags")?.GetSubKey("PatchCapsule") is not { } tag)
                {
                    continue;
                }

                var name = item.Value["name"].EToString();
                var series = item.GetSubKey("attributes")?.GetSubKey("set supply crate series");
                var seriesIndex = series?.HasSubKey("value") == true
                    ? series.Value["value"].EToString()
                    : series?.Value.EToString();
                if (!ClientLootLists.TryGetValue(name, out var lootList)
                    && !ClientLootLists.TryGetValue(tag.Value["tag_value"]?.EToString() ?? string.Empty, out lootList)
                    && !(seriesIndex is not null
                         && RevolvingLootLists.TryGetValue(seriesIndex, out var revolving)
                         && ClientLootLists.TryGetValue(revolving, out lootList)))
                {
                    Logger.LogDebug($"Patch pack {name} not found in ClientLootLists");
                    continue;
                }

                var patches = lootList.Items
                    .Where(entry => entry.BelongingItemName == "patch")
                    .Select(entry => Patches.GetValueOrDefault(entry.Name))
                    .OfType<PatchDefinition>()
                    .Distinct()
                    .ToList();
                if (patches.Count == 0)
                {
                    continue;
                }

                PatchCollections[name] = new PatchCollectionDefinition
                {
                    Name = name,
                    Index = int.Parse(item.Name),
                    LocalizedNames = GetLocalizedNames(item.Value["item_name"]?.EToString() ?? string.Empty, name),
                    Patches = patches
                };
                AddImageKey("patchcapsule:" + name, ImageInventory(item));
            }
        }
    }

    public void ParseColors()
    {
        foreach (var keys in Root.Children)
        {
            if (keys.Name == "colors")
            {
                foreach (var color in keys.Children)
                {
                    var definition = new ColorDefinition
                    {
                        Name = color.Name,
                        HexColor = color.Value["hex_color"].EToString()
                    };

                    Colors[definition.Name] = definition;
                }
            }
        }
    }

    public void ParseRarities()
    {
        foreach (var keys in Root.Children)
        {
            if (keys.Name == "rarities")
            {
                foreach (var rarity in keys.Children)
                {
                    var definition = new RarityDefinition
                    {
                        Name = rarity.Name,
                        Id = rarity.Value["value"].EToInt32(),
                        Color = Colors[rarity.Value["color"].EToString()]
                    };
                    Rarities[definition.Name] = definition;
                }
            }
        }
    }

    public void ParseRevolvingLootLists()
    {
        foreach (var keys in Root.Children)
        {
            if (keys.Name == "revolving_loot_lists")
            {
                foreach (var revolvingLootList in keys.Children)
                {
                    RevolvingLootLists[revolvingLootList.Name] = revolvingLootList.Value.EToString();
                }
            }
        }
    }

    public void ParseLanguages()
    {
        var regex = new Regex(
            @"""((?:[^""\\]|\\.)*)""\s+""((?:[^""\\]|\\.)*)""",
            RegexOptions.Compiled
        );
        var languages = Core.GameFileSystem.FindFileAbsoluteList("resource/csgo_*", "GAME");
        foreach (var language in languages)
        {
            var languagePath = language.Split(':').Last();
            var content = Core.GameFileSystem.ReadFile(languagePath, "GAME")[1..]; // BOM
            var reader = new StringReader(content);
            var languageName = languagePath.Split('/').Last().Split('\\').Last().Split('.').First().Split("_").Last();
            Languages[languageName] = new(StringComparer.OrdinalIgnoreCase);

            bool started = false;
            while (reader.ReadLine() is { } line)
            {
                line = line.Trim();
                if (line == "\"Tokens\"")
                {
                    started = true;
                    continue;
                }

                if (!started)
                {
                    continue;
                }

                if (line.StartsWith("//"))
                {
                    continue;
                }

                var match = regex.Match(line);
                if (match.Success)
                {
                    var key = match.Groups[1].Value;
                    var value = match.Groups[2].Value;
                    Languages[languageName][key] = value;
                }
            }
        }
    }

    public void ParseClientLootLists()
    {
        var lootEntries = new Dictionary<string, KVObject>();
        foreach (var keys in Root.Children)
        {
            if (keys.Name == "client_loot_lists")
            {
                foreach (var item in keys.Children)
                {
                    lootEntries[item.Name] = item;
                }
            }
        }

        var patches = new Dictionary<string, List<string>>()
        {
            ["crate_signature_pack_eslcologne2015_group_1"] =
            [
                "crate_signature_pack_eslcologne2015_group_1_rare",
                "crate_signature_pack_eslcologne2015_group_1_legendary"
            ],
            ["crate_signature_pack_eslcologne2015_group_2"] =
            [
                "crate_signature_pack_eslcologne2015_group_2_rare",
                "crate_signature_pack_eslcologne2015_group_2_legendary"
            ],
            ["crate_signature_pack_eslcologne2015_group_3"] =
            [
                "crate_signature_pack_eslcologne2015_group_3_rare",
                "crate_signature_pack_eslcologne2015_group_3_legendary"
            ],
            ["crate_signature_pack_eslcologne2015_group_4"] =
            [
                "crate_signature_pack_eslcologne2015_group_4_rare",
                "crate_signature_pack_eslcologne2015_group_4_legendary"
            ],
            ["crate_signature_pack_cluj2015_group_1"] =
            [
                "crate_signature_pack_cluj2015_group_1_rare",
                "crate_signature_pack_cluj2015_group_1_legendary"
            ],
            ["crate_signature_pack_cluj2015_group_2"] =
            [
                "crate_signature_pack_cluj2015_group_2_rare",
                "crate_signature_pack_cluj2015_group_2_legendary"
            ],
        };

        foreach (var (name, items) in patches)
        {
            var values = items.Select(item => new KVObject(item, "1"));
            var obj = new KVObject(name, values);
            lootEntries[name] = obj;
        }


        foreach (var (name, item) in lootEntries)
        {
            foreach (var child in item.Children)
            {
                if (child.Name.Contains("["))
                {
                    goto notMainEntry;
                }
            }

            var items = new List<ClientLootItemDefinition>();

            foreach (var child in item.Children)
            {
                if (lootEntries.TryGetValue(child.Name, out var collection))
                {
                    foreach (var collectionChild in collection.Children)
                    {
                        var split = collectionChild.Name.Split("]");

                        if (split.Length != 2)
                        {
                            continue;
                        }

                        var itemName = split[0][1..];
                        var belongingItemName = split[1];

                        items.Add(new ClientLootItemDefinition
                        {
                            Name = itemName,
                            BelongingItemName = belongingItemName,
                        });
                    }
                }
            }

            var definition = new ClientLootListDefinition { Name = name, Items = items, };
            ClientLootLists[definition.Name] = definition;

        notMainEntry:;
        }
    }

    public void ParsePaintkits()
    {
        Dictionary<string, RarityDefinition> paintkitRarities = new();
        foreach (var keys in Root.Children)
        {
            if (keys.Name == "paint_kits_rarity")
            {
                foreach (var rarity in keys.Children)
                {
                    paintkitRarities[rarity.Name] = RemapItemRarity(rarity.Value.EToString());
                }
            }
        }

        foreach (var keys in Root.Children)
        {
            if (keys.Name == "paint_kits")
            {
                foreach (var paintkit in keys.Children)
                {
                    if (!paintkitRarities.ContainsKey(paintkit.Value["name"].EToString()))
                    {
                        Logger.LogDebug(
                            $"Paintkit {paintkit.Value["name"].EToString()} not found in paintkitRarities");
                        continue;
                    }

                    var tag = paintkit.Value["description_tag"].EToString();
                    var definition = new PaintkitDefinition
                    {
                        Index = int.Parse(paintkit.Name),
                        Name = paintkit.Value["name"].EToString(),
                        UseLegacyModel = paintkit.HasSubKeyWithValue("use_legacy_model", "1"),
                        DescriptionTag = tag,
                        LocalizedNames = GetLocalizedNames(tag),
                        Rarity = paintkitRarities[paintkit.Value["name"].EToString()]
                    };
                    Paintkits[definition.Name] = definition;
                }
            }
        }
    }

    public void ParseWeaponToPaintkits()
    {
        var textures = Core.GameFileSystem.FindFileAbsoluteList("panorama/images/econ/default_generated/*", "GAME");
        foreach (var texture in textures)
        {
            var absolutePath = texture.Split('/').Last().Split("\\").Last();
            if (!absolutePath.EndsWith("_medium_png.vtex_c"))
            {
                continue;
            }

            var fullName = absolutePath.Replace("_medium_png.vtex_c", "");

            foreach (var weapon in NamedWeapons)
            {
                if (fullName.StartsWith(weapon))
                {
                    var paintKitName = fullName.Replace(weapon + "_", "");
                    if (Paintkits.TryGetValue(paintKitName, out var paintkit))
                    {
                        WeaponToPaintkits.GetOrAdd(weapon, () => new()).Add(paintkit);
                    }
                    else
                    {
                        Logger.LogDebug($"Paintkit: {paintKitName} not found in Paintkits");
                        continue;
                    }

                    break;
                }
            }
        }
    }

    private void ParseStickersInternal(Dictionary<string, KVObject> items,
        string texturePath,
        string materialPrefix)
    {
        var textureDirs = Core.GameFileSystem.FindFileAbsoluteList(Path.Combine(texturePath, "*"), "GAME");
        foreach (var textureDir in textureDirs)
        {
            var collectionName = textureDir.Split('/').Last().Split('\\').Last();
            var textures =
                Core.GameFileSystem.FindFileAbsoluteList(
                    Path.Combine(texturePath, collectionName, "*"), "GAME");
            foreach (var texture in textures)
            {
                var fileName = texture.Split('/').Last().Split('\\').Last();
                if (!fileName.EndsWith("_png.vtex_c"))
                {
                    ParseStickersInternal(items, $"{texturePath}/{collectionName}",
                        $"{materialPrefix}{collectionName}/");
                    continue;
                }

                var itemName = fileName.Replace("_png.vtex_c", "");

                if (itemName.EndsWith("_1355_37"))
                {
                    continue;
                }

                var materialName = $"{materialPrefix}{collectionName}/{itemName}";
                if (items.TryGetValue(materialName, out var item))
                {
                    var localizedNames = GetLocalizedNames(item.Value["item_name"].EToString());

                    var rarityName = item.Value["item_rarity"];
                    RarityDefinition rarity =
                        rarityName == null ? Rarities["default"] : Rarities[rarityName.EToString()];

                    var definition = new StickerDefinition
                    {
                        Name = item.Value["name"].EToString(),
                        Index = int.Parse(item.Name),
                        LocalizedNames = localizedNames,
                        Rarity = rarity
                    };
                    Stickers[definition.Name] = definition;
                    AddImageKey("sticker:" + definition.Name, "stickers/" + materialName);
                }
                else
                {
                    Logger.LogDebug($"Sticker: {materialName} not found in items");
                }
            }
        }
    }

    public void ParseStickers()
    {
        // temporary
        var items = new Dictionary<string, KVObject>();

        foreach (var keys in Root.Children)
        {
            if (keys.Name == "sticker_kits")
            {
                foreach (var item in keys.Children)
                {
                    if (item.HasSubKey("sticker_material"))
                    {
                        items[item.Value["sticker_material"].EToString()] = item;
                    }
                }
            }
        }


        ParseStickersInternal(items, "panorama/images/econ/stickers", "");
    }

    public void ParseStickerCollections()
    {
        foreach (var keys in Root.Children)
        {
            if (keys.Name == "items")
            {
                foreach (var stickerCollections in keys.Children)
                {
                    if (stickerCollections.HasSubKey("tags") &&
                        stickerCollections.GetSubKey("tags")!.HasSubKey("StickerCapsule"))
                    {
                        var name = stickerCollections.Value["name"].EToString();
                        var index = int.Parse(stickerCollections.Name);
                        var localizedNames = GetLocalizedNames(stickerCollections.Value["item_name"].EToString());
                        ClientLootListDefinition? lootList = null;
                        var lootListName = name;
                        var lootListName2 =
                            stickerCollections.GetSubKey("tags")!.GetSubKey("StickerCapsule")!.Value["tag_value"]!
                                .EToString();
                        if (!ClientLootLists.TryGetValue(lootListName, out lootList)
                            && !ClientLootLists.TryGetValue(lootListName2, out lootList))
                        {
                            Logger.LogWarning($"Sticker collection {name} not found in ClientLootLists");
                            continue;
                        }



                        var stickers = new List<StickerDefinition>();
                        foreach (var item in lootList.Items)
                        {
                            stickers.Add(Stickers[item.Name]);
                        }

                        var definition = new StickerCollectionDefinition
                        {
                            Name = name,
                            Index = index,
                            LocalizedNames = localizedNames,
                            Stickers = stickers,
                        };
                        StickerCollections[definition.Name] = definition;
                        AddImageKey("capsule:" + definition.Name, ImageInventory(stickerCollections));
                    }
                    else if (stickerCollections.HasSubKey("prefab"))
                    {
                        var prefab = stickerCollections.Value["prefab"].EToString();
                        // Stockholm 2021's patch packs share the capsule prefab naming.
                        if (!prefab.Contains("_capsule_prefab")
                            || stickerCollections.GetSubKey("tags")?.HasSubKey("PatchCapsule") == true)
                        {
                            continue;
                        }

                        if (!stickerCollections.HasSubKey("attributes"))
                        {
                            continue;
                        }

                        var attributes = stickerCollections.GetSubKey("attributes")!;
                        if (!attributes.HasSubKey("set supply crate series"))
                        {
                            continue;
                        }
                        var supplySeriesKey = attributes.GetSubKey("set supply crate series")!;
                        var revolvingIndex = supplySeriesKey.HasSubKey("value")
                            ? supplySeriesKey.Value["value"].EToString()
                            : supplySeriesKey.Value.EToString();
                        if (RevolvingLootLists.TryGetValue(revolvingIndex, out var revolvingLootListName))
                        {
                            if (ClientLootLists.TryGetValue(revolvingLootListName, out var lootList))
                            {
                                var name = stickerCollections.Value["name"].EToString();
                                var index = int.Parse(stickerCollections.Name);
                                var localizedNames =
                                    GetLocalizedNames(stickerCollections.Value["item_name"].EToString());
                                var stickers = new List<StickerDefinition>();
                                foreach (var item in lootList.Items)
                                {
                                    if (item.BelongingItemName != "sticker")
                                    {
                                        continue;
                                    }
                                    stickers.Add(Stickers[item.Name]);
                                }

                                if (stickers.Count == 0)
                                {
                                    continue;
                                }

                                var definition = new StickerCollectionDefinition
                                {
                                    Name = name,
                                    Index = index,
                                    LocalizedNames = localizedNames,
                                    Stickers = stickers,
                                };
                                StickerCollections[definition.Name] = definition;
                                AddImageKey("capsule:" + definition.Name, ImageInventory(stickerCollections));

                            }
                        }

                    }
                }
            }
        }
    }

    public void ParseKeychains()
    {
        foreach (var keys in Root.Children)
        {
            if (keys.Name == "keychain_definitions")
            {
                foreach (var keychain in keys.Children)
                {
                    var rarityName = keychain.Value["item_rarity"];
                    RarityDefinition rarity =
                        rarityName == null ? Rarities["default"] : Rarities[rarityName.EToString()];

                    var definition = new KeychainDefinition
                    {
                        Name = keychain.Name,
                        Index = int.Parse(keychain.Name),
                        LocalizedNames = GetLocalizedNames(keychain.Value["loc_name"].EToString()),
                        Rarity = rarity
                    };
                    Keychains[definition.Name] = definition;
                    if (keychain.HasSubKey("name"))
                    {
                        KeychainsByItemName[keychain.Value["name"].EToString()] = definition;
                    }
                    if (keychain.HasSubKey("base"))
                    {
                        KeychainBases[definition.Name] = keychain.Value["base"].EToString();
                    }
                    AddImageKey("keychain:" + definition.Name, ImageInventory(keychain));
                }
            }
        }
    }

    // Each collection is a "keychain_pack_<set>_lootlist"; the game names it and draws its icon from <set>.
    public void ParseKeychainCollections()
    {
        const string prefix = "keychain_pack_";
        const string suffix = "_lootlist";
        foreach (var (name, lootList) in ClientLootLists)
        {
            if (!name.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)
                || !name.EndsWith(suffix, StringComparison.OrdinalIgnoreCase)
                || lootList.Items.Any(item => item.BelongingItemName != "keychain"))
            {
                continue;
            }

            var set = name[prefix.Length..^suffix.Length];
            if (!TryGetLocalizedNames($"CSGO_crate_keychain_pack_{set}_capsule", out var localizedNames))
            {
                Logger.LogDebug($"Keychain collection {name} has no localized name");
                continue;
            }

            var keychains = new List<KeychainDefinition>();
            foreach (var item in lootList.Items)
            {
                if (KeychainsByItemName.TryGetValue(item.Name, out var keychain) && !keychains.Contains(keychain))
                {
                    keychains.Add(keychain);
                }
            }

            if (keychains.Count == 0)
            {
                continue;
            }

            var definition = new KeychainCollectionDefinition
            {
                Name = name,
                LocalizedNames = localizedNames,
                Keychains = keychains,
            };
            KeychainCollections[definition.Name] = definition;
            AddImageKey("keychaincollection:" + definition.Name, $"set_icons/set_{set}");
        }

        // Highlight charms carry no art of their own: the game draws each as the souvenir charm it is based on.
        foreach (var highlights in KeychainBases.GroupBy(entry => entry.Value, StringComparer.OrdinalIgnoreCase))
        {
            if (!KeychainsByItemName.TryGetValue(highlights.Key, out var baseKeychain))
            {
                continue;
            }

            var definition = new KeychainCollectionDefinition
            {
                Name = highlights.Key,
                LocalizedNames = baseKeychain.LocalizedNames,
                Keychains = [baseKeychain, .. highlights.Select(entry => Keychains[entry.Key])],
            };
            KeychainCollections[definition.Name] = definition;
            var image = GetImageKey("keychain", baseKeychain.Name);
            AddImageKey("keychaincollection:" + definition.Name, image);
            foreach (var entry in highlights)
            {
                AddImageKey("keychain:" + entry.Key, image);
            }
        }
    }

    private static string? ImageInventory(KVObject item) =>
        item.HasSubKey("image_inventory") ? item.Value["image_inventory"].EToString() : null;

    private void AddImageKey(string key, string? imageInventory)
    {
        if (string.IsNullOrWhiteSpace(imageInventory))
        {
            return;
        }

        var path = imageInventory.Replace('\\', '/');
        path = path.StartsWith("econ/", StringComparison.OrdinalIgnoreCase) ? path["econ/".Length..] : path;

        // Some items name art the game does not ship (Budapest 2025's charm); those get no key
        // rather than a path to a missing file.
        if (!Core.GameFileSystem.FileExists($"panorama/images/econ/{path}_png.vtex_c", "GAME"))
        {
            return;
        }

        ImageKeys[key] = path;
    }

    public string? GetImageKey(string kind, string name) =>
        ImageKeys.TryGetValue(kind + ":" + name, out var key) ? key : null;
}
