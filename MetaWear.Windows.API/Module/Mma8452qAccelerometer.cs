using MetaWear.Windows.API.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetaWear.Windows.API.Module
{
    public class Mma8452qAccelerometer : MWAccelrometer
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Mma8452qAccelerometer"/> class.
        /// This is the accelerometer in the MetaWear R board.
        /// </summary>
        /// <param name="board">The board.</param>
        public Mma8452qAccelerometer(MWBoard board) : base(board)
        {

        }
        /// <summary>
        /// Handles the callback of orientation data.
        /// </summary>
        /// <param name="data">The data.</param>
        public override void HandleCallback(byte[] data)
        {
            // Translate the numeric data from the metawear board to an index that represents an orientation
            var index = (4 * (data[0] & 0x1) + ((data[0] >> 1) & 0x3));

            var orientation = (MWOrientation)(Enum.GetValues(typeof(MWOrientation)).GetValue(index));
            
            _action.Invoke(orientation);
            
        }
    }
}
