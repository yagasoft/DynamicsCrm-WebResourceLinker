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
			this.label1.Size = new System.Drawing.Size(101, 13);
			this.label1.TabIndex = 0;
			this.label1.Text = "Connection String";
			// 
			// discourl
			// 
			this.discourl.Location = new System.Drawing.Point(12, 29);
			this.discourl.Name = "discourl";
			this.discourl.Size = new System.Drawing.Size(541, 22);
			this.discourl.TabIndex = 1;
			// 
			// connect
			// 
			this.connect.Location = new System.Drawing.Point(463, 57);
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
			this.status.Location = new System.Drawing.Point(0, 99);
			this.status.Name = "status";
			this.status.Size = new System.Drawing.Size(565, 22);
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
			this.ClientSize = new System.Drawing.Size(565, 121);
			this.Controls.Add(this.status);
			this.Controls.Add(this.connect);
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
        private System.Windows.Forms.Button connect;
        private System.Windows.Forms.StatusStrip status;
        private System.Windows.Forms.ToolStripStatusLabel statusmsg;
    }
}