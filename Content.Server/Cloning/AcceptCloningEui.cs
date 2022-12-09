using Content.Server.EUI;
using Content.Shared.Cloning;
using Content.Shared.Eui;

namespace Content.Server.Cloning
{
    public sealed class AcceptCloningEui : BaseEui
    {
        private readonly CloningSystem _cloningSystem;
        private readonly Mind.Mind _mind;
        private readonly bool _occupantAlive;
        private readonly EntityUid _beforeBody;
        private readonly EntityUid _afterBody;

        public AcceptCloningEui(Mind.Mind mind, CloningSystem cloningSys, bool occupantAlive, EntityUid beforeBody, EntityUid afterBody)
        {
            _mind = mind;
            _cloningSystem = cloningSys;
            _occupantAlive = occupantAlive;
            _beforeBody = beforeBody;
            _afterBody = afterBody;
        }

        public override void HandleMessage(EuiMessageBase msg)
        {
            base.HandleMessage(msg);

            if (msg is not AcceptCloningChoiceMessage choice ||
                choice.Button == AcceptCloningUiButton.Deny)
            {
                Close();
                _cloningSystem.FreeForGhosts(_afterBody);
                return;
            }

            if (_occupantAlive)
            {
                _cloningSystem.FreeForGhosts(_beforeBody);
            }
            _cloningSystem.TransferMindToClone(_mind);
            Close();
        }
    }
}
