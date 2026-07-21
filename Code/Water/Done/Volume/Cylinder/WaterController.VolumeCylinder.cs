// namespace AtmokineticAssets;

// [StructLayout( LayoutKind.Sequential )]
// public struct WetnessVolumeCylinderBuffer
// {
// 	public Vector3 VolumeCenter;
// 	public Vector3 VolumeSize;
// 	public float WetnessVolumeExtend;
// 	public float WetnessVolumeFade;
// 	public float WetnessVolumeSoftness;
// }

// public sealed partial class WaterController : Component
// {
// 	[Property, Title( "Radius (cm)" ), Feature("Wetness"), Group( "Properties" ), ShowIf( nameof(WetnessVolumeMode), WetnessVolumeModeEnumeration.Cylinder )]
// 	public float WetnessVolumeCylinderRadius { get; set; } = 500f;

// 	[Property, Title( "Depth Maximum (cm)" ), Feature("Wetness"), Group( "Properties" ), ShowIf( nameof(WetnessVolumeMode), WetnessVolumeModeEnumeration.Cylinder)]
// 	public float WetnessVolumeCylinderDepthMaximum { get; set; } = 500f;

// 	[Property, Title( "Depth Minimum (cm)" ), Feature("Wetness"), Group( "Properties" ), ShowIf( nameof(WetnessVolumeMode), WetnessVolumeModeEnumeration.Cylinder)]
// 	public float WetnessVolumeCylinderDepthMinimum { get; set; } = 500f;

// 	[Property, Title( "Enable Gizmo"  ), Feature("Wetness"), Group( "Properties" ), ShowIf( nameof(WetnessVolumeMode), WetnessVolumeModeEnumeration.Cylinder)]
// 	public bool EnableWetnessVolumeCylinderGizmos { get; set; } = true;

// 	private static readonly List<WaterController> _wetnessVolumeCylinderList = new();
// 	private static GpuBuffer<WetnessVolumeCylinderBuffer> _wetnessVolumeCylinderBuffer;
// 	private static int _currentWetnessVolumeCylinderBuffer = 0;

// 	public void EnableWetnessVolumeCylinderBuffer()
// 	{
// 		if ( !_wetnessVolumeCylinderList.Contains( this ) )
// 			_wetnessVolumeCylinderList.Add( this );

// 		if ( _wetnessVolumeCylinderBuffer == null )
// 		{
// 			_wetnessVolumeCylinderBuffer = new GpuBuffer<WetnessVolumeCylinderBuffer>( 1 );
// 			_currentWetnessVolumeCylinderBuffer = 1;
// 		}
// 		UpdateWetnessVolumeCylinderBuffer();
// 	}

// 	public void DisableWetnessVolumeCylinderBuffer()
// 	{
// 		_wetnessVolumeCylinderList.Remove( this );
// 		UpdateWetnessVolumeCylinderBuffer();
// 	}

// 	public static void UpdateWetnessVolumeCylinderBuffer()
// 	{
// 		if ( _wetnessVolumeCylinderList.Count == 0 ) return;

// 		var volumeBufferList = new List<WetnessVolumeCylinderBuffer>();

// 		foreach ( var wetnessVolumeCylinder in _wetnessVolumeCylinderList )
// 		{
// 			if ( !wetnessVolumeCylinder.IsValid() ) continue;

// 			float radiusInches = UnitConversion.CentimetersToInches( wetnessVolumeCylinder.WetnessVolumeCylinderRadius );
// 			float depthMinimumInches = UnitConversion.CentimetersToInches( wetnessVolumeCylinder.WetnessVolumeCylinderDepthMinimum );
// 			float depthMaximumInches = UnitConversion.CentimetersToInches( wetnessVolumeCylinder.WetnessVolumeCylinderDepthMaximum );

// 			// The shader currently treats Size as an AABB — encode the cylinder bounds as such.
// 			Vector3 localCenter = new Vector3( 0, 0, ( depthMaximumInches - depthMinimumInches ) * 0.5f );
// 			Vector3 worldCenter = wetnessVolumeCylinder.WorldPosition + localCenter;
// 			Vector3 size = new Vector3( radiusInches * 2f, radiusInches * 2f, depthMaximumInches + depthMinimumInches );

// 			volumeBufferList.Add( new WetnessVolumeCylinderBuffer
// 			{
// 				VolumeCenter = worldCenter,
// 				VolumeSize = size,
// 				WetnessVolumeExtend = UnitConversion.CentimetersToInches( wetnessVolumeCylinder.WetnessVolumeExtend ),
// 				WetnessVolumeFade = wetnessVolumeCylinder.WetnessVolumeFade,
// 				WetnessVolumeSoftness = wetnessVolumeCylinder.WetnessVolumeSoftness,
// 			} );
// 		}

// 		if ( _wetnessVolumeCylinderBuffer == null || volumeBufferList.Count > _currentWetnessVolumeCylinderBuffer )
// 		{
// 			_wetnessVolumeCylinderBuffer = new GpuBuffer<WetnessVolumeCylinderBuffer>( volumeBufferList.Count );
// 			_currentWetnessVolumeCylinderBuffer = volumeBufferList.Count;
// 		}

// 		if ( volumeBufferList.Count > 0 )
// 		{
// 			_wetnessVolumeCylinderBuffer.SetData( volumeBufferList );
// 		}

// 		if ( Game.ActiveScene?.SceneWorld == null ) return;

// 		var sceneObjects = Game.ActiveScene.SceneWorld.SceneObjects
// 			.Where( x => x != null && x.IsValid() )
// 			.ToList();

// 		foreach ( var sceneObject in sceneObjects )
// 		{
// 			if ( sceneObject.IsValid() )
// 			{
// 				sceneObject.Attributes.Set( "WetnessWetnessVolumeCylinderBuffer", _wetnessVolumeCylinderBuffer );
// 				sceneObject.Attributes.Set( "MaximumWetnessWetnessVolumeCylinders", volumeBufferList.Count );
// 			}
// 		}
// 	}





// }
