namespace AtmokineticAssets;

public sealed partial class CameraController : BasePostProcess<CameraController>
{
	public Material material {get;set;} = Material.Load("materials/mat-camera_weather.vmat");
	public bool IsCameraSubmerged { get; private set; }
	public bool IsCameraFullySubmerged { get; private set; }
	public float TimeSinceCameraLastExitedWater;
	public bool WasCameraFullySubmerged;
	private bool _transitioningFromFullySubmerged;
	public WaterController WaterController;
	public CameraComponent CameraComponent;

	protected override void OnAwake()
	{
		CameraComponent = GameObject.GetComponent<CameraComponent>();
	}

	protected override void OnUpdate()
	{
		CheckWaterSubmersion();
		UpdateWaterDrips();
	}

	public override void Render()
	{
		if (WaterLineCameraComponent.IsValid())
		{
			RenderWaterLineTexture();
		}

		if (WaterController.IsValid())
		{
			UnderWaterShaderAttributes();
			// WaterDripsShaderAttributes();
			WaterLineShaderAttributes();
		}

		WaterDripsFadeShaderAttribute();
		Attributes.Set("Is Camera Submerged", (IsCameraSubmerged && !IsCameraFullySubmerged) ? 1f : 0f);
		Attributes.Set("Is Fully Submerged", IsCameraFullySubmerged ? 1f : 0f);

		Blit(BlitMode.WithBackbuffer(material, Stage.BeforePostProcess, 100), "Water Post Process");
	}
}
