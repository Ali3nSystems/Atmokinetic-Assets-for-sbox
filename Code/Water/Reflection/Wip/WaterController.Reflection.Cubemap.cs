// namespace AtmokineticAssets;

// public sealed partial class WaterController : Component, Component.ExecuteInEditor
// {
// 	public bool ShowWaterReflectionCubemapSettings =>
// 	WaterReflectionMode == WaterReflectionModeEnumeration.Dynamic
// 	|| ( WaterReflectionMode == WaterReflectionModeEnumeration.Static
// 	&& WaterReflectionModeStaticMode == WaterReflectionModeStaticModeEnumeration.Cubemap );
// 	public bool ShowWaterReflectionCubemapResolutionDynamic => ShowWaterReflectionCubemapSettings && WaterReflectionCubemapResolutionMode == WaterReflectionCubemapResolutionModeEnumeration.Dynamic;
// 	public bool ShowWaterReflectionCubemapResolutionStatic => ShowWaterReflectionCubemapSettings && WaterReflectionCubemapResolutionMode == WaterReflectionCubemapResolutionModeEnumeration.Static;
// 	public bool ShowWaterReflectionCubemapFramesPerSecondDynamic => ShowWaterReflectionCubemapSettings && WaterReflectionCubemapFramesPerSecondMode == WaterReflectionCubemapFramesPerSecondModeEnumeration.Dynamic;
// 	public bool ShowWaterReflectionCubemapFramesPerSecondStatic => ShowWaterReflectionCubemapSettings && WaterReflectionCubemapFramesPerSecondMode == WaterReflectionCubemapFramesPerSecondModeEnumeration.Static;

// 	public enum WaterReflectionCubemapResolutionModeEnumeration
// 	{
// 		Dynamic,
// 		Static
// 	}

// 	[Property, Feature("Reflection"), Group("Cubemap"), Header("Resolution"), ShowIf( nameof(ShowWaterReflectionCubemapSettings), true )]
// 	public WaterReflectionCubemapResolutionModeEnumeration WaterReflectionCubemapResolutionMode {get;set;} = WaterReflectionCubemapResolutionModeEnumeration.Dynamic;

// 	[Property, Feature("Reflection"), Group("Cubemap"), ShowIf( nameof(ShowWaterReflectionCubemapResolutionDynamic), true )]
// 	public int WaterReflectionCubemapResolutionDynamicUltra {get;set;} = 1;

// 	[Property, Feature("Reflection"), Group("Cubemap"), ShowIf( nameof(ShowWaterReflectionCubemapResolutionDynamic), true )]
// 	public int WaterReflectionCubemapResolutionDynamicHigh {get;set;} = 2;

// 	[Property, Feature("Reflection"), Group("Cubemap"), ShowIf( nameof(ShowWaterReflectionCubemapResolutionDynamic), true )]
// 	public int WaterReflectionCubemapResolutionDynamicMedium {get;set;} = 4;

// 	[Property, Feature("Reflection"), Group("Cubemap"), ShowIf( nameof(ShowWaterReflectionCubemapResolutionDynamic), true )]
// 	public int WaterReflectionCubemapResolutionDynamicLow {get;set;} = 8;

// 	[Property, Feature("Reflection"), Group("Cubemap"), ShowIf( nameof(ShowWaterReflectionCubemapResolutionStatic), true )]
// 	public int WaterReflectionCubemapResolutionStatic {get;set;} = 2;

// 	/// <summary>
// 	///
// 	/// </summary>
// 	public enum WaterReflectionCubemapFramesPerSecondModeEnumeration
// 	{
// 		Dynamic,
// 		Static
// 	}

// 	[Property, Feature("Reflection"), Group("Cubemap"), Header("Frames Per Second"), ShowIf( nameof(ShowWaterReflectionCubemapSettings), true )]
// 	public WaterReflectionCubemapFramesPerSecondModeEnumeration WaterReflectionCubemapFramesPerSecondMode {get;set;} = WaterReflectionCubemapFramesPerSecondModeEnumeration.Dynamic;

// 	[Property, Feature("Reflection"), Group("Cubemap"), ShowIf( nameof(ShowWaterReflectionCubemapFramesPerSecondDynamic), true )]
// 	public float WaterReflectionCubemapFramesPerSecondDynamicUltra {get;set;} = 120f;

// 	[Property, Feature("Reflection"), Group("Cubemap"), ShowIf( nameof(ShowWaterReflectionCubemapFramesPerSecondDynamic), true )]
// 	public float WaterReflectionCubemapFramesPerSecondDynamicHigh {get;set;} = 60f;

// 	[Property, Feature("Reflection"), Group("Cubemap"), ShowIf( nameof(ShowWaterReflectionCubemapFramesPerSecondDynamic), true )]
// 	public float WaterReflectionCubemapFramesPerSecondDynamicMedium {get;set;} = 30f;

// 	[Property, Feature("Reflection"), Group("Cubemap"), ShowIf( nameof(ShowWaterReflectionCubemapFramesPerSecondDynamic), true )]
// 	public float WaterReflectionCubemapFramesPerSecondDynamicLow {get;set;} = 15f;

// 	[Property, Feature("Reflection"), Group("Cubemap"), ShowIf( nameof(ShowWaterReflectionCubemapFramesPerSecondStatic), true )]
// 	public float WaterReflectionCubemapFramesPerSecondStatic {get;set;} = 60f;
// }
