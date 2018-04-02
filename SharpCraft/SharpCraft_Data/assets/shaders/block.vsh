#version 330

in vec3 position;
in vec2 textureCoords;
in vec3 normal;

out vec2 pass_uv;
out float brightness;

uniform mat4 transformationMatrix;
uniform mat4 projectionMatrix;
uniform mat4 viewMatrix;

void main(void) {
	vec4 worldPos = transformationMatrix * vec4(position, 1.0);
	gl_Position = projectionMatrix * viewMatrix * worldPos;

	pass_uv = textureCoords;
	
	vec3 vector1 = vec3(1, 1.5, 1.2);
	//vec3 vector2 = vec3(-vector1.x - 0.5, -0.85, -vector1.z);
	
	vec3 light1 = normalize(vector1) * 1.5;
	//vec3 light2 = normalize(vector2) * 1.2;
	
	vec3 unitNormal = normalize((transformationMatrix * vec4(normal, 0.0)).xyz);
	
	float diffuse_value1 = max(dot(unitNormal, light1), 0.55);
	//float diffuse_value2 = max(dot(unitNormal, light2), 0.45);

	brightness = diffuse_value1;//(diffuse_value1 + diffuse_value2) / 1.2;
}