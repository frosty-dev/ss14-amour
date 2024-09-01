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

        var words = message.Split();
        var rnd = new Random();
        var value = 0;
        if (words.Length >= 2)
        {
            if (words.Length >= 4)
            {
                value = rnd.Next(0, words.Length);
                var temp = @"";
                for (int i = 0; i < words.Length; i++)
                {
                    temp = temp + words[i] + " ";
                    if (i == value)
                    {
                        temp = temp + @"дай шекелей ";
                    }
                }
                message = temp;
            }
            value = rnd.Next(0, 2);
            if (value == 1)
            {
                words = message.Split();
                words[0].ToLower();
                message = @"Таки " + message;
            }
        }
        message = Regex.Replace(message, @"Привет", "шалом ");
        message = Regex.Replace(message, @"привет", "Шалом ");

        message = Regex.Replace(message, @"Здравствуйте", "Шалом ходячий кошелёк");
        message = Regex.Replace(message, @"здравствуйте", "шалом ходячий кошелёк");
        message = Regex.Replace(message, @"Здравствуй", "Шалом ходячий кошелёк");
        message = Regex.Replace(message, @"здравствуй", "шалом ходячий кошелёк");

        message = Regex.Replace(message, @" плати ", " отдай шекели ");

        message = Regex.Replace(message, @" вау ", " ой вей ");

        message = Regex.Replace(message, @"Р", "Л");
        message = Regex.Replace(message, @"р", "л");
        message = Regex.Replace(message, @"ы", "и");

        args.Message = message;
    }
};
