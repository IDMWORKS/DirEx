using System.DirectoryServices;
using System.Web.Mvc;

namespace DirEx.Controllers
{
	public class HomeController : Controller
	{
		public ActionResult Index()
		{			
			var viewModel = new Models.DirectoryViewModel();

			var root = "dc=system,dc=backend";
            var dir = new DirectoryEntry("LDAP://idfdemo.dev.idmworks.net:6389/" + root);
			dir.AuthenticationType = AuthenticationTypes.Anonymous;

			var searcher = new DirectorySearcher(dir);
			searcher.SearchScope = SearchScope.OneLevel;

			var results = searcher.FindAll();

			viewModel.DistinguishedName = root;
			viewModel.RelativeName = dir.Name;

			foreach (SearchResult result in results)
			{
				var child = result.GetDirectoryEntry();
				var entry = new Models.EntryViewModel();
				entry.RelativeName = child.Name;
				entry.DistinguishedName = entry.RelativeName + "," + viewModel.DistinguishedName;
				viewModel.Entries.Add(entry);
			}
			
			return View(viewModel);
		}
	}
}