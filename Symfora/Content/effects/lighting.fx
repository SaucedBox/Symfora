sampler inputTexture;
sampler geoMap;
sampler shadowMap;
float2 resolution;
float2 cameraLocation;
float gameTime;
float zoom;
float3 ambientLight;

float2 lightPos;
float3 lightCol;
float brightness;
float radius;
int visible;
int shadow;
int directional;
float lightCount;
float dirRadius;
float angle;
float falloffDist;

float Dither(float col, int tx, int ty) {
	matrix <int, 4, 4> bayer = {0, 8, 2, 10, 12, 4, 14, 6, 3, 11, 1, 9, 15, 7, 13, 5};
	int d = bayer[ty % 4][tx % 4];
	return col * 10 <= d ? 0.8f : 1;
}

float4 Lighting(float2 coords: TEXCOORD0): COLOR0
{
	float4 col = tex2D(inputTexture, coords);
	float4 norm = tex2D(geoMap, coords);
	float4 shad = tex2D(shadowMap, coords + (cameraLocation / resolution));
	float2 pos = (coords * resolution);

	float relLightPosX = lightPos.x - cameraLocation.x;
	float relLightPosY = lightPos.y - cameraLocation.y;
	float xPos = pos.x - relLightPosX;
	float yPos = pos.y - relLightPosY;	
	xPos = ceil(xPos);
	yPos = ceil(yPos);

	float distance = sqrt(xPos * xPos + yPos * yPos);
	//distance = floor(distance / 3) * 3;
	distance = distance / 100 + 1;
	//distance = round(distance * 42) / 42;

	float intensity = 1 / pow(distance, radius + 1);
	intensity *= brightness;
	intensity = clamp(intensity, 0, brightness);
	float falloff = -((distance - falloffDist) / 2 - 1);
	falloff = clamp(falloff, 0, 1);
	intensity *= falloff;
	//intensity *= Dither(intensity, pos.x, pos.y);

	float normLum = (norm.r + norm.g + norm.b) / 3;
	float shadLev = floor(shad) > 0 || shadow == 0 ? 1 : 0.75f;
	float normLev = normLum == 0 ? 1 : normLum;	

	// * calAngle
	float3 lc = lightCol * visible * intensity * normLev * shadLev;
	col.rgb *= lc;
	col.rgb += ambientLight.rgb * col.a;

	return col;
}

technique Technique1
{
	pass Pass1
	{
		PixelShader = compile ps_3_0 Lighting();
	}
};