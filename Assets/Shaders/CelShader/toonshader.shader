HEADER
{
	Description = "Toon Shader";
	Version = 1;
}

//=========================================================================================================================

MODES
{
	VrForward();
	Depth();
	ToolsShadingComplexity( "tools_shading_complexity.shader" );
}

//=========================================================================================================================

FEATURES
{
	#include "common/features.hlsl"
	
	Feature( F_STYLIZED_HIGHLIGHT, 0..1, "Stylized Highlights" );
	Feature( F_TOON_SHADING, 0..1, "Toon Shading" );
}

//=========================================================================================================================

COMMON
{
	#include "common/shared.hlsl"
}

//=========================================================================================================================

struct VertexInput
{
	#include "common/vertexinput.hlsl"
};

//=========================================================================================================================

struct PixelInput
{
	#include "common/pixelinput.hlsl"
};

//=========================================================================================================================

VS
{
	#include "common/vertex.hlsl"

	PixelInput MainVs( VertexInput i )
	{
		PixelInput o = ProcessVertex( i );
		return FinalizeVertex( o );
	}
}

//=========================================================================================================================

PS
{
	#define CUSTOM_MATERIAL_INPUTS
	
	StaticCombo( S_STYLIZED_HIGHLIGHT, F_STYLIZED_HIGHLIGHT, Sys(ALL) );
	StaticCombo( S_TOON_SHADING, F_TOON_SHADING, Sys(ALL) );

	#include "common/pixel.hlsl"
	
	CreateInputTexture2D( ColorTexture, Srgb, 8, "", "_color", "Material,10/10", Default3( 1.0, 1.0, 1.0 ) );
	CreateInputTexture2D( NormalTexture, Linear, 8, "NormalizeNormals", "_normal", "Material,10/20", Default3( 0.5, 0.5, 1.0 ) );
	CreateInputTexture2D( RoughnessTexture, Linear, 8, "", "_rough", "Material,10/30", Default( 0.5 ) );
	
	Texture2D g_tColor < Channel( RGB, Box( ColorTexture ), Srgb ); OutputFormat( BC7 ); SrgbRead( true ); >;
	Texture2D g_tNormal < Channel( RGBA, Box( NormalTexture ), Linear ); OutputFormat( BC7 ); SrgbRead( false ); >;
	Texture2D g_tRoughness < Channel( R, Box( RoughnessTexture ), Linear ); OutputFormat( BC7 ); SrgbRead( false ); >;

	
	float4 g_vBaseColor < UiType( Color ); Default4( 0.7, 0.7, 0.75, 1.0 ); UiGroup( "Material,10/40" ); >;
	float4 g_vShadeColor < UiType( Color ); Default4( 0.5, 0.5, 0.55, 1.0 ); UiGroup( "Material,10/50" ); >;
	
	float g_flShadingSteps < UiType( Slider ); Range( 2.0, 8.0 ); Default( 3.0 ); UiGroup( "Toon Shading,30/10" ); >;
	float g_flShadingSharpness < UiType( Slider ); Range( 0.0, 1.0 ); Default( 0.9 ); UiGroup( "Toon Shading,30/20" ); >;
	float g_flShadingShift < UiType( Slider ); Range( -1.0, 1.0 ); Default( 0.0 ); UiGroup( "Toon Shading,30/30" ); >;
	
	float4 g_vHighlightColor < UiType( Color ); Default4( 1.0, 1.0, 1.0, 1.0 ); UiGroup( "Highlights,40/10" ); >;
	float g_flHighlightSize < UiType( Slider ); Range( 0.0, 0.1 ); Default( 0.005 ); UiGroup( "Highlights,40/20" ); >;
	float g_flHighlightSharpness < UiType( Slider ); Range( 1.0, 10.0 ); Default( 2.0 ); UiGroup( "Highlights,40/30" ); >;
	float g_flHighlightSquareMagnitude < UiType( Slider ); Range( 0.0, 1.0 ); Default( 0.02 ); UiGroup( "Highlights,40/40" ); >;
	float g_flHighlightScale < UiType( Slider ); Range( 0.0, 1.0 ); Default( 0.4 ); UiGroup( "Highlights,40/50" ); >;
	float g_flHighlightSplit < UiType( Slider ); Range( 0.0, 1.0 ); Default( 0.15 ); UiGroup( "Highlights,40/60" ); >;

	float ToonShade( float lightIntensity, float steps, float sharpness )
	{
		float stepped = floor( lightIntensity * steps ) / steps;
		float smoothStepped = smoothstep( stepped - ( 1.0 - sharpness ) / steps, 
										  stepped + ( 1.0 - sharpness ) / steps, 
										  lightIntensity );
		return lerp( stepped, smoothStepped, 1.0 - sharpness );
	}


	float3 StylizedHighlight( float3 normalWs, float3 viewDir, float3 lightDir )
	{
		float3 halfDir = normalize( lightDir + viewDir );
		float specular = saturate( dot( normalWs, halfDir ) );
		
		specular = pow( specular, g_flHighlightSharpness * 100.0 );
		
		float highlight = smoothstep( 1.0 - g_flHighlightSize, 1.0, specular );
		
		float squareEffect = abs( dot( normalWs, halfDir ) );
		squareEffect = pow( squareEffect, g_flHighlightSquareMagnitude * 50.0 );
		
		highlight = highlight * g_flHighlightScale;
		highlight = saturate( highlight + squareEffect * 0.1 );
		
		return g_vHighlightColor.rgb * highlight;
	}

	float4 MainPs( PixelInput i ) : SV_Target0
	{
		Material m = Material::From( i );
		
		float4 albedo = g_tColor.Sample( g_sAniso, i.vTextureCoords.xy );
		float3 normalMap = g_tNormal.Sample( g_sAniso, i.vTextureCoords.xy ).rgb;
		float roughness = g_tRoughness.Sample( g_sAniso, i.vTextureCoords.xy ).r;
		
		float3 normalWs = TransformNormal( DecodeNormal( normalMap ), i.vNormalWs, i.vTangentUWs, i.vTangentVWs );
		normalWs = normalize( normalWs );
		
		float3 positionWs = i.vPositionWithOffsetWs.xyz + g_vCameraPositionWs;
		float3 viewDir = normalize( g_vCameraPositionWs - positionWs );
		
		float3 baseColor = g_vBaseColor.rgb * albedo.rgb;
		float3 shadeColor = g_vShadeColor.rgb * albedo.rgb;
		
		float3 finalColor = baseColor;
		
		float3 accumulatedLight = float3( 0, 0, 0 );
		float totalLightIntensity = 0.0;
		
		for ( int lightIndex = 0; lightIndex < DynamicLight::Count( positionWs ); lightIndex++ )
		{
			Light light = DynamicLight::From( positionWs, lightIndex );
			
			float3 lightDir = light.Direction;
			float NdotL = dot( normalWs, lightDir );
			
			float lightIntensity = saturate( NdotL * 0.5 + 0.5 );
			lightIntensity *= light.Visibility * light.Attenuation;
			
			#if S_TOON_SHADING
				lightIntensity = lightIntensity * 2.0 - 1.0 + g_flShadingShift;
				lightIntensity = saturate( lightIntensity );
				
				lightIntensity = ToonShade( lightIntensity, g_flShadingSteps, g_flShadingSharpness );
			#endif
			
			float3 litColor = lerp( shadeColor, baseColor, lightIntensity );
			
			#if S_STYLIZED_HIGHLIGHT
				float3 highlight = StylizedHighlight( normalWs, viewDir, lightDir );
				litColor += highlight * light.Attenuation;
			#endif
			
			accumulatedLight += litColor * light.Color * light.Attenuation;
			totalLightIntensity += light.Attenuation;
		}
		
		float3 ambientColor = AmbientLight::From( positionWs, normalWs );
		accumulatedLight += ambientColor * baseColor * 0.3;
		
		if ( totalLightIntensity > 0.01 )
		{
			finalColor = accumulatedLight;
		}
		else
		{
			finalColor = baseColor * ambientColor * 0.5;
		}
	
		
		m.Albedo = finalColor;
		m.Normal = normalWs;
		m.Roughness = roughness;
		m.Metalness = 0.0;
		m.AmbientOcclusion = 1.0;
		m.Opacity = 1.0;
		m.Emission = float3( 0, 0, 0 );
		
		finalColor = DoAtmospherics( positionWs, i.vPositionSs.xy, float4( finalColor, 1.0 ) ).rgb;
		
		return float4( finalColor, 1.0 );
	}
}
