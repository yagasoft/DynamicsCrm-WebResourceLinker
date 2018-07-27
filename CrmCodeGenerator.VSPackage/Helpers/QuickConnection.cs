#region Imports

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.Text.RegularExpressions;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Sdk.Discovery;
using Microsoft.Xrm.Tooling.Connector;

#endregion

namespace WebResourceLinker
{
	public class QuickConnection
	{
		private static readonly IDictionary<string, IOrganizationService> serviceCache = new Dictionary<string, IOrganizationService>();

		public static IOrganizationService Connect(string url, string domain, string username, string password, out string publicUrl)
		{
			publicUrl = url.Trim('/');
			url = $"{publicUrl}";

			var key = url + username + password;

			if (serviceCache.ContainsKey(key))
			{
				return serviceCache[key];
			}

			var conn = 
				$"Url={url}; Username={username}; Password={password}"
				+ (domain != null ? $"; Domain={domain}" : "");

			conn = $"AuthType={(url.Contains("dynamics.com") ? "Office365" : "AD")};" + conn;
			conn = conn.Trim(';') + "RequireNewInstance=true";
			
			var escapedString = Regex.Replace(conn, @"Password\s*?=.*?(?:;{0,1}$|;)",
				"Password=********;");

			var sdk = new CrmServiceClient(conn);

			if (sdk.LastCrmException != null || !string.IsNullOrEmpty(sdk.LastCrmError))
			{
				var errorMessage = sdk.LastCrmError
					?? (sdk.LastCrmException != null ? BuildExceptionMessage(sdk.LastCrmException) : null);
				throw new ServiceActivationException($"Can't create connection to: \"{escapedString}\" due to\r\n{errorMessage}");
			}

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
