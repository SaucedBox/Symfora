sampler inputTexture;
sampler geoMap;
float2 resolution;
float2 cameraLocation;
float gameTime;
float zoom;
float3 ambientLight;

float2 lightPos;
float2 areaDim;
float3 lightCol;
float brightness;
float radius;
int visible;
float lightCount;

float4 AreaLighting(float2 coords: TEXCOORD0): COLOR0
{
	float4 col = tex2D(inputTexture, coords);
	float4 norm = tex2D(geoMap, coords);
	float2 pos = ceil(coords * resolution);

	float relLightPosX = ceil(lightPos.x - cameraLocation.x);
	float relLightPosY = ceil(lightPos.y - cameraLocation.y);

	float slX = smoothstep(relLightPosX, relLightPosX + (radius * 16), pos.x);
	float slY = smoothstep(relLightPosY, relLightPosY + (radius * 16), pos.y);
	slX *= 1 - smoothstep(relLightPosX + areaDim.x, relLightPosX + areaDim.x + (radius * 16), pos.x + (radius * 16));
	slY *= 1 - smoothstep(relLightPosY + areaDim.y, relLightPosY + areaDim.y + (radius * 16), pos.y + (radius * 16));

	float intensity = clamp(slX * slY * brightness, 0, brightness);
	float normLum = (norm.r + norm.g + norm.b) / 3;
	float normLev = normLum == 0 ? 1 : normLum;	

	float3 lc = lightCol * visible * intensity * normLev;
	col.rgb *= lc;
	col.rgb += ambientLight;
	return col;
}

technique Technique1
{
	pass Pass1
	{
		PixelShader = compile ps_3_0 AreaLighting();
	}
};