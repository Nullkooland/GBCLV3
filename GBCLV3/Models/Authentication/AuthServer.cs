namespace GBCLV3.Models.Authentication
{
    public class AuthServerInfo
    {
        public AuthServerMeta Meta { get; set; }

        public string[] SkinDomains { get; set; }

        //public string SignaturePublickey { get; set; }
    }

    public class AuthServerMeta
    {
        public string ImplementationName { get; set; }

        public string ImplementationVersion { get; set; }

        public string ServerName { get; set; }

        public AuthServerLinks Links { get; set; }
    }

    public class AuthServerLinks
    {
        public string Homepage { get; set; }

        public string Register { get; set; }
    }
}
