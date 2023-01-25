using UnityEngine;
using UnityEngine.AI;

public class TestBuild : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        GetComponent<NavMeshSurface>().BuildNavMesh();
    }
}
