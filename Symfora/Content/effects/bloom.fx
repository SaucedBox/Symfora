#define THRESH 0.45f
#define MULT 1

sampler inputTexture;
float2 resolution;
float zoom;

float4 Bloom(float2 coords: TEXCOORD0): COLOR0
{
	float4 color = tex2D(inputTexture, coords);
	float lum = (color.r + color.g + color.b) / 3;
	bool brightness = lum > THRESH;

	float weight[5] = { 0.227027f, 0.1945946f, 0.1216216f, 0.054054f, 0.016216f };
	float2 texOffset = 1.0f / resolution * zoom;
	float4 dumbCol = brightness ? color : float4(0, 0, 0, 0);
	float4 result = dumbCol * weight[0];
	for(int i = 1; i < 5; i++)
    {
		float4 b = tex2D(inputTexture, coords + float2(texOffset.x * i, 0)) * (weight[i] / 2);
    	result += (b.r + b.g + b.b) / 3 > THRESH * weight[i] / 2 ? b : 0;
		b = tex2D(inputTexture, coords - float2(texOffset.x * i, 0)) * (weight[i] / 2);
    	result += (b.r + b.g + b.b) / 3 > THRESH * weight[i] / 2 ? b : 0;
		b = tex2D(inputTexture, coords + float2(0, texOffset.y * i)) * (weight[i] / 2);
		result += (b.r + b.g + b.b) / 3 > THRESH * weight[i] / 2 ? b : 0;
		b = tex2D(inputTexture, coords - float2(0, texOffset.y * i)) * (weight[i] / 2);
     	result += (b.r + b.g + b.b) / 3 > THRESH * weight[i] / 2 ? b : 0;
	}

	result *= MULT;
	result.a = brightness ? 1 : result.a;
	return result.a == 1 ? color : color + result;
}

technique Technique1
{
	pass Pass1
	{
		PixelShader = compile ps_3_0 Bloom();
	}
};
