namespace ClientTesting
{
    partial class FrmMain
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
            this.TxtOutput = new System.Windows.Forms.TextBox();
            this.btnAuthenticate = new System.Windows.Forms.Button();
            this.BtnExit = new System.Windows.Forms.Button();
            this.BtnPing = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // TxtOutput
            // 
            this.TxtOutput.Location = new System.Drawing.Point(218, 12);
            this.TxtOutput.Multiline = true;
            this.TxtOutput.Name = "TxtOutput";
            this.TxtOutput.Size = new System.Drawing.Size(669, 629);
            this.TxtOutput.TabIndex = 0;
            // 
            // btnAuthenticate
            // 
            this.btnAuthenticate.Location = new System.Drawing.Point(12, 63);
            this.btnAuthenticate.Name = "btnAuthenticate";
            this.btnAuthenticate.Size = new System.Drawing.Size(200, 23);
            this.btnAuthenticate.TabIndex = 1;
            this.btnAuthenticate.Text = "Authenticate";
            this.btnAuthenticate.UseVisualStyleBackColor = true;
            this.btnAuthenticate.Click += new System.EventHandler(this.btnAuthenticate_Click);
            // 
            // BtnExit
            // 
            this.BtnExit.Location = new System.Drawing.Point(12, 647);
            this.BtnExit.Name = "BtnExit";
            this.BtnExit.Size = new System.Drawing.Size(200, 23);
            this.BtnExit.TabIndex = 2;
            this.BtnExit.Text = "Exit";
            this.BtnExit.UseVisualStyleBackColor = true;
            this.BtnExit.Click += new System.EventHandler(this.BtnExit_Click);
            // 
            // BtnPing
            // 
            this.BtnPing.Location = new System.Drawing.Point(12, 12);
            this.BtnPing.Name = "BtnPing";
            this.BtnPing.Size = new System.Drawing.Size(200, 23);
            this.BtnPing.TabIndex = 3;
            this.BtnPing.Text = "Ping";
            this.BtnPing.UseVisualStyleBackColor = true;
            this.BtnPing.Click += new System.EventHandler(this.BtnPing_Click);
            // 
            // FrmMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1105, 682);
            this.Controls.Add(this.BtnPing);
            this.Controls.Add(this.BtnExit);
            this.Controls.Add(this.btnAuthenticate);
            this.Controls.Add(this.TxtOutput);
            this.Name = "FrmMain";
            this.Text = "OpenDMS.Net Client Testing Application";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox TxtOutput;
        private System.Windows.Forms.Button btnAuthenticate;
        private System.Windows.Forms.Button BtnExit;
        private System.Windows.Forms.Button BtnPing;
    }
}

