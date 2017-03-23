using UnityEngine;
using System.Collections;
using System;
using UnityEngine.EventSystems;
// マウスのボタンをあらわす番号がわかりにくかったので名前を付けた
enum MouseButtonDown {
    MBD_LEFT = 0,
    MBD_RIGHT,
    MBD_MIDDLE,
};

public class CameraControlByMouse : MonoBehaviour {
    [SerializeField]    // privateなメンバもインスペクタで編集したいときに付ける
    public GameObject focusObj = null;  // 注視点となるオブジェクト

    private Vector3 oldPos; // マウスの位置を保存する変数
    private float old2dis;
    private bool isprevframe2touch = false; //前フレームで2ポイントタッチをしたかどうか

    // 注視点オブジェクトが未設定の場合、新規に生成する
    void setupFocusObject(string name) {
        GameObject obj = this.focusObj = new GameObject(name);
        obj.transform.position = Vector3.zero;

        return;
    }

    void Start() {
        // 注視点オブジェクトの有無を確認
        if (this.focusObj == null)
            this.setupFocusObject("CameraFocusObject");

        // 注視点オブジェクトをカメラの親にする
        Transform trans = this.transform;
        transform.parent = this.focusObj.transform;

        // カメラの方向ベクトル(ローカルのZ方向)を注視点オブジェクトに向ける
        trans.LookAt(this.focusObj.transform.position);

        return;
    }

    void Update() {
        //UGUIを操作しているときは無効に
        EventSystem current = EventSystem.current;
        if (current != null) {
            if (current.IsPointerOverGameObject()) {
                return;
            }

            foreach (Touch t in Input.touches) {
                if (current.IsPointerOverGameObject(t.fingerId)) {
                    return;
                }
            }
        }
        //モバイルかつペイントモードならカメラ移動無効に
        /*var mainsyscomp = GameObject.Find("MainSystem").GetComponent<MainSystem>();
        if (mainsyscomp.ismobile == true && mainsyscomp.ispaintmode == true) return;*/
        // マウス関係のイベントを関数にまとめる
        this.mouseEvent();

        if (Input.touchCount == 2) isprevframe2touch = true;
        else isprevframe2touch = false;
        // 現在のタッチ座標を次回のために保存する
        if (Input.touchCount == 2) {
            old2dis = (Input.GetTouch(0).position - Input.GetTouch(1).position).magnitude;
        }
    }

    // マウス関係のイベント
    void mouseEvent() {
        // マウスホイールの回転量を取得 deltaは-1から1
        float delta = Input.GetAxis("Mouse ScrollWheel");

        //  2つタッチの距離を調べる -1から1の値になるようにする
        if (isprevframe2touch == true && Input.touchCount >= 2) {
            float dis = (Input.GetTouch(0).position - Input.GetTouch(1).position).magnitude;
            delta = (dis / old2dis) - 1;
            delta *= -1.0f;
            delta = Math.Max(-1.0f, delta);
            delta = Math.Min(1.0f, delta);
        }
        // 回転量が0でなければホイールイベントを処理
        if (delta != 0.0f)
            this.mouseWheelEvent(delta);

        // マウスボタンが押されたタイミングで、マウスの位置を保存する
        if (Input.GetMouseButtonDown((int)MouseButtonDown.MBD_LEFT) ||
            Input.GetMouseButtonDown((int)MouseButtonDown.MBD_MIDDLE) ||
            Input.GetMouseButtonDown((int)MouseButtonDown.MBD_RIGHT))
            this.oldPos = Input.mousePosition;
        // マウスドラッグイベント
        this.DragEvent();

        return;
    }

    // マウスホイールイベント
    void mouseWheelEvent(float delta) {
        // 注視点からカメラまでのベクトルを計算
        Vector3 focusToPosition = this.transform.position - this.focusObj.transform.position;

        // ホイールの回転量を元に上で求めたベクトルの長さを変える
        Vector3 post = focusToPosition * (1.0f + delta);

        // 長さを変えたベクトルの長さが一定以上あれば、カメラの位置を変更する
        if (post.magnitude > 0.01f) {
            //距離が近すぎるor遠すぎると変更キャンセル
            var oldpos = this.transform.position;
            this.transform.position = this.focusObj.transform.position + post;
            focusToPosition = this.transform.position - this.focusObj.transform.position;
            if (focusToPosition.magnitude > 700.0 || focusToPosition.magnitude < 10)
                this.transform.position = oldpos;
        }
        return;
    }

    // マウスドラッグイベント関数
    void DragEvent() {
        Vector3 mousePos = Input.mousePosition;
        // マウスの現在の位置と過去の位置から差分を求める
        Vector3 diff = mousePos - oldPos;

        //タッチされていればタッチを優先
        if (Input.touchCount == 1) {
            diff = (Vector3)Input.GetTouch(0).deltaPosition;//(Vector3)Input.GetTouch(0).position - oldTouches[0]; //
            this.cameraRotate(new Vector3(diff.y, diff.x, 0.0f));
            return; //何故か下のif文でマウス右ぼたんドラッグの場合と同様にやるとうまくいかないのでここでcameraRotateを呼び出す

        }

        // 差分の長さが極小数より小さかったら、ドラッグしていないと判断する
        //if (diff.magnitude < Vector3.kEpsilon)
        //	return;
        if (Input.GetMouseButton((int)MouseButtonDown.MBD_LEFT)) {
            // マウス左ボタンをドラッグした場合(なにもしない)
        } else if (Input.GetMouseButton((int)MouseButtonDown.MBD_MIDDLE)) {
            // マウス中ボタンをドラッグした場合(注視点の移動)
            this.cameraTranslate(-diff / 10.0f);

        } else if (Input.GetMouseButton((int)MouseButtonDown.MBD_RIGHT) || Input.touchCount > 0) {
            // マウス右ボタンをドラッグした場合(カメラの回転)
            this.cameraRotate(new Vector3(diff.y, diff.x, 0.0f));
        }

        // 現在のマウス位置を、次回のために保存する
        this.oldPos = mousePos;
        return;
    }

    // カメラを移動する関数
    void cameraTranslate(Vector3 vec) {
        Transform focusTrans = this.focusObj.transform;
        Transform trans = this.transform;

        // カメラのローカル座標軸を元に注視点オブジェクトを移動する
        focusTrans.Translate((trans.right * -vec.x) + (trans.up * vec.y));

        return;
    }

    // カメラを回転する関数
    public void cameraRotate(Vector3 eulerAngle) {
        Vector3 focusPos = this.focusObj.transform.position;
        Transform trans = this.transform;

        // 回転前のカメラの情報を保存する
        Vector3 preUpV, preAngle, prePos;
        preUpV = trans.up;
        preAngle = trans.localEulerAngles;
        prePos = trans.position;

        // カメラの回転
        // 横方向の回転はグローバル座標系のY軸で回転する
        trans.RotateAround(focusPos, Vector3.up, eulerAngle.y);
        // 縦方向の回転はカメラのローカル座標系のX軸で回転する
        trans.RotateAround(focusPos, trans.right, -eulerAngle.x);

        // カメラを注視点に向ける
        trans.LookAt(focusPos);

        // ジンバルロック対策
        // カメラが真上や真下を向くとジンバルロックがおきる
        // ジンバルロックがおきるとカメラがぐるぐる回ってしまうので、一度に90度以上回るような計算結果になった場合は回転しないようにする(計算を元に戻す)
        Vector3 up = trans.up;
        if (Vector3.Angle(preUpV, up) > 90.0f) {
            trans.localEulerAngles = preAngle;
            trans.position = prePos;
        }

        return;
    }
}
