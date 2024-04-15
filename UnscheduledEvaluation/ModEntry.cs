using System;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;

namespace UnscheduledEvaluation;

public class ModEntry : Mod
{
    public override void Entry(IModHelper helper)
    {
        helper.Events.Content.AssetRequested += OnAssetRequested;
    }

    private void OnAssetRequested(object? sender, AssetRequestedEventArgs e)
    {
        if (e.NameWithoutLocale.IsEquivalentTo("Strings/Locations")) PatchStringsLocations(e);
        if (e.NameWithoutLocale.IsEquivalentTo("Data/Events/Farm")) PatchDataEventsFarm(e);
    }

    private static void PatchStringsLocations(AssetRequestedEventArgs e)
    {
        e.Edit(asset =>
        {
            var data = asset.AsDictionary<string, string?>().Data;
            data["Farm_GrandpaNote"] = "Edited in DLL\n\n" + data["Farm_GrandpaNote"];
        });
    }

    private static void PatchDataEventsFarm(AssetRequestedEventArgs e)
    {
        e.Edit(asset =>
        {
            var data = asset.AsDictionary<string, string?>().Data;
            const string showCandleUpdate = "2146991/y 3/H";
            if (data[showCandleUpdate] != null)
            {
                // This event is marked unseen by Event.DefaultCommands.GrandpaEvaluation2
                data["2146991/e 558291/H"] = data[showCandleUpdate];
                data[showCandleUpdate] = null;
            }
        });
    }
}