#region Imports

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using WebResourceLinkerExt.Properties;
using WebResourceLinkerExt.VSPackage.Helpers;

#endregion

namespace WebResourceLinker
{
	public partial class WebResourcePublisher : Form
	{
		#region multi-threading stuff

		private void UpdateStatus(string msg)
		{
			if (status.InvokeRequired)
			{
				status.BeginInvoke(new MethodInvoker(delegate { statusmsg.Text = msg; }));
			}
			else
			{
				statusmsg.Text = msg;
			}
		}

		private void ToggleControl(Control c, bool enabled)
		{
			if (c.InvokeRequired)
			{
				c.BeginInvoke(new MethodInvoker(delegate { c.Enabled = enabled; }));
			}
			else
			{
				c.Enabled = enabled;
			}
		}

		private void AddTreeNode(string key, string text, int imageIndex)
		{
			if (webresources.InvokeRequired)
			{
				webresources.BeginInvoke(
					new MethodInvoker(delegate { webresources.Nodes.Add(key, text, imageIndex, imageIndex); }));
			}
			else
			{
				webresources.Nodes.Add(key, text, imageIndex, imageIndex);
			}

			if (text.ToLower().Contains(".js"))
			{
				if (webresources.InvokeRequired)
				{
					webresources.BeginInvoke(
						new MethodInvoker(() => webresources.Nodes[key].ExpandAll()));
				}
				else
				{
					webresources.Nodes[key].ExpandAll();
				}
			}
		}

		private void AddTreeNode(string key, TreeNode node)
		{
			if (webresources.InvokeRequired)
			{
				webresources.BeginInvoke(new MethodInvoker(delegate { webresources.Nodes[key].Nodes.Add(node); }));
			}
			else
			{
				webresources.Nodes[key].Nodes.Add(node);
			}
		}

		#endregion

		public Controller Controller { get; set; }

		public bool ShowConnectionWindow { get; set; }
		public string LinkerDataPath { get; set; }
		public List<SelectedFile> SelectedFiles { get; set; }
		public SelectedFile UnmappedFile { get; set; }
		private readonly Semaphore unmappedLock = new Semaphore(1, 1);
		public int? UnMappedCount { get; set; }
		public List<SelectedFile> PublishedFiles { get; set; }

		public bool Relink { get; set; } // re-linking a file requires few extra operations so this flag needs to be set

		public WebResourcePublisher()
		{
			InitializeComponent();
		}

		public IOrganizationService Sdk { get; set; }
		public string PublicUrl { get; set; }
		// this is so when 'create new web resource' is clicked we can pop crm2011 web resource window

		private readonly string[] typeMapping =
		{
			".htm, .html", ".css", ".js", ".xml", ".png", ".jpg", ".gif", ".xap", ".xsl, .xslt",
			".ico"
		};

		private readonly int[] typeImageMapping = {0, 1, 2, 3, 4, 4, 4, 5, 3, 4};
		// maps the above idexes with the _treeImages list index below

		private readonly ImageList treeImages = new ImageList();

		private bool publishing; // if we're publishing don't enable the controls
		private bool isFormClosing;

		private void WebResourcePublisher_Load(object sender, EventArgs e)
		{ }

		private void linkorpublish_Click(object sender, EventArgs e)
		{
			TryPublishing();
		}


		private void connect_Click(object sender, EventArgs e)
		{
			ShowConnectionDialog();
		}

		private void createnew_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			if (string.IsNullOrEmpty(PublicUrl))
			{
				MessageBox.Show("Please click on 'Connect to different org.' and try again",
					"ERROR!", MessageBoxButtons.OK, MessageBoxIcon.Error);
				return;
			}

			// note: some urls won't work, you need to have the correct web urls set inside crm deployment manager, eg: http://server is different to http://server.fqdn.com
			Process.Start($"{PublicUrl}{(!PublicUrl.EndsWith("/") ? "/" : "")}main.aspx?etc=9333&pagetype=webresourceedit");
		}

		private void refresh_Click(object sender, EventArgs e)
		{
			if (Sdk != null)
			{
				ShowExistingWebResources();
			}
			else
			{
				ShowConnectionDialog();
			}
		}

		public void Initialize()
		{
			UnmappedFile = null;
			PublishedFiles = new List<SelectedFile>();

			LoadImages();

			if (ShowConnectionWindow)
			{
				ShowConnectionDialog();
			}
			else
			{
				ShowExistingWebResources();
			}

			// when this method is called while relink is set it wont attempt to publish anything else other than the 1st file inside selectedfiles array
			if (Relink)
			{
				unmappedLock.WaitOne();
				MarkAsUnmapped(SelectedFiles[0]);
			}
		}

		private void ShowConnectionDialog()
		{
			var cc = new CrmConnection {LinkerDataPath = LinkerDataPath};

			if (cc.ShowDialog() == DialogResult.OK)
			{
				Sdk = cc.Sdk;
				PublicUrl = cc.PublicUrl;

				ShowExistingWebResources();
			}
			else
			{
				ToggleControl(connect, true);
			}
		}

		private void LoadImages()
		{
			try
			{
				treeImages.Images.Add(new Icon(Resources._16x16_html, 16, 16).ToBitmap());
				treeImages.Images.Add(new Icon(Resources._16x16_css, 16, 16).ToBitmap());
				treeImages.Images.Add(new Icon(Resources._16x16_js, 16, 16).ToBitmap());
				treeImages.Images.Add(new Icon(Resources._16x16_xml, 16, 16).ToBitmap());
				treeImages.Images.Add(new Icon(Resources._16x16_images, 16, 16).ToBitmap());
				treeImages.Images.Add(new Icon(Resources._16x16_silverlight, 16, 16).ToBitmap());

				webresources.ImageList = treeImages;
			}
			catch (Exception ex)
			{
				Controller.Trace("Failed to load images: {0}", ex.ToString());
			}
		}

		public void TryPublishing()
		{
			try
			{
				ToggleControls(false);

				if (Sdk != null)
				{
					var existing = LinkerData.Get(LinkerDataPath);

					var isFoundUnmapped = false;

					if (!Relink && UnmappedFile == null)
					{
						var alreadyMappednMatching = SelectedFiles.Where(selectedFile =>
																		 {
																			 // grab correctly mapped files and publish them
																			 return existing.Mappings
																				 .Any(
																					 a =>
																						 a.SourceFilePath.Equals(selectedFile.FriendlyFilePath,
																							 StringComparison.InvariantCultureIgnoreCase))
																				 &&
																				 // make sure we don't attempt to publish already published items
																				 !PublishedFiles.Any(
																					 a =>
																						 a.FilePath.Equals(selectedFile.FilePath,
																							 StringComparison.InvariantCultureIgnoreCase));
																		 }).ToList();

						UnMappedCount = UnMappedCount ?? SelectedFiles
							.Count(selectedFile =>
								   {
									   return existing.Mappings.Where(
										   a =>
											   a.SourceFilePath.Equals(selectedFile.FriendlyFilePath,
												   StringComparison.InvariantCultureIgnoreCase))
										   .Take(1).SingleOrDefault() == null;
								   });

						if (alreadyMappednMatching.Count > 0)
						{
							PublishMappedResources(existing, alreadyMappednMatching);
						}

						// now find the unmapped files and mark them as unmapped, when the 'publish/link' button is clicked this unmapped file will be picked up by the same method, see 'if this.unmappedfile != null' check above
						foreach (var selectedFile in SelectedFiles)
						{
							if (isFormClosing)
							{
								break;
							}

							var matchingItem = existing.Mappings
								.Where(
									a =>
										a.SourceFilePath.Equals(selectedFile.FriendlyFilePath,
											StringComparison.InvariantCultureIgnoreCase))
								.Take(1).SingleOrDefault();

							if (matchingItem == null)
							{
								unmappedLock.WaitOne();

								if (isFormClosing)
								{
									break;
								}

								UnMappedCount--;
								MarkAsUnmapped(selectedFile);
								isFoundUnmapped = true;
							}
						}
					}

					if (UnmappedFile != null && Visible)
						// this property gets set if relink is set to true, otherwise this gets set by the logic below if there is no mapping
					{
						var existing1 = existing;
						BeginInvoke(new MethodInvoker(() =>
													  {
														  try
														  {
															  PublishUnmappedResource(existing1);
															  existing.Save(LinkerDataPath);
														  }
														  catch (Exception ex)
														  {
															  Status.Update("");
															  Status.Update($"ERROR: {ex.Message}");
															  Controller.Trace("ERROR: {0}", ex.Message);
															  ToggleControl(connect, true);
																  // enable the connect button so the user can try connecting to a different org
														  }
													  }));
						return;
					}

					if (isFoundUnmapped)
					{
						existing = LinkerData.Get(LinkerDataPath);
					}

					existing.Save(LinkerDataPath);
					// save the mappings file regardless of the state of the publish since content would've been updated already
				}
				else
				{
					ToggleControl(connect, true); // enable the connect button so the user can try connecting to a different org
				}
			}
			catch (Exception ex)
			{
				Status.Update("");
				Status.Update($"ERROR: {ex.Message}");
				Controller.Trace("ERROR: {0}", ex.Message);
				ToggleControl(connect, true); // enable the connect button so the user can try connecting to a different org
			}
		}

		private void MarkAsUnmapped(SelectedFile selectedFile)
		{
			UnmappedFile = selectedFile;

			currentmapping.Text = $"Please map and publish: {selectedFile.FriendlyFilePath}";
			ToggleControls(true);

			try
			{
				ShowDialog(); // need to show the ui so the user can pick the web resource to map with
			}
			catch
			{
				try
				{
					Show();
				}
				catch
				{
					// ignored
				}
			}

			Activate();
		}

		private void PublishMappedResources(LinkerData existing, List<SelectedFile> alreadyMappednMatching)
		{
			try
			{
				var webresourceIds = new Guid[alreadyMappednMatching.Count];
				var filePaths = new string[alreadyMappednMatching.Count];

				for (var i = 0; i < alreadyMappednMatching.Count; i++)
				{
					// bit of logic to figure out the webresourceid of the file basd on the filepath
					webresourceIds[i] = existing.Mappings
						.Where(
							a =>
								a.SourceFilePath.Equals(alreadyMappednMatching[i].FriendlyFilePath, StringComparison.InvariantCultureIgnoreCase))
						.Select(a => a.WebResourceId).Take(1).SingleOrDefault();

					filePaths[i] = alreadyMappednMatching[i].FilePath;
				}

				Publish(webresourceIds, filePaths, alreadyMappednMatching);
			}
			catch (Exception ex)
			{
				Status.Update("");
				Status.Update($"ERROR: {ex.Message}");
				Controller.Trace("ERROR: {0}", ex.Message);
				ToggleControl(connect, true); // enable the connect button so the user can try connecting to a different org
			}
		}

		private void PublishUnmappedResource(LinkerData existing)
		{
			try
			{
				if (webresources.SelectedNode?.Tag != null)
				{
					// tag is only set internally so we should be ok without an error check here :P
					var webresourceId = new Guid(webresources.SelectedNode.Tag.ToString());

					// get rid of anything that's mapped with this sourcefile/webresourceid and add a clean one to avoid corruption on the mapping file
					existing.Mappings.RemoveAll(a => a.WebResourceId == webresourceId
						// check both the id and sourcepath because we now have to support re-linking
						||
						a.SourceFilePath.Equals(UnmappedFile.FriendlyFilePath,
							StringComparison.InvariantCultureIgnoreCase));

					// add a clean mapping for this webresource and file
					existing.Mappings.Add(new LinkerDataItem
										  {
											  WebResourceId = webresourceId,
											  SourceFilePath = UnmappedFile.FriendlyFilePath
										  });

					Publish(new[] {webresourceId}, new[] {UnmappedFile.FilePath}, new List<SelectedFile>(new[] {UnmappedFile}));
					UnmappedFile = null;

					currentmapping.Text = "";
				}
			}
			catch (Exception ex)
			{
				Status.Update("");
				Status.Update($"ERROR: {ex.Message}");
				Controller.Trace("ERROR: {0}", ex.Message);
				ToggleControl(connect, true); // enable the connect button so the user can try connecting to a different org
			}
			finally
			{
				unmappedLock.Release();
			}
		}

		private void Publish(Guid[] webresourceIds, string[] filePaths, List<SelectedFile> files)
		{
			try
			{
				publishing = true;
				ToggleControls(false);

				Status.Update($"Attempting to publish: {string.Join("; ", filePaths.Select(Path.GetFileName))}");
				Controller.Trace("Attempting to publish: {0}", string.Join("; ", filePaths.Select(Path.GetFileName)));

				var base64Content = new string[webresourceIds.Length];
				for (var i = 0; i < webresourceIds.Length; i++)
				{
					base64Content[i] = Convert.ToBase64String(File.ReadAllBytes(filePaths[i]));
				}

				var resultUpdate = UpdateContent(webresourceIds, base64Content);
				var resultPublish = BeginPublish(resultUpdate);

				PublishedFiles.AddRange(files);

				EndPublish(resultPublish);
			}
			catch (Exception ex)
			{
				Status.Update("");
				Status.Update($"ERROR: {ex.Message}");
				Controller.Trace("ERROR: {0}", ex.Message);
				ToggleControl(connect, true); // enable the connect button so the user can try connecting to a different org
			}
		}

		private Guid[] UpdateContent(Guid[] webresourceIds, string[] base64Contents)
		{
			UpdateStatus("Updating content...");

			for (var i = 0; i < webresourceIds.Length; i++)
			{
				var resource = new Entity("webresource")
							   {
								   ["webresourceid"] = webresourceIds[i],
								   ["content"] = base64Contents[i]
							   };

				try
				{
					Sdk.Update(resource);
					Controller.Trace("Updated: {0}", webresourceIds[i]);
				}
				catch (Exception ex)
				{
					MessageBox.Show($"Exception: {ex.GetType().Name} => {ex.Message}", "ERROR!",
						MessageBoxButtons.OK, MessageBoxIcon.Error);
					return new Guid[0];
				}
			}

			return webresourceIds;
		}

		private Guid[] BeginPublish(Guid[] webresourceIds)
		{
			if (!webresourceIds.Any())
			{
				return new Guid[0];
			}

			UpdateStatus("Publishing...");

			var request = new OrganizationRequest
						  {
							  RequestName = "PublishXml",
							  Parameters = new ParameterCollection
										   {
											   new KeyValuePair<string, object>("ParameterXml",
												   "<importexportxml><webresources>" +
													   $"{string.Join("", webresourceIds.Select(a => $"<webresource>{a}</webresource>"))}" +
													   "</webresources></importexportxml>")
										   }
						  };

			try
			{
				Sdk.Execute(request);
			}
			catch (Exception ex)
			{
				MessageBox.Show($"Exception: {ex.GetType().Name} => {ex.Message}", "ERROR!",
					MessageBoxButtons.OK, MessageBoxIcon.Error);
				return new Guid[0];
			}

			return webresourceIds;
		}

		private void EndPublish(Guid[] webresourceIds)
		{
			try
			{
				var percentComplete = (PublishedFiles.Count / (decimal) SelectedFiles.Count) * 100m;
				var msg = $"Published: {PublishedFiles.Count} of {SelectedFiles.Count} ({percentComplete:N0}%)";
				Status.Update(msg);

				Controller.Trace("Published: {0}", string.Join("; ", webresourceIds));
				UpdateStatus(msg);
				Controller.Trace(msg);

				publishing = false;
				ToggleControls(true);

				var fileListString =
					PublishedFiles.Select(file => file.FriendlyFilePath).Aggregate((file1, file2) => file1 + "\n" + file2);
				var confirmationMessage = "Files:\n" + fileListString;

				if (UnMappedCount > 0)
				{
					if (Visible)
					{
						Invoke(new MethodInvoker(Hide));
					}
					return;
				}

				// once all the selected files are published we can close the dialog & show success message
				if (SelectedFiles.Count == PublishedFiles.Count && webresourceIds.Any())
				{
					if (InvokeRequired)
					{
						BeginInvoke(new MethodInvoker(delegate
													  {
														  Status.Update("Succeeded ... \n" + confirmationMessage);
														  MessageBox.Show(confirmationMessage, "Successfully published files.",
															  MessageBoxButtons.OK, MessageBoxIcon.Information);

														  Close();
													  }));
					}
					else
					{
						Status.Update("Succeeded ... \n" + confirmationMessage);
						MessageBox.Show(confirmationMessage, "Successfully published files.", MessageBoxButtons.OK,
							MessageBoxIcon.Information);

						if (Visible)
						{
							Invoke(new MethodInvoker(Close));
						}
					}
				}
				else
				{
					confirmationMessage = "Files:\n" + fileListString;

					if (InvokeRequired)
					{
						BeginInvoke(new MethodInvoker(delegate
													  {
														  Status.Update("Failed ... \n" + confirmationMessage);
														  MessageBox.Show(confirmationMessage, "Failed to publish files!",
															  MessageBoxButtons.OK, MessageBoxIcon.Error);

														  Close();
													  }));
					}
					else
					{
						Status.Update("Failed ... \n" + confirmationMessage);
						MessageBox.Show(confirmationMessage, "Failed to publish files!", MessageBoxButtons.OK, MessageBoxIcon.Error);

						if (Visible)
						{
							Invoke(new MethodInvoker(Close));
						}
					}
				}
			}
			catch (Exception ex)
			{
				Status.Update("");
				Status.Update($"ERROR: {ex.Message}");
				Controller.Trace("ERROR: {0}", ex.Message);
				ToggleControl(connect, true); // enable the connect button so the user can try connecting to a different org
			}
		}

		private delegate List<Entity> RetrieveWebResourceHandler();

		// todo: join with the solution and group by solution name in the tree
		private void ShowExistingWebResources()
		{
			try
			{
				ToggleControls(false);

				UpdateStatus("Loading web resources...");
				webresources.Nodes.Clear();

				var rwrh = new RetrieveWebResourceHandler(BeginShowExistingWebResources);
				var callback = new AsyncCallback(EndShowExistingWebResources);

				rwrh.BeginInvoke(callback, rwrh);
			}
			catch (Exception ex)
			{
				Status.Update("");
				Status.Update($"ERROR: {ex.Message}");
				Controller.Trace("ERROR: {0}", ex.Message);
				ToggleControl(connect, true); // enable the connect button so the user can try connecting to a different org
			}
		}

		private List<Entity> BeginShowExistingWebResources()
		{
			try
			{
				var qe = new QueryExpression("webresource")
						 {
							 ColumnSet =
								 new ColumnSet("webresourceid", "webresourcetype", "name", "displayname")
						 };
				qe.Criteria.AddCondition("ismanaged", ConditionOperator.Equal, false);
				qe.Criteria.AddCondition("iscustomizable", ConditionOperator.Equal, true);
				qe.AddOrder("name", OrderType.Ascending);

				return Sdk.RetrieveMultiple(qe).Entities.ToList();
			}
			catch (Exception ex)
			{
				Status.Update("");
				Status.Update($"ERROR: {ex.Message}");
				Controller.Trace("ERROR: {0}", ex.Message);
				ToggleControl(connect, true); // enable the connect button so the user can try connecting to a different org
			}

			return new List<Entity>();
		}

		private void EndShowExistingWebResources(IAsyncResult result)
		{
			try
			{
				var rwrh = (RetrieveWebResourceHandler) result.AsyncState;
				var results = rwrh.EndInvoke(result);

				Controller.Trace("Found {0} web resource(s)", results.Count);

				foreach (var a in results
					.GroupBy(a => a.GetAttributeValue<OptionSetValue>("webresourcetype").Value)
					.OrderByDescending(a => typeMapping[a.Key - 1].ToLower().Contains(".js"))
					.ThenBy(a => a.Key))
				{
					var key = a.Key.ToString();
					var imageIndex = typeImageMapping[a.Key - 1];

					AddTreeNode(key, typeMapping[a.Key - 1], imageIndex);

					foreach (var r in a)
					{
						var tn = new TreeNode(r.GetAttributeValue<string>("name"), imageIndex, imageIndex)
								 {
									 Tag = r.GetAttributeValue<Guid>("webresourceid")
								 };

						AddTreeNode(key, tn);
					}
				}

				UpdateStatus($"{results.Count} web resources loaded");

				ToggleControls(true);
			}
			catch (Exception ex)
			{
				Status.Update("");
				Status.Update($"ERROR: {ex.Message}");
				Controller.Trace("ERROR: {0}", ex.Message);
				ToggleControl(connect, true); // enable the connect button so the user can try connecting to a different org
			}
		}

		private void ToggleControls(bool enable)
		{
			enable = enable && !publishing; // only enable the buttons if we're not in a publishing state

			ToggleControl(linkorpublish, enable);
			ToggleControl(connect, enable);
			ToggleControl(createnew, enable);
			ToggleControl(refresh, enable);
		}

		private void WebResourcePublisher_FormClosing(object sender, FormClosingEventArgs e)
		{
			try
			{
				if (DialogResult == DialogResult.Cancel)
				{
					isFormClosing = true;

					if (unmappedLock.WaitOne(0))
					{
						unmappedLock.Release();
					}
				}
			}
			catch (Exception ex)
			{
				Status.Update("");
				Status.Update($"ERROR: {ex.Message}");
				Controller.Trace("ERROR: {0}", ex.Message);
				ToggleControl(connect, true); // enable the connect button so the user can try connecting to a different org
			}
		}
	}
}
