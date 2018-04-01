#version 330

in vec2 pass_uv;
in float brightness;

out vec4 out_Color;

uniform sampler2D textureSampler;
uniform float alpha;

void main(void){
	vec4 pixelColor = texture(textureSampler, pass_uv);
	
	if(pixelColor.a == 0)discard;
	
	pixelColor.a *= alpha;
	
	vec3 diffuse = vec3(brightness * 1.65);
	
	out_Color = vec4(diffuse, 1.0) * pixelColor;
}