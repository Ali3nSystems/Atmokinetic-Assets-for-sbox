
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
	DynamicCombo( D_TRANSLUCENT, 0..1, Sys( ALL ) );
	#ifndef S_TRANSLUCENT
	#define S_TRANSLUCENT D_TRANSLUCENT
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
	DynamicCombo( D_Enable_Water_Surface_Blur, 0..1, Sys( ALL ) );
	DynamicCombo( D_Water_Reflection_Mode, 0..1, Sys( ALL ) );
	DynamicCombo( D_Enable_Water_Reflection_Blur, 0..1, Sys( ALL ) );
	DynamicCombo( D_Enable_Water_Line, 0..1, Sys( ALL ) );
		
	SamplerState g_sSampler0 < Filter( ANISO ); AddressU( WRAP ); AddressV( WRAP ); >;
	Texture2D g_tWaterRipplesTextureA < Attribute( "WaterRipplesTextureA" ); >;
	Texture2D g_tWaterRipplesTextureB < Attribute( "WaterRipplesTextureB" ); >;
	Texture2D g_tWaterSimulationTexture < Attribute( "WaterSimulationTexture" ); >;
	float g_flWaterRipplesScaleA < Attribute( "Water Ripples Scale A" ); Default1( 0.01 ); >;
	float g_flWaterRipplesSpeedA < Attribute( "Water Ripples Speed A" ); Default1( 0.1 ); >;
	float g_flWaterRipplesRotationA < Attribute( "Water Ripples Rotation A" ); Default1( 0 ); >;
	float g_flWaterRipplesDisplacementA < Attribute( "Water Ripples Displacement A" ); Default1( 1 ); >;
	float g_flWaterRipplesScaleB < Attribute( "Water Ripples Scale B" ); Default1( 0.01 ); >;
	float g_flWaterRipplesSpeedB < Attribute( "Water Ripples Speed B" ); Default1( 0.1 ); >;
	float g_flWaterRipplesRotationB < Attribute( "Water Ripples Rotation B" ); Default1( 180 ); >;
	float g_flWaterRipplesDisplacementB < Attribute( "Water Ripples Displacement B" ); Default1( 1 ); >;
	float g_flWaterSimulationDisplacement < Attribute( "Water Simulation Displacement" ); Default1( 1 ); >;
		
			float3 SGERotateAboutAxisDegrees( float3 coordinates, float3 axis, float rotation )
			{
				rotation *= 3.1415926f/180.0f;
				float s = sin(rotation);
				float c = cos(rotation);
				float inverted_c = 1.0 - c;
	
				axis = normalize(axis);
				float3x3 rotationMatrix =
				{   inverted_c * axis.x * axis.x + c, inverted_c * axis.x * axis.y - axis.z * s, inverted_c * axis.z * axis.x + axis.y * s,
					inverted_c * axis.x * axis.y + axis.z * s, inverted_c * axis.y * axis.y + c, inverted_c * axis.y * axis.z - axis.x * s,
					inverted_c * axis.z * axis.x - axis.y * s, inverted_c * axis.y * axis.z + axis.x * s, inverted_c * axis.z * axis.z + c
				};
				return float3(mul(rotationMatrix, coordinates));
			}
			
			float3 ReorientedNormalBlend(float3 a, float3 b)
			{
				float3 t = a.xyz + float3(0.0, 0.0, 1.0);
				float3 u = b.xyz * float3(-1.0, -1.0, 1.0);
				return (t / t.z) * dot(t, u) - u;
			}
	
			struct WaterSimulation
			{
				float3 Position;
				float Radius;
				float Alpha;
				float Rotation;
				float Progress;
			};
	
			StructuredBuffer<WaterSimulation> WaterSimulationBuffer < Attribute( "WaterSimulationBuffer" ); >;
			int g_iMaxRipples < Attribute( "MaxRipples" ); Default( 0 ); >;
	
			float4 WaterSimulationFunction(float3 coordinates, Texture2D texture, SamplerState sampler, int maxRipples)
			{
				float3 blendedNormal = float3(0, 0, 1);
				float totalDisplacement = 0.0;        
	
				[loop]
				for(int i = 0; i < maxRipples; i++)
				{
					WaterSimulation ripple = WaterSimulationBuffer[i];
	
					// Skip inactive ripples
					if(ripple.Radius <= 0.01)
						continue;
	
					if(ripple.Alpha <= 0.001)
						continue;
	
					// Calculate offset from ripple center (only XY for water surface)
					float2 toRipple = coordinates.xy - ripple.Position.xy;
					float dist = length(toRipple);
	
					// Skip if outside radius
					if(dist > ripple.Radius)
						continue;
	
					// Normalized offset for UV calculation
					float2 normalizedOffset = toRipple / ripple.Radius;
					float2 rippleUV = normalizedOffset * 0.5 + 0.5;
	
					// Rotate UV around center (0.5, 0.5)
					float2 centeredUV = rippleUV - 0.5;
					float sinR = sin(ripple.Rotation);
					float cosR = cos(ripple.Rotation);
					rippleUV = float2(
						centeredUV.x * cosR - centeredUV.y * sinR,
						centeredUV.x * sinR + centeredUV.y * cosR
					) + 0.5;
	
					// Sample normal map and alpha (displacement) - use SampleLevel for vertex shader compatibility
					float4 rippleSample = texture.SampleLevel(sampler, rippleUV, 0);
	
					float3 rippleNormal = rippleSample.xyz * 2 - 1;
	
					// Remap displacement based on progress: (value - progress) / (1 - progress)
					// This makes the ring thin out as progress increases (0->1)
					float invProgress = max(1.0 - ripple.Progress, 0.001);
					float rippleDisplacement = saturate((rippleSample.a - ripple.Progress) / invProgress);
	
					// Blend normal
					float3 flatNormal = float3(0.0, 0.0, 1.0);
					rippleNormal = lerp(flatNormal, rippleNormal, ripple.Alpha);
					blendedNormal = ReorientedNormalBlend(blendedNormal, rippleNormal);
	
					// Accumulate displacement (additive)
					totalDisplacement += rippleDisplacement;
				}
	
				return float4(blendedNormal, totalDisplacement);
			}
			
	PixelInput MainVs( VertexInput v )
	{
		PixelInput i = ProcessVertex( v );
		i.vPositionOs = v.vPositionOs.xyz;
		i.vColor = v.vColor;

		ExtraShaderData_t extraShaderData = GetExtraPerInstanceShaderData( v.nInstanceTransformID );
		i.vTintColor = extraShaderData.vTint;

		VS_DecodeObjectSpaceNormalAndTangent( v, i.vNormalOs, i.vTangentUOs_flTangentVSign );
		
		float3 l_0 = i.vPositionWs;
		float l_1 = g_flWaterRipplesScaleA;
		float3 l_2 = l_0 * float3( l_1, l_1, l_1 );
		float l_3 = g_flWaterRipplesSpeedA;
		float l_4 = g_flTime * l_3;
		float2 l_5 = float2( l_4, 0 );
		float2 l_6 = TileAndOffsetUv( l_2.xy, float2( 1, 1 ), l_5 );
		float l_7 = g_flWaterRipplesRotationA;
		float3 l_8 = SGERotateAboutAxisDegrees( float3( l_6, 0 ), float3( 0, 0, 1 ), l_7 );
		float4 l_9 = g_tWaterRipplesTextureA.SampleLevel( g_sSampler0, l_8.xy, 0 );
		float l_10 = l_9.w;
		float l_11 = l_10 - 1;
		float l_12 = g_flWaterRipplesDisplacementA;
		float l_13 = l_11 * l_12;
		float l_14 = g_flWaterRipplesScaleB;
		float3 l_15 = l_0 * float3( l_14, l_14, l_14 );
		float l_16 = g_flWaterRipplesSpeedB;
		float l_17 = g_flTime * l_16;
		float2 l_18 = float2( l_17, 0 );
		float2 l_19 = TileAndOffsetUv( l_15.xy, float2( 1, 1 ), l_18 );
		float l_20 = g_flWaterRipplesRotationB;
		float3 l_21 = SGERotateAboutAxisDegrees( float3( l_19, 0 ), float3( 0, 0, 1 ), l_20 );
		float4 l_22 = g_tWaterRipplesTextureB.SampleLevel( g_sSampler0, l_21.xy, 0 );
		float l_23 = l_22.w;
		float l_24 = l_23 - 1;
		float l_25 = g_flWaterRipplesDisplacementB;
		float l_26 = l_24 * l_25;
		float l_27 = l_13 + l_26;
		float3 l_28 = i.vPositionWs;
		float4 l_29 = WaterSimulationFunction( l_28, g_tWaterSimulationTexture, g_sSampler0, g_iMaxRipples );
		float l_30 = l_29.w;
		float l_31 = g_flWaterSimulationDisplacement;
		float l_32 = l_30 * l_31;
		float l_33 = l_27 + l_32;
		float3 l_34 = float3( 0, 0, 1 );
		float3 l_35 = float3( l_33, l_33, l_33 ) * l_34;
		i.vPositionWs.xyz += l_35;
		i.vPositionPs.xyzw = Position3WsToPs( i.vPositionWs.xyz );
		
		return FinalizeVertex( i );
	}
}

PS
{
	#include "common/pixel.hlsl"
	#include "common/classes/EnvMap.hlsl"
	DynamicCombo( D_Enable_Water_Surface_Blur, 0..1, Sys( ALL ) );
	DynamicCombo( D_Water_Reflection_Mode, 0..1, Sys( ALL ) );
	DynamicCombo( D_Enable_Water_Reflection_Blur, 0..1, Sys( ALL ) );
	DynamicCombo( D_Enable_Water_Line, 0..1, Sys( ALL ) );
	RenderState( CullMode, F_RENDER_BACKFACES ? NONE : DEFAULT );
	
	BoolAttribute( bWantsFBCopyTexture, true );
	
	SamplerState g_sSampler0 < Filter( ANISO ); AddressU( WRAP ); AddressV( WRAP ); >;
	SamplerState g_sSampler1 < Filter( ANISO ); AddressU( CLAMP ); AddressV( CLAMP ); >;
	Texture2D g_tWaterRipplesTextureA < Attribute( "WaterRipplesTextureA" ); >;
	Texture2D g_tWaterRipplesTextureB < Attribute( "WaterRipplesTextureB" ); >;
	Texture2D g_tWaterSimulationTexture < Attribute( "WaterSimulationTexture" ); >;
	Texture2D g_tFrameBufferCopyTexture < Attribute( "FrameBufferCopyTexture" ); >;
	Texture2D g_tReflectionTexture < Attribute( "ReflectionTexture" ); >;
	float g_flWaterRipplesScaleA < Attribute( "Water Ripples Scale A" ); Default1( 0.01 ); >;
	float g_flWaterRipplesSpeedA < Attribute( "Water Ripples Speed A" ); Default1( 0.1 ); >;
	float g_flWaterRipplesRotationA < Attribute( "Water Ripples Rotation A" ); Default1( 0 ); >;
	float g_flWaterRipplesRefractionA < Attribute( "Water Ripples Refraction A" ); Default1( 0.1 ); >;
	float g_flWaterRipplesScaleB < Attribute( "Water Ripples Scale B" ); Default1( 0.01 ); >;
	float g_flWaterRipplesSpeedB < Attribute( "Water Ripples Speed B" ); Default1( 0.1 ); >;
	float g_flWaterRipplesRotationB < Attribute( "Water Ripples Rotation B" ); Default1( 180 ); >;
	float g_flWaterRipplesRefractionB < Attribute( "Water Ripples Refraction B" ); Default1( 0.1 ); >;
	float g_flWaterSimulationRefraction < Attribute( "Water Simulation Refraction" ); Default1( 0.025 ); >;
	float g_flWaterTensionRefraction < Attribute( "Water Tension Refraction" ); Default1( 10 ); >;
	float g_flWaterTensionDistance < Attribute( "Water Tension Distance" ); Default1( 2 ); >;
	float g_flWaterSurfaceBlur < Attribute( "Water Surface Blur" ); Default1( 0 ); >;
	float4 g_vWaterSurfaceColor < Attribute( "Water Surface Color" ); Default4( 0.25, 0.50, 1.00, 1.00 ); >;
	float g_flWaterReflectionBlur < Attribute( "Water Reflection Blur" ); Default1( 0 ); >;
	float4 g_vWaterReflectionColor < Attribute( "Water Reflection Color" ); Default4( 0.25, 0.50, 1.00, 1.00 ); >;
	float g_flWaterReflectionDistance < Attribute( "Water Reflection Distance" ); Default1( 4 ); >;
	float2 g_vWaterReflectionThreshold < Attribute( "Water Reflection Threshold" ); Default2( 0,0.5 ); >;
	float g_flWaterShorelineFade < Attribute( "Water Shoreline Fade" ); Default1( 4 ); >;
		
			float3 SGERotateAboutAxisDegrees( float3 coordinates, float3 axis, float rotation )
			{
				rotation *= 3.1415926f/180.0f;
				float s = sin(rotation);
				float c = cos(rotation);
				float inverted_c = 1.0 - c;
	
				axis = normalize(axis);
				float3x3 rotationMatrix =
				{   inverted_c * axis.x * axis.x + c, inverted_c * axis.x * axis.y - axis.z * s, inverted_c * axis.z * axis.x + axis.y * s,
					inverted_c * axis.x * axis.y + axis.z * s, inverted_c * axis.y * axis.y + c, inverted_c * axis.y * axis.z - axis.x * s,
					inverted_c * axis.z * axis.x - axis.y * s, inverted_c * axis.y * axis.z + axis.x * s, inverted_c * axis.z * axis.z + c
				};
				return float3(mul(rotationMatrix, coordinates));
			}
								
			float3 SGENormalBlendReoriented(float3 inputA, float3 inputB)
			{
				float3 t = inputA.rgb + float3(0.0, 0.0, 1.0);
				float3 u = inputB.rgb * float3(-1.0, -1.0, 1.0);
				return float3((t / t.b) * dot(t, u) - u);
			}
			
			float3 ReorientedNormalBlend(float3 a, float3 b)
			{
				float3 t = a.xyz + float3(0.0, 0.0, 1.0);
				float3 u = b.xyz * float3(-1.0, -1.0, 1.0);
				return (t / t.z) * dot(t, u) - u;
			}
	
			struct WaterSimulation
			{
				float3 Position;
				float Radius;
				float Alpha;
				float Rotation;
				float Progress;
			};
	
			StructuredBuffer<WaterSimulation> WaterSimulationBuffer < Attribute( "WaterSimulationBuffer" ); >;
			int g_iMaxRipples < Attribute( "MaxRipples" ); Default( 0 ); >;
	
			float4 WaterSimulationFunction(float3 coordinates, Texture2D texture, SamplerState sampler, int maxRipples)
			{
				float3 blendedNormal = float3(0, 0, 1);
				float totalDisplacement = 0.0;        
	
				[loop]
				for(int i = 0; i < maxRipples; i++)
				{
					WaterSimulation ripple = WaterSimulationBuffer[i];
	
					// Skip inactive ripples
					if(ripple.Radius <= 0.01)
						continue;
	
					if(ripple.Alpha <= 0.001)
						continue;
	
					// Calculate offset from ripple center (only XY for water surface)
					float2 toRipple = coordinates.xy - ripple.Position.xy;
					float dist = length(toRipple);
	
					// Skip if outside radius
					if(dist > ripple.Radius)
						continue;
	
					// Normalized offset for UV calculation
					float2 normalizedOffset = toRipple / ripple.Radius;
					float2 rippleUV = normalizedOffset * 0.5 + 0.5;
	
					// Rotate UV around center (0.5, 0.5)
					float2 centeredUV = rippleUV - 0.5;
					float sinR = sin(ripple.Rotation);
					float cosR = cos(ripple.Rotation);
					rippleUV = float2(
						centeredUV.x * cosR - centeredUV.y * sinR,
						centeredUV.x * sinR + centeredUV.y * cosR
					) + 0.5;
	
					// Sample normal map and alpha (displacement) - use SampleLevel for vertex shader compatibility
					float4 rippleSample = texture.SampleLevel(sampler, rippleUV, 0);
	
					float3 rippleNormal = rippleSample.xyz * 2 - 1;
	
					// Remap displacement based on progress: (value - progress) / (1 - progress)
					// This makes the ring thin out as progress increases (0->1)
					float invProgress = max(1.0 - ripple.Progress, 0.001);
					float rippleDisplacement = saturate((rippleSample.a - ripple.Progress) / invProgress);
	
					// Blend normal
					float3 flatNormal = float3(0.0, 0.0, 1.0);
					rippleNormal = lerp(flatNormal, rippleNormal, ripple.Alpha);
					blendedNormal = ReorientedNormalBlend(blendedNormal, rippleNormal);
	
					// Accumulate displacement (additive)
					totalDisplacement += rippleDisplacement;
				}
	
				return float4(blendedNormal, totalDisplacement);
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
			
	
			float3 SGECubemap( float3 coordinates, float4 screenPosition, float3 worldNormal, float roughness )
			{
				return EnvMap::From( coordinates, screenPosition, normalize( worldNormal ), roughness );
			}
		
	
	float4 MainPs( PixelInput i ) : SV_Target0
	{

		Material m = Material::Init( i );
		m.Albedo = float3( 1, 1, 1 );
		m.Normal = float3( 0, 0, 1 );
		m.Roughness = 1;
		m.Metalness = 0;
		m.AmbientOcclusion = 1;
		m.TintMask = 1;
		m.Opacity = 1;
		m.Emission = float3( 0, 0, 0 );
		m.Transmission = 0;
		
		float2 l_0 = CalculateViewportUv( i.vPositionSs.xy );
		float3 l_1 = i.vPositionWithOffsetWs.xyz + g_vHighPrecisionLightingOffsetWs.xyz;
		float l_2 = g_flWaterRipplesScaleA;
		float3 l_3 = l_1 * float3( l_2, l_2, l_2 );
		float l_4 = g_flWaterRipplesSpeedA;
		float l_5 = g_flTime * l_4;
		float2 l_6 = float2( l_5, 0 );
		float2 l_7 = TileAndOffsetUv( l_3.xy, float2( 1, 1 ), l_6 );
		float l_8 = g_flWaterRipplesRotationA;
		float3 l_9 = SGERotateAboutAxisDegrees( float3( l_7, 0 ), float3( 0, 0, 1 ), l_8 );
		float4 l_10 = Tex2DS( g_tWaterRipplesTextureA, g_sSampler0, l_9.xy );
		float3 l_11 = l_10.xyz;
		float3 l_12 = l_11 * float3( 2, 2, 2 );
		float3 l_13 = l_12 - float3( 1, 1, 1 );
		float l_14 = l_13.x;
		float l_15 = l_13.y;
		float2 l_16 = float2( l_14, l_15 );
		float l_17 = g_flWaterRipplesRefractionA;
		float2 l_18 = l_16 * float2( l_17, l_17 );
		float l_19 = l_13.z;
		float3 l_20 = float3( l_18, l_19 );
		float l_21 = g_flWaterRipplesScaleB;
		float3 l_22 = l_1 * float3( l_21, l_21, l_21 );
		float l_23 = g_flWaterRipplesSpeedB;
		float l_24 = g_flTime * l_23;
		float2 l_25 = float2( l_24, 0 );
		float2 l_26 = TileAndOffsetUv( l_22.xy, float2( 1, 1 ), l_25 );
		float l_27 = g_flWaterRipplesRotationB;
		float3 l_28 = SGERotateAboutAxisDegrees( float3( l_26, 0 ), float3( 0, 0, 1 ), l_27 );
		float4 l_29 = Tex2DS( g_tWaterRipplesTextureB, g_sSampler0, l_28.xy );
		float3 l_30 = l_29.xyz;
		float3 l_31 = l_30 * float3( 2, 2, 2 );
		float3 l_32 = l_31 - float3( 1, 1, 1 );
		float l_33 = l_32.x;
		float l_34 = l_32.y;
		float2 l_35 = float2( l_33, l_34 );
		float l_36 = g_flWaterRipplesRefractionB;
		float2 l_37 = l_35 * float2( l_36, l_36 );
		float l_38 = l_32.z;
		float3 l_39 = float3( l_37, l_38 );
		float3 l_40 = SGENormalBlendReoriented( l_20, l_39 );
		float3 l_41 = i.vPositionWithOffsetWs.xyz + g_vHighPrecisionLightingOffsetWs.xyz;
		float4 l_42 = WaterSimulationFunction( l_41, g_tWaterSimulationTexture, g_sSampler0, g_iMaxRipples );
		float3 l_43 = l_42.xyz;
		float l_44 = l_43.x;
		float l_45 = l_43.y;
		float2 l_46 = float2( l_44, l_45 );
		float l_47 = g_flWaterSimulationRefraction;
		float2 l_48 = l_46 * float2( l_47, l_47 );
		float l_49 = l_43.z;
		float3 l_50 = float3( l_48, l_49 );
		float3 l_51 = SGENormalBlendReoriented( l_40, l_50 );
		float3 l_52 = float3( 0, 1, 1 );
		float l_53 = l_52.x;
		float l_54 = l_52.y;
		float2 l_55 = float2( l_53, l_54 );
		float l_56 = g_flWaterTensionRefraction;
		float2 l_57 = l_55 * float2( l_56, l_56 );
		float l_58 = l_52.z;
		float3 l_59 = float3( l_57, l_58 );
		float3 l_60 = i.vPositionWithOffsetWs.xyz + g_vHighPrecisionLightingOffsetWs.xyz;
		float3 l_61 = g_vCameraPositionWs;
		float3 l_62 = l_60 - l_61;
		float3 l_63 = g_vCameraDirWs;
		float l_64 = dot( l_62, l_63 );
		float l_65 = g_flWaterTensionDistance;
		float l_66 = l_64 + l_65;
		float2 l_67 = i.vPositionSs.xy;
		float l_68 = Depth::GetLinear( l_67 );
		float l_69 = smoothstep( l_64, l_66, l_68 );
		float l_70 = 1 - l_69;
		float3 l_71 = lerp( l_51, l_59, l_70 );
		float3 l_72 = float3( l_0, 0 ) + l_71;
		float l_73 = g_flWaterSurfaceBlur;
		float3 l_74 = SGEBlur( l_72, g_tFrameBufferCopyTexture, g_sSampler1, l_73, 16, 4, 1 );
		float4 l_75 = Tex2DS( g_tFrameBufferCopyTexture, g_sSampler1, l_72.xy );
		float3 l_76 = l_75.xyz;
		float3 l_77 = (D_Enable_Water_Surface_Blur == 0 ? l_76 : l_74);
		float4 l_78 = g_vWaterSurfaceColor;
		float4 l_79 = float4( l_77, 0 ) * l_78;
		float l_80 = l_72.x;
		float l_81 = 1 - l_80;
		float l_82 = l_72.y;
		float2 l_83 = float2( l_81, l_82);
		float l_84 = g_flWaterReflectionBlur;
		float3 l_85 = SGEBlur( l_83, g_tReflectionTexture, g_sSampler1, l_84, 16, 4, 1 );
		float4 l_86 = Tex2DS( g_tReflectionTexture, g_sSampler1, l_83 );
		float3 l_87 = l_86.xyz;
		float3 l_88 = (D_Enable_Water_Reflection_Blur == 0 ? l_87 : l_85);
		float3 l_89 = i.vPositionWithOffsetWs.xyz + g_vHighPrecisionLightingOffsetWs.xyz;
		float2 l_90 = i.vPositionSs.xy;
		float l_91 = l_90.y;
		float l_92 = l_90.y;
		float l_93 = i.vPositionSs.z;
		float l_94 = i.vPositionSs.w;
		float4 l_95 = float4( l_91, l_92, l_93, l_94 );
		float3 l_96 = SGECubemap( l_89, l_95, l_71, l_84 );
		float3 l_97 = (D_Water_Reflection_Mode == 0 ? l_88 : l_96);
		float4 l_98 = g_vWaterReflectionColor;
		float4 l_99 = float4( l_97, 0 ) * l_98;
		float l_100 = g_flWaterReflectionDistance;
		float3 l_101 = CalculatePositionToCameraDirWs( i.vPositionWithOffsetWs.xyz + g_vHighPrecisionLightingOffsetWs.xyz );
		float3 l_102 = pow( 1.0 - dot( normalize( i.vNormalWs ), normalize( l_101 ) ), l_100 );
		float2 l_103 = g_vWaterReflectionThreshold;
		float3 l_104 = saturate( ( l_102 - float3( l_103.x, l_103.x, l_103.x ) ) / ( float3( l_103.y, l_103.y, l_103.y ) - float3( l_103.x, l_103.x, l_103.x ) ) ) * ( float3( 1, 1, 1 ) - float3( 0, 0, 0 ) ) + float3( 0, 0, 0 );
		float4 l_105 = lerp( l_79, l_99, float4( l_104, 0 ) );
		float4 l_106 = lerp( l_79, l_105, i.vFrontFacing );
		float l_107 = lerp( 1, 0, i.vFrontFacing );
		float l_108 = i.vNormalWs.z;
		float l_109 = 1 - l_108;
		float l_110 = l_107 + l_109;
		float l_111 = saturate( l_110 );
		float4 l_112 = float4( l_111, l_111, l_111, l_111 );
		float l_113 = (D_Enable_Water_Line == 0 ? 0 : 1);
		float4 l_114 = lerp( l_106, l_112, l_113 );
		float l_115 = i.vNormalOs.z;
		float3 l_116 = i.vPositionWithOffsetWs.xyz + g_vHighPrecisionLightingOffsetWs.xyz;
		float3 l_117 = g_vCameraPositionWs;
		float3 l_118 = l_116 - l_117;
		float3 l_119 = g_vCameraDirWs;
		float l_120 = dot( l_118, l_119 );
		float l_121 = g_flWaterShorelineFade;
		float l_122 = l_120 + l_121;
		float2 l_123 = i.vPositionSs.xy;
		float l_124 = Depth::GetLinear( l_123 );
		float l_125 = smoothstep( l_120, l_122, l_124 );
		float l_126 = 1 - l_125;
		float l_127 = 1 - l_126;
		float l_128 = saturate( ( l_127 - 0 ) / ( 1 - 0 ) ) * ( 1 - 0 ) + 0;
		float l_129 = l_115 * l_128;
		float l_130 = lerp( l_129, 1, l_113 );
		
		m.Albedo = l_114.xyz;
		m.Opacity = l_130;
		m.Roughness = 1;
		m.Metalness = 0;
		m.AmbientOcclusion = 1;
		
		m.AmbientOcclusion = saturate( m.AmbientOcclusion );
		m.Roughness = saturate( m.Roughness );
		m.Metalness = saturate( m.Metalness );
		m.Opacity = saturate( m.Opacity );

		// Result node takes normal as tangent space, convert it to world space now
		m.Normal = TransformNormal( m.Normal, i.vNormalWs, i.vTangentUWs, i.vTangentVWs );

		// Toolvis:
		m.WorldTangentU = i.vTangentUWs;
		m.WorldTangentV = i.vTangentVWs;
		m.TextureCoords = i.vTextureCoords.xy;
		
		return float4( m.Albedo, m.Opacity );
	}
}
