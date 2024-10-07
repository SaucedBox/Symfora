sampler inputTexture;
float3 phantomColor;

float4 Phantom(float2 coords: TEXCOORD0): COLOR0
{
	float4 col = tex2D(inputTexture, coords);
	//float3 selectColor = float3(0.227f, 0.164f, 0.941f);
	float lum = (col.r + col.g + col.b) / 3;
	col.rgb *= lum < 0.1f ? phantomColor * 0.1f : phantomColor;
	return col;
}

technique Technique1
{
	pass Pass1
	{
		PixelShader = compile ps_3_0 Phantom();
	}
};
