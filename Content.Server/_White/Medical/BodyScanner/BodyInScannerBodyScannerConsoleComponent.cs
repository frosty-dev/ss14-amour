using Content.Server.Medical.Components;

namespace Content.Server._White.Medical.BodyScanner
{
    [RegisterComponent]
    public sealed partial class BodyInScannerBodyScannerConsoleComponent : Component
    {
        [ViewVariables]
        public MedicalScannerComponent? MedicalCannerComponent;
    }
}
