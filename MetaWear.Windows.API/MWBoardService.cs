using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Devices.Enumeration;

namespace MetaWear.Windows.API
{
    /// <summary>
    /// Singleton service to get connected Metawear boards
    /// </summary>
    public class MWBoardService
    {
        private static MWBoardService _instance = new MWBoardService();

        private MWBoardService() { }
        
        public static MWBoardService Instance { get { return _instance; } }

        private readonly Guid MW_UUID = new Guid("326A9000-85CB-9195-D9DD-464CFBBAE75A");
        public async Task<List<MWBoard>> GetConnectedMWBoards()
        {
            var devices = new List<MWBoard>();

            foreach (DeviceInformation di in await DeviceInformation.FindAllAsync(
                   GattDeviceService.GetDeviceSelectorFromUuid(MW_UUID),
                   new string[] { "System.Devices.ContainerId" }))

            {
                devices.Add(new MWBoard(di.Id, di.Name));
            }

            return devices;
            
        }

    }
}
