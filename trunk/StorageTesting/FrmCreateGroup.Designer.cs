namespace StorageTesting
{
    partial class FrmCreateGroup
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
            this.label1 = new System.Windows.Forms.Label();
            this.TxtGroupName = new System.Windows.Forms.TextBox();
            this.TxtGroups = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.TxtUsers = new System.Windows.Forms.TextBox();
            this.BtnCancel = new System.Windows.Forms.Button();
            this.BtnCreate = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(98, 18);
            this.label1.TabIndex = 0;
            this.label1.Text = "Group Name:";
            // 
            // TxtGroupName
            // 
            this.TxtGroupName.Location = new System.Drawing.Point(116, 6);
            this.TxtGroupName.Name = "TxtGroupName";
            this.TxtGroupName.Size = new System.Drawing.Size(248, 24);
            this.TxtGroupName.TabIndex = 1;
            this.TxtGroupName.Text = "testgroup";
            // 
            // TxtGroups
            // 
            this.TxtGroups.Location = new System.Drawing.Point(15, 64);
            this.TxtGroups.Multiline = true;
            this.TxtGroups.Name = "TxtGroups";
            this.TxtGroups.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.TxtGroups.Size = new System.Drawing.Size(349, 184);
            this.TxtGroups.TabIndex = 2;
            this.TxtGroups.Text = "administrators";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 43);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(252, 18);
            this.label2.TabIndex = 3;
            this.label2.Text = "Other groups belonging to this group:";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(370, 4);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(205, 18);
            this.label3.TabIndex = 5;
            this.label3.Text = "Users belonging to this group:";
            // 
            // TxtUsers
            // 
            this.TxtUsers.Location = new System.Drawing.Point(373, 25);
            this.TxtUsers.Multiline = true;
            this.TxtUsers.Name = "TxtUsers";
            this.TxtUsers.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.TxtUsers.Size = new System.Drawing.Size(349, 223);
            this.TxtUsers.TabIndex = 4;
            this.TxtUsers.Text = "administrator";
            // 
            // BtnCancel
            // 
            this.BtnCancel.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F);
            this.BtnCancel.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.BtnCancel.Location = new System.Drawing.Point(231, 254);
            this.BtnCancel.Name = "BtnCancel";
            this.BtnCancel.Size = new System.Drawing.Size(133, 28);
            this.BtnCancel.TabIndex = 9;
            this.BtnCancel.Text = "Cancel";
            this.BtnCancel.UseVisualStyleBackColor = true;
            this.BtnCancel.Click += new System.EventHandler(this.BtnCancel_Click);
            // 
            // BtnCreate
            // 
            this.BtnCreate.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F);
            this.BtnCreate.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.BtnCreate.Location = new System.Drawing.Point(373, 254);
            this.BtnCreate.Name = "BtnCreate";
            this.BtnCreate.Size = new System.Drawing.Size(133, 28);
            this.BtnCreate.TabIndex = 8;
            this.BtnCreate.Text = "Create";
            this.BtnCreate.UseVisualStyleBackColor = true;
            this.BtnCreate.Click += new System.EventHandler(this.BtnCreate_Click);
            // 
            // FrmCreateGroup
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 18F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(731, 289);
            this.Controls.Add(this.BtnCancel);
            this.Controls.Add(this.BtnCreate);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.TxtUsers);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.TxtGroups);
            this.Controls.Add(this.TxtGroupName);
            this.Controls.Add(this.label1);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.Name = "FrmCreateGroup";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Create Group";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox TxtGroupName;
        private System.Windows.Forms.TextBox TxtGroups;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox TxtUsers;
        private System.Windows.Forms.Button BtnCancel;
        private System.Windows.Forms.Button BtnCreate;
    }
}