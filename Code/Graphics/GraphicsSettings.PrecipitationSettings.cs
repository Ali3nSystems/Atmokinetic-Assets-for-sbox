namespace AtmokineticAssets;

public sealed partial class GraphicsSettings : Component, Component.ExecuteInEditor
{

    public enum PrecipitationSettingsRenderingModeEnumeration
    {
        Standard,
        Culled
    }

    //[Property, Feature("Precipitation"), Title("Rendering")]
    public PrecipitationSettingsRenderingModeEnumeration PrecipitationSettingsRenderingMode { get; set; } = PrecipitationSettingsRenderingModeEnumeration.Standard;

    public enum PrecipitationSettingsQualityModeEnumeration
    {
        Ultra,
        High,
        Medium,
        Low
    }

    //[Property, Feature("Precipitation"), Title("Quality")]
    public PrecipitationSettingsQualityModeEnumeration PrecipitationSettingsQualityMode { get; set; } = PrecipitationSettingsQualityModeEnumeration.Medium;
}