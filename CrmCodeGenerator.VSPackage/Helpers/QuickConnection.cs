#region Imports

using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text.RegularExpressions;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Tooling.Connector;
using WebResourceLinkerExt.VSPackage.Helpers;
using Yagasoft.Libraries.Common;

#endregion

namespace WebResourceLinker
{
	public class QuickConnection
	{
		private static readonly IDictionary<string, IOrganizationService> serviceCache =
			new Dictionary<string, IOrganizationService>();

		public static IOrganizationService Connect(string connectionString, out string publicUrl)
		{
			if (connectionString == null)
			{
				publicUrl = null;
				return null;
			}

			var stringParts = connectionString.Split(';').Select(e => e.Split('=')).Where(e => e.Length == 2)
				.ToDictionary(e => e[0].Trim().ToLower(), e => e[1].Trim());

			var url = stringParts.FirstOrDefault(pair => pair.Key == "url").Value;
			publicUrl = url.Trim('/');

			var key = connectionString;

			if (serviceCache.ContainsKey(key))
			{
				return serviceCache[key];
			}

			var conn = connectionString;

			var password = stringParts.FirstOrDefault(pair => pair.Key == "password").Value;

			var escapedString = conn;

			if (password.IsFilled())
			{
				escapedString = Regex.Replace(conn, password, "********");
			}

			Status.Update($"[Connection] Getting connection ...");
			var sdk = new CrmServiceClient(conn);

			if (sdk.LastCrmException != null || !string.IsNullOrEmpty(sdk.LastCrmError))
			{
				var errorMessage = sdk.LastCrmError;
				errorMessage += (sdk.LastCrmException != null ? "\r\n" + sdk.LastCrmException.BuildExceptionMessage() : null);
				throw new ServiceActivationException($"Can't create connection to: \"{escapedString}\" due to\r\n{errorMessage}");
			}

			Status.Update($"[Connection] [Done] Connection ready.");

			return serviceCache[key] = sdk;
		}
	}
}
