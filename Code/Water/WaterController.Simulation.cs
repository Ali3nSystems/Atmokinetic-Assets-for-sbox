namespace AtmokineticAssets;

public sealed partial class WaterController : Component, Component.ITriggerListener
{
	[Property, Title("Texture"), Feature("Simulation")]
	[Sync]
	public Texture WaterSimulationTexture { get; set; } = Texture.Load("textures/tex-water_simulation.vtex");

	[Property, Feature("Simulation"), Title("Refraction")]
	[Sync]
	public float WaterSimulationRefraction { get; set; } = 0.05f;

	[Property, Feature("Simulation"), Title("Displacement")]
	[Sync]
	public float WaterSimulationDisplacement { get; set;}  = 0.5f;

	[Property, Feature("Simulation"), Group("Scale"), Title("Scale Multiplier")]
	[Sync]
	public float WaterSimulationScaleMultiplier { get; set; } = 16f;

	[Property, Feature("Simulation"), Group("Scale"), Title("Velocity Multiplier")]
	[Sync]
	public float WaterSimulationVelocityMultiplier { get; set; } = 0.05f;

	[Property, Feature("Simulation"), Group("Spawn"), Title("Minimum Velocity (km/h)")]
	[Sync]
	public float WaterSimulationMinimumVelocity { get; set; } = 2f;

	[Property, Feature("Simulation"), Group("Spawn"), Title("Spawn Interval (s)")]
	[Sync]
	public float WaterSimulationSpawnInterval { get; set; } = 0.1f;

	/// <summary>
	/// For colliders we want to ignore & not ripple from
	/// </summary>
	[Property, Feature("Simulation"), Group("Spawn"), Title("Ignore Tags")]
	[Sync]
	public TagSet WaterSimulationIgnoreTags { get; set; } = [];

	/// <summary>
	/// Hard ceiling for active simulated ripples. Since the shader loops over every active
	/// ripple per vertex & pixel, this keeps rendering cost bounded if needed
	/// </summary>
	[Property, Feature("Simulation"), Group("Spawn"), Title("Maximum Active Ripples"), Range(1, 128)]
	[Sync]
	public int WaterSimulationMaximumActiveRipples { get; set; } = 32;

	[Property, Feature("Simulation"), Group("Duration"), Title("Fade In Time (s)")]
	[Sync]
	public float WaterSimulationFadeInTime { get; set; } = 0.25f;

	[Property, Feature("Simulation"), Group("Duration"), Title("Constant Time (s)")]
	[Sync]
	public float WaterSimulationConstantTime { get; set; } = 0.75f;

	[Property,Feature("Simulation"),  Group("Duration"), Title("Fade Out Time (s)")]
	[Sync]
	public float WaterSimulationFadeOutTime { get; set; } = 1.0f;

	public ModelRenderer TargetRenderer { get; set; }
	private const float INCHES_PER_SEC_TO_KMH = 0.09144f;

	[StructLayout(LayoutKind.Sequential)]
	public struct WaterSimulation
	{
		public Vector3 Position;
		public float Radius;
		public float Alpha;
		public float Rotation;
		public float Progress;
	}

	public struct RippleData
	{
		public float SpawnTime;
		public float InitialRadius;
		public float Rotation;
		public float VelocityScale;
	}
	
	
	

	public List<WaterSimulation> ripples = new();
	private List<RippleData> rippleData = new();
	private GpuBuffer<WaterSimulation> rippleBuffer;
	private int currentBufferSize = 0;
	private readonly HashSet<Collider> trackedColliders = new();
	private readonly Dictionary<Rigidbody, TimeSince> lastRippleTime = new();
	private readonly Dictionary<ModelPhysics, TimeSince> lastModelRippleTime = new();

	// Reused per frame instead of allocating a new list every update
	private readonly List<Collider> _collidersToRemove = new();
	private readonly HashSet<ModelPhysics> _processedModelPhysics = new();

	// List of renderers the ripple attributes are stamped onto (tiles + waterline children).
	// Cached so the per frame path doesn't walk the descendant hierarchy, rebuilt when the
	// tile or waterline set changes
	private readonly List<ModelRenderer> _rippleRenderers = new();
	private bool _rippleRenderersDirty = true;
	private int _lastStampedRippleCount = -1;

	private void MarkRippleRenderersDirty()
	{
		_rippleRenderersDirty = true;
		MarkLocalizedSurfaceTintRenderersDirty();
	}

	private void RebuildRippleRendererCache()
	{
		_rippleRenderers.Clear();

		foreach ( var renderer in GetTileRenderers() )
			_rippleRenderers.Add( renderer );

		foreach ( var kvp in _waterLineChildren )
		{
			if ( !kvp.Value.IsValid() )
				continue;

			var renderer = kvp.Value.GetComponent<ModelRenderer>();
			if ( renderer.IsValid() )
				_rippleRenderers.Add( renderer );
		}

		_rippleRenderersDirty = false;
	}

	void ITriggerListener.OnTriggerEnter(Collider other)
	{
		trackedColliders.Add(other);
	}

	void ITriggerListener.OnTriggerExit(Collider other)
	{
		trackedColliders.Remove(other);
	}

	private void UpdateWaterSimulation()
	{
		// Need a TargetRenderer or the main GameObject as the tile creator
		bool hasTarget = TargetRenderer?.SceneObject != null;

		if ((!hasTarget && !GameObject.IsValid()) || rippleBuffer == null)
			return;

		var triggerCollider = Components.Get<Collider>();
		if (triggerCollider == null)
			return;

		var triggerBounds = triggerCollider.GetWorldBounds();
		var triggerTop = triggerBounds.Maxs.z;

		// Detect physical contacts once on the host, then broadcast the cosmetic
		// ripple so every peer renders the same event (so peers aren't all broadcasting the
		// same ripple to each other)
		if (Networking.IsHost)
		{
			_collidersToRemove.Clear();
			_processedModelPhysics.Clear();

			foreach (var collider in trackedColliders)
			{
				if (collider == null || !collider.IsValid)
				{
					_collidersToRemove.Add(collider);
					continue;
				}

				if (!WaterSimulationIgnoreTags.IsEmpty && collider.GameObject.Tags.HasAny(WaterSimulationIgnoreTags))
					continue;

				var modelPhysics = collider.GameObject.Components.GetInAncestorsOrSelf<ModelPhysics>();

				if (modelPhysics != null)
				{
					if (_processedModelPhysics.Add(modelPhysics))
						ProcessModelPhysics(modelPhysics, triggerTop);
				}
				else
				{
					var rigidbody = collider.GameObject.Components.Get<Rigidbody>();
					if (rigidbody != null)
						ProcessRigidbody(rigidbody, collider, triggerTop);
				}
			}

			foreach (var collider in _collidersToRemove)
				trackedColliders.Remove(collider);
		}

		UpdateRipples();
		UpdateBuffer();
	}

	private void ProcessModelPhysics(ModelPhysics modelPhysics, float triggerTop)
	{
		if (lastModelRippleTime.TryGetValue(modelPhysics, out var timeSince) && timeSince < WaterSimulationSpawnInterval)
			return;

		float totalMass = 0f;
		Vector3 weightedContactPoint = Vector3.Zero;
		float weightedContactRadius = 0f;
		float weightedVelocityKmh = 0f;

		foreach (var body in modelPhysics.Bodies)
		{
			var rb = body.Component;
			if (!rb.IsValid())
				continue;

			var physicsBody = rb.PhysicsBody;
			if (physicsBody == null || !physicsBody.IsValid())
				continue;

			var bodyBounds = physicsBody.GetBounds();
			if (!IsAtSurface(bodyBounds, triggerTop))
				continue;

			var velocityInchesPerSec = rb.Velocity.Length;
			var velocityKmh = velocityInchesPerSec * INCHES_PER_SEC_TO_KMH;

			if (velocityKmh < WaterSimulationMinimumVelocity)
				continue;

			var contactPoint = GetContactPoint(bodyBounds, triggerTop);
			var contactRadius = CalculateContactRadius(bodyBounds);
			float bodyMass = physicsBody.Mass;
			if (!float.IsFinite(bodyMass) || bodyMass <= 0f)
				bodyMass = 1f;

			totalMass += bodyMass;
			weightedContactPoint += contactPoint * bodyMass;
			weightedContactRadius += contactRadius * bodyMass;
			weightedVelocityKmh += velocityKmh * bodyMass;
		}

		if (totalMass <= 0f)
			return;

		CreateRipple(
			weightedContactPoint / totalMass,
			weightedContactRadius / totalMass,
			weightedVelocityKmh / totalMass);
		lastModelRippleTime[modelPhysics] = 0;
	}

	private void ProcessRigidbody(Rigidbody rb, Collider collider, float triggerTop)
	{
		if (lastRippleTime.TryGetValue(rb, out var timeSince) && timeSince < WaterSimulationSpawnInterval)
			return;

		var colliderBounds = collider.GetWorldBounds();
		if (!IsAtSurface(colliderBounds, triggerTop))
			return;

		var velocityInchesPerSec = rb.Velocity.Length;
		var velocityKmh = velocityInchesPerSec * INCHES_PER_SEC_TO_KMH;

		if (velocityKmh < WaterSimulationMinimumVelocity)
			return;

		var contactPoint = GetContactPoint(colliderBounds, triggerTop);
		var contactRadius = CalculateContactRadius(colliderBounds);

		CreateRipple(contactPoint, contactRadius, velocityKmh);
		lastRippleTime[rb] = 0;
	}

	private bool IsAtSurface(BBox bounds, float triggerTop)
	{
		return bounds.Mins.z < triggerTop && bounds.Maxs.z >= triggerTop;
	}

	private Vector3 GetContactPoint(BBox bounds, float triggerTop)
	{
		return new Vector3(bounds.Center.x, bounds.Center.y, triggerTop);
	}

	private static float CalculateContactRadius(BBox bounds)
	{
		float horizontalSize = MathF.Min(bounds.Size.x, bounds.Size.y);
		return horizontalSize / 2f;
	}

	[Rpc.Broadcast(NetFlags.HostOnly | NetFlags.Unreliable | NetFlags.DiscardOnDelay)]
	private void CreateRipple(Vector3 position, float contactRadius, float velocityKmh)
	{
		AddRipple(position, contactRadius, velocityKmh);
	}

	private void AddRipple(Vector3 position, float contactRadius, float velocityKmh)
	{
		int maximumActiveRipples = GetMaximumActiveRipples();
		if (ripples.Count >= maximumActiveRipples)
			return;

		contactRadius = MathF.Max(contactRadius, 0.01f);
		velocityKmh = MathF.Max(velocityKmh, 0f);

		float velocityScale = velocityKmh * WaterSimulationVelocityMultiplier;
		float rotation = Random.Shared.NextSingle() * MathF.PI * 2f;

		var data = new RippleData
		{
			SpawnTime = Time.Now,
			InitialRadius = contactRadius,
			Rotation = rotation,
			VelocityScale = velocityScale
		};

		var ripple = new WaterSimulation
		{
			Position = position,
			Radius = contactRadius,  // Start at base radius, scaling applied in UpdateRipples
			Alpha = 0f,
			Rotation = rotation,
			Progress = 0f
		};

		ripples.Add(ripple);
		rippleData.Add(data);
	}

	private void UpdateRipples()
	{
		TrimRipplesToMaximum();

		float totalLifetime = WaterSimulationFadeInTime + WaterSimulationConstantTime + WaterSimulationFadeOutTime;

		for (int i = ripples.Count - 1; i >= 0; i--)
		{
			var ripple = ripples[i];
			var data = rippleData[i];
			float t = Time.Now - data.SpawnTime;

			// Remove if past total lifetime
			if (t >= totalLifetime)
			{
				ripples.RemoveAt(i);
				rippleData.RemoveAt(i);
				continue;
			}

			// Calculate alpha based on phase
			float alpha;
			if (t < WaterSimulationFadeInTime)
			{
				// Phase 1: Fade In
				alpha = WaterSimulationFadeInTime > 0 ? t / WaterSimulationFadeInTime : 1f;
			}
			else if (t < WaterSimulationFadeInTime + WaterSimulationConstantTime)
			{
				// Phase 2: Constant
				alpha = 1f;
			}
			else
			{
				// Phase 3: Fade Out
				float fadeOutProgress = WaterSimulationFadeOutTime > 0
					? (t - WaterSimulationFadeInTime - WaterSimulationConstantTime) / WaterSimulationFadeOutTime
					: 1f;
				alpha = 1f - fadeOutProgress;
			}

			// Calculate radius based on lifetime progress
			// Both WaterSimulationScaleMultiplier and VelocityScale gradually increase from 1.0 to their final values
			float progress = t / totalLifetime;
			float scaleProgress = 1f + (WaterSimulationScaleMultiplier - 1f) * progress;
			float velocityProgress = 1f + data.VelocityScale * progress;
			ripple.Radius = data.InitialRadius * scaleProgress * velocityProgress;

			ripple.Alpha = alpha;
			ripple.Rotation = data.Rotation;
			ripple.Progress = progress;

			ripples[i] = ripple;
		}
	}

	private int GetMaximumActiveRipples()
	{
		return Math.Clamp(WaterSimulationMaximumActiveRipples, 1, 128);
	}

	private int FindOldestRippleIndex()
	{
		int oldestIndex = 0;
		float oldestSpawnTime = rippleData[0].SpawnTime;

		for (int i = 1; i < rippleData.Count; i++)
		{
			if (rippleData[i].SpawnTime >= oldestSpawnTime)
				continue;

			oldestSpawnTime = rippleData[i].SpawnTime;
			oldestIndex = i;
		}

		return oldestIndex;
	}

	private void TrimRipplesToMaximum()
	{
		int maximumActiveRipples = GetMaximumActiveRipples();
		while (ripples.Count > maximumActiveRipples)
		{
			int oldestIndex = FindOldestRippleIndex();
			ripples.RemoveAt(oldestIndex);
			rippleData.RemoveAt(oldestIndex);
		}
	}

	private void UpdateBuffer()
	{
		int desiredSize = GetMaximumActiveRipples();
		bool bufferRecreated = rippleBuffer == null || desiredSize != currentBufferSize;

		if (bufferRecreated)
		{
			rippleBuffer?.Dispose();
			rippleBuffer = new GpuBuffer<WaterSimulation>(desiredSize);
			currentBufferSize = desiredSize;
		}

		if (ripples.Count > 0)
		{
			rippleBuffer.SetData(ripples);
		}

		// Idle water doesn't need stamping, only push attributes while ripples are live,
		// when the count changes, or when the buffer object itself was recreated
		// (renderers would otherwise reference the disposed buffer)
		if (ripples.Count == 0 && _lastStampedRippleCount == 0 && !bufferRecreated)
			return;

		_lastStampedRippleCount = ripples.Count;

		if (_rippleRenderersDirty)
			RebuildRippleRendererCache();

		foreach (var renderer in _rippleRenderers)
		{
			if (renderer.IsValid() && renderer.SceneObject.IsValid())
			{
				renderer.SceneObject.Attributes.Set("WaterSimulationBuffer", rippleBuffer);
				renderer.SceneObject.Attributes.Set("MaxRipples", ripples.Count);
				renderer.SceneObject.Attributes.Set("WaterSimulationTexture", WaterSimulationTexture);
				renderer.SceneObject.Attributes.Set("Water Simulation Refraction", WaterSimulationRefraction);
				renderer.SceneObject.Attributes.Set("Water Simulation Displacement", WaterSimulationDisplacement);
			}
		}

		// Also push to TargetRenderer in case it's a separate, non-descendant renderer.
		if (TargetRenderer?.SceneObject != null)
		{
			TargetRenderer.SceneObject.Attributes.Set("WaterSimulationBuffer", rippleBuffer);
			TargetRenderer.SceneObject.Attributes.Set("MaxRipples", ripples.Count);
			TargetRenderer.SceneObject.Attributes.Set("WaterSimulationTexture", WaterSimulationTexture);
			TargetRenderer.SceneObject.Attributes.Set("Water Simulation Refraction", WaterSimulationRefraction);
			TargetRenderer.SceneObject.Attributes.Set("Water Simulation Displacement", WaterSimulationDisplacement);
		}
	}
}
