using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//選択された場所を表示するためのオブジェクト
public class SelectedViewer : MonoBehaviour {
    public bool TwoPointSpringSelectedVisible = false;
    public bool AreaSpringSelectedVisible = false;
    public bool HingeStencilSpringSelectedVisible = false;
    private bool[] oldSelectedVisibles = new bool[] { false, false, false };
    public Material RedMaterial;
    public TriangleSheet sheet; //親オブジェクト
    private GameObject[] AreaGameObjects;
	// Use this for initialization
	void Start () {
        //TriangleSheetのNが変わるたびにSelectedViewerは再生成されるので,SelectedViewer内ではNは固定と考えてよい
        //存在する面積分(2 * N-1 * N-1)だけGameObjectを作成する
        AreaGameObjects = new GameObject[2 * (sheet.N - 1) * (sheet.N - 1)];
        for (int i = 0; i < AreaGameObjects.Length; i++) {
            AreaGameObjects[i] = new GameObject();
            AreaGameObjects[i].AddComponent<MeshRenderer>();
            AreaGameObjects[i].AddComponent<MeshFilter>();
            AreaGameObjects[i].AddComponent<Renderer>();
            //はじめは表示しない
            AreaGameObjects[i].SetActive(false);
        }
	}
	
    //表示するものを切り替える
    public void ToggleVisible(bool[] isvisible){
        oldSelectedVisibles[0] = TwoPointSpringSelectedVisible;
        oldSelectedVisibles[1] = AreaSpringSelectedVisible;
        oldSelectedVisibles[2] = HingeStencilSpringSelectedVisible;

        TwoPointSpringSelectedVisible = isvisible[0];
        AreaSpringSelectedVisible  = isvisible[1];
        HingeStencilSpringSelectedVisible = isvisible[2];
        if (TwoPointSpringSelectedVisible) SetTwoPointSpringSelectedViewPosition();
        if (AreaSpringSelectedVisible) {
            //false->trueなので座標も再セット
            SetAreaSpringSelectedVisibleViewPosition();
            for (int i = 0; i < AreaGameObjects.Length; i++) changeAreaMaterial(i);
        }
        else {
            foreach (var g in AreaGameObjects) g.SetActive(false);
        }
        if (HingeStencilSpringSelectedVisible) SetHingeStencilSpringSelectedVisiblePosition();
    }
    //すべて非表示にする シミュレーションがスタートしたとき
    public void AllSelectedDisable() {
        foreach (var g in AreaGameObjects) g.SetActive(false);
    }
    //このコードはなんとかしたほうがいい
    public void OnSimulateStopped() {
        //シミュレーションがストップしたときに選択状態を復元する
        ToggleVisible(oldSelectedVisibles);
    }
    //選択した場合に変更されるnaturalLengthに応じてMaterialを変更する
    public void changeAreaMaterial(int surfaceindex) {
        for (int i = 0; i < AreaGameObjects.Length; i++) {
            if (sheet.SurfaceSpring_NaturalDurationArray[i] != 1.0f) {
                AreaGameObjects[i].GetComponent<Renderer>().material = RedMaterial;
                if(this.sheet.issimulating == false) AreaGameObjects[i].SetActive(true);
            }
            else {
                AreaGameObjects[i].SetActive(false);
            }
        }
    }
	// Update is called once per frame
	void Update () {
        //UpdateではMeshのVisibleの切り替えだけを行う
        //MeshのVisibleがfalse->trueになった瞬間だけMeshの座標を変更すればよい
        #region AreaSpring
        if (AreaSpringSelectedVisible) {
            //foreach (var g in AreaGameObjects) if(g != null) g.SetActive(true);
        }
        else {
            //foreach (var g in AreaGameObjects) if(g != null) g.SetActive(false);
        }
        #endregion
    }

    //表示切替された際は座標を変更する
    private void SetTwoPointSpringSelectedViewPosition() {

    }
    private void SetAreaSpringSelectedVisibleViewPosition() {
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
            var x = AreaGameObjects[i].GetComponent<MeshFilter>();
            x.sharedMesh = tmpMesh;
        }
    }
    private void SetHingeStencilSpringSelectedVisiblePosition() {

    }
}
