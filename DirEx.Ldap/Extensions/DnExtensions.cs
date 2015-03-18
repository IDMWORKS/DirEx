using System;
using System.Collections.Generic;
using System.Linq;

namespace DirEx.Ldap.Extensions
{
	static class DnExtensions
	{
		public static List<Tuple<string, string>> ParseDn(this string source)
		{
			return source.Split(',')
				.Select(value => value.Split('='))
				.Select(pair => new Tuple<string, string>(pair[0], pair[1]))
				.ToList();
		}

		public static string GetRdn(this string source, string parentDn)
		{
			return GetRdn(source.ParseDn(), parentDn);
		}

		public static string GetRdn(this List<Tuple<string, string>> source, string parentDn)
		{
			var working = source.ToList();
			parentDn.ParseDn().ForEach(pair => working.Remove(working.Last()));
			return working.GetDn();
		}

		public static string GetRdn(this List<Tuple<string, string>> source, List<Tuple<string, string>> parentDn)
		{
			var working = source.ToList();
			parentDn.ForEach(pair => working.Remove(working.Last()));
			return working.GetDn();
		}

		public static string GetDn(this List<Tuple<string, string>> source)
		{
			var result = String.Empty;
			source.ForEach(pair => result += "," + pair.Item1 + "=" + pair.Item2);
			result = result.TrimStart(',');
			return result;
		}
	}
}