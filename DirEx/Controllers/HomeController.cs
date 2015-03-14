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

			BuildEntry(root, viewModel, dir);

			return View(viewModel);
		}

		private void BuildEntry(string rootDn, Models.DirectoryViewModel viewModel, DirectoryEntry root)
		{
			viewModel.RelativeName = root.Name;
			viewModel.DistinguishedName = rootDn;
			foreach (DirectoryEntry child in root.Children)
			{
				var entry = new Models.EntryViewModel();
				entry.RelativeName = child.Name;
				entry.DistinguishedName = entry.RelativeName + "," + viewModel.DistinguishedName;
				viewModel.Entries.Add(entry);
			}
		}
	}
}