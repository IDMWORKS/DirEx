using System.Collections.Generic;

namespace DirEx.Ldap.Data
{
	public class LdapTree
	{
		public readonly string Server;
		public LdapEntry CurrentEntry { get; set; }
		public readonly ICollection<LdapEntry> Entries = new List<LdapEntry>();

		public LdapTree(string server)
		{
			Server = server;
		}

		// internal mapping of entries
		internal readonly Dictionary<string, LdapEntry> EntryMap = new Dictionary<string, LdapEntry>();

		// public exposure of EntryMap
		public Data.LdapEntry FindCachedEntry(string entryDn)
		{
			if (EntryMap.ContainsKey(entryDn))
				return EntryMap[entryDn];

			return null;
		}
	}
}