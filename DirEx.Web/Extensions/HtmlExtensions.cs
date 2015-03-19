namespace DirEx.Web.Extensions
{
	public static class HtmlExtensions
	{
		public static string ToHtmlId(this string source)
		{
			return source.GetHashCode().ToString();
		}
	}
}
