using UnityEngine;
using System.Collections;

public class EdgeScript : MonoBehaviour {
    
    public GameObject sphere1;
    public GameObject sphere2;
    public float naturallength;
    public float nowlength;
    public void CalcNaturalLength() {
        if (sphere1 == null || sphere2 == null) return;
        Vector3 s1 = sphere1.transform.position;
        Vector3 s2 = sphere2.transform.position;
        naturallength = Vector3.Distance(s1, s2);
    }
	void Start () {
        
    }
    //現在の長さは外部から呼ばれたときにはじめて再計算される
	public void CalcNowLength() {
        Vector3 s1 = sphere1.transform.position;
        Vector3 s2 = sphere2.transform.position;
        nowlength = Vector3.Distance(s1, s2);
    }
	//毎フレーム描画更新（長さは毎フレームは更新しない,外部からCalcNowLengthを呼んで更新）
	void Update () {
        if (sphere1 == null || sphere2 == null) return;
        LineRenderer line = this.gameObject.GetComponent<LineRenderer>();
        Vector3 s1 = sphere1.transform.position;
        Vector3 s2 = sphere2.transform.position;
        line.SetPosition(0, s1);
        line.SetPosition(1, s2);
    }
    
}
