#version 330

in vec2 position;

out vec2 pass_uv;

uniform mat4 transformationMatrix;
uniform vec2 textureCoords;

void main(void) {
	gl_Position = transformationMatrix * vec4(position, 0.0, 1.0);
	pass_uv = textureCoords;
}