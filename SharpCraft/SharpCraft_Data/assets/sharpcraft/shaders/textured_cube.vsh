#version 330

in vec3 position;

out vec2 pass_uv;

uniform mat4 transformationMatrix;
uniform mat4 projectionMatrix;
uniform mat4 viewMatrix;
uniform vec2 UVmin;
uniform vec2 UVmax;

void main(void) {
	vec4 worldPos = transformationMatrix * vec4(position, 1.0);
	gl_Position = projectionMatrix * viewMatrix * worldPos;

	int i = gl_VertexID % 6;

	if (i == 0) {
		pass_uv = UVmin;
	}
	else if (i == 1 || i == 4) {
		pass_uv = vec2(UVmin.x, UVmax.y);
	}
	else if (i == 2 || i == 3) {
		pass_uv = vec2(UVmax.x, UVmin.y);
	}
	else if (i == 5) {
		pass_uv = UVmax;
	}
}