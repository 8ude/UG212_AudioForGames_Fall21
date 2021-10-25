#ifndef MATH_CGINC
#define MATH_CGINC

float normalize(float v, float min, float max) {
    return clamp((v - min) / (max - min), 0.0, 1.0);
}

#endif
