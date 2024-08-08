using Content.Shared.Chemistry.Components;
using Content.Shared.Chemistry.Reagent;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Server._White.AutoRegenReagent
{
    [RegisterComponent]
    public sealed partial class AutoRegenReagentComponent : Component
    {
        [DataField("solution", required: true)]
        public string? SolutionName = null; // we'll fail during tests otherwise

        [DataField("reagents", required: true)]
        public List<string> Reagents = default!;

        [DataField]
        public string CurrentReagent = "";

        [DataField]
        public int CurrentIndex = 0;

        public Entity<SolutionComponent>? Solution = default!;

        [DataField("interval")]
        public TimeSpan Interval = TimeSpan.FromSeconds(10);

        [DataField(customTypeSerializer: typeof(TimeOffsetSerializer))]
        public TimeSpan NextUpdate;

        [DataField("unitsPerInterval")]
        public float UnitsPerInterval = 1f;
    }
}
