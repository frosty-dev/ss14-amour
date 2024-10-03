namespace Content.Server._Honk.GameTicking.Rules.Components;

[RegisterComponent, Access(typeof(DemolitionistRuleSystem))]
public sealed partial class DemolitionistRuleComponent : Component
{
    [DataField("DemolitionistShuttlePath")]
    public string DemolitionistShuttlePath = "Maps/_Honk/Shuttles/BoomShuttle.yml";
}
