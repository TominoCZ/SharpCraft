#version 330

in vec2 pass_textureCoords;
in float brightness;

out vec4 out_Color;

uniform sampler2D textureSampler;

void main(void){
	vec4 pixelColor = texture(textureSampler, pass_textureCoords);
	
	if(pixelColor.a == 0)discard;
	
	vec3 diffuse = brightness;
	
	out_Color = vec4(diffuse, 1.0) * pixelColor;
}