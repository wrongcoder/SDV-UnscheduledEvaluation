using System;
using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;

namespace UnscheduledEvaluation;

public class ModEntry : Mod
{
    private ModConfig? _config;

    public override void Entry(IModHelper helper)
    {
        _config = Helper.ReadConfig<ModConfig>();

        var harmony = new Harmony(ModManifest.UniqueID);
        if (_config.EnableAlwaysActiveShrinePatch)
        {
            harmony.Patch(
                original: AccessTools.Method(typeof(Farm), nameof(Farm.checkAction)),
                prefix: new HarmonyMethod(typeof(AlwaysActiveShrinePatch), nameof(AlwaysActiveShrinePatch.Prefix)),
                postfix: new HarmonyMethod(typeof(AlwaysActiveShrinePatch), nameof(AlwaysActiveShrinePatch.Postfix))
            );
        }

        helper.Events.Content.AssetRequested += OnAssetRequested;
    }

    private void OnAssetRequested(object? sender, AssetRequestedEventArgs e)
    {
        if (e.NameWithoutLocale.IsEquivalentTo("Strings/Locations")) PatchStringsLocations(e);
        if (e.NameWithoutLocale.IsEquivalentTo("Data/Events/Farm")) PatchDataEventsFarm(e);
        if (e.NameWithoutLocale.IsEquivalentTo("Data/Events/FarmHouse")) PatchDataEventsFarmHouse(e);
    }

    private void PatchStringsLocations(AssetRequestedEventArgs e)
    {
        e.Edit(asset =>
        {
            var data = asset.AsDictionary<string, string>().Data;
            var grandpaNote = _config?.GrandpaNote;
            if (grandpaNote != null)
            {
                data["Farm_GrandpaNote"] = grandpaNote;
            }
        });
    }

    private void PatchDataEventsFarm(AssetRequestedEventArgs e)
    {
        e.Edit(asset =>
        {
            var data = asset.AsDictionary<string, string>().Data;
            // Update Grandpa candles on shrine: 2146991
            // This event is marked unseen by Event.DefaultCommands.GrandpaEvaluation2
            RenameKey(data, "2146991/y 3/H", "2146991/e 558291/H");
        });
    }

    private void PatchDataEventsFarmHouse(AssetRequestedEventArgs e)
    {
        e.Edit(asset =>
        {
            var data = asset.AsDictionary<string, string>().Data;
            // First Grandpa evaluation: 558291
            // Refers to two years spent on the farm
            RenameKey(data, "558291/y 3/H", "558291/e 321777/t 600 620/H");
            // Subsequent Grandpa evaluations: 558292
            RenameKey(data, "558292/e 321777/t 600 620/H", "558292/e 321777/t 600 620/e 558291/H");
            // Diamond placed on shrine marks 321777 as seen
        });
    }

    private void RenameKey<TKey, TValue>(IDictionary<TKey, TValue> data, TKey oldKey, TKey newKey)
    {
        var oldKeyMissing = !data.ContainsKey(oldKey);
        var newKeyExists = data.ContainsKey(newKey);

        if (oldKeyMissing || newKeyExists)
        {
            var m = $"Skipping rename \"{oldKey}\" (missing={oldKeyMissing}) to \"{newKey}\" (exists={newKeyExists})";
            Monitor.Log(m, LogLevel.Error);
            return;
        }

        data[newKey] = data[oldKey];
        data.Remove(oldKey);
    }
}