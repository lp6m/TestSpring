using UnityEngine;
public class HingeStencil {
    
    public HingeStencil() {
    }

    public struct HingeStencilForce{
        public Vector3 f0, f1, f2, f3; //それぞれの頂点にかかる力 dΘ/dx
        public float theta; //折り曲げ角
    }
    static  public HingeStencilForce calcHingeStencilForce(Vector3 x0, Vector3 x1, Vector3 x2, Vector3 x3){
        HingeStencilForce rst = new HingeStencilForce();
        //x0,x1,x2で囲まれた三角形とx1,x2,x3で囲まれた三角形の折り曲げを計算する
        //https://s3-us-west-1.amazonaws.com/disneyresearch/wp-content/uploads/20150419230612/Discrete-Bending-Forces-and-Their-Jacobians-Paper.pdf 
        float cos_alpha1 = Vector3.Dot(x0 - x1, x2 - x1) / (x0 - x1).magnitude / (x2 - x1).magnitude;
        float cos_alpha2 = Vector3.Dot(x0 - x2, x1 - x2) / (x0 - x2).magnitude / (x1 - x2).magnitude;
        float cos_alpha1dash = Vector3.Dot(x2 - x1, x3 - x1) / (x2 - x1).magnitude / (x3 - x1).magnitude;
        float cos_alpha2dash = Vector3.Dot(x1 - x2, x3 - x2) / (x1 - x2).magnitude / (x3 - x2).magnitude;
        float S = Vector3.Magnitude(Vector3.Cross(x1 - x0, x2 - x0)) / 2.0f;
        float Sdash = Vector3.Magnitude(Vector3.Cross(x1 - x3, x2 - x3)) / 2.0f;
        float h0 = 2.0f * S / (x2 - x1).magnitude;
        float h1 = 2.0f * S / (x2 - x0).magnitude;
        float h2 = 2.0f * S / (x1 - x0).magnitude;
        float h0dash = 2.0f * Sdash / (x2 - x1).magnitude;
        float h1dash = 2.0f * Sdash / (x3 - x2).magnitude;
        float h2dash = 2.0f * Sdash / (x3 - x1).magnitude;
        Vector3 n = Vector3.Cross(x0 - x2, x1 - x2).normalized;
        Vector3 ndash = Vector3.Cross(x1 - x2, x3 - x2).normalized;

        rst.f0 = -1 * n / h0;
        rst.f1 = cos_alpha2 / h1 * n + cos_alpha2dash / h1dash * ndash;
        rst.f2 = cos_alpha1 / h2 * n + cos_alpha1dash / h2dash * ndash;
        rst.f3 = -1 * ndash / h0dash;

        float ndot = Vector3.Dot(n, ndash);
        //誤差修正(単位ベクトルどうしの内積とっても誤差で-1.0から1.0までに収まらずarccosがNaNになることがある
        if (ndot < -1.0f) ndot = -1.0f;
        if (ndot > 1.0f) ndot = 1.0f;
        rst.theta = Mathf.Acos(ndot);

        Vector3 r = (x0 + x1 + x2) / 3.0f;
        Vector3 rdash = (x1 + x2 + x3) / 3.0f;
        float tdot = Vector3.Dot(rdash - r, ndash - n);
        if (tdot < 0) rst.theta = -1 * rst.theta;
        return rst;
    }
}
