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
texture uImageCutout;
sampler2D tex1= sampler_state
{
    texture = <uImageCutout>;
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
    float2 screenCoords = (coords - 0.5) / uZoom / (uImageSize.x / uImageSize.y) + 0.5;
    float mask = length(tex2D(tex1, screenCoords).rgb * tex2D(tex0, screenCoords).rgb) > 0.0 ? 1 : 0;
    float4 map = tex2D(tex0, screenCoords) * mask;

    if (length(map.rgb) > 0 && debug)
    {
        return map;
    }
        
    float reflectionOffset = ((map.r + map.g) / 2.0 + 1 / uImageSize.y) * (uZoom * (uImageSize.x / uImageSize.y));
    float xtraClearer = smoothstep(-0.2, 0.5, coords.x) * smoothstep(-0.2, 0.5, 1 - coords.x) * smoothstep(-0.1, 0.02, reflectionOffset * uDepth / 16.0);
    float reflectionStrength = pow(1 - reflectionOffset * 16 / (uDepth / 2), 2) * smoothstep(0, 0.3, coords.y - reflectionOffset) * pow(map.b, 2);

    float yCoord = coords.y - reflectionOffset;
    float4 reflectedImage = tex2D(uImage0, float2(coords.x, yCoord));
    return screenTexture + reflectedImage * sqrt(screenTexture) * mask * xtraClearer * reflectionStrength * uClearness;

}

technique Technique1
{
    pass ShaderPass
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}