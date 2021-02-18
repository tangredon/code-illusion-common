namespace Illusion.Common.Authentication
{
    public class OpenIdConnectSettings
    {
        public static string Name => "OpenIdConnect";

        public string Authority { get; set; }
        public string Audience { get; set; }
        public string Value { get; set; }
    }
}