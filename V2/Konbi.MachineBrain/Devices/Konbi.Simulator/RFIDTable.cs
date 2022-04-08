using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using KonbiBrain.Common.Messages;
using KonbiBrain.Common.Services;
using Konbini.Messages.Commands.RFIDTable;

namespace Konbi.Simulator
{
    public partial class RFIDTable : Form
    {
        private readonly NsqMessageProducerService nsqService;
        public RFIDTable()
        {
            InitializeComponent();
            nsqService=new NsqMessageProducerService();
        }

        private void DetectedDiscBtn_Click(object sender, EventArgs e)
        {
            var command = new DetectPlatesCommand();
            command.CommandObject = new DetectPlatesCommandPayload();
            var listPlates = new List<PlateInfo>();
            if (!string.IsNullOrEmpty(TxtBoxPlateCode.Text))
            {
                listPlates.Add(new PlateInfo() { UType = TxtBoxPlateCode.Text.Trim(), UID = string.IsNullOrEmpty(TxtBoxDisUID.Text) ? Guid.NewGuid().ToString() : TxtBoxDisUID.Text.Trim() });
            }
            if (!string.IsNullOrEmpty(TxtBoxPlateCode2.Text))
            {
                listPlates.Add(new PlateInfo() { UType = TxtBoxPlateCode2.Text.Trim(), UID = string.IsNullOrEmpty(TxtBoxDisUID2.Text) ? Guid.NewGuid().ToString() : TxtBoxDisUID2.Text.Trim() });
            }
            if (!string.IsNullOrEmpty(TxtBoxPlateCode3.Text))
            {
                listPlates.Add(new PlateInfo() { UType = TxtBoxPlateCode3.Text.Trim(), UID = string.IsNullOrEmpty(TxtBoxDisUID3.Text) ? Guid.NewGuid().ToString() : TxtBoxDisUID3.Text.Trim() });
            }
            if (!string.IsNullOrEmpty(TxtBoxPlateCode4.Text))
            {
                listPlates.Add(new PlateInfo() { UType = TxtBoxPlateCode4.Text.Trim(), UID = string.IsNullOrEmpty(TxtBoxDisUID4.Text) ? Guid.NewGuid().ToString() : TxtBoxDisUID4.Text.Trim() });
            }
            if (!string.IsNullOrEmpty(TxtBoxPlateCode5.Text))
            {
                listPlates.Add(new PlateInfo() { UType = TxtBoxPlateCode5.Text.Trim(), UID = string.IsNullOrEmpty(TxtBoxDisUID5.Text) ? Guid.NewGuid().ToString() : TxtBoxDisUID5.Text.Trim() });
            }
            command.CommandObject.Plates = listPlates;
                nsqService.SendRfidTableResponseCommand(command);
            ShowMessage(string.Format("there are {0} plates are placed on the table", command.CommandObject.Plates.Count()),5);
            
        }

        private void BtnRemovedDisc_Click(object sender, EventArgs e)
        {
            var command = new DetectPlatesCommand();
            command.CommandObject = new DetectPlatesCommandPayload() { Plates = new List<PlateInfo>() };
            nsqService.SendRfidTableResponseCommand(command);
            ShowMessage("All plates removed",5);
        }

        private void RFIDTable_FormClosed(object sender, FormClosedEventArgs e)
        {
            nsqService?.Dispose();
        }
        /// <summary>
        /// display message and hide after X seconds, Default X=0  means  the message stays remain
        /// </summary>
        /// <param name="message"></param>
        /// <param name="hideAfter"></param>
        private void ShowMessage(string message, int hideAfter = 0)
        {
            lblMessage.Text = message;
            if (hideAfter > 0)
            {
                (new Thread(new ThreadStart(() => {
                    Thread.Sleep(hideAfter * 1000);
                    lblMessage.Invoke(new Action(() => { lblMessage.Text = string.Empty; }));                   
                }))).Start();
            }
        }
    }
}
