using System;
using System.DirectoryServices;
using System.Web.Mvc;
using DirEx.Models;

namespace DirEx.Controllers
{
	public class HomeController : Controller
	{
		private const string host = "idfdemo.dev.idmworks.net";
		private const int port = 6389;
		private const string username = "cn=idfRacfAdmin,dc=racf,dc=com";
		private const string password = "idfRacfPwd";

		public ActionResult Index()
		{
			var baseDn = "dc=racf,dc=com";

			var viewModel = PopulateDirectoryEntries(baseDn);

			return View(viewModel);
		}

		private DirectoryViewModel PopulateDirectoryEntries(string baseDn)
		{
			var viewModel = new DirectoryViewModel();

			var server = "LDAP://" + host + ":" + port + "/";

			var dir = new DirectoryEntry(server + baseDn);
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

			var rootEntry = new EntryViewModel();
			rootEntry.DistinguishedName = baseDn;
			rootEntry.RelativeName = dir.Name;
			viewModel.Entries.Add(rootEntry);

			foreach (SearchResult result in results)
			{
				var child = result.GetDirectoryEntry();
				var entry = new EntryViewModel();

				// not accessing child.Name directly here as the object may be a malformed LDAP entry
				// this is currently the case with the RACF connector and ou=Aliases
				// this will at least let us populate the entry and we can error fetching details later
				entry.RelativeName = child.Path.Substring(server.Length, child.Path.Length - server.Length - rootEntry.DistinguishedName.Length - 1);

				entry.DistinguishedName = entry.RelativeName + "," + rootEntry.DistinguishedName;
				rootEntry.Entries.Add(entry);
			}

			return viewModel;
		}
	}
}