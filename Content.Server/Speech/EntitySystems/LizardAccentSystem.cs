using System.Text.RegularExpressions;
using Content.Server.Speech.Components;
using Robust.Shared.Random;

namespace Content.Server.Speech.EntitySystems;

public sealed class LizardAccentSystem : EntitySystem
{
    [Dependency] private readonly IRobustRandom _random = default!;

    private static readonly Regex RegexLowerS = new("s+");
    private static readonly Regex RegexUpperS = new("S+");
    private static readonly Regex RegexInternalX = new(@"(\w)x");
    private static readonly Regex RegexLowerEndX = new(@"\bx([\-|r|R]|\b)");
    private static readonly Regex RegexUpperEndX = new(@"\bX([\-|r|R]|\b)");

    // Russian
    private static readonly Regex RussianRegexLowerS = new("с+");
    private static readonly Regex RussianRegexUpperS = new("С+");
    private static readonly Regex RussianRegexLowerZ = new("з+");
    private static readonly Regex RussianRegexUpperZ = new("З+");
    private static readonly Regex RussianRegexLowerSh= new("ш+");
    private static readonly Regex RussianRegexUpperSh= new("Ш+");
    private static readonly Regex RussianRegexLowerСh= new("ч+");
    private static readonly Regex RussianRegexUpperСh= new("Ч+");

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<LizardAccentComponent, AccentGetEvent>(OnAccent);
    }

    private void OnAccent(EntityUid uid, LizardAccentComponent component, AccentGetEvent args)
    {
        var message = args.Message;

        // hissss
        message = RegexLowerS.Replace(message, "sss");
        // hiSSS
        message = RegexUpperS.Replace(message, "SSS");
        // ekssit
        message = RegexInternalX.Replace(message, "$1kss");
        // ecks
        message = RegexLowerEndX.Replace(message, "ecks$1");
        // eckS
        message = RegexUpperEndX.Replace(message, "ECKS$1");


        //WD-EDIT start

        // c => ссс
        message = RussianRegexLowerS.Replace(
            message,
            _random.Pick(new List<string>() { "сс", "ссс" })
        );
        // С => CCC
        message = RussianRegexUpperS.Replace(
            message,
            _random.Pick(new List<string>() { "СС", "ССС" })
        );
        // з => ссс
        message = RussianRegexLowerZ.Replace(
            message,
            _random.Pick(new List<string>() { "сс", "ссс" })
        );
        // З => CCC
        message = RussianRegexUpperZ.Replace(
            message,
            _random.Pick(new List<string>() { "СС", "ССС" })
        );
        // ш => шшш
        message = RussianRegexLowerSh.Replace(
            message,
            _random.Pick(new List<string>() { "шш", "шшш" })
        );
        // Ш => ШШШ
        message = RussianRegexUpperSh.Replace(
            message,
            _random.Pick(new List<string>() { "ШШ", "ШШШ" })
        );
        // ч => щщщ
        message = RussianRegexLowerСh.Replace(
            message,
            _random.Pick(new List<string>() { "щщ", "щщщ" })
        );
        // Ч => ЩЩЩ
        message = RussianRegexUpperСh.Replace(
            message,
            _random.Pick(new List<string>() { "ЩШ", "ЩЩЩ" })
        );

        //WD-EDIT end

        args.Message = message;
    }
}
