using MetaWear.Windows.API.Enums;

namespace MetaWear.Windows.API.Helpers
{
    public static class OrientationHelpers
    {
        public static bool IsPortrait(this MWOrientation orientation)
        {
            bool isPortriat = true;
            switch (orientation)
            {
                case MWOrientation.FRONT_LANDSCAPE_RIGHT:
                case MWOrientation.FRONT_LANDSCAPE_LEFT:
                case MWOrientation.BACK_LANDSCAPE_RIGHT:
                case MWOrientation.BACK_LANDSCAPE_LEFT:
                    isPortriat = false;
                    break;
            }
            return isPortriat;
        }
        public static bool IsFront(this MWOrientation orientation)
        {
            bool isFront = true;
            switch (orientation)
            {
                case MWOrientation.BACK_PORTRAIT_UP:
                case MWOrientation.BACK_PORTRAIT_DOWN:
                case MWOrientation.BACK_LANDSCAPE_RIGHT:
                case MWOrientation.BACK_LANDSCAPE_LEFT:
                    isFront = false;
                    break;
            }
            return isFront;
        }
    }
}
