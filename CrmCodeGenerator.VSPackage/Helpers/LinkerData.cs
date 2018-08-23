#region Imports

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Xml.Serialization;
using EnvDTE;
using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.VersionControl.Client;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using WebResourceLinkerExt.VSPackage.Helpers;

#endregion

namespace WebResourceLinker
{
	[Serializable]
	public class LinkerData
	{
		public string DiscoveryUrl { get; set; }
		public string Domain { get; set; }
		public string Username { get; set; }
		public string Password { get; set; }
		public string UniqueOrgName { get; set; }
		public string PublicUrl { get; set; }

		public List<LinkerDataItem> Mappings { get; set; }

		public LinkerData()
		{
			Mappings = new List<LinkerDataItem>();
		}

		public static LinkerData Get(string dataPath)
		{
			Status.Update(">> Reading settings ... ", false);

			LinkerData result = null;

			var dte = Package.GetGlobalService(typeof (SDTE)) as DTE;
			var project = dte.GetSelectedProject();
			var file = project.GetProjectDirectory() + "\\" + dataPath;

			var newLine = false;

			if (File.Exists(file))
			{
				// get latest file if in TFS
				try
				{
					var workspaceInfo = Workstation.Current.GetLocalWorkspaceInfo(file);

					if (workspaceInfo != null)
					{
						var server = new TfsTeamProjectCollection(workspaceInfo.ServerUri);
						var workspace = workspaceInfo.GetWorkspace(server);

						var pending = workspace.GetPendingChanges(new[] {file});

						if (!pending.Any())
						{
							workspace.Get(new[] { file }, VersionSpec.Latest, RecursionType.Full, GetOptions.GetAll | GetOptions.Overwrite);
							Status.Update("\n\tRetrieved latest settings file from TFS' current workspace.");
							newLine = true;
						}
					}
				}
				catch (Exception)
				{
					// ignored
				}

				using (var sr = new StreamReader(file))
				{
					result = new XmlSerializer(typeof(LinkerData)).Deserialize(sr) as LinkerData;
				}
			}

			if (result == null)
			{
				result = new LinkerData();
			}
			else
			{
				result.DiscoveryUrl = Encryption.Decrypt(result.DiscoveryUrl, Encryption.EncryptionKey);
			}

			Status.Update(newLine ? "Done reading settings!" : "done!");

			return result;
		}

		public void Save(string dataPath)
		{
			Status.Update(">> Writing settings ... ", false);

			var dte = Package.GetGlobalService(typeof (SDTE)) as DTE;
			var project = dte.GetSelectedProject();
			var file = project.GetProjectDirectory() + "\\" + dataPath;

			var newLine = false;

			if (!File.Exists(file))
			{
				File.Create(file).Dispose();
				project.ProjectItems.AddFromFile(file);
				Status.Update("");
				Status.Update("\tCreated a new settings file.");
				newLine = true;
			}

			// check out file if in TFS
			try
			{
				var workspaceInfo = Workstation.Current.GetLocalWorkspaceInfo(file);

				if (workspaceInfo != null)
				{
					var server = new TfsTeamProjectCollection(workspaceInfo.ServerUri);
					var workspace = workspaceInfo.GetWorkspace(server);

					var pending = workspace.GetPendingChanges(new[] {file});

					if (!pending.Any())
					{
						workspace.Get(new[] { file }, VersionSpec.Latest, RecursionType.Full, GetOptions.GetAll | GetOptions.Overwrite);
						Status.Update("");
						Status.Update((newLine ? "" : "\n") + "\tRetrieved latest settings file from TFS' current workspace.");

						workspace.PendEdit(file);
						Status.Update("\tChecked out settings file from TFS' current workspace.");

						newLine = true;
					}
				}
			}
			catch (Exception)
			{
				// ignored
			}

			try
			{
				DiscoveryUrl = Encryption.Encrypt(DiscoveryUrl, Encryption.EncryptionKey);

				using (var sw = new StreamWriter(file, false))
				{
					new XmlSerializer(typeof(LinkerData)).Serialize(sw, this);
				}

				Status.Update(newLine ? ">> Done writing settings!" : "done!");
			}
			catch (Exception ex)
			{
				MessageBox.Show($"Unable to save settings. Error: {ex.Message}",
					"ERROR!", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}
	}
}
