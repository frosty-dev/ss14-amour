using System.Collections.Immutable;
using System.Net;
using System.Threading.Tasks;
using Content.Server._White.PandaSocket.Interfaces;
using Content.Server.Database;
using Content.Shared.Database;
using Content.Shared.Roles;
using Robust.Shared.Network;
using Robust.Shared.Player;

namespace Content.Server.Administration.Managers;

public interface IBanManager
{
    public void Initialize();
    public void Restart();

    /// <summary>
    /// Bans the specified target, address range and / or HWID. One of them must be non-null
    /// </summary>
    /// <param name="target">Target user, username or GUID, null for none</param>
    /// <param name="banningAdmin">The person who banned our target</param>
    /// <param name="addressRange">Address range, null for none</param>
    /// <param name="hwid">H</param>
    /// <param name="minutes">Number of minutes to ban for. 0 and null mean permanent</param>
    /// <param name="severity">Severity of the resulting ban note</param>
    /// <param name="reason">Reason for the ban</param>
    public void CreateServerBan(NetUserId? target, string? targetUsername, NetUserId? banningAdmin, (IPAddress, int)? addressRange, ImmutableArray<byte>? hwid, uint? minutes, NoteSeverity severity, string reason, bool isGlobalBan);
    public HashSet<string>? GetRoleBans(NetUserId playerUserId);
    public HashSet<string>? GetJobBans(NetUserId playerUserId);

    public HashSet<ServerBanDef> GetServerBans(NetUserId userId); // Miracle edit
    public void RemoveCachedServerBan(NetUserId userId, int? id); // Miracle edit
    public void AddCachedServerBan(ServerBanDef banDef); // Miracle edit

    /// <summary>
    /// Creates a job ban for the specified target, username or GUID
    /// </summary>
    /// <param name="target">Target user, username or GUID, null for none</param>
    /// <param name="role">Role to be banned from</param>
    /// <param name="severity">Severity of the resulting ban note</param>
    /// <param name="reason">Reason for the ban</param>
    /// <param name="minutes">Number of minutes to ban for. 0 and null mean permanent</param>
    /// <param name="timeOfBan">Time when the ban was applied, used for grouping role bans</param>
    public void CreateRoleBan(NetUserId? target, string? targetUsername, NetUserId? banningAdmin, (IPAddress, int)? addressRange, ImmutableArray<byte>? hwid, string role, uint? minutes, NoteSeverity severity, string reason, DateTimeOffset timeOfBan, bool isGlobalBan);

    /// <summary>
    /// Pardons a role ban for the specified target, username or GUID
    /// </summary>
    /// <param name="banId">The id of the role ban to pardon.</param>
    /// <param name="unbanningAdmin">The admin, if any, that pardoned the role ban.</param>
    /// <param name="unbanTime">The time at which this role ban was pardoned.</param>
    public Task<string> PardonRoleBan(int banId, NetUserId? unbanningAdmin, DateTimeOffset unbanTime);

    /// <summary>
    /// Sends role bans to the target
    /// </summary>
    /// <param name="pSession">Player's user ID</param>
    public void SendRoleBans(NetUserId userId);

    /// <summary>
    /// Sends role bans to the target
    /// </summary>
    /// <param name="pSession">Player's session</param>
    public void SendRoleBans(ICommonSession pSession);

    // WD START
    public void UtkaCreateDepartmentBan(string admin, string target, DepartmentPrototype department,
        string reason, uint minutes, bool isGlobalBan,
        IPandaStatusHandlerContext context);

    public void UtkaCreateJobBan(string admin, string target, string job, string reason, uint minutes, bool isGlobalBan,
        IPandaStatusHandlerContext context);
    // WD END
}
