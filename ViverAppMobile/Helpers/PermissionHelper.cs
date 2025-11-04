using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#if ANDROID
using Android;
using Android.Content.PM;
using AndroidX.Core.App;
using AndroidX.Core.Content;
#endif

namespace ViverAppMobile.Helpers
{
    public static class PermissionHelper
    {
        public static async Task<bool> RequestStoragePermissionAsync()
        {
#if ANDROID
            var writeStatus = await Permissions.CheckStatusAsync<Permissions.StorageWrite>();
            var readStatus = await Permissions.CheckStatusAsync<Permissions.StorageRead>();

            if (writeStatus == PermissionStatus.Granted && readStatus == PermissionStatus.Granted)
                return true;

            if (writeStatus != PermissionStatus.Granted)
                writeStatus = await Permissions.RequestAsync<Permissions.StorageWrite>();

            if (readStatus != PermissionStatus.Granted)
                readStatus = await Permissions.RequestAsync<Permissions.StorageRead>();

            return writeStatus == PermissionStatus.Granted && readStatus == PermissionStatus.Granted;
#else
            await Task.CompletedTask;            
            return true;
#endif
        }

        public static async Task<bool> RequestPostNotificationsAsync()
        {
#if ANDROID
            var permissionPostNotification = await Permissions.RequestAsync<Permissions.PostNotifications>();
            return permissionPostNotification == PermissionStatus.Granted;
#else
            await Task.CompletedTask;
            return true;
#endif
        }
    }
}
