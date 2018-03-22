#version 400 core

in vec2 pass_textureCoords;
in vec3 surfaceNormal;
in vec3 toLightVector;

out vec4 out_Color;

uniform sampler2D textureSampler;
uniform vec3 lightColor;

void main(void){
	vec4 pixelColor = texture(textureSampler, pass_textureCoords);
	if(pixelColor.a < 0.5)discard;

	vec3 unitNormal = normalize(surfaceNormal);
	vec3 unitNormalLightVector = normalize(toLightVector);
	
	float nDot1 = dot(unitNormal, unitNormalLightVector);
	float nDot2 = dot(unitNormal, vec3(-unitNormalLightVector.z, unitNormalLightVector.y, -unitNormalLightVector.x));

	float brightness1 = 0.2 + (max(nDot1, 0.0) * 0.5);
	float brightness2 = (max(nDot2, 0.0) * 0.6);
	
	float final = (brightness1 + brightness2);
	
	vec3 diffuse = final * lightColor;
	
	out_Color = vec4(diffuse, 1.0) * pixelColor;
}