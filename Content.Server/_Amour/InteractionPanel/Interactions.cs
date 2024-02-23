using Content.Shared._Amour.InteractionPanel;

namespace Content.Server._Amour.InteractionPanel;

public sealed class Interactions : EntitySystem
{
    public override void Initialize()
    {
        SubscribeLocalEvent<InteractionPanelComponent,InteractionBeginningEvent>(OnBegin);
        SubscribeLocalEvent<InteractionPanelComponent,InteractionEndingEvent>(OnEnd);
    }

    private void OnEnd(EntityUid uid, InteractionPanelComponent component, InteractionEndingEvent args)
    {
        if(args.Handled)
            return;

        Logger.Debug(args.Id + " END");

        switch (args.Id)
        {

        }
    }

    private void OnBegin(EntityUid uid, InteractionPanelComponent component, InteractionBeginningEvent args)
    {
        if(args.Handled)
            return;

        Logger.Debug(args.Id + " START");

        switch (args.Id)
        {

        }
    }
}
