#version 330

in vec2 pass_textureCoords;
in vec3 unitNormal;
in vec3 light1;
in vec3 light2;

out vec4 out_Color;

uniform sampler2D textureSampler;
uniform vec3 lightColor;

void main(void){
	vec4 pixelColor = texture(textureSampler, pass_textureCoords);
	
	if(pixelColor.a == 0)discard;
	
	float diffuse_value1 = max(dot(unitNormal, light1), 0.25);
	float diffuse_value2 = max(dot(unitNormal, light2), 0.25);

	float brightness = diffuse_value1 * 0.65 + diffuse_value2 * 0.35;
	
	vec3 diffuse = brightness * 2 * lightColor;
	
	out_Color = vec4(diffuse, 1.0) * pixelColor;
}