namespace AtmokineticAssets;

public sealed partial class CameraController : BasePostProcess<CameraController>
{
	private void UpdateWaterDrips()
	{
		// While a drip effect is active, tick both timers down each frame until it expires.
		if (TimeSinceCameraLastExitedWater > 0)
		{
			TimeSinceCameraLastExitedWater -= Time.Delta;
		}

		if (TimeSinceCameraLastExitedWater <= 0)
		{
			TimeSinceCameraLastExitedWater = 0;
		}
		
		// Just became fully submerged this frame: cancel any in-progress drip effect
		// (no drips while underwater).

		if (IsCameraFullySubmerged && !WasCameraFullySubmerged)
			TimeSinceCameraLastExitedWater = 0f;

		// Just surfaced this frame (was submerged, now isn't): start the drip effect by
		// resetting both timers to their full duration.

		if (!IsCameraFullySubmerged && WasCameraFullySubmerged)
		{
			TimeSinceCameraLastExitedWater = 1f;
		}

		// Remember this frame's submersion state so next frame can detect enter/exit transitions.
		WasCameraFullySubmerged = IsCameraFullySubmerged;
	}

    // public void WaterDripsShaderAttributes()
    // {
    //     Attributes.Set("Water Drips Strength", WaterController.WaterDripsStrength);
    //     Attributes.Set("Water Drips Color", WaterController.WaterDripsColor);
    // }

	public void WaterDripsFadeShaderAttribute()
	{
		Attributes.Set("Water Drips Fade", TimeSinceCameraLastExitedWater);
	}
}