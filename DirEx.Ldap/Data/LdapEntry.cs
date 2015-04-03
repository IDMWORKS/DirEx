using CPI.DirectoryServices;
using DirEx.Ldap.Extensions;
using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.Runtime.InteropServices;
using System.Text;

namespace DirEx.Ldap.Data
{
	public class LdapEntry
	{
		public string DistinguishedName { get; set; }

		private string relativeName;
		public string RelativeName
		{
			get
			{
				return relativeName;
			}
			set
			{
				relativeName = value;
				SetFriendlyName();
			}
		}

		private void SetFriendlyName()
		{
			FriendlyName = new DN(relativeName).RDNs[0].ToFriendlyName();
		}

		public string FriendlyName { get; private set; }

		public readonly List<Tuple<string, string>> AttributeValues = new List<Tuple<string, string>>();
		public readonly List<LdapEntry> Entries = new List<LdapEntry>();

		public void PopulateAttributes(ResultPropertyCollection source, bool sort = true)
		{
			foreach (string name in source.PropertyNames)
			{
				var values = source[name];
				foreach (object value in values)
				{
					var propValue = value is Byte[] ? Encoding.UTF8.GetString((Byte[])value) : value.ToString();
					AttributeValues.Add(new Tuple<string, string>(name, propValue));
				}
			}

			if (sort) SortAttributes();
		}

		public void SortEntries()
		{
			Entries.Sort((en1, en2) =>
            {
				return en1.RelativeName.CompareTo(en2.RelativeName);
			});
		}

		private void SortAttributes()
		{
			AttributeValues.Sort((av1, av2) =>
			{
				int result;

				// sort ObjectClass attributes to the top
				// then sort by attribute Name
				// then sort by attribute Value
				var av1IsOc = av1.Item1.Equals(ModelNames.ObjectClass, StringComparison.OrdinalIgnoreCase);
				var av2IsOc = av2.Item1.Equals(ModelNames.ObjectClass, StringComparison.OrdinalIgnoreCase);

				var av1IsRdn = RelativeName.IndexOf(String.Format("{0}={1}", av1.Item1, av1.Item2), StringComparison.OrdinalIgnoreCase) >= 0;
				var av2IsRdn = RelativeName.IndexOf(String.Format("{0}={1}", av2.Item1, av2.Item2), StringComparison.OrdinalIgnoreCase) >= 0;

				if (av1IsOc && av2IsOc)
					result = 0;
				else if (av1IsOc)
					result = -1;
				else if (av2IsOc)
					result = 1;
				else if (av1IsRdn && av2IsRdn)
					result = av1.Item1.CompareTo(av2.Item1);
				else if (av1IsRdn)
					result = -1;
				else if (av2IsRdn)
					result = 1;
				else
					result = av1.Item1.CompareTo(av2.Item1);

				if (result == 0)
					result = av1.Item2.CompareTo(av2.Item2);

				return result;
			});
		}

		public void PopulateAttributes(PropertyCollection source, bool sort = true)
		{
			foreach (string name in source.PropertyNames)
			{
				try
				{
					var values = source[name];
					foreach (object value in values)
					{
						var propValue = value is Byte[] ? Encoding.UTF8.GetString((Byte[])value) : value.ToString();
						AttributeValues.Add(new Tuple<string, string>(name, propValue));
					}
				}
				catch (COMException ex)
				{
					if (ex.Message.Contains("0x8000500c"))
					{
						// known issue / error
					}
					else
					{
						throw;
					}
				}
			}

			if (sort) SortAttributes();
		}
	}
}