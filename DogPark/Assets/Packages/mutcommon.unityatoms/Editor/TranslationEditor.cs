using UnityEngine;
using UnityEditor;

namespace MutCommon.UnityAtoms
{
  [CustomEditor(typeof(Translation), true)]
  public class TranslationEditor : Editor
  {

    void OnSceneGUI()
    {
      if (Application.isPlaying) return;
      var t = target as Translation;
      serializedObject.Update();

      var startProp = serializedObject.FindProperty("start");
      var start = startProp.vector3Value;

      var endProp = serializedObject.FindProperty("end");
      var end = endProp.vector3Value;

      var positionProp = serializedObject.FindProperty("position");
      var position = positionProp.floatValue;

      var accelCurve = (AnimationCurve)serializedObject.FindProperty("accelCurve").animationCurveValue;

      if (serializedObject.FindProperty("preview").boolValue)
      {
        t.transform.position = start + (end - start) * accelCurve.Evaluate(position);
      }
      else
      {
        if (position > 0)
        {
          positionProp.floatValue = 0;
          t.transform.position = start;
        }
        start = t.transform.position;
      }

      using (var cc = new EditorGUI.ChangeCheckScope())
      {
        start = Handles.PositionHandle(start, Quaternion.AngleAxis(180, t.transform.up) * t.transform.rotation);
        Handles.Label(start, "Start", "button");
        Handles.Label(end, "End", "button");
        end = Handles.PositionHandle(end, t.transform.rotation);
        if (cc.changed)
        {
          Undo.RecordObject(t, "Move Handles");
          startProp.vector3Value = start;
          endProp.vector3Value = end;
          t.transform.position = start + (end - start) * position;
          serializedObject.ApplyModifiedProperties();
        }
      }

      Handles.color = Color.yellow;
      Handles.DrawDottedLine(start, end, 5);
      Handles.Label(Vector3.Lerp(start, end, 0.5f), "Distance:" + (end - start).magnitude);
    }
  }
}
