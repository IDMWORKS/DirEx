namespace DirEx.Ldap.Data
{
	public class LdapConnection
	{
		public string Host { get; set; }
		public int Port { get; set; }
		public string BaseDn { get; set; }
		public string UserDn { get; set; }
		public string Password { get; set; }
		
		public LdapConnection()
		{
			Port = 389;
		}

		public string GetServerUri()
		{
			return "LDAP://" + Host + ":" + Port + "/";
		}
    }
}