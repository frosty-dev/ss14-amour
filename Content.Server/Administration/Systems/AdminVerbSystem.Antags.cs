using Content.Server.Administration.Commands;
using Content.Server.Antag;
using Content.Server.GameTicking.Rules.Components;
using Content.Server.Zombies;
using Content.Shared.Administration;
using Content.Shared.Database;
using Content.Shared.Mind.Components;
using Content.Shared.Roles;
using Content.Shared.Verbs;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;
using Content.Server._White.Cult.GameRule;
using Content.Server._White.Wizard;
using Content.Server.Changeling;
using Content.Server.GameTicking.Rules;

namespace Content.Server.Administration.Systems;

public sealed partial class AdminVerbSystem
{
    [Dependency] private readonly AntagSelectionSystem _antag = default!;
    [Dependency] private readonly ZombieSystem _zombie = default!;

    [ValidatePrototypeId<EntityPrototype>]
    private const string DefaultTraitorRule = "Traitor";

    [ValidatePrototypeId<EntityPrototype>]
    private const string DefaultNukeOpRule = "Nukeops";

    [ValidatePrototypeId<EntityPrototype>]
    private const string DefaultRevsRule = "Revolutionary";

    [ValidatePrototypeId<EntityPrototype>]
    private const string DefaultThiefRule = "Thief";

    // WD edit start
    [ValidatePrototypeId<EntityPrototype>]
    private const string DefaultCultRule = "Cult";

    [ValidatePrototypeId<EntityPrototype>]
    private const string DefaultChangelingRule = "Changeling";

    [ValidatePrototypeId<EntityPrototype>]
    private const string DefaultWizardRule = "Wizard";
    // WD edit end

    [ValidatePrototypeId<StartingGearPrototype>]
    private const string PirateGearId = "PirateGear";

    // All antag verbs have names so invokeverb works.
    private void AddAntagVerbs(GetVerbsEvent<Verb> args)
    {
        if (!TryComp<ActorComponent>(args.User, out var actor))
            return;

        var player = actor.PlayerSession;

        if (!_adminManager.HasAdminFlag(player, AdminFlags.Fun))
            return;

        if (!HasComp<MindContainerComponent>(args.Target))
            return;

        // WD edit start - fix admin verbs
        if (!TryComp<ActorComponent>(args.Target, out var tActorComponent))
            return;

        var target = tActorComponent.PlayerSession;
        // WD edit end - fix admin verbs

        Verb traitor = new()
        {
            Text = Loc.GetString("admin-verb-text-make-traitor"),
            Category = VerbCategory.Antag,
            Icon = new SpriteSpecifier.Rsi(new ResPath("/Textures/Structures/Wallmounts/posters.rsi"), "poster5_contraband"),
            Act = () =>
            {
                _antag.ForceMakeAntag<TraitorRuleComponent>(target, DefaultTraitorRule);
            },
            Impact = LogImpact.High,
            Message = Loc.GetString("admin-verb-make-traitor"),
        };
        args.Verbs.Add(traitor);

        Verb zombie = new()
        {
            Text = Loc.GetString("admin-verb-text-make-zombie"),
            Category = VerbCategory.Antag,
            Icon = new SpriteSpecifier.Texture(new("/Textures/Interface/Actions/zombie-turn.png")),
            Act = () =>
            {
                _zombie.ZombifyEntity(args.Target);
            },
            Impact = LogImpact.High,
            Message = Loc.GetString("admin-verb-make-zombie"),
        };
        args.Verbs.Add(zombie);


        Verb nukeOp = new()
        {
            Text = Loc.GetString("admin-verb-text-make-nuclear-operative"),
            Category = VerbCategory.Antag,
            Icon = new SpriteSpecifier.Rsi(new("/Textures/Structures/Wallmounts/signs.rsi"), "radiation"),
            Act = () =>
            {
                _antag.ForceMakeAntag<NukeopsRuleComponent>(target, DefaultNukeOpRule);
            },
            Impact = LogImpact.High,
            Message = Loc.GetString("admin-verb-make-nuclear-operative"),
        };
        args.Verbs.Add(nukeOp);

        Verb pirate = new()
        {
            Text = Loc.GetString("admin-verb-text-make-pirate"),
            Category = VerbCategory.Antag,
            Icon = new SpriteSpecifier.Rsi(new("/Textures/Clothing/Head/Hats/pirate.rsi"), "icon"),
            Act = () =>
            {
                // pirates just get an outfit because they don't really have logic associated with them
                SetOutfitCommand.SetOutfit(args.Target, PirateGearId, EntityManager);
            },
            Impact = LogImpact.High,
            Message = Loc.GetString("admin-verb-make-pirate"),
        };
        args.Verbs.Add(pirate);

        Verb headRev = new()
        {
            Text = Loc.GetString("admin-verb-text-make-head-rev"),
            Category = VerbCategory.Antag,
            Icon = new SpriteSpecifier.Rsi(new("/Textures/Interface/Misc/job_icons.rsi"), "HeadRevolutionary"),
            Act = () =>
            {
                _antag.ForceMakeAntag<RevolutionaryRuleComponent>(target, DefaultRevsRule);
            },
            Impact = LogImpact.High,
            Message = Loc.GetString("admin-verb-make-head-rev"),
        };
        args.Verbs.Add(headRev);

        Verb thief = new()
        {
            Text = Loc.GetString("admin-verb-text-make-thief"),
            Category = VerbCategory.Antag,
            Icon = new SpriteSpecifier.Rsi(new ResPath("/Textures/Clothing/Hands/Gloves/Color/black.rsi"), "icon"),
            Act = () =>
            {
                _antag.ForceMakeAntag<ThiefRuleComponent>(target, DefaultThiefRule);
            },
            Impact = LogImpact.High,
            Message = Loc.GetString("admin-verb-make-thief"),
        };
        args.Verbs.Add(thief);

        // WD edit start
        Verb cultist = new()
        {
            Text = "Сделать культистом",
            Category = VerbCategory.Antag,
            Icon = new SpriteSpecifier.Rsi(new("/Textures/White/Cult/interface.rsi"), "icon"),
            Act = () =>
            {
                _antag.ForceMakeAntag<CultRuleComponent>(target, DefaultCultRule);
            },
            Impact = LogImpact.High,
            Message = Loc.GetString("Сделать культистом"),
        };
        args.Verbs.Add(cultist);

        Verb changeling = new()
        {
            Text = Loc.GetString("admin-verb-text-make-changeling"),
            Category = VerbCategory.Antag,
            Icon = new SpriteSpecifier.Rsi(new("/Textures/White/Actions/changeling.rsi"), "absorb"),
            Act = () =>
            {
                _antag.ForceMakeAntag<ChangelingRuleComponent>(target, DefaultChangelingRule);
            },
            Impact = LogImpact.High,
            Message = Loc.GetString("admin-verb-make-changeling"),
        };
        args.Verbs.Add(changeling);

        Verb wizard = new()
        {
            Text = Loc.GetString("admin-verb-text-make-wizard"),
            Category = VerbCategory.Antag,
            Icon = new SpriteSpecifier.Rsi(new("/Textures/Clothing/Head/Hats/wizardhat.rsi"), "icon"),
            Act = () =>
            {
                _antag.ForceMakeAntag<WizardRuleComponent>(target, DefaultWizardRule);
            },
            Impact = LogImpact.High,
            Message = Loc.GetString("admin-verb-make-wizard"),
        };
        args.Verbs.Add(wizard);

        // WD edit end
    }
}
