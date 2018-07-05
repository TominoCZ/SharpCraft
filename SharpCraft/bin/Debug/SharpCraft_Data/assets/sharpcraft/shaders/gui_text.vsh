#version 330

in vec2 position;

out vec2 pass_uv;
out vec4 color_pass;

uniform mat4 transformationMatrix;
uniform vec3 colorIn;
uniform vec2 UVmin;
uniform vec2 UVmax;

void main() {
	gl_Position = transformationMatrix * vec4(position, 0.0, 1.0);

	int i = gl_VertexID;

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

	color_pass = vec4(colorIn, 1);
}