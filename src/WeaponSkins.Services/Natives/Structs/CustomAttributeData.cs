using System.Runtime.CompilerServices;

using SwiftlyS2.Shared;
using SwiftlyS2.Shared.Natives;

using WeaponSkins.Shared;

namespace WeaponSkins;

public class CustomAttributeData : INativeHandle
{
    [SwiftlyInject] private static ISwiftlyCore Core { get; set; } = null!;

    public nint Address { get; set; }
    public bool IsValid => Address != 0;

    public CustomAttributeData(nint address)
    {
        Address = address;
    }

    private ref byte _flags => ref Address.AsRef<byte>(0);
    public ref byte Count => ref Address.AsRef<byte>(2);

    public static CustomAttributeData Create()
    {
        var addr = Core.Memory.Alloc(4);
        var data = new CustomAttributeData(addr);
        data._flags = 0x3F;
        data.Count = 0;
        return data;
    }

    public void AddAttribute(Attribute attribute)
    {
        Address = Core.Memory.Resize(Address, (ulong)(4 + (Count + 1) * Unsafe.SizeOf<Attribute>()));
        this[Count] = attribute;
        Count++;
    }

    public void UpdateAttribute(Attribute attribute)
    {
        for (int i = 0; i < Count; i++)
        {
            if (this[i].AttributeDefinitionIndex == attribute.AttributeDefinitionIndex)
            {
                this[i] = attribute;
                return;
            }
        }

        AddAttribute(attribute);
    }

    public ref Attribute this[int index] => ref Address.AsRef<Attribute>(4 + index * Unsafe.SizeOf<Attribute>());

    public void SetPaintkit(int paintkit)
    {
        UpdateAttribute(new Attribute
        {
            AttributeDefinitionIndex = AttributeDefinitionIndex.SET_ITEM_TEXTURE_PREFAB,
            FloatData = Convert.ToSingle(paintkit)
        });
    }

    public void SetPaintkitSeed(int seed)
    {
        UpdateAttribute(new Attribute
        {
            AttributeDefinitionIndex = AttributeDefinitionIndex.SET_ITEM_TEXTURE_SEED, IntData = seed
        });
    }

    public void SetPaintkitWear(float wear)
    {
        UpdateAttribute(new Attribute
        {
            AttributeDefinitionIndex = AttributeDefinitionIndex.SET_ITEM_TEXTURE_WEAR, FloatData = wear
        });
    }

    public void SetKillEater(int scoreType)
    {
        UpdateAttribute(new Attribute
        {
            AttributeDefinitionIndex = AttributeDefinitionIndex.KILL_EATER, IntData = scoreType
        });
    }

    public void SetKillEaterScoreType(int scoreType)
    {
        UpdateAttribute(new Attribute
        {
            AttributeDefinitionIndex = AttributeDefinitionIndex.KILL_EATER_SCORE_TYPE, IntData = scoreType
        });
    }

    public void SetStattrak(int count)
    {
        SetKillEater(count);
        SetKillEaterScoreType(0);
    }

    public void SetCustomName(string name)
    {
        var str = StaticNativeService.Service.CreateAttributeString();
        str.Value = name;
        UpdateAttribute(new Attribute
        {
            AttributeDefinitionIndex = AttributeDefinitionIndex.CUSTOM_NAME_ATTR, PtrData = str.Address 
        });
    }

    public void SetSticker0(int id,
        float wear,
        float scale,
        float rotation,
        float offsetX,
        float offsetY,
        int schema)
    {
        UpdateAttribute(new Attribute
        {
            AttributeDefinitionIndex = AttributeDefinitionIndex.STICKER_SLOT_0_ID, IntData = id
        });
        UpdateAttribute(new Attribute
        {
            AttributeDefinitionIndex = AttributeDefinitionIndex.STICKER_SLOT_0_WEAR, FloatData = wear
        });
        UpdateAttribute(new Attribute
        {
            AttributeDefinitionIndex = AttributeDefinitionIndex.STICKER_SLOT_0_SCALE, FloatData = scale
        });
        UpdateAttribute(new Attribute
        {
            AttributeDefinitionIndex = AttributeDefinitionIndex.STICKER_SLOT_0_ROTATION, FloatData = rotation
        });
        if (schema == 1337) return;
        UpdateAttribute(new Attribute
        {
            AttributeDefinitionIndex = AttributeDefinitionIndex.STICKER_SLOT_0_OFFSET_X, FloatData = offsetX
        });
        UpdateAttribute(new Attribute
        {
            AttributeDefinitionIndex = AttributeDefinitionIndex.STICKER_SLOT_0_OFFSET_Y, FloatData = offsetY
        });
        UpdateAttribute(new Attribute
        {
            AttributeDefinitionIndex = AttributeDefinitionIndex.STICKER_SLOT_0_SCHEMA, IntData = schema
        });
    }

    public void SetSticker1(int id,
        float wear,
        float scale,
        float rotation,
        float offsetX,
        float offsetY,
        int schema)
    {
        UpdateAttribute(new Attribute
        {
            AttributeDefinitionIndex = AttributeDefinitionIndex.STICKER_SLOT_1_ID, IntData = id
        });
        UpdateAttribute(new Attribute
        {
            AttributeDefinitionIndex = AttributeDefinitionIndex.STICKER_SLOT_1_WEAR, FloatData = wear
        });
        UpdateAttribute(new Attribute
        {
            AttributeDefinitionIndex = AttributeDefinitionIndex.STICKER_SLOT_1_SCALE, FloatData = scale
        });
        UpdateAttribute(new Attribute
        {
            AttributeDefinitionIndex = AttributeDefinitionIndex.STICKER_SLOT_1_ROTATION, FloatData = rotation
        });
        if (schema == 1337) return;
        UpdateAttribute(new Attribute
        {
            AttributeDefinitionIndex = AttributeDefinitionIndex.STICKER_SLOT_1_OFFSET_X, FloatData = offsetX
        });
        UpdateAttribute(new Attribute
        {
            AttributeDefinitionIndex = AttributeDefinitionIndex.STICKER_SLOT_1_OFFSET_Y, FloatData = offsetY
        });
        UpdateAttribute(new Attribute
        {
            AttributeDefinitionIndex = AttributeDefinitionIndex.STICKER_SLOT_1_SCHEMA, IntData = schema
        });
    }

    public void SetSticker2(int id,
        float wear,
        float scale,
        float rotation,
        float offsetX,
        float offsetY,
        int schema)
    {
        UpdateAttribute(new Attribute
        {
            AttributeDefinitionIndex = AttributeDefinitionIndex.STICKER_SLOT_2_ID, IntData = id
        });
        UpdateAttribute(new Attribute
        {
            AttributeDefinitionIndex = AttributeDefinitionIndex.STICKER_SLOT_2_WEAR, FloatData = wear
        });
        UpdateAttribute(new Attribute
        {
            AttributeDefinitionIndex = AttributeDefinitionIndex.STICKER_SLOT_2_SCALE, FloatData = scale
        });
        UpdateAttribute(new Attribute
        {
            AttributeDefinitionIndex = AttributeDefinitionIndex.STICKER_SLOT_2_ROTATION, FloatData = rotation
        });
        if (schema == 1337) return;
        UpdateAttribute(new Attribute
        {
            AttributeDefinitionIndex = AttributeDefinitionIndex.STICKER_SLOT_2_OFFSET_X, FloatData = offsetX
        });
        UpdateAttribute(new Attribute
        {
            AttributeDefinitionIndex = AttributeDefinitionIndex.STICKER_SLOT_2_OFFSET_Y, FloatData = offsetY
        });
        UpdateAttribute(new Attribute
        {
            AttributeDefinitionIndex = AttributeDefinitionIndex.STICKER_SLOT_2_SCHEMA, IntData = schema
        });
    }

    public void SetSticker3(int id,
        float wear,
        float scale,
        float rotation,
        float offsetX,
        float offsetY,
        int schema)
    {
        UpdateAttribute(new Attribute
        {
            AttributeDefinitionIndex = AttributeDefinitionIndex.STICKER_SLOT_3_ID, IntData = id
        });
        UpdateAttribute(new Attribute
        {
            AttributeDefinitionIndex = AttributeDefinitionIndex.STICKER_SLOT_3_WEAR, FloatData = wear
        });
        UpdateAttribute(new Attribute
        {
            AttributeDefinitionIndex = AttributeDefinitionIndex.STICKER_SLOT_3_SCALE, FloatData = scale
        });
        UpdateAttribute(new Attribute
        {
            AttributeDefinitionIndex = AttributeDefinitionIndex.STICKER_SLOT_3_ROTATION, FloatData = rotation
        });
        if (schema == 1337) return;
        UpdateAttribute(new Attribute
        {
            AttributeDefinitionIndex = AttributeDefinitionIndex.STICKER_SLOT_3_OFFSET_X, FloatData = offsetX
        });
        UpdateAttribute(new Attribute
        {
            AttributeDefinitionIndex = AttributeDefinitionIndex.STICKER_SLOT_3_OFFSET_Y, FloatData = offsetY
        });
        UpdateAttribute(new Attribute
        {
            AttributeDefinitionIndex = AttributeDefinitionIndex.STICKER_SLOT_3_SCHEMA, IntData = schema
        });
    }

    public void SetSticker4(int id,
        float wear,
        float scale,
        float rotation,
        float offsetX,
        float offsetY,
        int schema)
    {
        UpdateAttribute(new Attribute
        {
            AttributeDefinitionIndex = AttributeDefinitionIndex.STICKER_SLOT_4_ID, IntData = id
        });
        UpdateAttribute(new Attribute
        {
            AttributeDefinitionIndex = AttributeDefinitionIndex.STICKER_SLOT_4_WEAR, FloatData = wear
        });
        UpdateAttribute(new Attribute
        {
            AttributeDefinitionIndex = AttributeDefinitionIndex.STICKER_SLOT_4_SCALE, FloatData = scale
        });
        UpdateAttribute(new Attribute
        {
            AttributeDefinitionIndex = AttributeDefinitionIndex.STICKER_SLOT_4_ROTATION, FloatData = rotation
        });
        if (schema == 1337) return;
        UpdateAttribute(new Attribute
        {
            AttributeDefinitionIndex = AttributeDefinitionIndex.STICKER_SLOT_4_OFFSET_X, FloatData = offsetX
        });
        UpdateAttribute(new Attribute
        {
            AttributeDefinitionIndex = AttributeDefinitionIndex.STICKER_SLOT_4_OFFSET_Y, FloatData = offsetY
        });
        UpdateAttribute(new Attribute
        {
            AttributeDefinitionIndex = AttributeDefinitionIndex.STICKER_SLOT_4_SCHEMA, IntData = schema
        });
    }

    public void SetSticker5(int id,
        float wear,
        float scale,
        float rotation,
        float offsetX,
        float offsetY,
        int schema)
    {
        UpdateAttribute(new Attribute
        {
            AttributeDefinitionIndex = AttributeDefinitionIndex.STICKER_SLOT_5_ID, IntData = id
        });
        UpdateAttribute(new Attribute
        {
            AttributeDefinitionIndex = AttributeDefinitionIndex.STICKER_SLOT_5_WEAR, FloatData = wear
        });
        UpdateAttribute(new Attribute
        {
            AttributeDefinitionIndex = AttributeDefinitionIndex.STICKER_SLOT_5_SCALE, FloatData = scale
        });
        UpdateAttribute(new Attribute
        {
            AttributeDefinitionIndex = AttributeDefinitionIndex.STICKER_SLOT_5_ROTATION, FloatData = rotation
        });
        if (schema == 1337) return;
        UpdateAttribute(new Attribute
        {
            AttributeDefinitionIndex = AttributeDefinitionIndex.STICKER_SLOT_5_OFFSET_X, FloatData = offsetX
        });
        UpdateAttribute(new Attribute
        {
            AttributeDefinitionIndex = AttributeDefinitionIndex.STICKER_SLOT_5_OFFSET_Y, FloatData = offsetY
        });
        UpdateAttribute(new Attribute
        {
            AttributeDefinitionIndex = AttributeDefinitionIndex.STICKER_SLOT_5_SCHEMA, IntData = schema
        });
    }

    public void SetKeychain0(int id,
        float offsetX,
        float offsetY,
        float offsetZ,
        int seed)
    {
        UpdateAttribute(new Attribute
        {
            AttributeDefinitionIndex = AttributeDefinitionIndex.KEYCHAIN_SLOT_0_ID, IntData = id
        });
        UpdateAttribute(new Attribute
        {
            AttributeDefinitionIndex = AttributeDefinitionIndex.KEYCHAIN_SLOT_0_OFFSET_X, FloatData = offsetX
        });
        UpdateAttribute(new Attribute
        {
            AttributeDefinitionIndex = AttributeDefinitionIndex.KEYCHAIN_SLOT_0_OFFSET_Y, FloatData = offsetY
        });
        UpdateAttribute(new Attribute
        {
            AttributeDefinitionIndex = AttributeDefinitionIndex.KEYCHAIN_SLOT_0_OFFSET_Z, FloatData = offsetZ
        });
        UpdateAttribute(new Attribute
        {
            AttributeDefinitionIndex = AttributeDefinitionIndex.KEYCHAIN_SLOT_0_SEED, IntData = seed
        });
    }

    public void SetMusicId(int musicId)
    {
        UpdateAttribute(new Attribute
        {
            AttributeDefinitionIndex = AttributeDefinitionIndex.MUSIC_ID, IntData = musicId
        });
    }
}