#version 330 core

layout (location = 0) in vec3 vPos;
layout (location = 1) in vec2 vUv;
layout (location = 2) in mat4 vModel;
layout (location = 6) in vec4 vCol;

uniform mat4 vp;
out vec2 fUv;
out vec4 fCol;

void main()
{
    mat4 mvp = vp * vModel;
    gl_Position = mvp * vec4(vPos, 1.0);
    fUv = vUv;
    fCol = vCol;
}