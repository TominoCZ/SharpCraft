#version 330

in vec3 position;
in vec2 textureCoords;
in vec3 normal;

out vec2 pass_textureCoords;
out vec3 unitNormal;
out vec3 light1;
out vec3 light2;

uniform mat4 transformationMatrix;
uniform mat4 projectionMatrix;
uniform mat4 viewMatrix;

void main(void) {
	vec4 worldPos = transformationMatrix * vec4(position, 1.0);
	gl_Position = projectionMatrix * viewMatrix * worldPos;

	pass_textureCoords = textureCoords;
	
	vec3 vector1 = vec3(300, 650, 375);
	
	vec4 pos_view = viewMatrix * vec4(position, 1.0);
	
	light1 = normalize(vector1 - pos_view.xyz);
	light2 = -light1;
	
	unitNormal = normalize((transformationMatrix * vec4(normal, 0.0)).xyz);
}