namespace Illusion.Common.Authentication
{
    public class OpenIdConnectOptions
    {
        public static string SectionName => "OpenIdConnect";

        public string Authority { get; set; }
        public string Audience { get; set; }
        public string Value { get; set; }
    }
}