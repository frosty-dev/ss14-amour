﻿using System.Numerics;
using Robust.Shared.Serialization;

namespace Content.Shared._White.Animations;

[Serializable, NetSerializable]
public sealed class StartedWaddlingEvent(NetEntity user) : EntityEventArgs
{
    public NetEntity User = user;
}

[Serializable, NetSerializable]
public sealed class StoppedWaddlingEvent(NetEntity user) : EntityEventArgs
{
    public NetEntity User = user;
}

[RegisterComponent]
public sealed partial class WaddleAnimationComponent : Component
{
    /// <summary>
    /// What's the name of this animation? Make sure it's unique so it can play along side other animations.
    /// This prevents someone accidentally causing two identical waddling effects to play on someone at the same time.
    /// </summary>
    [DataField]
    public string KeyName = "Waddle";

    ///<summary>
    /// How high should they hop during the waddle? Higher hop = more energy.
    /// </summary>
    [DataField]
    public Vector2 HopIntensity = new(0, 0.25f);

    /// <summary>
    /// How far should they rock backward and forward during the waddle?
    /// Each step will alternate between this being a positive and negative rotation. More rock = more scary.
    /// </summary>
    [DataField]
    public float TumbleIntensity = 20.0f;

    /// <summary>
    /// How long should a complete step take? Less time = more chaos.
    /// </summary>
    [DataField]
    public float AnimationLength = 0.66f;

    /// <summary>
    /// How much shorter should the animation be when running?
    /// </summary>
    [DataField]
    public float RunAnimationLengthMultiplier = 0.568f;

    /// <summary>
    /// Stores which step we made last, so if someone cancels out of the animation mid-step then restarts it looks more natural.
    /// </summary>
    public bool LastStep;

    /// <summary>
    /// Stores if we're currently waddling so we can start/stop as appropriate and can tell other systems our state.
    /// </summary>
    public bool IsCurrentlyWaddling;
}
