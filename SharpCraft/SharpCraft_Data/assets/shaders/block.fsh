#version 330

in vec2 pass_uv;
in float brightness;

out vec4 out_Color;

uniform sampler2D textureSampler;
uniform vec3 lightColor;

void main(void){
	vec4 pixelColor = texture(textureSampler, pass_uv);

	if(pixelColor.a == 0)discard;

	vec3 diffuse = brightness * lightColor+1;
	
	out_Color = vec4(diffuse, 1.0) * pixelColor;
}