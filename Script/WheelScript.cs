using UnityEngine;

public class WheelScript : MonoBehaviour
{
    private WheelCollider wheelCollider;
    private WheelHit wheelHit;

    void Start()
    {
        wheelCollider = GetComponent<WheelCollider>();
    }

    void Update()
    {
        // Check if the wheel is touching the ground
        if (wheelCollider.GetGroundHit(out wheelHit))
        {
            // Check if the wheel is touching the sidewalk
            if (wheelHit.collider.CompareTag("terrain"))
            {
                // Play a surface effect to simulate driving over a sidewalk
                LogitechGSDK.LogiPlaySurfaceEffect(0, LogitechGSDK.LOGI_PERIODICTYPE_SQUARE, 20, 10);
            }
            else
            {
                // Stop the surface effect when the wheel is no longer driving over the sidewalk
                LogitechGSDK.LogiStopSurfaceEffect(0);
            }
        }
    }
}