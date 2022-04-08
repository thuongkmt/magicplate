namespace Konbi.Camera
{
    using KonbiBrain.Common.Messages;
    using KonbiBrain.Common.Messages.Camera;
    using KonbiBrain.Common.Messages.Payment;
    using KonbiBrain.Common.Services;
    using KonbiBrain.Messages;
    using Konbini.Common.Messages;
    using Konbini.Messages.Commands;
    using Konbini.Messages.Enums;
    using Newtonsoft.Json;
    using NsqSharp;
    using System;
    using System.Diagnostics;
    using System.Drawing.Imaging;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Windows.Forms;

    public partial class FormMain : Form, INsqHandler
    {
        private readonly NsqMessageProducerService nsqProducerService;
        private readonly NsqMessageConsumerService nsqConsumerService;
        private readonly LogService logService;
        private readonly string SERVICE_NAME = "Camera";
        private readonly string SERVICE_TYPE = "Camera.Service";
        private const string tempImgFolder = "C:\\TempImgFolder";
        private const string jpgExtenstion = ".jpg";
        private string beginImage;
        private string endImage;

        public FormMain()
        {
            InitializeComponent();
            nsqProducerService = new NsqMessageProducerService();
            nsqConsumerService = new NsqMessageConsumerService(NsqTopics.CAMERA_REQUEST_TOPIC, this);
            logService = new LogService();
            if(!Directory.Exists(tempImgFolder))
            {
                Directory.CreateDirectory(tempImgFolder);
            }
        }

        /// <summary>
        /// The helper class for a combo box item.
        /// </summary>
        

        private void Form1_Load(object sender, EventArgs e)
        {
            foreach (WebCameraId camera in webCameraControl1.GetVideoCaptureDevices())
            {
                cbCamera.Items.Add(new ComboBoxItem (camera));
            }

            if (cbCamera.Items.Count > 0)
            {
                cbCamera.SelectedItem = cbCamera.Items[0];
                // TrungPQ commment for check camera CPU.
                //StartCapture();
            }
            cbCompress.Items.Add(new CompressItem { Name = "10%", Value = 10L });
            cbCompress.Items.Add(new CompressItem { Name = "20%", Value = 20L });
            cbCompress.Items.Add(new CompressItem { Name = "30%", Value = 30L });
            cbCompress.Items.Add(new CompressItem { Name = "40%", Value = 40L });
            cbCompress.Items.Add(new CompressItem { Name = "50%", Value = 50L });
            cbCompress.Items.Add(new CompressItem { Name = "60%", Value = 60L });
            cbCompress.Items.Add(new CompressItem { Name = "70%", Value = 70L });
            cbCompress.Items.Add(new CompressItem { Name = "80%", Value = 80L });
            cbCompress.Items.Add(new CompressItem { Name = "90%", Value = 90L });

            cbCompress.SelectedItem = cbCompress.Items[2];
            if (cbCamera.Items.Count > 0)
            {
                PublishDeviceInfo(false);
            }
            else
            {
                PublishDeviceInfo(true,"No camera attached to computer");
            }

            // Start camera in standby
            ComboBoxItem i = (ComboBoxItem)cbCamera.SelectedItem;
            //TODO: handle exceptions 
            webCameraControl1.StartCapture(i.Id);
        }

        public void PublishDeviceInfo(bool hasError, string errorMessage = "")
        {
            //publish service informative description
            var deviceInfoCmd = new DeviceInfoCommand();
            deviceInfoCmd.CommandObject.Name = this.SERVICE_NAME;
            deviceInfoCmd.CommandObject.Type = this.SERVICE_TYPE;
            deviceInfoCmd.CommandObject.HasError = hasError;
            if (!string.IsNullOrEmpty(errorMessage))
                deviceInfoCmd.CommandObject.Errors.Add(errorMessage);
            nsqProducerService.SendNsqCommand(NsqTopics.CAMERA_RESPONSE_TOPIC, deviceInfoCmd);
        }

        private void startButton_Click(object sender, EventArgs e)
        {
            StartCapture();
        }

        private void StartCapture()
        {
            ComboBoxItem i = (ComboBoxItem)cbCamera.SelectedItem;

            try
            {
                webCameraControl1.StartCapture(i.Id);
            }
            finally
            {
                UpdateButtons();
            }
        }

        private void stopButton_Click(object sender, EventArgs e)
        {
            webCameraControl1.StopCapture();

            UpdateButtons();
        }

        private void imageButton_Click(object sender, EventArgs e)
        {
            // TrungPQ add for start capture.
            ComboBoxItem i = (ComboBoxItem)cbCamera.SelectedItem;
            webCameraControl1.StartCapture(i.Id);

            if (cbCamera.Items == null || cbCamera.Items.Count == 0)
            {
                return;
            }
            SaveFileDialog saveFileDialog1 = new SaveFileDialog();
            saveFileDialog1.Filter = "JPEG Image|*.jpg";
            saveFileDialog1.Title = "Save an Image File";
            saveFileDialog1.DefaultExt = ".jpg";
            saveFileDialog1.FileName = "Test";

            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                var currentImg = webCameraControl1.GetCurrentImage();
                var compress = ((CompressItem)cbCompress.SelectedItem).Value;
                EncoderParameters encoderParameters = new EncoderParameters(1);
                encoderParameters.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, compress);
                currentImg.Save(saveFileDialog1.FileName, GetEncoder(ImageFormat.Jpeg), encoderParameters);
            }

            // TrungPQ add for stop capture.
            webCameraControl1.StopCapture();
        }

        /// <summary>
        /// Delay time.
        /// </summary>
        /// <param name="millisecond"></param>
        /// <returns></returns>
        private bool Delay(int millisecond)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            bool flag = false;
            while (!flag)
            {
                if (sw.ElapsedMilliseconds > millisecond)
                {
                    flag = true;
                }
            }
            sw.Stop();
            return true;
        }

        /// <summary>
        /// Save image.
        /// </summary>
        /// <param name="transactionId"></param>
        /// <param name="isBegin"></param>
        private void SaveImage(string transactionId, bool isBegin = true)
        {
            try
            {
                // TrungPQ add for start capture.
                //this.Invoke(new MethodInvoker(delegate () {
                //    ComboBoxItem i = (ComboBoxItem)cbCamera.SelectedItem;
                //    //TODO: handle exceptions 
                //    webCameraControl1.StartCapture(i.Id);
                //}));

                // Wait for start capture.            
                //TODO: consider to use Thread.Sleep(10) or Task.Delay
                Delay(1000);

                if (cbCamera.Items == null || cbCamera.Items.Count == 0)
                {
                    return;
                }
                //var imgName = DateTime.Now.Ticks.ToString() + jpgExtenstion;
                var imgName = isBegin ? transactionId + ".Begin" + jpgExtenstion : transactionId + ".End" + jpgExtenstion;
                var imgPath = Path.Combine(tempImgFolder, imgName);
                var currentImg = webCameraControl1.GetCurrentImage();
                var compress = 50L;

                Invoke(new Action(() => {
                    compress = ((CompressItem)cbCompress.SelectedItem).Value;
                }));
                
                EncoderParameters encoderParameters = new EncoderParameters(1);
                encoderParameters.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, compress);
                currentImg.Save(imgPath, GetEncoder(ImageFormat.Jpeg), encoderParameters);

                if(isBegin)
                {
                    beginImage = imgPath;
                }
                else
                {
                    endImage = imgPath;
                }
                logService?.LogInfo($"{imgPath} saved");
            }
            catch (Exception ex)
            {
                logService?.LogException(ex.Message);
                logService?.LogException(ex);
            }
            finally
            {
                // TrungPQ add for stop capture.
                //TODO: potential exceptions.
                //webCameraControl1.StopCapture();
            }
        }

        public static ImageCodecInfo GetEncoder(ImageFormat format)
        {
            return ImageCodecInfo.GetImageEncoders().FirstOrDefault(x => x.FormatID == ImageFormat.Jpeg.Guid);
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateButtons();
        }

        private void UpdateButtons()
        {
            startButton.Enabled = cbCamera.SelectedItem != null;
            stopButton.Enabled = webCameraControl1.IsCapturing;
            //imageButton.Enabled = webCameraControl1.IsCapturing;
            imageButton.Enabled = true;
        }

        public void HandleMessage(IMessage message)
        {
            try
            {
                var msg = Encoding.UTF8.GetString(message.Body);
                var obj = JsonConvert.DeserializeObject<NsqCameraRequestCommand>(msg);

                if (obj == null || obj.IsTimeout())
                    return;

                if (obj.Command == UniversalCommandConstants.CameraRequest)
                {
                    if (obj.IsPaymentStart)
                    {
                        SaveImage(obj.TransactionId);
                    }
                    else
                    {
                        SaveImage(obj.TransactionId, obj.IsPaymentStart);

                        var responseCmd = new NsqCameraResponseCommand()
                        {
                            TransactionId = obj.TransactionId,
                            BeginImage = beginImage,
                            EndImage = endImage
                        };

                        //TODO: TrungPQ => SendNsqCommand from other thread to avoid blocking.
                        nsqProducerService.SendNsqCommand(NsqTopics.CAMERA_RESPONSE_TOPIC, responseCmd);
                        beginImage = string.Empty;
                        endImage = string.Empty;
                    }
                }
                else if (obj.Command == UniversalCommandConstants.Ping)
                {
                    var pingCmd = JsonConvert.DeserializeObject<PingCommand>(msg);
                    if (cbCamera.Items.Count > 0)
                        PublishDeviceInfo(false);
                    else
                    {
                        PublishDeviceInfo(true, "No camera found on this PC.");
                    }
                    Task.Delay(500).Wait();
                    CompleteReceivedAsync(pingCmd).Wait();
                }
            }
            catch (Exception)
            {

            }
        }

        private async Task<bool> CompleteReceivedAsync(IUniversalCommands obj)
        {
            return await Task.Run(() =>
            {
                var command = new UniversalACKResponseCommand(obj.CommandId);              
                return nsqProducerService.SendNsqCommand(NsqTopics.CAMERA_RESPONSE_TOPIC, command);
            });
        }

        public void LogFailedMessage(IMessage message)
        {

        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            nsqProducerService?.Dispose();
        }
    }
}
