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

	/*float calAngle = atan2(pos.x - relLightPosX, pos.y - relLightPosY);
	calAngle += (angle - dirRadius >= 0 && calAngle <= 0) ? 3.141592f * 2 : 0;
	float top = smoothstep(angle - dirRadius, angle, calAngle);
	float bot = 1 - smoothstep(angle, angle + dirRadius, calAngle);
	calAngle = directional == 1 ? abs(top + (bot - 1)) : 1;*/

	float distance = sqrt(xPos * xPos + yPos * yPos);
	distance = floor(distance / 3) * 3;
	distance = distance / 100 + 1;
	distance = round(distance * 42) / 42;

	float intensity = 1 / pow(distance, radius + 1);
	intensity *= brightness;
	intensity = clamp(intensity, 0, brightness);
	float falloff = -((distance - falloffDist) / 2 - 1);
	falloff = clamp(falloff, 0, 1);
	intensity *= falloff;

	float normLum = (norm.r + norm.g + norm.b) / 3;
	float shadLev = floor(shad) > 0 || shadow == 0 ? 1 : 0.75f;
	float normLev = normLum == 0 ? 1 : normLum;	

	// * calAngle
	float3 lc = lightCol * visible * intensity * normLev * shadLev;
	col.rgb *= lc;
	col.rgb += ambientLight;

	return col;
}

technique Technique1
{
	pass Pass1
	{
		PixelShader = compile ps_3_0 Lighting();
	}
};