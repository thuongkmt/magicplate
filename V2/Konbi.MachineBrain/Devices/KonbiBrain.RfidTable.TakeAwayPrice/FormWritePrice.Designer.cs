namespace KonbiBrain.RfidTable.TakeAwayPrice
{
    partial class FormWritePrice
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
            this.pricceBox = new System.Windows.Forms.NumericUpDown();
            this.label1 = new System.Windows.Forms.Label();
            this.savePriceBtn = new System.Windows.Forms.Button();
            this.listView1 = new System.Windows.Forms.ListView();
            this.columnHeader4 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader7 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader8 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.label2 = new System.Windows.Forms.Label();
            this.lblTotalPlates = new System.Windows.Forms.Label();
            this.button1 = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.pricceBox)).BeginInit();
            this.SuspendLayout();
            // 
            // pricceBox
            // 
            this.pricceBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.pricceBox.DecimalPlaces = 2;
            this.pricceBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 35F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.pricceBox.Increment = new decimal(new int[] {
            1,
            0,
            0,
            65536});
            this.pricceBox.Location = new System.Drawing.Point(288, 463);
            this.pricceBox.Name = "pricceBox";
            this.pricceBox.Size = new System.Drawing.Size(335, 60);
            this.pricceBox.TabIndex = 0;
            this.pricceBox.Value = new decimal(new int[] {
            55,
            0,
            0,
            65536});
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 35F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(3, 465);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(212, 54);
            this.label1.TabIndex = 1;
            this.label1.Text = "Set Price";
            // 
            // savePriceBtn
            // 
            this.savePriceBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.savePriceBtn.Font = new System.Drawing.Font("Microsoft Sans Serif", 35F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.savePriceBtn.Location = new System.Drawing.Point(469, 557);
            this.savePriceBtn.Name = "savePriceBtn";
            this.savePriceBtn.Size = new System.Drawing.Size(154, 71);
            this.savePriceBtn.TabIndex = 6;
            this.savePriceBtn.Text = "Save";
            this.savePriceBtn.UseVisualStyleBackColor = true;
            this.savePriceBtn.Click += new System.EventHandler(this.savePriceBtn_Click);
            // 
            // listView1
            // 
            this.listView1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listView1.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader4,
            this.columnHeader7,
            this.columnHeader8});
            this.listView1.Font = new System.Drawing.Font("Microsoft Sans Serif", 15F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.listView1.GridLines = true;
            this.listView1.Location = new System.Drawing.Point(12, 84);
            this.listView1.Name = "listView1";
            this.listView1.Size = new System.Drawing.Size(611, 329);
            this.listView1.TabIndex = 7;
            this.listView1.UseCompatibleStateImageBehavior = false;
            this.listView1.View = System.Windows.Forms.View.Details;
            // 
            // columnHeader4
            // 
            this.columnHeader4.Text = "UUID";
            this.columnHeader4.Width = 220;
            // 
            // columnHeader7
            // 
            this.columnHeader7.Text = "Model";
            this.columnHeader7.Width = 120;
            // 
            // columnHeader8
            // 
            this.columnHeader8.Text = "Price";
            this.columnHeader8.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.columnHeader8.Width = 120;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 35F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(2, 9);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(185, 54);
            this.label2.TabIndex = 8;
            this.label2.Text = "Plate(s)";
            this.label2.Click += new System.EventHandler(this.label2_Click);
            // 
            // lblTotalPlates
            // 
            this.lblTotalPlates.AutoSize = true;
            this.lblTotalPlates.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblTotalPlates.ForeColor = System.Drawing.Color.Coral;
            this.lblTotalPlates.Location = new System.Drawing.Point(8, 416);
            this.lblTotalPlates.Name = "lblTotalPlates";
            this.lblTotalPlates.Size = new System.Drawing.Size(118, 20);
            this.lblTotalPlates.TabIndex = 9;
            this.lblTotalPlates.Text = "Total plate(s): 0";
            // 
            // button1
            // 
            this.button1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button1.Font = new System.Drawing.Font("Microsoft Sans Serif", 35F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button1.Location = new System.Drawing.Point(309, 557);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(154, 71);
            this.button1.TabIndex = 10;
            this.button1.Text = "Close";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Visible = false;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // FormWritePrice
            // 
            this.AcceptButton = this.savePriceBtn;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(652, 644);
            this.ControlBox = false;
            this.Controls.Add(this.button1);
            this.Controls.Add(this.lblTotalPlates);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.listView1);
            this.Controls.Add(this.savePriceBtn);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.pricceBox);
            this.MinimizeBox = false;
            this.Name = "FormWritePrice";
            this.ShowIcon = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Takeaway price";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FormWritePrice_FormClosing);
            this.Load += new System.EventHandler(this.FormWritePrice_Load);
            ((System.ComponentModel.ISupportInitialize)(this.pricceBox)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.NumericUpDown pricceBox;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button savePriceBtn;
        private System.Windows.Forms.ListView listView1;
        private System.Windows.Forms.ColumnHeader columnHeader4;
        private System.Windows.Forms.ColumnHeader columnHeader7;
        private System.Windows.Forms.ColumnHeader columnHeader8;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label lblTotalPlates;
        private System.Windows.Forms.Button button1;
    }
}