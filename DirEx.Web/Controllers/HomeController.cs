using DirEx.Web.Models;
using System;
using System.Runtime.InteropServices;
using System.Web.Mvc;

namespace DirEx.Web.Controllers
{
	public class HomeController : Controller
	{		
		[HttpGet]
		public ActionResult Connect(string host, int? port, string baseDn, string userDn, string password)
		{
			var viewModel = new ConnectViewModel
			{
				Host = host,
				Port = port.HasValue ? port.Value : 389,
				BaseDn = baseDn,
				UserDn = userDn,
				Password = password
			};

			if (!String.IsNullOrEmpty(viewModel.Password))
				return Connect(viewModel);

			return View(viewModel);
		}
		
		[HttpPost]
		public ActionResult Connect(ConnectViewModel viewModel)
		{
			Session.Clear();

			var connectionInfo = viewModel.ToConnectionInfo();
			var client = new Ldap.LdapClient(connectionInfo);

			using (var entry = client.GetNativeEntry(viewModel.BaseDn))
			{
				try
				{
					// trigger LDAP BIND
					entry.RefreshCache();
					// cache LdapConnection info
					Session[Keys.SessionData.ConnectionInfo] = connectionInfo;
				}
				catch (COMException ex)
				{
					TempData[Keys.TempData.AlertDanger] = "<strong>Error</strong> " + ex.Message;
					return RedirectToAction("connect", new { host = viewModel.Host, port = viewModel.Port, baseDn = viewModel.BaseDn, userDn = viewModel.UserDn });
				}
			}

			return RedirectToAction("index");
		}

		[HttpPost]
		public ActionResult Disconnect()
		{
			Session.Abandon();
			return RedirectToAction("connect");
		}


		public ActionResult Index(string currentDn = "")
		{
			// get cached ldapConnection
			var connectionInfo = (Ldap.Data.LdapConnection)Session[Keys.SessionData.ConnectionInfo];

			// redirect to connect action is no cached info
			if (connectionInfo == null)
				return RedirectToAction("connect");

			Ldap.Data.LdapTree ldapTree = new Ldap.Data.LdapTree(connectionInfo.GetServerUri());

			// key for caching tree data in the global ASP.NET cache
			string cacheKey = String.Format("{0}-{1}-{2}-{3}", ldapTree.Server, connectionInfo.UserDn, connectionInfo.BaseDn, connectionInfo.UserDn);
			
			if (!String.IsNullOrEmpty(currentDn))
			{
				// we must have a cached DirectoryViewModel			
				ldapTree = (Ldap.Data.LdapTree)HttpContext.Cache[cacheKey];
				if (ldapTree == null)
					// otherwise clear out currentDn	
					currentDn = "";
			}

			// default to the BaseDn specified in the connection info
			if (String.IsNullOrEmpty(currentDn))
				currentDn = connectionInfo.BaseDn;

			// do we need to populate this entry?
			ldapTree.CurrentEntry = ldapTree.FindCachedEntry(currentDn);
			var populated = (ldapTree.CurrentEntry != null) && (ldapTree.CurrentEntry.Entries.Count > 0);

			if (!populated)
			{
				try
				{
					var client = new Ldap.LdapClient(connectionInfo);
					client.PopulateTree(currentDn, ldapTree);

					ldapTree.CurrentEntry = ldapTree.FindCachedEntry(currentDn);

					// cache asset data for 30min sliding
					HttpContext.Cache.Insert(cacheKey, ldapTree, null,
						System.Web.Caching.Cache.NoAbsoluteExpiration, TimeSpan.FromMinutes(30));
				}
				catch (COMException ex)
				{
					TempData[Keys.TempData.AlertDanger] = "<strong>Error</strong> " + ex.Message;
				}
			}

			return View(ldapTree);
		}
	}
}