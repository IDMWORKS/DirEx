using DirEx.Ldap.Extensions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DirEx.Ldap.Tests
{
	[TestClass]
	public class DnExtensionsTests
	{
		[TestMethod]
		// should not raise an exception
		public void IsDn_ShaHash_ReturnsFalse()
		{
			// arrange
			const string value = "{SHA}W6ph5Mm5Pz8GgiULbPgzG37mj9g=";
			const bool expected = false;

			// act
			bool actual = value.IsDn();

			// assert
			Assert.AreEqual(expected, actual);
		}
	}
}
