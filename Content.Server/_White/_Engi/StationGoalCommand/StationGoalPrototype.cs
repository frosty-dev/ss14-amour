using Robust.Shared.Prototypes;

namespace Content.Server._White._Engi.StationGoal
{
    /// <summary>
    /// WD ENGI EXCLUSIVE.
    /// </summary>
    [Serializable, Prototype("stationGoal")]
    public sealed class StationGoalPrototype : IPrototype
    {
        [IdDataField]
        public string ID { get; } = default!;

        [DataField]
        public string Text { get; set; } = string.Empty;

        [DataField]
        public int? MinPlayers;

        [DataField]
        public int? MaxPlayers;

        /// <summary>
        /// Goal may require certain items to complete. These items will appear near the receving fax machine at the start of the round.
        /// </summary>
        [DataField]
        public List<EntProtoId> Spawns = new();
    }
}
