using System.Collections.Concurrent;

namespace WeaponSkins.Services;

public class PinDataService
{
    private readonly ConcurrentDictionary<ulong, int> _pins = new();

    public void SetPin(ulong steamId, int pinIndex)
    {
        _pins[steamId] = pinIndex;
    }

    public bool TryGetPin(ulong steamId, out int pinIndex)
    {
        return _pins.TryGetValue(steamId, out pinIndex);
    }

    public bool RemovePin(ulong steamId)
    {
        return _pins.TryRemove(steamId, out _);
    }
}
