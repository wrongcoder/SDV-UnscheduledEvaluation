using StardewValley;
using StardewValley.Extensions;

namespace UnscheduledEvaluation;

// ReSharper disable InconsistentNaming
public class AddEvaluation1MailPatch
{
    public const string Evaluation1Mail = "UnscheduledEvaluation-SawEvaluation1";

    public static void Postfix(Event __instance)
    {
        if (__instance.id == "558291")
        {
            Game1.player.eventsSeen.Toggle<string>("321777", false);
            Game1.addMailForTomorrow(Evaluation1Mail, true);
        }
    }
}