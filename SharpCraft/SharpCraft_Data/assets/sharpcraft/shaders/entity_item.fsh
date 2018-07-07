#version 330

in vec2 pass_uv;
in float brightness;

out vec4 out_Color;

uniform sampler2D textureSampler;

void main(void){
	vec4 texturePixelColor = texture(textureSampler, pass_uv);
	
	if(texturePixelColor.a == 0)discard;
	
	texturePixelColor.xyz *= brightness * 2;
	
	out_Color = texturePixelColor;
}