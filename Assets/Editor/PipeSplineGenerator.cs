using UnityEngine;
using UnityEditor;
using UnityEngine.Splines;
using System.Collections.Generic;

public class PipeSplineManual : EditorWindow
{
    public Transform[] controlPoints;  // Start, mid, mid2..., End
    public int interpolation = 20;

    [MenuItem("Tools/Pipe/Generate Manual Spline")]
    public static void ShowWindow()
    {
        GetWindow<PipeSplineManual>("Manual Pipe Spline");
    }

    void OnGUI()
    {
        GUILayout.Label("Manual Pipe Centerline Generator", EditorStyles.boldLabel);

        SerializedObject so = new SerializedObject(this);
        SerializedProperty cpProp = so.FindProperty("controlPoints");
        EditorGUILayout.PropertyField(cpProp, true);
        so.ApplyModifiedProperties();

        interpolation = EditorGUILayout.IntSlider("Curve Smoothness", interpolation, 5, 60);

        if (GUILayout.Button("Generate Spline"))
        {
            if (controlPoints == null || controlPoints.Length < 2)
            {
                Debug.LogError("컨트롤 포인트가 최소 2개 이상 필요합니다.");
                return;
            }

            GenerateSpline();
        }
    }

    void GenerateSpline()
    {
        List<Vector3> pts = new List<Vector3>();
        foreach (Transform t in controlPoints)
            pts.Add(t.position);

        // Catmull-Rom 보간
        List<Vector3> curve = CatmullRom(pts, interpolation);

        // Spline 생성
        GameObject splineObj = new GameObject("ManualPipeSpline");
        SplineContainer container = splineObj.AddComponent<SplineContainer>();
        Spline spline = new Spline();
        container.Spline = spline;

        foreach (Vector3 p in curve)
            spline.Add(new BezierKnot(p));

        Debug.Log($"스플라인 생성 완료! Points: {curve.Count}");
    }

    List<Vector3> CatmullRom(List<Vector3> pts, int interp)
    {
        List<Vector3> result = new List<Vector3>();

        for (int i = 0; i < pts.Count - 1; i++)
        {
            Vector3 p0 = (i == 0) ? pts[i] : pts[i - 1];
            Vector3 p1 = pts[i];
            Vector3 p2 = pts[i + 1];
            Vector3 p3 = (i + 2 < pts.Count) ? pts[i + 2] : pts[i + 1];

            for (int step = 0; step < interp; step++)
            {
                float t = step / (float)interp;

                Vector3 pos =
                    0.5f * ((2f * p1) +
                    (-p0 + p2) * t +
                    (2f * p0 - 5f * p1 + 4f * p2 - p3) * (t * t) +
                    (-p0 + 3f * p1 - 3f * p2 + p3) * (t * t * t));

                result.Add(pos);
            }
        }

        return result;
    }
}
