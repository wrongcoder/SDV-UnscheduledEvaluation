using StardewValley;
using xTile.Dimensions;

namespace UnscheduledEvaluation;

// ReSharper disable InconsistentNaming
public class AlwaysActiveShrinePatch
{
    public struct PatchState
    {
        internal int year;
        internal int grandpaScore;
    }

    public static void Prefix(out PatchState __state, Farm __instance, Location tileLocation, Farmer who)
    {
        __state.year = Game1.year;
        __state.grandpaScore = __instance.grandpaScore.Value;
        if (doReplaceYear(Game1.year)) Game1.year = 3;
        if (doReplaceGrandpaScore(__instance.grandpaScore.Value)) __instance.grandpaScore.Value = 1;
    }

    public static void Postfix(PatchState __state, Farm __instance, Farmer who)
    {
        if (doReplaceYear(__state.year)) Game1.year = __state.year;
        if (doReplaceGrandpaScore(__state.grandpaScore)) __instance.grandpaScore.Value = __state.grandpaScore;
    }

    private static bool doReplaceYear(int oldYear)
    {
        return oldYear < 3;
    }

    private static bool doReplaceGrandpaScore(int oldGrandpaScore)
    {
        return oldGrandpaScore < 1 | oldGrandpaScore >= 4;
    }
}