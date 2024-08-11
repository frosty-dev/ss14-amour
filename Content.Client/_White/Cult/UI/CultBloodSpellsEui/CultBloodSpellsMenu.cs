using System.Numerics;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;
using Robust.Client.UserInterface.CustomControls;
using static Robust.Client.UserInterface.Controls.BoxContainer;

namespace Content.Client._White.Cult.UI.CultBloodSpellsEui;

public sealed class CultBloodSpellsMenu : DefaultWindow
{
    public readonly Button RemoveButton;
    public readonly Button CreateButton;

    public CultBloodSpellsMenu()
    {
        Title = Loc.GetString("blood-spells-title");

        Contents.AddChild(new BoxContainer
        {
            Orientation = LayoutOrientation.Vertical,
            Children =
            {
                new BoxContainer
                {
                    Orientation = LayoutOrientation.Vertical,
                    Children =
                    {
                        (new Label()
                        {
                            Text = Loc.GetString("blood-spells-text")
                        }),
                        new BoxContainer
                        {
                            Orientation = LayoutOrientation.Horizontal,
                            Align = AlignMode.Center,
                            Children =
                            {
                                (CreateButton = new Button
                                {
                                    Text = Loc.GetString("blood-spells-create-button"),
                                }),

                                (new Control()
                                {
                                    MinSize = new Vector2(20, 0)
                                }),

                                (RemoveButton = new Button
                                {
                                    Text = Loc.GetString("blood-spells-remove-button"),
                                })
                            }
                        },
                    }
                },
            }
        });
    }
}

