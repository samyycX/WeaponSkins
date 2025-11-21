using System.Runtime.InteropServices;

using WeaponSkins.Shared;

namespace WeaponSkins;

[StructLayout(LayoutKind.Explicit, Size = 16)]
public struct Attribute
{
    [FieldOffset(0)] public AttributeDefinitionIndex AttributeDefinitionIndex;

    [FieldOffset(8)] public float FloatData;

    [FieldOffset(8)] public int IntData;
}