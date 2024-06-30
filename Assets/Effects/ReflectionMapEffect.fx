sampler uImage0 : register(s0);

float uDepth;
float uWidth;

float4 PixelShaderFunction(float4 baseColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{    
    float pb = coords.x / uWidth;
    float b = smoothstep(1.0, 2.5 / 3.0, pb) * smoothstep(0, 0.5 / 3.0, pb);
    return float4(0, 0, b, 1);
}

technique Technique1
{
    pass ShaderPass
    {
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}