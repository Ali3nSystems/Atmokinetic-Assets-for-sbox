namespace AtmokineticAssets;
public sealed partial class WaterController : Component
{
	[Property, Title("Size"), Feature("Post Processing"), Group("Water Line")]
	[Sync]
	public float WaterLineSize { get; set;} = 0.1f;

	[Property, Title("Blur"), Feature("Post Processing"), Group("Water Line")]
	[Sync]
	public float WaterLineBlur { get; set;} = 0.1f;

	[Property, Title("Color"), Feature("Post Processing"), Group("Water Line")]
	[Sync]
	public Color WaterLineColor { get; set;} = Color.Parse( "#000e11" ).Value;

	[Property, Title("Blur"), Feature("Post Processing"), Group("Under Water")]
	[Sync]
	public float UnderWaterBlur { get; set;} = 0.1f;

	[Property, Title("Color"), Feature("Post Processing"), Group("Under Water")]
	[Sync]
	public Color UnderWaterColor { get; set;} = Color.Parse( "#084c6d" ).Value;

	// [Property, Title("Scale"), Feature("Post Processing"), Group("Water Drips")]
	// [Sync]
	// public float WaterDripsScale { get; set;} = 1f;

	// [Property, Title("Speed"), Feature("Post Processing"), Group("Water Drips")]
	// [Sync]
	// public float WaterDripsSpeed{ get; set;} = 1f;

	// [Property, Title("Strength"), Feature("Post Processing"), Group("Water Drips")]
	// [Sync]
	// public float WaterDripsStrength{ get; set;} = 8f;

	// [Property, Title("Color"), Feature("Post Processing"), Group("Water Drips")]
	// [Sync]
	// public Color WaterDripsColor { get; set;} = Color.Parse( "#1a4d66" ).Value;
}