using KonbiBrain.Common.Services;
using Konbini.Messages.Commands.RFIDTable;
using Konbini.Messages.Enums;
using Konbini.Messages.Payment;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Konbi.Simulator
{
    public partial class AlarmLight : Form
    {
        private readonly NsqMessageProducerService nsqService;

        public AlarmLight()
        {
            InitializeComponent();
            nsqService = new NsqMessageProducerService();
        }

        private void btnOff_Click(object sender, EventArgs e)
        {
            var command = new AlarmLightCommand()
            {
                Off = true
            };
            nsqService.SendAlarmLightCommand(command);
        }

        private void btnRedLight_Click(object sender, EventArgs e)
        {
            var command = new AlarmLightCommand()
            {
                Red = true,
                Duration = 10 * 1000
            };
            nsqService.SendAlarmLightCommand(command);
        }

        private void btnRedBeep_Click(object sender, EventArgs e)
        {
            var command = new AlarmLightCommand()
            {
                Red = true,
                Beep = true,
                Duration = 10 * 1000
            };
            nsqService.SendAlarmLightCommand(command);
        }

        private void btnGreenLight_Click(object sender, EventArgs e)
        {
            var command = new AlarmLightCommand()
            {
                Green = true,
                Duration = 10 * 1000
            };
            nsqService.SendAlarmLightCommand(command);
        }

        private void btnGreenBeep_Click(object sender, EventArgs e)
        {
            var command = new AlarmLightCommand()
            {
                Green = true,
                Beep = true,
                Duration = 10 * 1000
            };
            nsqService.SendAlarmLightCommand(command);
        }

        private void btnRedGreen_Click(object sender, EventArgs e)
        {
            var command = new AlarmLightCommand()
            {
                Red = true,
                Green = true,
                Duration = 10 * 1000
            };
            nsqService.SendAlarmLightCommand(command);
        }

        private void btnBeep_Click(object sender, EventArgs e)
        {
            var command = new AlarmLightCommand()
            {
                Beep = true,
                Duration = 10 * 1000
            };
            nsqService.SendAlarmLightCommand(command);
        }

        private void btnRedGreenBeep_Click(object sender, EventArgs e)
        {
            var command = new AlarmLightCommand()
            {
                Red = true,
                Green = true,
                Beep = true,
                Duration = 10 * 1000
            };
            nsqService.SendAlarmLightCommand(command);
        }

        private void btnLevelCritical_Click(object sender, EventArgs e)
        {
            var command = new AlarmLightCommand()
            {
                Red = true,
                Blink = true,
                Beep = true,
                Duration = 10 * 1000
            };
            nsqService.SendAlarmLightCommand(command);
        }

        private void BtnLevelHigh_Click(object sender, EventArgs e)
        {
            var command = new AlarmLightCommand()
            {
                Red = true,
                Blink = true,
                Duration = 10 * 1000,
                SoundIntruction = SoundIntructionType.Sample
            };
            nsqService.SendAlarmLightCommand(command);
        }

        private void btnLevelMedium_Click(object sender, EventArgs e)
        {
            var command = new AlarmLightCommand()
            {
                Red = true,
                Duration = 10 * 1000,
                SoundIntruction = SoundIntructionType.Sample
            };
            nsqService.SendAlarmLightCommand(command);
        }

        private void btnLevelLow_Click(object sender, EventArgs e)
        {
            var command = new AlarmLightCommand()
            {
                Red = true,
                Duration = 10 * 1000
            };
            nsqService.SendAlarmLightCommand(command);
        }

        private void btnSampleSound_Click(object sender, EventArgs e)
        {
            var command = new AlarmLightCommand()
            {
                Red = true,
                Duration = 10 * 1000,
                SoundIntruction = SoundIntructionType.Sample
            };
            nsqService.SendAlarmLightCommand(command);
        }
    }
}
