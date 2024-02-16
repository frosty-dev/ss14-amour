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

        Logger.Debug(Profile.Appearance.Genitals.Count + "<<");

        foreach (var prototype in _prototypeManager.EnumeratePrototypes<GenitalsGroupPrototype>())
        {
            var selected = 0;
            foreach (var genital in Profile.Appearance.Genitals.Where(genital => prototype.Prototypes.Contains(genital.GenitalId)))
            {
                selected = prototype.Prototypes.IndexOf(genital.GenitalId);
            }

            var controller = new GenitalController();
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
