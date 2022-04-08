namespace Konbi.Simulator
{
    partial class MdbCashless
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
            this.BtnUserScan = new System.Windows.Forms.Button();
            this.TxtTranCode = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.button1 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.ckbSimulateEnablePayment = new System.Windows.Forms.CheckBox();
            this.ckbSimulateDisablePayment = new System.Windows.Forms.CheckBox();
            this.label3 = new System.Windows.Forms.Label();
            this.panel1 = new System.Windows.Forms.Panel();
            this.rbtnEnablePaymentFailed = new System.Windows.Forms.RadioButton();
            this.rbtnEnablePaymentSuccess = new System.Windows.Forms.RadioButton();
            this.panel2 = new System.Windows.Forms.Panel();
            this.rbtndisablePaymentFailed = new System.Windows.Forms.RadioButton();
            this.rbtndisablePaymentSuccess = new System.Windows.Forms.RadioButton();
            this.btnPaymentInProgress = new System.Windows.Forms.Button();
            this.panel1.SuspendLayout();
            this.panel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // BtnUserScan
            // 
            this.BtnUserScan.Location = new System.Drawing.Point(23, 28);
            this.BtnUserScan.Name = "BtnUserScan";
            this.BtnUserScan.Size = new System.Drawing.Size(126, 23);
            this.BtnUserScan.TabIndex = 0;
            this.BtnUserScan.Text = "User scan";
            this.BtnUserScan.UseVisualStyleBackColor = true;
            this.BtnUserScan.Click += new System.EventHandler(this.BtnUserScan_Click);
            // 
            // TxtTranCode
            // 
            this.TxtTranCode.Location = new System.Drawing.Point(244, 31);
            this.TxtTranCode.Name = "TxtTranCode";
            this.TxtTranCode.Size = new System.Drawing.Size(268, 20);
            this.TxtTranCode.TabIndex = 1;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(160, 33);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(54, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "TranCode";
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(23, 73);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(126, 23);
            this.button1.TabIndex = 3;
            this.button1.Text = "Payment Success";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(23, 121);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(126, 23);
            this.button2.TabIndex = 4;
            this.button2.Text = "Payment Error";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(160, 83);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(130, 13);
            this.label2.TabIndex = 5;
            this.label2.Text = "Simulate EnablePayment?";
            // 
            // ckbSimulateEnablePayment
            // 
            this.ckbSimulateEnablePayment.AutoSize = true;
            this.ckbSimulateEnablePayment.Checked = true;
            this.ckbSimulateEnablePayment.CheckState = System.Windows.Forms.CheckState.Checked;
            this.ckbSimulateEnablePayment.Location = new System.Drawing.Point(305, 82);
            this.ckbSimulateEnablePayment.Name = "ckbSimulateEnablePayment";
            this.ckbSimulateEnablePayment.Size = new System.Drawing.Size(15, 14);
            this.ckbSimulateEnablePayment.TabIndex = 6;
            this.ckbSimulateEnablePayment.UseVisualStyleBackColor = true;
            // 
            // ckbSimulateDisablePayment
            // 
            this.ckbSimulateDisablePayment.AutoSize = true;
            this.ckbSimulateDisablePayment.Checked = true;
            this.ckbSimulateDisablePayment.CheckState = System.Windows.Forms.CheckState.Checked;
            this.ckbSimulateDisablePayment.Location = new System.Drawing.Point(305, 130);
            this.ckbSimulateDisablePayment.Name = "ckbSimulateDisablePayment";
            this.ckbSimulateDisablePayment.Size = new System.Drawing.Size(15, 14);
            this.ckbSimulateDisablePayment.TabIndex = 11;
            this.ckbSimulateDisablePayment.UseVisualStyleBackColor = true;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(160, 131);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(132, 13);
            this.label3.TabIndex = 10;
            this.label3.Text = "Simulate DisablePayment?";
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.rbtnEnablePaymentFailed);
            this.panel1.Controls.Add(this.rbtnEnablePaymentSuccess);
            this.panel1.Location = new System.Drawing.Point(347, 73);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(200, 31);
            this.panel1.TabIndex = 14;
            // 
            // rbtnEnablePaymentFailed
            // 
            this.rbtnEnablePaymentFailed.AutoSize = true;
            this.rbtnEnablePaymentFailed.Location = new System.Drawing.Point(125, 7);
            this.rbtnEnablePaymentFailed.Name = "rbtnEnablePaymentFailed";
            this.rbtnEnablePaymentFailed.Size = new System.Drawing.Size(50, 17);
            this.rbtnEnablePaymentFailed.TabIndex = 11;
            this.rbtnEnablePaymentFailed.Text = "False";
            this.rbtnEnablePaymentFailed.UseVisualStyleBackColor = true;
            // 
            // rbtnEnablePaymentSuccess
            // 
            this.rbtnEnablePaymentSuccess.AutoSize = true;
            this.rbtnEnablePaymentSuccess.Checked = true;
            this.rbtnEnablePaymentSuccess.Location = new System.Drawing.Point(25, 7);
            this.rbtnEnablePaymentSuccess.Name = "rbtnEnablePaymentSuccess";
            this.rbtnEnablePaymentSuccess.Size = new System.Drawing.Size(66, 17);
            this.rbtnEnablePaymentSuccess.TabIndex = 10;
            this.rbtnEnablePaymentSuccess.TabStop = true;
            this.rbtnEnablePaymentSuccess.Text = "Success";
            this.rbtnEnablePaymentSuccess.UseVisualStyleBackColor = true;
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.rbtndisablePaymentFailed);
            this.panel2.Controls.Add(this.rbtndisablePaymentSuccess);
            this.panel2.Location = new System.Drawing.Point(347, 121);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(200, 31);
            this.panel2.TabIndex = 15;
            // 
            // rbtndisablePaymentFailed
            // 
            this.rbtndisablePaymentFailed.AutoSize = true;
            this.rbtndisablePaymentFailed.Location = new System.Drawing.Point(125, 7);
            this.rbtndisablePaymentFailed.Name = "rbtndisablePaymentFailed";
            this.rbtndisablePaymentFailed.Size = new System.Drawing.Size(50, 17);
            this.rbtndisablePaymentFailed.TabIndex = 11;
            this.rbtndisablePaymentFailed.Text = "False";
            this.rbtndisablePaymentFailed.UseVisualStyleBackColor = true;
            // 
            // rbtndisablePaymentSuccess
            // 
            this.rbtndisablePaymentSuccess.AutoSize = true;
            this.rbtndisablePaymentSuccess.Checked = true;
            this.rbtndisablePaymentSuccess.Location = new System.Drawing.Point(25, 7);
            this.rbtndisablePaymentSuccess.Name = "rbtndisablePaymentSuccess";
            this.rbtndisablePaymentSuccess.Size = new System.Drawing.Size(66, 17);
            this.rbtndisablePaymentSuccess.TabIndex = 10;
            this.rbtndisablePaymentSuccess.TabStop = true;
            this.rbtndisablePaymentSuccess.Text = "Success";
            this.rbtndisablePaymentSuccess.UseVisualStyleBackColor = true;
            // 
            // btnPaymentInProgress
            // 
            this.btnPaymentInProgress.Location = new System.Drawing.Point(23, 165);
            this.btnPaymentInProgress.Name = "btnPaymentInProgress";
            this.btnPaymentInProgress.Size = new System.Drawing.Size(126, 23);
            this.btnPaymentInProgress.TabIndex = 16;
            this.btnPaymentInProgress.Text = "Payment is InProgress";
            this.btnPaymentInProgress.UseVisualStyleBackColor = true;
            this.btnPaymentInProgress.Click += new System.EventHandler(this.btnPaymentInProgress_Click);
            // 
            // MdbCashless
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(639, 371);
            this.Controls.Add(this.btnPaymentInProgress);
            this.Controls.Add(this.panel2);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.ckbSimulateDisablePayment);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.ckbSimulateEnablePayment);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.TxtTranCode);
            this.Controls.Add(this.BtnUserScan);
            this.Name = "MdbCashless";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "MdbCashless";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.MdbCashless_FormClosed);
            this.Load += new System.EventHandler(this.MdbCashless_Load);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button BtnUserScan;
        private System.Windows.Forms.TextBox TxtTranCode;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.CheckBox ckbSimulateDisablePayment;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.RadioButton rbtnEnablePaymentFailed;
        private System.Windows.Forms.RadioButton rbtnEnablePaymentSuccess;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.RadioButton rbtndisablePaymentFailed;
        private System.Windows.Forms.RadioButton rbtndisablePaymentSuccess;
        private System.Windows.Forms.CheckBox ckbSimulateEnablePayment;
        private System.Windows.Forms.Button btnPaymentInProgress;
    }
}