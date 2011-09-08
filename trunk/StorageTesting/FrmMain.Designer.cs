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
            this.BtnGetAllUsers = new System.Windows.Forms.Button();
            this.BtnModifyUser = new System.Windows.Forms.Button();
            this.BtnCreateUser = new System.Windows.Forms.Button();
            this.BtnGetUser = new System.Windows.Forms.Button();
            this.BtnGetResource = new System.Windows.Forms.Button();
            this.BtnGetResourceReadOnly = new System.Windows.Forms.Button();
            this.BtnCreateNewResource = new System.Windows.Forms.Button();
            this.BtnModifyResource = new System.Windows.Forms.Button();
            this.BtnRollbackResource = new System.Windows.Forms.Button();
            this.BtnDeleteResource = new System.Windows.Forms.Button();
            this.BtnGetVersion = new System.Windows.Forms.Button();
            this.BtnGetCurrentVersion = new System.Windows.Forms.Button();
            this.BtnCreateNewVersion = new System.Windows.Forms.Button();
            this.BtnModifyVersion = new System.Windows.Forms.Button();
            this.BtnGetGlobalPermissions = new System.Windows.Forms.Button();
            this.BtnModifyGlobalPermissions = new System.Windows.Forms.Button();
            this.BtnGetResourceUsageRightsTemplate = new System.Windows.Forms.Button();
            this.BtnModifyResourceUsageRightsTemplate = new System.Windows.Forms.Button();
            this.BtnSearch = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // TxtOutput
            // 
            this.TxtOutput.Location = new System.Drawing.Point(150, 12);
            this.TxtOutput.Multiline = true;
            this.TxtOutput.Name = "TxtOutput";
            this.TxtOutput.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.TxtOutput.Size = new System.Drawing.Size(966, 468);
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
            this.BtnExit.Location = new System.Drawing.Point(12, 486);
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
            // BtnGetAllUsers
            // 
            this.BtnGetAllUsers.Location = new System.Drawing.Point(12, 231);
            this.BtnGetAllUsers.Name = "BtnGetAllUsers";
            this.BtnGetAllUsers.Size = new System.Drawing.Size(132, 20);
            this.BtnGetAllUsers.TabIndex = 9;
            this.BtnGetAllUsers.Text = "GetAllUsers";
            this.BtnGetAllUsers.UseVisualStyleBackColor = true;
            this.BtnGetAllUsers.Click += new System.EventHandler(this.BtnGetAllUsers_Click);
            // 
            // BtnModifyUser
            // 
            this.BtnModifyUser.Location = new System.Drawing.Point(12, 309);
            this.BtnModifyUser.Name = "BtnModifyUser";
            this.BtnModifyUser.Size = new System.Drawing.Size(132, 20);
            this.BtnModifyUser.TabIndex = 12;
            this.BtnModifyUser.Text = "ModifyUser";
            this.BtnModifyUser.UseVisualStyleBackColor = true;
            this.BtnModifyUser.Click += new System.EventHandler(this.BtnModifyUser_Click);
            // 
            // BtnCreateUser
            // 
            this.BtnCreateUser.Location = new System.Drawing.Point(12, 283);
            this.BtnCreateUser.Name = "BtnCreateUser";
            this.BtnCreateUser.Size = new System.Drawing.Size(132, 20);
            this.BtnCreateUser.TabIndex = 11;
            this.BtnCreateUser.Text = "CreateUser";
            this.BtnCreateUser.UseVisualStyleBackColor = true;
            this.BtnCreateUser.Click += new System.EventHandler(this.BtnCreateUser_Click);
            // 
            // BtnGetUser
            // 
            this.BtnGetUser.Location = new System.Drawing.Point(12, 257);
            this.BtnGetUser.Name = "BtnGetUser";
            this.BtnGetUser.Size = new System.Drawing.Size(132, 20);
            this.BtnGetUser.TabIndex = 10;
            this.BtnGetUser.Text = "GetUser";
            this.BtnGetUser.UseVisualStyleBackColor = true;
            this.BtnGetUser.Click += new System.EventHandler(this.BtnGetUser_Click);
            // 
            // BtnGetResource
            // 
            this.BtnGetResource.Location = new System.Drawing.Point(1122, 12);
            this.BtnGetResource.Name = "BtnGetResource";
            this.BtnGetResource.Size = new System.Drawing.Size(132, 20);
            this.BtnGetResource.TabIndex = 13;
            this.BtnGetResource.Text = "GetResource";
            this.BtnGetResource.UseVisualStyleBackColor = true;
            this.BtnGetResource.Click += new System.EventHandler(this.BtnGetResource_Click);
            // 
            // BtnGetResourceReadOnly
            // 
            this.BtnGetResourceReadOnly.Location = new System.Drawing.Point(1122, 38);
            this.BtnGetResourceReadOnly.Name = "BtnGetResourceReadOnly";
            this.BtnGetResourceReadOnly.Size = new System.Drawing.Size(132, 20);
            this.BtnGetResourceReadOnly.TabIndex = 14;
            this.BtnGetResourceReadOnly.Text = "GetResourceReadOnly";
            this.BtnGetResourceReadOnly.UseVisualStyleBackColor = true;
            this.BtnGetResourceReadOnly.Click += new System.EventHandler(this.BtnGetResourceReadOnly_Click);
            // 
            // BtnCreateNewResource
            // 
            this.BtnCreateNewResource.Location = new System.Drawing.Point(1122, 64);
            this.BtnCreateNewResource.Name = "BtnCreateNewResource";
            this.BtnCreateNewResource.Size = new System.Drawing.Size(132, 20);
            this.BtnCreateNewResource.TabIndex = 15;
            this.BtnCreateNewResource.Text = "CreateNewResource";
            this.BtnCreateNewResource.UseVisualStyleBackColor = true;
            this.BtnCreateNewResource.Click += new System.EventHandler(this.BtnCreateNewResource_Click);
            // 
            // BtnModifyResource
            // 
            this.BtnModifyResource.Location = new System.Drawing.Point(1122, 90);
            this.BtnModifyResource.Name = "BtnModifyResource";
            this.BtnModifyResource.Size = new System.Drawing.Size(132, 20);
            this.BtnModifyResource.TabIndex = 17;
            this.BtnModifyResource.Text = "ModifyResource";
            this.BtnModifyResource.UseVisualStyleBackColor = true;
            this.BtnModifyResource.Click += new System.EventHandler(this.BtnModifyResource_Click);
            // 
            // BtnRollbackResource
            // 
            this.BtnRollbackResource.Location = new System.Drawing.Point(1122, 116);
            this.BtnRollbackResource.Name = "BtnRollbackResource";
            this.BtnRollbackResource.Size = new System.Drawing.Size(132, 20);
            this.BtnRollbackResource.TabIndex = 18;
            this.BtnRollbackResource.Text = "RollbackResource";
            this.BtnRollbackResource.UseVisualStyleBackColor = true;
            this.BtnRollbackResource.Click += new System.EventHandler(this.BtnRollbackResource_Click);
            // 
            // BtnDeleteResource
            // 
            this.BtnDeleteResource.Location = new System.Drawing.Point(1122, 142);
            this.BtnDeleteResource.Name = "BtnDeleteResource";
            this.BtnDeleteResource.Size = new System.Drawing.Size(132, 20);
            this.BtnDeleteResource.TabIndex = 19;
            this.BtnDeleteResource.Text = "DeleteResource";
            this.BtnDeleteResource.UseVisualStyleBackColor = true;
            this.BtnDeleteResource.Click += new System.EventHandler(this.BtnDeleteResource_Click);
            // 
            // BtnGetVersion
            // 
            this.BtnGetVersion.Location = new System.Drawing.Point(1122, 196);
            this.BtnGetVersion.Name = "BtnGetVersion";
            this.BtnGetVersion.Size = new System.Drawing.Size(132, 20);
            this.BtnGetVersion.TabIndex = 20;
            this.BtnGetVersion.Text = "CheckoutVersion";
            this.BtnGetVersion.UseVisualStyleBackColor = true;
            this.BtnGetVersion.Click += new System.EventHandler(this.BtnGetVersion_Click);
            // 
            // BtnGetCurrentVersion
            // 
            this.BtnGetCurrentVersion.Location = new System.Drawing.Point(1122, 222);
            this.BtnGetCurrentVersion.Name = "BtnGetCurrentVersion";
            this.BtnGetCurrentVersion.Size = new System.Drawing.Size(132, 20);
            this.BtnGetCurrentVersion.TabIndex = 21;
            this.BtnGetCurrentVersion.Text = "CheckoutCurrentVersion";
            this.BtnGetCurrentVersion.UseVisualStyleBackColor = true;
            this.BtnGetCurrentVersion.Click += new System.EventHandler(this.BtnGetCurrentVersion_Click);
            // 
            // BtnCreateNewVersion
            // 
            this.BtnCreateNewVersion.Location = new System.Drawing.Point(1122, 248);
            this.BtnCreateNewVersion.Name = "BtnCreateNewVersion";
            this.BtnCreateNewVersion.Size = new System.Drawing.Size(132, 20);
            this.BtnCreateNewVersion.TabIndex = 22;
            this.BtnCreateNewVersion.Text = "CreateNewVersion";
            this.BtnCreateNewVersion.UseVisualStyleBackColor = true;
            this.BtnCreateNewVersion.Click += new System.EventHandler(this.BtnCreateNewVersion_Click);
            // 
            // BtnModifyVersion
            // 
            this.BtnModifyVersion.Location = new System.Drawing.Point(1122, 274);
            this.BtnModifyVersion.Name = "BtnModifyVersion";
            this.BtnModifyVersion.Size = new System.Drawing.Size(132, 20);
            this.BtnModifyVersion.TabIndex = 23;
            this.BtnModifyVersion.Text = "ModifyVersion";
            this.BtnModifyVersion.UseVisualStyleBackColor = true;
            this.BtnModifyVersion.Click += new System.EventHandler(this.BtnModifyVersion_Click);
            // 
            // BtnGetGlobalPermissions
            // 
            this.BtnGetGlobalPermissions.Location = new System.Drawing.Point(545, 486);
            this.BtnGetGlobalPermissions.Name = "BtnGetGlobalPermissions";
            this.BtnGetGlobalPermissions.Size = new System.Drawing.Size(132, 20);
            this.BtnGetGlobalPermissions.TabIndex = 25;
            this.BtnGetGlobalPermissions.Text = "GetGlobalPermissions";
            this.BtnGetGlobalPermissions.UseVisualStyleBackColor = true;
            this.BtnGetGlobalPermissions.Click += new System.EventHandler(this.BtnGetGlobalPermissions_Click);
            // 
            // BtnModifyGlobalPermissions
            // 
            this.BtnModifyGlobalPermissions.Location = new System.Drawing.Point(683, 486);
            this.BtnModifyGlobalPermissions.Name = "BtnModifyGlobalPermissions";
            this.BtnModifyGlobalPermissions.Size = new System.Drawing.Size(132, 20);
            this.BtnModifyGlobalPermissions.TabIndex = 26;
            this.BtnModifyGlobalPermissions.Text = "ModifyGlobalPermissions";
            this.BtnModifyGlobalPermissions.UseVisualStyleBackColor = true;
            this.BtnModifyGlobalPermissions.Click += new System.EventHandler(this.BtnModifyGlobalPermissions_Click);
            // 
            // BtnGetResourceUsageRightsTemplate
            // 
            this.BtnGetResourceUsageRightsTemplate.Location = new System.Drawing.Point(855, 486);
            this.BtnGetResourceUsageRightsTemplate.Name = "BtnGetResourceUsageRightsTemplate";
            this.BtnGetResourceUsageRightsTemplate.Size = new System.Drawing.Size(186, 20);
            this.BtnGetResourceUsageRightsTemplate.TabIndex = 27;
            this.BtnGetResourceUsageRightsTemplate.Text = "GetResourceUsageRightsTemplate";
            this.BtnGetResourceUsageRightsTemplate.UseVisualStyleBackColor = true;
            this.BtnGetResourceUsageRightsTemplate.Click += new System.EventHandler(this.BtnGetResourceUsageRightsTemplate_Click);
            // 
            // BtnModifyResourceUsageRightsTemplate
            // 
            this.BtnModifyResourceUsageRightsTemplate.Location = new System.Drawing.Point(1047, 486);
            this.BtnModifyResourceUsageRightsTemplate.Name = "BtnModifyResourceUsageRightsTemplate";
            this.BtnModifyResourceUsageRightsTemplate.Size = new System.Drawing.Size(207, 20);
            this.BtnModifyResourceUsageRightsTemplate.TabIndex = 28;
            this.BtnModifyResourceUsageRightsTemplate.Text = "ModifyResourceUsageRightsTemplate";
            this.BtnModifyResourceUsageRightsTemplate.UseVisualStyleBackColor = true;
            this.BtnModifyResourceUsageRightsTemplate.Click += new System.EventHandler(this.BtnModifyResourceUsageRightsTemplate_Click);
            // 
            // BtnSearch
            // 
            this.BtnSearch.Location = new System.Drawing.Point(1122, 327);
            this.BtnSearch.Name = "BtnSearch";
            this.BtnSearch.Size = new System.Drawing.Size(132, 20);
            this.BtnSearch.TabIndex = 29;
            this.BtnSearch.Text = "Search";
            this.BtnSearch.UseVisualStyleBackColor = true;
            this.BtnSearch.Click += new System.EventHandler(this.BtnSearch_Click);
            // 
            // FrmMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1261, 512);
            this.Controls.Add(this.BtnSearch);
            this.Controls.Add(this.BtnModifyResourceUsageRightsTemplate);
            this.Controls.Add(this.BtnGetResourceUsageRightsTemplate);
            this.Controls.Add(this.BtnModifyGlobalPermissions);
            this.Controls.Add(this.BtnGetGlobalPermissions);
            this.Controls.Add(this.BtnModifyVersion);
            this.Controls.Add(this.BtnCreateNewVersion);
            this.Controls.Add(this.BtnGetCurrentVersion);
            this.Controls.Add(this.BtnGetVersion);
            this.Controls.Add(this.BtnDeleteResource);
            this.Controls.Add(this.BtnRollbackResource);
            this.Controls.Add(this.BtnModifyResource);
            this.Controls.Add(this.BtnCreateNewResource);
            this.Controls.Add(this.BtnGetResourceReadOnly);
            this.Controls.Add(this.BtnGetResource);
            this.Controls.Add(this.BtnModifyUser);
            this.Controls.Add(this.BtnCreateUser);
            this.Controls.Add(this.BtnGetUser);
            this.Controls.Add(this.BtnGetAllUsers);
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
        private System.Windows.Forms.Button BtnGetAllUsers;
        private System.Windows.Forms.Button BtnModifyUser;
        private System.Windows.Forms.Button BtnCreateUser;
        private System.Windows.Forms.Button BtnGetUser;
        private System.Windows.Forms.Button BtnGetResource;
        private System.Windows.Forms.Button BtnGetResourceReadOnly;
        private System.Windows.Forms.Button BtnCreateNewResource;
        private System.Windows.Forms.Button BtnModifyResource;
        private System.Windows.Forms.Button BtnRollbackResource;
        private System.Windows.Forms.Button BtnDeleteResource;
        private System.Windows.Forms.Button BtnGetVersion;
        private System.Windows.Forms.Button BtnGetCurrentVersion;
        private System.Windows.Forms.Button BtnCreateNewVersion;
        private System.Windows.Forms.Button BtnModifyVersion;
        private System.Windows.Forms.Button BtnGetGlobalPermissions;
        private System.Windows.Forms.Button BtnModifyGlobalPermissions;
        private System.Windows.Forms.Button BtnGetResourceUsageRightsTemplate;
        private System.Windows.Forms.Button BtnModifyResourceUsageRightsTemplate;
        private System.Windows.Forms.Button BtnSearch;
    }
}

