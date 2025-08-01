﻿using Content.Shared._White.Guardian;
using Content.Shared.DoAfter;
using Robust.Shared.Serialization;

namespace Content.Shared.Guardian;

[Serializable, NetSerializable]
public sealed partial class GuardianCreatorDoAfterEvent : SimpleDoAfterEvent
{
    public GuardianSelector SelectedType; // Parsec Edit
}
