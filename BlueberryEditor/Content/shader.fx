sampler Texture : register(s0);

int mode;
float2 dimensions;
float3 hsv;

// 0 - saturation/value
// 1 - hue
// 2 - red
// 3 - green
// 4 - blue
// 5 - alpha
// 6 - hue
// 7 - saturation
// 8 - value
// 9 - color preview

float3 HSVtoRGB(float3 HSV)
{
    float3 RGB = 0;
    float C = HSV.z * HSV.y;
    float H = HSV.x * 6;
    float X = C * (1 - abs(fmod(H, 2) - 1));
    if (HSV.y != 0)
    {
        float I = floor(H);
        if (I == 0)
        {
            RGB = float3(C, X, 0);
        }
        else if (I == 1)
        {
            RGB = float3(X, C, 0);
        }
        else if (I == 2)
        {
            RGB = float3(0, C, X);
        }
        else if (I == 3)
        {
            RGB = float3(0, X, C);
        }
        else if (I == 4)
        {
            RGB = float3(X, 0, C);
        }
        else
        {
            RGB = float3(C, 0, X);
        }
    }
    float M = HSV.z - C;
    return RGB + M;
}

float3 RGBtoHSV(float3 RGB)
{
    float3 HSV = 0;
    float M = min(RGB.r, min(RGB.g, RGB.b));
    HSV.z = max(RGB.r, max(RGB.g, RGB.b));
    float C = HSV.z - M;
    if (C != 0)
    {
        HSV.y = C / HSV.z;
        float3 D = (((HSV.z - RGB) / 6) + (C / 2)) / C;
        if (RGB.r == HSV.z)
            HSV.x = D.b - D.g;
        else if (RGB.g == HSV.z)
            HSV.x = (1.0 / 3.0) + D.r - D.b;
        else if (RGB.b == HSV.z)
            HSV.x = (2.0 / 3.0) + D.g - D.r;
        if (HSV.x < 0.0)
        {
            HSV.x += 1.0;
        }
        if (HSV.x > 1.0)
        {
            HSV.x -= 1.0;
        }
    }
    return HSV;
}

float4 PixelShaderFunction(float2 coord : TEXCOORD0, float4 color : COLOR0) : COLOR0
{
    float4 pixel = tex2D(Texture, coord);
    
    if (mode == 0)
        pixel = float4(HSVtoRGB(float3(hsv.x, coord.x, 1.0 - coord.y)), pixel.a);
    else if (mode == 1)
        pixel = float4(HSVtoRGB(float3(coord.y, 1.0, 1.0)), pixel.a);
    else if (mode == 2)
        pixel.r = coord.x;
    else if (mode == 3)
        pixel.g = coord.x;
    else if (mode == 4)
        pixel.b = coord.x;
    else if (mode == 5)
    {
        pixel.a = 1.0 - coord.x;
        float size = 5.0;
        float2 Pos = floor(coord * dimensions / size);
        float PatternMask = fmod(Pos.x + fmod(Pos.y, 2.0), 2.0);
        //pixel = PatternMask * float4(1.0, 1.0, 1.0, 1.0);
        
        float alpha = pixel.a;
        float inv_alpha = 1 - alpha;
        if (PatternMask == 0)
            pixel = float4(0.4 * alpha + pixel.r * inv_alpha, 0.4 * alpha + pixel.g * inv_alpha, 0.4 * alpha + pixel.b * inv_alpha, 1.0);
        else
            pixel = float4(0.6 * alpha + pixel.r * inv_alpha, 0.6 * alpha + pixel.g * inv_alpha, 0.6 * alpha + pixel.b * inv_alpha, 1.0);
    }
    else if (mode == 6)
        pixel = float4(HSVtoRGB(float3(coord.x, hsv.y, hsv.z)), pixel.a);
    else if (mode == 7)
        pixel = float4(HSVtoRGB(float3(hsv.x, coord.x, hsv.z)), pixel.a);
    else if (mode == 8)
        pixel = float4(HSVtoRGB(float3(hsv.x, hsv.y, coord.x)), pixel.a);
    else if (mode == 9)
    {
        float size = 5.0;
        float2 Pos = floor(coord * dimensions / size);
        float PatternMask = fmod(Pos.x + fmod(Pos.y, 2.0), 2.0);
        //pixel = PatternMask * float4(1.0, 1.0, 1.0, 1.0);
        
        float alpha = pixel.a;
        float inv_alpha = 1 - alpha;
        if (PatternMask == 0)
            pixel = float4(pixel.r * alpha + 0.4 * inv_alpha, pixel.g * alpha + 0.4 * inv_alpha, pixel.b * alpha + 0.4 * inv_alpha, 1.0);
        else
            pixel = float4(pixel.r * alpha + 0.6 * inv_alpha, pixel.g * alpha + 0.6 * inv_alpha, pixel.b * alpha + 0.6 * inv_alpha, 1.0);
    }
    
    return pixel * color;
}

technique
{
    pass P0
    {
        AlphaBlendEnable = true;
        DestBlend = INVSRCALPHA;
        SrcBlend = SRCALPHA;
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}