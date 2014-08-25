#version 150

uniform float screenRatioFix;
uniform float spriteDepth;
uniform float spriteScale;
uniform vec4 spritePos;
uniform vec4 texCoordPosAndScale;
uniform vec3 camPosAndScale;

out vec2 pixpos;



in vec2 posattrib; // same name as your attrib calls in the shader code
in vec2 tcattrib;

out vec2 fragtexcoord; // output texcoord for fragment shader.. this is where we move it over to the right place

void main(){

	vec2 preCamPlace;
	preCamPlace = (spritePos.xy) + (spriteScale*posattrib*spritePos.zw); // sprite to world coords

	pixpos = preCamPlace; // world coords of pixel
	preCamPlace += camPosAndScale.xy; // now world to camera coords
	preCamPlace *= camPosAndScale.z;

	preCamPlace.x /= screenRatioFix;

	fragtexcoord = texCoordPosAndScale.xy - (tcattrib * texCoordPosAndScale.zw);
	gl_Position = vec4(preCamPlace, spriteDepth, 1.0);
}