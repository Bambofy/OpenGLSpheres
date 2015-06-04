#version 330

in vec4 vpos;       // model space
in vec4 vnormal;    // view space

uniform vec4 colour;
uniform vec2 resolution;
uniform vec3 lightPos;
uniform vec3 lightColour;

out vec4 out_color;

void main()
{
    vec4 LightPos = vec4(lightPos.xyz, 1.0);
    vec4 diffuseColor = vec4(colour.xyz, 1.0);

    vec4 LightDir = LightPos - vpos;
    LightDir.x *= resolution.x/resolution.y;

    float D = length(LightDir);
    vec4 L = normalize(LightDir);

    vec4 Diffuse = vec4(lightColour, 1.0) * max(dot(vnormal, L), 0.0);

    out_color = diffuseColor + Diffuse;
}
