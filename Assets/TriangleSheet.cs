using UnityEngine;
using System.Collections;
using System;
using System.IO;
using System.Collections.Generic;

//シミュレーションに用いる三角形シート
public class TriangleSheet : MonoBehaviour {
    private Main GameManagerMain;
    public GameObject forcePrefab;
    public GameObject VertexSpherePrefab;
    public GameObject EdgePrefab;
    public GameObject SurfacePrefab;
    public GameObject SurfaceObjectPrefab;
    public GameObject SelectedViewerPrefab;
    public int N = 10;
    public const double SphereInterval = 50.0;
    //頂点
    public GameObject[] Vertices;
    //エッジ
    public GameObject[] Edges;
    //面
    private GameObject[] Surfaces;
    //頂点にかかる力
    private GameObject[] ForceArrows;
    //面を貼るオブジェクト
    private GameObject SurfaceGameObject;
    //選択している場所を表示するためのオブジェクト
    public GameObject SelectedViewer;
    public float MaxForceSizeXYZ, MinForceSizeXYZ;
    private Vector3 ExternalForce;
    public bool issimulating = false;
    public float delta;
    private float timeCounter;
    private bool brokenflag = false; //崩壊したかのフラグ
    //Coroutineが複数出現しないようにするフラグ
    bool isCoroutineRunning = false;
    public bool[] isSimulateOn;//どのシミュレーションを使用するか
    public int[] SimulateSpeed;//何回コルーチン呼ぶか
    public float[] DefaultSpringConstant;
    public float[] SpringConstants;
    //シミュレーションパラメータ
    public int[] SurfaceSpring_NaturalDurationArray; //各面積の自然面積のインデックス
    private float[] SurfaceSpring_NaturalDurations = new float[] {0.5f, 1.0f, 2.0f, 3.0f};
    public List<int>[] Hinge_NaturalDurationAarray; //各ヒンジの自然角度のインデックス
    private int[] Hinge_NaturalDurations = new int[]{-180, -150, -120, -90, -60, -30, 0, 30, 60, 90, 120, 150, 180};
    void Start() {
        GameManagerMain = GameObject.Find("GameManager").GetComponent<Main>();
        timeCounter = delta;
        ExternalForce = new Vector3(0.0f, 0.0f, 0.0f);
        InitializeGameObjects(N);
    }
	public void DurationReset(){
		Hinge_NaturalDurationAarray = new List<int>[3];
		int default_hinge_naturalduration_index = -1;
		for (int i = 0; i < Hinge_NaturalDurations.Length; i++) if (Hinge_NaturalDurations[i] == 0) default_hinge_naturalduration_index = i;
		Hinge_NaturalDurationAarray[0] = new List<int>();
		for (int i = 0; i < (N - 2) * (N - 1); i++) Hinge_NaturalDurationAarray[0].Add(default_hinge_naturalduration_index);
		Hinge_NaturalDurationAarray[1] = new List<int>();
		for (int i = 0; i < (N - 1) * (N - 2); i++) Hinge_NaturalDurationAarray[1].Add(default_hinge_naturalduration_index);
		Hinge_NaturalDurationAarray[2] = new List<int>();
		for (int i = 0; i < (N - 1) * (N - 1); i++) Hinge_NaturalDurationAarray[2].Add(default_hinge_naturalduration_index);

		SurfaceSpring_NaturalDurationArray = new int[2 * (N - 1) * (N - 1)];
		int default_surface_naturalduration_index = -1; //1.0fになってるインデックスを探す
		for (int i = 0; i < SurfaceSpring_NaturalDurations.Length; i++) if (SurfaceSpring_NaturalDurations[i] == 1.0f) default_surface_naturalduration_index = i;
		for (int i = 0; i < SurfaceSpring_NaturalDurationArray.Length; i++) SurfaceSpring_NaturalDurationArray[i] = default_surface_naturalduration_index;
		this.SelectedViewer.GetComponent<SelectedViewer> ().ToggleVisible ();
	}
    void InitializeGameObjects(int changeN) {
        N = changeN;
        if (Vertices != null) foreach (GameObject g in Vertices) Destroy(g);
        if (Edges != null) foreach (GameObject g in Edges) Destroy(g);
        if (Surfaces != null) foreach (GameObject g in Surfaces) Destroy(g);
        if (ForceArrows != null) foreach (GameObject g in ForceArrows) Destroy(g);
        if (SurfaceGameObject != null) Destroy(SurfaceGameObject);
        if (SelectedViewer != null) Destroy(SelectedViewer);
        Vertices = new GameObject[N * N];
        Edges = new GameObject[2 * N * (N - 1) + (N - 1) * (N - 1) * 2];
        Surfaces = new GameObject[(N - 1) * (N - 1) * 2];
        ForceArrows = new GameObject[N * N];
        Hinge_NaturalDurationAarray = new List<int>[3];
        int default_hinge_naturalduration_index = -1;
        for (int i = 0; i < Hinge_NaturalDurations.Length; i++) if (Hinge_NaturalDurations[i] == 0) default_hinge_naturalduration_index = i;
        Hinge_NaturalDurationAarray[0] = new List<int>();
        for (int i = 0; i < (N - 2) * (N - 1); i++) Hinge_NaturalDurationAarray[0].Add(default_hinge_naturalduration_index);
        Hinge_NaturalDurationAarray[1] = new List<int>();
        for (int i = 0; i < (N - 1) * (N - 2); i++) Hinge_NaturalDurationAarray[1].Add(default_hinge_naturalduration_index);
        Hinge_NaturalDurationAarray[2] = new List<int>();
        for (int i = 0; i < (N - 1) * (N - 1); i++) Hinge_NaturalDurationAarray[2].Add(default_hinge_naturalduration_index);

        SurfaceSpring_NaturalDurationArray = new int[2 * (N - 1) * (N - 1)];
        int default_surface_naturalduration_index = -1; //1.0fになってるインデックスを探す
        for (int i = 0; i < SurfaceSpring_NaturalDurations.Length; i++) if (SurfaceSpring_NaturalDurations[i] == 1.0f) default_surface_naturalduration_index = i;
        for (int i = 0; i < SurfaceSpring_NaturalDurationArray.Length; i++) SurfaceSpring_NaturalDurationArray[i] = default_surface_naturalduration_index;

        #region MakeVertices
        //頂点の作成
        //(1,0) (0,1)→(1,0)(1/2,√3/2)の射交座標へ変換すると正方形が正三角形2つの平行四辺形になる
        double basecenter = (N-1) * SphereInterval / 2.0;
        double centerx = basecenter + basecenter / 2.0;
        double centerz = basecenter * Math.Sqrt(3) / 2.0;
        for (int i = 0; i < N * N; i++) {
            int h = i / N; int w = i % N;
            Vertices[i] = Instantiate(VertexSpherePrefab);
            Vertices[i].transform.parent = this.gameObject.transform;
            double basex = h * SphereInterval; double basey = w * SphereInterval;
            double posx = basex + basey / 2.0;
            double posz = basey * Math.Sqrt(3) / 2.0;
            Vertices[i].transform.localPosition = new Vector3((float)(posx - centerx), 0, (float)(posz - centerz));
        }
        #endregion
        #region MakeEdges
        //辺の作成 横
        int edgecnt = 0;
        for (int i = 0; i < N; i++) {
            for (int j = 0; j < N - 1; j++) {
                Edges[edgecnt] = Instantiate(EdgePrefab);
                Edges[edgecnt].transform.parent = this.gameObject.transform;
                EdgeScript es = Edges[edgecnt].GetComponent<EdgeScript>();
                es.sphere1 = Vertices[i * N + j];
                es.sphere2 = Vertices[i * N + j + 1];
                es.CalcNaturalLength();
                es.CalcNowLength();
                edgecnt++;
            }
        }
        //辺の作成 縦
        for (int i = 0; i < N; i++) {
            for (int j = 0; j < N - 1; j++) {
                Edges[edgecnt] = Instantiate(EdgePrefab);
                Edges[edgecnt].transform.parent = this.gameObject.transform;
                EdgeScript es = Edges[edgecnt].GetComponent<EdgeScript>();
                es.sphere1 = Vertices[j * N + i];
                es.sphere2 = Vertices[(j + 1) * N + i];
                es.CalcNaturalLength();
                es.CalcNowLength();
                edgecnt++;
            }
        }
        //辺の作成 右下ナナメ
        for (int i = 0; i < N - 1; i++) {
            for (int j = 0; j < N - 1; j++) {
                Edges[edgecnt] = Instantiate(EdgePrefab);
                Edges[edgecnt].transform.parent = this.gameObject.transform;
                EdgeScript es = Edges[edgecnt].GetComponent<EdgeScript>();
                es.sphere1 = Vertices[i * N + j];
                es.sphere2 = Vertices[(i + 1) * N + (j + 1)];
                es.CalcNaturalLength();
                es.CalcNowLength();
                Edges[edgecnt].gameObject.GetComponent<Renderer>().enabled = false;
                edgecnt++;
            }
        }
        //辺の作成 左下ナナメ
        for (int i = 0; i < N - 1; i++) {
            for (int j = 1; j < N; j++) {
                Edges[edgecnt] = Instantiate(EdgePrefab);
                Edges[edgecnt].transform.parent = this.gameObject.transform;
                EdgeScript es = Edges[edgecnt].GetComponent<EdgeScript>();
                es.sphere1 = Vertices[i * N + j];
                es.sphere2 = Vertices[(i + 1) * N + (j - 1)];
                es.CalcNaturalLength();
                es.CalcNowLength();
                edgecnt++;
            }
        }
        #endregion
        #region MakeSurfaces
        for (int i = 0; i < (N - 1) * (N - 1) * 2; i++) {
            Surfaces[i] = Instantiate(SurfacePrefab);
            Surfaces[i].transform.parent = this.gameObject.transform;
            SurfaceScript ss = Surfaces[i].GetComponent<SurfaceScript>();
            if (i % 2 == 0) {
                //(k, k+1, k+N)のtriangle
                int tmp = i / 2;
                int k = N * (tmp / (N - 1)) + (tmp % (N - 1));
                ss.sphere1 = Vertices[k];
                ss.sphere2 = Vertices[k + 1];
                ss.sphere3 = Vertices[k + N];
            }
            else {
                //(k, k+N, k+N-1)のtriangle
                int tmp = (i - 1) / 2;
                int k = N * (tmp / (N - 1)) + (tmp % (N - 1)) + 1;
                ss.sphere1 = Vertices[k];
                ss.sphere2 = Vertices[k + N - 1];
                ss.sphere3 = Vertices[k + N];
            }
            ss.CalcNaturalArea();
            ss.CalcNowArea();
        }
        #endregion
        #region SetForceArrow
        //頂点にかかる力を表す矢印
        for (int i = 0; i < N * N; i++) {
            ForceArrows[i] = Instantiate(forcePrefab);
            ForceArrows[i].transform.parent = this.gameObject.transform;
            ForceArrows[i].GetComponent<ForceArrow>().nekko = Vertices[i];
        }
        #endregion
        #region SetSurfaceObject
        //面をもつオブジェクトに座標を設定する
        if (SurfaceGameObject != null) Destroy(SurfaceGameObject);
        SurfaceGameObject = Instantiate(SurfaceObjectPrefab);
        SurfaceGameObject.transform.parent = this.gameObject.transform;
        SurfaceGameObject.GetComponent<SurfaceObject>().UpdateMesh(Vertices);
        SurfaceGameObject.GetComponent<SurfaceObject>().sheet = this;
        #endregion
        #region SetSelectedViewer
        if (SelectedViewer != null) Destroy(SelectedViewer);
        SelectedViewer = Instantiate(SelectedViewerPrefab);
        SelectedViewer.transform.parent = this.gameObject.transform;
        SelectedViewer.GetComponent<SelectedViewer>().sheet = this;

        #endregion
    }
    public void StartSimulate() {
        issimulating = true;
    }
    public void StopSimulate() {
        issimulating = false;
    }
    public void ToggleSimulate() {
        if (issimulating) {
            StopSimulate();
            //this.SelectedViewer.GetComponent<SelectedViewer>().OnSimulateStopped();//TODO:ちゃんとToggleにしましょう
        }
        else {
            //選択中のボタンは押せないように
            //this.SelectedViewer.GetComponent<SelectedViewer>().AllSelectedDisable();
            StartSimulate();
        }
    }

    public int tryChangeNValue(int changeN) {
        //シミュレーション中なら変更不可
        if (issimulating) return N;
        InitializeGameObjects(changeN);
        return changeN;
    }
    // Update is called once per frame
    void Update() {
        //最もマウスに近い点を検出したい
        /*float mindis = 100.0f;
        int nearestindex = -1;
        for (int i = 0; i < Vertices.Length; i++) {
            //カメラの左下は(0, 0) に、右上は (1, 1) 
            Vector2 vpos = Camera.main.WorldToViewportPoint(Vertices[i].transform.position);
            Vector2 mousepos = Input.mousePosition;
            float dis = Vector2.Distance(vpos, mousepos);
            if (i == 0) mindis = dis;
            else if (dis < mindis) {
                nearestindex = i;
                mindis = dis;
            }
        }
        if (nearestindex <= Vertices.Length) { //途中でN変更されたときバグるので
            Vector3 nearestpos = Vertices[nearestindex].transform.position;
            string str = string.Format("index = {0} \n x = {1}, y = {2}, z = {3}", nearestindex, nearestpos.x, nearestpos.y, nearestpos.z);
            GameObject.Find("PositionLabel").GetComponent<UnityEngine.UI.Text>().text = str;
        }*/
        SurfaceGameObject.GetComponent<SurfaceObject>().UpdateMesh(Vertices);
        #region SimulateMain    
        if (issimulating == false) return;
        timeCounter -= Time.deltaTime;
        if (timeCounter < 0) {
            //シミュレーション前の座標をコピーしておく
            Vector3[] oldposition_array = new Vector3[Vertices.Length];
            for (int i = 0; i < oldposition_array.Length; i++) {
                oldposition_array[i] = Vertices[i].transform.position;
            }
            //シミュレートを実行
            for (int j = 0; j < 10; j++) {
                if (isSimulateOn[0]) for (int i = 0; i < SimulateSpeed[0]; i++) StartCoroutine(SpringSimulate());
                if (isSimulateOn[1]) for (int i = 0; i < SimulateSpeed[1]; i++) StartCoroutine(SpringSimulate2());
                if (isSimulateOn[2]) for (int i = 0; i < SimulateSpeed[2]; i++) StartCoroutine(SpringSimulate3()); //速度のためroop
            }
            //もしシミュレーション結果、値が崩壊した場合、座標をシミュレーション前の座標に戻してシミュレーションを自動停止
            if (brokenflag) {
                PositionReset();
            }
            timeCounter = delta;
        }
        #endregion
    }
    public void UpdateExternalForce(Vector3 externalforce) {
        if (ForceArrows[0] == null || ForceArrows[N*N-1] == null) return; //アプリケーション開始時に実行されるがまだInstantiateされてないことがある
        ExternalForce = externalforce;
        ForceArrows[0].GetComponent<ForceArrow>().arrowvec = ExternalForce;
        ForceArrows[N * N - 1].GetComponent<ForceArrow>().arrowvec = -ExternalForce;
    }
    #region SimulationRoutine
    private int GetEdgeIndex(int a, int b) {
        //頂点番号aとbをつなぐedgeがedgesのどれに当たるかのインデックスを返す
        if (a > b) {
            int c = a;
            a = b;
            b = c;
        }
        int h = a / N; int w = a % N;
        if (b - a == 1) {
            //横
            return h * (N - 1) + w;
        } else if (b - a == N) {
            //縦
            return N * (N - 1) + h * N + w;
        } else if (b - a == N + 1) {
            //右下ナナメ
            return N * (N - 1) * 2 + h * (N - 1) + w;
        } else if (b - a == N - 1) {
            //左下ナナメ
            return N * (N - 1) * 2 + (N - 1) * (N - 1) + h * (N - 1) + w - 1;
        }
        return -1;
    }
    private int GetVertIndex(int h, int w) {
        return h * N + w;
    }
    public int GetSurfaceIndex(int i, int j, int k) {
        //頂点番号i,j,kでかこまれたtriangleがsurfacesのどれにあたるかのインデックスを返す
        int[] ary = new int[] { i, j, k };
        Array.Sort(ary);
        if (N != 2) {
            if (ary[0] + 1 == ary[1] && ary[0] + N == ary[2]) {
                //triangle(k,k+1,k+N)
                int h = ary[0] / N;
                int w = ary[0] % N;
                return 2 * ((N - 1) * h + w);
            } else if (ary[0] + N - 1 == ary[1] && ary[1] + 1 == ary[2]) {
                //triangle(k,k+N-1,k+N)
                int h = ary[0] / N;
                int w = ary[0] % N;
                return 2 * ((N - 1) * h + w) - 1;
            }
        }else {
            //N==2のときは上の方法だと(1,2,3)のトライアングルが上向きと判定されるので分ける
            if (ary[0] == 0 && ary[1] == 1 && ary[2] == 2) return 0;
            if (ary[0] == 1 && ary[1] == 2 && ary[2] == 3) return 1;
        }
        return -1;
    }
    
    IEnumerator SpringSimulate() {
        if (isCoroutineRunning) yield break;
        isCoroutineRunning = true;
        int[] dx = new int[] { -1, 0, 1, 0, -1, -1, 1, 1 };
        int[] dy = new int[] { 0, -1, 0, 1, 1, -1, 1, -1 };
        //シミュレーション前の座標をコピーしておく
        Vector3[] oldposition_array = new Vector3[Vertices.Length];
        for (int i = 0; i < oldposition_array.Length; i++) oldposition_array[i] = Vertices[i].transform.position;
        //N*Nの点それぞれについてシミュレーション前の座標を元に次の位置を計算
        for (int i = 0; i < N * N; i++) {
            int h = i / N; int w = i % N;
            Vector3 force = new Vector3(0, 0, 0);
            //ばねで接続されている物体との力の合力を計算
            for (int j = 0; j < dx.Length; j++) {
                if (h + dx[j] < 0 || h + dx[j] >= N || w + dy[j] < 0 || w + dy[j] >= N) continue;
                int v1 = GetVertIndex(h, w); int v2 = GetVertIndex(h + dx[j], w + dy[j]);
                EdgeScript nowedge = Edges[GetEdgeIndex(v1, v2)].GetComponent<EdgeScript>();
                Vector3 direction = Vector3.Normalize(oldposition_array[v2] - oldposition_array[v1]); //方向ベクトル
                force += direction * (nowedge.nowlength - nowedge.naturallength);
            }
            force *= SpringConstants[0]; //精度のために最後にバネ定数かける
            Vertices[i].transform.position += force * delta;
            //外力
            if (i == 0) Vertices[i].transform.position += SpringConstants[0] * ExternalForce * delta;
            if (i == N * N - 1) Vertices[i].transform.position -= SpringConstants[0] * ExternalForce * delta;
        }
        //シミュレーション後にバネの現在の長さを再計算しておく
        for (int i = 0; i < Edges.Length; i++) Edges[i].GetComponent<EdgeScript>().CalcNowLength();
        isCoroutineRunning = false;
        yield break;
    }

    IEnumerator SpringSimulate2() {
        if (isCoroutineRunning) yield break;
        isCoroutineRunning = true;
        if (brokenflag) yield break;
        //近接最大6面の面積バネを考慮したシミュレーションを行う
        //シミュレーション前の座標をコピーしておく
        Vector3[] oldposition_array = new Vector3[Vertices.Length];
        for (int t = 0; t < oldposition_array.Length; t++) oldposition_array[t] = Vertices[t].transform.position;
        //N*Nの点それぞれについてシミュレーション前の座標を元に次の位置を計算
        //StreamWriter sw = new StreamWriter("../LogData.txt", true); //true=追記 false=上書き
        for (int i = 0; i < N * N; i++) {
            int h = i / N; int w = i % N;
            Vector3 force = new Vector3(0, 0, 0);
            //近接6面の頂点リスト作成用配列
            int[] dx = new int[] { 0, 1, 1, 0, -1, -1 }; //+dx[]*N
            int[] dy = new int[] { -1, -1, 0, 1, 1, 0 }; //+dy[]+1
            for (int j = 0; j < dx.Length; j++) {
                //頂点iとrjとrkで囲まれた三角形のバネを考慮した力を計算する
                int kindex = (j + 1) % dx.Length; //(0,1),(1,2),(3,4),(4,5),(5,0)の(5,0)が(5,6)にならないように
                int rj_index = i + dx[j] * N + dy[j];
                int rk_index = i + dx[kindex] * N + dy[kindex];
                //sw.WriteLine(string.Format("i={0}, j={1}, h = {2}, w = {3}, rj_index = {4}, rk_index = {5}", i, j, h, w, rj_index, rk_index));
                //存在しない場合はスキップ
                if (h + dx[j] < 0 || h + dx[j] >= N || w + dy[j] < 0 || w + dy[j] >= N) continue;
                if (h + dx[kindex] < 0 || h + dx[kindex] >= N || w + dy[kindex] < 0 || w + dy[kindex] >= N) continue;

                Vector3 ri = oldposition_array[i];
                Vector3 rj = oldposition_array[rj_index];
                Vector3 rk = oldposition_array[rk_index];
                Vector3 dki = rk - ri; Vector3 dji = rj - ri;
                float tmp_fx = dji.x * dki.sqrMagnitude + dki.x * dji.sqrMagnitude - (dji.x + dki.x) * Vector3.Dot(dki, dji);
                float tmp_fy = dji.y * dki.sqrMagnitude + dki.y * dji.sqrMagnitude - (dji.y + dki.y) * Vector3.Dot(dki, dji);
                float tmp_fz = dji.z * dki.sqrMagnitude + dki.z * dji.sqrMagnitude - (dji.z + dki.z) * Vector3.Dot(dki, dji);
                float cross = Vector3.Magnitude(Vector3.Cross(dki, dji));
                Vector3 res = new Vector3(tmp_fx / cross / 2.0f, tmp_fy / cross / 2.0f, tmp_fz / cross / 2.0f);
                int surfaceindex = GetSurfaceIndex(i, rj_index, rk_index);
                SurfaceScript ss = Surfaces[surfaceindex].GetComponent<SurfaceScript>();
                //(S - s0)かける
                res *= (ss.now_area - ss.natural_area * SurfaceSpring_NaturalDurations[SurfaceSpring_NaturalDurationArray[surfaceindex]]);
                force += res;
            }
            //ForceArrows[i].GetComponent<ForceArrow>().arrowvec = force * SpringConstant;
            force *= SpringConstants[1] * 0.1f; //精度のために最後にバネ定数かける
            //forceが崩壊した場合はエラーフラグを立てて終了
            if (float.IsNaN(force.x) || float.IsNaN(force.y) || float.IsNaN(force.z) || float.IsInfinity(force.x) || float.IsInfinity(force.y) || float.IsInfinity(force.z)) {
                Console.WriteLine(force.x);
                brokenflag = true;
                isCoroutineRunning = false;
                yield break;
            }
            Vertices[i].transform.position += force * delta;
            //外力
            if (i == 0) Vertices[i].transform.position += ExternalForce * delta;
            if (i == N * N - 1) Vertices[i].transform.position -= ExternalForce * delta;
            if (float.IsNaN(Vertices[0].transform.position.x) || float.IsNaN(Vertices[0].transform.position.y) || float.IsNaN(Vertices[0].transform.position.z)
                || float.IsInfinity(Vertices[0].transform.position.x) || float.IsInfinity(Vertices[0].transform.position.y) || float.IsInfinity(Vertices[0].transform.position.z)) {
                brokenflag = true;
                isCoroutineRunning = false;
                yield break;
            }
        }
        //シミュレーション後に面積ばねの現在の面積を再計算しておく
        for (int i = 0; i < Surfaces.Length; i++) Surfaces[i].GetComponent<SurfaceScript>().CalcNowArea();
        isCoroutineRunning = false;
        yield break;
    }

    IEnumerator SpringSimulate3() {
        if (isCoroutineRunning) yield break;
        isCoroutineRunning = true;
        int[] dx = new int[] { -1, 0, 1, 0, -1, -1, 1, 1 };
        int[] dy = new int[] { 0, -1, 0, 1, 1, -1, 1, -1 };
        //シミュレーション前の座標をコピーしておく
        Vector3[] oldposition_array = new Vector3[Vertices.Length];
        for (int i = 0; i < oldposition_array.Length; i++) oldposition_array[i] = Vertices[i].transform.position;
        //各頂点にかかる力の格納用変数
        Vector3[] force_array = new Vector3[Vertices.Length];
        for (int i = 0; i < force_array.Length; i++) force_array[i] = Vector3.zero;
        //2面のヒンジになっているEdgeすべてについて調べて各頂点にかかる力を計算する
        //縦ヒンジ(N-2)*(N-1)
        for (int i = 1; i < N - 1; i++) {
            for (int j = 0; j < N - 1; j++) {
                //(i-1,j+1),(i,j),(i,j+1),(i+1,j)によるヒンジ
                int x0 = GetVertIndex(i - 1, j + 1);
                int x1 = GetVertIndex(i, j);
                int x2 = GetVertIndex(i, j + 1);
                int x3 = GetVertIndex(i + 1, j);
                HingeStencil.HingeStencilForce hf = HingeStencil.calcHingeStencilForce(oldposition_array[x0], oldposition_array[x1], oldposition_array[x2], oldposition_array[x3]);
                //dphi/dtheta = (2) * springconstant * (theta_i - theta_natural) springconstantは最後にかける
                //-1忘れるとバグる
                int hinge_index = (N - 1) * (i - 1) + (j + 1) - 1;
                force_array[x0] += hf.f0 * -1 * (hf.theta - ((float)Hinge_NaturalDurations[Hinge_NaturalDurationAarray[0][hinge_index]] * Mathf.PI / 180.0f));
                force_array[x1] += hf.f1 * -1 * (hf.theta - ((float)Hinge_NaturalDurations[Hinge_NaturalDurationAarray[0][hinge_index]] * Mathf.PI / 180.0f));
                force_array[x2] += hf.f2 * -1 * (hf.theta - ((float)Hinge_NaturalDurations[Hinge_NaturalDurationAarray[0][hinge_index]] * Mathf.PI / 180.0f));
                force_array[x3] += hf.f3 * -1 * (hf.theta - ((float)Hinge_NaturalDurations[Hinge_NaturalDurationAarray[0][hinge_index]] * Mathf.PI / 180.0f));
            }
        }
        //左上ヒンジ(N-1)*(N-2)
        for (int i = 0; i < N - 1; i++) {
            for (int j = 1; j < N - 1; j++) {
                //(i,j+1),(i,j),(i+1,j),(i+1,j-1)によるヒンジ
                int x0 = GetVertIndex(i, j + 1);
                int x1 = GetVertIndex(i, j);
                int x2 = GetVertIndex(i + 1, j);
                int x3 = GetVertIndex(i + 1, j - 1);
                HingeStencil.HingeStencilForce hf = HingeStencil.calcHingeStencilForce(oldposition_array[x0], oldposition_array[x1], oldposition_array[x2], oldposition_array[x3]);
                //dphi/dtheta = 2 * springconstant * (theta_i - theta_natural) springconstantは最後にかける
                //-1忘れるとバグる
                int hinge_index = (N - 2) * i + j - 1;
                force_array[x0] += hf.f0 * -1 * (hf.theta - ((float)Hinge_NaturalDurations[Hinge_NaturalDurationAarray[1][hinge_index]] * Mathf.PI / 180.0f));
                force_array[x1] += hf.f1 * -1 * (hf.theta - ((float)Hinge_NaturalDurations[Hinge_NaturalDurationAarray[1][hinge_index]] * Mathf.PI / 180.0f));
                force_array[x2] += hf.f2 * -1 * (hf.theta - ((float)Hinge_NaturalDurations[Hinge_NaturalDurationAarray[1][hinge_index]] * Mathf.PI / 180.0f));
                force_array[x3] += hf.f3 * -1 * (hf.theta - ((float)Hinge_NaturalDurations[Hinge_NaturalDurationAarray[1][hinge_index]] * Mathf.PI / 180.0f));
            }
        }
        //右上ヒンジ(N-1)*(N-1)
        for (int i = 0; i < N - 1; i++) {
            for (int j = 0; j < N - 1; j++) {
                //(i,j),(i+1,j),(i,j+1),(i+1,j+1)によるヒンジ
                int x0 = GetVertIndex(i, j);
                int x1 = GetVertIndex(i + 1, j);
                int x2 = GetVertIndex(i, j + 1);
                int x3 = GetVertIndex(i + 1, j + 1);
                HingeStencil.HingeStencilForce hf = HingeStencil.calcHingeStencilForce(oldposition_array[x0], oldposition_array[x1], oldposition_array[x2], oldposition_array[x3]);
                //dphi/dtheta = 2 * springconstant * (theta_i - theta_natural) springconstantは最後にかける
                //-1忘れるとバグる
                int hinge_index = (N - 1) * i + j;
                force_array[x0] += hf.f0 * -1 * (hf.theta - ((float)Hinge_NaturalDurations[Hinge_NaturalDurationAarray[2][hinge_index]] * Mathf.PI / 180.0f));
                force_array[x1] += hf.f1 * -1 * (hf.theta - ((float)Hinge_NaturalDurations[Hinge_NaturalDurationAarray[2][hinge_index]] * Mathf.PI / 180.0f));
                force_array[x2] += hf.f2 * -1 * (hf.theta - ((float)Hinge_NaturalDurations[Hinge_NaturalDurationAarray[2][hinge_index]] * Mathf.PI / 180.0f));
                force_array[x3] += hf.f3 * -1 * (hf.theta - ((float)Hinge_NaturalDurations[Hinge_NaturalDurationAarray[2][hinge_index]] * Mathf.PI / 180.0f));
            }
        }
        //force_arrayが崩壊した場合はエラーフラグを立てて終了
        foreach (Vector3 v in force_array) {
            if(float.IsNaN(v.x) || float.IsNaN(v.y) || float.IsNaN(v.z) || float.IsInfinity(v.x) || float.IsInfinity(v.y) || float.IsInfinity(v.z)) {
                brokenflag = true;
                isCoroutineRunning = false;
                yield break;
            }
        }
        //すべての点を計算した力に基づいて動かす
        for (int i = 0; i < N * N; i++) {
            //ForceArrows[i].GetComponent<ForceArrow>().arrowvec = force_array[i] * (10.0f / force_array[i].magnitude);
            //Vertices[i].transform.position += SpringConstant * force_array[i] * delta;
            Vertices[i].transform.position += SpringConstants[2] * force_array[i] * delta;
            //外力
            //if (i == 0) Vertices[i].transform.position += ExternalForce * delta;
            //if (i == N * N - 1) Vertices[i].transform.position -= ExternalForce * delta;
        }
        isCoroutineRunning = false;
        yield break;
    }
    #endregion

    public void PositionReset() {
        //頂点のリセット
        //(1,0) (0,1)→(1,0)(1/2,√3/2)の射交座標へ変換すると正方形が正三角形2つの平行四辺形になる
        double basecenter = (N-1) * SphereInterval / 2.0;
        double centerx = basecenter + basecenter / 2.0;
        double centerz = basecenter * Math.Sqrt(3) / 2.0;
        for (int i = 0; i < N * N; i++) {
            int h = i / N; int w = i % N;
            double basex = h * SphereInterval; double basey = w * SphereInterval;
            double posx = basex + basey / 2.0;
            double posz = basey * Math.Sqrt(3) / 2.0;
            Vertices[i].transform.position = new Vector3((float)(posx - centerx), 0, (float)(posz - centerz));
        }
        //頂点をリセットしたのでbrokenflagは戻す
        brokenflag = false;
        //面オブジェクトの座標も更新
        SurfaceGameObject.GetComponent<SurfaceObject>().UpdateMesh(Vertices);
    }

    #region changeNaturalDuration
    public void ChangeSurfaceNaturalDuration(int[] vertindex) {
        try {
            //裏点の場合は表点に変換
            Array.Sort(vertindex);
            if (vertindex[0] > N * N) {
                for (int i = 0; i < 3; i++) vertindex[i] -= N * N;
            }
            int surfaceindex = GetSurfaceIndex(vertindex[0], vertindex[1], vertindex[2]);
            //パレットで選択した自然長にする
            SurfaceSpring_NaturalDurationArray[surfaceindex] = GameManagerMain.pallet.index;
            this.SelectedViewer.GetComponent<SelectedViewer>().changeAreaMaterial(surfaceindex);
        }
        catch (ArgumentOutOfRangeException ex) {
            Console.WriteLine(ex.Message);
        }
    }
    public void ChangeHingeNaturalDuration(int startVertIndex, int endVertIndex) {
        //パレットで選択した角度に変更する
        //まずどのタイプのヒンジか調べる必要がある
        if(startVertIndex == endVertIndex) return;
        if(startVertIndex > endVertIndex){
            //swap
            var tmp = endVertIndex;
            endVertIndex = startVertIndex;
            startVertIndex = tmp;
        }
        //表点と裏点がつながっている場合はやばいので結ばない
        if (startVertIndex < N * N  && endVertIndex >= N * N ) return;
        //裏点同士がつながっている場合は表点に変換
        if (startVertIndex >= N * N && endVertIndex >= N * N) {
            startVertIndex -= N * N;
            endVertIndex -= N * N;
        }
        int h1 = startVertIndex / N;
        int h2 = endVertIndex / N;
        int w1 = startVertIndex % N;
        int w2 = endVertIndex % N;
		//N=2のときだけ例外処理
		if (N == 2) {
            if (startVertIndex == 1 && endVertIndex == 2) {
                //ヒンジタイプC
                Hinge_NaturalDurationAarray[2][0] = GameManagerMain.pallet.index;
                this.SelectedViewer.GetComponent<SelectedViewer>().changeHingeMaterial(2, 0);
            }
			return;
		}
        if (h1 == h2) {
            //ヒンジタイプA
            for (int i = 0; i < w2 - w1; i++) {
                int hinge_index = (N - 1) * (h1 - 1) + w1 + i;
                Hinge_NaturalDurationAarray[0][hinge_index] = GameManagerMain.pallet.index;
                this.SelectedViewer.GetComponent<SelectedViewer>().changeHingeMaterial(0, hinge_index);
            }
            return;
        }
        if (w1 == w2) {
            //ヒンジタイプB
            for (int i = 0; i < h2 - h1; i++) {
                int hinge_index = (N - 2) * (h1 + i) + w1 - 1;
                Hinge_NaturalDurationAarray[1][hinge_index] = GameManagerMain.pallet.index;
                this.SelectedViewer.GetComponent<SelectedViewer>().changeHingeMaterial(1, hinge_index);
            }
            return;
        }
        if (Math.Abs(h2 - h1) == Math.Abs(w2 - w1)) {
            //ヒンジタイプC
            for (int i = 0; i < Math.Abs(h2 - h1); i++) {
                int hinge_index = (N - 1) * h1 + w1 - 1 + (N - 2) * i;
                Hinge_NaturalDurationAarray[2][hinge_index] = GameManagerMain.pallet.index;
                this.SelectedViewer.GetComponent<SelectedViewer>().changeHingeMaterial(2, hinge_index);
            }
            return;
        }
    }
    #endregion

    //選択している場所関連
    #region SelectedVisible 

    public void ChangeSelectVisible() {
        if (SelectedViewer == null) return;
        this.SelectedViewer.GetComponent<SelectedViewer>().ToggleVisible();
    }
    #endregion

}
