namespace StorageTesting
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
            this.BtnInstall = new System.Windows.Forms.Button();
            this.BtnInitialize = new System.Windows.Forms.Button();
            this.BtnExit = new System.Windows.Forms.Button();
            this.BtnGetAllGroups = new System.Windows.Forms.Button();
            this.BtnAuthenticate = new System.Windows.Forms.Button();
            this.BtnGetGroup = new System.Windows.Forms.Button();
            this.BtnCreateGroup = new System.Windows.Forms.Button();
            this.BtnModifyGroup = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // TxtOutput
            // 
            this.TxtOutput.Location = new System.Drawing.Point(150, 12);
            this.TxtOutput.Multiline = true;
            this.TxtOutput.Name = "TxtOutput";
            this.TxtOutput.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.TxtOutput.Size = new System.Drawing.Size(1026, 580);
            this.TxtOutput.TabIndex = 0;
            // 
            // BtnInstall
            // 
            this.BtnInstall.Location = new System.Drawing.Point(12, 12);
            this.BtnInstall.Name = "BtnInstall";
            this.BtnInstall.Size = new System.Drawing.Size(132, 20);
            this.BtnInstall.TabIndex = 1;
            this.BtnInstall.Text = "Install";
            this.BtnInstall.UseVisualStyleBackColor = true;
            this.BtnInstall.Click += new System.EventHandler(this.BtnInstall_Click);
            // 
            // BtnInitialize
            // 
            this.BtnInitialize.Location = new System.Drawing.Point(12, 38);
            this.BtnInitialize.Name = "BtnInitialize";
            this.BtnInitialize.Size = new System.Drawing.Size(132, 20);
            this.BtnInitialize.TabIndex = 2;
            this.BtnInitialize.Text = "Initialize";
            this.BtnInitialize.UseVisualStyleBackColor = true;
            this.BtnInitialize.Click += new System.EventHandler(this.BtnInitialize_Click);
            // 
            // BtnExit
            // 
            this.BtnExit.Location = new System.Drawing.Point(12, 572);
            this.BtnExit.Name = "BtnExit";
            this.BtnExit.Size = new System.Drawing.Size(132, 20);
            this.BtnExit.TabIndex = 3;
            this.BtnExit.Text = "Exit";
            this.BtnExit.UseVisualStyleBackColor = true;
            this.BtnExit.Click += new System.EventHandler(this.BtnExit_Click);
            // 
            // BtnGetAllGroups
            // 
            this.BtnGetAllGroups.Location = new System.Drawing.Point(12, 107);
            this.BtnGetAllGroups.Name = "BtnGetAllGroups";
            this.BtnGetAllGroups.Size = new System.Drawing.Size(132, 20);
            this.BtnGetAllGroups.TabIndex = 4;
            this.BtnGetAllGroups.Text = "GetAllGroups";
            this.BtnGetAllGroups.UseVisualStyleBackColor = true;
            this.BtnGetAllGroups.Click += new System.EventHandler(this.BtnGetAllGroups_Click);
            // 
            // BtnAuthenticate
            // 
            this.BtnAuthenticate.Location = new System.Drawing.Point(12, 64);
            this.BtnAuthenticate.Name = "BtnAuthenticate";
            this.BtnAuthenticate.Size = new System.Drawing.Size(132, 20);
            this.BtnAuthenticate.TabIndex = 5;
            this.BtnAuthenticate.Text = "Authenticate (Login)";
            this.BtnAuthenticate.UseVisualStyleBackColor = true;
            this.BtnAuthenticate.Click += new System.EventHandler(this.BtnAuthenticate_Click);
            // 
            // BtnGetGroup
            // 
            this.BtnGetGroup.Location = new System.Drawing.Point(12, 133);
            this.BtnGetGroup.Name = "BtnGetGroup";
            this.BtnGetGroup.Size = new System.Drawing.Size(132, 20);
            this.BtnGetGroup.TabIndex = 6;
            this.BtnGetGroup.Text = "GetGroup";
            this.BtnGetGroup.UseVisualStyleBackColor = true;
            this.BtnGetGroup.Click += new System.EventHandler(this.BtnGetGroup_Click);
            // 
            // BtnCreateGroup
            // 
            this.BtnCreateGroup.Location = new System.Drawing.Point(12, 159);
            this.BtnCreateGroup.Name = "BtnCreateGroup";
            this.BtnCreateGroup.Size = new System.Drawing.Size(132, 20);
            this.BtnCreateGroup.TabIndex = 7;
            this.BtnCreateGroup.Text = "CreateGroup";
            this.BtnCreateGroup.UseVisualStyleBackColor = true;
            this.BtnCreateGroup.Click += new System.EventHandler(this.BtnCreateGroup_Click);
            // 
            // BtnModifyGroup
            // 
            this.BtnModifyGroup.Location = new System.Drawing.Point(12, 185);
            this.BtnModifyGroup.Name = "BtnModifyGroup";
            this.BtnModifyGroup.Size = new System.Drawing.Size(132, 20);
            this.BtnModifyGroup.TabIndex = 8;
            this.BtnModifyGroup.Text = "ModifyGroup";
            this.BtnModifyGroup.UseVisualStyleBackColor = true;
            this.BtnModifyGroup.Click += new System.EventHandler(this.BtnModifyGroup_Click);
            // 
            // FrmMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1188, 604);
            this.Controls.Add(this.BtnModifyGroup);
            this.Controls.Add(this.BtnCreateGroup);
            this.Controls.Add(this.BtnGetGroup);
            this.Controls.Add(this.BtnAuthenticate);
            this.Controls.Add(this.BtnGetAllGroups);
            this.Controls.Add(this.BtnExit);
            this.Controls.Add(this.BtnInitialize);
            this.Controls.Add(this.BtnInstall);
            this.Controls.Add(this.TxtOutput);
            this.Name = "FrmMain";
            this.Text = "OpenDMS.Storage Tester";
            this.Load += new System.EventHandler(this.FrmMain_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox TxtOutput;
        private System.Windows.Forms.Button BtnInstall;
        private System.Windows.Forms.Button BtnInitialize;
        private System.Windows.Forms.Button BtnExit;
        private System.Windows.Forms.Button BtnGetAllGroups;
        private System.Windows.Forms.Button BtnAuthenticate;
        private System.Windows.Forms.Button BtnGetGroup;
        private System.Windows.Forms.Button BtnCreateGroup;
        private System.Windows.Forms.Button BtnModifyGroup;
    }
}

