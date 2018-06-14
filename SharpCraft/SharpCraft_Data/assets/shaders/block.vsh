#version 330

in vec3 position;
in vec2 textureCoords;
in vec3 normal;

out vec2 pass_uv;
out float brightness;

uniform mat4 transformationMatrix;
uniform mat4 projectionMatrix;
uniform mat4 viewMatrix;

float calcLight(in vec3 lightPos)
{
	vec3 _normal = normalize((transformationMatrix * vec4(normal, 0)).xyz);

	vec3 fragPosition = vec3(transformationMatrix * vec4(position, 1));
	vec3 surfaceToLight = normalize(lightPos - position);

	return abs(dot(_normal, surfaceToLight)) + 0.5;
}

void main(void) {
	vec4 worldPos = transformationMatrix * vec4(position, 1.0);
	gl_Position = projectionMatrix * viewMatrix * worldPos;
	
	pass_uv = textureCoords;
	
	vec3 light1 = vec3(10, 100, 100)*100;
	vec3 light2 = vec3(-50, -100, -5)*100;

	brightness = (calcLight(light1) + calcLight(light2)) / 2;
}