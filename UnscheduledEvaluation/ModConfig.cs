namespace UnscheduledEvaluation;

public sealed class ModConfig
{
    public bool EnableAlwaysActiveShrinePatch { get; set; } = true;

    public string? GrandpaNote { get; set; } =
        "{0}-\n\nWhen you are ready for me, place a diamond on this shrine.\n\n-Grandpa";
}