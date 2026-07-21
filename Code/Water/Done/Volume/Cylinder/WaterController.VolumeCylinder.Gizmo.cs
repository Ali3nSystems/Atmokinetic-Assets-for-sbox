// namespace AtmokineticAssets;

// public sealed partial class WaterController : Component
// {
// 	private const int CylinderSlices = 32;

// 	private void DrawWetnessVolumeCylinderGizmos()
// 	{
// 		if ( !EnableWetnessVolumeCylinderGizmos )
// 			return;

// 		if ( !Gizmo.IsSelected && !Gizmo.IsHovered )
// 			return;

// 		float radius = UnitConversion.CentimetersToInches( WetnessVolumeCylinderRadius );
// 		float depthMinimum = UnitConversion.CentimetersToInches( WetnessVolumeCylinderDepthMinimum );
// 		float depthMaximum = UnitConversion.CentimetersToInches( WetnessVolumeCylinderDepthMaximum );

// 		var top = new Vector3( 0, 0, depthMaximum );
// 		var bottom = new Vector3( 0, 0, -depthMinimum );

// 		DrawWetnessVolumeCylinderBounds( top, bottom, radius );
// 		DrawWetnessVolumeCylinderHandles( radius, depthMinimum, depthMaximum );

// 		UpdateWetnessVolumeCylinderBuffer();
// 	}

// 	private static void DrawWetnessVolumeCylinderBounds( Vector3 top, Vector3 bottom, float radius )
// 	{
// 		Gizmo.Draw.LineThickness = 1;
// 		var tint = Gizmo.IsSelected ? Gizmo.Colors.Blue : Gizmo.Colors.Blue.WithAlpha( 0.4f );

// 		for ( int i = 0; i < 2; i++ )
// 		{
// 			bool xray = i == 0;
// 			Gizmo.Draw.IgnoreDepth = xray;
// 			float alpha = xray ? 0.2f : 1f;

// 			Gizmo.Draw.Color = tint.WithAlpha( tint.a * alpha );
// 			Gizmo.Draw.LineCylinder( bottom, top, radius, radius, CylinderSlices );

// 			Gizmo.Draw.Color = tint.WithAlpha( 0.05f * alpha );
// 			Gizmo.Draw.SolidCylinder( bottom, top, radius, CylinderSlices );
// 		}
// 	}

// 	private void DrawWetnessVolumeCylinderHandles( float radius, float depthMinimum, float depthMaximum )
// 	{
// 		var previousSnap = Gizmo.Settings.SnapToGrid;
// 		Gizmo.Settings.SnapToGrid = false;

// 		DrawWetnessVolumeCylinderRadiusHandle( radius );
// 		DrawWetnessVolumeCylinderDepthMinimumHandle( depthMinimum );
// 		DrawWetnessVolumeCylinderDepthMaximumHandle( depthMaximum );

// 		Gizmo.Settings.SnapToGrid = previousSnap;
// 	}

// 	private void DrawWetnessVolumeCylinderRadiusHandle( float radius )
// 	{
// 		using ( Gizmo.Scope( "cylinder_radius", new Vector3( radius, 0, 0 ) ) )
// 		{
// 			Gizmo.Draw.IgnoreDepth = true;
// 			Gizmo.Hitbox.DepthBias = 0f;
// 			Gizmo.Draw.Color = Gizmo.Colors.Red;
// 			if ( Gizmo.Control.Arrow( "out", Vector3.Forward, out float delta, length: 32, girth: 32, cullAngle: 0 ) )
// 			{
// 				float newRadius = MathF.Max( 1f, radius + delta );
// 				WetnessVolumeCylinderRadius = newRadius * 2.54f;
// 			}
// 		}
// 	}

// 	private void DrawWetnessVolumeCylinderDepthMinimumHandle( float depthMinimum )
// 	{
// 		using ( Gizmo.Scope( "cylinder_depth_minimum", new Vector3( 0, 0, -depthMinimum ) ) )
// 		{
// 			Gizmo.Draw.IgnoreDepth = true;
// 			Gizmo.Hitbox.DepthBias = 0f;
// 			Gizmo.Draw.Color = Gizmo.Colors.Green;
// 			if ( Gizmo.Control.Arrow( "down", Vector3.Down, out float delta, length: 32, girth: 32, cullAngle: 0 ) )
// 			{
// 				float newDepth = MathF.Max( 1f, depthMinimum + delta );
// 				WetnessVolumeCylinderDepthMinimum = UnitConversion.InchesToCentimeters(newDepth);
// 			}
// 		}
// 	}

// 	private void DrawWetnessVolumeCylinderDepthMaximumHandle( float depthMaximum )
// 	{
// 		using ( Gizmo.Scope( "cylinder_depth_maximum", new Vector3( 0, 0, depthMaximum ) ) )
// 		{
// 			Gizmo.Draw.IgnoreDepth = true;
// 			Gizmo.Hitbox.DepthBias = 0f;
// 			Gizmo.Draw.Color = Gizmo.Colors.Green;
// 			if ( Gizmo.Control.Arrow( "up", Vector3.Up, out float delta, length: 32, girth: 32, cullAngle: 0 ) )
// 			{
// 				float newDepth = MathF.Max( 1f, depthMaximum + delta );
// 				WetnessVolumeCylinderDepthMaximum = UnitConversion.InchesToCentimeters(newDepth);
// 			}
// 		}
// 	}
// }