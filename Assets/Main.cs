using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
public class Main : MonoBehaviour {
    #region GUIComponent
    public Slider XSlider, YSlider, ZSlider, DeltaSlider, NSlider;
    public Text XValText, YValText, ZValText,DeltaSliderText;
    public Text StartStopButtonText, NText;
    public Dropdown HingeNaturalBendangleDropdown, SurfaceSpringNaturalDropdown;
    public Toggle toggle1, toggle2, toggle3;
    public Slider SpeedSlider1, SpeedSlider2, SpeedSlider3;
    public Text SpeedSliderText1, SpeedSliderText2, SpeedSliderText3;
    public Slider[] SpringConstantSliders;
    public Text[] SpringConstantTexts;
    #endregion
    
    public float MaxForceSizeXYZ, MinForceSizeXYZ, SpringConstant;
    private Vector3 ExternalForce;
    public float delta;
    private float timeCounter;
    private GameObject ManageSheet; //シミュレーションするシート
    public GameObject TriangleSheetPrefab;
    // Use this for initialization
    void Start () {
        ManageSheet = Instantiate(TriangleSheetPrefab);
        //GUI初期化
        GUIInitialize();
	}
    void Update() {
        
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
            this.StartStopButtonText.text = "Stop";
        }
        else {
            this.StartStopButtonText.text = "Start";
        }
    }
    public void OnParamResetButtonPressed() {
        XSlider.value = YSlider.value = ZSlider.value = 0;
        SpringConstantSliders[0].value = 0.2f;
        SpringConstantSliders[1].value = 0.2f;
        SpringConstantSliders[2].value = 10.0f;

        DeltaSlider.value = 0.001f;
        OnSpringConstantSlider1Changed();
        OnSpringConstantSlider2Changed();
        OnSpringConstantSlider3Changed();
        OnDeltaSliderValueChanged();
        OnXSliderChanged();
        OnYSliderChanged();
        OnZSliderChanged();
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
    public void testButton() {
        for (int i = 0; i < 3; i++) {
            for (int j = 0; j < ManageSheet.GetComponent<TriangleSheet>().Hinge_NaturalDurationAarray[i].Count; j++) {
                ManageSheet.GetComponent<TriangleSheet>().Hinge_NaturalDurationAarray[i][j] = 0;
            }

        }
    }
    #endregion
}
