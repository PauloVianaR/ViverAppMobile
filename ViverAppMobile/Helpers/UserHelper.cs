using CommunityToolkit.Mvvm.Messaging;
using System.Text.Json;
using ViverApp.Shared.DTos;
using ViverAppMobile.Models;

namespace ViverAppMobile.Helpers
{
    public static class UserHelper
    {
        public static void GlobalUserChanged(bool userChanged) => WeakReferenceMessenger.Default.Send(new UserChangedMessage(userChanged));

        public static UserDto? GetLoggedUser()
        {
            UserDto? loggeduser;

            string? userJson = Preferences.Get("user", null);
            if (userJson is not null)
            {
                loggeduser = JsonSerializer.Deserialize<UserDto>(userJson);
                return loggeduser;
            }

            return null;
        }

        public static void SetLoggedUser(UserDto? user)
        {
            if (user is null)
                return;

            RemoveLoggedUser();

            string userJson = JsonSerializer.Serialize(user);
            Preferences.Set("user", userJson);
        }

        public static void RemoveLoggedUser()
        {
            Preferences.Remove("user");
        }
    }
}
