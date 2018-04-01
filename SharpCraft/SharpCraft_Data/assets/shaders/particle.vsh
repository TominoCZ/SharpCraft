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
uniform vec2 UVmin;
uniform vec2 UVmax;

void main(void) {
	vec4 worldPos = transformationMatrix * vec4(position, 1.0);
	gl_Position = projectionMatrix * viewMatrix * worldPos;

	int i = gl_VertexID % 4;
	
	if (i == 0){
		pass_textureCoords = UVmin;
	}
	else if (i == 1){
		pass_textureCoords = vec2(UVmin.x, UVmax.y);
	}
	else if (i == 2){
		pass_textureCoords = UVmax;
	}
	else if (i == 3){
		pass_textureCoords = vec2(UVmax.x, UVmin.y);
	}
	
	vec3 vector1 = vec3(300, 650, 350);
	
	vec4 pos_view = viewMatrix * vec4(position, 1.0);
	
	light1 = normalize(vector1 - pos_view.xyz);
	light2 = normalize(-vector1 - pos_view.xyz);
	
	unitNormal = normalize((transformationMatrix * vec4(normal, 0.0)).xyz);
}