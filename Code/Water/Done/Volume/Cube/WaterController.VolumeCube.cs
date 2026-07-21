// namespace AtmokineticAssets;

// [StructLayout(LayoutKind.Sequential)]
// public struct WetnessVolumeCubeBuffer
// {
// 	public Vector3 VolumeCenter;
// 	public Vector3 VolumeSize;
// 	public float WetnessVolumeExtend;
// 	public float WetnessVolumeFade;
// 	public float WetnessVolumeSoftness;
// }

// public sealed partial class WaterController : Component
// {
// 	[Property, Title("Bounds Minimum (cm)"), Feature("Wetness"), Group("Properties"), ShowIf( nameof(WetnessVolumeMode), WetnessVolumeModeEnumeration.Cube )]
// 	public Vector3 WetnessVolumeCubeBoundsMinimum {get;set;} = new Vector3(-500, -500, -500);

// 	[Property, Title("Bounds Maximum (cm)"), Feature("Wetness"), Group("Properties"), ShowIf( nameof(WetnessVolumeMode), WetnessVolumeModeEnumeration.Cube )]
// 	public Vector3 WetnessVolumeCubeBoundsMaximum {get;set;} = new Vector3( 500, 500, 500 );

// 	[Property, Title( "Enable Gizmo" ), Feature("Wetness"), Group("Properties"), ShowIf( nameof(WetnessVolumeMode), WetnessVolumeModeEnumeration.Cube )]
// 	public bool EnableWetnessVolumeCubeGizmos { get; set; } = true;

// 	private static readonly List<WaterController> _wetnessVolumeCubeList = new();
// 	private static GpuBuffer<WetnessVolumeCubeBuffer> _wetnessVolumeCubeBuffer;
// 	private static int _currentWetnessVolumeCubeBuffer = 0;

// 	public void EnableWetnessVolumeCubeBuffer()
// 	{
// 		if (!_wetnessVolumeCubeList.Contains(this))
// 			_wetnessVolumeCubeList.Add(this);

// 		if (_wetnessVolumeCubeBuffer == null)
// 		{
// 			_wetnessVolumeCubeBuffer = new GpuBuffer<WetnessVolumeCubeBuffer>(1);
// 			_currentWetnessVolumeCubeBuffer = 1;
// 		}
// 		UpdateWetnessVolumeCubeBuffer();
// 	}

// 	public void DisableWetnessVolumeCubeBuffer()
// 	{
// 		_wetnessVolumeCubeList.Remove( this );
// 		UpdateWetnessVolumeCubeBuffer();
// 	}

// 	public static void UpdateWetnessVolumeCubeBuffer()
// 	{
// 		if (_wetnessVolumeCubeList.Count == 0) return;

// 		var volumeBufferList = new List<WetnessVolumeCubeBuffer>();

// 		foreach (var wetnessVolumeCube in _wetnessVolumeCubeList)
// 		{
// 			if (!wetnessVolumeCube.IsValid()) continue;

// 			// Convert cm to inches
// 			var boundsMinimumInches = UnitConversion.CentimetersToInches(wetnessVolumeCube.WetnessVolumeCubeBoundsMinimum);
// 			var boundsMaximumInches = UnitConversion.CentimetersToInches(wetnessVolumeCube.WetnessVolumeCubeBoundsMaximum);

// 			// Calculate world space center and size
// 			Vector3 localCenter = (boundsMinimumInches + boundsMaximumInches) * 0.5f;
// 			Vector3 worldCenter = wetnessVolumeCube.WorldPosition + localCenter;
// 			Vector3 size = boundsMaximumInches - boundsMinimumInches;

// 			volumeBufferList.Add(new WetnessVolumeCubeBuffer
// 			{
// 				VolumeCenter = worldCenter,
// 				WetnessVolumeExtend = UnitConversion.CentimetersToInches( wetnessVolumeCube.WetnessVolumeExtend ),
// 				VolumeSize = size,
// 				WetnessVolumeFade = wetnessVolumeCube.WetnessVolumeFade,
// 				WetnessVolumeSoftness = wetnessVolumeCube.WetnessVolumeSoftness,

// 			});
// 		}

// 		if (_wetnessVolumeCubeBuffer == null || volumeBufferList.Count > _currentWetnessVolumeCubeBuffer)
// 		{
// 			_wetnessVolumeCubeBuffer = new GpuBuffer<WetnessVolumeCubeBuffer>(volumeBufferList.Count);
// 			_currentWetnessVolumeCubeBuffer = volumeBufferList.Count;
// 		}

// 		if (volumeBufferList.Count > 0)
// 		{
// 			_wetnessVolumeCubeBuffer.SetData(volumeBufferList);
// 		}

// 		if (Game.ActiveScene?.SceneWorld != null)
// 		{
// 			var sceneObjects = Game.ActiveScene.SceneWorld.SceneObjects
// 				.Where(x => x != null && x.IsValid())
// 				.ToList();

// 			foreach (var sceneObject in sceneObjects)
// 			{
// 				if (sceneObject.IsValid())
// 				{
// 					sceneObject.Attributes.Set("WetnessWetnessVolumeCubeBuffer", _wetnessVolumeCubeBuffer);
// 					sceneObject.Attributes.Set("MaximumWetnessWetnessVolumeCubes", volumeBufferList.Count);
// 				}
// 			}
// 		}
// 	}
// }
