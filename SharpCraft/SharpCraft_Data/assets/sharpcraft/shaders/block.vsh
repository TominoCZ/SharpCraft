#version 330

const float density = 0.05;
const float gradient = 4;

in vec3 position;
in vec3 normal;
in vec2 textureCoords;

out vec2 pass_uv;
out float visibility;
out float brightness;

uniform mat4 transformationMatrix;
uniform mat4 projectionMatrix;
uniform mat4 viewMatrix;
uniform float fogDistance;

void main(void) {
	vec4 worldPos = transformationMatrix * vec4(position, 1.0);
	gl_Position = projectionMatrix * viewMatrix * worldPos;
	
    vec4 positionRelativeToCam = viewMatrix * worldPos;
	
	float distance = length(positionRelativeToCam.xyz) * 2.5f / fogDistance;
    visibility = exp(-pow((distance*density), gradient));
    visibility = clamp(visibility, 0.0, 1.0);
	
	pass_uv = textureCoords;

	float lightY = 0.875;

	vec3 vector1 = vec3(1, lightY, 0);
	vec3 vector2 = vec3(-1, lightY, 0.15);

	vec3 vector3 = vec3(0, lightY, 1);
	vec3 vector4 = vec3(0.15, lightY, -1);

	vec3 light1 = normalize(vector1) * 1.5;
	vec3 light2 = normalize(vector2) * 1.35;
	vec3 light3 = normalize(vector3) * 0.75;
	vec3 light4 = normalize(vector4) * 0.75;

	vec3 unitNormal = normalize((transformationMatrix * vec4(normal, 0.0)).xyz);

	float diffuse_value1 = max(dot(unitNormal, light1), 0.55);
	float diffuse_value2 = max(dot(unitNormal, light2), 0.45);
	float diffuse_value3 = max(dot(unitNormal, light3), 0.25);
	float diffuse_value4 = max(dot(unitNormal, light4), 0.25);

	brightness = (diffuse_value1 + diffuse_value2 + diffuse_value3 + diffuse_value4) / 2.15;
}