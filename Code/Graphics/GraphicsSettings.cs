namespace AtmokineticAssets;

[Title( "Atmokinetic Assets - Graphics Settings" )]
[Category( "Atmokinetic Assets" )]
[Icon( "settings" )]

public sealed partial class GraphicsSettings : Component, Component.ExecuteInEditor
{
    /// <summary>
    /// The scene's GraphicsSettings. There should only ever be one.
    /// </summary>
    public static GraphicsSettings Instance { get; private set; }

    protected override void OnAwake()
    {
        if ( Instance.IsValid() && Instance != this )
            Log.Warning( $"Multiple GraphicsSettings components in the scene - {GameObject.Name} is replacing {Instance.GameObject.Name}" );

        Instance = this;
    }

    protected override void OnDestroy()
    {
        if ( Instance == this )
            Instance = null;
    }

    protected override void OnStart()
    {
        ApplyWaterReflectionModeDynamic();
    }

    protected override void OnUpdate()
    {

    }
}