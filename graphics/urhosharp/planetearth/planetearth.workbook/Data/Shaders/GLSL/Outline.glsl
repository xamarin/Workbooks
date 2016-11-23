#include "Uniforms.glsl"
#include "Samplers.glsl"
#include "Transform.glsl"
#include "ScreenPos.glsl"

varying vec2 vTexCoord;
varying vec2 vScreenPos;

#ifdef COMPILEPS
    uniform vec4 cOutlineColor;
    uniform vec2 cOutlineBlurredMaskHInvSize;
#endif

void VS()
{
    mat4 modelMatrix = iModelMatrix;
    vec3 worldPos = GetWorldPos(modelMatrix);
    gl_Position = GetClipPos(worldPos);
    vTexCoord = GetQuadTexCoord(gl_Position);
    vScreenPos = GetScreenPosPreDiv(gl_Position);
}

void PS()
{
    #ifdef MASK
        gl_FragColor = vec4(cOutlineColor.rgb, 1.0);
    #endif

    #ifdef BLURH
        vec4 rgba = texture2D(sDiffMap, vTexCoord + vec2(0.0, 0.0) * cOutlineBlurredMaskHInvSize)
                  + texture2D(sDiffMap, vTexCoord + vec2(-1.0, 0.0) * cOutlineBlurredMaskHInvSize)
                  + texture2D(sDiffMap, vTexCoord + vec2(1.0, 0.0) * cOutlineBlurredMaskHInvSize)
                  + texture2D(sDiffMap, vTexCoord + vec2(-2.0, 0.0) * cOutlineBlurredMaskHInvSize)
                  + texture2D(sDiffMap, vTexCoord + vec2(2.0, 0.0) * cOutlineBlurredMaskHInvSize);
        gl_FragColor = rgba * 0.2;
    #endif

    #ifdef BLURV
        vec4 rgba = texture2D(sDiffMap, vTexCoord + vec2(0.0, 0.0) * cOutlineBlurredMaskHInvSize)
                  + texture2D(sDiffMap, vTexCoord + vec2(0.0, -1.0) * cOutlineBlurredMaskHInvSize)
                  + texture2D(sDiffMap, vTexCoord + vec2(0.0, 1.0) * cOutlineBlurredMaskHInvSize)
                  + texture2D(sDiffMap, vTexCoord + vec2(0.0, -2.0) * cOutlineBlurredMaskHInvSize)
                  + texture2D(sDiffMap, vTexCoord + vec2(0.0, 2.0) * cOutlineBlurredMaskHInvSize);
        gl_FragColor = rgba * 0.2;
    #endif

    #ifdef OUTPUT
        vec4 blurredMask = texture2D(sDiffMap, vTexCoord);
        vec4 mask = texture2D(sNormalMap, vTexCoord);
        vec4 viewport = texture2D(sSpecMap, vTexCoord);
        blurredMask = clamp(blurredMask - mask.a, 0.0, 1.0);
        blurredMask *= 3.0;
        gl_FragColor = viewport * (1.0 - blurredMask.a) + blurredMask;
    #endif
}