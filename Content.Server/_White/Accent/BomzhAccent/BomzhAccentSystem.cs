using Content.Server.Speech;
using Content.Server.Speech.EntitySystems;
using System.Text.RegularExpressions;
using Robust.Shared.Random;

namespace Content.Server._White.Accent.BomzhAccent;

public sealed class BomzhAccentSystem : EntitySystem
{
    [Dependency] private readonly ReplacementAccentSystem _replacement = default!;
    [Dependency] private readonly IRobustRandom _random = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<BomzhAccentComponent, AccentGetEvent>(OnAccent);
    }

    public string Accentuate(string message)
    {
        var msg = message;

        msg = Regex.Replace(msg, @"(?<!\w)\bженщина\b", _random.Pick(new List<string>() { "манда", "мадмуазель", "милая", "красотка", "шлюха" }), RegexOptions.IgnoreCase);
        msg = Regex.Replace(msg, @"(?<!\w)\bдевушка\b", _random.Pick(new List<string>() { "манда", "мадмуазель", "милая", "красотка", "шлюха" }), RegexOptions.IgnoreCase);
        msg = Regex.Replace(msg, @"(?<!\w)\bдевочка\b", _random.Pick(new List<string>() { "манда", "мадмуазель", "милая", "красотка", "шлюха" }), RegexOptions.IgnoreCase);
        msg = Regex.Replace(msg, @"(?<!\w)\bдоча\b", _random.Pick(new List<string>() { "манда", "мадмуазель", "милая", "красотка", "шлюха" }), RegexOptions.IgnoreCase);

        msg = Regex.Replace(msg, @"(?<!\w)\bмужик\b", _random.Pick(new List<string>() { "пацан", "мудила", "гандонио", "мудаёб", "подкаблучник" }), RegexOptions.IgnoreCase);
        msg = Regex.Replace(msg, @"(?<!\w)\bпарень\b", _random.Pick(new List<string>() { "пацан", "мудила", "гандонио", "мудаёб", "подкаблучник" }), RegexOptions.IgnoreCase);
        msg = Regex.Replace(msg, @"(?<!\w)\bчувак\b", _random.Pick(new List<string>() { "пацан", "мудила", "гандонио", "мудаёб", "подкаблучник" }), RegexOptions.IgnoreCase);
        msg = Regex.Replace(msg, @"(?<!\w)\bчел\b", _random.Pick(new List<string>() { "пацан", "мудила", "гандонио", "мудаёб", "подкаблучник" }), RegexOptions.IgnoreCase);

        msg = Regex.Replace(msg, @"(?<!\w)\bубит\b", _random.Pick(new List<string>() { "кокнут", "насажен", "загашен", "угандошен", "ёбнут" }), RegexOptions.IgnoreCase);
        msg = Regex.Replace(msg, @"(?<!\w)\bубил\b", _random.Pick(new List<string>() { "кокнул", "насадил", "загасил", "угандошил", "ёбнул" }), RegexOptions.IgnoreCase);
        msg = Regex.Replace(msg, @"(?<!\w)\bубили\b", _random.Pick(new List<string>() { "кокнули", "насадили", "загасили", "угандошили", "ёбнули" }), RegexOptions.IgnoreCase);

        msg = Regex.Replace(msg, @"(?<!\w)\bассистент\b", _random.Pick(new List<string>() { "браток", "братан", "новичок", "акробат" }), RegexOptions.IgnoreCase);
        msg = Regex.Replace(msg, @"(?<!\w)\bассистента\b", _random.Pick(new List<string>() { "братка", "братана", "новичка", "акробата" }), RegexOptions.IgnoreCase);
        msg = Regex.Replace(msg, @"(?<!\w)\bасик\b", _random.Pick(new List<string>() { "браток", "братан", "новичок", "акробат" }), RegexOptions.IgnoreCase);
        msg = Regex.Replace(msg, @"(?<!\w)\bасистуха\b", _random.Pick(new List<string>() { "браток", "братан", "новичок", "акробат" }), RegexOptions.IgnoreCase);

        msg = Regex.Replace(msg, @"(?<!\w)\bглава\b", _random.Pick(new List<string>() { "начальник", "контролёр", "заправляющий" }), RegexOptions.IgnoreCase);

        msg = Regex.Replace(msg, @"(?<!\w)\bсобака\b", _random.Pick(new List<string>() { "сука", "сучка" }), RegexOptions.IgnoreCase);

        msg = Regex.Replace(msg, @"(?<!\w)\bсигарета\b", _random.Pick(new List<string>() { "косяк", "косячок", "бычок" }), RegexOptions.IgnoreCase);
        msg = Regex.Replace(msg, @"(?<!\w)\bсигарету\b", _random.Pick(new List<string>() { "косяка", "косячка", "бычка" }), RegexOptions.IgnoreCase);
        msg = Regex.Replace(msg, @"(?<!\w)\bсигаретку\b", _random.Pick(new List<string>() { "косяка", "косячка", "бычка" }), RegexOptions.IgnoreCase);

        msg = Regex.Replace(msg, @"(?<!\w)\bалкоголик\b", _random.Pick(new List<string>() { "шатун", "алканавт", "запойный", "бражник" }), RegexOptions.IgnoreCase);
        msg = Regex.Replace(msg, @"(?<!\w)\bалкоголика\b", _random.Pick(new List<string>() { "шатуна", "алканават", "запойного", "бражника" }), RegexOptions.IgnoreCase);
        msg = Regex.Replace(msg, @"(?<!\w)\bалкаш\b", _random.Pick(new List<string>() { "шатун", "алканавт", "запойный", "бражник" }), RegexOptions.IgnoreCase);
        msg = Regex.Replace(msg, @"(?<!\w)\bалкаша\b", _random.Pick(new List<string>() { "шатуна", "алканават", "запойного", "бражника" }), RegexOptions.IgnoreCase);

        msg = Regex.Replace(msg, @"(?<!\w)\bпиво\b", _random.Pick(new List<string>() { "бухло", "моча", "зелье" }), RegexOptions.IgnoreCase);
        msg = Regex.Replace(msg, @"(?<!\w)\bпива\b", _random.Pick(new List<string>() { "бухла", "мочи", "зельеца" }), RegexOptions.IgnoreCase);
        msg = Regex.Replace(msg, @"(?<!\w)\bпивка\b", _random.Pick(new List<string>() { "бухла", "мочи", "зельеца" }), RegexOptions.IgnoreCase);
        msg = Regex.Replace(msg, @"(?<!\w)\bалкашки\b", _random.Pick(new List<string>() { "бухла", "мочи", "зельеца" }), RegexOptions.IgnoreCase);

        msg = Regex.Replace(msg, @"(?<!\w)\bводка\b", _random.Pick(new List<string>() { "баян", "газ", "сапог", "хань" }), RegexOptions.IgnoreCase);
        msg = Regex.Replace(msg, @"(?<!\w)\bводки\b", _random.Pick(new List<string>() { "баяна", "газа", "сапога", "хани" }), RegexOptions.IgnoreCase);
        msg = Regex.Replace(msg, @"(?<!\w)\bводяры\b", _random.Pick(new List<string>() { "баяна", "газа", "сапога", "хани" }), RegexOptions.IgnoreCase);
        msg = Regex.Replace(msg, @"(?<!\w)\bводочки\b", _random.Pick(new List<string>() { "баяна", "газа", "сапога", "хани" }), RegexOptions.IgnoreCase);

        msg = Regex.Replace(msg, @"(?<!\w)\bтерпила\b", _random.Pick(new List<string>() { "немощь", "лошара", "опущенец" }), RegexOptions.IgnoreCase);

        msg = Regex.Replace(msg, @"(?<!\w)\bжопа\b", _random.Pick(new List<string>() { "очко", "дристалище", "гузно" }), RegexOptions.IgnoreCase);
        msg = Regex.Replace(msg, @"(?<!\w)\bжопе\b", _random.Pick(new List<string>() { "очке", "дристалище" }), RegexOptions.IgnoreCase);

        msg = Regex.Replace(msg, @"(?<!\w)\bфелинид\b", _random.Pick(new List<string>() { "хвостатый", "ушастый", "мерзкий", "лизун" }), RegexOptions.IgnoreCase);
        msg = Regex.Replace(msg, @"(?<!\w)\bфелинида\b", _random.Pick(new List<string>() { "хвостатого", "ушастого", "мерзкого", "лизуна" }), RegexOptions.IgnoreCase);
        msg = Regex.Replace(msg, @"(?<!\w)\bфелинидка\b", _random.Pick(new List<string>() { "хвостатая", "ушастая", "мерзкая", "лизунщица" }), RegexOptions.IgnoreCase);
        msg = Regex.Replace(msg, @"(?<!\w)\bкотик\b", _random.Pick(new List<string>() { "хвостатый", "ушастый", "мерзкий", "лизун" }), RegexOptions.IgnoreCase);
        msg = Regex.Replace(msg, @"(?<!\w)\bкотика\b", _random.Pick(new List<string>() { "хвостатая", "ушастая", "мерзкая", "лизунщица" }), RegexOptions.IgnoreCase);

        msg = Regex.Replace(msg, @"(?<!\w)\bгарпия\b", _random.Pick(new List<string>() { "горливая", "птенчик", "крылатый", "яйцеукладчик" }), RegexOptions.IgnoreCase);
        msg = Regex.Replace(msg, @"(?<!\w)\bгарпию\b", _random.Pick(new List<string>() { "горливую", "птенчика", "крылатую", "яйцеукладчика" }), RegexOptions.IgnoreCase);

        msg = Regex.Replace(msg, @"(?<!\w)\bящер\b", _random.Pick(new List<string>() { "тупой", "заумный", "лысый", "гладкий", "яйцеглот" }), RegexOptions.IgnoreCase);
        msg = Regex.Replace(msg, @"(?<!\w)\bящера\b", _random.Pick(new List<string>() { "тупого", "заумного", "лысого", "гладкого", "яйцеглотателя" }), RegexOptions.IgnoreCase);

        msg = Regex.Replace(msg, @"(?<!\w)\bдворф\b", _random.Pick(new List<string>() { "карлик", "выёбистый", "работяга", "наглый" }), RegexOptions.IgnoreCase);
        msg = Regex.Replace(msg, @"(?<!\w)\bдворфа\b", _random.Pick(new List<string>() { "карлика", "выёбистого", "работягу", "наглого" }), RegexOptions.IgnoreCase);

        msg = Regex.Replace(msg, @"(?<!\w)\bстанбатон\b", _random.Pick(new List<string>() { "дрючка", "жезл", "кол" }), RegexOptions.IgnoreCase);
        msg = Regex.Replace(msg, @"(?<!\w)\bдубинка\b", _random.Pick(new List<string>() { "дрючка", "жезл", "кол" }), RegexOptions.IgnoreCase);
        msg = Regex.Replace(msg, @"(?<!\w)\bдубина\b", _random.Pick(new List<string>() { "дрючка", "жезл", "кол" }), RegexOptions.IgnoreCase);
        msg = Regex.Replace(msg, @"(?<!\w)\bдубинку\b", _random.Pick(new List<string>() { "дрючку", "жезл", "кол" }), RegexOptions.IgnoreCase);

        msg = Regex.Replace(msg, @"(?<!\w)\bдизейблер\b", _random.Pick(new List<string>() { "пукалка", "решала" }), RegexOptions.IgnoreCase);

        msg = Regex.Replace(msg, @"(?<!\w)\bграната\b", _random.Pick(new List<string>() { "картошка", "лимонка", "снаряд" }), RegexOptions.IgnoreCase);
        msg = Regex.Replace(msg, @"(?<!\w)\bгранату\b", _random.Pick(new List<string>() { "картошку", "лимонку", "снаряд" }), RegexOptions.IgnoreCase);

        msg = string.Concat(msg[0].ToString().ToUpper(), msg.AsSpan(1));

        msg = _replacement.ApplyReplacements(msg, "bomzh");

        return msg;
    }

    private void OnAccent(EntityUid uid, BomzhAccentComponent component, AccentGetEvent args)
    {
        args.Message = Accentuate(args.Message);
    }
}
