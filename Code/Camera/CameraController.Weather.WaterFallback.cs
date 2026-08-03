namespace AtmokineticAssets;

/// <summary>
/// Camera water effect state for external water volume implementations
/// Allow camera presentation without requiring the provider to create
/// tiles, buoyancy, reflections, etc. Won't magically create the wave as nicely,
/// but the underwater/transition effects are generally still even without them
/// </summary>
public struct CameraWaterFallbackState
{
	public Guid VolumeId;
	public BBox Bounds;
	public int SubmergedCornerCount;
	public int FullySubmergedCornerCount;
	public float WaterLineSize;
	public float WaterLineBlur;
	public Color WaterLineColor;
	public float UnderWaterBlur;
	public Color UnderWaterColor;
}

/// <summary>
/// Camera water state from water volumes not managed by an Atmokinetic
/// <see cref="WaterController"/>.
/// </summary>
public interface ICameraWaterFallbackProvider
{
	bool TryGetCameraWaterState( Vector3[] nearPlaneCorners, out CameraWaterFallbackState state );
}

public sealed partial class CameraController : BasePostProcess<CameraController>
{
	private const string FallbackWaterLineModelPath = "models/mdl-water_tile.vmdl";
	private const string FallbackWaterRippleTexturePath = "textures/tex-water_ripples.vtex";
	private const float FallbackWaterLineModelScale = 5f;

	public ICameraWaterFallbackProvider WaterFallbackProvider { get; set; }

	private CameraWaterFallbackState _fallbackWaterState;
	private bool _usingFallbackWater;
	private GameObject _fallbackWaterLineGameObject;
	private ModelRenderer _fallbackWaterLineRenderer;
	private Model _fallbackWaterLineModel;
	private Texture _fallbackWaterRippleTexture;

	private bool TryGetFallbackWaterState( Vector3[] corners, out CameraWaterFallbackState state )
	{
		state = default;
		return WaterFallbackProvider != null
			&& WaterFallbackProvider.TryGetCameraWaterState( corners, out state )
			&& state.SubmergedCornerCount > 0;
	}

	private void CreateActiveWaterLineMesh()
	{
		if ( _usingFallbackWater )
		{
			CreateFallbackWaterLineMesh();
			return;
		}

		WaterController?.CreateWaterLineMesh();
	}

	private void DestroyActiveWaterLineMesh()
	{
		if ( _usingFallbackWater )
		{
			DestroyFallbackWaterLineMesh();
			return;
		}

		WaterController?.DestroyWaterLineMesh();
	}

	private void CreateFallbackWaterLineMesh()
	{
		if ( _fallbackWaterLineGameObject.IsValid() )
		{
			UpdateFallbackWaterLineMesh();
			return;
		}

		_fallbackWaterLineModel ??= Model.Load( FallbackWaterLineModelPath );
		if ( _fallbackWaterLineModel == null )
			return;

		_fallbackWaterRippleTexture ??= Texture.Load( FallbackWaterRippleTexturePath );

		_fallbackWaterLineGameObject = new GameObject( true, "Fallback Water Line" )
		{
			Parent = GameObject
		};
		_fallbackWaterLineGameObject.Flags |= GameObjectFlags.NotNetworked;
		_fallbackWaterLineGameObject.NetworkMode = NetworkMode.Never;
		_fallbackWaterLineGameObject.Tags.Add( "water_line_mesh" );
		_fallbackWaterLineGameObject.LocalScale = Vector3.One * FallbackWaterLineModelScale;

		_fallbackWaterLineRenderer = _fallbackWaterLineGameObject.Components.Create<ModelRenderer>();
		_fallbackWaterLineRenderer.Model = _fallbackWaterLineModel;
		_fallbackWaterLineRenderer.RenderType = ModelRenderer.ShadowRenderType.Off;
		_fallbackWaterLineRenderer.Attributes.SetCombo( "D_TRANSLUCENT", 0 );
		_fallbackWaterLineRenderer.Attributes.SetCombo( "D_Enable_Water_Line", 1 );
		_fallbackWaterLineRenderer.SetBodyGroup( "subdivision", 0 );

		ConfigureFallbackWaterLineRenderer();
		UpdateFallbackWaterLineMesh();
	}

	private void ConfigureFallbackWaterLineRenderer()
	{
		if ( !_fallbackWaterLineRenderer.IsValid() )
			return;

		var bounds = _fallbackWaterState.Bounds;
		var rendererAttributes = _fallbackWaterLineRenderer.SceneObject.Attributes;
		rendererAttributes.Set( "BoundsMin", new Vector2( bounds.Mins.x, bounds.Mins.y ) );
		rendererAttributes.Set( "BoundsMax", new Vector2( bounds.Maxs.x, bounds.Maxs.y ) );
		rendererAttributes.Set( "WaterRipplesTextureA", _fallbackWaterRippleTexture );
		rendererAttributes.Set( "Water Ripples Scale A", 0.01f );
		rendererAttributes.Set( "Water Ripples Rotation A", 0f );
		rendererAttributes.Set( "Water Ripples Speed A", 0.1f );
		rendererAttributes.Set( "Water Ripples Displacement A", 0.5f );
		rendererAttributes.Set( "WaterRipplesTextureB", _fallbackWaterRippleTexture );
		rendererAttributes.Set( "Water Ripples Scale B", 0.01f );
		rendererAttributes.Set( "Water Ripples Rotation B", 180f );
		rendererAttributes.Set( "Water Ripples Speed B", 0.1f );
		rendererAttributes.Set( "Water Ripples Displacement B", 0.5f );
	}

	private void UpdateFallbackWaterLineMesh()
	{
		if ( !_fallbackWaterLineGameObject.IsValid() || !CameraComponent.IsValid() )
			return;

		var bounds = _fallbackWaterState.Bounds;
		var cameraPosition = CameraComponent.WorldPosition;

		// The mask only needs to cover the near plane, so it follows the local camera
		// instead of allocating a complete water tile grid
		_fallbackWaterLineGameObject.WorldPosition = new Vector3( cameraPosition.x, cameraPosition.y, bounds.Maxs.z );
		_fallbackWaterLineGameObject.WorldRotation = Rotation.Identity;
	}

	private void DestroyFallbackWaterLineMesh()
	{
		if ( _fallbackWaterLineGameObject.IsValid() )
			_fallbackWaterLineGameObject.Destroy();

		_fallbackWaterLineGameObject = null;
		_fallbackWaterLineRenderer = null;
	}

	private void FallbackUnderWaterShaderAttributes()
	{
		Attributes.Set( "Under Water Blur", _fallbackWaterState.UnderWaterBlur );
		Attributes.Set( "Under Water Color", _fallbackWaterState.UnderWaterColor );
	}

	private void FallbackWaterLineShaderAttributes()
	{
		Attributes.Set( "Water Line Size", _fallbackWaterState.WaterLineSize );
		Attributes.Set( "Water Line Blur", _fallbackWaterState.WaterLineBlur );
		Attributes.Set( "Water Line Color", _fallbackWaterState.WaterLineColor );
	}
}
