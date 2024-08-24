using System.Text.RegularExpressions;
using Content.Server.Speech.Components;

namespace Content.Server.Speech.EntitySystems;

public sealed class MothAccentSystem : EntitySystem
{
    private static readonly Regex RegexLowerBuzz = new Regex("z{1,3}");
    private static readonly Regex RegexUpperBuzz = new Regex("Z{1,3}");

    private static readonly Regex RussianRegexLowerZ = new Regex("Z{1,3}"); // WD
    private static readonly Regex RussianRegexUpperZ = new Regex("Z{1,3}"); // WD
    private static readonly Regex RussianRegexLowerZH = new Regex("Z{1,3}"); // WD
    private static readonly Regex RussianRegexUpperZH = new Regex("Z{1,3}"); // WD

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<MothAccentComponent, AccentGetEvent>(OnAccent);
    }

    private void OnAccent(EntityUid uid, MothAccentComponent component, AccentGetEvent args)
    {
        var message = args.Message;

        // buzzz
        message = RegexLowerBuzz.Replace(message, "zzz");
        // buZZZ
        message = RegexUpperBuzz.Replace(message, "ZZZ");

        // WD EDIT START
        message = RussianRegexLowerZ.Replace(message, "ззз");

        message = RussianRegexUpperZ.Replace(message, "ЗЗЗ");

        message = RussianRegexLowerZH.Replace(message, "жжж");

        message = RussianRegexUpperZH.Replace(message, "ЖЖЖ");
        // WD EDIT END

        args.Message = message;
    }
}
