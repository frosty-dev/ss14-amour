using System.Linq;
using Content.Client._Amour.HumanoidProfileEditorExt;
using Content.Shared._Amour.Hole;

namespace Content.Client.Preferences.UI;

public sealed partial class HumanoidProfileEditor
{
    private Dictionary<string, Genital> _genitals = new();

    private void InitializeGenitals()
    {

    }

    private void UpdateGenitalsControls()
    {
        if (Profile is null)
            return;

        _genitals.Clear();
        GenitalBoxView.ClearChilds();

        var genitalsList = Profile.Appearance.Genitals.ToList();

        foreach (var prototype in _prototypeManager.EnumeratePrototypes<GenitalsGroupPrototype>())
        {
            var controller = new GenitalController();

            var selected = 0;
            foreach (var genital in genitalsList)
            {
                if (!prototype.Prototypes.Contains(genital.GenitalId))
                    continue;

                selected = prototype.Prototypes.IndexOf(genital.GenitalId) + 1;
                controller.SetColor(genital.Color);
            }

            controller.GenitalsCollectionPrototype = prototype;
            controller.InChange += ControllerOnInChange;
            controller.SelectedId = selected;

            GenitalBoxView.AddChild(controller);
        }
    }

    private void ControllerOnInChange(GenitalChangedEventArgs obj)
    {
        SelectGenital(obj.GenitalId,obj.Group,obj.Color);
    }

    private void SelectGenital(string? id, string group,Color? color)
    {
        if(Profile is null)
            return;

        if (id is null)
        {
            _genitals.Remove(group);
        }
        else
        {
            _genitals[group] = new Genital(id, color);
        }

        Profile = Profile.WithCharacterAppearance(Profile.Appearance.WithGenitals(_genitals.Values.ToList()));
        ShowClothes.Pressed = false;
        IsDirty = true;
    }


}
