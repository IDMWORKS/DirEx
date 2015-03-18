using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.Runtime.InteropServices;
using System.Text;

namespace DirEx.Models
{
	public class EntryViewModel
	{
		public string DistinguishedName { get; set; }
		public string RelativeName { get; set; }
		public readonly ICollection<Tuple<string, string>> AttributeValues = new List<Tuple<string, string>>();
		public readonly ICollection<EntryViewModel> Entries = new List<EntryViewModel>();

		public void PopulateAttributes(ResultPropertyCollection source)
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
		}

		public void PopulateAttributes(PropertyCollection source)
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
		}
	}
}