#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using ShakeLibrary;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(AnimationShake))]
public class AnimationShakeEditor : Editor {
    // draw lines between a chosen game object
    // and a selection of added game objects

    void OnSceneGUI() {
        // get the chosen game object
        AnimationShake t = target as AnimationShake;

        if (t == null)
            return;
        List<Vector3> point_list = t.Position;
        if (point_list == null)
            return;

        // grab the center of the parent
        Vector3 center = t.transform.position;

        var point_count = point_list.Count;
        for (int i = 0; i < point_count - 1; i++) {
            Handles.color = Color.yellow;
            Handles.DrawLine(point_list[i], point_list[i + 1]);
        }

        // iterate over game objects added to the array...
        for (int i = 0; i < point_list.Count; i++) {
            // ... and draw a line between them
            Vector3 point = point_list[i];

            Handles.color = Color.magenta;
            point_list[i] =
                Handles.FreeMoveHandle(point, Quaternion.identity,
                    0.5f,
                    Vector3.zero,
                    Handles.DotHandleCap);

            Handles.color = Color.white;

            Handles.Label(point, $"{i}");
        }
    }
}
#endif