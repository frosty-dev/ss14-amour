namespace Content.Server.Changeling;

[RegisterComponent, Access(typeof(ChangelingRuleSystem))]
public sealed partial class ChangelingRuleComponent : Component
{
    public readonly List<EntityUid> ChangelingMinds = new() { };

    public readonly int ChangelingMaxDifficulty = 5;
    public readonly int ChangelingMaxPicks = 20;

    public int TotalChangelings => ChangelingMinds.Count;
}
