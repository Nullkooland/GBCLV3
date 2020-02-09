namespace GBCLV3.Models.Authentication
{
    class AuthServerInfo
    {
        public AuthServerMeta Meta { get; set; }

        public string[] SkinDomains { get; set; }

        //public string SignaturePublickey { get; set; }
    }

    class AuthServerMeta
    {
        public string ImplementationName { get; set; }

        public string ImplementationVersion { get; set; }

        public string ServerName { get; set; }

        public AuthServerLinks Links { get; set; }
    }

    class AuthServerLinks
    {
        public string Homepage { get; set; }

        public string Register { get; set; }
    }
}
