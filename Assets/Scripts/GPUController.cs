using UnityEngine;

public class GPUController : MonoBehaviour {

    ComputeBuffer positionsBuffer;
    ComputeBuffer particle0;
    ComputeBuffer particle1;

    [SerializeField]
	Material material;

	[SerializeField]
	Mesh mesh;

    [SerializeField]
    ComputeShader particleShader;

    [SerializeField, Range(2, 200)]
	int nParticles = 10;

    [SerializeField, Range(0,1)]
    float timeStep;

    [SerializeField, Range(1,50)]
    int particleMass;

    [SerializeField, Range(0, 3.0f)]
    float stiffnessCoefficient = 0.01f;

    [SerializeField, Range(0, 3.0f)]
    float viscosityCoefficient = 0.1f;

    [SerializeField, Range(0, 3.0f)]
    float interactionRadius = 0.1f;

    [SerializeField, Range(0, 3.0f)]
    float restDensity = 0.1f;


    float boxSize = 5;
    float boxCoeff = 0.001f;

    static readonly int
		positionsId = Shader.PropertyToID("particlePositions"),
		nParticlesID = Shader.PropertyToID("nParticles"),
		stepId = Shader.PropertyToID("step"),
        frameID = Shader.PropertyToID("frame"),
        gravityID = Shader.PropertyToID("gravityVector"),
        boxSizeID = Shader.PropertyToID("BOX_SCALE"),
        boxCoeffID = Shader.PropertyToID("BOX_INFLUENCE"),
        WPolyhID = Shader.PropertyToID("WPolyh"),
        WSpikyhID = Shader.PropertyToID("WSpikyh"),
        WVischID = Shader.PropertyToID("WVisch"),
        timeStepID = Shader.PropertyToID("timeStep"),
        particleMassID = Shader.PropertyToID("particleMass"),
		timeId = Shader.PropertyToID("time");

    int frame = 0;    

    int ParticleIntegrationKernel;
    int ParticleDensityKernel;

    void Integrate()
    {
        particleShader.SetBuffer(0, "particleRenderPositions", positionsBuffer);
        particleShader.SetBuffer(0, "particleRead", frame % 2 == 0 ? particle0 : particle1);
        particleShader.SetBuffer(0, "particleWrite", frame % 2 == 0 ? particle1 : particle0);
        int nGroups = Mathf.CeilToInt(nParticles / 64.0f);
		particleShader.Dispatch(ParticleIntegrationKernel, nGroups, 1, 1);

        frame++;
    }

    void ComputeDensity()
    {
        particleShader.SetBuffer(1, "particleRead", frame % 2 == 0 ? particle0 : particle1);
        particleShader.SetBuffer(1, "particleWrite", frame % 2 == 0 ? particle1 : particle0);
        int nGroups = Mathf.CeilToInt(nParticles / 64.0f);

        particleShader.Dispatch(ParticleDensityKernel, nGroups, 1, 1);

        frame++;
    }




    void UpdateTestShader()
    {
        float step = 10f / nParticles;
        particleShader.SetFloat(stepId, step);
        particleShader.SetFloat(timeId, Time.time);
        particleShader.SetInt(nParticlesID, nParticles);
        particleShader.SetInt(frameID, frame);
        particleShader.SetVector(gravityID, Vector3.down);
        particleShader.SetFloat(timeStepID, timeStep);
        particleShader.SetFloat("interactionRadius", interactionRadius);
        particleShader.SetFloat("stiffnessCoefficient", stiffnessCoefficient);
        particleShader.SetFloat("viscosityCoefficient", viscosityCoefficient);
        particleShader.SetFloat("restDensity", restDensity);
        particleShader.SetInt(particleMassID, particleMass);

        Integrate();
        ComputeDensity();

        material.SetFloat(stepId, step);
        material.SetBuffer(positionsId, positionsBuffer);

        var bounds = new Bounds(Vector3.zero, Vector3.one * (boxSize + boxSize / nParticles));
        Graphics.DrawMeshInstancedProcedural(mesh, 0, material, bounds, positionsBuffer.count);
    }  

    void OnDrawGizmos()
    {
        var bounds = Vector3.one * (boxSize + boxSize / nParticles);
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(Vector3.zero, bounds);
    }  

    void OnEnable() {
		positionsBuffer = new ComputeBuffer(nParticles, 3 * sizeof(float));
        particle0 = new ComputeBuffer(nParticles, 8 * sizeof(float));
        particle1 = new ComputeBuffer(nParticles, 8 * sizeof(float));

        ParticleIntegrationKernel = particleShader.FindKernel("ParticleIntegration");
        ParticleDensityKernel = particleShader.FindKernel("ParticleDensity");
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
