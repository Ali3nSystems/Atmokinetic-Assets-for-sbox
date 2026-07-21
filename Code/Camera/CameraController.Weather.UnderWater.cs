namespace AtmokineticAssets;

public sealed partial class CameraController : BasePostProcess<CameraController>
{
    public void UnderWaterShaderAttributes()
    {
        Attributes.Set("Under Water Blur", WaterController.UnderWaterBlur);
        Attributes.Set("Under Water Color", WaterController.UnderWaterColor);
    }	
}