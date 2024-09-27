using System.Text;
using System.Text.RegularExpressions;
using Content.Server._Honk.Speech.Components;
using Content.Server.Speech;
using Content.Server.Speech.EntitySystems;

namespace Content.Server._Honk.Speech.EntitySystems;

public sealed class JewishAccentSystem : EntitySystem
{
    [Dependency] private readonly ReplacementAccentSystem _replacement = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<JewishAccentComponent, AccentGetEvent>(OnAccent);
    }

    private void OnAccent(EntityUid uid, JewishAccentComponent component, AccentGetEvent args)
    {
        var message = args.Message;

        message = _replacement.ApplyReplacements(message, "jewish");

        var temp = @"";
        var words = message.Split();
        var rnd = new Random();
        var value = 0;
        if (words.Length >= 2)
        {
            if (words.Length >= 4)
            {
                value = rnd.Next(0, 2);
                if (value == 1)
                    message = message + @" дай шекелей!";
            }
        }
        words = message.Split();
        for (var i = 0; i < words.Length; i++)
        {
            if (words[i] == @"Здравствуйте" || words[i] == @"Здравствуй")
                words[i] = words[i].Replace(words[i], "Шалом ходячий кошелёк");
            if (words[i] == @"здравствуйте" || words[i] == @"здравствуй")
                words[i] = words[i].Replace(words[i], "шалом ходячий кошелёк");
            if (words[i] == "да")
                words[i] = words[i].Replace(words[i], "таки да");
            if (words[i] == "да")
                words[i] = words[i].Replace(words[i], "таки да");
            if (words[i] == "Да")
                words[i] = words[i].Replace(words[i], "Таки да");
            if (words[i] == "вау")
                words[i] = words[i].Replace(words[i], "ой вей");
            if (words[i] == "Вау")
                words[i] = words[i].Replace(words[i], "Ой вей");
            if (words[i] == "Плати")
                words[i] = words[i].Replace(words[i], "Отдай шекели");
            if (words[i] == "плати")
                words[i] = words[i].Replace(words[i], "отдай шекели");
            if (i != words.Length - 1)
                temp = temp + words[i] + @" ";
            else
                temp = temp + words[i];
        }
        message = temp;
        temp = @"";
        message = Regex.Replace(message, @"Привет", "шалом");
        message = Regex.Replace(message, @"привет", "Шалом");

        message = Regex.Replace(message, @"Р", "Л");
        message = Regex.Replace(message, @"р", "л");
        message = Regex.Replace(message, @"ы", "и");

        args.Message = message;
    }
};
