sampler uImage0 : register(s0);
texture uReflectionMap;
sampler2D tex0 = sampler_state
{
    texture = <uReflectionMap>;
    magfilter = POINT;
    minfilter = POINT;
    mipfilter = POINT;
    AddressU = wrap;
    AddressV = wrap;
};
texture uScreenCutout;
sampler2D tex1 = sampler_state
{
    texture = <uScreenCutout>;
    magfilter = POINT;
    minfilter = POINT;
    mipfilter = POINT;
    AddressU = wrap;
    AddressV = wrap;
};
float2 uImageSize;
float uDepth;
float uClearness;
float2 uZoom;

float4 PixelShaderFunction(float4 baseColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{    
    float4 screenTexture = tex2D(uImage0, coords);
    float2 screenCoords = coords;
    float4 map = tex2D(tex0, screenCoords) * (length(tex2D(tex1, screenCoords).rgb > 0 ? 1 : 0));

    float mask = tex2D(tex1, coords).a > 0 ? 1 : 0;
    float mapPower = (1 - ((map.r + map.g) / 2.0));

    float reflectionOffset = (map.r + map.g) / uImageSize.y * (16.0 * uDepth);
    float xtraClearer = smoothstep(-0.2, 0.5, coords.x) * smoothstep(-0.2, 0.5, 1 - coords.x) * smoothstep(1.0, 0.5, reflectionOffset);    
    float reflectionStrength = uClearness * sqrt(mapPower) * smoothstep(1 - reflectionOffset * 0.4, 1.1, coords.y / uZoom.y + (1 - reflectionOffset)) * pow(map.b, 2);
    
    float4 reflectedImage = tex2D(uImage0, float2(coords.x, coords.y - (reflectionOffset - 2 / uImageSize.y) * uZoom.y));
    return screenTexture + reflectedImage * sqrt(screenTexture) * xtraClearer * mask * reflectionStrength;
}

technique Technique1
{
    pass ShaderPass
    {
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}