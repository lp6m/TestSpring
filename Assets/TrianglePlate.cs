using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//座標がかわることのない三角形
public class TrianglePlate : MonoBehaviour {
    public Material material;
	// Use this for initialization
    public int index;
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
	}
    public void SetupMesh(Vector3[] VertexArray) {
        MeshFilter meshfilter = GetComponent<MeshFilter>();
        List<Vector3> VertexList = new List<Vector3>();
        VertexList.Clear();
        foreach (var v in VertexArray) VertexList.Add(v);
        //裏のために頂点コピー
        VertexList.AddRange(VertexList);
        var mesh = new Mesh();
        mesh.vertices = VertexList.ToArray();
        mesh.triangles = new int[] { 0, 1, 2, 0, 2, 1 };
        
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        meshfilter.sharedMesh = mesh;

        GetComponent<MeshCollider>().sharedMesh = mesh;
        GetComponent<Renderer>().material = material;
        this.gameObject.tag = "Plate";
    }
}
