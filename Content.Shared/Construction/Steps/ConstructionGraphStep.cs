﻿using Content.Shared.Examine;
using Content.Shared.Whitelist;

namespace Content.Shared.Construction.Steps
{
    [Serializable]
    [ImplicitDataDefinitionForInheritors]
    public abstract partial class ConstructionGraphStep
    {
        [DataField("completed", serverOnly: true)] private IGraphAction[] _completed = Array.Empty<IGraphAction>();

        [DataField("doAfter")] public float DoAfter { get; private set; }

        [DataField]
        public EntityWhitelist? UserWhitelist { get; private set; } // WD

        public IReadOnlyList<IGraphAction> Completed => _completed;

        public abstract void DoExamine(ExaminedEvent examinedEvent);
        public abstract ConstructionGuideEntry GenerateGuideEntry();
    }
}
