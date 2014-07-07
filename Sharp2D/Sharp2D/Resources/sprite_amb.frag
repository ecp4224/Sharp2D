#version 150

uniform vec3 brightness;


in vec2 fragtexcoord;

in vec2 pixpos;
uniform sampler2D texture0; //only one texture for now, we can get creative later

out vec4 fragColor;

void main(){
		vec4 difcolor = texture(texture0, fragtexcoord);
		
		fragColor.rgb = brightness * difcolor.rgb;
		fragColor.a = difcolor.a;
//		fragColor = vec4(1.0);
}