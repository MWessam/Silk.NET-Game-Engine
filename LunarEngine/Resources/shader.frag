#version 330 core
in vec2 fUv;
in vec4 fCol;
  
out vec4 FragColor;

uniform sampler2D uTexture;

void main()
{
    vec4 texColor = texture(uTexture, fUv);
    //FragColor = power(2, 1 + texColor * fCol);
    FragColor = texColor * fCol;
}