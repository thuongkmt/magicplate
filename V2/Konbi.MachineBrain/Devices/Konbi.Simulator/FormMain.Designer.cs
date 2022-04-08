namespace Konbi.Simulator
{
    partial class FormMain
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
            this.button1 = new System.Windows.Forms.Button();
            this.BtnMdbCashless = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.btnTransactionStressTest = new System.Windows.Forms.Button();
            this.button3 = new System.Windows.Forms.Button();
            this.button4 = new System.Windows.Forms.Button();
            this.button5 = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(48, 27);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(88, 23);
            this.button1.TabIndex = 0;
            this.button1.Text = "RFID Table";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // BtnMdbCashless
            // 
            this.BtnMdbCashless.Location = new System.Drawing.Point(48, 70);
            this.BtnMdbCashless.Name = "BtnMdbCashless";
            this.BtnMdbCashless.Size = new System.Drawing.Size(88, 23);
            this.BtnMdbCashless.TabIndex = 1;
            this.BtnMdbCashless.Text = "Mdb Cashless";
            this.BtnMdbCashless.UseVisualStyleBackColor = true;
            this.BtnMdbCashless.Click += new System.EventHandler(this.BtnMdbCashless_Click);
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(183, 27);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(88, 23);
            this.button2.TabIndex = 2;
            this.button2.Text = "Mqtt Test";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // btnTransactionStressTest
            // 
            this.btnTransactionStressTest.Location = new System.Drawing.Point(183, 70);
            this.btnTransactionStressTest.Name = "btnTransactionStressTest";
            this.btnTransactionStressTest.Size = new System.Drawing.Size(160, 23);
            this.btnTransactionStressTest.TabIndex = 3;
            this.btnTransactionStressTest.Text = "Transactions - Stress Test";
            this.btnTransactionStressTest.UseVisualStyleBackColor = true;
            this.btnTransactionStressTest.Click += new System.EventHandler(this.btnTransactionStressTest_Click);
            // 
            // button3
            // 
            this.button3.Location = new System.Drawing.Point(183, 115);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(170, 23);
            this.button3.TabIndex = 4;
            this.button3.Text = "Send RabbitMQ";
            this.button3.UseVisualStyleBackColor = true;
            this.button3.Click += new System.EventHandler(this.button3_Click);
            // 
            // button4
            // 
            this.button4.Location = new System.Drawing.Point(48, 114);
            this.button4.Name = "button4";
            this.button4.Size = new System.Drawing.Size(75, 23);
            this.button4.TabIndex = 5;
            this.button4.Text = "Alarm Light";
            this.button4.UseVisualStyleBackColor = true;
            this.button4.Click += new System.EventHandler(this.button4_Click);
            // 
            // button5
            // 
            this.button5.Location = new System.Drawing.Point(48, 161);
            this.button5.Name = "button5";
            this.button5.Size = new System.Drawing.Size(88, 23);
            this.button5.TabIndex = 6;
            this.button5.Text = "Payment";
            this.button5.UseVisualStyleBackColor = true;
            this.button5.Click += new System.EventHandler(this.button5_Click);
            // 
            // FormMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(412, 310);
            this.Controls.Add(this.button5);
            this.Controls.Add(this.button4);
            this.Controls.Add(this.button3);
            this.Controls.Add(this.btnTransactionStressTest);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.BtnMdbCashless);
            this.Controls.Add(this.button1);
            this.Name = "FormMain";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Simulator - only use for debugging purpose";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button BtnMdbCashless;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Button btnTransactionStressTest;
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.Button button4;
        private System.Windows.Forms.Button button5;
    }
}

