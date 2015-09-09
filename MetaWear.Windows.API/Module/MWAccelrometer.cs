using MetaWear.Windows.API.Enums;
using MetaWear.Windows.API.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetaWear.Windows.API.Module
{
    /// <summary>
    /// Base Accelerometer class.
    /// </summary>
    public abstract class MWAccelrometer : ICallbackModule, IMWBoardModule
    {
        private readonly byte ORIENTATION_CALLBACK_INDEX = 0x03;
        internal MWBoard Board { get; private set; }

        public MWAccelrometer(MWBoard board)
        {
            if (board == null)
                throw new InvalidOperationException("Cannot construct Accelrometer without MWBoard");

            Board = board;
        }
        public abstract void HandleCallback(byte[] data);

        internal Action<MWOrientation> _action;

        public async Task SetOrientationCallback(Action<MWOrientation> action)
        {
            if (!Board.NotificationsEnabled)
                 await Board.ConfigureNotificationsAsync();

            _action = action;

             await Board.RegisterCallback(ORIENTATION_CALLBACK_INDEX, this);
        }

        public async Task StartOrientation()
        {
            // NOTE - You must be in a stopped state before you can configure or start!
            await StopOrientation();

            var startOrientation = new byte[] { 0x03, 0x08, 0x01 };
            var status =  Board.WriteCommandAsync(startOrientation);

            // TODO allow for more configuration here
            var configureOrientation = new byte[] { 0x03, 0x09, 0x00, 0xc0, 0x0a, 0x00, 0x00 };
            status =  Board.WriteCommandAsync(configureOrientation);

            var setEnableNotifications = new byte[] { 0x03, 0x0a, 0x01 };
            status =  Board.WriteCommandAsync(setEnableNotifications);

            var startAccel = new byte[] { 0x03, 0x01, 0x01 };
            status = Board.WriteCommandAsync(startAccel);
        }

        public async Task StopOrientation()
        {
            var stoptAccel = new byte[] { 0x03, 0x01, 0x0 };
            var status = Board.WriteCommandAsync(stoptAccel);
        }
    }
}
