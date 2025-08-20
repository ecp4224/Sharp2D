#version 330 core
in vec2 v_uv;
out vec4 fragColor;
uniform sampler2D u_texture;
uniform vec4 u_textColor;
uniform float u_threshold;
uniform float u_smoothing;
void main()
{
    float dist = texture(u_texture, v_uv).r;
    float alpha = smoothstep(u_threshold - u_smoothing, u_threshold + u_smoothing, dist);
    fragColor = vec4(u_textColor.rgb, u_textColor.a * alpha);
}
