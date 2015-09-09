using MetaWear.Windows.API;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Foundation;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
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
        public event PropertyChangedEventHandler PropertyChanged;

        public List<MWBoard> Devices { get; set; }

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
            Devices = new List<MWBoard>();

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
            
            await LoadMetaWearDevices();
           
        }

        /// <summary>
        /// Loads the meta wear devices.
        /// </summary>
        /// <returns></returns>
        private async Task LoadMetaWearDevices()
        {
            // Get a list of connected Metawear boards
            Devices = await MWBoardService.Instance.GetConnectedMWBoards();

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
