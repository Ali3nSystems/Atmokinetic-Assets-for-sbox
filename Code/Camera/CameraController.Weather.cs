namespace AtmokineticAssets;

public sealed partial class CameraController : BasePostProcess<CameraController>
{
	// Reused every frame - the corner set is only read within the same update.
	private readonly Vector3[] _cameraNearPlaneCorners = new Vector3[4];

	private Vector3[] GetCameraNearPlaneCorners()
	{
		var frustum = Scene.Camera.GetFrustum(new Rect(0, 0, Screen.Width, Screen.Height));

		for (int i = 0; i < 4; i++)
		{
			var corner = frustum.GetCorner(i);
			if (!corner.HasValue) return null;
			_cameraNearPlaneCorners[i] = corner.Value;
		}

		return _cameraNearPlaneCorners;
	}

	private static int CountCameraSubmergedCorners(Vector3[] corners, WaterController water)
	{
		int count = 0;
		for (int i = 0; i < corners.Length; i++)
		{
			if (water.IsPointInWaterVolume(corners[i]))
				count++;
		}
		return count;
	}

	private static int CountCameraFullySubmergedCorners(Vector3[] corners, WaterController water)
	{
		int count = 0;
		for (int i = 0; i < corners.Length; i++)
		{
			if (water.IsPointFullySubmerged(corners[i]))
				count++;
		}
		return count;
	}
}