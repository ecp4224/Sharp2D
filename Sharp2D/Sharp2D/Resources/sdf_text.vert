#version 330 core
layout(location = 0) in vec2 a_position;
layout(location = 1) in vec2 a_uv;
uniform float screenRatioFix;   // same as sprites
uniform float spriteDepth;      // depth for text layer
uniform vec3  camPosAndScale;   // camera (xy) and zoom (z)
out vec2 v_uv;
void main()
{
    vec2 preCam = a_position;         // already in pixels
    preCam += camPosAndScale.xy;       // pan
    preCam *= camPosAndScale.z;        // zoom
    preCam.x /= screenRatioFix;        // aspect fix
    v_uv = a_uv;
    gl_Position = vec4(preCam, spriteDepth, 1.0);
}
