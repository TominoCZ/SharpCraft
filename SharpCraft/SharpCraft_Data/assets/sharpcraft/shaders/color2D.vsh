#version 330

in vec2 pos;

out vec4 pass_color;

uniform vec4 color;

void main(void) {
	gl_Position = vec4(pos.x, pos.y, -1.0, 1.0);

	pass_color = color;
}