#version 330

in vec2 pass_uv;
in vec4 color_pass;

out vec4 out_Color;

uniform sampler2D guiTexture;

void main(void){
	vec4 pixel = texture(guiTexture, pass_uv);
	
	pixel.a = pixel.r;
	
	if (pixel.a <= 0.05)discard;
	
	out_Color = pixel * color_pass;
}