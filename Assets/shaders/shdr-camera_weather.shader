
HEADER
{
	Description = "";
}

FEATURES
{
	#include "common/features.hlsl"

}

MODES
{
	Forward();
	Depth();
	ToolsShadingComplexity( "tools_shading_complexity.shader" );
}

COMMON
{
	#ifndef S_ALPHA_TEST
	#define S_ALPHA_TEST 0
	#endif
	#ifndef S_TRANSLUCENT
	#define S_TRANSLUCENT 1
	#endif
	
	#include "common/shared.hlsl"
	#include "procedural.hlsl"

	#define S_UV2 1
}

struct VertexInput
{
	#include "common/vertexinput.hlsl"
	float4 vColor : COLOR0 < Semantic( Color ); >;

};

struct PixelInput
{
	#include "common/pixelinput.hlsl"
	float3 vPositionOs : TEXCOORD14;
	float3 vNormalOs : TEXCOORD15;
	float4 vTangentUOs_flTangentVSign : TANGENT	< Semantic( TangentU_SignV ); >;
	float4 vColor : COLOR0;
	float4 vTintColor : COLOR1;
	#if ( PROGRAM == VFX_PROGRAM_PS )
		bool vFrontFacing : SV_IsFrontFace;
	#endif

};

VS
{
	#include "common/vertex.hlsl"

	PixelInput MainVs( VertexInput v )
	{

		PixelInput i;
		i.vPositionPs = float4(v.vPositionOs.xy, 0.0f, 1.0f );
		i.vPositionWs = float3(v.vTexCoord, 0.0f);
					
		return i;
	}
}

PS
{
	#include "common/pixel.hlsl"
	#include "postprocess/functions.hlsl"
	#include "postprocess/common.hlsl"

	RenderState( CullMode, F_RENDER_BACKFACES ? NONE : DEFAULT );
		
	SamplerState g_sSampler0 < Filter( ANISO ); AddressU( WRAP ); AddressV( WRAP ); >;
	SamplerState g_sSampler1 < Filter( ANISO ); AddressU( CLAMP ); AddressV( CLAMP ); >;
	CreateInputTexture2D( RainDrops, Linear, 8, "None", "_color", "Weather,0/Weather - Rain Drops,0/0", DefaultFile( "textures/tex-rain-drops@1-1-1-r-12x12-normal,height,temporal.png" ) );
	CreateInputTexture2D( RainDripsTexture, Linear, 8, "None", "_color", ",0/,0/0", DefaultFile( "textures/tex-rain-drips@1-1-1-r-12x12-normal,height,temporal.png" ) );
	Texture2D g_tRainDrops < Channel( RGBA, Box( RainDrops ), Linear ); OutputFormat( DXT5 ); SrgbRead( False ); >;
	Texture2D g_tRainDripsTexture < Channel( RGBA, Box( RainDripsTexture ), Linear ); OutputFormat( DXT5 ); SrgbRead( False ); >;
	Texture2D g_tWaterLineTexture < Attribute( "WaterLineTexture" ); >;
	Texture2D g_tColorBuffer < Attribute( "ColorBuffer" ); >;
	float g_flRainDropsScale < UiGroup( "Weather,0/Weather - Rain Drops,0/0" ); Default1( 2.5 ); Range1( 0, 8 ); >;
	float g_flRainDropsSpeed < UiGroup( "Weather,0/Weather - Rain Drops,0/6" ); Default1( 0.5 ); Range1( 0, 9 ); >;
	float g_flRainDropsDelay < Attribute( "Rain Drops Delay" ); Default1( 0.25 ); >;
	float g_flRainDropsStrength < UiGroup( "Weather,0/Weather - Rain Drops,0/5" ); Default1( 1 ); Range1( 0, 8 ); >;
	float g_flRainDropsFade < Attribute( "Rain Drops Fade" ); Default1( 0 ); >;
	float g_flRainDripsFade < Attribute( "Water Drips Fade" ); Default1( 1 ); >;
	float g_flRainDripsSpeed < Attribute( "Rain Drips Speed" ); Default1( 1 ); >;
	float g_flRainDripsStrength < Attribute( "Rain Drips Strength" ); Default1( 1 ); >;
	float g_flIsCameraSubmerged < Attribute( "Is Camera Submerged" ); Default1( 0 ); >;
	float g_flIsFullySubmerged < Attribute( "Is Fully Submerged" ); Default1( 0 ); >;
	float g_flUnderWaterBlur < Attribute( "Under Water Blur" ); Default1( 0.25 ); >;
	float g_flUnderWaterBlurDirections < UiGroup( "Under Water,0/Blur - Settings,0/0" ); Default1( 16 ); Range1( 0, 32 ); >;
	float g_flUnderWaterBlurQuality < UiGroup( "Under Water,0/Blur - Settings,0/0" ); Default1( 4 ); Range1( 0, 16 ); >;
	float g_flUnderWaterBlurTaps < UiGroup( "Under Water,0/Blur - Settings,0/0" ); Default1( 1 ); Range1( 0, 4 ); >;
	float4 g_vUnderWaterColor < Attribute( "Under Water Color" ); Default4( 0.00, 0.28, 1.00, 1.00 ); >;
	float4 g_vWaterLineColor < Attribute( "Water Line Color" ); Default4( 0.00, 0.01, 0.00, 1.00 ); >;
	float g_flWaterLineSize < Attribute( "Water Line Size" ); Default1( 0.1 ); >;
	float g_flWaterLineBlur < Attribute( "Water Line Blur" ); Default1( 0.125 ); >;
	float g_flWaterLineBlurDirections < UiGroup( "Water Line,0/Blur - Settings,0/0" ); Default1( 16 ); Range1( 0, 32 ); >;
	float g_flWaterLineBlurQuality < UiGroup( "Water Line,0/Blur - Settings,0/0" ); Default1( 4 ); Range1( 0, 16 ); >;
	float g_flWaterLineBlurTaps < UiGroup( "Water Line,0/Blur - Settings,0/0" ); Default1( 1 ); Range1( 0, 4 ); >;
		
			float SGEClamp( float input, float minimum, float maximum )
			{
				return clamp(input, minimum, maximum);
			}
			
	float3 ReorientedNormalBlendVector( float3 a, float3 b )
	{
		float3 t = a.xyz + float3( 0.0, 0.0, 1.0 );
		float3 u = b.xyz * float3( -1.0, -1.0, 1.0 );
		return ( t / t.z ) * dot( t, u ) - u;
	}
	
			float3 SGEBlur( float2 coordinates, Texture2D texture, SamplerState sampler, float value, float directions, float quality, float taps )
			{
				float twoPi = 6.28318530718f;
	
				float3 color = texture.Sample( sampler, coordinates ).rgb;
	
				[unroll]
				for( float d=0.0; d<twoPi; d+=twoPi/directions)
				{
					[unroll]
					for(float j=1.0/quality; j<=1.0; j+=1.0/quality)
					{
						taps += 1;
						color += texture.Sample( sampler, coordinates + float2( cos(d), sin(d) ) * value * 0.1f * j ).rgb;
					}
				}
				return color / taps;
			}
			

	float4 MainPs( PixelInput i ) : SV_Target0
	{
		Material m = Material::Init( i );
		m.Albedo = float3( 1, 1, 1 );
		m.Opacity = 1;
		
		float2 l_0 = CalculateViewportUv( i.vPositionSs.xy );
		float4 l_1 = float4( 0, 0, 1, 1 );
		float2 l_2 = g_vViewportSize;
		float l_3 = l_2.x;
		float l_4 = l_2.y;
		float l_5 = l_3 / l_4;
		float l_6 = l_4 / l_4;
		float2 l_7 = float2( l_5, l_6);
		float2 l_8 = TileAndOffsetUv( l_0, l_7, float2( 0, 0 ) );
		float l_9 = g_flRainDropsScale;
		float2 l_10 = l_8 * float2( l_9, l_9 );
		float4 l_11 = Tex2DS( g_tRainDrops, g_sSampler0, l_10 );
		float l_12 = l_11.x;
		float l_13 = l_11.y;
		float2 l_14 = float2( l_12, l_13 );
		float2 l_15 = l_14 * float2( 2, 2 );
		float2 l_16 = l_15 - float2( 1, 1 );
		float2 l_17 = l_16 * l_16;
		float l_18 = l_17.x;
		float l_19 = 1 - l_18;
		float l_20 = l_17.y;
		float l_21 = l_19 - l_20;
		float l_22 = sqrt( l_21 );
		float3 l_23 = float3( l_16, l_22 );
		float l_24 = l_11.z;
		float l_25 = l_11.w;
		float l_26 = 1 - l_25;
		float l_27 = g_flRainDropsSpeed;
		float l_28 = g_flTime * l_27;
		float l_29 = g_flRainDropsDelay;
		float l_30 = saturate( ( l_29 - 0 ) / ( 1 - 0 ) ) * ( 0.001 - 1 ) + 1;
		float l_31 = l_28 * l_30;
		float l_32 = l_26 - l_31;
		float l_33 = frac( l_32 );
		float l_34 = 1 - l_30;
		float l_35 = l_33 - l_34;
		float l_36 = 1 / l_30;
		float l_37 = l_35 * l_36;
		float l_38 = saturate( l_37 );
		float l_39 = l_24 * l_38;
		float4 l_40 = lerp( l_1, float4( l_23, 0 ), l_39 );
		float l_41 = l_40.x;
		float l_42 = l_40.y;
		float2 l_43 = float2( l_41, l_42 );
		float l_44 = g_flRainDropsStrength;
		float l_45 = g_flRainDropsFade;
		float l_46 = l_44 * l_45;
		float2 l_47 = l_43 * float2( l_46, l_46 );
		float l_48 = l_40.z;
		float3 l_49 = float3( l_47, l_48 );
		float4 l_50 = float4( 0, 0, 1, 1 );
		float2 l_51 = l_8 * float2( 1, 1 );
		float4 l_52 = Tex2DS( g_tRainDripsTexture, g_sSampler0, l_51 );
		float l_53 = l_52.x;
		float l_54 = l_52.y;
		float2 l_55 = float2( l_53, l_54 );
		float2 l_56 = l_55 * float2( 2, 2 );
		float2 l_57 = l_56 - float2( 1, 1 );
		float2 l_58 = l_57 * l_57;
		float l_59 = l_58.x;
		float l_60 = 1 - l_59;
		float l_61 = l_58.y;
		float l_62 = l_60 - l_61;
		float l_63 = sqrt( l_62 );
		float3 l_64 = float3( l_57, l_63 );
		float l_65 = l_51.y;
		float l_66 = g_flRainDripsFade;
		float l_67 = g_flRainDripsSpeed;
		float l_68 = l_66 * l_67;
		float l_69 = l_68 - 0.1;
		float l_70 = l_65 + l_69;
		float l_71 = l_52.w;
		float l_72 = l_70 + l_71;
		float l_73 = l_72.x;
		float l_74 = SGEClamp( l_73, 0, 1 );
		float l_75 = 0.0f;
		float2 l_76 = float2( l_74, l_75);
		float2 l_77 = frac( l_76 );
		float2 l_78 = saturate( ( l_77 - float2( 0.95, 0.95 ) ) / ( float2( 0.775, 0.775 ) - float2( 0.95, 0.95 ) ) ) * ( float2( 1, 1 ) - float2( 0, 0 ) ) + float2( 0, 0 );
		float l_79 = lerp( 1, 0, l_78.x );
		float2 l_80 = saturate( ( l_77 - float2( 0.975, 0.975 ) ) / ( float2( 0.95, 0.95 ) - float2( 0.975, 0.975 ) ) ) * ( float2( 1, 1 ) - float2( 0, 0 ) ) + float2( 0, 0 );
		float l_81 = lerp( 0, l_79, l_80.x );
		float l_82 = l_52.z;
		float l_83 = l_81 * l_82;
		float4 l_84 = lerp( l_50, float4( l_64, 0 ), l_83 );
		float l_85 = l_84.x;
		float l_86 = l_84.y;
		float2 l_87 = float2( l_85, l_86 );
		float l_88 = g_flRainDripsStrength;
		float l_89 = l_66 * l_88;
		float2 l_90 = l_87 * float2( l_89, l_89 );
		float l_91 = l_84.z;
		float3 l_92 = float3( l_90, l_91 );
		float3 l_93 = ReorientedNormalBlendVector( l_49, l_92 );
		float3 l_94 = float3( 0, 0, 1 );
		float2 l_95 = CalculateViewportUv( i.vPositionSs.xy );
		float4 l_96 = Tex2DS( g_tWaterLineTexture, g_sSampler1, l_95 );
		float l_97 = l_96.x;
		float l_98 = saturate( l_97 );
		float l_99 = g_flIsCameraSubmerged;
		float l_100 = lerp( 0, l_98, l_99 );
		float l_101 = g_flIsFullySubmerged;
		float l_102 = lerp( l_100, 1, l_101 );
		float3 l_103 = lerp( l_93, l_94, l_102 );
		float3 l_104 = float3( l_0, 0 ) + l_103;
		float4 l_105 = Tex2DS( g_tColorBuffer, g_sSampler1, l_104.xy );
		float l_106 = g_flUnderWaterBlur;
		float l_107 = g_flUnderWaterBlurDirections;
		float l_108 = g_flUnderWaterBlurQuality;
		float l_109 = g_flUnderWaterBlurTaps;
		float3 l_110 = SGEBlur( l_104, g_tColorBuffer, g_sSampler1, l_106, l_107, l_108, l_109 );
		float4 l_111 = g_vUnderWaterColor;
		float4 l_112 = float4( l_110, 0 ) * l_111;
		float4 l_113 = lerp( l_105, l_112, l_102 );
		float4 l_114 = g_vWaterLineColor;
		float l_115 = g_flWaterLineSize;
		float l_116 = l_115 * 0.1;
		float2 l_117 = float2( 0, l_116 );
		float2 l_118 = TileAndOffsetUv( l_95, float2( 1, 1 ), l_117 );
		float l_119 = g_flWaterLineBlur;
		float l_120 = g_flWaterLineBlurDirections;
		float l_121 = g_flWaterLineBlurQuality;
		float l_122 = g_flWaterLineBlurTaps;
		float3 l_123 = SGEBlur( l_118, g_tWaterLineTexture, g_sSampler1, l_119, l_120, l_121, l_122 );
		float l_124 = l_123.x;
		float l_125 = l_124 - l_97;
		float2 l_126 = l_117 * float2( -1, -1 );
		float2 l_127 = TileAndOffsetUv( l_95, float2( 1, 1 ), l_126 );
		float3 l_128 = SGEBlur( l_127, g_tWaterLineTexture, g_sSampler1, l_119, l_120, l_121, l_122 );
		float l_129 = l_128.x;
		float l_130 = 1 - l_129;
		float l_131 = 1 - l_97;
		float l_132 = l_130 - l_131;
		float l_133 = l_125 + l_132;
		float l_134 = saturate( l_133 );
		float l_135 = lerp( 0, l_134, l_99 );
		float l_136 = saturate( ( l_135 - 0 ) / ( 1 - 0 ) ) * ( 1 - 0 ) + 0;
		float4 l_137 = lerp( l_113, l_114, l_136 );
		
		m.Albedo = l_137.xyz;
		m.Opacity = 1;
		m.Roughness = 1;
		m.Metalness = 0;
		m.AmbientOcclusion = 1;
		
		m.Opacity = saturate( m.Opacity );
		
		return float4( m.Albedo, m.Opacity );

	}
}
