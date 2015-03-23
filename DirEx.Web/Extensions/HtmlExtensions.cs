using DirEx.Ldap.Data;

namespace DirEx.Web.Extensions
{
	public static class HtmlExtensions
	{
		public static string ToHtmlId(this string source)
		{
			return source.GetHashCode().ToString();
		}

		public static string DnToIconClass(this string source)
		{
			return source.StartsWith(ModelNames.DomainComponent + "=") ? "sitemap" :
				source.StartsWith(ModelNames.OrganizationUnit + "=") ? "users" : "user";
        }
	}
}
