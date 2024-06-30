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
bool debug;

float4 PixelShaderFunction(float4 baseColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{    
    float4 screenTexture = tex2D(uImage0, coords);
    float2 screenCoords = ((coords - 1) / (uImageSize.x / uImageSize.y)) / uZoom;
    float4 map = tex2D(tex0, screenCoords);

    float2 texelSize = 1 / uImageSize;
    float maxRadius = 1;
    
    for (int i = -maxRadius; i < maxRadius; i++)
    {
        for (int j = -maxRadius; j < maxRadius; j++)
        {
            
        }
    }
    
    float xtraClearer = smoothstep(-0.2, 0.5, coords.x) * smoothstep(-0.2, 0.5, 1 - coords.x);

    return screenTexture + map * xtraClearer;
}

technique Technique1
{
    pass ShaderPass
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}