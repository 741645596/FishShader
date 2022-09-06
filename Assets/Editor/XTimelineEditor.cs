using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


[CustomEditor(typeof(XTimeline))]
class EffectTimelineEditor : UnityEditor.Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        //ShowProperty();
        GUILayout.Box(string.Empty, GUILayout.Width(500), GUILayout.Height(20));
        serializedObject.Update();
        XTimeline com = (XTimeline)target;
        

        if (GUILayout.Button("Add"))
        {
            int length = 0;
            if (com.Nodes != null)
            {
                length = com.Nodes.Length;
            }
            var newArray = new XTimelineNode[length + 1];
            if (length != 0)
            {
                com.Nodes.CopyTo(newArray, 0);
            }
            com.Nodes = newArray;
        }
        if (GUILayout.Button("Sort"))
        {
            Array.Sort<XTimelineNode>(com.Nodes, (p1, p2)=>{
                int ret = p1.delayTime < p2.delayTime ? -1 : 1;
                return ret;
            });
        }
        if (GUILayout.Button("Play"))
        {
            com.Play();
        }
        serializedObject.ApplyModifiedProperties();
        if (GUI.changed)
        {
            EditorUtility.SetDirty(target);
        }
    }

    void ShowProperty()
    {
        XTimeline com = (XTimeline)target;
        var array = com.Nodes;
        if (array != null)
        {
            int length = array.Length;
            int deleteIdx = -1;
            for (int i = 0; i < length; i++)
            {
                EditorGUILayout.LabelField("Element" + (i + 1), EditorStyles.largeLabel);
                if (GUILayout.Button("删除"))
                {
                    deleteIdx = i;
                }
                var unit = array[i];
                unit.effectType = (XTimelineEffectType)EditorGUILayout.EnumFlagsField("特效类型", unit.effectType);
                unit.delayTime = EditorGUILayout.FloatField("延迟时间", unit.delayTime);
                unit.param = EditorGUILayout.TextField("参数", unit.param);
                
                GUILayout.Box(string.Empty, GUILayout.Width(500), GUILayout.Height(3));
            }
            if (deleteIdx != -1)
            {
                Debug.Log("删除特效" + deleteIdx);
                var newArray = new XTimelineNode[length - 1];
                int idx = 0;
                for (int i = 0; i < length; i++)
                {
                    if (i != deleteIdx)
                    {
                        newArray[idx] = array[i];
                        idx++;
                    }
                }
                com.Nodes = newArray;
            }
        }
    }
}
