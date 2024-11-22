using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using System;

public enum CheckerMode
{
    Color24_6x4,
}

public struct XColor24
{
    public int r, g, b;
    public XColor24(int r_,int g_,int b_)
    {
        r = r_;
        g = g_;
        b = b_;
    }
}

public enum WBSolveMode
{
    CCM3x3,
}

public class ColorCheckerMgr : MonoBehaviour
{
    public Vector2 uv_checkerStart=new Vector2(0,0);
    public Vector2 uvDir_checkerXDir = new Vector2(1,0);
    public float step_checkXStep = 0.1f;
    public Vector2 uvDir_checkerYDir = new Vector2(0,1);
    public float step_checkYStep = 0.1f;
    public float debug_cubeSize = 0.1f;
    public CheckerMode mode = CheckerMode.Color24_6x4;

    public WBSolveMode solveMode = WBSolveMode.CCM3x3;
    public MeshRenderer mr_targetWB;
    [SerializeField]
    public Texture2D tex_targetWB = null;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Vector3 p0 = GetPosFromUV(uv_checkerStart);
        //Gizmos.DrawWireCube(p0, debug_cubeSize * Vector3.one);
        //Gizmos.color = Color.red;
        Vector3 unit_checkerX = GetOffsetFromUV(uvDir_checkerXDir.normalized * step_checkXStep);
        //Gizmos.DrawWireCube(p0+1* unit_checkerX, debug_cubeSize * Vector3.one);
        //Gizmos.color = Color.green;
        Vector3 unit_checkerY = GetOffsetFromUV(uvDir_checkerYDir.normalized * step_checkYStep);
        //Gizmos.DrawWireCube(p0 + 1 * unit_checkerY, debug_cubeSize * Vector3.one);
        if(GetComponent<MeshRenderer>()==null|| GetComponent<MeshRenderer>().sharedMaterial==null)
        {
            return;
        }
        var tex = (Texture2D)GetComponent<MeshRenderer>().sharedMaterial.GetTexture("_MainTex");
        if(tex==null)
        {
            return;
        }
        tex.filterMode = FilterMode.Point;
        tex.wrapMode = TextureWrapMode.Clamp;
        if(tex!=null&&mode == CheckerMode.Color24_6x4)
        {
            for(int j=0;j<4;j++)
            {
                for(int i=0;i<6;i++)
                {
                    Vector3 p=p0+i* unit_checkerX+j* unit_checkerY;
                    var c = GetColorFromUV(tex, uv_checkerStart + uvDir_checkerXDir.normalized * step_checkXStep * i + uvDir_checkerYDir.normalized * step_checkYStep * j);
                    Gizmos.color = c;
                    //Debug.Log("color " + i + " " + j + " " + c);
                    Gizmos.DrawCube(p, debug_cubeSize * Vector3.one);
                    var c_true = GetTrueColorByID(mode,i+6*j);
                    Gizmos.color = c_true;
                    Gizmos.DrawCube(p-transform.forward* debug_cubeSize, debug_cubeSize*0.4f * Vector3.one);
                }
            }
        }
    }
    //######################################################################3
    public void GenerateTextureWB()
    {
        if(solveMode == WBSolveMode.CCM3x3)
        {
            var tex = (Texture2D)GetComponent<MeshRenderer>().sharedMaterial.GetTexture("_MainTex");
            tex.filterMode = FilterMode.Point;
            tex.wrapMode = TextureWrapMode.Clamp;
            if (tex != null && mode == CheckerMode.Color24_6x4)
            {
                double[,] A_array = new double[24, 3];
                for (int j = 0; j < 4; j++)
                {
                    for (int i = 0; i < 6; i++)
                    {
                        var c = GetColorFromUV(tex, uv_checkerStart + uvDir_checkerXDir.normalized * step_checkXStep * i + uvDir_checkerYDir.normalized * step_checkYStep * j);
                        int id = i + j * 6;
                        A_array[id,0] = c.r;
                        A_array[id,1] = c.g;
                        A_array[id,2] = c.b;
                    }
                }
                Matrix<double> mat_A = DenseMatrix.OfArray(A_array);
                Matrix<double> mat_B = DenseMatrix.OfArray(MakeArrayOfTrueColor24x3());
                //--debug
                for (int i = 0; i < 24; i++)
                {
                    var c0 = new Color((float)mat_A[i, 0], (float)mat_A[i, 1], (float)mat_A[i, 2]);
                    var c1 = new Color((float)mat_B[i, 0], (float)mat_B[i, 1], (float)mat_B[i, 2]);
                    Debug.Log("c"+i+"_input: " + c0);
                    Debug.Log("c"+i+"_true: " + c1);
                }
                //mat_B = mat_A;
                //mat_A = DenseMatrix.OfArray(new double[1, 3] { { c0.r, c0.g, c0.b } });
                //mat_B = DenseMatrix.OfArray(new double[1, 3] { { c1.r, c1.g, c1.b } });
                //___
                //(AT*A)^(-1)*AT*B
                var mat_AT = mat_A.Transpose();
                Matrix<double> mat_X = (mat_AT*mat_A).Inverse()* mat_AT* mat_B;
                Debug.Log(mat_X);
                CCMNormalizeMatX(ref mat_X);
                if(tex_targetWB==null)
                {
                    tex_targetWB = new Texture2D(tex.width, tex.height, TextureFormat.RGBAFloat,false,true);
                    tex_targetWB.filterMode = FilterMode.Point;
                    tex_targetWB.wrapMode = TextureWrapMode.Clamp;
                }
                DoWB(tex, ref tex_targetWB, mat_X);
                mr_targetWB.sharedMaterial.SetTexture("_MainTex", tex_targetWB);
            }
            else
            {
                Debug.LogError("NOT tex != null && mode == CheckerMode.Color24_6x4");
            }
        }
        else
        {
            Debug.LogError("Unhandle");
            return;
        }
    }

    public void ClearCalculation()
    {
        //DestroyImmediate(mat_targetWB);
        //mat_targetWB = null;
        DestroyImmediate(tex_targetWB);
        tex_targetWB = null;
        mr_targetWB.sharedMaterial.SetTexture("_MainTex", null);
    }

    public void TestMathNet()
    {
        Matrix<double> mat1 = DenseMatrix.OfArray(new double[,] {
        {1,0,0,1},
        {0,1,0,1},
        {0,0,1,1},
        { 0,0,0,1} });
        
        Vector<double> vec = Vector<double>.Build.DenseOfArray(new double[] { 1, 2, 3, 1 });
        var re = mat1 * vec;
        Debug.Log(re+" "+re[0]+" " + re[1]);
        //var mat1_rev = mat1.Inverse();

        //Matrix<double> mat_B = DenseMatrix.OfArray(MakeArrayOfTrueColor24x3());
        //Debug.Log(mat_B);
    }
    //#####################################################################
    Vector3 GetPosFromUV(Vector2 uv)
    {
        Vector3 uDir = transform.right;
        Vector3 vDir = transform.up;
        Vector3 uv_p = transform.position +(- 0.5f+uv.x) * uDir +(- 0.5f+uv.y) * vDir;
        return uv_p;
    }

    Vector3 GetOffsetFromUV(Vector2 uv)
    {
        Vector3 uDir = transform.right;
        Vector3 vDir = transform.up;
        return uv.x * uDir+uv.y*vDir;
    }

    Color GetColorFromUV(Texture2D tex, Vector2 uv)
    {
        return tex.GetPixel((int)(tex.width * uv.x), (int)(tex.height * uv.y));
    }

    public static readonly XColor24[] trueColor24_6x4 = new XColor24[24] {
        //row 1
        new XColor24(115,82 ,69),
        new XColor24(204,161,141),
        new XColor24(101,134,179),
        new XColor24(89,109,61),
        new XColor24(141,137,194),
        new XColor24(132,228,208),
        //row2
        new XColor24(249,118,35),
        new XColor24(80,91,182),
        new XColor24(222,91,125),
        new XColor24(91,63,123),
        new XColor24(173,232,91),
        new XColor24(255,164,26),
        //row 3
        new XColor24(44,56,142),
        new XColor24(74,148,81),
        new XColor24(179,42,50),
        new XColor24(250,226,21),
        new XColor24(191,81,160),
        new XColor24(6,142,172),
        //row4
        new XColor24(252,252,252),
        new XColor24(230,230,230),
        new XColor24(200,200,200),
        new XColor24(143,143,142),
        new XColor24(100,100,100),
        new XColor24(50,50,50)
    };
    static Color FromTrueColor(XColor24 xc)
    {
        return new Color(xc.r / 255.0f, xc.g / 255.0f, xc.b / 255.0f, 1.0f);
    }
    static Color GetTrueColorByID(CheckerMode mode,int id)
    {
        if(mode == CheckerMode.Color24_6x4)
        {
            return FromTrueColor(trueColor24_6x4[id]);
        }
        else
        {
            Debug.LogError("Unhandle");
            return Color.black;
        }
    }

    double[,] MakeArrayOfTrueColor24x3()
    {
        double[,] re = new double[24,3];
        for(int i=0;i<24;i++)
        {
            re[i, 0] = trueColor24_6x4[i].r / 255.0;
            re[i, 1] = trueColor24_6x4[i].g / 255.0;
            re[i, 2] = trueColor24_6x4[i].b / 255.0;
        }
        return re;
    }

    void DoWB(Texture2D tex, ref Texture2D tex_targetWB, in Matrix<double> mat_X)
    {
        var colors = tex.GetPixels();
        var re = new Color[colors.Length];
        for(int i=0;i<colors.Length;i++)
        {
            double[] vecArr = new double[3];
            vecArr[0] = colors[i].r;
            vecArr[1] = colors[i].g;
            vecArr[2] = colors[i].b;
            Vector<double> vec = Vector<double>.Build.DenseOfArray(vecArr);
            var r = mat_X * vec;
            r[0] = Math.Pow(r[0], 2.2);
            r[1] = Math.Pow(r[1], 2.2);
            r[2] = Math.Pow(r[2], 2.2);
            re[i] = new Color((float)r[0], (float)r[1], (float)r[2]);
        }
        //Debug.Log(colors[0]);
        //Debug.Log(re[0]);
        tex_targetWB.SetPixels(re);
        tex_targetWB.Apply();
    }

    void CCMNormalizeMatX(ref Matrix<double> mat_X)
    {
        for(int i=0;i<3;i++)
        {
            double a = mat_X[i, 0];
            double b = mat_X[i, 1];
            double c = mat_X[i, 2];
            double r = (a + b + c);
            mat_X[i, 0] /= r;
            mat_X[i, 1] /= r;
            mat_X[i, 2] /= r;
        }
    }
}
