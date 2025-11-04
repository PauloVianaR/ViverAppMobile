using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ViverAppMobile.Controls
{
    public interface IPopupAsync
    {
        public Task<object?> WaitForResultAsync();
        public bool ClosePopup();
    }
}
