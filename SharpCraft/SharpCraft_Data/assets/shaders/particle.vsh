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

float calcLight(in vec3 lightDir)
{
	vec3 _normal = normalize((transformationMatrix * vec4(normal, 0)).xyz);

	//vec3 fragPosition = vec3(transformationMatrix * vec4(position, 0));
	//vec3 surfaceToLight = normalize(lightPos - position);

	return max(dot(_normal, lightDir), 0.1);
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

	vec3 light1 = normalize(vec3(700, 1000, 750));
	vec3 light2 =  normalize(vec3(-700, -800, -750));
	
	vec3 light3 = normalize(vec3(-700, 900, -750));
	vec3 light4 =  normalize(vec3(700, -700, 750));

	brightness = (calcLight(normalize(vec3(0.5,0.5,0.4))) + calcLight(normalize(vec3(-0.5,-0.3,-0.4))) * 1.2) * 1.25;
}