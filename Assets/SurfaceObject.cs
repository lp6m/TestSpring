using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class SurfaceObject : MonoBehaviour {
    private Mesh SurfaceObjectMesh;
    int[] triangles;
    public Material VertexMaterial;
    public TriangleSheet sheet; //この三角シートの面をつくる
    // Use this for initialization
    void Start () {
        SurfaceObjectMesh = new Mesh();
        SetupMeshTriangles();
        GetComponent<Renderer>().material = VertexMaterial;
    }
    private int GetVertexIndex(int x, int y) {
        return sheet.N * x + y;
    }
    private int GetVertexIndex2(int x, int y) {
        return sheet.N * x + y + sheet.N * sheet.N;
    }
    void SetupMeshTriangles() {
        List<int> triList = new List<int>();
        triList.Clear();
        for (int i = 0; i < sheet.N - 1; i++) {
            for (int j = 0; j < sheet.N - 1; j++) {
                //表のMesh
                triList.Add(GetVertexIndex(i, j));
                triList.Add(GetVertexIndex(i, j + 1));
                triList.Add(GetVertexIndex(i + 1, j));

                triList.Add(GetVertexIndex(i, j + 1));
                triList.Add(GetVertexIndex(i + 1, j + 1));
                triList.Add(GetVertexIndex(i + 1, j));

                //裏のMesh
                triList.Add(GetVertexIndex2(i, j));
                triList.Add(GetVertexIndex2(i + 1, j));
                triList.Add(GetVertexIndex2(i, j + 1));

                triList.Add(GetVertexIndex2(i, j + 1));
                triList.Add(GetVertexIndex2(i + 1, j));
                triList.Add(GetVertexIndex2(i + 1, j + 1));
            }
        }
        triangles = triList.ToArray();
    }

    public void UpdateMesh(GameObject[] vertexobjects) {
        if (SurfaceObjectMesh == null) return;
        MeshFilter meshfilter = GetComponent<MeshFilter>();
        List<Vector3> tmpList = new List<Vector3>();
        foreach (GameObject g in vertexobjects) tmpList.Add(g.transform.position);
        //裏のために頂点コピー
        tmpList.AddRange(tmpList);
        SurfaceObjectMesh.vertices = tmpList.ToArray();
        SurfaceObjectMesh.triangles = triangles;

        SurfaceObjectMesh.RecalculateNormals();
        SurfaceObjectMesh.RecalculateBounds();

        meshfilter.sharedMesh = SurfaceObjectMesh;
    }
    // Update is called once per frame
    void Update () {
	
	}
}
