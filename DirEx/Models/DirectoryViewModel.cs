using System.Collections.Generic;

namespace DirEx.Models
{
	public class DirectoryViewModel
	{
		public string Server { get; set; }
		public EntryViewModel CurrentEntry { get; set; }
		public readonly ICollection<EntryViewModel> Entries = new List<EntryViewModel>();
		public readonly Dictionary<string, EntryViewModel> EntryMap = new Dictionary<string, EntryViewModel>();
	}
}