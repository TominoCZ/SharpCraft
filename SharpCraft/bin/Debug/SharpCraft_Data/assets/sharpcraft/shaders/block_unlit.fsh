#version 330

in vec2 pass_uv;

out vec4 out_Color;

uniform sampler2D textureSampler;

void main(void){
	vec4 pixelColor = texture(textureSampler, pass_uv);
	if(pixelColor.a==0)discard;
	out_Color = pixelColor;
}