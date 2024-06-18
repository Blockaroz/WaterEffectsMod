sampler uImage0 : register(s0);

float4 uCutoutColor;

float4 PixelShaderFunction(float4 baseColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    float4 baseImage = tex2D(uImage0, coords);   
    if (baseImage.r == uCutoutColor.r
        && baseImage.g == uCutoutColor.g
        && baseImage.b == uCutoutColor.b
        && baseImage.a == uCutoutColor.a)
        return baseColor;

    return 0;
}

technique Technique1
{
    pass ShaderPass
    {
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}