#[compute]
#version 450

// Invocations in the (x, y, z) dimension
layout(local_size_x = 1, local_size_y = 1, local_size_z = 1) in;

layout(set = 0, binding = 0, std430) readonly buffer Vertices {
    float data[]; // vec3
} vertices;

layout(set = 0, binding = 1, std430) writeonly buffer Heights {
    float data[];
} heights;

layout(set = 0, binding = 2, std430) buffer Params {
    float amplitude;
    float frequency;
    float lacunarity;
    float gain;
} params;

vec3 randomGradient(vec3 point) {
    point = vec3(dot(point, vec3(127.1, 311.7, 513.7)),
            dot(point, vec3(269.5, 183.3, 396.5)),
            dot(point, vec3(421.3, 314.1, 119.7)));

    return -1.0 + 2.0 * fract(sin(point) * 43758.5453123);
}

float noise(vec3 point) {
    vec3 gridIndex = floor(point);
    vec3 gridFract = fract(point);

    vec3 blur = smoothstep(0.0, 1.0, gridFract);

    vec3 blb = gridIndex + vec3(0.0, 0.0, 0.0);
    vec3 brb = gridIndex + vec3(1.0, 0.0, 0.0);
    vec3 tlb = gridIndex + vec3(0.0, 1.0, 0.0);
    vec3 trb = gridIndex + vec3(1.0, 1.0, 0.0);
    vec3 blf = gridIndex + vec3(0.0, 0.0, 1.0);
    vec3 brf = gridIndex + vec3(1.0, 0.0, 1.0);
    vec3 tlf = gridIndex + vec3(0.0, 1.0, 1.0);
    vec3 trf = gridIndex + vec3(1.0, 1.0, 1.0);

    vec3 gradBLB = randomGradient(blb);
    vec3 gradBRB = randomGradient(brb);
    vec3 gradTLB = randomGradient(tlb);
    vec3 gradTRB = randomGradient(trb);
    vec3 gradBLF = randomGradient(blf);
    vec3 gradBRF = randomGradient(brf);
    vec3 gradTLF = randomGradient(tlf);
    vec3 gradTRF = randomGradient(trf);

    vec3 distToPixelFromBLB = gridFract - vec3(0.0, 0.0, 0.0);
    vec3 distToPixelFromBRB = gridFract - vec3(1.0, 0.0, 0.0);
    vec3 distToPixelFromTLB = gridFract - vec3(0.0, 1.0, 0.0);
    vec3 distToPixelFromTRB = gridFract - vec3(1.0, 1.0, 0.0);
    vec3 distToPixelFromBLF = gridFract - vec3(0.0, 0.0, 1.0);
    vec3 distToPixelFromBRF = gridFract - vec3(1.0, 0.0, 1.0);
    vec3 distToPixelFromTLF = gridFract - vec3(0.0, 1.0, 1.0);
    vec3 distToPixelFromTRF = gridFract - vec3(1.0, 1.0, 1.0);

    float dotBLB = dot(gradBLB, distToPixelFromBLB);
    float dotBRB = dot(gradBRB, distToPixelFromBRB);
    float dotTLB = dot(gradTLB, distToPixelFromTLB);
    float dotTRB = dot(gradTRB, distToPixelFromTRB);
    float dotBLF = dot(gradBLF, distToPixelFromBLF);
    float dotBRF = dot(gradBRF, distToPixelFromBRF);
    float dotTLF = dot(gradTLF, distToPixelFromTLF);
    float dotTRF = dot(gradTRF, distToPixelFromTRF);

    return mix(
        mix(
            mix(dotBLB, dotBRB, blur.x),
            mix(dotTLB, dotTRB, blur.x), blur.y
        ),
        mix(
            mix(dotBLF, dotBRF, blur.x),
            mix(dotTLF, dotTRF, blur.x), blur.y
        ), blur.z
    ) + 0.5;
}

float fractalNoise(vec3 point)
{
    int octaves = 5;

    float noiseSum = 0;
    float amplitude = 1;
    float frequency = 1;

    for (int i = 0; i < octaves; i++) {
        noiseSum += noise(point * frequency) * amplitude;
        frequency *= params.lacunarity;
        amplitude *= params.gain;
    }
    return noiseSum;
}

float turbulence()
{
    int octaves = 5;

    float noiseSum = 0;
    float amplitude = 1;
    float frequency = 1;

    for (int i = 0; i < octaves; i++) {
        //noiseSum += abs()noise(point * frequency) * amplitude;
        frequency *= params.lacunarity;
        amplitude *= params.gain;
    }
    return noiseSum;
}

float smin(float a, float b, float k) {
    float h = clamp(0.5 + 0.5 * (a - b) / k, 0.0, 1.0);
    return mix(a, b, h) - k * h * (1.0 - h);
}

void main() {
    uint vertexIndex = gl_GlobalInvocationID.x;

    if (vertexIndex >= int(vertices.data.length() / 3))
    {
        return;
    }

    float x = vertices.data[vertexIndex * 3];
    float y = vertices.data[vertexIndex * 3 + 1];
    float z = vertices.data[vertexIndex * 3 + 2];
    vec3 vertex = vec3(x, y, z);

    //float latitude = atan(y / base);
    // assuming unit vector
    //float longitude = atan(x, z) / PI;
    //float latitude = asin(y) * 2 / PI;
    // (0, 0) to (1, 1)
    //vec2 coord = vec2(longitude, latitude);

    float height = 1;
    float fractal = fractalNoise(vertex * params.frequency) * params.amplitude;
    //float lake = noise(vertex * 2) * 1;
    //float lake = 0.5;
    //float shape = max(lake, fractal);
    height += fractal;

    //float height = 1 + sin(y * params.testValue) * 0.05;
    //float height = 1.0 + noise(coord * 1.0) * 0.2;

    heights.data[vertexIndex] = height;
}
