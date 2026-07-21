namespace AtmokineticAssets;
// Also add:

//Caustics
//Weather (Rain, Ice)
//Caustics
//Depth

//Finish Area Mode

public sealed partial class WaterController : Component, Component.ExecuteInEditor
{
	public enum WaterModeEnumeration
	{
		Tiles,
		//Area,
		//Custom
	}

	[Property, Feature("General")]
	public WaterModeEnumeration WaterMode { get; set; } = WaterModeEnumeration.Tiles;

	protected override void OnAwake()
	{
		CreateWaterReflectionPlanarCamera();
	
		rippleBuffer = new GpuBuffer<WaterSimulation>(1);
		currentBufferSize = 1;
	}

	protected override void OnStart()
	{
		GameObject.Tags.Add("water");
		CalculateBounds();
		CreateWaterCollider();
		CreateWaterBuoyancy();
		DiscoverBakedTiles();
		WaterSurfaceAttributes();
		ApplyWaterReflectionModeToTiles();

		_tilesStarted = true;	
	}

	protected override void OnEnabled()
	{
		_lastSnappedPosition = new Vector2( float.MinValue, float.MinValue );
	}


	protected override void OnPreRender()
	{
		RenderWaterReflectionPlanar();
	}

	protected override void OnUpdate()
	{
		// switch(WetnessVolumeMode)
		// {
		// 	case WetnessVolumeModeEnumeration.Cube:
		// 		UpdateWetnessVolumeCubeBuffer();
		// 		break;

		// 	case WetnessVolumeModeEnumeration.Cylinder:
		// 		UpdateWetnessVolumeCylinderBuffer();
		// 		break;
		// }
		
		// 	Log.Info( component.GetType().Name );
			
		UpdateRings();
		UpdateWaterSimulation();
		WaterSurfaceAttributes();
		
		// Culling changes as the camera moves, so the tiles' reflection mode has to
		// keep up - a fully culled water falls back to the cubemap. Only push to the
		// tiles when the resolved mode actually changes; the camera itself is
		// enabled/disabled to match by RenderWaterReflectionPlanar.
		if ( IsWaterReflectionPlanarRendering() != _lastWaterReflectionPlanarRendering )
			ApplyWaterReflectionModeToTiles();
	}

	protected override void OnDestroy()
	{
		rippleBuffer?.Dispose();
	}

	protected override void DrawGizmos()
	{
		switch(WaterMode)
		{
			case WaterModeEnumeration.Tiles:	

				DrawWaterTileGizmo();		
				break;
			/*
			case WaterModeEnumeration.Area:
				break;
		
			case WaterModeEnumeration.Custom:
				break;
			*/
		}

		// switch(WetnessVolumeMode)
		// {
		// 	case WetnessVolumeModeEnumeration.Cube:
		// 		DrawWetnessVolumeCubeGizmos();
		// 		break;

		// 	case WetnessVolumeModeEnumeration.Cylinder:
		// 		DrawWetnessVolumeCylinderGizmos();
		// 		break;
		// }

		//Water Reflection Planar
		ShowWaterReflectionPlanarCameraGameObject();

		WaterSurfaceAttributes();
	}
}