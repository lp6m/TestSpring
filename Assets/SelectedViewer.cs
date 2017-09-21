using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//選択された場所を表示するためのオブジェクト
public class SelectedViewer : MonoBehaviour {
    private Main GameManagerMain;
    public GameObject EdgePrefab;
    public Material RedMaterial;
    public Material ArrowMaterial;
    public Material[] AreaMaterials; //0.5 2.0, 3.0
    public Material[] HingeMaterials;//30,60,90,120,150,180, -180,-150,-120,-90,-60,-30,
    public TriangleSheet sheet; //親オブジェクト
    private GameObject[] AreaGameObjects; //選択した面を表示するためのオブジェクト
    private GameObject[] HingeGameObjects; //選択したヒンジを表示するためのオブジェクト

    void OnDestroy() {
        if (AreaGameObjects != null) foreach (var g in AreaGameObjects) if (g != null) Destroy(g);
        if (HingeGameObjects != null) foreach (var g in HingeGameObjects) if (g != null) Destroy(g);
    }
	// Use this for initialization
	void Start () {
        GameManagerMain = GameObject.Find("GameManager").GetComponent<Main>();
        //TriangleSheetのNが変わるたびにSelectedViewerは再生成されるので,SelectedViewer内ではNは固定と考えてよい
        //存在する面積分(2 * N-1 * N-1)だけGameObjectを作成する
        AreaGameObjects = new GameObject[2 * (sheet.N - 1) * (sheet.N - 1)];
        for (int i = 0; i < AreaGameObjects.Length; i++) {
            AreaGameObjects[i] = new GameObject();
            AreaGameObjects[i].name = "SelectedViewer_Area";
            AreaGameObjects[i].transform.parent = this.transform;
            AreaGameObjects[i].AddComponent<MeshRenderer>();
            AreaGameObjects[i].AddComponent<MeshFilter>();
            //はじめは表示しない
            AreaGameObjects[i].SetActive(false);
        }
        //存在するヒンジの数だけGameObjectを作成する（辺の数ではないので注意）
        HingeGameObjects = new GameObject[2 * (sheet.N - 2) * (sheet.N - 1) + (sheet.N - 1) * (sheet.N - 1)];
        for (int i = 0; i < HingeGameObjects.Length; i++) {
            HingeGameObjects[i] = Instantiate(EdgePrefab);
            HingeGameObjects[i].name = "SelectedViewer_Hinge";
            HingeGameObjects[i].transform.parent = this.transform;
            HingeGameObjects[i].GetComponent<EdgeScript>().changeMaterial(ArrowMaterial);
            HingeGameObjects[i].GetComponent<EdgeScript>().changeWidth(5.0f);
            //はじめは表示しない
            HingeGameObjects[i].SetActive(false);
        }
        //メッシュを作成
        SetAreaSpringMesh();
        //ヒンジタイプに応じて対応するvertexのオブジェクト設定
        int hingecnt = 0;
        for (int i = 0; i < (sheet.N - 2) * (sheet.N - 1); i++) {
            int h = i / (sheet.N - 1); int w = i % (sheet.N - 1);
            int index1 = (sheet.N) * (h + 1) + w;
            HingeGameObjects[hingecnt].GetComponent<EdgeScript>().sphere1 = sheet.Vertices[index1];
            HingeGameObjects[hingecnt].GetComponent<EdgeScript>().sphere2 = sheet.Vertices[index1 + 1];
            hingecnt++;
        }
        for (int i = 0; i < (sheet.N - 1) * (sheet.N - 2); i++) {
            int h = i / (sheet.N - 2); int w = i % (sheet.N - 2);
            int index1 = (sheet.N) * h + w + 1;
            HingeGameObjects[hingecnt].GetComponent<EdgeScript>().sphere1 = sheet.Vertices[index1];
            HingeGameObjects[hingecnt].GetComponent<EdgeScript>().sphere2 = sheet.Vertices[index1 + sheet.N];
            hingecnt++;
        }
        for (int i = 0; i < (sheet.N - 1) * (sheet.N - 1); i++) {
            int h = i / (sheet.N - 1); int w = i % (sheet.N - 1);
            int index1 = sheet.N * h + w + 1;
            HingeGameObjects[hingecnt].GetComponent<EdgeScript>().sphere1 = sheet.Vertices[index1];
            HingeGameObjects[hingecnt].GetComponent<EdgeScript>().sphere2 = sheet.Vertices[index1 + sheet.N - 1];
            hingecnt++;
        }
	}
	
    //表示するものを切り替える
    public void ToggleVisible(){
        for (int i = 0; i < AreaGameObjects.Length; i++) changeAreaMaterial(i);
        for (int i = 0; i < (sheet.N - 2) * (sheet.N - 1); i++) changeHingeMaterial(0, i);
        for (int i = 0; i < (sheet.N - 1) * (sheet.N - 2); i++) changeHingeMaterial(1, i);
        for (int i = 0; i < (sheet.N - 1) * (sheet.N - 1); i++) changeHingeMaterial(2, i);
    }
    //選択した場合に変更されるnaturalLengthに応じてMaterialを変更する
    public void changeAreaMaterial(int surfaceindex) {
        int duration_index = sheet.SurfaceSpring_NaturalDurationArray[surfaceindex];
        switch (duration_index) {
            case 0: //0.5
                AreaGameObjects[surfaceindex].GetComponent<Renderer>().material = AreaMaterials[0];
                AreaGameObjects[surfaceindex].SetActive(true);
                break;
            case 1: //1.0
                AreaGameObjects[surfaceindex].SetActive(false);
                break;
            case 2: //2.0
                AreaGameObjects[surfaceindex].GetComponent<Renderer>().material = AreaMaterials[1];
                AreaGameObjects[surfaceindex].SetActive(true);
                break;
            case 3: //3.0
                AreaGameObjects[surfaceindex].GetComponent<Renderer>().material = AreaMaterials[2];
                AreaGameObjects[surfaceindex].SetActive(true);
                break;
        }
    }
    //選択した場合に変更されるnaturalBendAngleに応じてMaterialを変更する
    public void changeHingeMaterial(int hinge_type, int hinge_index) {
        //hinge_typeとhinge_indexからHingeGameObjectsのindexを作成
        int hingegameobject_index = hinge_index;
        if (hinge_type >= 1) hingegameobject_index += (sheet.N - 2) * (sheet.N - 1);
        if (hinge_type >= 2) hingegameobject_index += (sheet.N - 1) * (sheet.N - 2);
        int duration_index = sheet.Hinge_NaturalDurationAarray[hinge_type][hinge_index];
        if (duration_index == 6) { //0度
            HingeGameObjects[hingegameobject_index].SetActive(false);
        }else if(duration_index <= 5){
            HingeGameObjects[hingegameobject_index].GetComponent<EdgeScript>().changeMaterial(HingeMaterials[duration_index]);
            HingeGameObjects[hingegameobject_index].SetActive(true);
        }else {
            HingeGameObjects[hingegameobject_index].GetComponent<EdgeScript>().changeMaterial(HingeMaterials[duration_index - 1]);
            HingeGameObjects[hingegameobject_index].SetActive(true);
        }
    }
    //SelectedViewerを隠す
    public void HideSelectedViweer() {
        foreach (var g in AreaGameObjects) g.SetActive(false);
        foreach (var g in HingeGameObjects) g.SetActive(false);
    }
    //changeMaterialを呼ぶことで、DurationArrayに応じてSelectedViewerを再描画（重いのでUpdateでは呼ばない)
    public void RedrawSelectedViewer() {
        for (int i = 0; i < AreaGameObjects.Length; i++) changeAreaMaterial(i);
        for (int i = 0; i < (sheet.N - 2) * (sheet.N - 1); i++) changeHingeMaterial(0, i);
        for (int i = 0; i < (sheet.N - 1) * (sheet.N - 2); i++) changeHingeMaterial(1, i);
        for (int i = 0; i < (sheet.N - 1) * (sheet.N - 1); i++) changeHingeMaterial(2, i);
    }
	void Update () {
		UpdateAreaSpringSelectedVisibleViewPosition();
        UpdateHingeStencilSpringSelectedVisiblePosition();
    }

    private void SetAreaSpringMesh() {
        for (int i = 0; i < AreaGameObjects.Length; i++) {
            Mesh tmpMesh = new Mesh();
            if (i % 2 == 0) {
                //(k, k+1, k+N)のtriangle
                int tmp = i / 2;
                int k = sheet.N * (tmp / (sheet.N - 1)) + (tmp % (sheet.N - 1));
                tmpMesh.vertices = new Vector3[] { sheet.Vertices[k].transform.position, sheet.Vertices[k + 1].transform.position, sheet.Vertices[k + sheet.N].transform.position };
            }
            else {
                //(k, k+N, k+N-1)のtriangle
                int tmp = (i - 1) / 2;
                int k = sheet.N * (tmp / (sheet.N - 1)) + (tmp % (sheet.N - 1)) + 1;
                tmpMesh.vertices = new Vector3[] { sheet.Vertices[k].transform.position, sheet.Vertices[k + sheet.N - 1].transform.position, sheet.Vertices[k + sheet.N].transform.position };
            }
            tmpMesh.triangles = new int[3] { 0, 1, 2 };
            tmpMesh.RecalculateNormals();
            tmpMesh.RecalculateBounds();
            AreaGameObjects[i].GetComponent<MeshFilter>().sharedMesh = tmpMesh;
        }
    }
	private void UpdateAreaSpringSelectedVisibleViewPosition(){
		Vector3 x = new Vector3 (0, 1.0f, 0);
		for (int i = 0; i < AreaGameObjects.Length; i++) {
			if (i % 2 == 0) {
				//(k, k+1, k+N)のtriangle
				int tmp = i / 2;
				int k = sheet.N * (tmp / (sheet.N - 1)) + (tmp % (sheet.N - 1));
				AreaGameObjects[i].GetComponent<MeshFilter>().mesh.vertices = new Vector3[] { sheet.Vertices[k].transform.position + x, sheet.Vertices[k + 1].transform.position + x, sheet.Vertices[k + sheet.N].transform.position + x};
			}
			else {
				//(k, k+N, k+N-1)のtriangle
				int tmp = (i - 1) / 2;
				int k = sheet.N * (tmp / (sheet.N - 1)) + (tmp % (sheet.N - 1)) + 1;
				AreaGameObjects[i].GetComponent<MeshFilter>().mesh.vertices = new Vector3[] { sheet.Vertices[k].transform.position + x, sheet.Vertices[k + sheet.N - 1].transform.position + x, sheet.Vertices[k + sheet.N].transform.position  + x};
			}
			AreaGameObjects[i].GetComponent<MeshFilter>().mesh.RecalculateNormals();
			AreaGameObjects[i].GetComponent<MeshFilter>().mesh.RecalculateBounds();
		}
	}
    private void UpdateHingeStencilSpringSelectedVisiblePosition() {
        //ヒンジの表示はTriangleSheetのEdgeを利用しているので点が動けばEdgeScript.csのUpdate()内で勝手に座標更新されるので何もしなくていい
    }
}
