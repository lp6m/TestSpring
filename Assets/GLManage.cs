using UnityEngine;
using System.Collections;

//GL関連用に一般化しようとしたが現在はSmoothFaceから呼び出すためだけに書いている
public class GLManage : MonoBehaviour {
    public Vector3 p0, p1, p2; //現在マウスがあたっている部分を表示するための3頂点
    public Vector3 q0, q1, q2, q3; //4活計用
    public Material RedMaterial;
    public Material BlueMaterial;
    //GLによる表示はカメラにアタッチしたスクリプトで行わなければならない
    //(SmoothFaceにわりあてただけだたとうまくいかなかった)

    // Use this for initialization
    void Start() {
    }

    // Update is called once per frame
    void Update() {

    }

    //UnityシステムからRender時に呼び出されるメソッド.ここでGL描画を呼び出す必要がある. GameView用
    void OnPostRender() {
        GameObject sheet = GameObject.Find("TriangleSheet");
        if (sheet != null && sheet.GetComponent<TriangleSheet>().issimulating) return;
        DrawTriangle(p0, p1, p2, Color.red, RedMaterial);
        DrawQuad(q0, q1, q2, q3, Color.blue, BlueMaterial);
    }

    //UnityシステムからRender時に呼び出されるメソッド.ここでGL描画を呼び出す必要がある. Editor用
    /*void OnDrawGizmos() {
        DrawQuad(p0, p1, p2, p3, Color.blue, SmoothFaceMaterial);
        DrawSmoothFaceMask();
    }*/
    private Material createMaterial(Color color) {
        return new Material("Shader \"Lines/Background\" { Properties { _Color (\"Main Color\", Color) = (" + color.r + "," + color.g + "," + color.b + "," + color.a + ") } SubShader { Pass { ZWrite on  Blend SrcAlpha OneMinusSrcAlpha Colormask RGBA Lighting Off Offset 1, 1 Color[_Color] }}}");
    }
    //SmoothFaceの選択された部分のマスク表示を行う
   /* void DrawSmoothFaceMask() {
        if (mainsys.GetComponent<MainSystem>().stopflag == false) return;
        int height = MainSystem.MAP_HEIGHT_NUM;
        int width = MainSystem.MAP_WIDTH_NUM;
        //Stopしていないときは表示しない
        if (mainsys.GetComponent<MainSystem>().stopflag == false) return;

        this.SmoothFaceVerticies = GameObject.Find("SmoothFace").GetComponent<SmoothFace>().SmoothFaceMesh.vertices;
        if (SmoothFaceVerticies == null) return;
        RedMaterial.SetPass(0);
        GL.PushMatrix();
        GL.Begin(GL.QUADS);
        for (int i = 0; i < height; i++) {
            for (int j = 0; j < width; j++) {
                if (mainsys.GetComponent<MainSystem>().maparray[i, j] == 1) {
                    Vector3 v0 = SmoothFaceVerticies[i * (height + 1) + j];
                    Vector3 v1 = SmoothFaceVerticies[i * (height + 1) + j + 1];
                    Vector3 v2 = SmoothFaceVerticies[(i + 1) * (height + 1) + j + 1];
                    Vector3 v3 = SmoothFaceVerticies[(i + 1) * (height + 1) + j];
                    GL.Vertex3(v0.x, v0.y + 3.0f, v0.z);
                    GL.Vertex3(v1.x, v1.y + 3.0f, v1.z);
                    GL.Vertex3(v2.x, v2.y + 3.0f, v2.z);
                    GL.Vertex3(v3.x, v3.y + 3.0f, v3.z);
                }
            }
        }
        GL.End();
        GL.PopMatrix();

    }*/
    //GL.PushMatrix及びGL.PopMatrixは何度も呼び出すと重たくなるのでこの関数をループで何度も呼び出すのは避ける
    void DrawTriangle(Vector3 v0, Vector3 v1, Vector3 v2, Color col, Material mat) {
        if (RedMaterial == null) return;
        mat.SetPass(0);
        GL.PushMatrix();
        GL.Begin(GL.TRIANGLES);
        GL.Color(col);
        GL.Vertex3(v0.x, v0.y + 1.5f, v0.z);
        GL.Vertex3(v1.x, v1.y + 1.5f, v1.z);
        GL.Vertex3(v2.x, v2.y + 1.5f, v2.z);
        GL.End();
        GL.PopMatrix();
    }

    void DrawQuad(Vector3 v0, Vector3 v1, Vector3 v2, Vector3 v3, Color col, Material mat) {
        if (RedMaterial == null) return;
        mat.SetPass(0);
        GL.PushMatrix();
        GL.Begin(GL.QUADS);
        GL.Color(col);
        GL.Vertex3(v0.x, v0.y + 1.5f, v0.z);
        GL.Vertex3(v1.x, v1.y + 1.5f, v1.z);
        GL.Vertex3(v2.x, v2.y + 1.5f, v2.z);
        GL.Vertex3(v3.x, v3.y + 1.5f, v3.z);
        GL.End();
        GL.PopMatrix();
    }
}
