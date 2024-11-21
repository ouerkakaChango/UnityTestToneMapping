using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

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
public class ColorCheckerMgr : MonoBehaviour
{
    public Vector2 uv_checkerStart=new Vector2(0,0);
    public Vector2 uvDir_checkerXDir = new Vector2(1,0);
    public float step_checkXStep = 0.1f;
    public Vector2 uvDir_checkerYDir = new Vector2(0,1);
    public float step_checkYStep = 0.1f;
    public float debug_cubeSize = 0.1f;
    public CheckerMode mode = CheckerMode.Color24_6x4;
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
        var tex = (Texture2D)GetComponent<MeshRenderer>().sharedMaterial.GetTexture("_MainTex");
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
}
