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

float LightPower;
float FogStart;
float FogEnd;

float3 CameraPosition;
float3 FogColour;

Texture TreeTexture;


//////////////////
//Sampler states//
//////////////////
sampler TreeTextureSampler = sampler_state
{
	texture = <TreeTexture>;
	minfilter = LINEAR;
	magfilter = LINEAR;
	mipfilter = LINEAR;
	AddressU = Wrap;
	AddressV = Wrap;
};


//////////////////
//I/O structures//
//////////////////
struct VertexShaderInput
{
	float4 Position           : POSITION0;
	float2 TextureCoordinates : TEXCOORD0;
};

struct VertexShaderOutput
{
	float4 Position           : POSITION0;
	float2 TextureCoordinates : TEXCOORD0;
	float3 PositionWorld      : TEXCOORD1;
};


///////////
//Shaders//
///////////
VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
	VertexShaderOutput output;

	output.Position = mul(input.Position, WorldViewProjection);
	output.TextureCoordinates = input.TextureCoordinates;
	output.PositionWorld = mul(input.Position, World).xyz;

	return output;
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
	float4 treeColour = tex2D(TreeTextureSampler, input.TextureCoordinates);

	clip(treeColour.a - 0.5f);

	float4 finalColour = float4(treeColour.rgb*LightPower, treeColour.a);
	finalColour.rgb = lerp(finalColour.rgb, FogColour, saturate((length(CameraPosition - input.PositionWorld) - FogStart)/(FogEnd - FogStart)));

	return finalColour;
}

technique TreeTechnique
{
	pass Pass1
	{
		VertexShader = compile VS_SHADERMODEL VertexShaderFunction();
		PixelShader = compile PS_SHADERMODEL PixelShaderFunction();
	}
}