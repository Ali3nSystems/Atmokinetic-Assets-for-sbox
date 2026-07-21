// namespace AtmokineticAssets;

// public sealed partial class WaterController : Component
// {
// 	public enum WetnessVolumeModeEnumeration
// 	{
// 		Cube,
// 		Cylinder
// 	}

// 	[Property, Feature("Wetness")]
// 	public WetnessVolumeModeEnumeration WetnessVolumeMode { get; set; } = WetnessVolumeModeEnumeration.Cube;

//     [Property, Title( "Extend (cm)" ), Feature("Wetness"), Group( "Properties" )]
// 	public float WetnessVolumeExtend { get; set { field = MathF.Max( 0f, value ); UpdateWetnessVolumeCubeBuffer(); UpdateWetnessVolumeCylinderBuffer();} } = 25f;

// 	[Property, Title( "Fade" ), Feature("Wetness"), Group( "Properties" ), Range( 0f, 1f )]
// 	public float WetnessVolumeFade { get; set { field = value; UpdateWetnessVolumeCubeBuffer(); UpdateWetnessVolumeCylinderBuffer(); } } = 0.1f;

// 	//Add the height blend here


// 	// Also get rid of the wetness references, it's old stuff

// 	[Property, Title( "Softness" ), Feature("Wetness"), Group( "Properties" ), Range( 0f, 1f )]
// 	public float WetnessVolumeSoftness { get; set { field = value; UpdateWetnessVolumeCubeBuffer(); UpdateWetnessVolumeCylinderBuffer(); } } = 0.1f;
// }