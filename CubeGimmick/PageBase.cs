using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Foundation;
using Windows.UI.Core;
using Windows.UI.Xaml.Controls;

namespace CubeGimmick
{
    public class PageBase : Page, INotifyPropertyChanged
    {
        private readonly CoreDispatcher _dispatcher;
        public PageBase()
        {

            if (!DesignMode.DesignModeEnabled)
            {
                _dispatcher = CoreWindow.GetForCurrentThread().Dispatcher;
            }

        }
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            if (_dispatcher == null || _dispatcher.HasThreadAccess)
            {
                var eventHandler = this.PropertyChanged;
                if (eventHandler != null)
                {
                    eventHandler(this,
                        new PropertyChangedEventArgs(propertyName));
                }
            }
            else
            {
                IAsyncAction doNotAwait =
                    _dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                    () => OnPropertyChanged(propertyName));
            }
        }

    }
}
