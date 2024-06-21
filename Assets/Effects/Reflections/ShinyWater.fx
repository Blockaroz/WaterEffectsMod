sampler uImage0 : register(s0);
float2 uZoom;
texture uReflectionMap;
sampler2D texture0 = sampler_state
{
    texture = <uReflectionMap>;
    magfilter = LINEAR;
    minfilter = LINEAR;
    mipfilter = LINEAR;
    AddressU = wrap;
    AddressV = wrap;
};
texture uScreenCutout;
sampler2D texture1 = sampler_state
{
    texture = <uScreenCutout>;
    magfilter = LINEAR;
    minfilter = LINEAR;
    mipfilter = LINEAR;
    AddressU = wrap;
    AddressV = wrap;
};
texture uNoise0;
sampler2D texture2 = sampler_state
{
    texture = <uNoise0>;
    magfilter = LINEAR;
    minfilter = LINEAR;
    mipfilter = LINEAR;
    AddressU = wrap;
    AddressV = wrap;
};
texture uNoise1;
sampler2D texture3 = sampler_state
{
    texture = <uNoise1>;
    magfilter = LINEAR;
    minfilter = LINEAR;
    mipfilter = LINEAR;
    AddressU = wrap;
    AddressV = wrap;
};
float2 uImageSize;
float uDepth;
float uClearness;
float2 uScreenPosition;
float uTime;

float4 PixelShaderFunction(float4 baseColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{    
    float4 screenTexture = tex2D(uImage0, coords);
    float4 map = tex2D(texture0, coords) * tex2D(texture1, coords);  
    float mask = map.a > 0 ? 1 : 0;
    float mapPower = (1 - ((map.r + map.g) / 2.0));

    float reflectionOffset = (map.r + map.g) / uImageSize.y * (16.0 * uDepth);
    float xtraClearer = smoothstep(-0.2, 0.5, coords.x) * smoothstep(-0.2, 0.5, 1 - coords.x) * smoothstep(1.0, 0.5, reflectionOffset);

    float2 powerCoords = float2(coords.x, coords.y);
    float noise = tex2D(texture2, powerCoords * 2 / uZoom * (uImageSize / uImageSize.y) + uScreenPosition * 2 / uImageSize.y + float2(-frac(uTime / 15), -frac(uTime / 9))) * smoothstep(0.9, 0.6, reflectionOffset);
    float noise2 = tex2D(texture3, powerCoords * 2 / uZoom * (uImageSize / uImageSize.y) + uScreenPosition * 2 / uImageSize.y + float2(frac(uTime / 15), frac(uTime / 6 - noise * 0.1)));
    
    float reflectionStrength = uClearness * sqrt(mapPower) * smoothstep(0.95, 1.4, coords.y / uZoom.y + (1 - reflectionOffset)) * pow(map.b, 2);
    
    float2 noiseCoords = float2((noise * 2 - 0.25) * 0.1, (noise2 * 2 - 1) * 0.1) * (1 - mapPower) * noise;
    float4 reflectedImage = tex2D(uImage0, noiseCoords + float2(coords.x, coords.y - (reflectionOffset - 2 / uImageSize.y) * uZoom.y));

    float4 noiseColor = screenTexture * noise2 * noise * 2 * pow(map.b, 2) * mask;
    
    return screenTexture + noiseColor + reflectedImage * (length(screenTexture.rgb) * 0.75 + 0.25) * reflectionStrength * xtraClearer * mask;

}

technique Technique1
{
    pass ShaderPass
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}