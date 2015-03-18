using DirEx.Extensions;
using DirEx.Models;
using System;
using System.DirectoryServices;
using System.Runtime.InteropServices;
using System.Web.Mvc;

namespace DirEx.Controllers
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

			using (var entry = GetDirectoryEntry(viewModel, viewModel.BaseDn))
			{
				try
				{
					entry.RefreshCache(); // force bind
					Session[Keys.SessionData.ConnectionInfo] = viewModel;
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

		private DirectoryEntry GetDirectoryEntry(ConnectViewModel connectionInfo, string baseDn = "")
		{
			var dir = new DirectoryEntry(connectionInfo.GetServerUri() + baseDn);
			if (String.IsNullOrEmpty(connectionInfo.UserDn))
				dir.AuthenticationType = AuthenticationTypes.Anonymous;
			else
			{
				dir.Username = connectionInfo.UserDn;
				dir.Password = connectionInfo.Password;
				dir.AuthenticationType = AuthenticationTypes.ServerBind;
			}
			return dir;
		}

		public ActionResult Index(string currentDn = "")
		{
			var connectionInfo = (ConnectViewModel)Session[Keys.SessionData.ConnectionInfo];
			if (connectionInfo == null)
				return RedirectToAction("connect");

			var server = connectionInfo.GetServerUri();
			string cacheKey = GetDirectoryCacheKey(server, connectionInfo.UserDn, connectionInfo.BaseDn);
			DirectoryViewModel viewModel = new DirectoryViewModel();

			viewModel.Server = server;

			if (!String.IsNullOrEmpty(currentDn))
			{
				// we must have a cached DirectoryViewModel			
				viewModel = (DirectoryViewModel)HttpContext.Cache[cacheKey];
				if (viewModel == null)
					// otherwise clear out currentDn	
					currentDn = "";
			}

			if (String.IsNullOrEmpty(currentDn))
				currentDn = connectionInfo.BaseDn;

			var populated = false;
			if (viewModel.EntryMap.ContainsKey(currentDn))
			{
				viewModel.CurrentEntry = viewModel.EntryMap[currentDn];
				populated = viewModel.EntryMap[currentDn].Entries.Count > 0;
			}

			if (!populated)
			{
				try
				{
					PopulateDirectoryEntries(currentDn, viewModel);

					viewModel.CurrentEntry = viewModel.EntryMap[currentDn];

					// cache asset data for 30min sliding
					HttpContext.Cache.Insert(cacheKey, viewModel, null,
						System.Web.Caching.Cache.NoAbsoluteExpiration, TimeSpan.FromMinutes(30));
				}
				catch (COMException ex)
				{
					TempData[Keys.TempData.AlertDanger] = "<strong>Error</strong> " + ex.Message;
				}
			}

			return View(viewModel);
		}

		private string GetDirectoryCacheKey(string server, string username, string baseDn)
		{
			return String.Format("{0}-{1}-{2}", server, username, baseDn);
		}

		private void PopulateDirectoryEntries(string currentDn, DirectoryViewModel viewModel)
		{
			var connectionInfo = (ConnectViewModel)Session[Keys.SessionData.ConnectionInfo];

			using (var dir = GetDirectoryEntry(connectionInfo, currentDn))
			{
				using (var searcher = new DirectorySearcher(dir))
				{
					// the default ClientTimeout is -1 which will wait indefinitely
					// in practice this means that a misbehaving server will not return at all
					// and eventually exhaust the pooled connections available in DirectoryServices
					searcher.ClientTimeout = TimeSpan.FromSeconds(5);

					searcher.SearchScope = SearchScope.OneLevel;
					using (var results = searcher.FindAll())
					{
						ApplySearchResults(dir, results, viewModel);
					}
				}
			}
		}

		private void ApplySearchResults(DirectoryEntry dir, SearchResultCollection results, DirectoryViewModel viewModel)
		{
			var connectionInfo = (ConnectViewModel)Session[Keys.SessionData.ConnectionInfo];
			var server = connectionInfo.GetServerUri();

			EntryViewModel parent;
			var currentDn = dir.Path.Substring(server.Length);
			if (viewModel.EntryMap.ContainsKey(currentDn))
			{
				parent = viewModel.EntryMap[currentDn];
			}
			else
			{
				parent = new EntryViewModel();
				parent.DistinguishedName = currentDn;
				parent.RelativeName = dir.Name;
				viewModel.Entries.Add(parent);
				viewModel.EntryMap[parent.DistinguishedName] = parent;

				parent.PopulateAttributes(dir.Properties);
			}

			foreach (SearchResult result in results)
			{
				if (result.Path.Equals(server + parent.DistinguishedName))
					continue;

				var entry = new EntryViewModel();

				// not accessing child.Name directly here as the object may be a malformed LDAP entry
				// this is currently the case with the RACF connector and ou=Aliases
				// this will at least let us populate the entry and we can error fetching details later
				// may have spaces too, e.g.: "LDAP://idfdemo.dev.idmworks.net:6389/ou=as400, ou=People, dc=system,dc=backend"
				var entryDn = result.Path.Substring(server.Length);
				entry.RelativeName = entryDn.GetRdn(parent.DistinguishedName);

				entry.DistinguishedName = entry.RelativeName + "," + parent.DistinguishedName;
				parent.Entries.Add(entry);
				viewModel.EntryMap[entry.DistinguishedName] = entry;

				entry.PopulateAttributes(result.Properties);
			}
		}
	}
}