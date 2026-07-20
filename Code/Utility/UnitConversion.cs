namespace AtmokineticAssets;

public static class UnitConversion
{
	//Inches To Meters
	public static float InchesToMeters( float inches )
	{
		return inches * 0.0254f;
	}

	public static Vector2 InchesToMeters( Vector2 inches )
	{
		return inches * 0.0254f;
	}

	public static Vector3 InchesToMeters( Vector3 inches )
	{
		return inches * 0.0254f;
	}

	public static Vector4 InchesToMeters( Vector4 inches )
	{
		return inches * 0.0254f;
	}



	//Inches To Centimeters
	public static float InchesToCentimeters( float inches)
	{
		return inches * 2.54f;
	}

	public static Vector2 InchesToCentimeters( Vector2 inches)
	{
		return inches * 2.54f;
	}

	public static Vector3 InchesToCentimeters( Vector3 inches)
	{
		return inches * 2.54f;
	}

	public static Vector4 InchesToCentimeters( Vector4 inches)
	{
		return inches * 2.54f;
	}

	//Centimeters To Inches
	public static float CentimetersToInches( float centimeters )
	{
		return centimeters / 2.54f;
	}
	
	public static Vector2 CentimetersToInches( Vector2 centimeters )
	{
		return centimeters / 2.54f;
	}
	
	public static Vector3 CentimetersToInches( Vector3 centimeters )
	{
		return centimeters / 2.54f;
	}
	
	public static Vector4 CentimetersToInches( Vector4 centimeters )
	{
		return centimeters / 2.54f;
	}

	//Meters To Centimeters
	public static float MetersToCentimeters( float meters )
	{
		return meters * 100f;
	}

	//Meters To Inches
	public static float MetersToInches(float meters)
	{
		return meters / 0.0254f;
	}
}
