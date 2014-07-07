#version 150

uniform vec3 lightdata; //for now rg is pos, b is radius, a is intensity
uniform vec3 lightcolor;
uniform vec3 ambient;
uniform float ambientmult;

in vec2 fragtexcoord;

in vec2 pixpos;
uniform sampler2D texture0; //only one texture for now, we can get creative later

out vec4 fragColor;

void main(){
		vec4 difcolor = texture(texture0, fragtexcoord);
		
		
		vec2 lightpos = lightdata.xy;
		vec2 deltapos = lightpos - pixpos;
		float lightdist = length(deltapos);
		float lsize = lightdata.b;
		float attenuation = clamp(1.0 - lightdist*lightdist/(lsize*lsize), 0.0, 1.0); 
		attenuation *= attenuation;
		
		
		vec3 lightout = attenuation * lightcolor;
		vec3 ambientout = ambient * ambientmult;
		
		fragColor.rgb = (lightout + ambientout) * difcolor.rgb;
		fragColor.a = ambientmult * difcolor.a;
//		fragColor = vec4(1.0);
}