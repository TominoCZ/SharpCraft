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
	mat3 normalMatrix = transpose(inverse(mat3(transformationMatrix)));
	vec3 _normal = normalize(normalMatrix * normal);

	//calculate the location of this fragment (pixel) in world coordinates
	vec3 fragPosition = vec3(transformationMatrix * vec4(position, 1));

	//calculate the vector from this pixels surface to the light source
	vec3 surfaceToLight = lightPos - position;

	//calculate the cosine of the angle of incidence
	float brightness = dot(normalize(_normal), normalize(surfaceToLight));// / (length(surfaceToLight) * length(_normal));
	return clamp(brightness, 0.25, 1);
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