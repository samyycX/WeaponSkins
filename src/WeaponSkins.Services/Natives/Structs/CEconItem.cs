using System.Runtime.InteropServices;

using SwiftlyS2.Shared.Natives;
using SwiftlyS2.Shared.SchemaDefinitions;

using WeaponSkins.Shared;

namespace WeaponSkins;

[StructLayout(LayoutKind.Explicit, Size = 72)]
public ref struct CEconItemStruct
{
    [FieldOffset(0x10)] public ulong ItemID;

    [FieldOffset(0x20)] public nint pCustomData;

    [FieldOffset(0x28)] public uint AccountID;

    [FieldOffset(0x2C)] public uint InventoryPosition; // Backpack slot

    [FieldOffset(0x30)] public ushort DefinitionIndex;

    [FieldOffset(0x32)] private ushort _packedBits;

    private const int ORIGIN_SHIFT = 0;
    private const ushort ORIGIN_MASK = (ushort)((1 << 5) - 1); // 0b1_1111

    private const int QUALITY_SHIFT = 5;
    private const ushort QUALITY_MASK = (ushort)((1 << 4) - 1); // 0b1111

    private const int LEVEL_SHIFT = 9;
    private const ushort LEVEL_MASK = (ushort)((1 << 2) - 1); // 0b11

    private const int RARITY_SHIFT = 11;
    private const ushort RARITY_MASK = (ushort)((1 << 4) - 1); // 0b1111

    private const int INUSE_SHIFT = 15;
    private const ushort INUSE_MASK = 0x0001; // 1 bit

    public ushort Origin
    {
        get => (ushort)((_packedBits >> ORIGIN_SHIFT) & ORIGIN_MASK);
        set
        {
            if (value > ORIGIN_MASK) throw new ArgumentOutOfRangeException(nameof(value), "Origin out of 5 bit range.");
            _packedBits = (ushort)((_packedBits & ~(ORIGIN_MASK << ORIGIN_SHIFT)) |
                                   ((value & ORIGIN_MASK) << ORIGIN_SHIFT));
        }
    }

    public ushort Quality
    {
        get => (ushort)((_packedBits >> QUALITY_SHIFT) & QUALITY_MASK);
        set
        {
            if (value > QUALITY_MASK)
                throw new ArgumentOutOfRangeException(nameof(value), "Quality out of 4bit range.");
            _packedBits = (ushort)((_packedBits & ~(QUALITY_MASK << QUALITY_SHIFT)) |
                                   ((value & QUALITY_MASK) << QUALITY_SHIFT));
        }
    }

    public ushort Level
    {
        get => (ushort)((_packedBits >> LEVEL_SHIFT) & LEVEL_MASK);
        set
        {
            if (value > LEVEL_MASK) throw new ArgumentOutOfRangeException(nameof(value), "Level out of 2 bit range.");
            _packedBits =
                (ushort)((_packedBits & ~(LEVEL_MASK << LEVEL_SHIFT)) | ((value & LEVEL_MASK) << LEVEL_SHIFT));
        }
    }

    public ushort Rarity
    {
        get => (ushort)((_packedBits >> RARITY_SHIFT) & RARITY_MASK);
        set
        {
            if (value > RARITY_MASK) throw new ArgumentOutOfRangeException(nameof(value), "Rarity out of 4 bit range.");
            _packedBits = (ushort)((_packedBits & ~(RARITY_MASK << RARITY_SHIFT)) |
                                   ((value & RARITY_MASK) << RARITY_SHIFT));
        }
    }

    public bool InUse
    {
        get => ((_packedBits >> INUSE_SHIFT) & INUSE_MASK) != 0;
        set
        {
            if (value)
                _packedBits = (ushort)(_packedBits | (INUSE_MASK << INUSE_SHIFT));
            else
                _packedBits = (ushort)(_packedBits & ~(INUSE_MASK << INUSE_SHIFT));
        }
    }
}

public class CEconItem : INativeHandle
{
    public nint Address { get; set; }
    public bool IsValid => Address != 0;

    public CEconItem(nint address)
    {
        Address = address;
    }

    private ref CEconItemStruct Struct => ref Address.AsRef<CEconItemStruct>();

    public ulong ItemID { get => Struct.ItemID; set => Struct.ItemID = value; }
    public nint pCustomData { get => Struct.pCustomData; set => Struct.pCustomData = value; }
    public uint AccountID { get => Struct.AccountID; set => Struct.AccountID = value; }
    public uint InventoryPosition { get => Struct.InventoryPosition; set => Struct.InventoryPosition = value; }
    public ushort DefinitionIndex { get => Struct.DefinitionIndex; set => Struct.DefinitionIndex = value; }
    public ushort Origin { get => Struct.Origin; set => Struct.Origin = value; }
    public EconItemQuality Quality { get => (EconItemQuality)Struct.Quality; set => Struct.Quality = (byte)value; }
    public ushort Level { get => Struct.Level; set => Struct.Level = value; }
    public EconItemRarity Rarity { get => (EconItemRarity)Struct.Rarity; set => Struct.Rarity = (byte)value; }
    public bool InUse { get => Struct.InUse; set => Struct.InUse = value; }


    public void ConfigureAttributes(Action<CustomAttributeData> configure)
    {
        if (pCustomData == 0)
        {
            pCustomData = CustomAttributeData.Create().Address;
        }

        var customData = new CustomAttributeData(pCustomData);
        configure(customData);
        pCustomData = customData.Address; // handle realloc
    }

    public void Apply(WeaponSkinData data)
    {
        DefinitionIndex = data.DefinitionIndex;
        Quality = data.Quality;
        ConfigureAttributes(customData =>
        {
            customData.SetPaintkit(data.Paintkit);
            customData.SetPaintkitSeed(data.PaintkitSeed);
            customData.SetPaintkitWear(data.PaintkitWear);

            if (data.Quality == EconItemQuality.StatTrak)
            {
                customData.SetStattrak(data.StattrakCount);
            }

            if (data.Nametag != null)
            {
                customData.SetCustomName(data.Nametag);
            }

            if (data.Sticker0 != null)
            {
                customData.SetSticker0(data.Sticker0.Id, data.Sticker0.Wear, data.Sticker0.Scale,
                    data.Sticker0.Rotation, data.Sticker0.OffsetX, data.Sticker0.OffsetY, data.Sticker0.Schema);
            }

            if (data.Sticker1 != null)
            {
                customData.SetSticker1(data.Sticker1.Id, data.Sticker1.Wear, data.Sticker1.Scale,
                    data.Sticker1.Rotation, data.Sticker1.OffsetX, data.Sticker1.OffsetY, data.Sticker1.Schema);
            }

            if (data.Sticker2 != null)
            {
                customData.SetSticker2(data.Sticker2.Id, data.Sticker2.Wear, data.Sticker2.Scale,
                    data.Sticker2.Rotation, data.Sticker2.OffsetX, data.Sticker2.OffsetY, data.Sticker2.Schema);
            }

            if (data.Sticker3 != null)
            {
                customData.SetSticker3(data.Sticker3.Id, data.Sticker3.Wear, data.Sticker3.Scale,
                    data.Sticker3.Rotation, data.Sticker3.OffsetX, data.Sticker3.OffsetY, data.Sticker3.Schema);
            }

            if (data.Sticker4 != null)
            {
                customData.SetSticker4(data.Sticker4.Id, data.Sticker4.Wear, data.Sticker4.Scale,
                    data.Sticker4.Rotation, data.Sticker4.OffsetX, data.Sticker4.OffsetY, data.Sticker4.Schema);
            }

            if (data.Sticker5 != null)
            {
                customData.SetSticker5(data.Sticker5.Id, data.Sticker5.Wear, data.Sticker5.Scale,
                    data.Sticker5.Rotation, data.Sticker5.OffsetX, data.Sticker5.OffsetY, data.Sticker5.Schema);
            }

            if (data.Keychain0 != null)
            {
                customData.SetKeychain0(data.Keychain0.Id, data.Keychain0.OffsetX, data.Keychain0.OffsetY,
                    data.Keychain0.OffsetZ, data.Keychain0.Seed);
            }
        });
    }

    public void Apply(KnifeSkinData data)
    {
        DefinitionIndex = data.DefinitionIndex;
        Quality = data.Quality;
        ConfigureAttributes(customData =>
        {
            customData.SetStattrak(data.StattrakCount);

            if (data.Nametag != null)
            {
                customData.SetCustomName(data.Nametag);
            }
            customData.SetPaintkit(data.Paintkit);
            customData.SetPaintkitSeed(data.PaintkitSeed);
            customData.SetPaintkitWear(data.PaintkitWear);
        });
    }

    public void Apply(GloveData data)
    {
        DefinitionIndex = data.DefinitionIndex;
        ConfigureAttributes(customData =>
        {
            customData.SetPaintkit(data.Paintkit);
            customData.SetPaintkitSeed(data.PaintkitSeed);
            customData.SetPaintkitWear(data.PaintkitWear);
        });
    }
}