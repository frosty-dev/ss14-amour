using Content.Server.Objectives.Interfaces;
using JetBrains.Annotations;
using Robust.Shared.Utility;

namespace Content.Server.Objectives.Conditions;

[UsedImplicitly]
[DataDefinition]
public sealed class CustomCondition : IObjectiveCondition
{
    public string CustomTitle = string.Empty;
    public string CustomDesc = string.Empty;
    public float CustomProgress;

    public string Title => CustomTitle;
    public string Description => CustomDesc;
    public SpriteSpecifier Icon => new SpriteSpecifier.Rsi(new("Mobs/Species/Human/parts.rsi"), "full");
    public float Progress => CustomProgress;
    public float Difficulty => 0f;

    public bool Equals(IObjectiveCondition? other)
    {
        return other is CustomCondition condition && condition.Description == CustomDesc && condition.Title == CustomTitle;
    }

    public IObjectiveCondition GetAssigned(Mind.Mind mind)
    {
        return new CustomCondition
        {
            CustomTitle = Title,
            CustomDesc = Description,
            CustomProgress = Progress
        };
    }
}
