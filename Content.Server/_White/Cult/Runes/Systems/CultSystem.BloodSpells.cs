using System.Linq;
using Content.Server.Body.Components;
using Content.Shared._White.Cult;
using Content.Shared._White.Cult.Components;
using Content.Shared.DoAfter;
using Robust.Shared.Player;

namespace Content.Server._White.Cult.Runes.Systems;

public sealed partial class CultSystem
{
    public void InitializeSpells()
    {
        SubscribeLocalEvent<CultistComponent, CultEmpowerSelectedBuiMessage>(OnCultistEmpowerSelected);
        SubscribeLocalEvent<CultistComponent, CultEmpowerRemoveBuiMessage>(OnCultistEmpowerRemove);
        SubscribeLocalEvent<CultistComponent, SpellCreatedEvent>(OnSpellCreated);
    }

    private void OnCultistEmpowerRemove(Entity<CultistComponent> ent, ref CultEmpowerRemoveBuiMessage args)
    {
        var entity = GetEntity(args.ActionType);
        ent.Comp.SelectedEmpowers.Remove(args.ActionType);
        _actionsSystem.RemoveAction(entity);
        Dirty(ent);
    }

    private void OnSpellCreated(EntityUid ent, CultistComponent comp, SpellCreatedEvent args)
    {
        if (args.Cancelled || comp.SelectedEmpowers.Count >= 2)
            return;

        var action = CultistComponent.CultistActions.FirstOrDefault(x => x.Equals(args.Spell));

        if (action == null)
            return;

        var howMuchBloodTake = HasComp<CultBuffComponent>(ent) ? -10f : -20f;

        if (!TryComp<BloodstreamComponent>(ent, out var bloodstreamComponent))
            return;

        _bloodstreamSystem.TryModifyBloodLevel(ent, howMuchBloodTake, bloodstreamComponent, createPuddle: false);

        comp.SelectedEmpowers.Add(GetNetEntity(_actionsSystem.AddAction(ent, args.Spell)));

        Dirty(ent, comp);
    }

    private void OnCultistEmpowerSelected(EntityUid ent, CultistComponent comp, CultEmpowerSelectedBuiMessage args)
    {
        var action = CultistComponent.CultistActions.FirstOrDefault(x => x.Equals(args.ActionType));

        if (action == null)
            return;

        if (comp.SelectedEmpowers.Count >= 2)
        {
            _popupSystem.PopupEntity(Loc.GetString("blood-spell-create-too-much"), ent, ent);
            return;
        }

        var creationTime = HasComp<CultBuffComponent>(ent) ? 2.5f : 5f;

        _doAfterSystem.TryStartDoAfter(
            new DoAfterArgs(_entityManager, ent, creationTime, new SpellCreatedEvent {Spell = action}, ent)
            {
                BreakOnDamage = true,
                BreakOnMove = true
            });
    }

    public void CreateSpell(Entity<CultistComponent> ent, ICommonSession session)
    {
        if (!HasComp<ActorComponent>(ent.Owner))
            return;

        _ui.TryOpenUi(ent.Owner, CultEmpowerUiKey.Key, ent.Owner);
    }

    public void RemoveSpell(Entity<CultistComponent> ent, ICommonSession session)
    {
        if (ent.Comp.SelectedEmpowers.Count == 0)
        {
            _popupSystem.PopupEntity(Loc.GetString("blood-spell-remove-no-spells"), ent, ent);
            return;
        }

        if (!HasComp<ActorComponent>(ent.Owner))
            return;

        _ui.TryOpenUi(ent.Owner, CultEmpowerRemoveUiKey.Key, ent.Owner);
    }
}
