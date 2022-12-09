using Robust.Client.AutoGenerated;
using Robust.Client.UserInterface.CustomControls;
using Robust.Client.UserInterface.XAML;
using Content.Client.Message;
using Robust.Shared.Timing;
using Content.Shared.Cloning.CloningConsole;

namespace Content.Client.CloningConsole.UI
{
    [GenerateTypedNameReferences]
    public partial class CloningConsoleWindow : DefaultWindow
    {
        public CloningConsoleWindow()
        {
            IoCManager.InjectDependencies(this);
            RobustXamlLoader.Load(this);
        }

        private CloningConsoleBoundUserInterfaceState? _lastUpdate;

        protected override void FrameUpdate(FrameEventArgs args)
        {
            base.FrameUpdate(args);
        }

        public void Populate(CloningConsoleBoundUserInterfaceState state)
        {
            _lastUpdate = state;
            // BUILD SCANNER UI
            if (state.ScannerConnected)
            {
                if (!state.ScannerInRange)
                {
                    GeneticScannerFar.Visible = true;
                    GeneticScannerContents.Visible = false;
                    GeneticScannerMissing.Visible = false;
                    return;
                }

                GeneticScannerContents.Visible = true;
                GeneticScannerFar.Visible = false;
                GeneticScannerMissing.Visible = false;
                CloneButton.Disabled = state.CloningStatus != ClonerStatus.Ready &&
                                       state.CloningStatus != ClonerStatus.ScannerOccupantAlive;

                switch (state.CloningStatus)
                    {
                        case ClonerStatus.NoClonerDetected:
                            CloningActivity.Text = (Loc.GetString("cloning-console-component-msg-no-cloner"));
                            break;
                        case ClonerStatus.Ready:
                            CloningActivity.Text = (Loc.GetString("cloning-console-component-msg-ready"));
                            break;
                        case ClonerStatus.ClonerOccupied:
                            CloningActivity.Text = (Loc.GetString("cloning-console-component-msg-occupied"));
                            break;
                        case ClonerStatus.ScannerEmpty:
                            CloningActivity.Text = (Loc.GetString("cloning-console-component-msg-empty"));
                            break;
                        case ClonerStatus.ScannerOccupantAlive:
                            CloningActivity.Text = (Loc.GetString("cloning-console-component-msg-scanner-occupant-alive"));
                            break;
                        case ClonerStatus.OccupantMetaphyiscal:
                            CloningActivity.Text = (Loc.GetString("cloning-console-component-msg-already-alive"));
                            break;
                        case ClonerStatus.NoMindDetected:
                            CloningActivity.Text = (Loc.GetString("cloning-console-component-msg-no-mind"));
                            break;
                    }
                // Set label depending on if scanner is occupied or not.
                ScannerInfoLabel.SetMarkup(state.ScannerBodyInfo != null ?
                    Loc.GetString("cloning-console-window-scanner-id", ("scannerOccupantName", state.ScannerBodyInfo)) :
                    Loc.GetString("cloning-console-window-id-blank"));
            }
            else
            {
                // Scanner is missing, set error message visible
                GeneticScannerContents.Visible = false;
                GeneticScannerFar.Visible = false;
                GeneticScannerMissing.Visible = true;
            }

            // BUILD ClONER UI
            if (state.ClonerConnected)
            {
                if (!state.ClonerInRange)
                {
                    CloningPodFar.Visible = true;
                    CloningPodContents.Visible = false;
                    CloningPodMissing.Visible = false;
                    return;
                }

                CloningPodContents.Visible = true;
                CloningPodFar.Visible = false;
                CloningPodMissing.Visible = false;

                ClonerBrainActivity.SetMarkup(Loc.GetString(state.MindPresent ? "cloning-console-mind-present-text" : "cloning-console-no-mind-activity-text"));
                // Set label depending if clonepod is occupied or not
                ClonerInfoLabel.SetMarkup(state.ClonerBodyInfo != null ?
                    Loc.GetString("cloning-console-window-pod-id", ("podOccupantName", state.ClonerBodyInfo)) :
                    Loc.GetString("cloning-console-window-id-blank"));
            }
            else
            {
                // Clone pod is missing, set error message visible
                CloningPodContents.Visible = false;
                CloningPodFar.Visible = false;
                CloningPodMissing.Visible = true;
            }
        }
    }
}
