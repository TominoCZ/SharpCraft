#version 400 core

in vec2 position;
in vec2 textureCoords;

out vec2 pass_textureCoords;

uniform mat4 transformationMatrix;

void main(void) {
	gl_Position = transformationMatrix * vec4(position, 0.0, 1.0);
	pass_textureCoords = textureCoords;
	//textureCoords = vec2((position.x+1.0)/2.0, 1 - (position.y+1.0)/2.0);
}