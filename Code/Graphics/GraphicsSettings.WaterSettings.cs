namespace AtmokineticAssets;

public sealed partial class GraphicsSettings : Component, Component.ExecuteInEditor
{
    // Water Settings
    public enum WaterReflectionModeDynamicEnumeration
    {
        Planar,
        Cubemap
    }

    [Property, Feature("Water"), Title("Mode")]
    public WaterReflectionModeDynamicEnumeration WaterReflectionModeDynamic
    {
        get;
        set { field = value; ApplyWaterReflectionModeDynamic(); }
    } = WaterReflectionModeDynamicEnumeration.Cubemap;

    public enum WaterReflectionPlanarResolutionModeDynamicEnumeration
    {
        Ultra,
        High,
        Medium,
        Low
    }

    [Property,Feature("Water"), Title("Resolution")]
    [ShowIf(nameof(WaterReflectionModeDynamic), WaterReflectionModeDynamicEnumeration.Planar)]
    public WaterReflectionPlanarResolutionModeDynamicEnumeration WaterReflectionPlanarResolutionModeDynamic {get;set;}= WaterReflectionPlanarResolutionModeDynamicEnumeration.High;

    public enum WaterReflectionPlanarFramesPerSecondModeDynamicEnumeration
    {
        Ultra,
        High,
        Medium,
        Low
    }

    [Property,Feature("Water"), Title("Frames Per Second")]
    [ShowIf(nameof(WaterReflectionModeDynamic), WaterReflectionModeDynamicEnumeration.Planar)]
    public WaterReflectionPlanarFramesPerSecondModeDynamicEnumeration WaterReflectionPlanarFramesPerSecondModeDynamic {get;set;}= WaterReflectionPlanarFramesPerSecondModeDynamicEnumeration.High;

    /// <summary>
    /// True when the given water should render planar reflections. Static mode uses
    /// the water's own configuration and ignores GraphicsSettings; Dynamic mode
    /// follows GraphicsSettings, falling back to the water's Static configuration
    /// when no GraphicsSettings exists.
    /// </summary>
    public static bool IsWaterReflectionPlanarActive( WaterController water )
    {
        if ( water.WaterReflectionMode == WaterController.WaterReflectionModeEnumeration.Static || !Instance.IsValid() )
            return water.WaterReflectionModeStaticMode == WaterController.WaterReflectionModeStaticModeEnumeration.Planar;

        return Instance.WaterReflectionModeDynamic == WaterReflectionModeDynamicEnumeration.Planar;
    }

    /// <summary>
    /// The texture size divider the given water should use. 1 = screen, 2 = screen/2,
    /// 4 = screen/4. Static mode uses the water's own value; Dynamic mode resolves the
    /// tier selected by GraphicsSettings, falling back to the water's Static value
    /// when no GraphicsSettings exists.
    /// </summary>
    public static int GetWaterReflectionPlanarResolution( WaterController water )
    {
        if ( water.WaterReflectionPlanarResolutionMode == WaterController.WaterReflectionPlanarResolutionModeEnumeration.Static || !Instance.IsValid() )
            return water.WaterReflectionPlanarResolutionStatic;

        return Instance.WaterReflectionPlanarResolutionModeDynamic switch
        {
            WaterReflectionPlanarResolutionModeDynamicEnumeration.Ultra => water.WaterReflectionPlanarResolutionDynamicUltra,
            WaterReflectionPlanarResolutionModeDynamicEnumeration.High => water.WaterReflectionPlanarResolutionDynamicHigh,
            WaterReflectionPlanarResolutionModeDynamicEnumeration.Medium => water.WaterReflectionPlanarResolutionDynamicMedium,
            WaterReflectionPlanarResolutionModeDynamicEnumeration.Low => water.WaterReflectionPlanarResolutionDynamicLow,
            _ => water.WaterReflectionPlanarResolutionDynamicMedium
        };
    }

    /// <summary>
    /// The reflection render rate the given water should use. Static mode uses the
    /// water's own value; Dynamic mode resolves the tier selected by GraphicsSettings,
    /// falling back to the water's Static value when no GraphicsSettings exists.
    /// </summary>
    public static float GetWaterReflectionPlanarFramesPerSecond( WaterController water )
    {
        if ( water.WaterReflectionPlanarFramesPerSecondMode == WaterController.WaterReflectionPlanarFramesPerSecondModeEnumeration.Static || !Instance.IsValid() )
            return water.WaterReflectionPlanarFramesPerSecondStatic;

        return Instance.WaterReflectionPlanarFramesPerSecondModeDynamic switch
        {
            WaterReflectionPlanarFramesPerSecondModeDynamicEnumeration.Ultra => water.WaterReflectionPlanarFramesPerSecondDynamicUltra,
            WaterReflectionPlanarFramesPerSecondModeDynamicEnumeration.High => water.WaterReflectionPlanarFramesPerSecondDynamicHigh,
            WaterReflectionPlanarFramesPerSecondModeDynamicEnumeration.Medium => water.WaterReflectionPlanarFramesPerSecondDynamicMedium,
            WaterReflectionPlanarFramesPerSecondModeDynamicEnumeration.Low => water.WaterReflectionPlanarFramesPerSecondDynamicLow,
            _ => water.WaterReflectionPlanarFramesPerSecondDynamicMedium
        };
    }

    /// <summary>
    /// Pushes each water's resolved Planar/Cubemap choice to its own tiles and
    /// releases the reflection texture of any water that isn't rendering planar.
    /// Static-mode waters keep their own choice and are unaffected by this setting.
    /// </summary>
    private void ApplyWaterReflectionModeDynamic()
    {
        if ( Scene is null ) return;

        // The combo is resolved per water - a Static water keeps its own mode even
        // when the global Dynamic setting says otherwise, so it can't be stamped
        // scene-wide from the global value.
        foreach ( var water in EnvironmentController.CachedWaterControllers )
        {
            if ( !water.IsValid() ) continue;

            water.ApplyWaterReflectionModeToTiles();

            if ( !IsWaterReflectionPlanarActive( water ) )
                water.DisableWaterReflectionPlanar();
        }
    }
}