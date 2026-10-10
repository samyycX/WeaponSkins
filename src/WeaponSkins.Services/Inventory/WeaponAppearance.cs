using SwiftlyS2.Shared.SchemaDefinitions;

using WeaponSkins.Shared;

namespace WeaponSkins.Services;

internal static class WeaponAppearance
{
    public static void Apply(CBasePlayerWeapon weapon,
        WeaponSkinData skin)
    {
        var item = weapon.AttributeManager.Item;
        var itemId = (ulong)Random.Shared.NextInt64(1, long.MaxValue);
        item.ItemID = itemId;
        item.ItemIDLow = (uint)itemId;
        item.ItemIDHigh = (uint)(itemId >> 32);
        item.AccountID = (uint)skin.SteamID;
        item.Initialized = true;
        item.ItemDefinitionIndex = skin.DefinitionIndex;
        item.EntityQuality = (int)skin.Quality;
        item.CustomName = skin.Nametag ?? string.Empty;
        weapon.OriginalOwnerXuidLow = (uint)skin.SteamID;
        weapon.OriginalOwnerXuidHigh = (uint)(skin.SteamID >> 32);
        weapon.FallbackPaintKit = skin.Paintkit;
        weapon.FallbackSeed = skin.PaintkitSeed;
        weapon.FallbackWear = skin.PaintkitWear;
        weapon.FallbackStatTrak = skin.Quality == EconItemQuality.StatTrak ? skin.StattrakCount : -1;

        RemoveCosmeticAttributes(item.AttributeList);
        RemoveCosmeticAttributes(item.NetworkedDynamicAttributes);
        WriteAttributes(skin, (name, value) =>
        {
            item.AttributeList.SetOrAddAttribute(name, value);
            item.NetworkedDynamicAttributes.SetOrAddAttribute(name, value);
        });

        item.ItemIDLowUpdated();
        item.ItemIDHighUpdated();
        item.AccountIDUpdated();
        item.InitializedUpdated();
        item.CustomNameUpdated();
        item.AttributeListUpdated();
        item.NetworkedDynamicAttributesUpdated();
        weapon.FallbackPaintKitUpdated();
        weapon.FallbackSeedUpdated();
        weapon.FallbackWearUpdated();
        weapon.FallbackStatTrakUpdated();
    }

    private static void RemoveCosmeticAttributes(CAttributeList list)
    {
        for (var index = list.Attributes.Count - 1; index >= 0; index--)
        {
            var definition = list.Attributes[index].AttributeDefinitionIndex;
            if (definition is
                >= (ushort)AttributeDefinitionIndex.STICKER_SLOT_0_ID and <= (ushort)AttributeDefinitionIndex.STICKER_SLOT_5_ROTATION or
                >= (ushort)AttributeDefinitionIndex.STICKER_SLOT_0_OFFSET_X and <= (ushort)AttributeDefinitionIndex.STICKER_SLOT_5_SCHEMA or
                >= (ushort)AttributeDefinitionIndex.KEYCHAIN_SLOT_0_ID and <= (ushort)AttributeDefinitionIndex.KEYCHAIN_SLOT_0_OFFSET_Z or
                (ushort)AttributeDefinitionIndex.KEYCHAIN_SLOT_0_SEED or
                (ushort)AttributeDefinitionIndex.KILL_EATER or (ushort)AttributeDefinitionIndex.KILL_EATER_SCORE_TYPE)
            {
                list.Attributes.Remove(index);
            }
        }
    }

    internal static void WriteAttributes(WeaponSkinData skin,
        Action<string, float> setAttribute)
    {
        setAttribute("set item texture prefab", skin.Paintkit);
        setAttribute("set item texture seed", skin.PaintkitSeed);
        setAttribute("set item texture wear", skin.PaintkitWear);
        if (skin.Quality == EconItemQuality.StatTrak)
        {
            setAttribute("kill eater", BitConverter.Int32BitsToSingle(skin.StattrakCount));
            setAttribute("kill eater score type", 0);
        }

        for (var index = 0; index < 6; index++)
        {
            var sticker = skin.GetSticker(index);
            if (sticker is not { Id: > 0 }) continue;
            var prefix = $"sticker slot {index}";
            setAttribute($"{prefix} id", BitConverter.Int32BitsToSingle(sticker.Id));
            // 1337 means no schema override; never send it as a model anchor.
            if (sticker.Schema != 1337)
            {
                setAttribute($"{prefix} schema", BitConverter.Int32BitsToSingle(sticker.Schema));
                setAttribute($"{prefix} offset x", sticker.OffsetX);
                setAttribute($"{prefix} offset y", sticker.OffsetY);
            }
            setAttribute($"{prefix} wear", sticker.Wear);
            setAttribute($"{prefix} scale", sticker.Scale);
            setAttribute($"{prefix} rotation", sticker.Rotation);
        }

        if (skin.Keychain0 is { Id: > 0 } keychain)
        {
            setAttribute("keychain slot 0 id", BitConverter.Int32BitsToSingle(keychain.Id));
            setAttribute("keychain slot 0 offset x", keychain.OffsetX);
            setAttribute("keychain slot 0 offset y", keychain.OffsetY);
            setAttribute("keychain slot 0 offset z", keychain.OffsetZ);
            setAttribute("keychain slot 0 seed", BitConverter.Int32BitsToSingle(keychain.Seed));
        }
    }
}