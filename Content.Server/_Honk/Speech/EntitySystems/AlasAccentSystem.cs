using System.Linq;
using Content.Server._Honk.Speech.Components;
using Content.Server.Speech;
using Content.Server.Speech.EntitySystems;

namespace Content.Server._Honk.Speech.EntitySystems;

public sealed class AlasAccentSystem : EntitySystem
{
    [Dependency] private readonly ReplacementAccentSystem _replacement = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<AlasAccentComponent, AccentGetEvent>(OnAccent);
    }

    private void OnAccent(EntityUid uid, AlasAccentComponent component, AccentGetEvent args)
    {
        var message = args.Message;

        message = _replacement.ApplyReplacements(message, "alas");

        var words = message.Split();
        if (words.Length > 3)
        {
            var lastWord = words.Last();
            var modificator = "";
            switch (lastWord.Last())
            {
                case '.':
                    modificator = @" Увы.";
                    break;
                case ',':
                    modificator = @" увы.";
                    break;
                default:
                    modificator = @", увы.";
                    break;
            }
            message = message + modificator;
        }

        args.Message = message;
    }
};
