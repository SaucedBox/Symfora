sampler inputTexture;

float4 Soano(float2 coords: TEXCOORD0): COLOR0
{
	float4 color = tex2D(inputTexture, coords);
	float lum = (color.r + color.g + color.b) / 3;
	color.rgb = round(lum * 30) / 30;
	//color.a = 1;
	color.rgb *= float3(0.7f, 0.6f, 1);
	return color;
}

technique Technique1
{
	pass Pass1
	{
		PixelShader = compile ps_3_0 Soano();
	}
};
