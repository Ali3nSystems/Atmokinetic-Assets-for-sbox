namespace AtmokineticAssets;

public sealed partial class WaterController : Component, Component.ExecuteInEditor
{
	public bool ShowWaterReflectionPlanarSettings =>
	WaterReflectionMode == WaterReflectionModeEnumeration.Dynamic
	|| ( WaterReflectionMode == WaterReflectionModeEnumeration.Static
	&& WaterReflectionModeStaticMode == WaterReflectionModeStaticModeEnumeration.Planar );
	public bool ShowWaterReflectionPlanarResolutionDynamic => ShowWaterReflectionPlanarSettings && WaterReflectionPlanarResolutionMode == WaterReflectionPlanarResolutionModeEnumeration.Dynamic;
	public bool ShowWaterReflectionPlanarResolutionStatic => ShowWaterReflectionPlanarSettings && WaterReflectionPlanarResolutionMode == WaterReflectionPlanarResolutionModeEnumeration.Static;
	public bool ShowWaterReflectionPlanarFramesPerSecondDynamic => ShowWaterReflectionPlanarSettings && WaterReflectionPlanarFramesPerSecondMode == WaterReflectionPlanarFramesPerSecondModeEnumeration.Dynamic;
	public bool ShowWaterReflectionPlanarFramesPerSecondStatic => ShowWaterReflectionPlanarSettings && WaterReflectionPlanarFramesPerSecondMode == WaterReflectionPlanarFramesPerSecondModeEnumeration.Static;

	
	[Property, Feature("Reflection"), Group("Planar"), Header("General"), ShowIf( nameof(ShowWaterReflectionPlanarSettings), true )]
	public bool ShowWaterReflectionCamera {get;set;} = false;
	
	/// <summary>
	/// If the distance between the center of the bounds and the camera goes beyond the selected value then the camera will be disabled and the cubemap will be used as fallback.
	/// </summary>

	[Property, Feature("Reflection"), Group("Planar"), Title("Distance Culling (m)"), ShowIf( nameof(ShowWaterReflectionPlanarSettings), true )]
	public float WaterReflectionPlanarDistanceCulling {get;set;} = 10f;

	/// <summary>
	/// 
	/// </summary>
	public enum WaterReflectionPlanarResolutionModeEnumeration
	{
		Dynamic,
		Static
	}

	[Property, Feature("Reflection"), Group("Planar"), Header("Resolution"), ShowIf( nameof(ShowWaterReflectionPlanarSettings), true )]
	[Sync]
	public WaterReflectionPlanarResolutionModeEnumeration WaterReflectionPlanarResolutionMode {get;set;} = WaterReflectionPlanarResolutionModeEnumeration.Dynamic;

	[Property, Feature("Reflection"), Group("Planar"), ShowIf( nameof(ShowWaterReflectionPlanarResolutionDynamic), true )]
	[Sync]
	public int WaterReflectionPlanarResolutionDynamicUltra {get;set;} = 1;

	[Property, Feature("Reflection"), Group("Planar"), ShowIf( nameof(ShowWaterReflectionPlanarResolutionDynamic), true )]
	[Sync]
	public int WaterReflectionPlanarResolutionDynamicHigh {get;set;} = 2;

	[Property, Feature("Reflection"), Group("Planar"), ShowIf( nameof(ShowWaterReflectionPlanarResolutionDynamic), true )]
	[Sync]
	public int WaterReflectionPlanarResolutionDynamicMedium {get;set;} = 4;

	[Property, Feature("Reflection"), Group("Planar"), ShowIf( nameof(ShowWaterReflectionPlanarResolutionDynamic), true )]
	[Sync]
	public int WaterReflectionPlanarResolutionDynamicLow {get;set;} = 8;

	[Property, Feature("Reflection"), Group("Planar"), ShowIf( nameof(ShowWaterReflectionPlanarResolutionStatic), true )]
	[Sync]
	public int WaterReflectionPlanarResolutionStatic {get;set;} = 2;

	/// <summary>
	///
	/// </summary>
	
	public enum WaterReflectionPlanarFramesPerSecondModeEnumeration
	{
		Dynamic,
		Static
	}

	[Property, Feature("Reflection"), Group("Planar"), Header("Frames Per Second"), ShowIf( nameof(ShowWaterReflectionPlanarSettings), true )]
	[Sync]
	public WaterReflectionPlanarFramesPerSecondModeEnumeration WaterReflectionPlanarFramesPerSecondMode {get;set;} = WaterReflectionPlanarFramesPerSecondModeEnumeration.Dynamic;

	[Property, Feature("Reflection"), Group("Planar"), ShowIf( nameof(ShowWaterReflectionPlanarFramesPerSecondDynamic), true )]
	[Sync]
	public float WaterReflectionPlanarFramesPerSecondDynamicUltra {get;set;} = 120f;

	[Property, Feature("Reflection"), Group("Planar"), ShowIf( nameof(ShowWaterReflectionPlanarFramesPerSecondDynamic), true )]
	[Sync]
	public float WaterReflectionPlanarFramesPerSecondDynamicHigh {get;set;} = 60f;

	[Property, Feature("Reflection"), Group("Planar"), ShowIf( nameof(ShowWaterReflectionPlanarFramesPerSecondDynamic), true )]
	[Sync]
	public float WaterReflectionPlanarFramesPerSecondDynamicMedium {get;set;} = 30f;

	[Property, Feature("Reflection"), Group("Planar"), ShowIf( nameof(ShowWaterReflectionPlanarFramesPerSecondDynamic), true )]
	[Sync]
	public float WaterReflectionPlanarFramesPerSecondDynamicLow {get;set;} = 15f;

	[Property, Feature("Reflection"), Group("Planar"), ShowIf( nameof(ShowWaterReflectionPlanarFramesPerSecondStatic), true )]
	[Sync]
	public float WaterReflectionPlanarFramesPerSecondStatic {get;set;} = 60f;

	public GameObject WaterReflectionCameraGameObject { get; set; }
	public CameraComponent WaterReflectionCameraComponent { get; set; }
	public Texture WaterReflectionTexture { get; set; }

	private void CreateWaterReflectionPlanarCamera()
	{
		if (!WaterReflectionCameraComponent.IsValid())
			WaterReflectionCameraComponent = GameObject
				.GetComponentInChildren<CameraComponent>( true, false );

		if (WaterReflectionCameraComponent.IsValid())
		{
			WaterReflectionCameraGameObject = WaterReflectionCameraComponent.GameObject;
			WaterReflectionCameraComponent.Enabled = false;
			return;
		}

		WaterReflectionCameraGameObject = new GameObject{ Name = "Planar Reflection Camera", Parent = GameObject, Flags = GameObjectFlags.Absolute };
		WaterReflectionCameraGameObject.Tags.Add("reflection");
		WaterReflectionCameraGameObject.Flags |= GameObjectFlags.NotNetworked;
		WaterReflectionCameraGameObject.NetworkMode = NetworkMode.Never;

		WaterReflectionCameraComponent = WaterReflectionCameraGameObject.AddComponent<CameraComponent>();

		WaterReflectionCameraComponent.Enabled = false;
		WaterReflectionCameraComponent.IsMainCamera = false;
		WaterReflectionCameraComponent.Priority = -100;
		WaterReflectionCameraComponent.RenderExcludeTags.Add("reflection");
		WaterReflectionCameraComponent.RenderExcludeTags.Add("player");
		WaterReflectionCameraComponent.RenderExcludeTags.Add("debugoverlay");
		WaterReflectionCameraComponent.EnablePostProcessing = true;

		if (!Scene.Camera.IsValid())
		return;

		SyncWaterReflectionPlanarCameraWithSceneCamera();
	}

	private void SyncWaterReflectionPlanarCameraWithSceneCamera()
	{
		WaterReflectionCameraComponent.BackgroundColor = Scene.Camera.BackgroundColor;
		WaterReflectionCameraComponent.FovAxis = Scene.Camera.FovAxis;
		WaterReflectionCameraComponent.FieldOfView = Scene.Camera.FieldOfView;
		WaterReflectionCameraComponent.ZNear = Scene.Camera.ZNear;
		WaterReflectionCameraComponent.ZFar = Scene.Camera.ZFar;
		WaterReflectionCameraComponent.Orthographic = Scene.Camera.Orthographic;
		WaterReflectionCameraComponent.OrthographicHeight = Scene.Camera.OrthographicHeight;
		WaterReflectionCameraComponent.TargetEye = Scene.Camera.TargetEye;
		WaterReflectionCameraComponent.Viewport = Scene.Camera.Viewport;
	}

	private void ShowWaterReflectionPlanarCameraGameObject()
	{
		if (!WaterReflectionCameraGameObject.IsValid())
		return;

		if (ShowWaterReflectionCamera == true)
		{
			WaterReflectionCameraGameObject.Flags = WaterReflectionCameraGameObject.Flags.WithFlag(GameObjectFlags.Hidden, false);
		}

		else
		{
			WaterReflectionCameraGameObject.Flags = WaterReflectionCameraGameObject.Flags.WithFlag(GameObjectFlags.Hidden, true);
		}
	}



	/// <summary>
	/// True when this water is actually rendering planar reflections right now.
	/// Planar has to be the resolved mode AND the water has to be on screen AND the
	/// camera has to be within the distance-culling range - when any of those fail
	/// there's nothing to reflect into, so the reflection is shut down and the tiles
	/// fall back to the cubemap.
	/// </summary>
	public bool IsWaterReflectionPlanarRendering()
	{
		return GraphicsSettings.IsWaterReflectionPlanarActive( this )
			&& !AreAllTilesCulled()
			&& !IsWaterReflectionPlanarDistanceCulled();
	}

	// True when the camera is farther from the water surface centre than the
	// distance-culling limit. Beyond this range the reflection isn't worth rendering,
	// so the planar camera shuts off and the tiles fall back to the cubemap.
	private bool IsWaterReflectionPlanarDistanceCulled()
	{
		var camera = Scene.Camera;
		if ( !camera.IsValid() )
			return true;

		// Top-middle of the water bounds: horizontal centre at the displaced surface height.
		var bounds = GetWorldBounds();
		var surfaceCentre = new Vector3( bounds.Center.x, bounds.Center.y, GetWaterSurfaceZWithOffset() );

		float distanceMeters = UnitConversion.InchesToMeters( camera.WorldPosition.Distance( surfaceCentre ) );
		return distanceMeters > WaterReflectionPlanarDistanceCulling;
	}

	// The mode last pushed to the tiles. Null means dirty - e.g. tiles were rebuilt
	// and need the combo even though the mode itself didn't change.
	private bool? _lastWaterReflectionPlanarRendering;

	// Push this water's resolved reflection mode to its own tiles. Resolved per water
	// so a Static water keeps its own choice regardless of the GraphicsSettings mode,
	// and so a culled water falls back to the cubemap.
	// 0 = Planar, 1 = Cubemap (matches the D_Water_Reflection_Mode combo).
	public void ApplyWaterReflectionModeToTiles()
	{
		bool planarRendering = IsWaterReflectionPlanarRendering();
		_lastWaterReflectionPlanarRendering = planarRendering;

		int waterReflectionMode = planarRendering ? 0 : 1;

		foreach ( var renderer in GetTileRenderers() )
		{
			if ( renderer.SceneObject.IsValid() )
				renderer.SceneObject.Attributes.SetCombo( "D_Water_Reflection_Mode", waterReflectionMode );
		}
	}

	// Turn the reflection off: disable the camera GameObject and release its texture.
	// The camera COMPONENT's enabled flag is never touched - it stays permanently
	// disabled and is driven by RenderToTexture. RenderWaterReflectionPlanar re-enables
	// the GameObject and re-creates the texture when planar becomes active again.
	public void DisableWaterReflectionPlanar()
	{
		if (WaterReflectionCameraGameObject.IsValid())
			WaterReflectionCameraGameObject.Enabled = false;

		WaterReflectionTexture?.Dispose();
		WaterReflectionTexture = null;
	}

	private float _waterReflectionPlanarLastRenderTime;

	private void RenderWaterReflectionPlanar()
	{
		if ( !WaterReflectionCameraComponent.IsValid() )
			return;

		// Planar isn't rendering (Cubemap selected, or every tile culled) - shut the
		// camera down and let the tiles fall back to the cubemap.
		if ( !IsWaterReflectionPlanarRendering() )
		{
			DisableWaterReflectionPlanar();
			return;
		}

		if ( !Scene.Camera.IsValid() )
			return;

		// On the very first frame the render context isn't ready and Screen.Size is zero;
		// creating a zero-sized render target yields a null texture and NREs below.
		if ( Screen.Width <= 0 || Screen.Height <= 0 )
			return;

		// Planar is rendering again - undo the GameObject disable.
		if ( WaterReflectionCameraGameObject.IsValid() && !WaterReflectionCameraGameObject.Enabled )
			WaterReflectionCameraGameObject.Enabled = true;

		// Frames-per-second limiter - re-rendering the reflection every frame hogs
		// performance, so only re-fire the render once the target interval has
		// elapsed. The tiles keep sampling the last rendered texture in between.
		float framesPerSecond = GraphicsSettings.GetWaterReflectionPlanarFramesPerSecond( this );
		if ( framesPerSecond > 0f && RealTime.Now - _waterReflectionPlanarLastRenderTime < 1f / framesPerSecond )
			return;

		_waterReflectionPlanarLastRenderTime = RealTime.Now;

		// The reflection plane in world space — at the actual water SURFACE height
		// (_waterZ + SubmersionOffset, the displaced surface), not the GameObject origin.
		var waterNormalAxis = WorldTransform.NormalToWorld( WaterNormalAxis.Normal );
		var waterPosition = WorldPosition.WithZ( GetWaterSurfaceZWithOffset() );

		// Mirror the main camera across the plane.
		var cameraPosition = Scene.Camera.WorldPosition;
		var mirroredPosition = cameraPosition - 2f * Vector3.Dot( cameraPosition - waterPosition, waterNormalAxis ) * waterNormalAxis;

		var forward = Scene.Camera.WorldRotation.Forward;
		var up = Scene.Camera.WorldRotation.Up;
		var mirroredForward = forward - 2f * Vector3.Dot( forward, waterNormalAxis ) * waterNormalAxis;
		var mirroredUp = up - 2f * Vector3.Dot( up, waterNormalAxis ) * waterNormalAxis;

		var mirroredRotation = Rotation.LookAt( mirroredForward, mirroredUp );

		// Apply debug offsets in the mirror camera's local space so you can dial in the correct transform.
		WaterReflectionCameraComponent.WorldRotation = mirroredRotation;
		WaterReflectionCameraComponent.WorldPosition = mirroredPosition;

		SyncWaterReflectionPlanarCameraWithSceneCamera();

		// Render into the reflection texture on demand and hand it to every water
		// tile's shader. RenderToTexture fires a single render from this camera -
		// the FPS limiter above is what makes it on-demand instead of every frame.
		// Divider below 1 would upscale past screen size (or divide by zero) - floor it at 1.
		int waterReflectionPlanarResolution = Math.Max( 1, GraphicsSettings.GetWaterReflectionPlanarResolution( this ) );
		WaterReflectionTexture = Texture.CreateRenderTarget( "PlanarReflection", ImageFormat.RGBA16161616F, Screen.Size / waterReflectionPlanarResolution, WaterReflectionTexture );
		WaterReflectionCameraComponent.RenderToTexture( WaterReflectionTexture );

		foreach ( var renderer in GetTileRenderers() )
		{
			if ( renderer.SceneObject.IsValid() )
				renderer.SceneObject.Attributes.Set( "ReflectionTexture", WaterReflectionTexture );
		}
	}
}
