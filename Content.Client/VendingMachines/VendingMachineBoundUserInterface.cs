using Content.Shared.VendingMachines;
using System.Linq;
using Content.Client._White.Economy.Ui;

namespace Content.Client.VendingMachines
{
    public sealed class VendingMachineBoundUserInterface : BoundUserInterface
    {
        [ViewVariables]
        private VendingMenu? _menu; // WD EDIT

        [ViewVariables]
        private List<VendingMachineInventoryEntry> _cachedInventory = new();

        [ViewVariables]
        private List<int> _cachedFilteredIndex = new();

        private VendingMachineComponent component = new();//WD edit
        public VendingMachineBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
        {
        }
       
        protected override void Open()
        {
            base.Open();

            var vendingMachineSys = EntMan.System<VendingMachineSystem>();

            // WD EDIT START
            component = EntMan.GetComponent<VendingMachineComponent>(Owner);
            _cachedInventory = vendingMachineSys.GetAllInventory(Owner, component);

            _menu = new VendingMenu { Title = EntMan.GetComponent<MetaDataComponent>(Owner).EntityName };

            _menu.OnClose += Close;
            _menu.OnItemSelected += OnItemSelected;
            _menu.OnWithdraw += SendMessage;
            _menu.SearchBar.OnTextChanged += UpdateFilter;
            // WD EDIT END

            _menu.Populate(_cachedInventory, component.PriceMultiplier, component.Credits);

            _menu.OpenCenteredLeft();
        }

        // WD EDIT START
        private void UpdateFilter(Robust.Client.UserInterface.Controls.LineEdit.LineEditEventArgs obj)
        {
            if (_menu != null)
            {
                _menu.filter = obj.Text;
                _menu.Populate(_cachedInventory, component.PriceMultiplier, component.Credits);
            }
        }
        // WD EDIT END








        protected override void UpdateState(BoundUserInterfaceState state)
        {
            base.UpdateState(state);

            if (state is not VendingMachineInterfaceState newState)
                return;

            _cachedInventory = newState.Inventory;

            _menu?.Populate(_cachedInventory, newState.PriceMultiplier, newState.Credits);
        }

        // WD EDIT START
        private void OnItemSelected(int index)
        {
            if (_cachedInventory.Count == 0)
                return;

            var selectedItem = _cachedInventory.ElementAtOrDefault(index);

            if (selectedItem == null)
                return;

            SendMessage(new VendingMachineEjectMessage(selectedItem.Type, selectedItem.ID));
        }
        // WD EDIT END

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (!disposing)
                return;

            if (_menu == null)
                return;

            _menu.OnItemSelected -= OnItemSelected;
            _menu.OnClose -= Close;
            _menu.Dispose();
        }

        private void OnSearchChanged(string? filter)
        {
            //_menu?.filter = (filter ?? "");
        }
    }
}
