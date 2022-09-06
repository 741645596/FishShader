using System;
using UnityEditor;
using UnityEngine;

namespace EditerUtils
{
    public static class AudioHelper
    {
        /// <summary>
        /// 获取音频大小
        /// </summary>
        /// <returns></returns>
        public static float GetAudioLength(string audioPath)
        {
            var clip = AssetDatabase.LoadAssetAtPath<AudioClip>(audioPath);
            if (clip != null)
            {
                return clip.length;
            }
            return 0;
        }
    }
}


