#region Imports

using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text.RegularExpressions;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Tooling.Connector;

#endregion

namespace WebResourceLinker
{
	public class QuickConnection
	{
		private static readonly IDictionary<string, IOrganizationService> serviceCache =
			new Dictionary<string, IOrganizationService>();

		private static string latestConnectionString;

		public static IOrganizationService Connect(string connectionString, out string publicUrl)
		{
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

			if (latestConnectionString != connectionString)
			{
				conn = conn.Trim(';') + "RequireNewInstance=true";
			}

			var escapedString = Regex
				.Replace(conn, stringParts.FirstOrDefault(pair => pair.Key == "password").Value, "********");

			var sdk = new CrmServiceClient(conn);

			if (sdk.LastCrmException != null || !string.IsNullOrEmpty(sdk.LastCrmError))
			{
				var errorMessage = sdk.LastCrmError
					?? (sdk.LastCrmException != null ? BuildExceptionMessage(sdk.LastCrmException) : null);
				throw new ServiceActivationException($"Can't create connection to: \"{escapedString}\" due to\r\n{errorMessage}");
			}

			latestConnectionString = connectionString;

			return serviceCache[key] = sdk;
		}

		public static string BuildExceptionMessage(Exception ex, string preMessage = null)
		{
			return (preMessage == null ? "" : preMessage + "\r\n") +
				"Exception: " + ex.GetType() +
				"\r\nMessage: " + ex.Message +
				(ex.Source == null ? "" : "\r\nSource: " + ex.Source) +
				(ex.StackTrace == null
					? ""
					: "\r\nStack trace:\r\n" + ex.StackTrace) +
				(ex.InnerException == null
					? ""
					: "\r\nInner exception: " + ex.InnerException.GetType() +
						"\r\nInner message: " + ex.InnerException.Message +
						(ex.InnerException.Source == null ? "" : "\r\nInner source: " + ex.InnerException.Source) +
						(ex.InnerException.StackTrace == null
							? ""
							: "\r\nInner stack trace:\r\n" + ex.InnerException.StackTrace));
		}
	}
}
