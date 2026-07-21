namespace AtmokineticAssets;

public sealed partial class EnvironmentController : Component
{
	/// <summary>
	/// Every WaterController in the scene, cached on awake so consumers
	/// (e.g. GraphicsSettings) don't re-scan the scene.
	/// </summary>
	public static IReadOnlyList<WaterController> CachedWaterControllers => _cachedWaterControllers;
	private static readonly List<WaterController> _cachedWaterControllers = new();

	public void CacheWaterControllers()
	{
		_cachedWaterControllers.Clear();
		_cachedWaterControllers.AddRange( Scene.GetAllComponents<WaterController>() );
	}
}
