using Android.Gms.Tasks;
using Firebase.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Task = Android.Gms.Tasks.Task;

namespace ViverAppMobile.Platforms.Android.Services
{
    public static class FirebaseTokenService
    {
        public static Task<string?> GetTokenAsync()
        {
            var tcs = new TaskCompletionSource<string?>();

            FirebaseMessaging.Instance.GetToken().AddOnCompleteListener(new OnCompleteListener(tcs));

            return tcs.Task;
        }

        private class OnCompleteListener(TaskCompletionSource<string?> tcs) : Java.Lang.Object, IOnCompleteListener
        {
            private readonly TaskCompletionSource<string?> _tcs = tcs;

            public void OnComplete(Task task)
            {
                if (task.IsSuccessful)
                {
                    _tcs.SetResult(task.Result?.ToString());
                }
                else
                {
                    _tcs.SetResult(null);
                }
            }
        }
    }
}
