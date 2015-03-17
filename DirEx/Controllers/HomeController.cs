using DirEx.Extensions;
using DirEx.Models;
using System;
using System.DirectoryServices;
using System.Web.Mvc;

namespace DirEx.Controllers
{
	public class HomeController : Controller
	{
		private const string host = "idfdemo.dev.idmworks.net";
		private const int port = 6389;

		private const string baseDn = "dc=system,dc=backend";
		private const string username = "cn=Directory Manager,dc=system,dc=backend";
		private const string password = "testpass";

		private readonly string server = "LDAP://" + host + ":" + port + "/";

		public ActionResult Index(string currentDn = "")
		{
			string cacheKey = GetDirectoryCacheKey(server, username, baseDn);
			DirectoryViewModel viewModel = new DirectoryViewModel();

			if (!String.IsNullOrEmpty(currentDn))
			{
				// we must have a cached DirectoryViewModel			
				viewModel = (DirectoryViewModel)HttpContext.Cache[cacheKey];
				if (viewModel == null)
					// otherwise clear out currentDn	
					currentDn = "";
			}

			if (String.IsNullOrEmpty(currentDn))
				currentDn = baseDn;

			var populated = false;
			if (viewModel.EntryMap.ContainsKey(currentDn))
				populated = viewModel.EntryMap[currentDn].Entries.Count > 0;

			if (!populated)
				PopulateDirectoryEntries(currentDn, viewModel);

			// cache asset data for 30min sliding
			HttpContext.Cache.Insert(cacheKey, viewModel, null,
				System.Web.Caching.Cache.NoAbsoluteExpiration, TimeSpan.FromMinutes(30));

			return View(viewModel);
		}

		private string GetDirectoryCacheKey(string server, string username, string baseDn)
		{
			return String.Format("{0}-{1}-{2}", server, username, baseDn);
		}

		private void PopulateDirectoryEntries(string currentDn, DirectoryViewModel viewModel)
		{			
			var dir = new DirectoryEntry(server + currentDn);
			if (String.IsNullOrEmpty(username))
				dir.AuthenticationType = AuthenticationTypes.Anonymous;
			else
			{
				dir.Username = username;
				dir.Password = password;
				dir.AuthenticationType = AuthenticationTypes.ServerBind;
			}

			var searcher = new DirectorySearcher(dir);
			searcher.SearchScope = SearchScope.OneLevel;

			var results = searcher.FindAll();

			EntryViewModel parent;
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
			}

			foreach (SearchResult result in results)
			{
				var child = result.GetDirectoryEntry();
				if (child.Path.Equals(server + parent.DistinguishedName))
					continue;

				var entry = new EntryViewModel();

				// not accessing child.Name directly here as the object may be a malformed LDAP entry
				// this is currently the case with the RACF connector and ou=Aliases
				// this will at least let us populate the entry and we can error fetching details later
				// may have spaces too, e.g.: "LDAP://idfdemo.dev.idmworks.net:6389/ou=as400, ou=People, dc=system,dc=backend"
				var entryDn = child.Path.Substring(server.Length);
				entry.RelativeName = entryDn.GetRdn(parent.DistinguishedName);

				entry.DistinguishedName = entry.RelativeName + "," + parent.DistinguishedName;
				parent.Entries.Add(entry);
				viewModel.EntryMap[entry.DistinguishedName] = entry;
			}
		}
	}
}