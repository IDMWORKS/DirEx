﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace DirEx.Ldap.Extensions
{
	static class DnExtensions
	{
		public static string GetRdn(this string source, string parentDn)
		{
			return GetRdn(source.ParseDn(), parentDn);
		}

		private static List<Tuple<string, string>> ParseDn(this string source)
		{
			// need to parse comma-separated key-value pairs
			// note that values may be quoted and themselves contain commas (RFC 2253)
			// may have spaces too, e.g.: "ou=as400, ou=People, dc=system,dc=backend"

			// use internal method in System.DirectoryServices for this:
			// http://stackoverflow.com/a/23370901/33112
			// alternatively could adopt the following implementation:
			// http://www.codeproject.com/Articles/9788/An-RFC-Compliant-Distinguished-Name-Parser

			var dirSvcAsm = Assembly.Load("System.DirectoryServices");
			var dirSvcUtilsType = dirSvcAsm.GetType("System.DirectoryServices.ActiveDirectory.Utils");
            var getCompsMethod = dirSvcUtilsType.GetMethod("GetDNComponents", BindingFlags.NonPublic | BindingFlags.Static);
			string[] parameters = { source };
			Array comps = (Array)getCompsMethod.Invoke(null, parameters);

			var result = new List<Tuple<string, string>>();
            foreach (object comp in comps)
			{
				var nameProp = comp.GetType().GetField("Name");
				var valueProp = comp.GetType().GetField("Value");
				result.Add(new Tuple<string, string>(nameProp.GetValue(comp).ToString(), valueProp.GetValue(comp).ToString()));
			}

			return result;
		}

		private static string GetRdn(this List<Tuple<string, string>> source, string parentDn)
		{
			var working = source.ToList();
			parentDn.ParseDn().ForEach(pair => working.Remove(working.Last()));
			return working.GetDn();
		}

		private static string GetDn(this List<Tuple<string, string>> source)
		{
			var result = String.Empty;
			source.ForEach(pair => result += "," + pair.Item1 + "=" + pair.Item2);
			result = result.TrimStart(',');
			return result;
		}
	}
}