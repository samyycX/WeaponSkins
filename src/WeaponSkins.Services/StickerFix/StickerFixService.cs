using SwiftlyS2.Shared;

using WeaponSkins.Shared;

namespace WeaponSkins;

public static class StickerFixService
{
    private static Dictionary<ulong /* steamid */, Dictionary<int /* key hash */, int /* sticker hash */>> _stickerHashes = new();

    [SwiftlyInject]
    private static ISwiftlyCore Core { get; set; } = null!;

    public static void Initialize()
    {
        foreach(var player in Core.PlayerManager.GetAllPlayers())
        {
            _stickerHashes[player.SteamID] = new();
        }

        Core.Event.OnClientSteamAuthorize += (@event) =>
        {
            var player = Core.PlayerManager.GetPlayer(@event.PlayerId);
            _stickerHashes[player.SteamID] = new();
        };

        Core.Event.OnClientDisconnected += (@event) =>
        {
            var player = Core.PlayerManager.GetPlayer(@event.PlayerId);
            _stickerHashes[player.SteamID] = new();
        };
    }

    public static void FixSticker(WeaponSkinData skin)
    {
        var newStickerHash = CalculateStickerHash(skin);
        if (_stickerHashes.TryGetValue(skin.SteamID, out var hashes))
        {
            while (true)
            {
                if (hashes.TryGetValue(CalculateKeyHash(skin), out var stickerHash))
                {
                    if (stickerHash != newStickerHash)
                    {
                        skin.PaintkitWear += 0.001f;
                        continue;
                    }

                    return;
                }
                else
                {
                    hashes[CalculateKeyHash(skin)] = newStickerHash;
                    return;
                }
            }
        }
    }

    private static int CalculateKeyHash(WeaponSkinData skin)
    {
        var hash = new HashCode();

        hash.Add(skin.DefinitionIndex);
        hash.Add(skin.Paintkit);
        hash.Add(skin.PaintkitWear);
        hash.Add(skin.PaintkitSeed);

        return hash.ToHashCode();
    }

    private static int CalculateStickerHash(WeaponSkinData skin)
    {
        var hash = new HashCode();

        hash.Add(skin.Sticker0?.GetHashCode());
        hash.Add(skin.Sticker1?.GetHashCode());
        hash.Add(skin.Sticker2?.GetHashCode());
        hash.Add(skin.Sticker3?.GetHashCode());
        hash.Add(skin.Sticker4?.GetHashCode());
        hash.Add(skin.Sticker5?.GetHashCode());

        return hash.ToHashCode();
    }
}