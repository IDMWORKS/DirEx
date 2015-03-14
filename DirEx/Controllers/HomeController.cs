using System;
using System.DirectoryServices;
using System.Web.Mvc;

namespace DirEx.Controllers
{
	public class HomeController : Controller
	{
		public ActionResult Index()
		{			
			var viewModel = new Models.DirectoryViewModel();

			var root = "dc=racf,dc=com";
			var host = "idfdemo.dev.idmworks.net";
			var port = 6389;
			var username = "cn=idfRacfAdmin,dc=racf,dc=com";
			var password = "idfRacfPwd";

			var server = "LDAP://" + host + ":" + port + "/";

			var dir = new DirectoryEntry(server + root);
			if (String.IsNullOrEmpty(username))
			{
				dir.AuthenticationType = AuthenticationTypes.Anonymous;
			}
			else
			{
				dir.Username = username;
				dir.Password = password;
				dir.AuthenticationType = AuthenticationTypes.ServerBind;
			}

			var searcher = new DirectorySearcher(dir);
			searcher.SearchScope = SearchScope.OneLevel;

			var results = searcher.FindAll();

			viewModel.DistinguishedName = root;
			viewModel.RelativeName = dir.Name;

			foreach (SearchResult result in results)
			{
				var child = result.GetDirectoryEntry();
				var entry = new Models.EntryViewModel();

				// not accessing child.Name directly here as the object may be a malformed LDAP entry
				// this is currently the case with the RACF connector and ou=Aliases
				// this will at least let us populate the entry and we can error fetching details later
				entry.RelativeName = child.Path.Substring(server.Length, child.Path.Length - server.Length - viewModel.DistinguishedName.Length - 1);

                entry.DistinguishedName = entry.RelativeName + "," + viewModel.DistinguishedName;
				viewModel.Entries.Add(entry);
			}
			
			return View(viewModel);
		}
	}
}