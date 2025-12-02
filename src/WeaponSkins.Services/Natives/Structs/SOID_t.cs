namespace WeaponSkins;

public struct SOID_t
{
    private ulong m_id;
    private uint m_type;
    private uint m_padding;

    public SOID_t(ulong steamid)
    {
        m_id = steamid;
        m_type = 1;
        m_padding = 0;
    }

    public SOID_t(ulong soid1, ulong soid2)
    {
        m_id = soid1;
        m_type = (uint)soid2;
        m_padding = 0;
    }

    public ulong SteamID => m_id;

    public ulong Part1 => m_id;
    public ulong Part2 => m_type;
}