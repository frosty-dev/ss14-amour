using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Content.Server.GameTicking.Rules;
using Content.Server.Popups;
using Content.Server.Roles;
using Content.Server.Store.Components;
using Content.Server.Store.Systems;
using Content.Server._White.Sponsors;
using Content.Server.Antag;
using Content.Server.Administration.Managers;
using Content.Server.Chat.Managers;
using Content.Server.GameTicking;
using Content.Server.GameTicking.Rules.Components;
using Content.Shared.FixedPoint;
using Content.Shared.GameTicking;
using Content.Shared.Humanoid;
using Content.Shared.Mind;
using Content.Shared.Mind.Components;
using Content.Shared.Roles.Jobs;
using Content.Shared.Verbs;
using Content.Shared._White;
using Content.Shared._White.MeatyOre;
using Content.Shared.Database;
using Newtonsoft.Json.Linq;
using Robust.Server.GameStates;
using Robust.Server.Player;
using Robust.Shared.Configuration;
using Robust.Shared.Map;
using Robust.Shared.Network;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;

namespace Content.Server._White.MeatyOre;

public sealed class MeatyOreStoreSystem : EntitySystem
{
    [Dependency] private readonly IPlayerManager _playerManager = default!;
    [Dependency] private readonly StoreSystem _storeSystem = default!;
    [Dependency] private readonly IEntityManager _entityManager = default!;
    [Dependency] private readonly AntagSelectionSystem _antag = default!;
    [Dependency] private readonly IConfigurationManager _configurationManager = default!;
    [Dependency] private readonly IChatManager _chatManager = default!;
    [Dependency] private readonly SponsorsManager _sponsorsManager = default!;
    [Dependency] private readonly PvsOverrideSystem _pvsOverrideSystem = default!;
    [Dependency] private readonly RoleSystem _roleSystem = default!;
    [Dependency] private readonly SharedJobSystem _jobSystem = default!;
    [Dependency] private readonly PopupSystem _popupSystem = default!;
    [Dependency] private readonly IBanManager _banManager = default!;
    [Dependency] private readonly GameTicker _gameTicker = default!;
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;

    private HttpClient _httpClient = default!;
    private string _apiUrl = default!;

    private const string StorePresetPrototype = "StorePresetMeatyOre";
    private const string MeatyOreCurrencyPrototype = "MeatyOreCoin";

    private bool _meatyOrePanelEnabled;
    private bool _antagGrantEnabled;

    private readonly Dictionary<NetUserId, (EntityUid Entity, StoreComponent Component)> _meatyOreStores = new();

    public override void Initialize()
    {
        base.Initialize();

        _httpClient = new HttpClient();

        _configurationManager.OnValueChanged(WhiteCVars.MeatyOrePanelEnabled, OnPanelEnableChanged, true);
        _configurationManager.OnValueChanged(WhiteCVars.OnlyInOhio, s => _apiUrl = s, true);
        _configurationManager.OnValueChanged(WhiteCVars.EnableGrantAntag, b => _antagGrantEnabled = b, true);

        SubscribeLocalEvent<RoundRestartCleanupEvent>(OnPostRoundCleanup);
        SubscribeNetworkEvent<MeatyOreShopRequestEvent>(OnShopRequested);

        // SubscribeLocalEvent<GetVerbsEvent<Verb>>(MeatyOreVerbs);
    }

    //    private void MeatyOreVerbs(GetVerbsEvent<Verb> ev)
    //    {
    //        if (!_antagGrantEnabled)
    //            return;
    //
    //        if (!EntityManager.TryGetComponent<ActorComponent>(ev.User, out var actorComponent))
    //            return;
    //
    //        if (!_sponsorsManager.TryGetInfo(actorComponent.PlayerSession.UserId, out _))
    //            return;
    //
    //        if (!HasComp<HumanoidAppearanceComponent>(ev.Target))
    //            return;
    //
    //         if (!TryGetStore(actorComponent.PlayerSession, out var store, out var storeEntity))
    //             return;
    //
    //         var verb = new Verb
    //         {
    //             Text = "Выдать роль.",
    //             ConfirmationPopup = true,
    //             Message = $"Цена - {MeatyOreCurrencyPrototype}:10",
    //             Act = () =>
    //             {
    //                 TryAddRole(ev.User, ev.Target, store, storeEntity.Value);
    //             },
    //             Category = VerbCategory.MeatyOre
    //         };
    //
    //        ev.Verbs.Add();
    //    }

    private void OnPanelEnableChanged(bool enabled)
    {
        if (!enabled)
        {
            foreach (var meatyOreStoreData in _meatyOreStores)
            {
                var session = _playerManager.GetSessionById(meatyOreStoreData.Key);

                var playerEntity = session.AttachedEntity;

                if (!playerEntity.HasValue)
                    continue;

                _storeSystem.CloseUi(playerEntity.Value, meatyOreStoreData.Value.Component);
            }
        }

        _meatyOrePanelEnabled = enabled;
    }

    private void OnShopRequested(MeatyOreShopRequestEvent msg, EntitySessionEventArgs args)
    {
        var playerSession = args.SenderSession;

        if (!_meatyOrePanelEnabled)
        {
            _chatManager.DispatchServerMessage(playerSession,
                "Мясная панель отключена на данном сервере! Приятной игры!");

            return;
        }

        var playerEntity = args.SenderSession.AttachedEntity;

        if (!playerEntity.HasValue)
            return;

        if (!HasComp<HumanoidAppearanceComponent>(playerEntity.Value))
            return;

        if (!TryGetStore(playerSession, out var storeComponent, out var storeEntity))
            return;

        _pvsOverrideSystem.AddSessionOverride(storeEntity.Value, playerSession);
        _storeSystem.ToggleUi(playerEntity.Value, storeEntity.Value);
    }

    private bool TryGetStore(ICommonSession session, out StoreComponent store, [NotNullWhen(true)] out EntityUid? storeEntity)
    {
        store = null!;
        storeEntity = null!;

        if (!_sponsorsManager.TryGetInfo(session.UserId, out var sponsorInfo))
            return false;

        if (_meatyOreStores.TryGetValue(session.UserId, out var pair))
        {
            storeEntity = pair.Entity;
            store = pair.Component;
            return true;
        }

        if (sponsorInfo.MeatyOreCoin == 0)
            return false;

        (storeEntity, store) = CreateStore(session.UserId, sponsorInfo.MeatyOreCoin);
        return true;
    }

    private void OnPostRoundCleanup(RoundRestartCleanupEvent ev)
    {
        foreach (var store in _meatyOreStores.Values)
        {
            Del(store.Entity);
        }

        _meatyOreStores.Clear();
    }

    private (EntityUid, StoreComponent) CreateStore(NetUserId userId, int balance)
    {
        var session = _playerManager.GetSessionById(userId);
        var user = session.AttachedEntity!;

        var storeComponent = Comp<StoreComponent>(user.Value);

        _storeSystem.InitializeFromPreset(StorePresetPrototype, user.Value, storeComponent);
        storeComponent.Balance.Clear();

        _storeSystem.TryAddCurrency(new Dictionary<string, FixedPoint2> { { MeatyOreCurrencyPrototype, balance } },
            user.Value, storeComponent);

        _meatyOreStores[userId] = (user.Value, storeComponent);

        return (user.Value, storeComponent);
    }

    private async void TryAddRole(EntityUid user, EntityUid target, StoreComponent store, EntityUid storeEntity)
    {
        if (!EntityManager.TryGetComponent<ActorComponent>(user, out var userActorComponent))
            return;

        if (!EntityManager.TryGetComponent<ActorComponent>(target, out var targetActorComponent))
            return;

        if (!TryComp<MindContainerComponent>(target, out var targetMind) ||
            !TryComp(targetMind.Mind, out MindComponent? mindComponent) || mindComponent.Session == null)
        {
            return;
        }

        if (!store.Balance.TryGetValue(MeatyOreCurrencyPrototype, out var currency))
            return;

        if (currency - 10 < 0)
            return;


        var fake = _roleSystem.MindIsAntagonist(targetMind.Mind.Value)
                   || !_jobSystem.CanBeAntag(mindComponent.Session)
                   // If nukeops declared war
                   || _gameTicker.GetActiveGameRules().Any(x =>
                       TryComp(x, out NukeopsRuleComponent? nukeops) && nukeops.WarDeclaredTime != null);

        var ckey = userActorComponent.PlayerSession.Name;
        var grant = user == target;
        var result = await GrantAntagonist(ckey, !grant);

        if (result)
        {
            _storeSystem.TryAddCurrency(new Dictionary<string, FixedPoint2> { { MeatyOreCurrencyPrototype, -10 } },
                storeEntity, store);

            if (!fake)
            {
                _antag.ForceMakeAntag<TraitorRuleComponent>(mindComponent.Session, "Traitor");

                var msg = $"Игрок с сикеем {ckey} выдал антажку {targetActorComponent.PlayerSession.Name}";
                _chatManager.SendAdminAnnouncement(msg);
            }
            else
            {
                var msg =
                    $"Игрок с сикеем {ckey} попытался выдать антажку {targetActorComponent.PlayerSession.Name}. Но обосрался. Была выдана фейковая антажка.";

                _chatManager.SendAdminAnnouncement(msg);
            }
        }
        else
        {
            var timeMessage = grant
                ? $"Вы сможете выдать себе антага через: {await GetCooldownRemaining(ckey, false)}"
                : $"Вы сможете выдать антага другу через: {await GetCooldownRemaining(ckey, true)}";

            _popupSystem.PopupEntity(timeMessage, user, user);
        }
    }

    //    private async void TryBanDolboeb(ICommonSession session)
    //     {
    //         if(_banManager.GetServerBans(session.UserId).Count > 0)
    //             return;
    //
    //         _banManager.CreateServerBan(session.UserId,
    //             session.Name,
    //             null,
    //             null,
    //             null,
    //             2880,
    //             NoteSeverity.Minor,
    //             "Кусок дерьма, блядина нахуй! У НАС АНТАЖКУ ВЫДАВАТЬ ЗАПРЕЩЕНО НАХУЙ!!!! ЧТОБ ТЯ ВЫЕБАЛИ СТО НЕГРОВ НАХУЙ!",
    //             false);
    //     }

    private async Task<bool> GrantAntagonist(string ckey, bool isFriend)
    {
        var result = false;

        try
        {
            var url = $"{_apiUrl}/api/Antagonist/grantUser";

            if (isFriend)
            {
                url = $"{_apiUrl}/api/Antagonist/grantFriend";
            }

            var requestData = new { UserId = ckey };

            var response = await _httpClient.PostAsJsonAsync(url, requestData);

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                result = bool.Parse(responseContent);
            }
            else
            {
                response.EnsureSuccessStatusCode();
            }
        }
        catch (Exception)
        {
            // ignored
        }

        return result;
    }

    private async Task<TimeSpan> GetCooldownRemaining(string ckey, bool isFriend)
    {
        try
        {
            var url = $"{_apiUrl}/api/Antagonist/cooldownUser?userId={ckey}";

            if (isFriend)
            {
                url = $"{_apiUrl}/api/Antagonist/cooldownFriend?userId={ckey}";
            }

            var response = await _httpClient.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                var responseData = await response.Content.ReadAsStringAsync();
                if (!string.IsNullOrEmpty(responseData))
                {
                    var jsonObject = JObject.Parse(responseData);
                    if (jsonObject.TryGetValue("remainingTime", out var remainingTimeToken) &&
                        TimeSpan.TryParse(remainingTimeToken.ToString(), out var remainingTime))
                    {
                        var time = new TimeSpan(remainingTime.Hours, remainingTime.Minutes, 0);
                        return time;
                    }
                }
            }
            else
            {
                response.EnsureSuccessStatusCode();
            }
        }
        catch (Exception)
        {
            // ignored
        }

        return TimeSpan.Zero;
    }
}
