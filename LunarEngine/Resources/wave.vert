#version 330 core

layout (location = 0) in vec3 vPos;
layout (location = 1) in vec2 vUv;
layout (location = 2) in mat4 vModel;
layout (location = 6) in vec4 vCol;

uniform mat4 vp;
uniform float rng;
out vec2 fUv;
out vec4 fCol;

void main()
{
    mat4 mvp = vp * vModel;
    vec3 modPos = vec3(vPos.x + cos(rng * (gl_VertexID + 1)), vPos.y + sin(rng * (gl_VertexID + 1)), 0.0);
    gl_Position = mvp * vec4(modPos, 1.0);
    fUv = vUv;
    fCol = vCol;
}