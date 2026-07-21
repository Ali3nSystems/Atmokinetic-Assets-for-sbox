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

	public class RippleData
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

	// Reused per frame instead of allocating a new list every update.
	private readonly List<Collider> _collidersToRemove = new();

	// Renderers the ripple attributes are stamped onto (tiles + waterline children),
	// cached so the per-frame path never walks the descendant hierarchy. Rebuilt when
	// the tile or waterline set changes.
	private readonly List<ModelRenderer> _rippleRenderers = new();
	private bool _rippleRenderersDirty = true;
	private int _lastStampedRippleCount = -1;

	private void MarkRippleRenderersDirty() => _rippleRenderersDirty = true;

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
		// Need a TargetRenderer or the main GameObject as the tile creator.
		bool hasTarget = TargetRenderer?.SceneObject != null;

		if ((!hasTarget && !GameObject.IsValid()) || rippleBuffer == null)
			return;

		var triggerCollider = Components.Get<Collider>();
		if (triggerCollider == null)
			return;

		var triggerBounds = triggerCollider.GetWorldBounds();
		var triggerTop = triggerBounds.Maxs.z;

		// Ripples are purely cosmetic, so every client detects splashes locally from
		// the replicated physics - no RPC needed, no network traffic, and proxies'
		// splashes appear because their trigger events and velocity replicate.
		_collidersToRemove.Clear();

		foreach (var collider in trackedColliders)
		{
			if (collider == null || !collider.IsValid)
			{
				_collidersToRemove.Add(collider);
				continue;
			}

			var modelPhysics = collider.GameObject.Components.GetInAncestorsOrSelf<ModelPhysics>();

			if (modelPhysics != null)
			{
				ProcessModelPhysics(modelPhysics, triggerTop);
			}
			else
			{
				var rigidbody = collider.GameObject.Components.Get<Rigidbody>();
				if (rigidbody != null)
				{
					ProcessRigidbody(rigidbody, collider, triggerTop);
				}
			}
		}

		foreach (var collider in _collidersToRemove)
		{
			trackedColliders.Remove(collider);
		}

		UpdateRipples();
		UpdateBuffer();
	}

	private void ProcessModelPhysics(ModelPhysics modelPhysics, float triggerTop)
	{
		foreach (var body in modelPhysics.Bodies)
		{
			var rb = body.Component;
			if (!rb.IsValid())
				continue;

			if (lastRippleTime.TryGetValue(rb, out var timeSince) && timeSince < WaterSimulationSpawnInterval)
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

			CreateRipple(contactPoint, contactRadius, velocityKmh);
			lastRippleTime[rb] = 0;
		}
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

	[Rpc.Broadcast]
	private void CreateRipple(Vector3 position, float contactRadius, float velocityKmh)
	{
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

	//[Rpc.Broadcast]
	private void UpdateBuffer()
	{
		// Track ripple count with geometric growth and hysteresis shrink, so long
		// sessions of continuous movement don't reallocate the buffer every time a
		// ripple spawns or expires - grows by doubling, shrinks by halving only once
		// the count falls to a quarter of capacity.
		int desiredSize = currentBufferSize;

		if (ripples.Count > currentBufferSize)
			desiredSize = Math.Max(ripples.Count, currentBufferSize * 2);
		else if (currentBufferSize > 1 && ripples.Count <= currentBufferSize / 4)
			desiredSize = Math.Max(1, currentBufferSize / 2);

		bool bufferRecreated = desiredSize != currentBufferSize;

		if (bufferRecreated)
		{
			rippleBuffer?.Dispose();
			rippleBuffer = new GpuBuffer<WaterSimulation>(desiredSize);
			currentBufferSize = desiredSize;

			// Zero freshly allocated buffers we won't fill below, so no stale
			// ripple data lingers on the GPU.
			if (ripples.Count == 0)
				rippleBuffer.SetData(new WaterSimulation[desiredSize]);
		}

		if (ripples.Count > 0)
		{
			rippleBuffer.SetData(ripples);
		}

		// Idle water needs no stamping - only push attributes while ripples are live,
		// when the count changes, or when the buffer object itself was recreated
		// (renderers would otherwise reference the disposed buffer).
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