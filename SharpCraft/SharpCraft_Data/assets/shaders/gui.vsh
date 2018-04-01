#version 330

in vec2 position;

out vec2 pass_uv;

uniform mat4 transformationMatrix;

void main() {
	gl_Position = transformationMatrix * vec4(position, 0.0, 1.0);
	pass_uv = vec2((position.x + 1.0) / 2.0, 1 - (position.y + 1.0) / 2.0);
}