sampler uImage0 : register(s0);
float2 uZoom;
texture uReflectionMap;
sampler2D tex0 = sampler_state
{
    texture = <uReflectionMap>;
    magfilter = LINEAR;
    minfilter = LINEAR;
    mipfilter = LINEAR;
    AddressU = wrap;
    AddressV = wrap;
};
texture uScreenCutout;
sampler2D tex1 = sampler_state
{
    texture = <uScreenCutout>;
    magfilter = LINEAR;
    minfilter = LINEAR;
    mipfilter = LINEAR;
    AddressU = wrap;
    AddressV = wrap;
};
float2 uImageSize;
float uDepth;
float uClearness;

float4 PixelShaderFunction(float4 baseColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{    
    float4 screenTexture = tex2D(uImage0, coords);
    float4 map = tex2D(tex0, coords) * tex2D(tex1, coords);
    float mask = map.a > 0 ? 1 : 0;
    float mapPower = (1 - ((map.r + map.g) / 2.0));

    float reflectionOffset = (map.r + map.g) / uImageSize.y * (16.0 * uDepth);
    float xtraClearer = smoothstep(-0.2, 0.5, coords.x) * smoothstep(-0.2, 0.5, 1 - coords.x) * smoothstep(1.0, 0.5, reflectionOffset);
    
    float reflectionStrength = uClearness * pow(mapPower, 2) * smoothstep(0.95, 1.4, coords.y / uZoom.y + (1 - reflectionOffset)) * pow(map.b, 2);
    
    float4 reflectedImage = tex2D(uImage0, float2(coords.x, coords.y - (reflectionOffset - 2 / uImageSize.y) * uZoom.y));
    
    return screenTexture + reflectedImage * (length(screenTexture.rgb) * 0.75 + 0.25) * reflectionStrength * xtraClearer * mask;
}

technique Technique1
{
    pass ShaderPass
    {
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}