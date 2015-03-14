using System.Collections.Generic;

namespace DirEx.Models
{
	public class DirectoryViewModel
	{
		public EntryViewModel CurrentEntry { get; set; }
		public readonly ICollection<EntryViewModel> Entries = new List<EntryViewModel>();
	}
}