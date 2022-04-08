namespace Konbi.Simulator
{
    partial class TransactionStressTest
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.txtPlateConfig = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.btnStart = new System.Windows.Forms.Button();
            this.txtTranNumber = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.lblPlateCount = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.cbRandomPlate = new System.Windows.Forms.CheckBox();
            this.cbRandomPayment = new System.Windows.Forms.CheckBox();
            this.lblScanning = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // txtPlateConfig
            // 
            this.txtPlateConfig.Location = new System.Drawing.Point(5, 85);
            this.txtPlateConfig.Multiline = true;
            this.txtPlateConfig.Name = "txtPlateConfig";
            this.txtPlateConfig.Size = new System.Drawing.Size(351, 164);
            this.txtPlateConfig.TabIndex = 0;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(2, 19);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(304, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "- Predefined Plates below. following format {PlateType} - {UID}.";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(3, 38);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(119, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "- Each plate is in a line. ";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(3, 58);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(301, 13);
            this.label3.TabIndex = 3;
            this.label3.Text = "- The program will pickup those randomly to simulate scanning.";
            // 
            // btnStart
            // 
            this.btnStart.Location = new System.Drawing.Point(281, 266);
            this.btnStart.Name = "btnStart";
            this.btnStart.Size = new System.Drawing.Size(75, 23);
            this.btnStart.TabIndex = 4;
            this.btnStart.Text = "Start";
            this.btnStart.UseVisualStyleBackColor = true;
            this.btnStart.Click += new System.EventHandler(this.btnStart_Click);
            // 
            // txtTranNumber
            // 
            this.txtTranNumber.Location = new System.Drawing.Point(137, 268);
            this.txtTranNumber.Name = "txtTranNumber";
            this.txtTranNumber.Size = new System.Drawing.Size(100, 20);
            this.txtTranNumber.TabIndex = 5;
            this.txtTranNumber.Text = "400";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(12, 271);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(123, 13);
            this.label4.TabIndex = 6;
            this.label4.Text = "Number of Transactions:";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(12, 356);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(39, 13);
            this.label5.TabIndex = 7;
            this.label5.Text = "Plates:";
            // 
            // lblPlateCount
            // 
            this.lblPlateCount.AutoSize = true;
            this.lblPlateCount.Location = new System.Drawing.Point(57, 356);
            this.lblPlateCount.Name = "lblPlateCount";
            this.lblPlateCount.Size = new System.Drawing.Size(43, 13);
            this.lblPlateCount.TabIndex = 8;
            this.lblPlateCount.Text = "{plates}";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(106, 356);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(55, 13);
            this.label6.TabIndex = 9;
            this.label6.Text = "Scanning:";
            // 
            // cbRandomPlate
            // 
            this.cbRandomPlate.AutoSize = true;
            this.cbRandomPlate.Checked = true;
            this.cbRandomPlate.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cbRandomPlate.Enabled = false;
            this.cbRandomPlate.Location = new System.Drawing.Point(15, 303);
            this.cbRandomPlate.Name = "cbRandomPlate";
            this.cbRandomPlate.Size = new System.Drawing.Size(98, 17);
            this.cbRandomPlate.TabIndex = 10;
            this.cbRandomPlate.Text = "Random Plates";
            this.cbRandomPlate.UseVisualStyleBackColor = true;
            // 
            // cbRandomPayment
            // 
            this.cbRandomPayment.AutoSize = true;
            this.cbRandomPayment.Checked = true;
            this.cbRandomPayment.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cbRandomPayment.Location = new System.Drawing.Point(137, 303);
            this.cbRandomPayment.Name = "cbRandomPayment";
            this.cbRandomPayment.Size = new System.Drawing.Size(190, 17);
            this.cbRandomPayment.TabIndex = 11;
            this.cbRandomPayment.Text = "Random Payment Success/Failure";
            this.cbRandomPayment.UseVisualStyleBackColor = true;
            // 
            // lblScanning
            // 
            this.lblScanning.AutoSize = true;
            this.lblScanning.Location = new System.Drawing.Point(167, 356);
            this.lblScanning.Name = "lblScanning";
            this.lblScanning.Size = new System.Drawing.Size(60, 13);
            this.lblScanning.TabIndex = 12;
            this.lblScanning.Text = "{Scanning}";
            // 
            // TransactionStressTest
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(383, 394);
            this.Controls.Add(this.lblScanning);
            this.Controls.Add(this.cbRandomPayment);
            this.Controls.Add(this.cbRandomPlate);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.lblPlateCount);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.txtTranNumber);
            this.Controls.Add(this.btnStart);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.txtPlateConfig);
            this.Name = "TransactionStressTest";
            this.Text = "Test Transactions";
            this.Load += new System.EventHandler(this.TransactionStressTest_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox txtPlateConfig;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button btnStart;
        private System.Windows.Forms.TextBox txtTranNumber;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label lblPlateCount;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.CheckBox cbRandomPlate;
        private System.Windows.Forms.CheckBox cbRandomPayment;
        private System.Windows.Forms.Label lblScanning;
    }
}