using Content.Shared.Atmos;
using Content.Shared.Clothing.Components;
using Content.Shared.Inventory;
using Content.Shared.Examine; // WD
using Content.Shared.Verbs; // WD
using Robust.Shared.Utility; // WD

namespace Content.Shared.Clothing.EntitySystems;

/// <summary>
/// Handles reducing fire damage when wearing clothing with <see cref="FireProtectionComponent"/>.
/// </summary>
public sealed class FireProtectionSystem : EntitySystem
{
    [Dependency] private readonly ExamineSystemShared _examine = default!; // WD

    private const string IconTexture = "/Textures/Interface/VerbIcons/dot.svg.192dpi.png"; // WD

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<FireProtectionComponent, InventoryRelayedEvent<GetFireProtectionEvent>>(OnGetProtection);
        SubscribeLocalEvent<FireProtectionComponent, GetVerbsEvent<ExamineVerb>>(OnProtectionVerbExamine); // WD
    }

    private void OnGetProtection(Entity<FireProtectionComponent> ent, ref InventoryRelayedEvent<GetFireProtectionEvent> args)
    {
        args.Args.Reduce(ent.Comp.Reduction);
    }

    // WD EDIT START
    private void OnProtectionVerbExamine(Entity<FireProtectionComponent> entity, ref GetVerbsEvent<ExamineVerb> args)
    {
        if (!args.CanInteract || !args.CanAccess)
            return;

        var modifierPercentage = MathF.Round(entity.Comp.Reduction * 100f, 1);

        if (modifierPercentage == float.NegativeZero)
            return;

        var msg = new FormattedMessage();

        msg.AddMarkup(Loc.GetString("fire-protection-examine", ("modifier", modifierPercentage)));

        _examine.AddDetailedExamineVerb(args, entity.Comp, msg,
            Loc.GetString("fire-protection-examinable-verb-text"), IconTexture,
            Loc.GetString("fire-protection-examinable-verb-message"));
    }
    // WD EDIT END
}
