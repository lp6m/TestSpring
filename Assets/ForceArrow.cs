using UnityEngine;
using System.Collections;

public class ForceArrow : MonoBehaviour {
    //GameObjectからVector3の矢を表示するだけ
    public GameObject nekko;
    public Vector3 arrowvec;
	// Use this for initialization
	void Start () {
        arrowvec = new Vector3(0.0f, 0.0f, 0.0f);
	}
	
	// Update is called once per frame
	void Update () {
        LineRenderer line = this.gameObject.GetComponent<LineRenderer>();
        line.SetPosition(0, nekko.transform.position);
        line.SetPosition(1, nekko.transform.position + arrowvec);
    }
    void OnDestroy() {
        if(nekko != null) GameObject.Destroy(nekko);
    }
}
