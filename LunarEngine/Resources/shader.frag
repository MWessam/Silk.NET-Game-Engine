#version 330 core
in vec2 fUv;
in vec4 fCol;
  
out vec4 FragColor;

uniform sampler2D uTexture;

void main()
{
//    FragColor = pow(texColor * fCol, vec4(10));
    vec4 texColor = texture(uTexture, fUv);
    FragColor = texColor * fCol;
}