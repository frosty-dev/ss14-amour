using Robust.Shared.Prototypes;

namespace Content.Server._White._Engi.StationGoal
{
    /// <summary>
    /// WD ENGI EXCLUSIVE.
    /// If attached to a station prototype, will send the station a random goal from the list.
    /// </summary>
    [RegisterComponent]
    public sealed partial class StationGoalComponent : Component
    {
        [DataField]
        public List<ProtoId<StationGoalPrototype>> Goals = new();
    }
}
