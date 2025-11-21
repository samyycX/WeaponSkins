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

    public ulong SteamID => m_id;
}