using System.Diagnostics;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.RegularExpressions;

using Microsoft.Extensions.Logging;

using SwiftlyS2.Shared;

using ValveKeyValue;

namespace WeaponSkins.Econ;

public class EconService
{
    private ISwiftlyCore Core { get; init; }
    private KVObject Root { get; set; }
    private ILogger<EconService> Logger { get; init; }

    public Dictionary<string /* Name */, ItemDefinition> Items { get; set; } = new(StringComparer.OrdinalIgnoreCase);
    public List<string> NamedWeapons { get; set; } = new();
    public Dictionary<string /* Name */, RarityDefinition> Rarities { get; } = new(StringComparer.OrdinalIgnoreCase);
    public Dictionary<string /* Name */, ColorDefinition> Colors { get; } = new(StringComparer.OrdinalIgnoreCase);
    public Dictionary<string /* Name */, PaintkitDefinition> Paintkits { get; } = new(StringComparer.OrdinalIgnoreCase);

    public Dictionary<string /* Name */, List<PaintkitDefinition>> WeaponToPaintkits { get; } =
        new(StringComparer.OrdinalIgnoreCase);

    public Dictionary<string /* Collection */, List<StickerDefinition>> Stickers { get; } =
        new(StringComparer.OrdinalIgnoreCase);

    public Dictionary<string /* Name */, KeychainDefinition> Keychains { get; } = new(StringComparer.OrdinalIgnoreCase);

    private Dictionary<string /* Language */, Dictionary<string /* Key */, string /* Value */>> Languages { get; } =
        new(StringComparer.OrdinalIgnoreCase);

    private Dictionary<string, string> _RemappedRarityColor = new()
    {
        ["desc_legendary"] = "#eb4b4b",
        ["desc_mythical"] = "#d32ce6",
        ["desc_rare"] = "#8847ff",
        ["desc_uncommon"] = "#4b69ff",
        ["desc_common"] = "#5e98d9",
    };

    public EconService(ISwiftlyCore core,
        ILogger<EconService> logger)
    {
        Core = core;
        Logger = logger;

        var items = Core.GameFileSystem.ReadFile("scripts/items/items_game.txt", "GAME");
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
        ParseWeapons();
        watch.Restart();
        Logger.LogInformation($"Parsed {Items.Count} items in {watch.ElapsedMilliseconds}ms.");
        ParsePaintkits();
        Logger.LogInformation($"Parsed {Paintkits.Count} paintkits in {watch.ElapsedMilliseconds}ms.");
        watch.Restart();
        ParseWeaponToPaintkits();
        Logger.LogInformation(
            $"Parsed {WeaponToPaintkits.Count} weapon to paintkits in {watch.ElapsedMilliseconds}ms.");
        watch.Restart();
        ParseStickers();
        Logger.LogInformation(
            $"Parsed {Stickers.Count} sticker collections (stickers count: {Stickers.Values.Sum(s => s.Count)}) in {watch.ElapsedMilliseconds}ms.");
        watch.Restart();
        ParseKeychains();
        Logger.LogInformation($"Parsed {Keychains.Count} keychains in {watch.ElapsedMilliseconds}ms.");
        watch.Restart();
        Logger.LogInformation($"Finished parsing data in {totalWatch.ElapsedMilliseconds}ms.");

        Core.Profiler.RecordTime("ParseEcon", totalWatch.ElapsedMilliseconds);

        // foreach (var (name, rarity) in Rarities)
        // {
        //     Console.WriteLine($"Rarity: {name}, Id: {rarity.Id}, Color: {rarity.Color}");
        // }

        // foreach (var (name, paintkit) in Paintkits)
        // {
        //     Console.WriteLine($"Paintkit: {name}, Value: {paintkit}");
        // }

        // foreach (var (name, paintkits) in WeaponToPaintkits)
        // {
        //     Console.WriteLine($"Weapon: {name}, Paintkits: {string.Join(", ", paintkits)}");
        // }

        // foreach (var (language, tokens) in Languages)
        // {
        //     Console.WriteLine($"Language: {language}");
        //     foreach (var (key, value) in tokens)
        //     {
        //         Console.WriteLine($"  {key}: {value}");
        //     }
        // }

        var dataDirectory = Core.PluginDataDirectory;

        // File.WriteAllText(Path.Combine(dataDirectory, "languages.json"),
        //     JsonSerializer.Serialize(Languages, new JsonSerializerOptions
        //     {
        //         WriteIndented = true,
        //         Encoder =
        //             JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        //     }));

        File.WriteAllText(Path.Combine(dataDirectory, "weapon_to_paintkits.json"),
            JsonSerializer.Serialize(WeaponToPaintkits, new JsonSerializerOptions
            {
                WriteIndented = true,
                Encoder =
                    JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            }));

        File.WriteAllText(Path.Combine(dataDirectory, "items.json"),
            JsonSerializer.Serialize(Items, new JsonSerializerOptions
            {
                WriteIndented = true,
                Encoder =
                    JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            }));

        File.WriteAllText(Path.Combine(dataDirectory, "stickers.json"),
            JsonSerializer.Serialize(Stickers, new JsonSerializerOptions
            {
                WriteIndented = true,
                Encoder =
                    JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            }));

        File.WriteAllText(Path.Combine(dataDirectory, "keychains.json"),
            JsonSerializer.Serialize(Keychains, new JsonSerializerOptions
            {
                WriteIndented = true,
                Encoder =
                    JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            }));
        Languages.Clear();
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

                    var key = itemName.Replace("#", "");

                    var localizedNames = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                    foreach (var (languageName, tokens) in Languages)
                    {
                        if (tokens.TryGetValue(key, out var value))
                        {
                            localizedNames[languageName] = value;
                        }
                    }

                    var definition = new ItemDefinition
                    {
                        Name = item.Value["name"].EToString(),
                        Index = int.Parse(item.Name),
                        LocalizedNames = localizedNames
                    };
                    Items[definition.Name] = definition;
                }
            }
        }

        NamedWeapons = Items.Keys.OrderByDescending(i => i.Length).ToList();
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
                    if (_RemappedRarityColor.TryGetValue(definition.Name, out var remappedColor))
                    {
                        definition.HexColor = remappedColor;
                    }
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
            var content = Core.GameFileSystem.ReadFile(languagePath, "GAME")[1..];
            // var bytes = Encoding.UTF8.GetBytes(content);
            // content = Encoding.Latin1.GetString(bytes);
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

    public void ParsePaintkits()
    {
        Dictionary<string, RarityDefinition> paintkitRarities = new();
        foreach (var keys in Root.Children)
        {
            if (keys.Name == "paint_kits_rarity")
            {
                foreach (var rarity in keys.Children)
                {
                    paintkitRarities[rarity.Name] = Rarities[rarity.Value.EToString()];
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
                    var key = tag.Replace("#", "").Trim();
                    var localizedNames = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                    var notFoundLanguages = new List<string>();
                    foreach (var (languageName, tokens) in Languages)
                    {
                        if (tokens.TryGetValue(key, out var value))
                        {
                            localizedNames[languageName] = value;
                        }
                        else
                        {
                            notFoundLanguages.Add(languageName);
                        }
                    }

                    if (localizedNames.ContainsKey("english"))
                    {
                        foreach (var notfoundLanguage in notFoundLanguages)
                        {
                            localizedNames[notfoundLanguage] = localizedNames["english"];
                        }
                    }

                    var definition = new PaintkitDefinition
                    {
                        Index = int.Parse(paintkit.Name),
                        Name = paintkit.Value["name"].EToString(),
                        UseLegacyModel = paintkit.HasSubKeyWithValue("use_legacy_model", "1"),
                        DescriptionTag = tag,
                        LocalizedNames = localizedNames,
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


        var textureDirs = Core.GameFileSystem.FindFileAbsoluteList("panorama/images/econ/stickers/*", "GAME");
        foreach (var textureDir in textureDirs)
        {
            var collectionName = textureDir.Split('/').Last().Split('\\').Last();
            var textures =
                Core.GameFileSystem.FindFileAbsoluteList(
                    Path.Combine("panorama/images/econ/stickers/", collectionName, "*"), "GAME");
            Stickers[collectionName] = new();
            foreach (var texture in textures)
            {
                var fileName = texture.Split('/').Last().Split('\\').Last();
                var itemName = fileName.Replace("_png.vtex_c", "");

                if (itemName.EndsWith("_1355_37"))
                {
                    continue;
                }

                var materialName = $"{collectionName}/{itemName}";
                if (items.TryGetValue(materialName, out var item))
                {
                    var localizedNames = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                    var key = item.Value["item_name"].EToString().Replace("#", "");
                    foreach (var (languageName, tokens) in Languages)
                    {
                        if (tokens.TryGetValue(key, out var value))
                        {
                            localizedNames[languageName] = value;
                        }
                    }

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
                    Stickers[collectionName].Add(definition);
                }
                else
                {
                    Logger.LogDebug($"Sticker: {materialName} not found in items");
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

                    var localizedNames = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                    var key = keychain.Value["loc_name"].EToString().Replace("#", "");
                    foreach (var (languageName, tokens) in Languages)
                    {
                        if (tokens.TryGetValue(key, out var value))
                        {
                            localizedNames[languageName] = value;
                        }
                    }

                    var definition = new KeychainDefinition
                    {
                        Name = keychain.Name,
                        Index = int.Parse(keychain.Name),
                        LocalizedNames = localizedNames,
                        Rarity = rarity
                    };
                    Keychains[definition.Name] = definition;
                }
            }
        }
    }
}