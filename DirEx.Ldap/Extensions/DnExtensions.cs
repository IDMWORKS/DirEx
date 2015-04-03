using CPI.DirectoryServices;
using System;
using System.Linq;

namespace DirEx.Ldap.Extensions
{
	public static class DnExtensions
	{
		// take a DistinguishedName that may or may not have spaces between the comma-
		// separated key-value-pairs and return a DistinguishedName that has no spaces
		// between key-value-pairs
		public static string CompressDn(this string source)
		{
			return new DN(source).ToString();
		}

		public static bool IsDn(this string source)
		{
			if (!source.Contains('=')) return false;
			if (source.StartsWith("ldap://", StringComparison.OrdinalIgnoreCase)) return false;

			try
			{
				new DN(source);
			}
			catch (ArgumentException)
			{
				return false;
			}

			return true;
		}

		public static string ToFriendlyName(this RDN source)
		{
			var result = String.Empty;
			foreach (RDNComponent comp in source.Components)
				result += String.Format("{0}+", comp.ComponentValue);
			result = result.TrimEnd('+');
			return result;
		}		
	}
}