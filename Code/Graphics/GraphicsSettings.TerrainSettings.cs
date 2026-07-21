namespace AtmokineticAssets;

public sealed partial class GraphicsSettings : Component, Component.ExecuteInEditor
{
    //Terrain Settings
    public enum TerrainRenderingModeEnumeration
    {
        [Title("Parallax Occlusion Mapping")]
        ParallaxOcclusionMapping,
        
        [Title("Bump Offset")]
        BumpOffset,
       
        Standard
    }

    //[Property, Feature("Terrain"), Title("Rendering")]
    public TerrainRenderingModeEnumeration TerrainRenderingMode { get; set; } = TerrainRenderingModeEnumeration.Standard;

    public enum TerrainRenderingParallaxOcclusionMappingQualityEnumeration
    {
        Ultra,
        High,
        Medium,
        Low
    }

    //[Property, Feature("Terrain"), Title("Quality")] 
    [ShowIf(nameof(TerrainRenderingMode), TerrainRenderingModeEnumeration.ParallaxOcclusionMapping)]
    public TerrainRenderingParallaxOcclusionMappingQualityEnumeration TerrainRenderingParallaxOcclusionMapping { get; set; } = TerrainRenderingParallaxOcclusionMappingQualityEnumeration.Medium;

    public enum TerrainRenderingBumpOffsetQualityEnumeration
    {
        Ultra,
        High,
        Medium,
        Low
    }
   
    //[Property, Feature("Terrain"), Title("Quality")] 
    [ShowIf(nameof(TerrainRenderingMode), TerrainRenderingModeEnumeration.BumpOffset)]
    public TerrainRenderingParallaxOcclusionMappingQualityEnumeration TerrainRenderingBumpOffset { get; set; } = TerrainRenderingParallaxOcclusionMappingQualityEnumeration.Medium;


    // public void ChangeTerrainSettings()
    // {
    //             // Terrain rendering mode: 0 = POM, 1 = Bump Offset, 2 = Standard
    //     int terrainRenderingMode = TerrainRenderingMode switch
    //     {
    //         TerrainRenderingModeEnumeration.ParallaxOcclusionMapping => 0,
    //         TerrainRenderingModeEnumeration.BumpOffset => 1,
    //         TerrainRenderingModeEnumeration.Standard => 2,
    //         _ => 1
    //     };

    //     // Quality: Ultra = 1, High = 2, Medium = 4, Low = 8
    //     float quality;
    //     switch ( TerrainRenderingMode )
    //     {
    //         case TerrainRenderingModeEnumeration.ParallaxOcclusionMapping:
    //             quality = TerrainRenderingParallaxOcclusionMapping switch
    //             {
    //                 TerrainRenderingParallaxOcclusionMappingQualityEnumeration.Ultra => 1f,
    //                 TerrainRenderingParallaxOcclusionMappingQualityEnumeration.High => 2f,
    //                 TerrainRenderingParallaxOcclusionMappingQualityEnumeration.Medium => 4f,
    //                 TerrainRenderingParallaxOcclusionMappingQualityEnumeration.Low => 8f,
    //                 _ => 4f
    //             };
    //             break;

    //         case TerrainRenderingModeEnumeration.BumpOffset:
    //             quality = TerrainRenderingBumpOffset switch
    //             {
    //                 TerrainRenderingParallaxOcclusionMappingQualityEnumeration.Ultra => 1f,
    //                 TerrainRenderingParallaxOcclusionMappingQualityEnumeration.High => 2f,
    //                 TerrainRenderingParallaxOcclusionMappingQualityEnumeration.Medium => 4f,
    //                 TerrainRenderingParallaxOcclusionMappingQualityEnumeration.Low => 8f,
    //                 _ => 4f
    //             };
    //             break;

    //         default:
    //             quality = 4f;
    //             break;
    //     }

    //     // Enumerate live — objects spawned after startup (e.g. baked water tiles) must be included.
    //     foreach ( var sceneObject in Scene.SceneWorld.SceneObjects )
    //     {
    //         if ( sceneObject is null || !sceneObject.IsValid() ) continue;
    //         sceneObject.Attributes.Set( "D_Rendering", terrainRenderingMode );
    //         sceneObject.Attributes.Set( "Quality", quality );
    //     }
    // }
}