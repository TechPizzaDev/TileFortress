#define VS_SHADERMODEL vs_3_0
#define PS_SHADERMODEL ps_3_0

Texture2D TileTexture;
sampler2D TileTextureSampler = sampler_state
{
	Texture = <TileTexture>;
};

struct VertexShaderInput
{
	float2 Position : SV_POSITION;
	float4 Color : COLOR0;
	float2 TexCoords : TEXCOORD0;
};

float4 MainTile(VertexShaderInput input) : COLOR
{
	float4 result = tex2D(TileTextureSampler, input.TexCoords) * input.Color;
	
	return result;
}

technique TileDrawing
{
	pass Tile0
	{
		PixelShader = compile PS_SHADERMODEL MainTile();
	}
};