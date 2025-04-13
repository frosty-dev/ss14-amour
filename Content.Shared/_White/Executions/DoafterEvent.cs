using Content.Shared.DoAfter;
using Robust.Shared.Serialization;

namespace Content.Shared._White.Executions;

[Serializable, NetSerializable]
public sealed partial class ExecutionDoAfterEvent : SimpleDoAfterEvent
{
}
