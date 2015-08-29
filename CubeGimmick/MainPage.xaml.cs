using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Devices.Enumeration;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace CubeGimmick
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page, INotifyPropertyChanged
    {   
        private string _side;
        private bool _loading;
        private readonly CoreDispatcher _dispatcher;
            
        public ObservableCollection<DeviceInformation> Devices { get; set; }

        public string Side
        {
            get { return _side; }
            set { _side = value; OnPropertyChanged(); }
        }

        public bool Loading { get { return _loading; }
            set
            {
                _loading = value;
                OnPropertyChanged();
                OnPropertyChanged("LoadingVisability"); //TODO replace with ValueConverter;
            }
        }
        public Visibility LoadingVisability
        {
            get { return _loading ? Visibility.Visible : Visibility.Collapsed; ; }
            set { _loading = value == Visibility.Visible; }
            
        }

        public MainPage() 
        {
            Loading = true;
            Side = "Loading ...";
            Devices = new ObservableCollection<DeviceInformation>();

            if (!DesignMode.DesignModeEnabled)
            {
                _dispatcher = CoreWindow.GetForCurrentThread().Dispatcher;
            }

            this.InitializeComponent();            

            base.NavigationCacheMode = NavigationCacheMode.Required;
        }

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.
        /// This parameter is typically used to configure the page.</param>
        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            // TODO: Prepare page for display here.

            // TODO: If your application contains multiple pages, ensure that you are
            // handling the hardware Back button by registering for the
            // Windows.Phone.UI.Input.HardwareButtons.BackPressed event.
            // If you are using the NavigationHelper provided by some templates,
            // this event is handled for you.
            await LoadMetaWearDevices();
            //LoadingVisability = Visibility.Collapsed;
        }

        private async Task LoadMetaWearDevices()
        {
            var metawearUUID = new Guid("326A9000-85CB-9195-D9DD-464CFBBAE75A");

            foreach (DeviceInformation di in await DeviceInformation.FindAllAsync(
                   GattDeviceService.GetDeviceSelectorFromUuid(metawearUUID),
                   new string[] { "System.Devices.ContainerId" }))

            {
                Devices.Add(di);
            }

            if (Devices.Count == 0)
            {
                await new MessageDialog("There are no MetaWear deivces connected.  Go to Settings and pair you deivce.").ShowAsync();
                Application.Current.Exit();
            }
            else if (Devices.Count == 1)
                base.Frame.Navigate(typeof(DisplaySide), Devices.First());

            Loading = false;
        }

        private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            base.Frame.Navigate(typeof(DisplaySide), e.AddedItems.First());

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
