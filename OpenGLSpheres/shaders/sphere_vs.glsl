#version 330

layout (location = 0) in vec3 vertex_position;
layout (location = 1) in vec3 vertex_normal;

uniform mat4 proj;
uniform mat4 view;
uniform mat4 model;
uniform int isPickup;

out vec4 vpos;
out vec4 vnormal;
                         
// the normals given from the program is just vertex_position.normalized() so ignore.
void main()
{
    // view space
    vnormal = normalize(vec4(vertex_position, 1.0)); //vec4(vertex_normal, 0.0);

    // if it's a pickup then oscillate.
    vec4 offset = vec4(0, 0, 0, 0);
    if (isPickup == 1)
    {
        float length = 1;           // vertex_normal.x + (sin(ticker) / 10);
        offset = vnormal * length;
    }

    // apply the amplitude * length of sine value.
    vec4 newPosition = vec4(vertex_position + offset.xyz, 1.0);


    vnormal = normalize(newPosition);
    vpos = model * newPosition;

    gl_Position = proj * view * vpos;
}