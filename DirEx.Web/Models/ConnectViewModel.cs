using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace DirEx.Web.Models
{
	public class ConnectViewModel
	{
		[Required]
		public string Host { get; set; }
		[Required]
		public int Port { get; set; }
		[DisplayName("Base DN")]
		public string BaseDn { get; set; }
		[DisplayName("User DN")]
		public string UserDn { get; set; }
		[DataType(DataType.Password)]
		public string Password { get; set; }
		
		public ConnectViewModel()
		{
			Port = 389;
		}

		public Ldap.Data.LdapConnection ToConnectionInfo()
		{
			return new Ldap.Data.LdapConnection
			{
				Host = this.Host,
				Port = this.Port,
				BaseDn = this.BaseDn,
				UserDn = this.UserDn,
				Password = this.Password
			};
		}
    }
}