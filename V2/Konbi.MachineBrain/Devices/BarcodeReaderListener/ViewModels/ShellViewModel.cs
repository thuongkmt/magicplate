using System;
using Caliburn.Micro;
using Konbi.Common.Interfaces;
using KonbiBrain.Interfaces;
using Konbini.Messages.Commands;
using System.Timers;

namespace BarcodeReaderListener.ViewModels
{
    public class ShellViewModel : Conductor<object>, IShell, IDisposable
    {
        KeyboardListener KListener = new KeyboardListener();
        public IMessageProducerService NsqMessageProducerService { get; set; }
        public IKonbiBrainLogService KonbiBrainLogService { get; set; }
        Timer timer;
        string inputText = string.Empty;
        
        protected override void OnActivate()
        {
            try
            {
                base.OnActivate();
                KListener.KeyDown += new RawKeyEventHandler(KListener_KeyDown);
                timer = new Timer();
                // timer.Interval = 10000;
                timer.Interval = 300;
                timer.Elapsed += KeyboardTimoutHandler;
            }
            catch (Exception ex)
            {
                KonbiBrainLogService.LogException(ex);
            }
        }

        void KListener_KeyDown(object sender, RawKeyEventArgs args)
        {
            timer.Start();
            var key = args.Key;
            var inputChar = args.ToString();

            if (key == System.Windows.Input.Key.Enter || key == System.Windows.Input.Key.Return)
            {
                if (string.IsNullOrEmpty(inputText))
                {
                    return;
                }

                var command = new BarcodeScannerCommand();
                command.CommandObject.BarcodeValue = inputText;
                KonbiBrainLogService.LogInfo($"Barcode value: {inputText}");
                NsqMessageProducerService.SendBarcodecommand(command);
                inputText = string.Empty;
            }
            inputText += inputChar;
        }

        void KeyboardTimoutHandler(object sender, ElapsedEventArgs e)
        {
            inputText = string.Empty;
            timer.Stop();
        }

        protected override void OnDeactivate(bool close)
        {
            base.OnDeactivate(close);
            timer.Dispose();
            KListener.Dispose();
        }

        public void Dispose()
        {
            KListener.Dispose();
        }
    }
}