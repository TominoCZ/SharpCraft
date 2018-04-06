#version 330

in vec2 pass_uv;

out vec4 out_Color;

uniform sampler2D guiTexture;

void main(void){
	vec4 color = texture(guiTexture, pass_uv);
	if (color.a <= 0.1) discard;
	
	out_Color = color;
}