using System.Text;
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

        var temp = @"";
        var words = message.Split();
        var rnd = new Random();
        if (words.Length > 2)
        {
            var value = rnd.Next(0, 2);
            if (value == 1)
                message = message + @" ежжи брат!";
        }
        words = message.Split();
        for (var i = 0; i < words.Length; i++)
        {
            if (words[i] == @"нт" || words[i] == @"Нт" || words[i] == @"НТ")
            {
                if (words[i] != @"НТ")
                    words[i] = words[i].Replace(words[i], @"Халифат");
                else
                    words[i] = words[i].Replace(words[i], @"ХАЛИФАТ");
            }
            if (words[i] == @"вау")
                words[i] = words[i].Replace(words[i], @"ой вей");
            if (words[i] == @"Вау")
                words[i] = words[i].Replace(words[i], @"Ой вей");
            if (words[i] == @"брат")
                words[i] = words[i].Replace(words[i], @"ежжи брат");
            if (words[i] == @"Брат")
                words[i] = words[i].Replace(words[i], @"Ежжи брат");
            if (words[i] == @"Бог" || words[i] == @"бог")
                words[i] = words[i].Replace(words[i], @"Аллах");
            if (words[i] == @"Богу" || words[i] == @"богу")
                words[i] = words[i].Replace(words[i], @"Аллаху");
            if (words[i] == @"Бога" || words[i] == @"бога")
                words[i] = words[i].Replace(words[i], @"Аллаха");
            if (i != words.Length - 1)
                temp = temp + words[i] + @" ";
            else
                temp = temp + words[i];
        }
        message = temp;
        temp = @"";

        message = Regex.Replace(message, @"Свинья", "Хиндзир");

        message = Regex.Replace(message, @"Здравствуйте", "Ас-саляму алейкум ва-рахмату-ллахи ва-баракятух");
        message = Regex.Replace(message, @"здравствуйте", "ас-саляму алейкум ва-рахмату-ллахи ва-баракятух");
        message = Regex.Replace(message, @"Здравствуй", "Ас-саляму алейкум ва-рахмату-ллахи ва-баракятух");
        message = Regex.Replace(message, @"здравствуй", "ас-саляму алейкум ва-рахмату-ллахи ва-баракятух");

        message = Regex.Replace(message, @"Законы", "Шариат");
        message = Regex.Replace(message, @"законы", "шариат");
        message = Regex.Replace(message, @"Закону", "Шариату");
        message = Regex.Replace(message, @"закону", "шариату");
        message = Regex.Replace(message, @"Закона", "Шариата");
        message = Regex.Replace(message, @"закона", "шариата");
        message = Regex.Replace(message, @"Закон", "Шариат");
        message = Regex.Replace(message, @"закон", "шариат");

        message = Regex.Replace(message, @"Библия", "Коран");
        message = Regex.Replace(message, @"библия", "коран");
        message = Regex.Replace(message, @"Библию", "Коран");
        message = Regex.Replace(message, @"библию", "коран");
        message = Regex.Replace(message, @"Библии", "Корана");
        message = Regex.Replace(message, @"библии", "корана");

        message = Regex.Replace(message, @"Священнику", "Имаму");
        message = Regex.Replace(message, @"священнику", "имаму");
        message = Regex.Replace(message, @"Священника", "Имама");
        message = Regex.Replace(message, @"священника", "имама");
        message = Regex.Replace(message, @"Священники", "Имамы");
        message = Regex.Replace(message, @"священники", "имамы");
        message = Regex.Replace(message, @"Священник", "Имам");
        message = Regex.Replace(message, @"священник", "имам");
        message = Regex.Replace(message, @"Священику", "Имаму");
        message = Regex.Replace(message, @"священику", "имаму");
        message = Regex.Replace(message, @"Священика", "Имама");
        message = Regex.Replace(message, @"священика", "имама");
        message = Regex.Replace(message, @"Священики", "Имамы");
        message = Regex.Replace(message, @"священики", "имамы");
        message = Regex.Replace(message, @"Священик", "Имам");
        message = Regex.Replace(message, @"священик", "имам");

        message = Regex.Replace(message, @"Нельзя", "Харам");
        message = Regex.Replace(message, @"нельзя", "харам");
        message = Regex.Replace(message, @"Запрещаю", "Харам");
        message = Regex.Replace(message, @"запрещаю", "харам");
        message = Regex.Replace(message, @"Запрет", "Харам");
        message = Regex.Replace(message, @"запрет", "харам");

        message = Regex.Replace(message, @"Разрещаю", "Халял");
        message = Regex.Replace(message, @"разрешаю", "халял");
        message = Regex.Replace(message, @"Можно", "Халял");
        message = Regex.Replace(message, @"можно", "халял");

        message = Regex.Replace(message, @"Нанотрайзен", "Халифат");
        message = Regex.Replace(message, @"нанотрайзен", "халифат");

        message = Regex.Replace(message, @"Ревенанты", "Иблисы");
        message = Regex.Replace(message, @"ревенанты", "иблисы");
        message = Regex.Replace(message, @"Ревенанту", "Иблису");
        message = Regex.Replace(message, @"ревенанту", "иблису");
        message = Regex.Replace(message, @"Ревенанта", "Иблиса");
        message = Regex.Replace(message, @"ревенанта", "иблиса");
        message = Regex.Replace(message, @"Ревенант", "Иблис");
        message = Regex.Replace(message, @"ревенант", "иблис");

        message = Regex.Replace(message, @"Привет", "Ас-саля́му але́йкум");
        message = Regex.Replace(message, @"привет", "ас-саля́му але́йкум");
        message = Regex.Replace(message, @"Здарова", "Ас-саля́му але́йкум");
        message = Regex.Replace(message, @"здарова", "ас-саля́му але́йкум");
        message = Regex.Replace(message, @"Здравие", "Ас-саля́му але́йкум");
        message = Regex.Replace(message, @"здравие", "ас-саля́му але́йкум");

        message = Regex.Replace(message, @"Спасибо", "Альхамдуллиля");
        message = Regex.Replace(message, @"спасибо", "альхамдуллиля");

        message = Regex.Replace(message, @"Л", "Ль");
        message = Regex.Replace(message, @"л", "ль");

        message = Regex.Replace(message, @"К", "Кх");
        message = Regex.Replace(message, @"к", "кх");

        message = Regex.Replace(message, @" В", " У");
        message = Regex.Replace(message, @" в", " у");

        message = Regex.Replace(message, @" В", " У");
        message = Regex.Replace(message, @" в", " у");

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
