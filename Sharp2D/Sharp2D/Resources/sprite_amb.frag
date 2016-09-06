#version 150

uniform vec3 brightness;
uniform vec4 tint;

in vec2 fragtexcoord;

in vec2 pixpos;
uniform sampler2D texture0; //only one texture for now, we can get creative later

out vec4 fragColor;

void main(){
		vec4 difcolor = texture(texture0, fragtexcoord);
		float real_alpha = clamp(difcolor.a - tint.a, 0.0, 1.0);
		if(real_alpha <= 0.0) discard;
		
		fragColor.rgb = brightness * difcolor.rgb;
		fragColor.rgb *= tint.rgb;
		fragColor.a = real_alpha;
}