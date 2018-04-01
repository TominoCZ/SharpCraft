#version 330

in vec3 position;

out vec3 pass_uv;

uniform mat4 projectionMatrix;
uniform mat4 viewMatrix;

void main(void) {
	gl_Position = projectionMatrix * viewMatrix * vec4(position, 1.0);

	pass_uv = position;
}