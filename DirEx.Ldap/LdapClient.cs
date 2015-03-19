using DirEx.Ldap.Extensions;
using System;
using System.DirectoryServices;

namespace DirEx.Ldap
{
	public class LdapClient
	{
		private readonly Data.LdapConnection connectionInfo;

		public LdapClient(Data.LdapConnection connectionInfo)
		{
			this.connectionInfo = connectionInfo;
		}

		public void PopulateTree(string currentDn, Data.LdapTree ldapTree)
		{
			using (var dir = GetNativeEntry(currentDn))
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
						ApplySearchResults(dir, results, ldapTree);
					}
				}
			}
		}

		private void ApplySearchResults(DirectoryEntry dir, SearchResultCollection results, Data.LdapTree viewModel)
		{
			var server = connectionInfo.GetServerUri();

			Data.LdapEntry parent;
			var currentDn = dir.Path.Substring(server.Length);
			if (viewModel.EntryMap.ContainsKey(currentDn))
			{
				parent = viewModel.EntryMap[currentDn];
			}
			else
			{
				parent = new Data.LdapEntry();
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

				var entry = new Data.LdapEntry();

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

			parent.SortEntries();
		}
		
		public DirectoryEntry GetNativeEntry(string entryDn)
		{
			var dir = new DirectoryEntry(connectionInfo.GetServerUri() + entryDn);
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
	}
}
