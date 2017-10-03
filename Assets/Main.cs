using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
public class Main : MonoBehaviour {
    #region GUIComponent
    public Slider XSlider, YSlider, ZSlider, DeltaSlider, NSlider;
    public Text XValText, YValText, ZValText,DeltaSliderText;
    public Text NText;//StartStopButtonText,
    public Button StartStopButton;
    public Toggle toggle1, toggle2, toggle3;
    public Slider SpeedSlider1, SpeedSlider2, SpeedSlider3;
    public Text SpeedSliderText1, SpeedSliderText2, SpeedSliderText3;
    public Slider[] SpringConstantSliders;
    public Text[] SpringConstantTexts;
	public Text CamMoveOrPaintToggleText;
    public GameObject SettingsPanel;
    public GameObject PaintPanel;
	public GameObject TutorialPanel;
    public Sprite StartSprite;
    public Sprite StopSprite;
    #endregion
	public bool IsPaintMode = false; //PaintModeならカメラは移動しないがかける PaintModeでないならカメラは移動するがかけない
    public float MaxForceSizeXYZ, MinForceSizeXYZ, SpringConstant;
    private Vector3 ExternalForce;
    public float delta;
    private GameObject ManageSheet; //シミュレーションするシート
    public GameObject TriangleSheetPrefab;
    public Material EdgeMaterial;
    public Toggle[] AreaPalletToggles;
    public Toggle[] HingePalletToggles;
    //ペイントパネルで現在どのチェックボックスがONか
    public class Pallet {
        public string mode;//areaまたはhinge
        public int index;//area: 0-3 hinge: 0 - 12
        public Pallet(string mode, int index) {
            this.mode = mode;
            this.index = index;
        }
    }
    public Pallet pallet = new Pallet("area", 1);
    // Use this for initialization
    void Start () {
        ManageSheet = Instantiate(TriangleSheetPrefab);
        ManageSheet.name = "TriangleSheet";
        //GUI初期化
        GUIInitialize();
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
    public bool nowSelectedViewerVisible = true; //SelectedViewerがtrueか
    public void OpenPaintButtonPressed() {
        if (IsPaintMode) return;
        IsPaintMode = true;
        nowSelectedViewerVisible = true;
        this.ManageSheet.GetComponent<TriangleSheet>().SelectedViewer.GetComponent<SelectedViewer>().RedrawSelectedViewer();
        PaintPanel.SetActive(true);
        
    }
    public void ClosePaintButtonPressed() {
        if (!IsPaintMode) return;
        IsPaintMode = false;
        PaintPanel.SetActive(false);
        //PaintPanelの子を全て削除
        //Paintパネルに塗られたTrianleSheetから色塗りデータを取得
        foreach (Transform n in PaintPanel.transform) {
            if (n.gameObject.name == "plate" || n.gameObject.name == "edge") Destroy(n.gameObject);
        }
    }
    //面積バネのパレットボタンの値が変更されたとき
    public void OnAreaPalletPressed() {
        for (int index = 0; index < 4; index++) {
            if(AreaPalletToggles[index].isOn) pallet = new Pallet("area", index);
        }
    }
    //折り曲げバネのパレットボタンの値が変更されたとき
    public void OnHingePalletPressed() {
        for (int index = 0; index < 13; index++) {
            if(HingePalletToggles[index].isOn) pallet = new Pallet("hinge", index);
        }
    }
    //SelectedViewerの見える・見えないを切り替える
    public void OnToggleSelectedViewerActiveButtonPressed() {
        if (this.IsPaintMode) return;//ペイントモードのときは切り替え不可
        nowSelectedViewerVisible = !nowSelectedViewerVisible;
        var selectedViewer = this.ManageSheet.GetComponent<TriangleSheet>().SelectedViewer.GetComponent<SelectedViewer>();
        if (nowSelectedViewerVisible) selectedViewer.RedrawSelectedViewer();
        else selectedViewer.HideSelectedViweer();
    }
    //Undoボタン
    public void OnUndoButtonPressed() {
        ManageSheet.GetComponent<TriangleSheet>().undoSelected();
    }

	//PresetButton
	public void OnPresetButtonPressed(){
        ManageSheet.GetComponent<TriangleSheet>().GenerateSetPalletString();
        ManageSheet.GetComponent<TriangleSheet>().TogglePreset();
	}

	public void OnTutorialButtonPressed(){
		TutorialPanel.SetActive (true);
	}
	public void CloseTutorialButtonPressed(){
		TutorialPanel.SetActive (false);
	}
    #endregion
}
