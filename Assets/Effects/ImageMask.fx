sampler uImage0 : register(s0);

texture uMaskAdd;
sampler2D maskTexture0 = sampler_state
{
    texture = <uMaskAdd>;
    magfilter = LINEAR;
    minfilter = LINEAR;
    mipfilter = LINEAR;
    AddressU = wrap;
    AddressV = wrap;
};
texture uMaskSubtract;
sampler2D maskTexture1 = sampler_state
{
    texture = <uMaskSubtract>;
    magfilter = LINEAR;
    minfilter = LINEAR;
    mipfilter = LINEAR;
    AddressU = wrap;
    AddressV = wrap;
};
texture uMaskColor;
sampler2D maskTexture2 = sampler_state
{
    texture = <uMaskColor>;
    magfilter = LINEAR;
    minfilter = LINEAR;
    mipfilter = LINEAR;
    AddressU = wrap;
    AddressV = wrap;
};

bool useAlpha;
bool useColor;
float4 uColor;

float4 PixelShaderFunction(float4 baseColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    float4 baseImage = tex2D(uImage0, coords);   
    float alpha = useAlpha ? (tex2D(maskTexture0, coords).a - tex2D(maskTexture1, coords).a) : 1;
    float mask = (tex2D(maskTexture0, coords).a > 0 ? 1 : 0) - (tex2D(maskTexture1, coords).a > 0 ? 1 : 0);
    if (useColor)
    {
        float colorMask = 0;
        float4 colorMaskImage = tex2D(maskTexture2, coords);
        if (colorMaskImage.r == uColor.r && colorMaskImage.g == uColor.g && colorMaskImage.b == uColor.b)
            colorMask = 1;

        mask *= colorMask;
    }

    return baseImage * baseColor * mask * alpha;
}

technique Technique1
{
    pass ShaderPass
    {
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}