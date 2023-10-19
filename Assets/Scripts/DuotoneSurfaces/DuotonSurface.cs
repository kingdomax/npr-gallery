using UnityEngine;

public class DuotonSurface : MonoBehaviour
{
    private  Mesh mesh;
    public  int subdivisionsBeforeColoring = 1;
    public  int subdivisionsAfterColoring = 0;
    public  bool changePostitionsOfOriginalVertices = true;

    void Start()
    {
        mesh = GetComponent<MeshFilter>().mesh;
        mesh = Subdivider.getSubdividedMesh(mesh, subdivisionsBeforeColoring, subdivisionsAfterColoring, changePostitionsOfOriginalVertices);
    }
}
