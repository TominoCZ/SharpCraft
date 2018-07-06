#version 330

in vec3 skycolor;
in vec2 pass_uv;
in float visibility;
in float brightness;

out vec4 out_Color;

uniform sampler2D textureSampler;

void main(void){
	vec4 texturePixelColor = texture(textureSampler, pass_uv);

	if(texturePixelColor.a == 0)discard;
	
	texturePixelColor = vec4(brightness * texturePixelColor.rgb, texturePixelColor.a);
	
	out_Color = mix(vec4(skycolor, 1.0), texturePixelColor, visibility);
}