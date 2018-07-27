namespace WebResourceLinker
{
    partial class CrmConnection
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
			this.discourl = new System.Windows.Forms.TextBox();
			this.domain = new System.Windows.Forms.TextBox();
			this.label2 = new System.Windows.Forms.Label();
			this.username = new System.Windows.Forms.TextBox();
			this.label3 = new System.Windows.Forms.Label();
			this.password = new System.Windows.Forms.TextBox();
			this.label4 = new System.Windows.Forms.Label();
			this.connect = new System.Windows.Forms.Button();
			this.status = new System.Windows.Forms.StatusStrip();
			this.statusmsg = new System.Windows.Forms.ToolStripStatusLabel();
			this.status.SuspendLayout();
			this.SuspendLayout();
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(13, 13);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(254, 13);
			this.label1.TabIndex = 0;
			this.label1.Text = "Server URL (e.g. \"https://test.crm4.dynamics.com)";
			// 
			// discourl
			// 
			this.discourl.Location = new System.Drawing.Point(12, 29);
			this.discourl.Name = "discourl";
			this.discourl.Size = new System.Drawing.Size(368, 22);
			this.discourl.TabIndex = 1;
			// 
			// domain
			// 
			this.domain.Location = new System.Drawing.Point(12, 70);
			this.domain.Name = "domain";
			this.domain.Size = new System.Drawing.Size(200, 22);
			this.domain.TabIndex = 3;
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(13, 54);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(199, 13);
			this.label2.TabIndex = 2;
			this.label2.Text = "Domain (not needed for CRM Online)";
			// 
			// username
			// 
			this.username.Location = new System.Drawing.Point(12, 111);
			this.username.Name = "username";
			this.username.Size = new System.Drawing.Size(200, 22);
			this.username.TabIndex = 5;
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Location = new System.Drawing.Point(13, 95);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(58, 13);
			this.label3.TabIndex = 4;
			this.label3.Text = "Username";
			// 
			// password
			// 
			this.password.Location = new System.Drawing.Point(12, 152);
			this.password.Name = "password";
			this.password.PasswordChar = '*';
			this.password.Size = new System.Drawing.Size(200, 22);
			this.password.TabIndex = 7;
			// 
			// label4
			// 
			this.label4.AutoSize = true;
			this.label4.Location = new System.Drawing.Point(13, 136);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(56, 13);
			this.label4.TabIndex = 6;
			this.label4.Text = "Password";
			// 
			// connect
			// 
			this.connect.Location = new System.Drawing.Point(12, 185);
			this.connect.Name = "connect";
			this.connect.Size = new System.Drawing.Size(90, 30);
			this.connect.TabIndex = 10;
			this.connect.Text = "Connect";
			this.connect.UseVisualStyleBackColor = true;
			this.connect.Click += new System.EventHandler(this.connect_Click);
			// 
			// status
			// 
			this.status.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.statusmsg});
			this.status.Location = new System.Drawing.Point(0, 226);
			this.status.Name = "status";
			this.status.Size = new System.Drawing.Size(392, 22);
			this.status.SizingGrip = false;
			this.status.TabIndex = 11;
			// 
			// statusmsg
			// 
			this.statusmsg.Name = "statusmsg";
			this.statusmsg.Size = new System.Drawing.Size(16, 17);
			this.statusmsg.Text = "...";
			// 
			// CrmConnection
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(392, 248);
			this.Controls.Add(this.status);
			this.Controls.Add(this.connect);
			this.Controls.Add(this.password);
			this.Controls.Add(this.label4);
			this.Controls.Add(this.username);
			this.Controls.Add(this.label3);
			this.Controls.Add(this.domain);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.discourl);
			this.Controls.Add(this.label1);
			this.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.MaximizeBox = false;
			this.Name = "CrmConnection";
			this.ShowIcon = false;
			this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Connect to CRM";
			this.Load += new System.EventHandler(this.CrmConnection_Load);
			this.status.ResumeLayout(false);
			this.status.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox discourl;
        private System.Windows.Forms.TextBox domain;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox username;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox password;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Button connect;
        private System.Windows.Forms.StatusStrip status;
        private System.Windows.Forms.ToolStripStatusLabel statusmsg;
    }
}