using System.Text.RegularExpressions;
using Content.Server._Honk.Speech.Components;
using Content.Server.Speech;
using Content.Server.Speech.EntitySystems;

namespace Content.Server._Honk.Speech.EntitySystems;

public sealed class CaucasianAccentSystem : EntitySystem
{
    [Dependency] private readonly ReplacementAccentSystem _replacement = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<CaucasianAccentComponent, AccentGetEvent>(OnAccent);
    }

    private void OnAccent(EntityUid uid, CaucasianAccentComponent component, AccentGetEvent args)
    {
        var message = args.Message;

        message = _replacement.ApplyReplacements(message, "caucasian");

        //They shoulda started runnin' an' hidin' from me!

        message = Regex.Replace(message, @"Эй", "Эу");
        message = Regex.Replace(message, @"эй", "эу");

        message = Regex.Replace(message, @"ые", "ии");

        message = Regex.Replace(message, @"Ы", "И");
        message = Regex.Replace(message, @"ы", "и");

        message = Regex.Replace(message, @"Ё", "Йо");
        message = Regex.Replace(message, @"ё", "йо");

        message = Regex.Replace(message, @"Е", "Э");
        message = Regex.Replace(message, @"е", "э");

        args.Message = message;
    }
};
