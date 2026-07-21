namespace AtmokineticAssets;

// Also add:

//Ice
//Caustics

public sealed partial class WaterController : Component, Component.ExecuteInEditor
{
	// Surface
	[Property, Feature("Attributes"), Group("Surface"), Title("Color"), Order(1)]
	[Sync]
	public Color WaterSurfaceColor { get; set { field = value; _surfaceAttributesDirty = true; } } = Color.Parse( "#c5d9ff" ).Value;

	[Property, Feature("Attributes"), Group("Surface"), Title("Blur"), Order(2)]
	[Sync]
	public float WaterSurfaceBlur { get; set { field = value; _surfaceAttributesDirty = true; } } = 0;

	// Reflection
	[Property, Feature("Attributes"), Group("Reflection"), Title("Color"), Order(3)]
	[Sync]
	public Color WaterReflectionColor { get; set { field = value; _surfaceAttributesDirty = true; } } = Color.Parse( "#c5d9ff" ).Value;

	[Property, Feature("Attributes"), Group("Reflection"), Title("Blur"), Order(4)]
	[Sync]
	public float WaterReflectionBlur { get; set { field = value; _surfaceAttributesDirty = true; } } = 0;

	[Property, Feature("Attributes"), Group("Reflection"), Title("Distance"), Order(5)]
	[Sync]
	public float WaterReflectionDistance { get; set { field = value; _surfaceAttributesDirty = true; } } = 4f;

	[Property, Feature("Attributes"), Group("Reflection"), Title("Threshold"), Order(6)]
	[Sync]
	public Vector2 WaterReflectionThreshold { get; set { field = value; _surfaceAttributesDirty = true; } } = new Vector2( 0, 0.25f );

	// Shoreline
	[Property, Feature("Attributes"), Group("Shoreline"), Title("Fade"), Order(7)]
	[Sync]
	public float WaterShorelineFade { get; set { field = value; _surfaceAttributesDirty = true; } } = 2f;

	// Tension
	[Property, Feature("Attributes"), Group("Tension"), Title("Distance"), Order(8)]
	[Sync]
	public float WaterTensionDistance { get; set { field = value; _surfaceAttributesDirty = true; } } = 2f;

	[Property, Feature("Attributes"), Group("Tension"), Title("Refraction"), Order(9)]
	[Sync]
	public float WaterTensionRefraction { get; set { field = value; _surfaceAttributesDirty = true; } } = 0.1f;

	// Texture A
	[Property, Feature("Attributes"), Group("Water Ripples A"), Title("Texture A"), Order(10)]
	[Sync]
	public Texture WaterRipplesTextureA { get; set { field = value; _surfaceAttributesDirty = true; } } = Texture.Load("textures/tex-water_ripples.vtex");

	[Property, Feature("Attributes"), Group("Water Ripples A"), Title("Scale A"), Order(11)]
	[Sync]
	public float WaterRipplesScaleA { get; set { field = value; _surfaceAttributesDirty = true; } } = 0.01f;

	[Property, Feature("Attributes"), Group("Water Ripples A"), Title("Rotation A"), Order(12)]
	[Sync]
	public float WaterRipplesRotationA { get; set { field = value; _surfaceAttributesDirty = true; } } = 0f;

	[Property, Feature("Attributes"), Group("Water Ripples A"), Title("Speed A"), Order(13)]
	[Sync]
	public float WaterRipplesSpeedA { get; set { field = value; _surfaceAttributesDirty = true; } } = 0.1f;

	[Property, Feature("Attributes"), Group("Water Ripples A"), Title("Refraction A"), Order(14)]
	[Sync]
	public float WaterRipplesRefractionA { get; set { field = value; _surfaceAttributesDirty = true; } } = 0.05f;

	[Property, Feature("Attributes"), Group("Water Ripples A"), Title("Displacement A"), Order(15)]
	[Sync]
	public float WaterRipplesDisplacementA { get; set { field = value; _surfaceAttributesDirty = true; } } = 0.5f;

	// Texture B
	[Property, Feature("Attributes"), Group("Water Ripples B"), Title("Texture B"), Order(16)]
	[Sync]
	public Texture WaterRipplesTextureB { get; set { field = value; _surfaceAttributesDirty = true; } } = Texture.Load("textures/tex-water_ripples.vtex");

	[Property, Feature("Attributes"), Group("Water Ripples B"), Title("Scale B"), Order(17)]
	[Sync]
	public float WaterRipplesScaleB { get; set { field = value; _surfaceAttributesDirty = true; } } = 0.01f;

	[Property, Feature("Attributes"), Group("Water Ripples B"), Title("Rotation B"), Order(18)]
	[Sync]
	public float WaterRipplesRotationB { get; set { field = value; _surfaceAttributesDirty = true; } } = 180f;

	[Property, Feature("Attributes"), Group("Water Ripples B"), Title("Speed B"), Order(19)]
	[Sync]
	public float WaterRipplesSpeedB { get; set { field = value; _surfaceAttributesDirty = true; } } = 0.1f;

	[Property, Feature("Attributes"), Group("Water Ripples B"), Title("Refraction B"), Order(20)]
	[Sync]
	public float WaterRipplesRefractionB { get; set { field = value; _surfaceAttributesDirty = true; } } = 0.05f;

	[Property, Feature("Attributes"), Group("Water Ripples B"), Title("Displacement B"), Order(21)]
	[Sync]
	public float WaterRipplesDisplacementB { get; set { field = value; _surfaceAttributesDirty = true; } } = 0.5f;

	// Starts dirty so the first WaterSurfaceAttributes call always pushes.
	private bool _surfaceAttributesDirty = true;

	public void WaterSurfaceAttributes()
	{
		if ( !_surfaceAttributesDirty )
			return;

		_surfaceAttributesDirty = false;

		foreach ( var renderer in GetTileRenderers() )
		{

			if (WaterSurfaceBlur != 0)
			{	
				renderer.Attributes.SetCombo("D_Enable_Water_Surface_Blur", 1);
			}
			else
			{
				renderer.Attributes.SetCombo("D_Enable_Water_Surface_Blur", 0);
			}

			if (WaterReflectionBlur != 0)
			{
				renderer.Attributes.SetCombo("D_Enable_Water_Reflection_Blur", 1);
			}
			else
			{
				renderer.Attributes.SetCombo("D_Enable_Water_Reflection_Blur", 0);
			}


			// Surface
			renderer.Attributes.Set( "Water Surface Color", WaterSurfaceColor );
			renderer.Attributes.Set( "Water Surface Blur", WaterSurfaceBlur );

			// Reflection
			renderer.Attributes.Set( "Water Reflection Color", WaterReflectionColor );
			renderer.Attributes.Set( "Water Reflection Blur", WaterReflectionBlur );
			renderer.Attributes.Set( "Water Reflection Distance", WaterReflectionDistance );
			renderer.Attributes.Set( "Water Reflection Threshold", WaterReflectionThreshold );

			// Shoreline
			renderer.Attributes.Set( "Water Shoreline Fade", WaterShorelineFade );
			
			// Tension
			renderer.Attributes.Set( "Water Tension Distance", WaterTensionDistance );
			renderer.Attributes.Set( "Water Tension Refraction", WaterTensionRefraction );

			// Water Ripples A
			renderer.Attributes.Set( "WaterRipplesTextureA", WaterRipplesTextureA );
			renderer.Attributes.Set( "Water Ripples Scale A", WaterRipplesScaleA );
			renderer.Attributes.Set( "Water Ripples Rotation A", WaterRipplesRotationA );
			renderer.Attributes.Set( "Water Ripples Speed A", WaterRipplesSpeedA );
			renderer.Attributes.Set( "Water Ripples Refraction A", WaterRipplesRefractionA );
			renderer.Attributes.Set( "Water Ripples Displacement A", WaterRipplesDisplacementA );

			// Water Ripples B
			renderer.Attributes.Set( "WaterRipplesTextureB", WaterRipplesTextureB );
			renderer.Attributes.Set( "Water Ripples Scale B", WaterRipplesScaleB );
			renderer.Attributes.Set( "Water Ripples Rotation B", WaterRipplesRotationB );
			renderer.Attributes.Set( "Water Ripples Speed B", WaterRipplesSpeedB );
			renderer.Attributes.Set( "Water Ripples Refraction B", WaterRipplesRefractionB );
			renderer.Attributes.Set( "Water Ripples Displacement B", WaterRipplesDisplacementB );
		}
	}
}