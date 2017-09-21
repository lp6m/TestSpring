using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System;
using UnityEngine.EventSystems;

public class SurfaceObject : MonoBehaviour {
    private Mesh SurfaceObjectMesh;
    int[] triangles;
    public Material VertexMaterial;
    public Material NowSelectingEdgeMaterial;
    public TriangleSheet sheet; //この三角シートの面をつくる
    private Main GameManagerMain;

    private List<Vector3> VertexList = new List<Vector3>();
    // Use this for initialization
    void Start () {
        GameManagerMain = GameObject.Find("GameManager").GetComponent<Main>();
        SurfaceObjectMesh = new Mesh();
        SetupMeshTriangles();
        GetComponent<Renderer>().material = VertexMaterial;
        SetupSelectingHinge();
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
    void SetupSelectingHinge() {
        nowSelectingHinge = new GameObject();
        nowSelectingHinge.name = "nowSelectingHinge";
        nowSelectingHinge.transform.parent = this.transform;
        nowSelectingHinge.AddComponent<LineRenderer>();
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
    private int fingerID = -1;
    private void Awake() {
#if !UNITY_EDITOR
     fingerID = 0; 
#endif
    }
    bool nowHingeSelecting = false;
    GameObject nowSelectingHinge;
    int selectingHingeStartIndex; //選択中のヒンジの始点
    int selectingHingeEndIndex; //選択中のヒンジの終点
    void Update () {
        //ヒンジバネのPaintのためのルーチン
        if (Input.GetMouseButton(0) == false) {
            if (nowHingeSelecting) {
                //ヒンジを書き終わったのでnaturalDurationを変更する
                this.sheet.ChangeHingeNaturalDuration(selectingHingeStartIndex, selectingHingeEndIndex);
            }
            nowHingeSelecting = false;
            nowSelectingHinge.SetActive(false);
            return;
        }
        if (EventSystem.current.IsPointerOverGameObject(fingerID)) {
            //UIをタップしているのであたり判定を行わない
            return;
        }
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
        if (GameManagerMain.IsPaintMode && GameManagerMain.pallet.mode == "area") {
            GLManage glmanage = Camera.main.GetComponent<GLManage>();
            glmanage.p0 = VertexList[hitindex[0]];
            glmanage.p1 = VertexList[hitindex[1]];
            glmanage.p2 = VertexList[hitindex[2]];
            if (Input.GetMouseButton(0)) {
                this.sheet.ChangeSurfaceNaturalDuration(hitindex);
            }
        }
        #endregion
        #region TouchHinge
        if (GameManagerMain.IsPaintMode == false || GameManagerMain.pallet.mode != "hinge") return;
        Vector3 hitpos = hit.point;
        //ヒットした点から一番近い頂点をさがす
        float dis0 = Vector3.Distance(VertexList[hitindex[0]], hitpos);
        float dis1 = Vector3.Distance(VertexList[hitindex[1]], hitpos);
        float dis2 = Vector3.Distance(VertexList[hitindex[2]], hitpos);
        int nearestIndex = hitindex[0];
        if (dis1 < dis0 && dis1 < dis2) nearestIndex = hitindex[1];
        if (dis2 < dis0 && dis2 < dis1) nearestIndex = hitindex[2];
        var selectingEdge = nowSelectingHinge.GetComponent<LineRenderer>();
        if (nowHingeSelecting) {
            //現在選択中のエッジの終点をセットする
            selectingEdge.SetPosition(1, VertexList[nearestIndex]);
            this.selectingHingeEndIndex = nearestIndex;
        }
        else {
            //現在選択中のエッジの始点をセットする
            nowHingeSelecting = true;
            nowSelectingHinge.SetActive(true);
            nowSelectingHinge.GetComponent<LineRenderer>().material = NowSelectingEdgeMaterial;
            nowSelectingHinge.GetComponent<LineRenderer>().material.color = Color.black;
            nowSelectingHinge.GetComponent<LineRenderer>().startWidth = 5.0f;
            nowSelectingHinge.GetComponent<LineRenderer>().endWidth = 5.0f;
            selectingEdge.SetPosition(0, VertexList[nearestIndex]);
            //以前のエッジが表示されるちらつき回避のため終点も同じ点でセット
            selectingEdge.SetPosition(1, VertexList[nearestIndex]);
            this.selectingHingeStartIndex = nearestIndex;
        }
        #endregion 
    }
}
