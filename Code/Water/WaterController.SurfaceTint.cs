namespace AtmokineticAssets;

/// <summary>
/// A fixed cap set of generic world space color influences for the water
/// surface. The library owns the rendering primitive and games can decide what
/// causes it. Stuff like blood, oil, pollution etc.
/// </summary>
public sealed partial class WaterController : Component, Component.ExecuteInEditor
{
	public const int MaximumLocalizedSurfaceTints = 8;

	[StructLayout( LayoutKind.Sequential )]
	public struct WaterSurfaceTint
	{
		public Vector3 Position;
		public float StartTime;
		public float InitialRadius;
		public float MaximumRadius;
		public float GrowthDuration;
		public float FadeInDuration;
		public float Strength;
		public float Feather;
		public float DiffusionDelay;
		public float DiffusionDuration;
		public float DiffusionExpansion;
		public float GlobalStrength;
		public float RippleAgeOffset;
	}

	[Property, Feature( "Localized Surface Tint" ), Title( "Debug Stain Count" ), Range( 0, MaximumLocalizedSurfaceTints )]
	public int LocalizedSurfaceTintDebugCount
	{
		get;
		set
		{
			field = Math.Clamp( value, 0, MaximumLocalizedSurfaceTints );
			MarkLocalizedSurfaceTintDirty();
		}
	} = 0;

	[Property, Feature( "Localized Surface Tint" ), Title( "Debug Center Offset" )]
	public Vector3 LocalizedSurfaceTintDebugCenterOffset
	{
		get;
		set
		{
			field = value;
			MarkLocalizedSurfaceTintDirty();
		}
	} = Vector3.Zero;

	[Property, Feature( "Localized Surface Tint" ), Title( "Debug Stain Spacing" ), Range( 1f, 1000f )]
	public float LocalizedSurfaceTintDebugSpacing
	{
		get;
		set
		{
			field = MathF.Max( value, 1f );
			MarkLocalizedSurfaceTintDirty();
		}
	} = 260f;

	[Property, Feature( "Localized Surface Tint" ), Title( "Color" )]
	public Color LocalizedSurfaceTintColor
	{
		get;
		set
		{
			field = value;
			MarkLocalizedSurfaceTintRenderersDirty();
		}
	} = Color.Parse( "#7d0712" ).Value;

	[Property, Feature( "Localized Surface Tint" ), Title( "Initial Radius" ), Range( 0f, 1000f )]
	public float LocalizedSurfaceTintInitialRadius
	{
		get;
		set
		{
			field = MathF.Max( value, 0f );
			MarkLocalizedSurfaceTintDirty();
		}
	} = 0f;

	[Property, Feature( "Localized Surface Tint" ), Title( "Maximum Radius" ), Range( 1f, 1000f )]
	public float LocalizedSurfaceTintMaximumRadius
	{
		get;
		set
		{
			field = MathF.Max( value, 1f );
			MarkLocalizedSurfaceTintDirty();
		}
	} = 220f;

	[Property, Feature( "Localized Surface Tint" ), Title( "Growth Duration" ), Range( 0.1f, 120f )]
	public float LocalizedSurfaceTintGrowthDuration
	{
		get;
		set
		{
			field = MathF.Max( value, 0.1f );
			MarkLocalizedSurfaceTintDirty();
		}
	} = 12f;

	[Property, Feature( "Localized Surface Tint" ), Title( "Fade In Duration" ), Range( 0.1f, 30f )]
	public float LocalizedSurfaceTintFadeInDuration
	{
		get;
		set
		{
			field = MathF.Max( value, 0.1f );
			MarkLocalizedSurfaceTintDirty();
		}
	} = 4f;

	[Property, Feature( "Localized Surface Tint" ), Title( "Strength" ), Range( 0f, 1f )]
	public float LocalizedSurfaceTintStrength
	{
		get;
		set
		{
			field = Math.Clamp( value, 0f, 1f );
			MarkLocalizedSurfaceTintDirty();
		}
	} = 0.9f;

	[Property, Feature( "Localized Surface Tint" ), Title( "Edge Feather" ), Range( 0.01f, 1f )]
	public float LocalizedSurfaceTintFeather
	{
		get;
		set
		{
			field = Math.Clamp( value, 0.01f, 1f );
			MarkLocalizedSurfaceTintDirty();
		}
	} = 0.55f;

	[Property, Feature( "Localized Surface Tint" ), Group( "Diffusion" ), Title( "Delay" ), Range( 0f, 300f )]
	public float LocalizedSurfaceTintDiffusionDelay
	{
		get;
		set
		{
			field = MathF.Max( value, 0f );
			MarkLocalizedSurfaceTintDirty();
		}
	} = 20f;

	[Property, Feature( "Localized Surface Tint" ), Group( "Diffusion" ), Title( "Duration" ), Range( 0.1f, 300f )]
	public float LocalizedSurfaceTintDiffusionDuration
	{
		get;
		set
		{
			field = MathF.Max( value, 0.1f );
			MarkLocalizedSurfaceTintDirty();
		}
	} = 30f;

	[Property, Feature( "Localized Surface Tint" ), Group( "Diffusion" ), Title( "Radius Expansion" ), Range( 1f, 4f )]
	public float LocalizedSurfaceTintDiffusionExpansion
	{
		get;
		set
		{
			field = MathF.Max( value, 1f );
			MarkLocalizedSurfaceTintDirty();
		}
	} = 1.35f;

	[Property, Feature( "Localized Surface Tint" ), Group( "Diffusion" ), Title( "Global Contribution" ), Range( 0f, 0.25f )]
	public float LocalizedSurfaceTintGlobalStrength
	{
		get;
		set
		{
			field = Math.Clamp( value, 0f, 0.25f );
			MarkLocalizedSurfaceTintDirty();
		}
	} = 0.025f;

	[Property, Feature( "Localized Surface Tint" ), Group( "Diffusion" ), Title( "Maximum Global Strength" ), Range( 0f, 1f )]
	public float LocalizedSurfaceTintMaximumGlobalStrength
	{
		get;
		set
		{
			field = Math.Clamp( value, 0f, 1f );
			MarkLocalizedSurfaceTintRenderersDirty();
		}
	} = 0.2f;

	[Property, Feature( "Localized Surface Tint" ), Group( "Diffusion" ), Title( "Ripple Diffusion Rate" ), Range( 0f, 60f )]
	public float LocalizedSurfaceTintRippleDiffusionRate
	{
		get;
		set
		{
			field = Math.Clamp( value, 0f, 60f );
		}
	} = 20f;

	[Property, Feature( "Localized Surface Tint" ), Title( "Edge Variation" ), Range( 0f, 0.35f )]
	public float LocalizedSurfaceTintEdgeVariation
	{
		get;
		set
		{
			field = Math.Clamp( value, 0f, 0.35f );
			MarkLocalizedSurfaceTintRenderersDirty();
		}
	} = 0.12f;

	private readonly List<WaterSurfaceTint> _localizedSurfaceTints = new( MaximumLocalizedSurfaceTints );
	private GpuBuffer<WaterSurfaceTint> _localizedSurfaceTintBuffer;
	private bool _localizedSurfaceTintDataDirty = true;
	private bool _localizedSurfaceTintRenderersDirty = true;
	private float _localizedSurfaceTintStartTime;
	private float _nextLocalizedSurfaceTintRippleSampleAt;
	private const float LocalizedSurfaceTintRippleSampleInterval = 0.2f;

	private void InitializeLocalizedSurfaceTints()
	{
		_localizedSurfaceTintBuffer ??= new GpuBuffer<WaterSurfaceTint>( MaximumLocalizedSurfaceTints );
		_localizedSurfaceTintDataDirty = true;
		_localizedSurfaceTintRenderersDirty = true;
	}

	private void DisposeLocalizedSurfaceTints()
	{
		_localizedSurfaceTintBuffer?.Dispose();
		_localizedSurfaceTintBuffer = null;
	}

	private void MarkLocalizedSurfaceTintDirty()
	{
		_localizedSurfaceTintDataDirty = true;
		_localizedSurfaceTintRenderersDirty = true;
	}

	private void MarkLocalizedSurfaceTintRenderersDirty()
	{
		_localizedSurfaceTintRenderersDirty = true;
	}

	/// <summary>
	/// Replaces the fixed tint set and uploads it once. Callers own lifetime/networking for records and the water renderer just consumes a snapshot
	/// </summary>
	public void SetLocalizedSurfaceTints( IReadOnlyList<WaterSurfaceTint> tints )
	{
		InitializeLocalizedSurfaceTints();
		var previousTints = _localizedSurfaceTints.ToArray();
		_localizedSurfaceTints.Clear();

		if ( tints != null )
		{
			int count = Math.Min( tints.Count, MaximumLocalizedSurfaceTints );
			for ( int index = 0; index < count; index++ )
			{
				var tint = tints[index];
				tint.RippleAgeOffset = MathF.Max( tint.RippleAgeOffset, FindPreviousRippleAgeOffset( previousTints, tint ) );
				_localizedSurfaceTints.Add( tint );
			}
		}

		if ( _localizedSurfaceTints.Count > 0 )
			_localizedSurfaceTintBuffer.SetData( _localizedSurfaceTints );

		_localizedSurfaceTintDataDirty = false;
		MarkLocalizedSurfaceTintRenderersDirty();
	}

	private static float FindPreviousRippleAgeOffset( IReadOnlyList<WaterSurfaceTint> previousTints, WaterSurfaceTint tint )
	{
		for ( int index = 0; index < previousTints.Count; index++ )
		{
			var previous = previousTints[index];
			if ( MathF.Abs( previous.StartTime - tint.StartTime ) > 0.001f )
				continue;

			var delta = previous.Position - tint.Position;
			if ( delta.LengthSquared <= 0.01f )
				return previous.RippleAgeOffset;
		}

		return 0f;
	}

	public bool IsLocalizedSurfaceTintFullyDiffused( Vector3 position, float startTime )
	{
		for ( int index = 0; index < _localizedSurfaceTints.Count; index++ )
		{
			var tint = _localizedSurfaceTints[index];
			if ( MathF.Abs( tint.StartTime - startTime ) > 0.001f )
				continue;

			var delta = tint.Position - position;
			if ( delta.LengthSquared > 0.01f )
				continue;

			float diffusionAge = MathF.Max( Time.Now - tint.StartTime, 0f ) + tint.RippleAgeOffset;
			float fullyDiffusedAt = MathF.Max( tint.DiffusionDelay, 0f )
				+ MathF.Max( tint.DiffusionDuration, 0.1f );
			return diffusionAge >= fullyDiffusedAt;
		}

		return false;
	}

	public void ClearLocalizedSurfaceTints()
	{
		SetLocalizedSurfaceTints( null );
	}

	private void RebuildLocalizedSurfaceTintData()
	{
		_localizedSurfaceTints.Clear();
		_localizedSurfaceTintStartTime = Time.Now;

		int count = Math.Clamp( LocalizedSurfaceTintDebugCount, 0, MaximumLocalizedSurfaceTints );
		if ( count > 0 )
		{
			int columns = count <= 1 ? 1 : count <= 4 ? 2 : 4;
			int rows = (count + columns - 1) / columns;
			Vector3 center = WorldPosition + LocalizedSurfaceTintDebugCenterOffset;

			for ( int index = 0; index < count; index++ )
			{
				int row = index / columns;
				int column = index % columns;
				int entriesInRow = Math.Min( columns, count - row * columns );
				float offsetX = (column - (entriesInRow - 1) * 0.5f) * LocalizedSurfaceTintDebugSpacing;
				float offsetY = (row - (rows - 1) * 0.5f) * LocalizedSurfaceTintDebugSpacing;

				_localizedSurfaceTints.Add( new WaterSurfaceTint
				{
					Position = center + new Vector3( offsetX, offsetY, 0f ),
					StartTime = _localizedSurfaceTintStartTime,
					InitialRadius = LocalizedSurfaceTintInitialRadius,
					MaximumRadius = LocalizedSurfaceTintMaximumRadius,
					GrowthDuration = LocalizedSurfaceTintGrowthDuration,
					FadeInDuration = LocalizedSurfaceTintFadeInDuration,
					Strength = LocalizedSurfaceTintStrength,
					Feather = LocalizedSurfaceTintFeather,
					DiffusionDelay = LocalizedSurfaceTintDiffusionDelay,
					DiffusionDuration = LocalizedSurfaceTintDiffusionDuration,
					DiffusionExpansion = LocalizedSurfaceTintDiffusionExpansion,
					GlobalStrength = LocalizedSurfaceTintGlobalStrength,
					RippleAgeOffset = 0f
				} );
			}

			_localizedSurfaceTintBuffer.SetData( _localizedSurfaceTints );
		}

		_localizedSurfaceTintDataDirty = false;
	}

	private void AccumulateLocalizedSurfaceTintRippleExposure()
	{
		if ( _localizedSurfaceTints.Count == 0 || ripples.Count == 0 || LocalizedSurfaceTintRippleDiffusionRate <= 0f )
			return;

		bool changed = false;
		float now = Time.Now;
		for ( int tintIndex = 0; tintIndex < _localizedSurfaceTints.Count; tintIndex++ )
		{
			var tint = _localizedSurfaceTints[tintIndex];
			float ordinaryAge = MathF.Max( now - tint.StartTime, 0f );
			float growth = Math.Clamp( ordinaryAge / MathF.Max( tint.GrowthDuration, 0.001f ), 0f, 1f );
			growth = growth * growth * (3f - 2f * growth);
			float diffusion = Math.Clamp(
				(ordinaryAge + tint.RippleAgeOffset - MathF.Max( tint.DiffusionDelay, 0f ))
				/ MathF.Max( tint.DiffusionDuration, 0.001f ),
				0f,
				1f );
			float radius = MathX.Lerp( tint.InitialRadius, tint.MaximumRadius, growth )
				* MathX.Lerp( 1f, MathF.Max( tint.DiffusionExpansion, 1f ), diffusion );
			float strongestOverlap = 0f;

			for ( int rippleIndex = 0; rippleIndex < ripples.Count; rippleIndex++ )
			{
				var ripple = ripples[rippleIndex];
				if ( ripple.Alpha <= 0.001f || ripple.Radius <= 0.01f )
					continue;

				float overlapRadius = radius + ripple.Radius;
				var delta = tint.Position - ripple.Position;
				float distanceSquared = delta.x * delta.x + delta.y * delta.y;
				if ( distanceSquared <= overlapRadius * overlapRadius )
					strongestOverlap = MathF.Max( strongestOverlap, ripple.Alpha );
			}

			if ( strongestOverlap <= 0f )
				continue;

			tint.RippleAgeOffset += LocalizedSurfaceTintRippleSampleInterval
				* LocalizedSurfaceTintRippleDiffusionRate
				* strongestOverlap;
			_localizedSurfaceTints[tintIndex] = tint;
			changed = true;
		}

		if ( changed )
			_localizedSurfaceTintBuffer.SetData( _localizedSurfaceTints );
	}

	private void UpdateLocalizedSurfaceTints()
	{
		if ( _localizedSurfaceTintBuffer == null )
			InitializeLocalizedSurfaceTints();

		if ( _localizedSurfaceTintDataDirty )
			RebuildLocalizedSurfaceTintData();

		if ( Time.Now >= _nextLocalizedSurfaceTintRippleSampleAt )
		{
			_nextLocalizedSurfaceTintRippleSampleAt = Time.Now + LocalizedSurfaceTintRippleSampleInterval;
			AccumulateLocalizedSurfaceTintRippleExposure();
		}

		if ( !_localizedSurfaceTintRenderersDirty )
			return;

		if ( _rippleRenderersDirty )
			RebuildRippleRendererCache();

		_localizedSurfaceTintRenderersDirty = false;

		foreach ( var renderer in _rippleRenderers )
		{
			if ( !renderer.IsValid() || !renderer.SceneObject.IsValid() )
				continue;

			renderer.SceneObject.Attributes.Set( "WaterSurfaceTintBuffer", _localizedSurfaceTintBuffer );
			renderer.SceneObject.Attributes.Set( "WaterSurfaceTintCount", _localizedSurfaceTints.Count );
			renderer.SceneObject.Attributes.Set( "Water Surface Tint Color", LocalizedSurfaceTintColor );
			renderer.SceneObject.Attributes.Set( "Water Surface Tint Edge Variation", LocalizedSurfaceTintEdgeVariation );
			renderer.SceneObject.Attributes.Set( "Water Surface Tint Maximum Global Strength", LocalizedSurfaceTintMaximumGlobalStrength );
		}

		if ( TargetRenderer?.SceneObject.IsValid() == true )
		{
			TargetRenderer.SceneObject.Attributes.Set( "WaterSurfaceTintBuffer", _localizedSurfaceTintBuffer );
			TargetRenderer.SceneObject.Attributes.Set( "WaterSurfaceTintCount", _localizedSurfaceTints.Count );
			TargetRenderer.SceneObject.Attributes.Set( "Water Surface Tint Color", LocalizedSurfaceTintColor );
			TargetRenderer.SceneObject.Attributes.Set( "Water Surface Tint Edge Variation", LocalizedSurfaceTintEdgeVariation );
			TargetRenderer.SceneObject.Attributes.Set( "Water Surface Tint Maximum Global Strength", LocalizedSurfaceTintMaximumGlobalStrength );
		}
	}
}
