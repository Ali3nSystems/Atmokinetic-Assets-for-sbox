namespace AtmokineticAssets;

public sealed partial class WaterController : Component
{
	private void DrawWaterTileGizmo()
	{
		if ( !EnableWaterTileGizmo )
			return;

		if ( !Gizmo.IsSelected && !Gizmo.IsHovered )
			return;

		// Raw box from the user's typed bounds — used for the editing handles.
		var rawBox = new BBox(
			UnitConversion.CentimetersToInches( WaterTileBoundsMinimum ),
			UnitConversion.CentimetersToInches( WaterTileBoundsMaximum ) );

		// Snapped box — the actual world-grid region the tiles fill — used for drawing.
		var snappedBox = GetSnappedLocalBox( rawBox );

		DrawWaterTileBounds( snappedBox );
		DrawWaterTileHandles( rawBox );
	}

	// Snaps the bounds extent to the tile grid relative to the GameObject's own origin,
	// so the box keeps its tile-sized footprint but follows the GameObject smoothly (no world-grid jumping).
	private BBox GetSnappedLocalBox( BBox localBox )
	{
		float tileSizeInches = UnitConversion.CentimetersToInches( WaterTileSize );
		if ( tileSizeInches <= 0 )
			return localBox;

		float minX = MathF.Floor( MathF.Round( localBox.Mins.x / tileSizeInches, 4 ) ) * tileSizeInches;
		float minY = MathF.Floor( MathF.Round( localBox.Mins.y / tileSizeInches, 4 ) ) * tileSizeInches;
		float maxX = MathF.Ceiling( MathF.Round( localBox.Maxs.x / tileSizeInches, 4 ) ) * tileSizeInches;
		float maxY = MathF.Ceiling( MathF.Round( localBox.Maxs.y / tileSizeInches, 4 ) ) * tileSizeInches;

		return new BBox(
			new Vector3( minX, minY, localBox.Mins.z ),
			new Vector3( maxX, maxY, localBox.Maxs.z ) );
	}

	private void DrawWaterTileBounds( BBox box )
	{
		Gizmo.Draw.LineThickness = 1;
		var tint = Gizmo.IsSelected ? Gizmo.Colors.Blue : Gizmo.Colors.Blue.WithAlpha( 0.4f );

		for ( int i = 0; i < 2; i++ )
		{
			bool xray = i == 0;
			Gizmo.Draw.IgnoreDepth = xray;
			float alpha = xray ? 0.2f : 1f;

			Gizmo.Draw.Color = tint.WithAlpha( tint.a * alpha );
			Gizmo.Draw.LineBBox( box );

			Gizmo.Draw.Color = tint.WithAlpha( 0.05f * alpha );
			Gizmo.Draw.SolidBox( box );
		}
	}
	
	private void DrawWaterTileHandles( BBox box )
	{
		Gizmo.Hitbox.BBox( box );

		// Disable the editor's grid snap for this control so it doesn't double up with our tile snap.
		var previousSnap = Gizmo.Settings.SnapToGrid;
		Gizmo.Settings.SnapToGrid = false;

		if ( Gizmo.Control.BoundingBox( "water_tile_bounds", box, out var newBox ) )
		{
			var minCm = UnitConversion.InchesToCentimeters( newBox.Mins );
			var maxCm = UnitConversion.InchesToCentimeters( newBox.Maxs );

			WaterTileBoundsMinimum = SnapChangedAxes( WaterTileBoundsMinimum, minCm, WaterTileSize, floor: true );
			WaterTileBoundsMaximum = SnapChangedAxes( WaterTileBoundsMaximum, maxCm, WaterTileSize, floor: false );
		}

		Gizmo.Settings.SnapToGrid = previousSnap;
	}

	private float ResizeWaterTileBounds( float oldSize, float newSize )
	{
		// Skip during deserialization / before start — only rescale on genuine edits.
		if ( !_tilesStarted || oldSize == newSize || newSize <= 0 || oldSize <= 0 )
			return newSize;

		float scale = newSize / oldSize;

		// Scale X/Y of the bounds so the box stays the same tile count.
		WaterTileBoundsMinimum = new Vector3( WaterTileBoundsMinimum.x * scale, WaterTileBoundsMinimum.y * scale, WaterTileBoundsMinimum.z );
		WaterTileBoundsMaximum = new Vector3( WaterTileBoundsMaximum.x * scale, WaterTileBoundsMaximum.y * scale, WaterTileBoundsMaximum.z );

		return newSize;
	}

	private static Vector3 SnapChangedAxes( Vector3 oldValue, Vector3 newValue, float tileSize, bool floor )
	{
		if ( tileSize <= 0 )
			return newValue;

		float SnapAxis( float n ) =>
			( floor ? MathF.Floor( n / tileSize ) : MathF.Ceiling( n / tileSize ) ) * tileSize;

		return new Vector3(
			oldValue.x.AlmostEqual( newValue.x ) ? oldValue.x : SnapAxis( newValue.x ),
			oldValue.y.AlmostEqual( newValue.y ) ? oldValue.y : SnapAxis( newValue.y ),
			oldValue.z.AlmostEqual( newValue.z ) ? oldValue.z : newValue.z );
	}
}
