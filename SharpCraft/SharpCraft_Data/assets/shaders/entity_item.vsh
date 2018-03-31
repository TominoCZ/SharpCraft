#version 330
#extension GL_ARB_shading_language_include : require
#include <modules/rotation.m>

in vec3 position;
in vec2 textureCoords;
in vec3 normal;

out vec2 pass_textureCoords;
out vec3 surfaceNormal;

uniform mat4 projectionMatrix;
uniform mat4 viewMatrix;
uniform float time;

void main(void) {
	mat4 rotation = rotationMatrix(time * PI * 2);
	vec4 worldPos = rotation * vec4(position, 1.0);
	gl_Position = projectionMatrix * viewMatrix * worldPos;

	pass_textureCoords = textureCoords;
	surfaceNormal = (rotation * vec4(normal, 0.0)).xyz;
}