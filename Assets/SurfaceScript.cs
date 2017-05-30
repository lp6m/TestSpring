using UnityEngine;
using System.Collections;

public class SurfaceScript : MonoBehaviour {
    public GameObject sphere1;
    public GameObject sphere2;
    public GameObject sphere3;
    public float natural_area; //初期面積
    public float now_area; //現在の面積
    public void CalcNaturalArea() {
        if (sphere1 == null || sphere2 == null || sphere3 == null) return;
        Vector3 d1 = sphere1.transform.position - sphere2.transform.position;
        Vector3 d2 = sphere1.transform.position - sphere3.transform.position;
        natural_area = Vector3.Magnitude(Vector3.Cross(d1, d2)) / 2.0f;
    }
    void Start() {

    }
    void OnDestroy() {
        if (sphere1 != null) Destroy(sphere1);
        if (sphere2 != null) Destroy(sphere2);
        if (sphere3 != null) Destroy(sphere3);
    }
    //現在の長さは外部から呼ばれたときにはじめて再計算される
    public void CalcNowArea() {
        if (sphere1 == null || sphere2 == null || sphere3 == null) return;
        Vector3 d1 = sphere1.transform.position - sphere2.transform.position;
        Vector3 d2 = sphere1.transform.position - sphere3.transform.position;
        now_area = Vector3.Magnitude(Vector3.Cross(d1, d2)) / 2.0f;
    }
    //（面積は毎フレームは更新しない,外部からCalcNowAreaを呼んで更新）
    void Update() {
        if (sphere1 == null || sphere2 == null || sphere3 == null) return;
    }
}
