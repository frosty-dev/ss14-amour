using System.Linq;
using Content.Server.Station.Components;
using Content.Server.Station.Systems;
using Content.Server.Xenoarchaeology.XenoArtifacts;
using Content.Shared._White;
using Content.Shared.Damage;
using Content.Shared.Stacks;
using Content.Shared.Item;
using Robust.Shared.Configuration;
using Robust.Shared.Random;
using Robust.Server.GameObjects;

namespace Content.Server._White.RandomArtifacts;

public sealed class RandomArtifactsSystem : EntitySystem
{
    [Dependency] private readonly ArtifactSystem _artifactsSystem = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly IConfigurationManager _configurationManager = default!;
    [Dependency] private readonly StationSystem _station = default!;

    private float _itemToArtifactRatio; // from 0 to 100. In % percents. Default is 0.4%
    private bool _artifactsEnabled;

    public override void Initialize()
    {
        base.Initialize();

        _configurationManager.OnValueChanged(WhiteCVars.EnableRandomArtifacts, b => OnCvarChanged(b), true);
        _configurationManager.OnValueChanged(WhiteCVars.ItemToArtifactRatio, r => _itemToArtifactRatio = r, true);

        SubscribeLocalEvent<MainMapInitEvent>(OnRoundStart);
    }

    private void OnRoundStart(MainMapInitEvent ev)
    {
        if (!_artifactsEnabled)
            return;

        // No we don't
        // // Removing old artifact-items and replace it with new funny stealthy items
        // foreach (var oldArtifact in EntityQuery<ArtifactComponent>())
        // {
        //     QueueDel(oldArtifact.Owner);
        // }

        var items = EntityQuery<ItemComponent>().ToList();
        _random.Shuffle(items);

        var selectedItems = GetPercentageOfHashSet(items, _itemToArtifactRatio);

        foreach (var item in selectedItems)
        {
            var entity = item.Owner;
            var xform = Transform(entity);

            var station = _station.GetStationInMap(xform.MapID);

            if (!HasComp<StationDataComponent>(station))
                continue;

            if (HasComp<StackComponent>(entity))
                continue;

            if (HasComp<PointLightComponent>(entity))
                continue;

            var artifactComponent = EnsureComp<ArtifactComponent>(entity);
            _artifactsSystem.SafeRandomizeArtifact(entity, ref artifactComponent);

            EnsureComp<DamageableComponent>(entity);
        }
    }

    private HashSet<ItemComponent> GetPercentageOfHashSet(List<ItemComponent> sourceList, float percentage)
    {
        var countToAdd = (int) Math.Round((double) sourceList.Count * percentage / 100);

        return sourceList.Where(x => !Transform(x.Owner).Anchored).Take(countToAdd).ToHashSet();
    }

    private void OnCvarChanged(bool enabled)
    {
        if (_artifactsEnabled != enabled && !enabled)
        {
            var items = EntityQuery<ItemComponent, ArtifactComponent>();

            foreach (var (_, artifact) in items)
            {
                RemComp<ArtifactComponent>(artifact.Owner);
            }
        }

        _artifactsEnabled = enabled;
    }

    public sealed class MainMapInitEvent : EntityEventArgs { }

}

/*
    Number of items on maps
    DEV - 1527
    WhiteBox - 13692
    WonderBox - 15306
*/
