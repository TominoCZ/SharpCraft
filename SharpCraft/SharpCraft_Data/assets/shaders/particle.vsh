#version 330

in vec3 position;
in vec2 textureCoords;
in vec3 normal;

out vec2 pass_uv;
out float brightness;

uniform mat4 transformationMatrix;
uniform mat4 projectionMatrix;
uniform mat4 viewMatrix;
uniform vec2 UVmin;
uniform vec2 UVmax;

float calcLight(in vec3 lightPos)
{
	vec3 _normal = normalize((transformationMatrix * vec4(normal, 0)).xyz);

	vec3 fragPosition = vec3(transformationMatrix * vec4(position, 1));
	vec3 surfaceToLight = normalize(lightPos - position);

	return clamp(dot(_normal, surfaceToLight), 0.25, 1);
}

void main(void)
{
	vec4 worldPos = transformationMatrix * vec4(position, 1);
	gl_Position = projectionMatrix * viewMatrix * worldPos;

	int i = gl_VertexID % 4;

	if (i == 0) {
		pass_uv = UVmin;
	}
	else if (i == 1) {
		pass_uv = vec2(UVmin.x, UVmax.y);
	}
	else if (i == 2) {
		pass_uv = UVmax;
	}
	else if (i == 3) {
		pass_uv = vec2(UVmax.x, UVmin.y);
	}

	vec3 light1 = vec3(7000, 20000, 500);
	vec3 light2 = vec3(-7000, -10000, -500);

	brightness = (calcLight(light1) + calcLight(light2)) * 1.3;
}