using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.RegularExpressions;

using Microsoft.Extensions.Logging;

using SwiftlyS2.Shared;

using ValveKeyValue;

using WeaponSkins.Shared;
using AgentDefinition = WeaponSkins.Shared.AgentDefinition;

namespace WeaponSkins.Econ;

public class EconService
{
    private ISwiftlyCore Core { get; init; }
    private KVObject Root { get; set; }
    private ILogger<EconService> Logger { get; init; }

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

    public Dictionary<string /* Name */, AgentDefinition> Agents { get; set; } = new(StringComparer.OrdinalIgnoreCase);

    private Dictionary<string /* Language */, Dictionary<string /* Key */, string /* Value */>> Languages { get; } =
        new(StringComparer.OrdinalIgnoreCase);

    private Dictionary<string, string> RevolvingLootLists { get; } = new(StringComparer.OrdinalIgnoreCase);

    private const int SchemaVersion = 17;

    public EconService(ISwiftlyCore core,
        ILogger<EconService> logger)
    {
        Core = core;
        Logger = logger;

        var items = Core.GameFileSystem.ReadFile("scripts/items/items_game.txt", "GAME");
        var version = GetVersion(items);
        string[] allowedLanguages = ["english", "schinese"];
        if (File.Exists(Path.Combine(Core.PluginDataDirectory, "version.lock")))
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            var startMemory = GC.GetTotalMemory(true);
            var lockVersion =
                JsonSerializer.Deserialize<EconVersion>(
                    File.ReadAllText(Path.Combine(Core.PluginDataDirectory, "version.lock")));
            if (lockVersion?.EconDataVersion == version && lockVersion.SchemaVersion == SchemaVersion)
            {
                Logger.LogInformation("Econ data is up to date, skipping parsing...");
                // TODO read from json
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

                // TrimLanguages(allowedLanguages);
                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();
                var endMemory = GC.GetTotalMemory(true);
                Logger.LogInformation($"Memory usage: {endMemory - startMemory} bytes");
                return;
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

        var version = new EconVersion { EconDataVersion = GetVersion(items), SchemaVersion = SchemaVersion };

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

        Stickers.Clear();
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

    private Dictionary<string, string> GetLocalizedNames(string key)
    {
        if (key.StartsWith("#"))
        {
            key = key.Substring(1);
        }

        var localizedNames = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var notFoundLanguages = new List<string>();
        foreach (var (languageName, tokens) in Languages)
        {
            if (tokens.TryGetValue(key, out var value))
            {
                localizedNames[string.Intern(languageName)] = value;
            }
            else
            {
                notFoundLanguages.Add(string.Intern(languageName));
            }
        }

        if (localizedNames.ContainsKey("english"))
        {
            foreach (var notfoundLanguage in notFoundLanguages)
            {
                localizedNames[string.Intern(notfoundLanguage)] = localizedNames["english"];
            }
        }

        return localizedNames;
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
            }
        }
        
        Logger.LogInformation($"ParseAgents completed. Total agents found: {Agents.Count}");
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
                        Name = color.Name, HexColor = color.Value["hex_color"].EToString()
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
                            Name = itemName, BelongingItemName = belongingItemName,
                        });
                    }
                }
            }

            var definition = new ClientLootListDefinition { Name = name, Items = items, };
            ClientLootLists[definition.Name] = definition;

            notMainEntry: ;
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
                            Name = name, Index = index, LocalizedNames = localizedNames, Stickers = stickers,
                        };
                        StickerCollections[definition.Name] = definition;
                    } else if (stickerCollections.HasSubKey("prefab"))
                    {
                        var prefab = stickerCollections.Value["prefab"].EToString();
                        if (!prefab.Contains("_capsule_prefab"))
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
                        var revolvingIndex = attributes.GetSubKey("set supply crate series")!.Value["value"].EToString();
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

                                var definition = new StickerCollectionDefinition
                                {
                                    Name = name, Index = index, LocalizedNames = localizedNames, Stickers = stickers,
                                };
                                StickerCollections[definition.Name] = definition;

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
                }
            }
        }
    }
}