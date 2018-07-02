#version 330

in vec3 pass_uv;

out vec4 out_Color;

uniform samplerCube cubeMap;

void main(void){
	out_Color = texture(cubeMap, pass_uv);
}