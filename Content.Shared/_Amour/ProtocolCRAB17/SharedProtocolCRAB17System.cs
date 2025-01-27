using Content.Shared._White.Economy;
using Content.Shared.DoAfter;
using Content.Shared.Examine;
using Content.Shared.Interaction;
using Content.Shared.Interaction.Events;
using Content.Shared.Popups;
using Content.Shared.Timing;
using Robust.Shared.Audio.Systems;

namespace Content.Shared._Amour.ProtocolCRAB17;

public abstract class SharedProtocolCRAB17System : EntitySystem
{
    [Dependency] private readonly SharedPopupSystem _popupSystem = default!;
    [Dependency] private readonly UseDelaySystem _useDelay = default!;
    [Dependency] private readonly SharedAudioSystem _audioSystem = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfterSystem = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ProtocolCRAB17Component, UseInHandEvent>(OnUseInHand);
        SubscribeLocalEvent<ProtocolCRAB17Component, ProtocolCRAB17DoAfterEvent>(OnDoAfter);
        SubscribeLocalEvent<ProtocolCRAB17Component, InteractUsingEvent>(OnInteractUsing);
        SubscribeLocalEvent<ProtocolCRAB17Component, ExaminedEvent>(OnExamine);
    }

    private void OnInteractUsing(EntityUid uid, ProtocolCRAB17Component component, InteractUsingEvent args)
    {
        if (!TryComp(args.Used, out BankCardComponent? bankCard))
            return;

        if (!TryComp(uid, out UseDelayComponent? useDelay) ||
            !_useDelay.TryResetDelay((uid, useDelay), true))
            return;

        component.BankAccountId = bankCard.AccountId;
        component.HasBankAccountId = true;
        _popupSystem.PopupPredicted(Loc.GetString("protocol-CRAB17-card-accepted"), uid, args.User);
        _audioSystem.PlayPredicted(component.SoundApply, uid, args.User);
    }

    private void OnUseInHand(EntityUid uid, ProtocolCRAB17Component component, ref UseInHandEvent args)
    {
        if (args.Handled ||
            !TryComp(uid, out UseDelayComponent? useDelay) ||
            !_useDelay.TryResetDelay((uid, useDelay), true))
            return;

        if (component.HasBankAccountId == false)
        {
            _popupSystem.PopupClient(Loc.GetString("protocol-CRAB17-no-card"), uid, args.User);
            return;
        }

        var doAfterEventArgs = new DoAfterArgs(EntityManager, args.User, useDelay.Delay, new ProtocolCRAB17DoAfterEvent(), uid, target: uid)
        {
            BreakOnMove = true,
            BreakOnDamage = true,
            NeedHand = true
        };

        // TODO find a way to stop playsound on doafter cancel
        _audioSystem.PlayPredicted(component.UseSound, uid, args.User);
        _popupSystem.PopupClient(Loc.GetString("protocol-CRAB17-try-activate"), uid, args.User);
        _doAfterSystem.TryStartDoAfter(doAfterEventArgs);
        args.Handled = true;
    }

    private void OnExamine(EntityUid uid, ProtocolCRAB17Component component, ExaminedEvent args)
    {
        if (!TryComp<ProtocolCRAB17Component>(uid, out var comp))
            return;

        if (!args.IsInDetailsRange)
            return;

        string bankID = component.BankAccountId != null ? ((int) component.BankAccountId).ToString() : "отсутствует";

        var message = Loc.GetString("protocol-CRAB17-cardID", ("item", bankID));
        args.PushMarkup(message);
    }

    public abstract void OnDoAfter(Entity<ProtocolCRAB17Component> ent, ref ProtocolCRAB17DoAfterEvent args);

}
