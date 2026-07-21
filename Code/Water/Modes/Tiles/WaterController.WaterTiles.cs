namespace AtmokineticAssets;

public sealed partial class WaterController: Component, Component.ITriggerListener
{
	
	
	[Property, Title("Tile Model"), Feature("General"), Group("Properties"), ShowIf( nameof(WaterMode), WaterModeEnumeration.Tiles )]
	public Model WaterTileModel { get; set; } = Model.Load("models/mdl-water_tile.vmdl");
	
	[Property, Title("Tile Size (cm)"), Feature("General"), Group("Properties"), ShowIf( nameof(WaterMode), WaterModeEnumeration.Tiles )]
	public float WaterTileSize
	{
		get;
		set
		{
			field = ResizeWaterTileBounds( field, value );
		}
	} = 500f;
	
	[Property, Title("Bounds Maximum (cm)"), Feature("General"), Group("Properties"), ShowIf( nameof(WaterMode), WaterModeEnumeration.Tiles )]
	public Vector3 WaterTileBoundsMaximum { get; set; } = new( 500, 500, 500 );

	[Property, Title("Bounds Minimum (cm)"), Feature("General"), Group("Properties"), ShowIf( nameof(WaterMode), WaterModeEnumeration.Tiles )]
	public Vector3 WaterTileBoundsMinimum { get; set; } = new( -500, -500, -500 );

	[Button("Generate Water Tiles"), Feature("General"), Group("Properties"), ShowIf( nameof(WaterMode), WaterModeEnumeration.Tiles )]
	public void GenerateWaterTiles()
	{
		Bake();
		WaterSurfaceAttributes();
	}

	[Property, Title("Enable Gizmo"),Feature( "General" ), ShowIf( nameof(WaterMode), WaterModeEnumeration.Tiles)]
	public bool EnableWaterTileGizmo { get; set; } = true;
	private readonly Dictionary<(int x, int y), (GameObject go, ModelRenderer renderer)> _tiles = [];
	private Vector2 _lastSnappedPosition = new( float.MinValue, float.MinValue );
	private BoxCollider _waterCollider;
	private bool _waterLineActive;
	private readonly Dictionary<(int x, int y), GameObject> _waterLineChildren = [];

	// True once the component has started — gates editor-only bounds rescaling so it
	// never fires during scene deserialization (which would clobber the saved bounds).
	private bool _tilesStarted;

	public float SubmersionOffset => -(WaterRipplesDisplacementA + WaterRipplesDisplacementB);

	// World-adjusted snapped bounds (inches). Internal counterparts to the public
	// WaterTileBounds* props, which must NEVER be written by code.
	private Vector3 _waterTileBoundsMinimum;
	private Vector3 _waterTileBoundsMaximum;

	// Integer tile-grid indices, computed once in CalculateBounds (half-open range).
	private int _tileMinX, _tileMaxX, _tileMinY, _tileMaxY;

	// Cached bounds in world space (inches) — derived from the integer indices above.
	private Vector2 _boundsMin;
	private Vector2 _boundsMax;
	private float _waterZ;

	// Tile size in cm (all meshes are same size)

	public IEnumerable<ModelRenderer> GetTileRenderers()
	{
		foreach ( var kvp in _tiles )
		{
			if ( kvp.Value.renderer.IsValid() )
				yield return kvp.Value.renderer;
		}
	}

	public void Bake()
	{
		CalculateRingLayout();
		DestroyAllTiles();
		CalculateBounds();
		CreateWaterCollider();
		BakeTiles();
	}

	public float GetWaterSurfaceZ() => _waterZ;
	public float GetWaterSurfaceZWithOffset() => _waterZ + SubmersionOffset;

	public BBox GetWorldBounds()
	{
		float minZ = WorldPosition.z + UnitConversion.CentimetersToInches( WaterTileBoundsMinimum.z );
		return new BBox(
			new Vector3( _boundsMin.x, _boundsMin.y, minZ ),
			new Vector3( _boundsMax.x, _boundsMax.y, _waterZ ) );
	}

	// True when the whole water bounds are outside the camera frustum, i.e. every tile is
	// culled. One frustum-vs-BBox test per frame, independent of tile count.
	// (SceneObject.RenderingEnabled can't be used — it's a toggle you set, not a per-frame
	// cull result, and the engine doesn't expose "was culled last frame" publicly. Frustum
	// is the only reliable option; it misses occlusion, which isn't cheaply queryable.)
	public bool AreAllTilesCulled()
	{
		var camera = Scene.Camera;
		if ( !camera.IsValid() )
			return true;

		var frustum = camera.GetFrustum( new Rect( 0, 0, Screen.Width, Screen.Height ) );
		return !frustum.IsInside( GetWorldBounds(), partially: true );
	}

	public bool IsPointInWaterVolume( Vector3 point )
	{
		var b = GetWorldBounds();
		return point.x >= b.Mins.x && point.x <= b.Maxs.x &&
			   point.y >= b.Mins.y && point.y <= b.Maxs.y &&
			   point.z < GetWaterSurfaceZ() + ripples.Count && point.z >= b.Mins.z;
	}

	public bool IsPointFullySubmerged( Vector3 point )
	{
		var b = GetWorldBounds();
		return point.x >= b.Mins.x && point.x <= b.Maxs.x &&
			   point.y >= b.Mins.y && point.y <= b.Maxs.y &&
			   point.z < GetWaterSurfaceZWithOffset() && point.z >= b.Mins.z;
	}

	// Body group layout convention on the "subdivision" body group:
	//   0        : water-line variant of ring 0 (rendered only by the water-line camera)
	//   1        : ring 0 (center, 2x2 quadrants)
	//   2..n-2   : an end/corner pair per ring (subdivided_N_end, subdivided_N_corner)
	//   n-1      : the last subdivision (far filler, no variants)
	// MaxRings and the far index are derived from the model in CalculateRingLayout.
	private int MaxRings { get; set; } = 4;
	private int FarBodyGroupIndex { get; set; } = 8;

	// Derive the ring count from the model's "subdivision" body group, so adding or
	// removing ring meshes in the model needs no code change.
	private void CalculateRingLayout()
	{
		var subdivisionPart = WaterTileModel?.Parts.All
			.FirstOrDefault( p => p.Name == "subdivision" );

		// Minimum viable layout: waterline + ring 0 + one end/corner pair + far.
		if ( subdivisionPart is null || subdivisionPart.Choices.Count < 5 )
			return;

		int choiceCount = subdivisionPart.Choices.Count;

		if ( ( choiceCount - 3 ) % 2 != 0 )
			Log.Warning( $"Water tile model '{WaterTileModel.ResourceName}' has {choiceCount} subdivision choices - expected waterline + ring0 + end/corner pairs + far. Ring layout may be wrong." );

		FarBodyGroupIndex = choiceCount - 1;
		MaxRings = ( choiceCount - 3 ) / 2 + 1;
	}

	private enum TileType
	{
		End,      // Outline side
		Corner    // Outline corner
	}

	private int GetBodyGroupForRing( int ringLevel, TileType tileType )
	{
		if ( ringLevel == 0 )
			return 1;
		if ( ringLevel >= MaxRings )
			return FarBodyGroupIndex;

		int baseIndex = ( ringLevel - 1 ) * 2 + 2;
		return tileType == TileType.Corner ? baseIndex + 1 : baseIndex;
	}

	private void UpdateRings()
	{
		if ( _tiles.Count == 0 )
			return;

		var camera = Scene.Camera;
		if ( camera == null )
			return;

		// Half-tile bias shifts ring transitions to tile centers instead of tile edges.
		// Snap the camera in box-local space (relative to the GameObject origin) to match the tile layout.
		float tileSizeInches = UnitConversion.CentimetersToInches( WaterTileSize );
		float halfTile = tileSizeInches * 0.5f;
		float localCamX = camera.WorldPosition.x - WorldPosition.x;
		float localCamY = camera.WorldPosition.y - WorldPosition.y;
		float snappedX = MathF.Floor( ( localCamX - halfTile ) / tileSizeInches ) * tileSizeInches;
		float snappedY = MathF.Floor( ( localCamY - halfTile ) / tileSizeInches ) * tileSizeInches;
		Vector2 snappedPos = new( snappedX, snappedY );

		if ( snappedPos != _lastSnappedPosition )
		{
			_lastSnappedPosition = snappedPos;
			UpdateTileBodyGroups( snappedPos );
			SyncWaterLineChildren();
		}
	}

	private void DestroyAllTiles()
	{
		foreach ( var kvp in _tiles )
		{
			if ( kvp.Value.go.IsValid() )
				kvp.Value.go.Destroy();
		}
		_tiles.Clear();
		MarkRippleRenderersDirty();
	}

	public void CreateWaterLineMesh()
	{
		if ( _waterLineActive ) return;
		_waterLineActive = true;
		// If tiles are already live, sync immediately; otherwise OnUpdate will handle it on next snap.
		if ( _tiles.Count > 0 )
			SyncWaterLineChildren();
	}

	public void DestroyWaterLineMesh()
	{
		_waterLineActive = false;
		foreach ( var kvp in _waterLineChildren )
		{
			if ( kvp.Value.IsValid() )
				kvp.Value.Destroy();
		}
		_waterLineChildren.Clear();
		MarkRippleRenderersDirty();
	}

	private void SyncWaterLineChildren()
	{
		if ( !_waterLineActive || WaterTileModel == null )
			return;

		var ringZeroKeys = new HashSet<(int x, int y)>();
		foreach ( var kvp in _tiles )
		{
			if ( kvp.Value.renderer.IsValid() && kvp.Value.renderer.GetBodyGroup( "subdivision" ) == 1 )
				ringZeroKeys.Add( kvp.Key );
		}

		var stale = new List<(int x, int y)>();
		foreach ( var kvp in _waterLineChildren )
		{
			if ( ringZeroKeys.Contains( kvp.Key ) )
				continue;
			if ( kvp.Value.IsValid() )
				kvp.Value.Destroy();
			stale.Add( kvp.Key );
		}
		foreach ( var k in stale )
			_waterLineChildren.Remove( k );

		if ( stale.Count > 0 )
			MarkRippleRenderersDirty();

		foreach ( var key in ringZeroKeys )
		{
			if ( _waterLineChildren.ContainsKey( key ) )
				continue;
			if ( !_tiles.TryGetValue( key, out var tile ) || !tile.go.IsValid() )
				continue;

			var childGo = new GameObject( true, "waterline" ) { Parent = tile.go };
			// Local-only render mesh - never include it in network snapshots.
			childGo.Flags |= GameObjectFlags.NotNetworked;
			childGo.NetworkMode = NetworkMode.Never;
			childGo.Tags.Add( "water_line_mesh" );

			var r = childGo.Components.Create<ModelRenderer>();
			r.Model = WaterTileModel;

			r.RenderType = ModelRenderer.ShadowRenderType.Off;
			r.SceneObject.Attributes.Set( "BoundsMin", _boundsMin );
			r.SceneObject.Attributes.Set( "BoundsMax", _boundsMax );
			
			r.SceneObject.Attributes.Set( "WaterRipplesTextureA", WaterRipplesTextureA );
			r.SceneObject.Attributes.Set( "Water Ripples Scale A", WaterRipplesScaleA );
			r.SceneObject.Attributes.Set( "Water Ripples Rotation A", WaterRipplesRotationA );
			r.SceneObject.Attributes.Set( "Water Ripples Speed A", WaterRipplesSpeedA );
			r.SceneObject.Attributes.Set( "Water Ripples Displacement A", WaterRipplesDisplacementA );

			r.SceneObject.Attributes.Set( "WaterRipplesTextureB", WaterRipplesTextureB );
			r.SceneObject.Attributes.Set( "Water Ripples Scale B", WaterRipplesScaleB );
			r.SceneObject.Attributes.Set( "Water Ripples Rotation B", WaterRipplesRotationB );
			r.SceneObject.Attributes.Set( "Water Ripples Speed B", WaterRipplesSpeedB );
			r.SceneObject.Attributes.Set( "Water Ripples Displacement B", WaterRipplesDisplacementB );


			r.Attributes.SetCombo( "D_TRANSLUCENT", 0 );
			r.Attributes.SetCombo( "D_Enable_Water_Line", 1 );
			r.SetBodyGroup( "subdivision", 0 );

			_waterLineChildren[key] = childGo;
			MarkRippleRenderersDirty();
		}
	}

	public void OnTriggerEnter( Collider other )
	{
		if ( !other.GameObject.Tags.Has( "particles" ) )
			return;

		other.GameObject.Destroy();
	}

	public void OnTriggerExit( Collider other )
	{
	}

	private void CreateWaterCollider()
	{
		// Only create the trigger collider at runtime, not while editing the scene.
		if ( Scene.IsEditor )
			return;

		// Use the snapped tile region so the trigger matches the spawned tiles.
		// _waterTileBounds* are world-space; convert to local for the (parented) collider.
		Vector3 localMin = _waterTileBoundsMinimum - WorldPosition;
		Vector3 localMax = _waterTileBoundsMaximum - WorldPosition;

		Vector3 center = ( localMin + localMax ) * 0.5f;
		Vector3 size = localMax - localMin;

		_waterCollider ??= Components.Get<BoxCollider>( FindMode.InSelf ) ?? Components.Create<BoxCollider>();
		_waterCollider.Flags = ComponentFlags.Hidden;
		_waterCollider.Center = center;
		_waterCollider.Scale = size;
		_waterCollider.IsTrigger = true;
		_waterCollider.Static = true;
	}

	private void CalculateBounds()
	{
		var minInches = UnitConversion.CentimetersToInches( WaterTileBoundsMinimum );
		var maxInches = UnitConversion.CentimetersToInches( WaterTileBoundsMaximum );

		float tileSizeInches = UnitConversion.CentimetersToInches( WaterTileSize );

		// Snap the extent to the tile grid in LOCAL space (relative to the GameObject origin).
		// Tiles are anchored to the box, so they fill it exactly and follow the GameObject smoothly.
		_tileMinX = (int)MathF.Floor( MathF.Round( minInches.x / tileSizeInches, 4 ) );
		_tileMinY = (int)MathF.Floor( MathF.Round( minInches.y / tileSizeInches, 4 ) );
		_tileMaxX = (int)MathF.Ceiling( MathF.Round( maxInches.x / tileSizeInches, 4 ) );
		_tileMaxY = (int)MathF.Ceiling( MathF.Round( maxInches.y / tileSizeInches, 4 ) );

		// Local-space snapped corners.
		float minLocalX = _tileMinX * tileSizeInches;
		float minLocalY = _tileMinY * tileSizeInches;
		float maxLocalX = _tileMaxX * tileSizeInches;
		float maxLocalY = _tileMaxY * tileSizeInches;

		// World-space bounds (for collider-independent consumers, shader clipping, point queries).
		_waterTileBoundsMinimum = new Vector3( WorldPosition.x + minLocalX, WorldPosition.y + minLocalY, WorldPosition.z + minInches.z );
		_waterTileBoundsMaximum = new Vector3( WorldPosition.x + maxLocalX, WorldPosition.y + maxLocalY, WorldPosition.z + maxInches.z );

		_boundsMin = new Vector2( _waterTileBoundsMinimum.x, _waterTileBoundsMinimum.y );
		_boundsMax = new Vector2( _waterTileBoundsMaximum.x, _waterTileBoundsMaximum.y );
		_waterZ = _waterTileBoundsMaximum.z;
	}

	private void DiscoverBakedTiles()
	{
		CalculateRingLayout();

		// Rediscovered renderers need the surface attributes even if no property changed.
		_surfaceAttributesDirty = true;
		MarkRippleRenderersDirty();


		foreach ( var child in GameObject.Children.ToList() )
		{
			if ( !child.Name.StartsWith( "tile_" ) )
				continue;

			var renderer = child.Components.Get<ModelRenderer>();
			if ( renderer == null )
				continue;

			renderer.SceneObject.Attributes.Set( "BoundsMin", _boundsMin );
			renderer.SceneObject.Attributes.Set( "BoundsMax", _boundsMax );
			renderer.Attributes.SetCombo( "D_TRANSLUCENT", 1 );
			renderer.Attributes.SetCombo( "D_Enable_Water_Line", 0 );

			// Parse tileX, tileY from name "tile_X_Y"
			var parts = child.Name.Split( '_' );
			if ( parts.Length == 3 && int.TryParse( parts[1], out int tileX ) && int.TryParse( parts[2], out int tileY ) )
			{
				_tiles[(tileX, tileY)] = (child, renderer);
			}
		}
	}

	private void BakeTiles()
	{
		if ( WaterTileModel == null )
			return;

		float tileSizeInches = UnitConversion.CentimetersToInches( WaterTileSize );
		float halfTile = tileSizeInches * 0.5f;

		for ( int tileX = _tileMinX; tileX < _tileMaxX; tileX++ )
		{
			for ( int tileY = _tileMinY; tileY < _tileMaxY; tileY++ )
			{
				// Tile indices are local-grid; anchor positions to the GameObject origin.
				float worldX = WorldPosition.x + tileX * tileSizeInches;
				float worldY = WorldPosition.y + tileY * tileSizeInches;

				Vector3 worldPosition = new Vector3( worldX + halfTile, worldY + halfTile, _waterZ );
				SpawnTile( worldPosition, tileX, tileY );
			}
		}
	}

	private void UpdateTileBodyGroups( Vector2 snappedPos )
	{
		float tileSizeInches = UnitConversion.CentimetersToInches( WaterTileSize );

		// First pass: calculate ring levels and find outermost ring
		var tileData = new Dictionary<(int x, int y), (int ringLevel, int gridX, int gridY)>();
		int outermostRenderedRing = 0;

		foreach ( var kvp in _tiles )
		{
			int tileX = kvp.Key.x;
			int tileY = kvp.Key.y;

			// Box-local tile position (indices are local-grid; snappedPos is box-local).
			float localX = tileX * tileSizeInches;
			float localY = tileY * tileSizeInches;

			int gridX = (int)MathF.Round( ( localX - snappedPos.x ) / tileSizeInches );
			int gridY = (int)MathF.Round( ( localY - snappedPos.y ) / tileSizeInches );

			int ringLevel = Math.Max( GetRingForCoord( gridX ), GetRingForCoord( gridY ) );
			if ( ringLevel > MaxRings )
				ringLevel = MaxRings;

			if ( ringLevel > outermostRenderedRing )
				outermostRenderedRing = ringLevel;

			tileData[(tileX, tileY)] = (ringLevel, gridX, gridY);
		}

		// Second pass: update body groups
		foreach ( var kvp in _tiles )
		{
			var renderer = kvp.Value.renderer;
			if ( !renderer.IsValid() )
				continue;

			var (ringLevel, gridX, gridY) = tileData[kvp.Key];

			TileType tileType = TileType.End;
			float rotation = 0f;

			if ( ringLevel == 0 )
			{
				if ( gridX == 0 && gridY == 0 ) rotation = 0f;
				else if ( gridX == 1 && gridY == 0 ) rotation = 90f;
				else if ( gridX == 0 && gridY == 1 ) rotation = 270f;
				else if ( gridX == 1 && gridY == 1 ) rotation = 180f;
			}
			else if ( ringLevel < MaxRings )
			{
				// Rings are 1-tile-thick outlines - every tile is an End or a Corner.
				if ( IsCornerTile( gridX, gridY, ringLevel ) )
				{
					tileType = TileType.Corner;
					rotation = GetCornerRotation( gridX, gridY, ringLevel );
				}
				else
				{
					rotation = GetEdgeRotation( gridX, gridY, ringLevel );
				}
			}

			renderer.SetBodyGroup( "subdivision", GetBodyGroupForRing( ringLevel, tileType ) );
			kvp.Value.go.WorldRotation = Rotation.FromYaw( -90f + rotation );
		}
	}

	private int GetRingForCoord( int gridCoord )
	{
		// Center (ring 0): coords 0, 1 (the 2x2 quadrant block)
		// Ring N (1 tile thick outline): coord -N on the negative side, N+1 on the positive side
		//   Ring 1: -1, 2
		//   Ring 2: -2, 3
		//   Ring 3: -3, 4
		if ( gridCoord >= 0 && gridCoord <= 1 )
			return 0;

		int ring = gridCoord < 0 ? -gridCoord : gridCoord - 1;

		if ( ring > MaxRings )
			return MaxRings + 1;

		return ring;
	}

	private bool IsCornerTile( int gridX, int gridY, int ringLevel )
	{
		int outerMin = -ringLevel;
		int outerMax = ringLevel + 1;

		bool atCornerX = ( gridX == outerMin || gridX == outerMax );
		bool atCornerY = ( gridY == outerMin || gridY == outerMax );

		return atCornerX && atCornerY;
	}

	private float GetCornerRotation( int gridX, int gridY, int ringLevel )
	{
		int outerMin = -ringLevel;
		int outerMax = ringLevel + 1;

		if ( gridX == outerMin && gridY == outerMax ) return 270f;
		if ( gridX == outerMax && gridY == outerMax ) return 180f;
		if ( gridX == outerMax && gridY == outerMin ) return 90f;
		if ( gridX == outerMin && gridY == outerMin ) return 0f;

		return 0f;
	}

	private float GetEdgeRotation( int gridX, int gridY, int ringLevel )
	{
		int outerMin = -ringLevel;
		int outerMax = ringLevel + 1;

		if ( gridY == outerMax ) return 180f;
		if ( gridX == outerMax ) return 90f;
		if ( gridY == outerMin ) return 0f;
		if ( gridX == outerMin ) return 270f;

		return 0f;
	}

	private void SpawnTile( Vector3 worldPosition, int tileX, int tileY )
	{
		var tileGo = new GameObject( true, $"tile_{tileX}_{tileY}" );
		tileGo.Parent = GameObject;
		tileGo.Tags.Add( "reflection" ); // excluded from the reflection camera so water doesn't reflect itself
		tileGo.WorldPosition = worldPosition;
		tileGo.LocalScale = Vector3.One * ( WaterTileSize / 100f );

		var renderer = tileGo.Components.Create<ModelRenderer>();
		renderer.Model = WaterTileModel;

		renderer.SceneObject.Attributes.Set( "BoundsMin", _boundsMin );
		renderer.SceneObject.Attributes.Set( "BoundsMax", _boundsMax );
		renderer.Attributes.SetCombo( "D_TRANSLUCENT", 1 );
		renderer.Attributes.SetCombo( "D_Enable_Water_Line", 0 );

		renderer.SetBodyGroup( "subdivision", FarBodyGroupIndex );

		_tiles[(tileX, tileY)] = (tileGo, renderer);

		// New renderer needs the surface attributes, ripple attributes and the
		// reflection-mode combo even if nothing changed.
		_surfaceAttributesDirty = true;
		MarkRippleRenderersDirty();
		_lastWaterReflectionPlanarRendering = null;
	}
}
