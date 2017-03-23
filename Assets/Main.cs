using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
public class Main : MonoBehaviour {
    #region GUIComponent
    public Slider XSlider, YSlider, ZSlider, SpringConstantSlider, DeltaSlider, NSlider;
    public Text XValText, YValText, ZValText, SpringConstantSliderText, DeltaSliderText;
    public Text StartStopButtonText, NText;
    public Dropdown HingeNaturalBendangleDropdown, SurfaceSpringNaturalDropdown;
    public Toggle toggle1, toggle2, toggle3;
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
        SpringConstantSlider.value = t.SpringConstant;
        SpringConstantSliderText.text = t.SpringConstant.ToString();
        DeltaSlider.value = t.delta;
        DeltaSliderText.text = t.delta.ToString();
        NSlider.value = t.N;
        NText.text = t.N.ToString();
        HingeNaturalBendangleDropdown.value = 6; //0度
        SurfaceSpringNaturalDropdown.value = 1; //x1.0f
        toggle1.isOn = t.isSimulateOn[0];
        toggle2.isOn = t.isSimulateOn[1];
        toggle3.isOn = t.isSimulateOn[2];
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
        SpringConstantSlider.value = 0.2f;
        DeltaSlider.value = 0.001f;
        OnSpringConstantSliderChanged();
        OnDeltaSliderValueChanged();
        OnXSliderChanged();
        OnYSliderChanged();
        OnZSliderChanged();
    }
    public void OnPositionResetButtonPressed() {
        ManageSheet.GetComponent<TriangleSheet>().PositionReset();
    }
    public void OnSpringConstantSliderChanged() {
        SpringConstant = SpringConstantSlider.value;
        SpringConstantSliderText.text = SpringConstantSlider.value.ToString();
        ManageSheet.GetComponent<TriangleSheet>().SpringConstant = SpringConstant;
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
    public void testButton() {
        /*foreach (GameObject g in ManageSheets) {
            Vector3 f = g.GetComponent<TriangleSheet>().Vertices[TriangleSheet.N * TriangleSheet.N - 1].transform.position;
            g.GetComponent<TriangleSheet>().Vertices[TriangleSheet.N * TriangleSheet.N - 1].transform.position = new Vector3(f.x, f.y -= 100.0f, f.z);
            g.GetComponent<TriangleSheet>().SurfaceObject.GetComponent<SurfaceObject>().UpdateMesh(g.GetComponent<TriangleSheet>().Vertices);
        }*/
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
    #endregion
}
