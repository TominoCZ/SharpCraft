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

void main(void) {
	vec4 worldPos = transformationMatrix * vec4(position, 1.0);
	gl_Position = projectionMatrix * viewMatrix * worldPos;

	int i = gl_VertexID % 4;
	
	if (i == 0){
		pass_uv = UVmin;
	}
	else if (i == 1){
		pass_uv = vec2(UVmin.x, UVmax.y);
	}
	else if (i == 2){
		pass_uv = UVmax;
	}
	else if (i == 3){
		pass_uv = vec2(UVmax.x, UVmin.y);
	}
	
	float lightY = 0.85;
	
	vec3 vector1 = vec3(1, lightY, 0);
	vec3 vector2 = vec3(-1, lightY, 0.15);
	
	vec3 vector3 = vec3(0, -lightY, 1);
	vec3 vector4 = vec3(0.15, -lightY, -1);
	
	vec3 light1 = normalize(vector1) * 1.5;
	vec3 light2 = normalize(vector2) * 1.35;
	vec3 light3 = normalize(vector3) * 0.75;
	vec3 light4 = normalize(vector4) * 0.5;
	
	vec3 unitNormal = normalize((transformationMatrix * vec4(normal, 0.0)).xyz);
	
	float diffuse_value1 = max(dot(unitNormal, light1), 0.55);
	float diffuse_value2 = max(dot(unitNormal, light2), 0.45);
	float diffuse_value3 = max(dot(unitNormal, light3), 0.35);
	float diffuse_value4 = max(dot(unitNormal, light4), 0.25);
	
	brightness = (diffuse_value1 + diffuse_value2 + diffuse_value3 + diffuse_value4) / 3;
}