using UnityEngine;

public class GPUController : MonoBehaviour {

    ComputeBuffer positionsBuffer;

    [SerializeField]
	Material material;

	[SerializeField]
	Mesh mesh;

    [SerializeField]
    ComputeShader particleShader;

    ComputeBuffer particle0;
    ComputeBuffer particle1;

    int frame = 0;

    static readonly int
		positionsId = Shader.PropertyToID("particlePositions"),
		nXId = Shader.PropertyToID("nParticles"),
        nYId = Shader.PropertyToID("nParticlesY"),
		stepId = Shader.PropertyToID("step"),
		timeId = Shader.PropertyToID("time");

    void UpdateTestShader()
    {
        float step = 4f / nParticlesX;
        particleShader.SetFloat(stepId, step);
        particleShader.SetFloat(timeId, Time.time);
        particleShader.SetInt(nXId, nParticlesX);
        particleShader.SetInt(nYId, nParticlesY);
        particleShader.SetInt("frame", frame);

        particleShader.SetBuffer(0, positionsId, positionsBuffer);
        particleShader.SetBuffer(0, "particleRead", frame % 2 == 0 ? particle0 : particle1);
        particleShader.SetBuffer(0, "particleWrite", frame % 2 == 0 ? particle1 : particle0);
        int groupsX = Mathf.CeilToInt(nParticlesX / 64.0f);
		particleShader.Dispatch(0, groupsX, 1, 1);

        material.SetFloat(stepId, step);
        material.SetBuffer(positionsId, positionsBuffer);

        var bounds = new Bounds(Vector3.zero, Vector3.one * (2f + 2f / Mathf.Min(nParticlesX, nParticlesY)));
        Graphics.DrawMeshInstancedProcedural(mesh, 0, material, bounds, positionsBuffer.count);

        frame++;
    }    

	[SerializeField, Range(10, 200)]
	int nParticlesX = 10;

    [SerializeField, Range(10, 200)]
	int nParticlesY = 10;

    void OnEnable() {
		positionsBuffer = new ComputeBuffer(nParticlesX, 3 * 4);
        particle0 = new ComputeBuffer(nParticlesX, 4 * 8);
        particle1 = new ComputeBuffer(nParticlesX, 4 * 8);
	}

    void OnDisable()
    {
        positionsBuffer.Release();
        positionsBuffer = null;

        particle0.Release();
        particle0 = null;

        particle1.Release();
        particle1 = null;
    }

	void Update () 
    {
        UpdateTestShader();
    }
}
