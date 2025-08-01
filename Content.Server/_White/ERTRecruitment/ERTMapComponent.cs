using Robust.Shared.Map;
using Robust.Shared.Utility;

namespace Content.Server._White.ERTRecruitment;

[RegisterComponent]
public sealed partial class ERTMapComponent : Component
{
    [ViewVariables]
    public MapId? MapId;
    [ViewVariables]
    public EntityUid? Shuttle;

    public static ResPath OutpostMap = new("/Maps/White/ERTStation.yml");
}
