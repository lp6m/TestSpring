﻿using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
public class Main : MonoBehaviour {
    #region GUIComponent
    public Slider XSlider, YSlider, ZSlider, SpringConstantSlider, DeltaSlider, NSlider;
    public Text XValText, YValText, ZValText, SpringConstantSliderText, DeltaSliderText;
    public Text StartStopButtonText, NText;
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
    #endregion
}
