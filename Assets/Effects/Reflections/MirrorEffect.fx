sampler uImage0 : register(s0);
float2 uZoom;
texture uScreenImage;
texture uScreenCutout;
sampler2D tex0 = sampler_state
{
    texture = <uScreenImage>;
    magfilter = LINEAR;
    minfilter = LINEAR;
    mipfilter = LINEAR;
    AddressU = wrap;
    AddressV = wrap;
};
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
    float reflectionOffset = (map.r + map.g) / uImageSize.y * (16.0 * uDepth);
    float4 reflectedImage = tex2D(uImage0, float2(coords.x, coords.y - (reflectionOffset - 2 / uImageSize.y) * uZoom.y));     
    float reflectionStrength = pow(1 - reflectionOffset, 3) * uClearness * smoothstep(0, 0.45, coords.y - reflectionOffset);
    float xtraClearer = smoothstep(-0.2, 0.5, coords.x) * smoothstep(-0.2, 0.5, 1 - coords.x) * smoothstep(-0.01, 0.2, reflectionOffset) * map.b;  
    return lerp(screenTexture, reflectedImage, reflectionStrength * xtraClearer * uClearness);
}

technique Technique1
{
    pass ShaderPass
    {
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}