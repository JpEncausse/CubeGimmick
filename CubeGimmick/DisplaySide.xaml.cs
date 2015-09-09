using MetaWear.Windows.API;
using MetaWear.Windows.API.Enums;
using MetaWear.Windows.API.Module;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Windows.ApplicationModel;
using Windows.Foundation;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;


namespace CubeGimmick
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class DisplaySide : Page, INotifyPropertyChanged
    {
        private readonly CoreDispatcher _dispatcher;
        public event PropertyChangedEventHandler PropertyChanged;
        private string _side;
        private int _accelRead;

        /// <summary>
        /// Gets or sets the Metawear device that is connected.
        /// </summary>
        /// <value>
        /// The device.
        /// </value>
        public MWBoard Device { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="DisplaySide"/> class.
        /// </summary>
        public DisplaySide() 
        {
            this.InitializeComponent();
            Side = "Hello";

            if (!DesignMode.DesignModeEnabled)
            {
                _dispatcher = CoreWindow.GetForCurrentThread().Dispatcher;
            }

        }

        /// <summary>
        /// Gets or sets the text representing which side the cube is on.
        /// </summary>
        /// <value>
        /// The side.
        /// </value>
        public string Side
        {
            get { return _side; }
            set { _side = value; OnPropertyChanged(); }
        }
       
        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.
        /// This parameter is typically used to configure the page.</param>
        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            // Set the device as the selected one from the previous page
            Device = e.Parameter as MWBoard;

            // Enable notifications on the board
            await Device.ConfigureNotificationsAsync();

            // Get an instance of the metawear accelerometer module
            var accel = await Device.GetModule<MWAccelrometer>();

            // Sets the callback to be made when the orientation changes
            await accel.SetOrientationCallback((orientation) => 
            {
                switch (orientation)
                {
                    case MWOrientation.BACK_LANDSCAPE_LEFT:
                        Side = "1";
                        break;
                    case MWOrientation.BACK_LANDSCAPE_RIGHT:
                        Side = "2";
                        break;
                    case MWOrientation.BACK_PORTRAIT_DOWN:
                        Side = "3";
                        break;
                    case MWOrientation.BACK_PORTRAIT_UP:
                        Side = "4";
                        break;
                    case MWOrientation.FRONT_LANDSCAPE_LEFT:
                        Side = "5";
                        break;
                    case MWOrientation.FRONT_LANDSCAPE_RIGHT:
                        Side = "6";
                        break;
                    case MWOrientation.FRONT_PORTRAIT_DOWN:
                        Side = "7";
                        break;
                    case MWOrientation.FRONT_PORTRAIT_UP:
                        Side = "8";
                        break;
                }
                });

            // Starts the accelerometer sending orientation data back 
            await accel.StartOrientation();
            
            base.OnNavigatedTo(e);
        }

        /// <summary>
        /// Called when [property changed].
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
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

        /// <summary>
        /// Handles the Click event of the Stop button.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private async void Stop_Click(object sender, RoutedEventArgs e)
        {
            // get an instance of the metawear accelerometer module
            var accel = await Device.GetModule<MWAccelrometer>();

            // Stop sending orientation data
           await accel.StopOrientation();

            Application.Current.Exit();
        }
    }
}
