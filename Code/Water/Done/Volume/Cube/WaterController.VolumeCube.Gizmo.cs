// namespace AtmokineticAssets;

// public sealed partial class WaterController : Component
// {
// 	private void DrawWetnessVolumeCubeGizmos()
// 	{
// 		if ( !EnableWetnessVolumeCubeGizmos )
// 			return;

// 		if ( !Gizmo.IsSelected && !Gizmo.IsHovered )
// 			return;

// 		var box = new BBox(
// 			UnitConversion.CentimetersToInches( WetnessVolumeCubeBoundsMinimum ),
// 			UnitConversion.CentimetersToInches( WetnessVolumeCubeBoundsMaximum ) );

// 		DrawWetnessVolumeCubeBounds( box );
// 		DrawWetnessVolumeCubeHandles( box );

// 		UpdateWetnessVolumeCubeBuffer();
// 	}

// 	private void DrawWetnessVolumeCubeBounds( BBox box )
// 	{
// 		Gizmo.Draw.LineThickness = 1;
// 		var tint = Gizmo.IsSelected ? Gizmo.Colors.Blue : Gizmo.Colors.Blue.WithAlpha( 0.4f );

// 		for ( int i = 0; i < 2; i++ )
// 		{
// 			bool xray = i == 0;
// 			Gizmo.Draw.IgnoreDepth = xray;
// 			float alpha = xray ? 0.2f : 1f;

// 			Gizmo.Draw.Color = tint.WithAlpha( tint.a * alpha );
// 			Gizmo.Draw.LineBBox( box );

// 			Gizmo.Draw.Color = tint.WithAlpha( 0.05f * alpha );
// 			Gizmo.Draw.SolidBox( box );
// 		}
// 	}

// 	private void DrawWetnessVolumeCubeHandles( BBox box )
// 	{
// 		Gizmo.Hitbox.BBox( box );

// 		if ( Gizmo.Control.BoundingBox( "cube_volume_bounds", box, out var newBox ) )
// 		{
// 			WetnessVolumeCubeBoundsMinimum = UnitConversion.InchesToCentimeters( newBox.Mins );
// 			WetnessVolumeCubeBoundsMaximum = UnitConversion.InchesToCentimeters( newBox.Maxs );
// 		}
// 	}
// }
