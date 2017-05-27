using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System;

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

        //当たっていなければreturn
        RaycastHit hit;
        if (!Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit))
            return;

        MeshCollider meshCollider = hit.collider as MeshCollider;
        if (meshCollider == null || meshCollider.sharedMesh == null)
            return;
        //マウスのRayが当たったメッシュを取得する
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
        if (Input.GetMouseButtonDown(0)) {
            this.sheet.ChangeSurfaceNaturalDuration(hitindex);
        }
        Vector3 hitpos = hit.point;
        int surfaceindex = this.sheet.GetSurfaceIndex(hitindex[0], hitindex[1], hitindex[2]);
        int nearest_surface_point = surfaceindex; //今選択している三角形の周囲3つのうちマウスの当たった点から一番ちかい三角形を構成するために必要な点
        float now_min_dis = float.MaxValue;
        Array.Sort(hitindex);
        if (hitindex[0] + 1 == hitindex[1] && hitindex[0] + this.sheet.N == hitindex[2]) {
            //triangle(k,k+1,k+N)
            //k+1-N, k+N-1, k+N+1のうち一番近い点を選択
            int vc = VertexList.Count / 2; //裏面のためコピーされてるので2でわる
            int k = hitindex[0];
            int[] candidate = new int[] { k + 1 - this.sheet.N, k + this.sheet.N - 1, k + this.sheet.N + 1 };
            foreach (int c in candidate) {
                if (0 <= c && c < vc) {
                    float dis = Vector3.Distance(VertexList[c], hitpos);
                    if (dis < now_min_dis) {
                        now_min_dis = dis;
                        nearest_surface_point = c;
                    }
                }
            }
        }
        else if (hitindex[0] + this.sheet.N - 1 == hitindex[1] && hitindex[1] + 1 == hitindex[2]) {
            //triangle(k,k+N-1,k+N)
            //k+2N-1, k-1, k+1のうち一番近い点を選択
            int vc = VertexList.Count / 2; //裏面のためコピーされてるので2でわる
            int k = hitindex[0];
            int[] candidate = new int[] { k - 1, k + 1, k + 2 * this.sheet.N - 1 };
            foreach(int c in candidate){
                if (0 <= c && c < vc) {
                    float dis = Vector3.Distance(VertexList[c], hitpos);
                    if (dis < now_min_dis) {
                        now_min_dis = dis;
                        nearest_surface_point = c;
                    }
                }
            }
        }
        int[] quad_index_array = { nearest_surface_point, hitindex[0], hitindex[1], hitindex[2] };
        try {
            Array.Sort(quad_index_array);
            glmanage.q0 = VertexList[quad_index_array[0]];
            glmanage.q1 = VertexList[quad_index_array[1]];
            glmanage.q2 = VertexList[quad_index_array[3]]; //この順番でないと正しく描画されない
            glmanage.q3 = VertexList[quad_index_array[2]];
            if (Input.GetMouseButtonDown(1)) {
                this.sheet.ChangeHingeNaturalDuration(quad_index_array);
            }
        }
        catch (ArgumentOutOfRangeException) {

        }
        #endregion 
    }
}
