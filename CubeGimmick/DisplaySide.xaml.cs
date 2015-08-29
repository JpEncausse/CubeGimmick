using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Devices.Enumeration;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage.Streams;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkID=390556

namespace CubeGimmick
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class DisplaySide : Page, INotifyPropertyChanged
    {
        private readonly CoreDispatcher _dispatcher;
        public event PropertyChangedEventHandler PropertyChanged;
        public DeviceInformation Device { get; set; }

        public DisplaySide() 
        {
            this.InitializeComponent();
            Side = "Hello";

            if (!DesignMode.DesignModeEnabled)
            {
                _dispatcher = CoreWindow.GetForCurrentThread().Dispatcher;
            }

        }

        private string _side;
        
        public string Side
        {
            get { return _side; }
            set { _side = value; OnPropertyChanged(); }
        }

        private int _accelRead;
        public int AccelReadValue
        {
            get
            {
                return _accelRead;
            }
            set
            {
                _accelRead = value;
                OnPropertyChanged();
            }
        }
        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.
        /// This parameter is typically used to configure the page.</param>
        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            Device = e.Parameter as DeviceInformation;
           
            Guid CommandUUID = Guid.Parse("326a9001-85cb-9195-d9dd-464cfbbae75a");

            var gattDeviceService = await GattDeviceService.FromIdAsync(Device.Id);
            var chars = gattDeviceService.GetAllCharacteristics();
            foreach (var c in chars)
            {
                var a = await GetValue(c.Uuid);
            }
            
            await TurnOnAccelorometer();
            
            base.OnNavigatedTo(e);
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

        public async Task<byte[]> GetValue(Guid gattCharacteristicUuids)
        {
            try
            {
                var gattDeviceService = await GattDeviceService.FromIdAsync(Device.Id);
                if (gattDeviceService != null)
                {

                    var characteristics = gattDeviceService.GetCharacteristics(gattCharacteristicUuids).First();

                    //If the characteristic supports Notify then tell it to notify us.
                    try
                    {
                        if (characteristics.CharacteristicProperties.HasFlag(GattCharacteristicProperties.Notify))
                        {
                            GattCommunicationStatus result;
                            characteristics.ValueChanged += characteristics_ValueChanged;
                            result = await characteristics.WriteClientCharacteristicConfigurationDescriptorAsync(GattClientCharacteristicConfigurationDescriptorValue.Notify);

                            var name = string.IsNullOrEmpty(characteristics.UserDescription) ? characteristics.Uuid.ToString() : characteristics.UserDescription;

                           
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageDialog md = new MessageDialog(ex.Message);
                        await md.ShowAsync();
                    }

                    //Read
                    if (characteristics.CharacteristicProperties.HasFlag(GattCharacteristicProperties.Read))
                    {
                        var result = await characteristics.ReadValueAsync(BluetoothCacheMode.Uncached);

                        if (result.Status == GattCommunicationStatus.Success)
                        {
                            byte[] forceData = new byte[result.Value.Length];
                            DataReader.FromBuffer(result.Value).ReadBytes(forceData);
                            return forceData;
                        }
                        else
                        {
                            await new MessageDialog(result.Status.ToString()).ShowAsync();
                        }
                    }
                }
                else
                {
                    await new MessageDialog("Access to the device has been denied =(").ShowAsync();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
            return null;
        }

        void characteristics_ValueChanged(GattCharacteristic sender, GattValueChangedEventArgs args)
        {
            byte[] data = new byte[args.CharacteristicValue.Length];
            Windows.Storage.Streams.DataReader.FromBuffer(args.CharacteristicValue).ReadBytes(data);

            //Update properties
            if (sender.Uuid == GattCharacteristicUuids.BatteryLevel)
            {
                AccelReadValue = Convert.ToInt32(data[0]);
            }

            if (data[0] == 0x03 && data.Length >= 3)
            {
                AccelReadValue = data[2];
                switch (AccelReadValue)
                {
                    case 198:
                    case 196:
                    case 194:
                    case 192:
                        Side = "1";
                        break;
                    case 132:
                    case 133:
                        Side = "4";
                        break;
                    case 195:
                    case 197:
                    case 199:
                    case 193:
                        Side = "6";
                        break;
                    case 130:
                    case 131:
                        Side = "5";
                        break;
                    case 129:
                    case 128:
                        Side = "2";
                        break;
                    case 134:
                    case 135:
                        Side = "3";
                        break;
                    default:
                        Side = "?? " + AccelReadValue;
                        break;
                }
            }
        }



        private async Task TurnOnAccelorometer()
        {
            //Accelerometer accelModule = mwBoard.getModule(Accelerometer.class);

            //// Set the sampling frequency to 50Hz, or closest valid ODR
            //accelModule.setOutputDataRate(50.f);

            //var setSampleRate = new byte[] { 3, 3, 0, 0, 40, 0, 0 };
            //var status = await writeCommandAsync(setSampleRate);

            //// Set the measurement range to +/- 4g, or closet valid range
            //accelModule.setAxisSamplingRange(4.0);

            var stoptAccel = new byte[] { 0x03, 0x01, 0x0 };
            var status = await writeCommandAsync(stoptAccel);


            var startOrientation = new byte[] { 0x03, 0x08, 0x01 };
            status = await writeCommandAsync(startOrientation);

            var configureOrientation = new byte[] { 0x03, 0x09, 0x00, 0xc0, 0x0a, 0x00, 0x00 };
            status = await writeCommandAsync(configureOrientation);

            var setEnableNotificatoins = new byte[] { 0x03, 0x0a, 0x01 };
            status = await writeCommandAsync(setEnableNotificatoins);

            var startAccel = new byte[] { 0x03, 0x01, 0x01 };
            status = await writeCommandAsync(startAccel);
        }

        private async Task<GattCommunicationStatus> writeCommandAsync(byte[] data)
        {
            try
            {
                Guid CommandUUID = Guid.Parse("326a9001-85cb-9195-d9dd-464cfbbae75a");

                GattDeviceService service = await GattDeviceService.FromIdAsync(Device.Id);

                var characterisitics = service.GetAllCharacteristics();
                var cmd = characterisitics.Where(c => c.Uuid == CommandUUID).FirstOrDefault();

                GattCharacteristic command = service.GetCharacteristics(CommandUUID)[0];
                // command.ProtectionLevel = GattProtectionLevel.EncryptionRequired;
                byte[] buf = data;
                DataWriter writer = new DataWriter();
                writer.WriteBytes(buf);
                GattCommunicationStatus st = await command.WriteValueAsync(writer.DetachBuffer());

                //deviceState = DeviceState.READY;

                return st;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
            return GattCommunicationStatus.Unreachable;
        }

        private async void Stop_Click(object sender, RoutedEventArgs e)
        {
            var stoptAccel = new byte[] { 0x03, 0x01, 0x0 };
            var status = await writeCommandAsync(stoptAccel);

            Application.Current.Exit();
        }
    }
}
