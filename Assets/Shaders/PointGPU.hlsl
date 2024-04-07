#if defined(UNITY_PROCEDURAL_INSTANCING_ENABLED)
    StructuredBuffer<float3> particlePositions;
#endif

float _Radius;

void ConfigureProcedural () 
{
    #if defined(UNITY_PROCEDURAL_INSTANCING_ENABLED)
    float3 position = particlePositions[unity_InstanceID];

    unity_ObjectToWorld = 0.0;
    unity_ObjectToWorld._m03_m13_m23_m33 = float4(position, 1.0);
    unity_ObjectToWorld._m00_m11_m22 = _Radius;
    #endif
}

void ShaderGraphFunction_float (float3 In, out float3 Out) {
	Out = In;
}

void ShaderGraphFunction_half (half3 In, out half3 Out) {
	Out = In;
}