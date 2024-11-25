using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(ColorCheckerMgr))]
public class ColorCheckerMgrEditor : Editor
{
    ColorCheckerMgr Target;
    void OnEnable()
    {
        Target = (ColorCheckerMgr)target;
    }

    //@@@
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        //SerializedProperty weaponTime = serializedObject.FindProperty("weaponTime");
        //EditorGUILayout.PropertyField(weaponTime, new GUIContent("weaponTime"), true);

        //Target.weaponTime.up = EditorGUILayout.FloatField("weaponUp", Target.weaponTime.up);
        //var newcd = EditorGUILayout.FloatField("weaponCD", Target.weaponTime.cd);
        //if(newcd!=Target.weaponTime.cd)
        //{
        //    Target.weaponTime.cd = newcd;
        //}
        //Target.weaponTime.down = EditorGUILayout.FloatField("weaponDown", Target.weaponTime.down);

        if (GUILayout.Button("GenerateTextureWB"))
        {
            Target.GenerateTextureWB();
        }

        if (GUILayout.Button("Test MathNet"))
        {
            Target.TestMathNet();
        }
        if (GUILayout.Button("ClearCalculation"))
        {
            Target.ClearCalculation();
        }
        if(GUILayout.Button("OutputConfig"))
        {
            Target.OutputConfig();
        }

        serializedObject.ApplyModifiedProperties();
    }
}
