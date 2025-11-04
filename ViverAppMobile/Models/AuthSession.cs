using ViverApp.Shared.DTos;
using ViverAppMobile.Services;

namespace ViverAppMobile.Models
{
    public static class AuthSession
    {
        private const string AccessTokenKey = "access_token";
        private const string RefreshTokenKey = "refresh_token";

        public static Task<string?> GetAccessTokenAsync() => SecureStorage.GetAsync(AccessTokenKey);
        public static Task<string?> GetRefreshTokenAsync() => SecureStorage.GetAsync(RefreshTokenKey);

        public static async Task SaveTokensAsync(string accessToken, string refreshToken, int usertype)
        {
            await SecureStorage.SetAsync(AccessTokenKey, accessToken);
            await SecureStorage.SetAsync(RefreshTokenKey, refreshToken);
            Preferences.Set("usertype", usertype);
        }

        public static void Clear()
        {
            SecureStorage.Remove(AccessTokenKey);
            SecureStorage.Remove(RefreshTokenKey);
            Preferences.Remove("usertype");
        }

        public static async Task ClearAsync(UserDto? user, bool clearDeviceToken = true)
        {
            Clear();

            if(user is not null)
            {
                AuthService service = new();
                await service.DeleteTokensAsync(user.IdUser, clearDeviceToken);
            }
        }
    }
}
