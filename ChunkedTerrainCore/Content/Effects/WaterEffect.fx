#if OPENGL
	#define SV_POSITION POSITION
	#define VS_SHADERMODEL vs_3_0
	#define PS_SHADERMODEL ps_3_0
#else
	#define VS_SHADERMODEL vs_4_0_level_9_1
	#define PS_SHADERMODEL ps_4_0_level_9_1
#endif


////////////////////
//Global variables//
////////////////////
float4x4 World;
float4x4 WorldViewProjection;

float SpecularExponent;
float WaveScale;
float FogStart;
float FogEnd;

float2 TextureCoordinateOffset1;
float2 TextureCoordinateOffset2;

float3 CameraPosition;
float3 LightDirection;
float3 FogColour;
float3 WaterIntrinsicColour;

Texture NormalMapTexture;
Texture RefractionMapTexture;
Texture ReflectionMapTexture;


//////////////////
//Sampler states//
//////////////////
sampler NormalMapTextureSampler = sampler_state
{
	texture = <NormalMapTexture>;
	magfilter = LINEAR;
	minfilter = LINEAR;
	mipfilter = LINEAR;
	AddressU = Wrap;
	AddressV = Wrap;
};

sampler RefractionMapTextureSampler = sampler_state
{
	texture = <RefractionMapTexture>;
	magfilter = LINEAR;
	minfilter = LINEAR;
	mipfilter = LINEAR;
	AddressU = Mirror;
	AddressV = Mirror;
};

sampler ReflectionMapTextureSampler = sampler_state
{
	texture = <ReflectionMapTexture>;
	magfilter = LINEAR;
	minfilter = LINEAR;
	mipfilter = LINEAR;
	AddressU = Mirror;
	AddressV = Mirror;
};


//////////////////
//I/O structures//
//////////////////
struct VertexShaderInput
{
	float4 Position           : POSITION0;
	float2 TextureCoordinates : TEXCOORD0;
	float3 Normal             : NORMAL0;
	float3 Tangent            : TANGENT0;
	float3 Binormal           : BINORMAL0;
};

struct VertexShaderOutput
{
	float4 Position           : POSITION0;
	float2 TextureCoordinates : TEXCOORD0;
	float3 Normal             : TEXCOORD1;
	float3 Tangent            : TEXCOORD2;
	float3 Binormal           : TEXCOORD3;
	float3 PositionWorld      : TEXCOORD4;
	float4 ProjectedPosition  : TEXCOORD5;
};


///////////
//Shaders//
///////////
VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
	VertexShaderOutput output;

	float3x3 world3x3 = (float3x3)World;

	output.Position = mul(input.Position, WorldViewProjection);
	output.TextureCoordinates = input.TextureCoordinates;
	output.Normal = normalize(mul(input.Normal, world3x3));
	output.Tangent = normalize(mul(input.Tangent, world3x3));
	output.Binormal = normalize(mul(input.Binormal, world3x3));
	output.PositionWorld = mul(input.Position, World).xyz;
	output.ProjectedPosition = output.Position;
	
	return output;
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
	float3 normalMapColour1 = 2.0f*tex2D(NormalMapTextureSampler, input.TextureCoordinates + TextureCoordinateOffset1).rgb - 1.0f;
	float3 normalMapColour2 = 2.0f*tex2D(NormalMapTextureSampler, input.TextureCoordinates + TextureCoordinateOffset2).rgb - 1.0f;
	float3 normalMapCombinedColour = (normalMapColour1 + normalMapColour2)/2.0f;
	float3 normal = normalize(normalMapCombinedColour.r*input.Tangent + normalMapCombinedColour.g*input.Binormal + normalMapCombinedColour.b*input.Normal);

	float2 projectedTextureCoordinatesRefraction;
	projectedTextureCoordinatesRefraction.x = (input.ProjectedPosition.x/input.ProjectedPosition.w + 1.0f)/2.0f;
	projectedTextureCoordinatesRefraction.y = (-input.ProjectedPosition.y/input.ProjectedPosition.w + 1.0f)/2.0f;

	float2 projectedTextureCoordinatesReflection;
	projectedTextureCoordinatesReflection.x = projectedTextureCoordinatesRefraction.x;
	projectedTextureCoordinatesReflection.y = (input.ProjectedPosition.y/input.ProjectedPosition.w + 1.0f)/2.0f;

	float4 refractionColour = tex2D(RefractionMapTextureSampler, projectedTextureCoordinatesRefraction + normal.xz*WaveScale);
	refractionColour.rgb = lerp(refractionColour.rgb, WaterIntrinsicColour, refractionColour.a);
	
	float3 reflectionColour = tex2D(ReflectionMapTextureSampler, projectedTextureCoordinatesReflection + normal.xz*WaveScale).rgb;

	float3 viewVector = CameraPosition - input.PositionWorld;
	float3 viewVectorNormalised = normalize(viewVector);
	float3 reflectionVector = normalize(reflect(LightDirection, normal));
	
	float specularLightingFactor = pow(saturate(dot(reflectionVector, viewVectorNormalised)), SpecularExponent);

	float fresnelTerm = 1.0f/pow(1.0f + dot(viewVectorNormalised, float3(0.0f, 1.0f, 0.0f)), 5.0f);
	float shorelineDepth = tex2D(RefractionMapTextureSampler, projectedTextureCoordinatesRefraction).a;

	float4 finalColour = float4(lerp(refractionColour.rgb, reflectionColour, fresnelTerm) + float3(1.0f, 1.0f, 1.0f)*specularLightingFactor, shorelineDepth*5.0f);
	finalColour.rgb = lerp(finalColour.rgb, FogColour, saturate((length(viewVector) - FogStart)/(FogEnd - FogStart)));

	return finalColour;
}

technique WaterTechnique
{
	pass Pass1
	{
		VertexShader = compile VS_SHADERMODEL VertexShaderFunction();
		PixelShader = compile PS_SHADERMODEL PixelShaderFunction();
	}
}