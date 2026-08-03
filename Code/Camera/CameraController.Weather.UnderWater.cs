namespace AtmokineticAssets;

public sealed partial class CameraController : BasePostProcess<CameraController>
{
    public void UnderWaterShaderAttributes()
    {
		if ( _usingFallbackWater )
		{
			FallbackUnderWaterShaderAttributes();
			return;
		}

        Attributes.Set("Under Water Blur", WaterController.UnderWaterBlur);
        Attributes.Set("Under Water Color", WaterController.UnderWaterColor);
    }	
}
