using System;

namespace BlazorApp.Shared.Auth
{
    public class Token
    {
        public string AccessToken { get; set; }
        public DateTime? AccessTokenExpiredAt { get; set; }

        public string RefreshToken { get; set; }
        public DateTime? RefreshTokenExpiredAt { get; set; }
    }
}
