using Content.Shared.ActionBlocker;
using Content.Server.Popups;
using Robust.Shared.Audio.Systems;
using Content.Shared.Interaction;
using Content.Shared.Verbs;
using Content.Server.Bible.Components;
using Content.Shared.Timing;
using Content.Shared.CombatMode.Pacification;

namespace Content.Server._White._Engi.PacifiedOnChaplainAction
{
    /// <summary>
    /// WD Engi Exclusive.
    /// </summary>
    public sealed class PacifiedOnChaplainAction : EntitySystem
    {
        [Dependency] private readonly ActionBlockerSystem _blocker = default!;
        [Dependency] private readonly PopupSystem _popupSystem = default!;
        [Dependency] private readonly SharedAudioSystem _audio = default!;
        [Dependency] private readonly UseDelaySystem _delay = default!;

        public override void Initialize()
        {
            base.Initialize();

            SubscribeLocalEvent<PacifiedOnChaplainActionComponent, AfterInteractEvent>(OnAfterInteract);
            SubscribeLocalEvent<PacifiedOnChaplainActionComponent, GetVerbsEvent<AlternativeVerb>>(AddPacifiedOnChaplainVerb);
        }

        private void Action(PacifiedOnChaplainActionComponent component, EntityUid target, EntityUid user)
        {
            var popup = "";

            if (HasComp<PacifiedComponent>(target))
            {
                popup = "unpacified-by-chaplain";
                RemComp<PacifiedComponent>(target);
            }
            else
            {
                popup = "pacified-by-chaplain";
                EnsureComp<PacifiedComponent>(target);
            }

            _popupSystem.PopupEntity(Loc.GetString(popup, ("target", target)), user, user);

            _audio.PlayPvs(component.ActionSound, user);

        }

        private void OnAfterInteract(EntityUid uid, PacifiedOnChaplainActionComponent component, AfterInteractEvent args)
        {
            if (!args.CanReach)
                return;

            if (!TryComp(uid, out UseDelayComponent? useDelay) || _delay.IsDelayed((uid, useDelay)))
                return;

            if (args.Target == null)
                return;

            if (!HasComp<BibleUserComponent>(args.User))
                return;

            Action(component, (EntityUid) args.Target, args.User);

            _delay.TryResetDelay((uid, useDelay));

            return;
        }
        private void AddPacifiedOnChaplainVerb(EntityUid uid, PacifiedOnChaplainActionComponent component, GetVerbsEvent<AlternativeVerb> args)
        {
            if (!args.CanInteract || !args.CanAccess)
                return;

            if (!HasComp<BibleUserComponent>(args.User))
                return;

            if (!_blocker.CanInteract(args.User, uid))
                return;

            var verbName = "";

            if (HasComp<PacifiedComponent>(args.Target))
                verbName = Loc.GetString("unpacify-by-chaplain");
            else
                verbName = Loc.GetString("pacify-by-chaplain");

            AlternativeVerb verb = new()
            {
                Act = () =>
                {
                    if (!TryComp(uid, out UseDelayComponent? useDelay) || _delay.IsDelayed((uid, useDelay)))
                        return;

                    Action(component, args.Target, args.User);

                    _delay.TryResetDelay((uid, useDelay));
                },
                Text = verbName,
                Priority = 2
            };
            args.Verbs.Add(verb);
        }
    }
}
