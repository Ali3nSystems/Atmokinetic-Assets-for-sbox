namespace AtmokineticAssets;

public sealed partial class CameraController : BasePostProcess<CameraController>, Component.ExecuteInEditor
{
	public GameObject WaterLineCameraGameObject;
	public CameraComponent WaterLineCameraComponent;
	public Texture WaterLineTexture;

	private void CheckWaterSubmersion()
	{
		var corners = GetCameraNearPlaneCorners();
		if (corners == null)
		{
			IsCameraSubmerged = false;
			IsCameraFullySubmerged = false;
			if (WaterLineCameraGameObject.IsValid())
			{
				WaterController?.DestroyWaterLineMesh();
				DisableWaterLineRendering();
			}
			return;
		}

		// Cached by EnvironmentController on awake - no per-frame scene scan.
		var waterControllers = EnvironmentController.CachedWaterControllers;
		int maxSubmergedCount = 0;
		WaterController activeWater = null;

		foreach (var water in waterControllers)
		{
			if (!water.IsValid()) continue;

			int submergedCount = CountCameraSubmergedCorners(corners, water);
			if (submergedCount > maxSubmergedCount)
			{
				maxSubmergedCount = submergedCount;
				activeWater = water;
			}
		}

		bool WasCameraFullySubmerged = IsCameraFullySubmerged;
		IsCameraSubmerged = maxSubmergedCount >= 1;

		// Use offset check for fully Submerged (only applies when already in water)
		int fullySubmergedCount = activeWater != null ? CountCameraFullySubmergedCorners(corners, activeWater) : 0;
		bool isNowFullySubmerged = fullySubmergedCount == 4;
		bool isNowPartiallySubmerged = IsCameraSubmerged && !isNowFullySubmerged;

		// Handle water controller change
		if (activeWater != WaterController)
		{
			WaterController?.DestroyWaterLineMesh();
			WaterController = activeWater;
		}

		// Water line cube (only when partially Submerged)
		if (isNowPartiallySubmerged && !WaterLineCameraGameObject.IsValid())
		{
			WaterController?.CreateWaterLineMesh();
			ActivateWaterLineRendering();
		}

		else if (!isNowPartiallySubmerged && WaterLineCameraGameObject.IsValid())
		{
			WaterController?.DestroyWaterLineMesh();
			DisableWaterLineRendering();
		}

		// Delay IsCameraFullySubmerged off by one frame to prevent flash
		if (WasCameraFullySubmerged && isNowPartiallySubmerged)
		{
			if (_transitioningFromFullySubmerged)
			{
				IsCameraFullySubmerged = false;
				_transitioningFromFullySubmerged = false;
			}
			else
			{
				IsCameraFullySubmerged = true;
				_transitioningFromFullySubmerged = true;
			}
		}
		else
		{
			IsCameraFullySubmerged = isNowFullySubmerged;
			_transitioningFromFullySubmerged = false;
		}
	}

	public void WaterLineShaderAttributes()
	{
		Attributes.Set("Water Line Size", WaterController.WaterLineSize);
		Attributes.Set("Water Line Blur", WaterController.WaterLineBlur);
		Attributes.Set("Water Line Color", WaterController.WaterLineColor);
	}

    private void ActivateWaterLineRendering()
	{
		CameraComponent.RenderExcludeTags.Add("water_line_mesh");

		WaterLineCameraGameObject = new GameObject { Name = "Water Line Camera", Parent = GameObject };
		// Local-only render helper - never include it in network snapshots.
		WaterLineCameraGameObject.Flags |= GameObjectFlags.NotNetworked;
		WaterLineCameraGameObject.NetworkMode = NetworkMode.Never;
		WaterLineCameraComponent = WaterLineCameraGameObject.AddComponent<CameraComponent>();
		WaterLineCameraComponent.BackgroundColor = Color.Black;
		WaterLineCameraComponent.IsMainCamera = false;
		WaterLineCameraComponent.FovAxis = CameraComponent.FovAxis;
		WaterLineCameraComponent.FieldOfView = CameraComponent.FieldOfView;
		WaterLineCameraComponent.ZNear = CameraComponent.ZNear;
		WaterLineCameraComponent.ZFar = CameraComponent.ZFar;
		WaterLineCameraComponent.Priority = CameraComponent.Priority + 1;
		WaterLineCameraComponent.Orthographic = CameraComponent.Orthographic;
		WaterLineCameraComponent.OrthographicHeight = CameraComponent.OrthographicHeight;
		WaterLineCameraComponent.TargetEye = CameraComponent.TargetEye;
		WaterLineCameraComponent.Viewport = CameraComponent.Viewport;
		WaterLineCameraComponent.RenderTags.Add("water_line_mesh");
		WaterLineCameraComponent.RenderExcludeTags.RemoveAll();
		WaterLineCameraComponent.EnablePostProcessing = false;
	}

	private void RenderWaterLineTexture()
	{
		WaterLineTexture = Texture.CreateRenderTarget("WaterLineRenderTexture", ImageFormat.A8, Screen.Size, WaterLineTexture);
		WaterLineCameraComponent.RenderTarget = WaterLineTexture;
		Attributes.Set("WaterLineTexture", WaterLineTexture);
	}

	private void DisableWaterLineRendering()
	{
		WaterLineCameraGameObject.Destroy();
		WaterLineCameraGameObject = null;
		WaterLineCameraComponent = null;

		WaterLineTexture.Dispose();
		WaterLineTexture = null;

		CameraComponent.RenderExcludeTags.Remove("water_line_mesh");
	}
}