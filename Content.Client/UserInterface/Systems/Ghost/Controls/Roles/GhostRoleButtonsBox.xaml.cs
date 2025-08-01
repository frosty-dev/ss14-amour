using System.Numerics;
using Content.Shared.Ghost.Roles;
using Robust.Client.AutoGenerated;
using Robust.Client.GameObjects;
using Robust.Client.UserInterface.Controls;
using Robust.Client.UserInterface.CustomControls;
using Robust.Client.UserInterface.XAML;
using Robust.Shared.Utility;

namespace Content.Client.UserInterface.Systems.Ghost.Controls.Roles
{
    [GenerateTypedNameReferences]
    public sealed partial class GhostRoleButtonsBox : BoxContainer // WD Edit ahead of wizden upstream
    {
        private SpriteSystem _spriteSystem;
        public event Action<GhostRoleInfo>? OnRoleSelected;
        public event Action<GhostRoleInfo>? OnRoleFollow;

        public GhostRoleButtonsBox(bool hasAccess, FormattedMessage? reason, IEnumerable<GhostRoleInfo> roles, SpriteSystem spriteSystem) // WD Edit ahead of wizden upstream
        {
            RobustXamlLoader.Load(this);
            _spriteSystem = spriteSystem;

            foreach (var role in roles)
            {
                var button = new GhostRoleEntryButtons(role);
                button.RequestButton.OnPressed += _ => OnRoleSelected?.Invoke(role);
                button.FollowButton.OnPressed += _ => OnRoleFollow?.Invoke(role);

                if (!hasAccess)
                {
                    button.RequestButton.Disabled = true;

                    if (reason != null && !reason.IsEmpty)
                    {
                        var tooltip = new Tooltip();
                        tooltip.SetMessage(reason);
                        button.RequestButton.TooltipSupplier = _ => tooltip;
                    }

                    button.RequestButton.AddChild(new TextureRect
                    {
                        TextureScale = new Vector2(0.4f, 0.4f),
                        Stretch = TextureRect.StretchMode.KeepCentered,
                        Texture = _spriteSystem.Frame0(new SpriteSpecifier.Texture(new("/Textures/Interface/Nano/lock.svg.192dpi.png"))),
                        HorizontalExpand = true,
                        HorizontalAlignment = HAlignment.Right,
                    });
                }

                Buttons.AddChild(button);
            }
        }
    }
}
