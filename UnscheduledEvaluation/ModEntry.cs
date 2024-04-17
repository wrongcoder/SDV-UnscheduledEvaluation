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
    private const string Evaluation1Mail = "UnscheduledEvaluation-SawEvaluation1";

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
            const string origShrineCandlesKey = "2146991/y 3/H";
            const string newShrineCandlesKey = "2146991/e 558291/H";
            if (_config?.SkippableShrineCandles ?? false) PrependEventCommand(data, origShrineCandlesKey, "skippable");
            RenameKey(data, origShrineCandlesKey, newShrineCandlesKey);
        });
    }

    private void PatchDataEventsFarmHouse(AssetRequestedEventArgs e)
    {
        e.Edit(asset =>
        {
            var data = asset.AsDictionary<string, string>().Data;
            // First Grandpa evaluation: 558291
            // Refers to two years spent on the farm
            const string origEvaluation1Key = "558291/y 3/H";
            const string newEvaluation1Key = "558291/e 321777/t 600 620/H";
            if (_config?.SkippableEvaluation1 ?? false) PrependEventCommand(data, origEvaluation1Key, "skippable");
            PrependEventCommand(data, origEvaluation1Key, $"mail {Evaluation1Mail}");
            RenameKey(data, origEvaluation1Key, newEvaluation1Key);
            // Subsequent Grandpa evaluations: 558292
            const string origEvaluation2Key = "558292/e 321777/t 600 620/H";
            const string newEvaluation2Key = $"558292/e 321777/t 600 620/Hn {Evaluation1Mail}/H";
            if (_config?.SkippableEvaluation2 ?? false) PrependEventCommand(data, origEvaluation2Key, "skippable");
            RenameKey(data, origEvaluation2Key, newEvaluation2Key);
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

    private void PrependEventCommand(IDictionary<string, string> data, string eventKey, string eventCommand)
    {
        if (!data.TryGetValue(eventKey, out var eventScript))
        {
            Monitor.Log($"Skipping prepend to missing key \"{eventKey}\"", LogLevel.Error);
            return;
        }

        var parts = eventScript.Split('/', 4);
        var insertIndex = $"{parts[0]}/{parts[1]}/{parts[2]}/".Length;
        var prependedEventScript = eventScript.Insert(insertIndex, $"{eventCommand}/");
        data[eventKey] = prependedEventScript;
    }
}