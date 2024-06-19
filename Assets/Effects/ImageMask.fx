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

bool inverse;

float4 PixelShaderFunction(float4 baseColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    float4 baseImage = tex2D(uImage0, coords);   
    float mask = (tex2D(maskTexture0, coords).a > 0 ? 1 : 0) - (tex2D(maskTexture1, coords).a > 0 ? 1 : 0);
    if (inverse)
    {
        return baseImage * (1 - mask);
    }
    return baseImage * mask;
}

technique Technique1
{
    pass ShaderPass
    {
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}