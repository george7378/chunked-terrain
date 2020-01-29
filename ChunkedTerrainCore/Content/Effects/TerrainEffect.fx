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
float4x4 ViewProjection;

bool EnableFog;

// -1 = clip above water, 0 = no clipping, 1 = clip below water
int WaterClipMode;

float LightPower;
float AmbientLightPower;
float WaterClipDepthOffset;
float FogStart;
float FogEnd;

float3 CameraPosition;
float3 LightDirection;
float3 FogColour;

Texture GroundTexture;
Texture GroundSlopeTexture;
Texture GroundDetailTexture;


//////////////////
//Sampler states//
//////////////////
sampler GroundTextureSampler = sampler_state
{
	texture = <GroundTexture>;
	minfilter = LINEAR;
	magfilter = LINEAR;
	mipfilter = LINEAR;
	AddressU = Wrap;
	AddressV = Wrap;
};

sampler GroundSlopeTextureSampler = sampler_state
{
	texture = <GroundSlopeTexture>;
	minfilter = LINEAR;
	magfilter = LINEAR;
	mipfilter = LINEAR;
	AddressU = Wrap;
	AddressV = Wrap;
};

sampler GroundDetailTextureSampler = sampler_state
{
	texture = <GroundDetailTexture>;
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
	float4 Position                 : POSITION0;
	float3 Normal                   : NORMAL0;
	float2 TextureCoordinates       : TEXCOORD0;
	float2 TextureCoordinatesDetail : TEXCOORD1;
};

struct VertexShaderOutput
{
	float4 Position                 : POSITION0;
	float3 Normal                   : TEXCOORD0;
	float2 TextureCoordinates       : TEXCOORD1;
	float2 TextureCoordinatesDetail : TEXCOORD2;
	float3 PositionWorld            : TEXCOORD3;
};


///////////
//Shaders//
///////////
VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
	VertexShaderOutput output;

	output.Position = mul(input.Position, ViewProjection);
	output.Normal = input.Normal;
	output.TextureCoordinates = input.TextureCoordinates;
	output.TextureCoordinatesDetail = input.TextureCoordinatesDetail;
	output.PositionWorld = input.Position.xyz;

	return output;
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
	clip((input.PositionWorld.y + WaterClipDepthOffset)*WaterClipMode);

	float4 baseColour = tex2D(GroundTextureSampler, input.TextureCoordinates);

	float slope = 1.0f - input.Normal.y;
	if (slope > 0.02f)
	{
		float4 slopeColour = tex2D(GroundSlopeTextureSampler, input.TextureCoordinates);
		baseColour = lerp(baseColour, slopeColour, saturate((slope - 0.02f)/(0.05f - 0.02f)));
	}

	float viewDistance = length(CameraPosition - input.PositionWorld);
	if (viewDistance < 100.0f)
	{
		float4 detailColour = tex2D(GroundDetailTextureSampler, input.TextureCoordinatesDetail);
		baseColour.rgb = lerp(baseColour.rgb*detailColour.rgb*2.0f, baseColour.rgb, saturate(viewDistance/100.0f));
	}

	float diffuseLightingFactor = saturate(dot(-LightDirection, input.Normal))*LightPower;

	float4 finalColour = float4(baseColour.rgb*(AmbientLightPower + diffuseLightingFactor), -input.PositionWorld.y/20.0f);
	finalColour.rgb = lerp(finalColour.rgb, FogColour, saturate((viewDistance - FogStart)/(FogEnd - FogStart))*EnableFog);

	return finalColour;
}

technique TerrainTechnique
{
	pass Pass1
	{
		//Fillmode = Wireframe;

		VertexShader = compile VS_SHADERMODEL VertexShaderFunction();
		PixelShader = compile PS_SHADERMODEL PixelShaderFunction();
	}
}