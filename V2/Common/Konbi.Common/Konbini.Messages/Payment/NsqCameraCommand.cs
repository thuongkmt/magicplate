using System;

namespace KonbiBrain.Common.Messages.Camera
{
    //TODO: TrungPQ: this class need to be derived from BaseCommand it support IsTimeOut() by default every message has age over than 10s is considered as deprecated should not be proccessed.
    public class NsqCameraRequestCommand:BaseCommand
    {
        public NsqCameraRequestCommand()
        {
            Command = UniversalCommandConstants.CameraRequest;
        }
        public string Command { get; set; }
        public bool IsPaymentStart { get; set; }
        public string TransactionId { get; set; }
    }

    public class NsqCameraResponseCommand: BaseCommand
    {
        public NsqCameraResponseCommand()
        {
            Command = UniversalCommandConstants.CameraResponse;
        }
        public string Command { get; set; }
        public string TransactionId { get; set; }
        public string BeginImage { get; set; }
        public string EndImage { get; set; }
    }
}
