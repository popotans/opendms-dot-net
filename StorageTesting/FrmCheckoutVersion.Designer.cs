namespace StorageTesting
{
    partial class FrmCheckoutVersion
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
            this.label2 = new System.Windows.Forms.Label();
            this.TxtResourceId = new System.Windows.Forms.TextBox();
            this.TxtVersionNo = new System.Windows.Forms.TextBox();
            this.BtnCancel = new System.Windows.Forms.Button();
            this.BtnGo = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(13, 21);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(95, 18);
            this.label1.TabIndex = 0;
            this.label1.Text = "Resource ID:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(13, 51);
            this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(74, 18);
            this.label2.TabIndex = 1;
            this.label2.Text = "Version #:";
            // 
            // TxtResourceId
            // 
            this.TxtResourceId.Location = new System.Drawing.Point(115, 18);
            this.TxtResourceId.Name = "TxtResourceId";
            this.TxtResourceId.Size = new System.Drawing.Size(299, 24);
            this.TxtResourceId.TabIndex = 2;
            // 
            // TxtVersionNo
            // 
            this.TxtVersionNo.Location = new System.Drawing.Point(115, 48);
            this.TxtVersionNo.Name = "TxtVersionNo";
            this.TxtVersionNo.Size = new System.Drawing.Size(299, 24);
            this.TxtVersionNo.TabIndex = 3;
            // 
            // BtnCancel
            // 
            this.BtnCancel.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F);
            this.BtnCancel.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.BtnCancel.Location = new System.Drawing.Point(72, 78);
            this.BtnCancel.Name = "BtnCancel";
            this.BtnCancel.Size = new System.Drawing.Size(133, 28);
            this.BtnCancel.TabIndex = 9;
            this.BtnCancel.Text = "Cancel";
            this.BtnCancel.UseVisualStyleBackColor = true;
            this.BtnCancel.Click += new System.EventHandler(this.BtnCancel_Click);
            // 
            // BtnGo
            // 
            this.BtnGo.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F);
            this.BtnGo.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.BtnGo.Location = new System.Drawing.Point(223, 78);
            this.BtnGo.Name = "BtnGo";
            this.BtnGo.Size = new System.Drawing.Size(133, 28);
            this.BtnGo.TabIndex = 8;
            this.BtnGo.Text = "Go";
            this.BtnGo.UseVisualStyleBackColor = true;
            this.BtnGo.Click += new System.EventHandler(this.BtnGo_Click);
            // 
            // FrmCheckoutVersion
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 18F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(426, 117);
            this.Controls.Add(this.BtnCancel);
            this.Controls.Add(this.BtnGo);
            this.Controls.Add(this.TxtVersionNo);
            this.Controls.Add(this.TxtResourceId);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.Name = "FrmCheckoutVersion";
            this.Text = "Checkout Version";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox TxtResourceId;
        private System.Windows.Forms.TextBox TxtVersionNo;
        private System.Windows.Forms.Button BtnCancel;
        private System.Windows.Forms.Button BtnGo;
    }
}