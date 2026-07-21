namespace AtmokineticAssets;

public sealed partial class WaterController : Component, Component.ExecuteInEditor
{
	public enum WaterReflectionModeEnumeration
	{
		Dynamic,
		Static
	}

	[Property, Feature("Reflection"), Header("Mode")]
	[Sync]
	public WaterReflectionModeEnumeration WaterReflectionMode
	{
		get;
		set { field = value; ApplyWaterReflectionMode(); }
	} = WaterReflectionModeEnumeration.Dynamic;

	public enum WaterReflectionModeStaticModeEnumeration
	{
		Planar,
		Cubemap
	}

	[Property, Feature("Reflection"), ShowIf( nameof(WaterReflectionMode), WaterReflectionModeEnumeration.Static)]
	[Sync]
	public WaterReflectionModeStaticModeEnumeration WaterReflectionModeStaticMode
	{
		get;
		set { field = value; ApplyWaterReflectionMode(); }
	} = WaterReflectionModeStaticModeEnumeration.Planar;

	// Mark the reflection mode dirty after its own settings change. Deliberately does
	// NOT apply immediately: these setters also fire from [Sync] snapshots while a
	// client is joining, before the scene camera exists - frustum work here NREs.
	// OnUpdate's change-check applies it on the next frame instead.
	private void ApplyWaterReflectionMode()
	{
		_lastWaterReflectionPlanarRendering = null;
	}

	public Vector3 WaterNormalAxis { get; set; } = Vector3.Up;
	
}
