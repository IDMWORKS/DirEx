using System;
using System.Collections.Generic;

namespace DirEx.Models
{
	public class EntryViewModel
	{
		public string DistinguishedName { get; set; }
		public string RelativeName { get; set; }
		public readonly ICollection<Tuple<string, string>> AttributeValues = new List<Tuple<string, string>>();
		public readonly ICollection<EntryViewModel> Entries = new List<EntryViewModel>();
	}
}