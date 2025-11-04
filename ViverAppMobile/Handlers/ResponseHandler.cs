using ViverAppMobile.Controls;
using ViverAppMobile.Workers;

namespace ViverAppMobile.Handlers
{
    public class ResponseHandler<T>
    {
        public bool WasSuccessful { get; set; } = false;
        public string? ResponseErr { get; set; } = string.Empty;
        public T? Response { get; set; }

        public static readonly ResponseHandler<T>? Empty;

        public ResponseHandler() { }

        public void IsSuccess(T resp)
        {
            WasSuccessful = true;
            ResponseErr = "Sucess!";
            Response = resp;
        }

        public void IsNotSuccess(string msg)
        {
            if (Master.WasUnauthorized)
                Master.CancelGlobalToken();

            WasSuccessful = false;
            ResponseErr = msg;
        }

        public void ThrowIfIsNotSucess()
        {
            if(!WasSuccessful)
                throw new Exception(ResponseErr);
        }
    }
}
