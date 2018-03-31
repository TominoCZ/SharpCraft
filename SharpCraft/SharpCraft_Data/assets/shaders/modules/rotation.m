#version 330

mat4 rotationMatrix(float angle)
{
    float s = sin(angle);
    float c = cos(angle);
    float oc = 1.0 - c;
    
    return mat4(c,                         0, axis.y * s, 0,
                0, oc * axis.y * axis.y +  c,          0, 0,
                -axis.y * s,            0, c,          0,
                0                        , 0,          0, 1);
}