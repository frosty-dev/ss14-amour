using Content.Server.Speech.Components;

namespace Content.Server.Speech.EntitySystems;

/// <summary>
/// System that Gnomes the Gnomes talking
/// </summary>
public sealed class GnomeAccentSystem : EntitySystem
{
    [Dependency] private readonly ReplacementAccentSystem _replacement = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<GnomeAccentComponent, AccentGetEvent>(OnAccentGet);
    }
    public string Accentuate(string message, GnomeAccentComponent component)
    {
        var msg = message;

        msg = _replacement.ApplyReplacements(msg, "gnome");

        return msg;
    }


    private void OnAccentGet(EntityUid uid, GnomeAccentComponent component, AccentGetEvent args)
    {
        args.Message = Accentuate(args.Message, component);
    }
}
