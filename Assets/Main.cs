using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
public class Main : MonoBehaviour {
    #region GUIComponent
    public Slider XSlider, YSlider, ZSlider, DeltaSlider, NSlider;
    public Text XValText, YValText, ZValText,DeltaSliderText;
    public Text NText;//StartStopButtonText,
    public Button StartStopButton;
    public Dropdown HingeNaturalBendangleDropdown, SurfaceSpringNaturalDropdown;
    public Toggle toggle1, toggle2, toggle3;
    public Slider SpeedSlider1, SpeedSlider2, SpeedSlider3;
    public Text SpeedSliderText1, SpeedSliderText2, SpeedSliderText3;
    public Slider[] SpringConstantSliders;
    public Text[] SpringConstantTexts;
    public GameObject[] SelectButton;
	public Text CamMoveOrPaintToggleText;
    public GameObject SettingsPanel;
    public GameObject PaintPanel;
    public Sprite StartSprite;
    public Sprite StopSprite;
    #endregion
	public bool IsPaintMode = false; //PaintModeならカメラは移動しないがかける PaintModeでないならカメラは移動するがかけない
    public float MaxForceSizeXYZ, MinForceSizeXYZ, SpringConstant;
    private Vector3 ExternalForce;
    public float delta;
    private GameObject ManageSheet; //シミュレーションするシート
    public GameObject TriangleSheetPrefab;
    public GameObject TrianglePlatePrefab;
    public GameObject TriangleSpritePrefab;
    public Material EdgeMaterial;
    // Use this for initialization
    void Start () {
        ManageSheet = Instantiate(TriangleSheetPrefab);
        ManageSheet.GetComponent<TriangleSheet>().TouchDetection = false;
        ManageSheet.name = "TriangleSheet";
        //GUI初期化
        GUIInitialize();
        nowSelectEdge = new GameObject();
        nowSelectEdge.AddComponent<LineRenderer>();
        var line = nowSelectEdge.GetComponent<LineRenderer>();
        line.material = EdgeMaterial;
        line.material.color = Color.black;
	}
    private void GUIInitialize() {
        XSlider.maxValue = YSlider.maxValue = ZSlider.maxValue = MaxForceSizeXYZ;
        XSlider.minValue = YSlider.minValue = ZSlider.minValue = MinForceSizeXYZ;
        //最初はTriangleSheetから値を取得してGUIに取得
        //(以降はGUI側からTriangleSheetの値を設定する
        TriangleSheet t = ManageSheet.GetComponent<TriangleSheet>();
        XSlider.value = YSlider.value = ZSlider.value = 0;
        XValText.text = YValText.text = ZValText.text = "0";
        DeltaSlider.value = t.delta;
        DeltaSliderText.text = t.delta.ToString();
        NSlider.value = t.N;
        NText.text = t.N.ToString();
        HingeNaturalBendangleDropdown.value = 6; //0度
        SurfaceSpringNaturalDropdown.value = 1; //x1.0f
        toggle1.isOn = t.isSimulateOn[0];
        toggle2.isOn = t.isSimulateOn[1];
        toggle3.isOn = t.isSimulateOn[2];
        SpeedSlider1.value = t.SimulateSpeed[0];
        SpeedSlider2.value = t.SimulateSpeed[1];
        SpeedSlider3.value = t.SimulateSpeed[2];
        SpeedSliderText1.text = SpeedSlider1.value.ToString();
        SpeedSliderText2.text = SpeedSlider2.value.ToString();
        SpeedSliderText3.text = SpeedSlider3.value.ToString();
        for (int i = 0; i < 3; i++) {
            SpringConstantSliders[i].value = t.DefaultSpringConstant[i];
            SpringConstantTexts[i].text = SpringConstantSliders[i].value.ToString();
        }

    }
    private void UpdateExternalForce() {
        ExternalForce = new Vector3(XSlider.value, YSlider.value, ZSlider.value);
        ManageSheet.GetComponent<TriangleSheet>().UpdateExternalForce(ExternalForce);
    }
    #region GUIfunction
    public void OnXSliderChanged() {
        XValText.text = XSlider.value.ToString();
        UpdateExternalForce();
    }
    public void OnYSliderChanged() {
        YValText.text = YSlider.value.ToString();
        UpdateExternalForce();
    }
    public void OnZSliderChanged() {
        ZValText.text = ZSlider.value.ToString();
        UpdateExternalForce();
    }
    public void OnStartStopButtonPressed() {
        ManageSheet.GetComponent<TriangleSheet>().ToggleSimulate();
        if (ManageSheet.GetComponent<TriangleSheet>().issimulating) {
            this.StartStopButton.interactable = true;
            this.StartStopButton.GetComponent<Image>().sprite = StopSprite;
            //this.StartStopButtonText.text = "Stop";
            //for (int i = 0; i < 3; i++) this.SelectButton[i].SetActive(false);
        }
        else {
            this.StartStopButton.interactable = true;
            this.StartStopButton.GetComponent<Image>().sprite = StartSprite;
            //this.StartStopButtonText.text = "Start";
            //for (int i = 0; i < 3; i++) this.SelectButton[i].SetActive(true);
        }
    }
    public void OnParamResetButtonPressed() {
        XSlider.value = YSlider.value = ZSlider.value = 0;
        SpringConstantSliders[0].value = 0.2f;
        SpringConstantSliders[1].value = 0.2f;
        SpringConstantSliders[2].value = 50.0f;

        DeltaSlider.value = 0.001f;
        OnSpringConstantSlider1Changed();
        OnSpringConstantSlider2Changed();
        OnSpringConstantSlider3Changed();
        OnDeltaSliderValueChanged();
        OnXSliderChanged();
        OnYSliderChanged();
        OnZSliderChanged();
		ManageSheet.GetComponent<TriangleSheet> ().DurationReset ();
    }
    public void OnPositionResetButtonPressed() {
        ManageSheet.GetComponent<TriangleSheet>().PositionReset();
    }
    public void OnSpringConstantSlider1Changed() {
        
        SpringConstantTexts[0].text = SpringConstantSliders[0].value.ToString();
        ManageSheet.GetComponent<TriangleSheet>().SpringConstants[0] = SpringConstantSliders[0].value;
    }
    public void OnSpringConstantSlider2Changed() {

        SpringConstantTexts[1].text = SpringConstantSliders[1].value.ToString();
        ManageSheet.GetComponent<TriangleSheet>().SpringConstants[1] = SpringConstantSliders[1].value;
    }
    public void OnSpringConstantSlider3Changed() {

        SpringConstantTexts[2].text = SpringConstantSliders[2].value.ToString();
        ManageSheet.GetComponent<TriangleSheet>().SpringConstants[2] = SpringConstantSliders[2].value;
    }

    public void OnDeltaSliderValueChanged() {
        delta = DeltaSlider.value;
        DeltaSliderText.text = DeltaSlider.value.ToString();
        ManageSheet.GetComponent<TriangleSheet>().delta = delta;
    }

    public void OnNSliderChanged() {
        NSlider.value = ManageSheet.GetComponent<TriangleSheet>().tryChangeNValue((int)NSlider.value);
        NText.text = NSlider.value.ToString();
    }
   
    public void OnHingeNaturalBendangleDropdownChanged() {
        int angle = -180 + HingeNaturalBendangleDropdown.value * 30;
        ManageSheet.GetComponent<TriangleSheet>().natural_bendangle = angle * Mathf.PI / 180.0f;
    }
    public void OnSurfaceSpringNaturalDropdownChanged() {
        float[] values = new float[] { 0.5f, 1.0f, 2.0f, 3.0f };
        ManageSheet.GetComponent<TriangleSheet>().surfacespring_naturalduration = values[SurfaceSpringNaturalDropdown.value];
    }
    public void OnToggle1Changed() {
        ManageSheet.GetComponent<TriangleSheet>().isSimulateOn[0] = toggle1.isOn;
    }
    public void OnToggle2Changed() {
        ManageSheet.GetComponent<TriangleSheet>().isSimulateOn[1] = toggle2.isOn;
    }
    public void OnToggle3Changed() {
        ManageSheet.GetComponent<TriangleSheet>().isSimulateOn[2] = toggle3.isOn;
    }
    public void OnSpeedSlider1Changed() {
        ManageSheet.GetComponent<TriangleSheet>().SimulateSpeed[0] = (int)SpeedSlider1.value;
        SpeedSliderText1.text = SpeedSlider1.value.ToString();
    }
    public void OnSpeedSlider2Changed() {
        ManageSheet.GetComponent<TriangleSheet>().SimulateSpeed[1] = (int)SpeedSlider2.value;
        SpeedSliderText2.text = SpeedSlider2.value.ToString();
    }
    public void OnSpeedSlider3Changed() {
        ManageSheet.GetComponent<TriangleSheet>().SimulateSpeed[2] = (int)SpeedSlider3.value;
        SpeedSliderText3.text = SpeedSlider3.value.ToString();
    }

    public void OnTwoPointSpringSelectButtonPressed() {
        ManageSheet.GetComponent<TriangleSheet>().ChangeSelectVisible();
    }
    public void OnAreaSpringSelectButtonPressed() {
        ManageSheet.GetComponent<TriangleSheet>().ChangeSelectVisible();
    }
    public void OnHingeStencilSpringSelectButtonPressed() {
        ManageSheet.GetComponent<TriangleSheet>().ChangeSelectVisible();
    }
    public void OpenSettingsButtonPressed() {
        SettingsPanel.SetActive(true);
    }
    public void CloseSettingsButtonPressed() {
        SettingsPanel.SetActive(false);
    }
	public void OnCamMoveOrPaintTogglePressed(){
		IsPaintMode = !IsPaintMode;
		if (IsPaintMode)
			CamMoveOrPaintToggleText.text = "move";
		else
			CamMoveOrPaintToggleText.text = "paint";
	}
    public void OpenPaintButtonPressed() {
        PaintPanel.SetActive(true);
        PaintPanelDraw();
        //TriangleSheetを作成してパネルより手前に配置
        
    }
    public void ClosePaintButtonPressed() {
        PaintPanel.SetActive(false);
        //PaintPanelの子を全て削除
        //Paintパネルに塗られたTrianleSheetから色塗りデータを取得
        foreach (Transform n in PaintPanel.transform) {
            if (n.gameObject.name == "plate" || n.gameObject.name == "edge") Destroy(n.gameObject);
        }
    }
    private List<Vector3> paintPanelVertList = new List<Vector3>();
    public void PaintPanelDraw() {
        var canvas = GameObject.Find("Canvas");
        var canvasRect = canvas.GetComponent<RectTransform>();
        int N = this.ManageSheet.GetComponent<TriangleSheet>().N;
        float TriangleDrawAreaScale = 0.6f; //Canvasに対してこのスケールで描画
        float width = Screen.width;
        float height = Screen.height;
        //三角形の1辺の長さを決定する
        float a = Mathf.Min(width * TriangleDrawAreaScale / (Mathf.Sqrt(3) * ((float)N - 1)), height * TriangleDrawAreaScale / ((float)N - 1));
        //三角形の集合の左端の位置
        Vector2 basePos = new Vector2(150f, height - height * TriangleDrawAreaScale / 2.0f);
        Vector2 shakoVecX = new Vector2(Mathf.Sqrt(3.0f) / 2.0f * a, -1 * a / 2.0f);
        Vector2 shakoVecY = new Vector2(Mathf.Sqrt(3.0f) / 2.0f * a, 1 * a / 2.0f);
        paintPanelVertList.Clear();
        for (int i = 0; i < 2 * (N - 1) * (N - 1); i++) {
            Vector2 v1, v2, v3;
            if (i % 2 == 0) {
                //斜交座標を求める
                int xx = (i / 2) % (N - 1);
                int yy = (i / 2) / (N - 1);
                //斜交座標から変換
                v1 = ((float)xx) * shakoVecX + ((float)yy) * shakoVecY;
                v2 = ((float)xx + 1) * shakoVecX + ((float)yy) * shakoVecY;
                v3 = ((float)xx) * shakoVecX + ((float)yy + 1) * shakoVecY;
                v1 += basePos; v2 += basePos; v3 += basePos;
            }
            else {
                //斜交座標を求める
                int xx = ((i - 1) / 2) % (N - 1) + 1;
                int yy = ((i - 1) / 2) / (N - 1);
                //斜交座標から変換
                v1 = ((float)xx) * shakoVecX + ((float)yy) * shakoVecY;
                v2 = ((float)xx - 1) * shakoVecX + ((float)yy + 1) * shakoVecY;
                v3 = ((float)xx) * shakoVecX + ((float)yy + 1) * shakoVecY;
                v1 += basePos; v2 += basePos; v3 += basePos;  
            }
            //v1,v2,v3を頂点とするMeshをもつGameObjectを作成
            GameObject plateObject = Instantiate(TrianglePlatePrefab);
            plateObject.name = "plate";
            plateObject.GetComponent<TrianglePlate>().index = i;
            plateObject.transform.parent = PaintPanel.transform;
            plateObject.transform.position = Vector3.zero;
            //ScreenPosからWorldPosに変換
            Vector3 vv1, vv2, vv3;
            RectTransformUtility.ScreenPointToWorldPointInRectangle(canvasRect, v1, Camera.main, out vv1);
            RectTransformUtility.ScreenPointToWorldPointInRectangle(canvasRect, v2, Camera.main, out vv2);
            RectTransformUtility.ScreenPointToWorldPointInRectangle(canvasRect, v3, Camera.main, out vv3);
            plateObject.GetComponent<TrianglePlate>().SetupMesh(new Vector3[] { vv1, vv2, vv3});
            paintPanelVertList.Add(vv1);
            paintPanelVertList.Add(vv2);
            paintPanelVertList.Add(vv3);
        }

        //三角形の境界線をLineRendererで描画する
        //左上から右下
        for (int i = 0; i < N; i++) {
            GameObject g = new GameObject("edge");
            g.AddComponent<LineRenderer>();
            g.transform.parent = PaintPanel.transform;
            LineRenderer line = g.GetComponent<LineRenderer>();
            Vector2 v1 = shakoVecX * 0 + shakoVecY * i + basePos;
            Vector2 v2 = shakoVecX * (N - 1) + shakoVecY * i + basePos;
            Vector3 vv1, vv2;
            RectTransformUtility.ScreenPointToWorldPointInRectangle(canvasRect, v1, Camera.main, out vv1);
            RectTransformUtility.ScreenPointToWorldPointInRectangle(canvasRect, v2, Camera.main, out vv2);
            line.material = EdgeMaterial;
            line.material.color = Color.white;
            line.startWidth = line.endWidth = 0.2f;
            line.SetPosition(0, vv1);
            line.SetPosition(1, vv2);
        }
        //上から下前半
        for (int i = 0; i < N - 1; i++) {
            GameObject g = new GameObject("edge");
            g.AddComponent<LineRenderer>();
            g.transform.parent = PaintPanel.transform;
            LineRenderer line = g.GetComponent<LineRenderer>();
            Vector2 v1 = shakoVecX * 0 + shakoVecY * (i + 1) + basePos;
            Vector2 v2 = shakoVecX * (i + 1) + shakoVecY * 0 + basePos;
            Vector3 vv1, vv2;
            RectTransformUtility.ScreenPointToWorldPointInRectangle(canvasRect, v1, Camera.main, out vv1);
            RectTransformUtility.ScreenPointToWorldPointInRectangle(canvasRect, v2, Camera.main, out vv2);
            line.material = EdgeMaterial;
            line.material.color = Color.white;
            line.startWidth = line.endWidth = 0.2f;
            line.SetPosition(0, vv1);
            line.SetPosition(1, vv2);
        }
        //上から下後半
        for (int i = 0; i < N - 2; i++) {
            GameObject g = new GameObject("edge");
            g.AddComponent<LineRenderer>();
            g.transform.parent = PaintPanel.transform;
            LineRenderer line = g.GetComponent<LineRenderer>();
            Vector2 v1 = shakoVecX * (i + 1) + shakoVecY * (N - 1) + basePos;
            Vector2 v2 = shakoVecX * (N - 1) + shakoVecY * (i + 1) + basePos;
            Vector3 vv1, vv2;
            RectTransformUtility.ScreenPointToWorldPointInRectangle(canvasRect, v1, Camera.main, out vv1);
            RectTransformUtility.ScreenPointToWorldPointInRectangle(canvasRect, v2, Camera.main, out vv2);
            line.material = EdgeMaterial;
            line.material.color = Color.white;
            line.startWidth = line.endWidth = 0.2f;
            line.SetPosition(0, vv1);
            line.SetPosition(1, vv2);
        }
        //左下から右上
        for (int i = 0; i < N; i++) {
            GameObject g = new GameObject("edge");
            g.AddComponent<LineRenderer>();
            g.transform.parent = PaintPanel.transform;
            LineRenderer line = g.GetComponent<LineRenderer>();
            Vector2 v1 = shakoVecX * i + shakoVecY * 0 + basePos;
            Vector2 v2 = shakoVecX * i + shakoVecY * (N - 1) + basePos;
            Vector3 vv1, vv2;
            RectTransformUtility.ScreenPointToWorldPointInRectangle(canvasRect, v1, Camera.main, out vv1);
            RectTransformUtility.ScreenPointToWorldPointInRectangle(canvasRect, v2, Camera.main, out vv2);
            line.material = EdgeMaterial;
            line.material.color = Color.white;
            line.startWidth = line.endWidth = 0.2f;
            line.SetPosition(0, vv1);
            line.SetPosition(1, vv2);
        }
    }
    void Update() {
        if (Input.GetMouseButton(0) == false) {
            //マウス押してないなら選択線描画しない
            nowSelecting = false;
            nowSelectEdge.SetActive(false);
        }
        //PaintPanelのあたり判定をここで行う
        RaycastHit hit;
        if (!Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit))
            return;

        MeshCollider meshCollider = hit.collider as MeshCollider;
        if (meshCollider == null || meshCollider.sharedMesh == null)
            return;
        Mesh mesh = meshCollider.sharedMesh;
        //GetComponent<Renderer>().material.color = Color.blue;
        if (hit.collider.tag == "Plate" && Input.GetMouseButton(0)) {
            //どの面があたったか調べる
            int index = hit.collider.gameObject.GetComponent<TrianglePlate>().index;
            Debug.Log(index);
            //hit.collider.gameObject.GetComponent<Renderer>().material.color = Color.gray;
            //どの面があたったかわかればどの点が一番近いかわかる
            if (paintPanelVertList.Count < index * 3 + 2) return;
            Vector3 v1 = paintPanelVertList[index * 3];
            Vector3 v2 = paintPanelVertList[index * 3 + 1];
            Vector3 v3 = paintPanelVertList[index * 3 + 2];
            float d1 = Vector3.Distance(v1, hit.point);
            float d2 = Vector3.Distance(v2, hit.point);
            float d3 = Vector3.Distance(v3, hit.point);
            int nearestpoint = index * 3;
            if (d1 > d2 && d3 > d2) nearestpoint = index * 3 + 1;
            if (d1 > d3 && d2 > d3) nearestpoint = index * 3 + 2;
            var line = nowSelectEdge.GetComponent<LineRenderer>();
            if (nowSelecting == false) {
                nowSelectEdge.SetActive(true);
                line.SetPosition(0, paintPanelVertList[nearestpoint]);
                nowSelecting = true;
            }
            else {
                line.SetPosition(1, paintPanelVertList[nearestpoint]);
            }
        }
    }
    #endregion
    int nowSelectPointStartIndex;
    int nowSelectPointEndIndex;
    bool nowSelecting = false;
    GameObject nowSelectEdge;
}
