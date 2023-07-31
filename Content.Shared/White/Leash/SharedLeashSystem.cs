using System.Linq;
using System.Numerics;
using Content.Shared.Body.Components;
using Content.Shared.Interaction;
using Content.Shared.Interaction.Events;
using Content.Shared.Item;
using Content.Shared.Physics;
using Robust.Shared.Physics;
using Robust.Shared.Physics.Systems;
using Robust.Shared.Serialization;
using Robust.Shared.Utility;

namespace Content.Shared.White.Leash;

public sealed class SharedLeashSystem : EntitySystem
{

    [Dependency] private readonly SharedJointSystem _joints = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<LeashComponent,AfterInteractEvent>(OnAfterInteract);

        SubscribeLocalEvent<LeashedComponent,LeashedEvent>(OnStartup);
        SubscribeLocalEvent<LeashedComponent,ComponentShutdown>(OnShutDown);
        SubscribeLocalEvent<LeashedComponent, PickupAttemptEvent>(OnPickup);

        SubscribeLocalEvent<LeashComponent,UseInHandEvent>(OnUseInHand);
    }

    private void OnPickup(EntityUid uid, LeashedComponent component, PickupAttemptEvent args)
    {
        if(component.LeashUid == args.Item)
            UnleashEntity(component.LeashUid,uid);
    }

    private void OnUseInHand(EntityUid uid, LeashComponent component, UseInHandEvent args)
    {
        foreach (var leashed in component.LeashedUid.ToList())
        {
            UnleashEntity(uid,leashed,component);
        }
    }

    private void OnShutDown(EntityUid uid, LeashedComponent component, ComponentShutdown args)
    {
        RemComp<JointVisualsComponent>(component.LeashUid);

        _joints.RemoveJoint(component.LeashUid,LeashedComponent.LeashJoint);
    }

    private void OnStartup(EntityUid uid, LeashedComponent component, LeashedEvent args)
    {
        var visuals = EnsureComp<JointVisualsComponent>(component.LeashUid);
        visuals.Sprite =
            new SpriteSpecifier.Rsi(new ResPath("Objects/Weapons/Guns/Launchers/grappling_gun.rsi"), "rope");
        visuals.OffsetA = new Vector2(0f, 0.2f);
        visuals.Target = uid;
        Dirty(visuals);

        var jointComp = EnsureComp<JointComponent>(uid);
        var joint = _joints.CreateDistanceJoint(args.LeashUid, uid, anchorA: new Vector2(0f, 0.5f), id: LeashedComponent.LeashJoint);
        joint.MaxLength = joint.Length;
        joint.Stiffness = 1f;
        joint.MinLength = 0.35f;
        Dirty(jointComp);
    }

    private void OnAfterInteract(EntityUid uid, LeashComponent component, AfterInteractEvent args)
    {
        if(!args.Target.HasValue || !TryComp<BodyComponent>(args.Target,out var bodyComponent) || !args.CanReach || args.User == args.Target)
            return;

        LeashEntity(uid,args.Target.Value,component,bodyComponent);
    }

    public void LeashEntity(EntityUid leashUid, EntityUid uid, LeashComponent? leashComponent = null,
        BodyComponent? bodyComponent = null)
    {
        if(!Resolve(uid,ref bodyComponent) || !Resolve(leashUid,ref leashComponent))
            return;

        if (TryComp<LeashedComponent>(uid,out var leashedComponent))
        {
            UnleashEntity( leashUid,uid, leashComponent, leashedComponent);
            return;
        }

        if(leashComponent.LeashedCount >= leashComponent.LeashedMax)
            return;

        leashedComponent = EnsureComp<LeashedComponent>(uid);
        leashedComponent.LeashUid = leashUid;
        Dirty(leashedComponent);

        leashComponent.LeashedUid.Add(uid);
        leashComponent.LeashedCount += 1;
        RaiseLocalEvent(uid,new LeashedEvent(leashUid));
    }



    public void UnleashEntity(EntityUid leashUid, EntityUid uid, LeashComponent? leashComponent = null,
        LeashedComponent? leashedComponent = null)
    {
        if(!Resolve(uid,ref leashedComponent) || !Resolve(leashUid,ref leashComponent))
            return;

        leashComponent.LeashedCount -= 1;
        leashComponent.LeashedUid.Remove(uid);
        Dirty(leashedComponent);
        RemComp(uid, leashedComponent);
    }
}

[Serializable, NetSerializable]
public sealed class LeashedEvent : EntityEventArgs
{
    public EntityUid LeashUid;

    public LeashedEvent(EntityUid leashUid)
    {
        LeashUid = leashUid;
    }
}
