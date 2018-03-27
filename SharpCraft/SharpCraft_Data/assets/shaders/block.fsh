#version 330

in vec2 pass_textureCoords;
in vec3 surfaceNormal;

out vec4 out_Color;

uniform sampler2D textureSampler;
uniform vec3 lightColor;

void main(void){
	vec4 pixelColor = texture(textureSampler, pass_textureCoords);
	if(pixelColor.a == 0)discard;

	vec3 unitNormal = normalize(surfaceNormal);
	vec3 vector1 = normalize(vec3(300, 700, 375));
	vec3 vector2 = normalize(vec3(-375, -200, -300));
	
	float nDot1 = dot(unitNormal, vector1);
	float nDot2 = dot(unitNormal, vector2);

	float brightness = max(nDot1, 0.0) + max(nDot2, 0.0) * 0.75;
	
	vec3 diffuse = brightness * 1.5 * lightColor;
	
	out_Color = vec4(diffuse, 1.0) * pixelColor;
}