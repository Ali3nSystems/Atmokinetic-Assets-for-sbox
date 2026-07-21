namespace AtmokineticAssets;

public sealed partial class WaterController : Component, Component.ExecuteInEditor
{
    
   	/// <summary>
	/// How heavy the fluid is. Higher values make things float more.
	/// Water is 1000, oil is around 900, something very heavy like mercury is 13600.
	/// </summary>
	[Property, Feature("Buoyancy"), Group( "Fluid" ), Title("Fluid Density"), Range( 0, 15000 )]
	[Sync]
	public float WaterBuoyancyFluidDensity { get; set; } = 1000.0f;

	/// <summary>
	/// Direction and speed of the water current.
	/// </summary>
	[Property, Feature("Buoyancy"), Group( "Fluid" ), Title("Fluid Velocity")]
	[Sync]
	public Vector3 WaterBuoyancyFluidVelocity { get; set; } = Vector3.Zero;

	/// <summary>
	/// How much the fluid slows down movement.
	/// </summary>
	[Property, Feature("Buoyancy"), Group( "Fluid" ), Title("Linear Drag"),Range( 0, 1 )]
	[Sync]
	public float WaterBuoyancyLinearDrag { get; set; } = 0.1f;

	/// <summary>
	/// How much the fluid slows down spinning.
	/// </summary>
	[Property, Feature("Buoyancy"), Group( "Fluid" ), Title("Angular Drag"), Range( 0, 2 )]
	[Sync]
	public float WaterBuoyancyAngularDrag { get; set; } = 0.5f;

	/// <summary>
	/// Moves the water surface up or down.
	/// </summary>
	[Property, Feature("Buoyancy"), Group( "Surface" ), Title("Surface Offset")]
	[Sync]
	public float WaterBuoyancySurfaceOffset { get; set; } = 0.0f;

	/// <summary>
	/// How tall the waves are. Set to 0 for calm water.
	/// </summary>
	[Property, Feature("Buoyancy"), Group( "Surface" ), Title("Wave Amplitude"), Range( 0, 100 )]
	[Sync]
	public float WaterBuoyancyWaveAmplitude { get; set; } = 0.1f;

	/// <summary>
	/// How fast the waves move.
	/// </summary>
	[Property, Feature("Buoyancy"), Group( "Surface" ), Title("Wave Frequency"), Range( 0, 5 )]
	[Sync]
	public float WaterBuoyancyWaveFrequency { get; set; } = 0.5f;

	/// <summary>
	/// Objects with any of these tags won't be affected.
	/// </summary>
	[Property, Feature("Buoyancy"), Group( "Filtering" ), Title("Ignore Tags")]
	[Sync]
	public TagSet WaterBuoyancyIgnoreTags { get; set; } = [];

	private WaterVolume _waterBuoyancy;

	public void CreateWaterBuoyancy()
	{
		_waterBuoyancy ??= Components.Get<Sandbox.WaterVolume>( FindMode.InSelf ) ?? Components.Create<Sandbox.WaterVolume>();
		_waterBuoyancy.Flags |= ComponentFlags.Hidden;

		_waterBuoyancy.FluidDensity = WaterBuoyancyFluidDensity;
		_waterBuoyancy.FluidVelocity = WaterBuoyancyFluidVelocity;
		_waterBuoyancy.LinearDrag = WaterBuoyancyLinearDrag;
		_waterBuoyancy.AngularDrag = WaterBuoyancyAngularDrag;
		_waterBuoyancy.SurfaceOffset = _waterZ - WorldPosition.z + WaterBuoyancySurfaceOffset;
		_waterBuoyancy.WaveAmplitude = WaterBuoyancyWaveAmplitude;
		_waterBuoyancy.WaveFrequency = WaterBuoyancyWaveFrequency;
		_waterBuoyancy.IgnoreTags = WaterBuoyancyIgnoreTags;
	}
}