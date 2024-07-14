sampler uImage0 : register(s0);

float uDepth;
float uWidth;

float4 PixelShaderFunction(float4 baseColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{    
    float p = coords.y / 16.0;

    float r = p;
    float g = max(p - 1, 0);
    float pb = coords.x / 3;
    float b = smoothstep(0, 0.34, pb) * smoothstep(0, 0.34, 1.0 - pb);
    return float4(r, g, b, 1);
}

technique Technique1
{
    pass ShaderPass
    {
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}