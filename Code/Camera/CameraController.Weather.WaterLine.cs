namespace AtmokineticAssets;

public sealed partial class CameraController : BasePostProcess<CameraController>, Component.ExecuteInEditor
{
	public GameObject WaterLineCameraGameObject;
	public CameraComponent WaterLineCameraComponent;
	public Texture WaterLineTexture;
	private Vector2 _waterLineTextureSize;
	private bool _waterLineRenderingActive;

	private void CheckWaterSubmersion()
	{
		var corners = GetCameraNearPlaneCorners();
		if (corners == null)
		{
			IsCameraSubmerged = false;
			IsCameraFullySubmerged = false;
			if (_waterLineRenderingActive)
			{
				DestroyActiveWaterLineMesh();
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

		var useFallbackWater = false;
		var fallbackState = default(CameraWaterFallbackState);
		if ( maxSubmergedCount == 0 && TryGetFallbackWaterState( corners, out fallbackState ) )
		{
			maxSubmergedCount = fallbackState.SubmergedCornerCount;
			useFallbackWater = true;
		}

		bool WasCameraFullySubmerged = IsCameraFullySubmerged;
		IsCameraSubmerged = maxSubmergedCount >= 1;

		// Use offset check for fully Submerged (only applies when already in water)
		int fullySubmergedCount = useFallbackWater
			? fallbackState.FullySubmergedCornerCount
			: activeWater != null
				? CountCameraFullySubmergedCorners(corners, activeWater)
				: 0;
		bool isNowFullySubmerged = fullySubmergedCount == 4;
		bool isNowPartiallySubmerged = IsCameraSubmerged && !isNowFullySubmerged;

		// Handle water controller change
		var fallbackVolumeChanged = useFallbackWater
			&& (!_usingFallbackWater || fallbackState.VolumeId != _fallbackWaterState.VolumeId);
		if (activeWater != WaterController || useFallbackWater != _usingFallbackWater || fallbackVolumeChanged)
		{
			DestroyActiveWaterLineMesh();
			DisableWaterLineRendering();
			WaterController = activeWater;
			_usingFallbackWater = useFallbackWater;
		}

		if ( useFallbackWater )
			_fallbackWaterState = fallbackState;

		// Water line cube (only when partially Submerged)
		if (isNowPartiallySubmerged && !_waterLineRenderingActive)
		{
			CreateActiveWaterLineMesh();
			ActivateWaterLineRendering();
		}

		else if (!isNowPartiallySubmerged && _waterLineRenderingActive)
		{
			DestroyActiveWaterLineMesh();
			DisableWaterLineRendering();
		}

		if ( isNowPartiallySubmerged && useFallbackWater )
			UpdateFallbackWaterLineMesh();

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
		if ( _usingFallbackWater )
		{
			FallbackWaterLineShaderAttributes();
			return;
		}

		Attributes.Set("Water Line Size", WaterController.WaterLineSize);
		Attributes.Set("Water Line Blur", WaterController.WaterLineBlur);
		Attributes.Set("Water Line Color", WaterController.WaterLineColor);
	}

    private void ActivateWaterLineRendering()
	{
		CameraComponent.RenderExcludeTags.Add("water_line_mesh");

		EnsureWaterLineCamera();
		if (!WaterLineCameraComponent.IsValid())
			return;

		SyncWaterLineCameraSettings();
		WaterLineCameraComponent.Enabled = true;
		_waterLineRenderingActive = true;
	}

	private void EnsureWaterLineCamera()
	{
		if (WaterLineCameraComponent.IsValid())
			return;

		WaterLineCameraGameObject = new GameObject { Name = "Water Line Camera", Parent = GameObject };
		// Local-only render helper - never include it in network snapshots.
		WaterLineCameraGameObject.Flags |= GameObjectFlags.NotNetworked;
		WaterLineCameraGameObject.NetworkMode = NetworkMode.Never;
		WaterLineCameraComponent = WaterLineCameraGameObject.AddComponent<CameraComponent>();
		WaterLineCameraComponent.Enabled = false;
		WaterLineCameraComponent.BackgroundColor = Color.Black;
		WaterLineCameraComponent.IsMainCamera = false;
		WaterLineCameraComponent.RenderTags.Add("water_line_mesh");
		WaterLineCameraComponent.RenderExcludeTags.RemoveAll();
		WaterLineCameraComponent.EnablePostProcessing = false;
	}

	private void SyncWaterLineCameraSettings()
	{
		if (!WaterLineCameraComponent.IsValid() || !CameraComponent.IsValid())
			return;

		WaterLineCameraComponent.FovAxis = CameraComponent.FovAxis;
		WaterLineCameraComponent.FieldOfView = CameraComponent.FieldOfView;
		WaterLineCameraComponent.ZNear = CameraComponent.ZNear;
		WaterLineCameraComponent.ZFar = CameraComponent.ZFar;
		WaterLineCameraComponent.Priority = CameraComponent.Priority + 1;
		WaterLineCameraComponent.Orthographic = CameraComponent.Orthographic;
		WaterLineCameraComponent.OrthographicHeight = CameraComponent.OrthographicHeight;
		WaterLineCameraComponent.TargetEye = CameraComponent.TargetEye;
		WaterLineCameraComponent.Viewport = CameraComponent.Viewport;
	}

	private void RenderWaterLineTexture()
	{
		if (!_waterLineRenderingActive || !WaterLineCameraComponent.IsValid())
			return;

		var screenSize = Screen.Size;
		if (screenSize.x <= 0 || screenSize.y <= 0)
			return;

		SyncWaterLineCameraSettings();

		if (!WaterLineTexture.IsValid() || !_waterLineTextureSize.AlmostEqual(screenSize, 0.5f))
		{
			var oldTexture = WaterLineTexture;
			WaterLineTexture = Texture.CreateRenderTarget("WaterLineRenderTexture", ImageFormat.A8, screenSize, oldTexture);
			_waterLineTextureSize = screenSize;

			if (oldTexture.IsValid() && oldTexture != WaterLineTexture)
				oldTexture.Dispose();
		}

		WaterLineCameraComponent.RenderTarget = WaterLineTexture;
		Attributes.Set("WaterLineTexture", WaterLineTexture);
	}

	private void DisableWaterLineRendering()
	{
		_waterLineRenderingActive = false;
		if (WaterLineCameraComponent.IsValid())
			WaterLineCameraComponent.Enabled = false;

		if (CameraComponent.IsValid())
			CameraComponent.RenderExcludeTags.Remove("water_line_mesh");
	}

	private void DestroyWaterLineRendering()
	{
		DisableWaterLineRendering();

		if (WaterLineCameraGameObject.IsValid())
			WaterLineCameraGameObject.Destroy();

		WaterLineCameraGameObject = null;
		WaterLineCameraComponent = null;

		if (WaterLineTexture.IsValid())
			WaterLineTexture.Dispose();

		WaterLineTexture = null;
		_waterLineTextureSize = Vector2.Zero;
	}

	protected override void OnDisabled()
	{
		DestroyActiveWaterLineMesh();
		DisableWaterLineRendering();
		base.OnDisabled();
	}

	protected override void OnDestroy()
	{
		DestroyActiveWaterLineMesh();
		DestroyWaterLineRendering();
		base.OnDestroy();
	}
}
