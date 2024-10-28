using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Content.Shared._Miracle.Nya;
using Robust.Client.Player;
using Robust.Client.UserInterface;
using Robust.Shared.Configuration;
using Robust.Shared.Reflection;

namespace Content.Client._Miracle.Nya;

public sealed class NyaCheckClientSystem : EntitySystem
{
    [Dependency] private readonly IReflectionManager _reflection = default!;
    [Dependency] private readonly IConfigurationManager _configuration = default!;
    [Dependency] private readonly IEntitySystemManager _esm = default!;
    [Dependency] private readonly IPlayerManager _player = default!;
    [Dependency] private readonly IUserInterfaceManager _ui = default!;

    [ViewVariables(VVAccess.ReadOnly)]
    private readonly string[] _allowed =
    [
        "Content.Client",
        "Content.Shared",
        "Content.Server",
        "Content.Shared.Database",
        "Robust.Client",
        "Robust.Shared",
        "Robust.Server"
    ];

    public override void Initialize()
    {
        base.Initialize();
        SubscribeNetworkEvent<CheatCheckRequestEvent>(OnCheckRequest);
    }

    private void OnCheckRequest(CheatCheckRequestEvent ev)
    {
        var response = RunChecks();
        RaiseNetworkEvent(response);
    }

    private CheatCheckResponseEvent RunChecks()
    {
        return new CheatCheckResponseEvent
        {
            HasPatchMetadata = FoundPatchMetadataTypes(),
            ReflectionOffender = FoundExtraTypesIReflection(out var reflectionOffender) ? reflectionOffender : null,
            HasMoonyware = FoundMoonywareModuleReflection(),
            IoCOffender = TypesNotFromContentIoC(out var iocOffender) ? iocOffender : null,
            ExtraModuleOffender = CheckExtraModule(out var moduleOffender) ? moduleOffender : null,
            CvarOffender = CheckCommonCheatCvars(out var cvarOffender) ? cvarOffender : null,
            SystemOffender = FoundTypesEntitySystemManager(out var systemOffender) ? systemOffender : null,
            ComponentOffender = CheckComponents(out var componentOffender) ? componentOffender : null,
            WindowOffender = CheckExtraWindows(out var windowOffender) ? windowOffender : null
        };
    }

    private bool FoundPatchMetadataTypes()
    {
        var found = Type.GetType("MarseyPatch") ?? Type.GetType("SubverterPatch");
        return found is not null;
    }

    private bool FoundExtraTypesIReflection([NotNullWhen(true)] out string? offender)
    {
        offender = null;
        string[] typenames = ["SubverterPatch", "MarseyPatch", "MarseyEntry", "Sedition", "Ware"];

        var types = _reflection.FindAllTypes();

        foreach (var type in types)
        {
            foreach (var name in typenames)
            {
                if (!type.Name.Contains(name))
                    continue;

                offender = type.Name;
                return true;
            }
        }

        return false;
    }

    private bool FoundMoonywareModuleReflection()
    {
        var modules = _reflection.Assemblies;

        foreach (var asm in modules)
        {
            if (asm.FullName!.Contains("Moonyware"))
                return true;
        }

        return false;
    }

    private bool FoundTypesEntitySystemManager([NotNullWhen(true)] out string? offend)
    {
        offend = null;

        var types = _esm.GetEntitySystemTypes();

        foreach (var type in types)
        {
            if (!NotFromGameModule(type))
                continue;

            offend = type.FullName!;
            return true;
        }

        return false;
    }

    private bool TypesNotFromContentIoC([NotNullWhen(true)] out string? offend)
    {
        offend = null;

        var types = IoCManager.Instance!.GetRegisteredTypes();

        foreach (var type in types)
        {
            if (!NotFromGameModule(type))
                continue;

            offend = type.FullName!;
            return true;
        }

        return false;
    }

    private bool CheckExtraModule([NotNullWhen(true)] out string? offend)
    {
        offend = null;

        var modules = _reflection.Assemblies;

        foreach (var module in modules)
        {
            var allowed = false;

            foreach (var allow in _allowed)
            {
                if (module.FullName!.Contains(allow))
                {
                    allowed = true;
                    break;
                }
            }

            if (allowed)
                continue;

            offend = module.FullName!;
            return true;
        }

        return false;
    }

    private bool CheckCommonCheatCvars([NotNullWhen(true)] out string? offend)
    {
        string[] keywords =
        [
            "aimbot",
            "visuals",
            "esp",
            "noslip",
            "exploit",
            "fun"
        ];

        offend = null;

        var cvars = _configuration.GetRegisteredCVars();

        foreach (var cvar in cvars)
        {
            if (!keywords.Any(kw => cvar.Contains(kw, StringComparison.CurrentCultureIgnoreCase)))
                continue;

            offend = cvar;
            return true;
        }

        return false;
    }

    private bool CheckComponents([NotNullWhen(true)] out string? offend)
    {
        offend = null;

        if (_player.LocalEntity is null)
            return false;

        var comps = AllComps(_player.LocalEntity.Value);

        foreach (var comp in comps)
        {
            var type = comp.GetType();

            if (!NotFromGameModule(type))
                continue;

            offend = type.FullName!;
            return true;
        }

        return false;
    }

    private bool CheckExtraWindows([NotNullWhen(true)] out string? offend)
    {
        offend = null;

        var children = _ui.WindowRoot.Children;

        foreach (var child in children)
        {
            var type = child.GetType();

            if (!NotFromGameModule(type))
                continue;

            offend = type.FullName!;
            return true;
        }

        return false;
    }

    private bool NotFromGameModule(Type type)
    {
        var name = type.FullName;

        foreach (var allow in _allowed)
        {
            if (name!.Contains(allow))
                return false;
        }

        return true;
    }
}
