using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace ShakeLibrary {
    public class AnimationShake : MonoBehaviour {
        public Transform Target;
        public List<Vector3> Position = new List<Vector3>();
        public List<Vector2> PositionDurationAndDelay = new List<Vector2>();

        public List<Vector3> Euler = new List<Vector3>();
        public List<Vector2> EulerDurationAndDelay = new List<Vector2>();


        public List<Vector3> Scale = new List<Vector3>();
        public List<Vector2> ScaleDurationAndDelay = new List<Vector2>();

        public int RepeatAnimationOfAll;

        private Vector3 lastPosition;
        private Vector3 lastEuler;
        private Vector3 lastScale;
        List<AnimationBase> animationList;
        private int currentPlayCount;

        private void Init() {
            animationList = new List<AnimationBase>();

            lastPosition = Target.localPosition;
            float delay = 0;
            for (int i = 0; i < Position.Count; i++) {
                var pos = new Vector3(Position[i].x, Position[i].y, Position[i].z);
                pos /= 100.0f;
                float t1 = PositionDurationAndDelay[i].x;
                MoveTo gAnimationMoveTo =
                    new MoveTo(Target, lastPosition, pos, t1, delay + PositionDurationAndDelay[i].y);
                animationList.Add(gAnimationMoveTo);
                lastPosition = pos;
                delay += t1;
            }

            lastEuler = Target.localEulerAngles;
            delay = 0;
            for (int j = 0; j < Euler.Count; j++) {
                var pos = new Vector3(Euler[j].x, Euler[j].y, Euler[j].z);
                float t1 = EulerDurationAndDelay[j].x;
                RotationTo gAnimationRotationTo =
                    new RotationTo(Target, lastEuler, pos, t1, delay + EulerDurationAndDelay[j].y);
                animationList.Add(gAnimationRotationTo);
                lastEuler = pos;
                delay += t1;
            }

            lastScale = Target.localScale;
            delay = 0;
            for (int k = 0; k < Euler.Count; k++) {
                var pos = new Vector3(Scale[k].x, Scale[k].y, Scale[k].z);
                float t1 = ScaleDurationAndDelay[k].x;
                ScaleTo gAnimationScaleTo = new ScaleTo(Target, lastScale, pos, t1, delay + ScaleDurationAndDelay[k].y);
                animationList.Add(gAnimationScaleTo);
                lastScale = pos;
                delay += t1;
            }
        }

        public void ResetToBeginning() {
            if (animationList == null) {
                Init();
            }

            Target.localPosition = Vector3.zero;
            Target.localEulerAngles = Vector3.zero;
            Target.localScale = Vector3.one;
            currentPlayCount = animationList.Count;
            //Debug.Log(currentPlayCount);
            for (int i = 0; i < currentPlayCount; i++) {
                animationList[i].ResetToBeginning();
            }
        }

        public void Stop() {
            Target.localPosition = Vector3.zero;
            Target.localEulerAngles = Vector3.zero;
            Target.localScale = Vector3.one;
            currentPlayCount = 0;
        }

        public void UpdateAnimation(float dt) {
            currentPlayCount = 0;
            for (int i = 0; i < animationList.Count; i++) {
                if (animationList[i].IsPlaying()) {
                    animationList[i].Update(dt);
                    currentPlayCount++;
                    //Debug.Log(animationList[i].ToString());
                }
            }
            //Debug.Log(currentPlayCount);
        }

        public int GetPlayingCount() {
            return currentPlayCount;
        }

        /*private void OnDrawGizmosSelected() {
            Gizmos.color = Color.blue;

            var point_list = Position;
            var point_count = point_list.Count;
            for (int i = 0; i < point_count - 1; i++) {
                Gizmos.DrawLine(point_list[i], point_list[i + 1]);
            }

            Gizmos.color = Color.yellow;
            float radiusOfDot = 0.1f;
            //if you want to add spacing, just iterate i = i+2
            for (int i = 0; i < point_count; i++) {
                Vector3 point = point_list[i];
                
                Gizmos.DrawSphere(point, radiusOfDot);
            }
        }*/
    }
}