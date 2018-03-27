#version 330

in vec2 pass_textureCoords;

out vec4 out_Color;

uniform sampler2D guiTexture;

void main(void){
	vec4 color = texture(guiTexture,pass_textureCoords);
	
	if(color.a < 0.5)
		discard;
	
	out_Color = color;
}