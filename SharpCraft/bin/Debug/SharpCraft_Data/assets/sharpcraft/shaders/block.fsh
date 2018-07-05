#version 330

in vec3 skycolor;
in vec2 pass_uv;
in float visibility;
in float brightness;

out vec4 out_Color;

uniform sampler2D textureSampler;

void main(void){
	vec4 pixelColor = texture(textureSampler, pass_uv);

	if(pixelColor.a == 0)discard;

	vec3 diffuse = vec3(brightness);
	
	vec4 color = vec4(diffuse, 1.0) * pixelColor;
	
	out_Color = mix(vec4(skycolor, 1.0), color, visibility);
}