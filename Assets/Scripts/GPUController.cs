using UnityEngine;

public class GPUController : MonoBehaviour {

    ComputeBuffer positionsBuffer;

    [SerializeField]
	Material material;

	[SerializeField]
	Mesh mesh;

    [SerializeField]
    ComputeShader particleShader;

    static readonly int
		positionsId = Shader.PropertyToID("particlePositions"),
		nXId = Shader.PropertyToID("nParticlesX"),
        nYId = Shader.PropertyToID("nParticlesY"),
		stepId = Shader.PropertyToID("step"),
		timeId = Shader.PropertyToID("time");

    void UpdateTestShader()
    {
        float step = 2f / nParticlesX;
        particleShader.SetFloat(stepId, step);
        particleShader.SetFloat(timeId, Time.time);
        particleShader.SetInt(nXId, nParticlesX);
        particleShader.SetInt(nYId, nParticlesY);

        particleShader.SetBuffer(0, positionsId, positionsBuffer);
        int groupsX = Mathf.CeilToInt(nParticlesX / 8f);
        int groupsY = Mathf.CeilToInt(nParticlesY / 8f);
		particleShader.Dispatch(0, groupsX, groupsY, 1);

        material.SetBuffer(positionsId, positionsBuffer);
		material.SetFloat(stepId, step);

        var bounds = new Bounds(Vector3.zero, Vector3.right * (2 + 2/nParticlesX) + Vector3.up * (2 + 2/nParticlesY));
        Graphics.DrawMeshInstancedProcedural(mesh, 0, material, bounds, positionsBuffer.count);

        Debug.Log("Calculating stuff!");
    }    

	[SerializeField, Range(10, 200)]
	int nParticlesX = 10;

    [SerializeField, Range(10, 200)]
	int nParticlesY = 10;

    void Awake () {
		positionsBuffer = new ComputeBuffer(nParticlesX, 2 * 4);
	}
    void OnEnable() {
		positionsBuffer = new ComputeBuffer(nParticlesY, 2 * 4);
	}

    void OnDisable()
    {
        positionsBuffer.Release();
        positionsBuffer = null;
    }

	void Update () 
    {
        UpdateTestShader();
    }
}
