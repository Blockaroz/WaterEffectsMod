sampler uImage0 : register(s0);

float uDepth;

float4 PixelShaderFunction(float4 baseColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{    
    float p = coords.y / uDepth;

    float r = clamp(p * 2, 0, 1);
    float g = clamp(lerp(0, 1, p * 2 - 1), 0, 1); 
    float pb = coords.x / 4;
    float b = smoothstep(-0.1, 1.0 / 3, pb) * smoothstep(-0.1, 1.0 / 3, 1 - pb);
    return float4(r, g, b, 1);
}

technique Technique1
{
    pass ShaderPass
    {
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}