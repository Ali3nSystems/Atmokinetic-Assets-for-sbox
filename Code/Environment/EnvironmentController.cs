namespace AtmokineticAssets;

[Title("Atmokinetic Assets - Environment Controller")]
[Category("Atmokinetic Assets")]
[Icon("cloud")]

public sealed partial class EnvironmentController : Component
{
	protected override void OnAwake()
	{
		CacheWaterControllers();
	}

	protected override void OnUpdate()
	{
		
	} 

	// public enum PrecipitationModeEnumeration
	// {
	// 	None,
	// 	Rain,
	// 	Snow
	// }

	// [Property, Feature("Weather"), Sync]
	// public PrecipitationModeEnumeration PrecipitationMode { get; set; } = PrecipitationModeEnumeration.None;

	// [Property, Feature("Weather")] PrefabScene PrecipitationController { get; set;}

	// [Property, Feature("Weather"), Range( 0f, 1f ), Sync]
	// public float WaterPuddles { get; set; } = 0f;

	// [Property, Feature("Weather"), Range( 0.0001f, 1f ), Sync]
	// public float WaterLevel { get; set; } = 0f;

	// [Property, Feature("Weather"), Range( 0, 4 ), Step(1), Sync]
	// public float PrecipitationIntensity { get; set; } = 0f;

	// [Property,Feature("Time"), Sync]
	// public GameObject Sun {get;set;}

	// [Property, Feature("Time"), Range(0,24), Sync]
	// public float Hour {get;set;} = 6f;
	
	// [Property, Feature("Time"), Range(1,31), Step(1)]
	// public float Day {get;set;} = 0f;
	
	// [Property, Feature("Time"), Range(1,12), Step(1)]
	// public float Month {get;set;} = 0f;
	
	// [Property, Feature("Time"), Step(1)]
	// public float Year {get;set;} = 0f;

	// private float _lastWaterPuddlesValue = -1f;
	// private float _lastPrecipitationIntensityValue = -1f;
	// private float _lastPrecipitationIntensityValueForParticles = -1f;
	// private float _lastWaterLevelValue = -1f;
	
	// private List<SceneObject> _cachedSceneObjects;
	// private List<EnvmapProbe> _cachedEnvmapProbes;

	// // Rain Precipitation
	// private GameObject _rainGameObject;
	// private ParticleEffect _particleEffectRain;
	// private PrecipitationController _precipitationControllerRain;
	// private PrecipitationControllerEmitter _precipitationControllerRainEmitter;
	// private bool _activePrecipitationControllerRain;

	// // Snow Precipitation
	// private GameObject _snowGameObject;
	// private ParticleEffect _particleEffectSnow;
	// private PrecipitationController _precipitationControllerSnow;
	// private PrecipitationControllerEmitter _precipitationControllerSnowEmitter;
	// private bool _activePrecipitationControllerSnow;

	// private PrecipitationModeEnumeration _lastPrecipitationMode = PrecipitationModeEnumeration.None;

	// // Lerp state
	// private float _startMaxParticles;
	// private float _startRate;
	// private float _targetMaxParticles;
	// private float _targetRate;
	// private float _transitionProgress = 1f;
	// private float _lastHour = -1f;

	// protected override void OnStart()
	// {
	// 	// Cache all scene objects on startup
	// 	_cachedSceneObjects = Scene.SceneWorld.SceneObjects.Where( x => x != null ).ToList();

	// 	// Cache all environment map probes
	// 	_cachedEnvmapProbes = Scene.GetAllComponents<EnvmapProbe>().ToList();
	// }
	
	// [Rpc.Broadcast]
	// protected override void OnUpdate()
	// {
	// 	HandlePrecipitationModeChange();
	// 	PrecipitationEffects();
	// 	TimeEffects();
	// }

	// protected override void DrawGizmos()
	// {
	// 	if (_cachedSceneObjects == null)
	// 	{
	// 		_cachedSceneObjects = Scene.SceneWorld.SceneObjects.Where( x => x != null ).ToList();
	// 	}

	// 	if (_cachedEnvmapProbes == null)
	// 	{
	// 		_cachedEnvmapProbes = Scene.GetAllComponents<EnvmapProbe>().ToList();
	// 	}

	// 	if ( WaterPuddles != _lastWaterPuddlesValue || WaterLevel != _lastWaterLevelValue || PrecipitationIntensity != _lastPrecipitationIntensityValue)
	// 	{
	// 		_lastWaterPuddlesValue = WaterPuddles;
	// 		_lastWaterLevelValue = WaterLevel;
	// 		_lastPrecipitationIntensityValue = PrecipitationIntensity;

	// 		foreach ( var sceneModel in _cachedSceneObjects )
	// 		{
	// 			sceneModel.Attributes.Set( "Water Puddles Accumulation", WaterPuddles );
	// 			sceneModel.Attributes.Set( "Water Plane Level", WaterLevel );
	// 			sceneModel.Attributes.Set( "Rain Ripples Intensity", PrecipitationIntensity );
	// 		}
	// 	}

	// 	TimeEffects();
	// }

	// private void HandlePrecipitationModeChange()
	// {
	// 	if (PrecipitationMode == _lastPrecipitationMode) return;

	// 	// Only host spawns networked objects

	// 	// Handle mode transitions
	// 	if (_lastPrecipitationMode == PrecipitationModeEnumeration.Rain)
	// 	{
	// 		TransitionOutRain();
	// 	}
	// 	else if (_lastPrecipitationMode == PrecipitationModeEnumeration.Snow)
	// 	{
	// 		TransitionOutSnow();
	// 	}

	// 	// Spawn new precipitation if not None
	// 	if (PrecipitationMode == PrecipitationModeEnumeration.Rain)
	// 	{
	// 		SpawnRain();
	// 	}
	// 	else if (PrecipitationMode == PrecipitationModeEnumeration.Snow)
	// 	{
	// 		SpawnSnow();
	// 	}

	// 	_lastPrecipitationMode = PrecipitationMode;
	// }

	// private void SpawnRain()
	// {
	// 	// Reset transition state to trigger fresh lerp
	// 	_lastPrecipitationIntensityValue = -1f;
	// 	_lastPrecipitationIntensityValueForParticles = -1f;

	// 	// If rain instance already exists (transitioning out), reactivate it
	// 	if (_precipitationControllerRain.IsValid())
	// 	{
	// 		_activePrecipitationControllerRain = true;
	// 		return;
	// 	}

	// 	if (!PrecipitationController.IsValid()) return;

	// 	var clonedGameObject = PrecipitationController.Clone();
	// 	clonedGameObject.Name = "Precipitation Controller (Rain)";

	// 	_rainGameObject = clonedGameObject;
	// 	_particleEffectRain = clonedGameObject.GetComponent<ParticleEffect>();
	// 	_precipitationControllerRain = clonedGameObject.GetComponent<PrecipitationController>();
	// 	_precipitationControllerRainEmitter = clonedGameObject.GetComponent<PrecipitationControllerEmitter>();

	// 	_precipitationControllerRain.PrecipitationMode = AtmokineticAssets.PrecipitationController.PrecipitationModeEnumeration.Rain;

	// 	_particleEffectRain.MaxParticles = 0;
	// 	_precipitationControllerRainEmitter.Rate = 0;
	// 	_activePrecipitationControllerRain = true;

	// 	// Reset transition to ensure smooth lerp from current state
	// }

	// private void SpawnSnow()
	// {
	// 	// Reset transition state to trigger fresh lerp
	// 	_lastPrecipitationIntensityValue = -1f;
	// 	_lastPrecipitationIntensityValueForParticles = -1f;

	// 	// If snow instance already exists (transitioning out), reactivate it
	// 	if (_precipitationControllerSnow.IsValid())
	// 	{
	// 		_activePrecipitationControllerSnow = true;
	// 		return;
	// 	}

	// 	if (!PrecipitationController.IsValid()) return;

	// 	var clonedGameObject = PrecipitationController.Clone();
	// 	clonedGameObject.Name = "Precipitation Controller (Snow)";

	// 	_snowGameObject = clonedGameObject;
	// 	_particleEffectSnow = clonedGameObject.GetComponent<ParticleEffect>();
	// 	_precipitationControllerSnow = clonedGameObject.GetComponent<PrecipitationController>();
	// 	_precipitationControllerSnowEmitter = clonedGameObject.GetComponent<PrecipitationControllerEmitter>();

	// 	_precipitationControllerSnow.PrecipitationMode = AtmokineticAssets.PrecipitationController.PrecipitationModeEnumeration.Snow;

	// 	_particleEffectSnow.MaxParticles = 0;
	// 	_precipitationControllerSnowEmitter.Rate = 0;
	// 	_activePrecipitationControllerSnow = true;

	// 	// Reset transition to ensure smooth lerp from current state
	// }

	// private void TransitionOutRain() => _activePrecipitationControllerRain = false;

	// private void TransitionOutSnow() => _activePrecipitationControllerSnow = false;
	
	// public void PrecipitationEffects()
	// {
	// 	if (_precipitationControllerRain.IsValid())
	// 	{
	// 		if (_activePrecipitationControllerRain)
	// 		{
	// 			UpdatePrecipitationTransition(
	// 				_particleEffectRain,
	// 				_precipitationControllerRainEmitter,
	// 				_precipitationControllerRain.RainIntensityMaximumParticlesLight,
	// 				_precipitationControllerRain.RainIntensityRateLight,
	// 				_precipitationControllerRain.RainIntensityMaximumParticlesModerate,
	// 				_precipitationControllerRain.RainIntensityRateModerate,
	// 				_precipitationControllerRain.RainIntensityMaximumParticlesHeavy,
	// 				_precipitationControllerRain.RainIntensityRateHeavy,
	// 				_precipitationControllerRain.RainIntensityMaximumParticlesExtreme,
	// 				_precipitationControllerRain.RainIntensityRateExtreme,
	// 				_precipitationControllerRain.RainParticleTransition
	// 			);
	// 		}
	// 		else
	// 		{
	// 			TransitionPrecipitationToZero(_particleEffectRain, _precipitationControllerRainEmitter, _precipitationControllerRain.RainParticleTransition);
	// 		}
	// 	}

	// 	if (_precipitationControllerSnow.IsValid())
	// 	{
	// 		if (_activePrecipitationControllerSnow)
	// 		{
	// 			UpdatePrecipitationTransition(
	// 				_particleEffectSnow,
	// 				_precipitationControllerSnowEmitter,
	// 				_precipitationControllerSnow.SnowIntensityMaximumParticlesLight,
	// 				_precipitationControllerSnow.SnowIntensityRateLight,
	// 				_precipitationControllerSnow.SnowIntensityMaximumParticlesModerate,
	// 				_precipitationControllerSnow.SnowIntensityRateModerate,
	// 				_precipitationControllerSnow.SnowIntensityMaximumParticlesHeavy,
	// 				_precipitationControllerSnow.SnowIntensityRateHeavy,
	// 				_precipitationControllerSnow.SnowIntensityMaximumParticlesExtreme,
	// 				_precipitationControllerSnow.SnowIntensityRateExtreme,
	// 				_precipitationControllerSnow.SnowParticleTransition
	// 			);
	// 		}
	// 		else
	// 		{
	// 			TransitionPrecipitationToZero(_particleEffectSnow, _precipitationControllerSnowEmitter, _precipitationControllerSnow.SnowParticleTransition);
	// 		}
	// 	}

	// 	if ( WaterPuddles != _lastWaterPuddlesValue || WaterLevel != _lastWaterLevelValue || PrecipitationIntensity != _lastPrecipitationIntensityValue)
	// 	{
	// 		_lastWaterPuddlesValue = WaterPuddles;
	// 		_lastWaterLevelValue = WaterLevel;
	// 		_lastPrecipitationIntensityValue = PrecipitationIntensity;

	// 		foreach ( var sceneModel in _cachedSceneObjects )
	// 		{
	// 			sceneModel.Attributes.Set( "Water Puddles Accumulation", WaterPuddles );
	// 			sceneModel.Attributes.Set( "Water Plane Level", WaterLevel );
	// 			sceneModel.Attributes.Set( "Rain Ripples Intensity", PrecipitationIntensity );
	// 		}
	// 	}
	// }

	// private void UpdatePrecipitationTransition(
	// 	ParticleEffect particleEffect,
	// 	PrecipitationControllerEmitter emitter,
	// 	int lightMaxParticles, int lightRate,
	// 	int moderateMaxParticles, int moderateRate,
	// 	int heavyMaxParticles, int heavyRate,
	// 	int extremeMaxParticles, int extremeRate,
	// 	float transitionDuration)
	// {
	// 	if ( PrecipitationIntensity != _lastPrecipitationIntensityValueForParticles )
	// 	{
	// 		_lastPrecipitationIntensityValueForParticles = PrecipitationIntensity;

	// 		_startMaxParticles = particleEffect.MaxParticles;
	// 		_startRate = emitter.Rate.ConstantValue;

	// 		int intensity = (int)PrecipitationIntensity;
	// 		(_targetMaxParticles, _targetRate) = intensity switch
	// 		{
	// 			1 => (lightMaxParticles, lightRate),
	// 			2 => (moderateMaxParticles, moderateRate),
	// 			3 => (heavyMaxParticles, heavyRate),
	// 			4 => (extremeMaxParticles, extremeRate),
	// 			_ => (0, 0)
	// 		};
	// 		_transitionProgress = 0f;
	// 	}

	// 	if ( _transitionProgress < 1f )
	// 	{
	// 		_transitionProgress += Time.Delta / transitionDuration;
	// 		if ( _transitionProgress >= 1f )
	// 		{
	// 			_transitionProgress = 1f;
	// 			particleEffect.MaxParticles = (int)_targetMaxParticles;
	// 			emitter.Rate = _targetRate;
	// 		}
	// 		else
	// 		{
	// 			particleEffect.MaxParticles = (int)(_startMaxParticles + (_targetMaxParticles - _startMaxParticles) * _transitionProgress);
	// 			emitter.Rate = _startRate + (_targetRate - _startRate) * _transitionProgress;
	// 		}
	// 	}
	// }

	// private void TransitionPrecipitationToZero(ParticleEffect particleEffect, PrecipitationControllerEmitter emitter, float transitionDuration)
	// {
	// 	float step = Time.Delta / transitionDuration;
	// 	particleEffect.MaxParticles = (int)Math.Max(0, particleEffect.MaxParticles - (particleEffect.MaxParticles * step));
	// 	emitter.Rate = Math.Max(0, emitter.Rate.ConstantValue - (emitter.Rate.ConstantValue * step));

	// 	if (particleEffect.MaxParticles == 0)
	// 	{
	// 		if (_precipitationControllerRain.IsValid() && particleEffect == _particleEffectRain)
	// 		{
	// 			_rainGameObject?.Destroy();
	// 			_rainGameObject = null;
	// 			_precipitationControllerRain = null;
	// 			_particleEffectRain = null;
	// 			_precipitationControllerRainEmitter = null;
	// 		}
	// 		else if (_precipitationControllerSnow.IsValid() && particleEffect == _particleEffectSnow)
	// 		{
	// 			_snowGameObject?.Destroy();
	// 			_snowGameObject = null;
	// 			_precipitationControllerSnow = null;
	// 			_particleEffectSnow = null;
	// 			_precipitationControllerSnowEmitter = null;
	// 		}
	// 	}
	// }
	// public void TimeEffects()
	// {
	// 	if ( Hour != _lastHour)
	// 	{
	// 		_lastHour = Hour;

	// 		const float sunriseStart = 5.6f;
	// 		const float sunrisePeak  = 6.3f;
	// 		const float sunriseEnd   = 7f;
	// 		const float sunsetStart  = 17f;
	// 		const float sunsetPeak   = 18.0f;
	// 		const float sunsetEnd    = 18.4f;
	// 		const float degreesPerHour = 15f;
	// 		const float sunAngleOffset = 90f;

	// 		Sun.WorldRotation = Rotation.From( (Hour * degreesPerHour) - sunAngleOffset, 0, 0 );

	// 		var directionalLight = Sun.Components.Get<DirectionalLight>();
	// 		if ( directionalLight.IsValid() )
	// 		{
	// 			var goldenHour = new Color( 1.0f, 0.45f, 0.1f );
	// 			Color sunColor;

	// 			if ( Hour >= sunsetEnd || Hour < sunriseStart )
	// 			{
	// 				sunColor = Color.Black;
	// 			}
	// 			else if ( Hour >= sunriseStart && Hour < sunrisePeak )
	// 			{
	// 				float time = (Hour - sunriseStart) / (sunrisePeak - sunriseStart);
	// 				sunColor = Color.Lerp( Color.Black, goldenHour, time );
	// 			}
	// 			else if ( Hour >= sunrisePeak && Hour < sunriseEnd )
	// 			{
	// 				float time = (Hour - sunrisePeak) / (sunriseEnd - sunrisePeak);
	// 				sunColor = Color.Lerp( goldenHour, Color.White, time );
	// 			}
	// 			else if ( Hour >= sunriseEnd && Hour < sunsetStart )
	// 			{
	// 				sunColor = Color.White;
	// 			}
	// 			else if ( Hour >= sunsetStart && Hour < sunsetPeak )
	// 			{
	// 				float time = (Hour - sunsetStart) / (sunsetPeak - sunsetStart);
	// 				sunColor = Color.Lerp( Color.White, goldenHour, time );
	// 			}
	// 			else
	// 			{
	// 				float time = (Hour - sunsetPeak) / (sunsetEnd - sunsetPeak);
	// 				sunColor = Color.Lerp( goldenHour, Color.Black, time );
	// 			}

	// 			directionalLight.LightColor = sunColor;
	// 		}

	// 		foreach ( var envmapProbe in _cachedEnvmapProbes )
	// 		{
	// 			if ( envmapProbe.IsValid() )
	// 			{
	// 				envmapProbe.Dirty = true;
	// 			}
	// 		}
	// 	}
	// }
}