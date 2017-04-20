using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class SurfaceObject : MonoBehaviour {
    private Mesh SurfaceObjectMesh;
    int[] triangles;
    public Material VertexMaterial;
    public TriangleSheet sheet; //この三角シートの面をつくる

    private List<Vector3> VertexList = new List<Vector3>();
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
        VertexList.Clear();
        foreach (GameObject g in vertexobjects) VertexList.Add(g.transform.position);
        //裏のために頂点コピー
        VertexList.AddRange(VertexList);
        SurfaceObjectMesh.vertices = VertexList.ToArray();
        SurfaceObjectMesh.triangles = triangles;

        SurfaceObjectMesh.RecalculateNormals();
        SurfaceObjectMesh.RecalculateBounds();

        meshfilter.sharedMesh = SurfaceObjectMesh;

        GetComponent<MeshCollider>().sharedMesh = SurfaceObjectMesh;
    }
    // Update is called once per frame
    void Update () {
        #region TouchSurface
        //マウスのrayとMeshの当たり判定を行う
        RaycastHit hit;
        if (!Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit))
            return;

        MeshCollider meshCollider = hit.collider as MeshCollider;
        if (meshCollider == null || meshCollider.sharedMesh == null)
            return;

        Mesh mesh = meshCollider.sharedMesh;
        Vector3[] vertices = mesh.vertices;
        int[] triangles = mesh.triangles;

        int[] hitindex = new int[3];
        hitindex[0] = triangles[hit.triangleIndex * 3 + 0];
        hitindex[1] = triangles[hit.triangleIndex * 3 + 1];
        hitindex[2] = triangles[hit.triangleIndex * 3 + 2];
        GLManage glmanage = Camera.main.GetComponent<GLManage>();
        glmanage.p0 = VertexList[hitindex[0]];
        glmanage.p1 = VertexList[hitindex[1]];
        glmanage.p2 = VertexList[hitindex[2]];
        #endregion 
    }
}
