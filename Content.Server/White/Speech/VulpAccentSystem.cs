using System.Text.RegularExpressions;
using Content.Server.Speech;
using Content.Shared.White.Speech;

namespace Content.Server.White.Speech;

public sealed class VulpAccentSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<VulpAccentComponent, AccentGetEvent>(OnAccent);
    }

    private void OnAccent(EntityUid uid, VulpAccentComponent component, AccentGetEvent args)
    {
        var message = args.Message;

        // herrr
        message = Regex.Replace(message, "r+", "rrr");
        // heRRR
        message = Regex.Replace(message, "R+", "RRR");

        // ррработай
        message = Regex.Replace(message, "р+", "ррр");
        // РРРаботай
        message = Regex.Replace(message, "Р+", "РРР");

        args.Message = message;
    }
}
