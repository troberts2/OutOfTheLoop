using UnityEngine;

public static class OrientationUtils
{
    public static ScreenOrientation GetCurrentOrientation()
    {
        switch (Input.deviceOrientation)
        {
            case DeviceOrientation.LandscapeLeft:
                return ScreenOrientation.LandscapeLeft;
            case DeviceOrientation.LandscapeRight:
                return ScreenOrientation.LandscapeRight;
            case DeviceOrientation.Portrait:
                return ScreenOrientation.Portrait;
            case DeviceOrientation.PortraitUpsideDown:
                return ScreenOrientation.PortraitUpsideDown;

            // Handle edge cases
            case DeviceOrientation.FaceUp:
            case DeviceOrientation.FaceDown:
            case DeviceOrientation.Unknown:
            default:
                // Fallback: just use current Screen.orientation
                return Screen.orientation;
        }
    }
}
