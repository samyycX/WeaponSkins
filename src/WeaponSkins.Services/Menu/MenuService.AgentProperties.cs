using SwiftlyS2.Shared.Menu;
using SwiftlyS2.Shared.Menu.Components;
using SwiftlyS2.Shared.Players;

using WeaponSkins.Services;
using WeaponSkins.Shared;

namespace WeaponSkins;

public partial class MenuService
{
    // Patches for the agent the player wears on their current team, whichever agent that is.
    private IMenu BuildAgentPropertiesMenu(IPlayer player)
    {
        var loc = LocalizationService[player];
        var language = player.PlayerLanguage.Value;
        var team = player.Controller.Team;
        var main = CreateMenu("weaponskins.agentproperties")
            .WithTitle(loc.MenuTitleAgentProperties);
        IMenu slots = null!;

        main.Add(CreateButton(loc.MenuReset, p => Api.ResetAgentPatches(p.SteamID, team)));

        for (var i = 0; i < AgentPatchDataService.Slots; i++)
        {
            var slot = i;
            PatchDefinition? Applied() => Api.TryGetAgentPatches(player.SteamID, team, out var patches)
                ? FindPatch(patches[slot])
                : null;
            // Read on every draw: the page this opens changes the patch it names.
            string Title() => loc.MenuAgentPropertiesSetPatch(slot + 1,
                (Applied() is { } patch ? EconService.GetLocalizedName(patch.LocalizedNames, language) : null)
                ?? loc.MenuSkinPropertiesNametagNone);

            // A row's Tag is the item it stands for, so any renderer can identify it.
            main.Add(new SubmenuComponent(Title(), _ => BuildPatchSlotMenu(player, team, slot, Title(), slots))
            {
                TextProvider = _ => Title(),
                Tag = slot
            });
        }

        slots = main.Build();
        TrackFocus(slots);
        return slots;
    }

    private IMenu BuildPatchSlotMenu(IPlayer player,
        Team team,
        int slot,
        string title,
        IMenu slots)
    {
        var loc = LocalizationService[player];
        var language = player.PlayerLanguage.Value;
        var main = CreateMenu($"weaponskins.patches.slot{slot}")
            .WithTitle(title);

        // Picking or clearing the patch finishes the slot, as it does for stickers.
        void Apply(int patchIndex)
        {
            Api.SetAgentPatch(player.SteamID, team, slot, patchIndex);
            _ = slots.Open(player);
        }

        main.Add(CreateButton(loc.MenuReset, _ => Apply(0)));

        foreach (var collection in EconService.PatchCollections.Values)
        {
            var collectionName = EconService.GetLocalizedName(collection.LocalizedNames, language);
            var pack = CreateSubmenu(collectionName, _ =>
            {
                var patchMenu = CreateMenu($"weaponskins.patches.slot{slot}.collection")
                    .WithTitle(collectionName);
                foreach (var patch in collection.Patches)
                {
                    patchMenu.Add(CreatePatchButton(patch, language, Apply));
                }

                return patchMenu.Build();
            });
            pack.Tag = collection;
            main.Add(pack);
        }

        var packed = EconService.PatchCollections.Values
            .SelectMany(collection => collection.Patches)
            .Select(patch => patch.Index)
            .ToHashSet();
        foreach (var patch in EconService.Patches.Values.Where(patch => !packed.Contains(patch.Index)))
        {
            main.Add(CreatePatchButton(patch, language, Apply));
        }

        return main.Build();
    }

    private ButtonComponent CreatePatchButton(PatchDefinition patch, string language, Action<int> apply)
    {
        var index = patch.Index;
        var button = CreateRarityButton(EconService.GetLocalizedName(patch.LocalizedNames, language),
            patch.Rarity.Color.HexColor, _ => apply(index));
        button.Tag = patch;
        return button;
    }

    private PatchDefinition? FindPatch(int index)
    {
        return index == 0 ? null : EconService.Patches.Values.FirstOrDefault(patch => patch.Index == index);
    }
}
