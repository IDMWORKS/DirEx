using System.ComponentModel.DataAnnotations;

namespace DirEx.Web.Extensions
{
	public static class StringExtensions
	{
		public static bool IsEmailAddress(this string source)
		{
			return new EmailAddressAttribute().IsValid(source);
		}
	}
}