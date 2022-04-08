namespace KonbiBrain.RfidTable.TakeAwayPrice
{
    partial class FormMessageBox
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormMessageBox));
            this.iconSuccess = new System.Windows.Forms.PictureBox();
            this.labelMessage = new System.Windows.Forms.Label();
            this.btnOK = new Bunifu.Framework.UI.BunifuFlatButton();
            this.iconError = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.iconSuccess)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.iconError)).BeginInit();
            this.SuspendLayout();
            // 
            // iconSuccess
            // 
            this.iconSuccess.Image = ((System.Drawing.Image)(resources.GetObject("iconSuccess.Image")));
            this.iconSuccess.Location = new System.Drawing.Point(207, 29);
            this.iconSuccess.Name = "iconSuccess";
            this.iconSuccess.Size = new System.Drawing.Size(99, 99);
            this.iconSuccess.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.iconSuccess.TabIndex = 0;
            this.iconSuccess.TabStop = false;
            // 
            // labelMessage
            // 
            this.labelMessage.Font = new System.Drawing.Font("Microsoft Sans Serif", 13.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelMessage.ForeColor = System.Drawing.Color.White;
            this.labelMessage.Location = new System.Drawing.Point(1, 141);
            this.labelMessage.Name = "labelMessage";
            this.labelMessage.Size = new System.Drawing.Size(508, 104);
            this.labelMessage.TabIndex = 1;
            this.labelMessage.Text = "successful";
            this.labelMessage.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // btnOK
            // 
            this.btnOK.Activecolor = System.Drawing.Color.FromArgb(((int)(((byte)(50)))), ((int)(((byte)(186)))), ((int)(((byte)(124)))));
            this.btnOK.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(50)))), ((int)(((byte)(186)))), ((int)(((byte)(124)))));
            this.btnOK.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnOK.BorderRadius = 6;
            this.btnOK.ButtonText = "        OK";
            this.btnOK.Cursor = System.Windows.Forms.Cursors.WaitCursor;
            this.btnOK.DisabledColor = System.Drawing.Color.Gray;
            this.btnOK.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnOK.Iconcolor = System.Drawing.Color.Transparent;
            this.btnOK.Iconimage = ((System.Drawing.Image)(resources.GetObject("btnOK.Iconimage")));
            this.btnOK.Iconimage_right = null;
            this.btnOK.Iconimage_right_Selected = null;
            this.btnOK.Iconimage_Selected = null;
            this.btnOK.IconMarginLeft = 0;
            this.btnOK.IconMarginRight = 0;
            this.btnOK.IconRightVisible = true;
            this.btnOK.IconRightZoom = 0D;
            this.btnOK.IconVisible = true;
            this.btnOK.IconZoom = 90D;
            this.btnOK.IsTab = false;
            this.btnOK.Location = new System.Drawing.Point(190, 249);
            this.btnOK.Margin = new System.Windows.Forms.Padding(4);
            this.btnOK.Name = "btnOK";
            this.btnOK.Normalcolor = System.Drawing.Color.FromArgb(((int)(((byte)(50)))), ((int)(((byte)(186)))), ((int)(((byte)(124)))));
            this.btnOK.OnHovercolor = System.Drawing.Color.FromArgb(((int)(((byte)(36)))), ((int)(((byte)(129)))), ((int)(((byte)(77)))));
            this.btnOK.OnHoverTextColor = System.Drawing.Color.White;
            this.btnOK.selected = false;
            this.btnOK.Size = new System.Drawing.Size(129, 59);
            this.btnOK.TabIndex = 2;
            this.btnOK.Text = "        OK";
            this.btnOK.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.btnOK.Textcolor = System.Drawing.Color.White;
            this.btnOK.TextFont = new System.Drawing.Font("Microsoft Sans Serif", 13.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnOK.UseWaitCursor = true;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            this.btnOK.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.btnOK_KeyPress);
            // 
            // iconError
            // 
            this.iconError.Image = ((System.Drawing.Image)(resources.GetObject("iconError.Image")));
            this.iconError.Location = new System.Drawing.Point(207, 29);
            this.iconError.Name = "iconError";
            this.iconError.Size = new System.Drawing.Size(99, 99);
            this.iconError.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.iconError.TabIndex = 3;
            this.iconError.TabStop = false;
            // 
            // FormMessageBox
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(56)))), ((int)(((byte)(77)))), ((int)(((byte)(98)))));
            this.ClientSize = new System.Drawing.Size(511, 319);
            this.Controls.Add(this.iconError);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.labelMessage);
            this.Controls.Add(this.iconSuccess);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "FormMessageBox";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "FormMessageBox";
            ((System.ComponentModel.ISupportInitialize)(this.iconSuccess)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.iconError)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.PictureBox iconSuccess;
        private System.Windows.Forms.Label labelMessage;
        private Bunifu.Framework.UI.BunifuFlatButton btnOK;
        private System.Windows.Forms.PictureBox iconError;
    }
}