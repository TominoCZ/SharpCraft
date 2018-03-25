#version 400 core

in vec2 pass_textureCoords;

out vec4 out_Color;

uniform sampler2D textureSampler;

void main(void){
	vec4 pixelColor = texture(textureSampler, pass_textureCoords);
	if(pixelColor.a==0)discard;
	out_Color = pixelColor;
}