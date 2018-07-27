using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace WebResourceLinker
{
    public partial class CrmConnection : Form
    {
        public CrmConnection()
        {
            InitializeComponent();
        }

        #region multi-threading stuff
        private void UpdateStatus(string msg)
        {
            if (this.status.InvokeRequired)
            {
                this.status.BeginInvoke(new MethodInvoker(delegate()
                {
                    this.statusmsg.Text = msg;
                }));
            }
            else { this.statusmsg.Text = msg; }
        }

        private void ToggleControl(Control c, bool enabled)
        {
            if (c.InvokeRequired)
            {
                c.BeginInvoke(new MethodInvoker(delegate()
                {
                    c.Enabled = enabled;
                }));
            }
            else { c.Enabled = enabled; }
        }
        #endregion

        public IOrganizationService Sdk { get; set; }
        public string PublicUrl { get; set; }
        public string LinkerDataPath { get; set; }

        private delegate IOrganizationService GetCrmConnectionHandler(string url, string domain, string username, string password);

        private void connect_Click(object sender, EventArgs e)
        {
            ToggleControl(this.connect, false);
            UpdateStatus("Connecting...");

            GetCrmConnectionHandler gcch = new GetCrmConnectionHandler(BeginGetCrmConnection);
            AsyncCallback callback = new AsyncCallback(EndGetCrmConnection);

            gcch.BeginInvoke(this.discourl.Text, this.domain.Text, this.username.Text, this.password.Text, callback, gcch);
        }

        private IOrganizationService BeginGetCrmConnection(string url, string domain, string username, string password)
        {
            IOrganizationService sdk = null;

            try
            {
                string publicUrl = "";
                sdk = QuickConnection.Connect(url, domain, username, password, out publicUrl);
                this.PublicUrl = publicUrl;

                Controller.SaveConnectionDetails(this.LinkerDataPath, url, domain, username, password, publicUrl);
            }
            catch (Exception ex)
            {
                UpdateStatus(string.Format("Connection failed: {0}", ex.Message));
            }

            return sdk;
        }

        private void EndGetCrmConnection(IAsyncResult result)
        {
            GetCrmConnectionHandler gcch = result.AsyncState as GetCrmConnectionHandler;
            IOrganizationService sdk = gcch.EndInvoke(result);

            ToggleControl(this.connect, true);

            if (sdk != null)
            {
                UpdateStatus("Connected");

                this.Sdk = sdk;

                this.DialogResult = System.Windows.Forms.DialogResult.OK;
            }
        }

        private void CrmConnection_Load(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(this.LinkerDataPath))
            {
                var data = LinkerData.Get(this.LinkerDataPath);
                if (data != null)
                {
					this.discourl.Text = string.IsNullOrEmpty(data.DiscoveryUrl) ? "" : data.DiscoveryUrl.Replace("/XRMServices/2011/Discovery.svc", "");
                    this.domain.Text = data.Domain;
                    this.username.Text = data.Username;
                    this.password.Text = data.Password;
                }
            }
        }
    }
}
