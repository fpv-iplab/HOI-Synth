using System;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Perception.Randomization.Randomizers;
using UnityEngine.Perception.Randomization.Samplers;
using UnityEngine.Perception.Randomization.Scenarios;

[Serializable]
[AddRandomizerMenu("CustomNavmeshPlacementRandomizer")]
public class CustomNavmeshPlacementRandomizer : Randomizer, ICustomRandomizer
{
    public float time = 0.5f;
    float tmpTime;
    UniformSampler uniformSampler;

    public bool rotateHuman = true;

    public bool ready { get; set; }

    CustomScenario customScenario;

    private bool skipIteraction;

    public bool build = true;

    NavMeshSurface navMeshSurface;
    GameObject human;
    UniformSampler randomsampler = new UniformSampler();

    protected override void OnAwake()
    {
        base.OnAwake();
        customScenario = (CustomScenario)ScenarioBase.activeScenario;
    }

    protected override void OnIterationStart()
    {
        skipIteraction = false;
        tmpTime = time;
        uniformSampler = new UniformSampler();
    }

    protected override void OnUpdate()
    {
        base.OnUpdate();

        if (!ready)
        {
            navMeshSurface = null;

            foreach (var tag in tagManager.Query<NavMeshSurfaceRandomizerTag>())
            {
                navMeshSurface = tag.GetComponent<NavMeshSurface>();
                if (navMeshSurface != null)
                    break;
            }

            if (navMeshSurface != null)
            {
                if (navMeshSurface.navMeshData == null)
                {
                    if ((tmpTime -= Time.deltaTime) <= 0 && build)
                        navMeshSurface.BuildNavMesh();
                }
                else
                {
                    //Place Human
                    if (
                        UnityEngine
                            .Object.FindObjectsByType<CustomHumanTag>(FindObjectsSortMode.None)
                            .Length == 1
                    )
                    {
                        human = UnityEngine
                            .Object.FindObjectsByType<CustomHumanTag>(FindObjectsSortMode.None)[0]
                            .gameObject;
                        Vector3 randomPoint = GetRandomNavMeshLocation();
                        human.transform.position = randomPoint;
                        uniformSampler.range = new FloatRange(0, 360);
                        human.transform.Rotate(uniformSampler.Sample() * Vector3.up);
                        ready = !skipIteraction;
                    }
                }
            }
        }
    }

    private Vector3 GetRandomNavMeshLocation()
    {
        NavMeshTriangulation navMeshData = NavMesh.CalculateTriangulation();

        Mesh mesh = new Mesh();
        try
        {
            mesh.vertices = navMeshData.vertices;
            mesh.triangles = navMeshData.indices;
            return GetRandomPointOnMesh(mesh);
        }
        catch
        {
            Debug.Log($"Skipped for navmesh error");
            customScenario.DestroyCurrentEnv();
            customScenario.NextIteraction();
            skipIteraction = true;
            return Vector3.zero;
        }
    }

    Vector3 GetRandomPointOnMesh(Mesh mesh)
    {
        //if you're repeatedly doing this on a single mesh, you'll likely want to cache cumulativeSizes and total
        float[] sizes = GetTriSizes(mesh.triangles, mesh.vertices);
        float[] cumulativeSizes = new float[sizes.Length];
        float total = 0;

        for (int i = 0; i < sizes.Length; i++)
        {
            total += sizes[i];
            cumulativeSizes[i] = total;
        }

        //so everything above this point wants to be factored out

        float randomsample = randomsampler.Sample() * total;

        int triIndex = -1;

        for (int i = 0; i < sizes.Length; i++)
        {
            if (randomsample <= cumulativeSizes[i])
            {
                triIndex = i;
                break;
            }
        }

        if (triIndex == -1)
            Debug.LogError("triIndex should never be -1");

        Vector3 a = mesh.vertices[mesh.triangles[triIndex * 3]];
        Vector3 b = mesh.vertices[mesh.triangles[triIndex * 3 + 1]];
        Vector3 c = mesh.vertices[mesh.triangles[triIndex * 3 + 2]];

        //generate random barycentric coordinates

        float r = randomsampler.Sample();
        float s = randomsampler.Sample();

        if (r + s >= 1)
        {
            r = 1 - r;
            s = 1 - s;
        }
        //and then turn them back to a Vector3
        Vector3 pointOnMesh = a + r * (b - a) + s * (c - a);
        return pointOnMesh;
    }

    float[] GetTriSizes(int[] tris, Vector3[] verts)
    {
        int triCount = tris.Length / 3;
        float[] sizes = new float[triCount];
        for (int i = 0; i < triCount; i++)
        {
            sizes[i] =
                .5f
                * Vector3
                    .Cross(
                        verts[tris[i * 3 + 1]] - verts[tris[i * 3]],
                        verts[tris[i * 3 + 2]] - verts[tris[i * 3]]
                    )
                    .magnitude;
        }
        return sizes;
    }

    protected override void OnIterationEnd()
    {
        ready = false;
    }
}
