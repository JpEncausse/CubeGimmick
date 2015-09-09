using MetaWear.Windows.API.Interfaces;
using MetaWear.Windows.API.Module;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Storage.Streams;

namespace MetaWear.Windows.API
{
    /// <summary>
    /// Class to represent a Metawear board
    /// </summary>
    public class MWBoard
    {
        private string _id; // the device ID
        private static Guid COMMAND_UUID = new Guid("326a9001-85cb-9195-d9dd-464cfbbae75a");
        private static Guid NOTIFY_UUID = new Guid("326A9006-85CB-9195-D9DD-464CFBBAE75A");
        public string Name { get; set; } // The device Name
        public bool NotificationsEnabled { get; internal set; }


        /// <summary>
        /// Dictionary to hold the callbacks for a given module
        /// </summary>
        private Dictionary<byte, ICallbackModule> _callbackModules;

        /// <summary>
        /// The implemented modules for this MW Board
        /// </summary>
        private Dictionary<Type, IMWBoardModule> _loadedModules;

        public MWBoard(string id, string name)
        {
            _id = id;
            Name = name;
            NotificationsEnabled = false;

            _callbackModules = new Dictionary<byte, ICallbackModule>();
            _loadedModules = new Dictionary<Type, IMWBoardModule>();

            _loadedModules.Add(typeof(MWAccelrometer), new Mma8452qAccelerometer(this) as IMWBoardModule);
        }

        /// <summary>
        /// Asynchronous method to configure notifications for this MW Board. This method must be called in order
        /// to receive any callback data from the board.
        /// </summary>
        /// <returns></returns>
        public async Task ConfigureNotificationsAsync()
        {
            var gattDeviceService = GattDeviceService.FromIdAsync(_id).AsTask().Result;
            var chars = gattDeviceService.GetAllCharacteristics();
            foreach (var c in chars)
            {
                if (c.CharacteristicProperties.HasFlag(GattCharacteristicProperties.Notify))
                {
                    GattCommunicationStatus result;
                    c.ValueChanged += Characteristic_ValueChanged;
                    result = c.WriteClientCharacteristicConfigurationDescriptorAsync(GattClientCharacteristicConfigurationDescriptorValue.Notify).AsTask().Result;
                    
                }

                if (c.CharacteristicProperties.HasFlag(GattCharacteristicProperties.Read))
                {
                    var result = c.ReadValueAsync(BluetoothCacheMode.Uncached).AsTask().Result;

                    if (result.Status == GattCommunicationStatus.Success)
                    {
                        byte[] forceData = new byte[result.Value.Length];
                        DataReader.FromBuffer(result.Value).ReadBytes(forceData);
                        //return forceData;
                    }
                    else
                    {
                        //await new MessageDialog(result.Status.ToString()).ShowAsync();
                    }
                }
            }

            NotificationsEnabled = true;
        }
        


        /// <summary>
        /// Call back method for the MW device to send back data asynchronously
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="args">The <see cref="GattValueChangedEventArgs"/> instance containing the event data.</param>
        private void Characteristic_ValueChanged(GattCharacteristic sender, GattValueChangedEventArgs args)
        {
            byte[] data = new byte[args.CharacteristicValue.Length];
            DataReader.FromBuffer(args.CharacteristicValue).ReadBytes(data);

            byte[] moduleData = new byte[data.Length - 2];
            Array.Copy(data, 2, moduleData, 0, moduleData.Length);

            _callbackModules[data[0]].HandleCallback(moduleData);

        }

        /// <summary>
        /// Gets the module.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public async Task<T> GetModule<T>()  where T : IMWBoardModule
        {
            if (_loadedModules.ContainsKey(typeof(T)))
                return (T)_loadedModules[typeof(T)];
            else
                return default(T);
        }

        /// <summary>
        /// Registers the callback.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <param name="module">The module.</param>
        internal async Task RegisterCallback(byte index, ICallbackModule module)
        {
            _callbackModules.Add(index, module);
        }

        /// <summary>
        /// Writes the command asynchronously to the MW Board.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <returns></returns>
        internal async Task<GattCommunicationStatus> WriteCommandAsync(byte[] data)
        {
            try
            {
                GattDeviceService service = await GattDeviceService.FromIdAsync(_id);

                var characterisitics = service.GetAllCharacteristics();
                var cmd = characterisitics.Where(c => c.Uuid == COMMAND_UUID).FirstOrDefault();

                GattCharacteristic command = service.GetCharacteristics(COMMAND_UUID)[0];
                byte[] buf = data;
                DataWriter writer = new DataWriter();
                writer.WriteBytes(buf);
                GattCommunicationStatus st = await command.WriteValueAsync(writer.DetachBuffer());

                return st;
            }
            catch (Exception ex)
            {
                //Debug.WriteLine(ex.Message);
            }
            return GattCommunicationStatus.Unreachable;
        }

    }
}
