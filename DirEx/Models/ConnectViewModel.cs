using System.ComponentModel.DataAnnotations;

namespace DirEx.Models
{
	public class ConnectViewModel
	{
		[Required]
		public string Host { get; set; }
		[Required]
		public int Port { get; set; }
		public string BaseDn { get; set; }
		public string UserDn { get; set; }
		[DataType(DataType.Password)]
		public string Password { get; set; }
		
		public ConnectViewModel()
		{
			Port = 389;
		}

		public string GetServerUri()
		{
			return "LDAP://" + Host + ":" + Port + "/";
		}
    }
}