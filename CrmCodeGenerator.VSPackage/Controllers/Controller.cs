#region Imports

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Xrm.Sdk;
using WebResourceLinkerExt.VSPackage;
using WebResourceLinkerExt.VSPackage.Helpers;

#endregion

namespace WebResourceLinker
{
	public class Controller
	{
		public Controller()
		{
		}

		private readonly WebResourceLinkerExt_VSPackagePackage vsAddin;
		// this is used for writing back to the output window and setting flags to control single publish operations

		public Controller(WebResourceLinkerExt_VSPackagePackage vsAddin)
		{
			this.vsAddin = vsAddin;
		}

		internal void TryLinkOrPublish(string linkerDataPath, List<SelectedFile> selectedFiles, bool relinking)
		{
			var linked = LinkerData.Get(linkerDataPath);

			var message = relinking ? "Initializing re-link on: {0}" : "Initializing link/publish on: {0}";
			Trace(message, linked.PublicUrl);

			var wrp = new WebResourcePublisher
			          {
				          Relink = relinking,
				          Controller = this,
				          LinkerDataPath = linkerDataPath,
				          SelectedFiles = selectedFiles
			          };
			// setting this will cause the wrp to mark the 1st file in selectedfiles to be relinked

			Task.Factory.StartNew(() =>
			                      {
				                      Trace("Connecting...");
				                      Status.Update("Connecting ... ", false);

				                      var publicUrl = "";
				                      IOrganizationService sdk = null;

				                      try
				                      {
					                      sdk = QuickConnection.Connect(linked.DiscoveryUrl, linked.Domain, linked.Username,
						                      linked.Password, out publicUrl);
					                      Status.Update("done!");
				                      }
				                      catch (Exception ex)
				                      {
					                      Status.Update("");
					                      Status.Update($"Connection failed: {ex.Message}");
					                      Trace("Connection failed: {0}", ex.Message);
				                      }

				                      return new object[] {sdk, publicUrl};
			                      }).ContinueWith(state =>
			                                      {
				                                      try
				                                      {
					                                      if (state?.Result?.FirstOrDefault() == null)
					                                      {
						                                      Status.Update("");
						                                      Status.Update("ERROR: couldn't connect to CRM.");
						                                      Trace("ERROR: couldn't connect to CRM.");

						                                      wrp.Relink = false;
						                                      wrp.ShowConnectionWindow = true;
						                                      wrp.Initialize();
															  
															  return;
					                                      }

					                                      var result = state.Result;

					                                      var sdk = (IOrganizationService) result[0];
					                                      wrp.Sdk = sdk;
					                                      wrp.PublicUrl = result[1].ToString();
					                                      wrp.ShowConnectionWindow = wrp.Sdk == null;

					                                      wrp.Initialize();
					                                      wrp.TryPublishing();
				                                      }
				                                      catch (Exception ex)
				                                      {
					                                      Status.Update("");
					                                      Status.Update($"ERROR: {ex.Message}");
					                                      Trace("ERROR: {0}", ex.Message);
				                                      }
			                                      }, TaskScheduler.FromCurrentSynchronizationContext());
		}

		internal static void SaveConnectionDetails(string linkerDataPath, string url, string domain,
			string username, string password, string publicUrl)
		{
			try
			{
				var existing = LinkerData.Get(linkerDataPath);

				existing.DiscoveryUrl = url;
				existing.Domain = domain;
				existing.Username = username;
				existing.Password = password; // don't worry the password is encrypted :)
				existing.PublicUrl = publicUrl;

				existing.Save(linkerDataPath);
			}
			catch (Exception ex)
			{
				MessageBox.Show($"Unable to save connection details. Error: {ex.Message}",
					"ERROR!", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}

		public void Trace(string format, params object[] args)
		{
			vsAddin?.WriteToOutputWindow(format, args);
		}
	}
}
