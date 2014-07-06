#version 150

uniform vec3 lightdata; //for now rg is pos, b is radius
uniform vec3 lightcolor; //for color..

in vec2 fragtexcoord;

in vec2 pixpos;
uniform sampler2D texture0; //only one texture for now, we can get creative later

out vec4 fragColor;

void main(){
		vec3 difcolor = texture(texture0, fragtexcoord).rgb;
		
		
		vec2 lightpos = lightdata.xy;
		vec2 deltapos = lightpos - pixpos;
		float lightdist = length(deltapos);
		float lsize = lightdata.b;
		float attenuation = clamp(1.0 - lightdist*lightdist/(lsize*lsize), 0.0, 1.0); 
		attenuation *= attenuation;
		//attenuation = 1.0;
		
		fragColor.rgb = attenuation * lightcolor * difcolor;
		fragColor.a = 1.0;
//		fragColor = vec4(1.0);
}