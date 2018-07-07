#version 330

const vec3 skycolor = vec3(0, 0.815, 1);

in vec2 pass_uv;
in float visibility;
in float brightness;

out vec4 out_Color;

uniform sampler2D textureSampler;

void main(void){
	vec4 texturePixelColor = texture(textureSampler, pass_uv);
	
	if(texturePixelColor.a == 0)discard;
	
	texturePixelColor.rgb *= brightness * 2;
	
	out_Color = mix(vec4(skycolor, 1.0), texturePixelColor, visibility);
}