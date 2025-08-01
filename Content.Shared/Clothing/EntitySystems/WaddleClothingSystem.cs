﻿using Content.Shared._White.Animations;
using Content.Shared.Clothing.Components;
using Content.Shared.Inventory.Events;

namespace Content.Shared.Clothing.EntitySystems;

public sealed class WaddleClothingSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<WaddleWhenWornComponent, GotEquippedEvent>(OnGotEquipped);
        SubscribeLocalEvent<WaddleWhenWornComponent, GotUnequippedEvent>(OnGotUnequipped);
    }

    private void OnGotEquipped(EntityUid entity, WaddleWhenWornComponent comp, GotEquippedEvent args)
    {
        var waddleAnimComp = EnsureComp<WaddleAnimationComponent>(args.Equipee);

        waddleAnimComp.AnimationLength = comp.AnimationLength;
        waddleAnimComp.HopIntensity = comp.HopIntensity;
        waddleAnimComp.RunAnimationLengthMultiplier = comp.RunAnimationLengthMultiplier;
        waddleAnimComp.TumbleIntensity = comp.TumbleIntensity;
    }

    private void OnGotUnequipped(EntityUid entity, WaddleWhenWornComponent comp, GotUnequippedEvent args)
    {
        RemComp<WaddleAnimationComponent>(args.Equipee);
    }
}
