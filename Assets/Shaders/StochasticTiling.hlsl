// StochasticTiling.hlsl
// Uso: Custom Function Node en Shader Graph (Unity 6 / URP)
// Basado en el algoritmo de "tiling and blending" de Heitz & Neyret,
// version simplificada (sin histogram-preserving blend).
//
// En el Custom Function Node:
//   Type: File
//   Source: (arrastrar este archivo)
//   Name: StochasticTile_float
//   Inputs:  Tex (Texture2D), Samp (SamplerState), UV (Vector2)
//   Outputs: Out (Vector4)

// --- Hash simple 2D -> 2D, determinista, uniforme ---
float2 Hash2D(float2 p)
{
    float2 r = float2(
        dot(p, float2(127.1, 311.7)),
        dot(p, float2(269.5, 183.3))
    );
    return frac(sin(r) * 43758.5453);
}

// --- Divide el UV en una grilla triangular y devuelve los 3 vertices
//     mas cercanos junto con sus pesos baricentricos ---
void TriangleGrid(
    float2 uv,
    out float w1, out float w2, out float w3,
    out float2 vertex1, out float2 vertex2, out float2 vertex3)
{
    // Escala para que las celdas queden a un tamano razonable
    uv *= 3.464; // 2 * sqrt(3)

    // Deforma (skew) el espacio para convertir la grilla cuadrada en triangular
    const float2x2 gridToSkewedGrid = float2x2(1.0, 0.0, -0.57735027, 1.15470054);
    float2 skewedCoord = mul(gridToSkewedGrid, uv);

    float2 baseId = floor(skewedCoord);
    float3 temp = float3(frac(skewedCoord), 0.0);
    temp.z = 1.0 - temp.x - temp.y;

    if (temp.z > 0.0)
    {
        w1 = temp.z;
        w2 = temp.y;
        w3 = temp.x;
        vertex1 = baseId;
        vertex2 = baseId + float2(0.0, 1.0);
        vertex3 = baseId + float2(1.0, 0.0);
    }
    else
    {
        w1 = -temp.z;
        w2 = 1.0 - temp.y;
        w3 = 1.0 - temp.x;
        vertex1 = baseId + float2(1.0, 1.0);
        vertex2 = baseId + float2(1.0, 0.0);
        vertex3 = baseId + float2(0.0, 1.0);
    }
}

// --- Funcion principal que llama el Custom Function Node ---
void StochasticTile_float(
    UnityTexture2D Tex, UnitySamplerState Samp, float2 UV,
    out float4 Out)
{
    float w1, w2, w3;
    float2 vertex1, vertex2, vertex3;
    TriangleGrid(UV, w1, w2, w3, vertex1, vertex2, vertex3);

    // Offset unico por vertice de la grilla (esto es lo que rompe la repeticion)
    float2 uv1 = UV + Hash2D(vertex1);
    float2 uv2 = UV + Hash2D(vertex2);
    float2 uv3 = UV + Hash2D(vertex3);

    // Derivadas calculadas sobre el UV original para que el mipmapping
    // y el filtrado anisotropico se vean correctos pese al offset
    float2 duvdx = ddx(UV);
    float2 duvdy = ddy(UV);

    float4 c1 = SAMPLE_TEXTURE2D_GRAD(Tex.tex, Samp.samplerstate, uv1, duvdx, duvdy);
    float4 c2 = SAMPLE_TEXTURE2D_GRAD(Tex.tex, Samp.samplerstate, uv2, duvdx, duvdy);
    float4 c3 = SAMPLE_TEXTURE2D_GRAD(Tex.tex, Samp.samplerstate, uv3, duvdx, duvdy);

    Out = w1 * c1 + w2 * c2 + w3 * c3;
}
