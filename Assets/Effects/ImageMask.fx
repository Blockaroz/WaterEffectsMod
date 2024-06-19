sampler uImage0 : register(s0);
texture uMaskImage;
sampler2D maskTexture = sampler_state
{
    texture = <uMaskImage>;
    magfilter = LINEAR;
    minfilter = LINEAR;
    mipfilter = LINEAR;
    AddressU = wrap;
    AddressV = wrap;
};
float2 uMaskSize;
bool inverse;

float4 PixelShaderFunction(float4 baseColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    float4 baseImage = tex2D(uImage0, coords);   
    float4 mask = tex2D(maskTexture, coords);

    return baseImage * (1 - mask.a);
}

technique Technique1
{
    pass ShaderPass
    {
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}