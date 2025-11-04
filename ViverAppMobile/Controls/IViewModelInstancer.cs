namespace ViverAppMobile.Controls
{
    public interface IViewModelInstancer
    {
        /// <summary>
        /// Method with the simple function of being bound in the code-behind to call LoadAllAsync
        /// </summary>
        /// <returns></returns>
        Task InitializeAsync();

        /// <summary>
        /// Method that performs the initial loading of the viewmodel asynchronously within a parallel thread
        /// </summary>
        /// <returns></returns>
        protected Task<string?> LoadAllAsync();
    }
}
