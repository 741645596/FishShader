using System.Collections;
using UnityEngine;

namespace WeiLe.TA
{
    public class FishHit : MonoBehaviour
    {
        public Renderer mr;
        private Material _mat;
        private Coroutine _dying;
        public bool isStartHit = false;
        public bool isDying = false;
        public Color HitColor;
        public float hitFlashTime = 0.1f;
        private bool m_isDying =  true;
        private int hitColorTagID;

        private void Awake()
        {
            _mat = mr.material;
            isStartHit = false;
            isDying = false;
            hitColorTagID = Shader.PropertyToID("_HitColor");
        }
        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            if (isStartHit == true)
            {
                isStartHit = false;
                HitFlash();
            }

            if (isDying == true)
            {
                Dying();
                isDying = false;
            }
        }


        public void HitFlash()
        {
            StartCoroutine(hitFlash());
        }

        IEnumerator hitFlash()
        {
            m_isDying = false;
            _mat.SetColor(hitColorTagID, HitColor);
            yield return new WaitForSeconds(hitFlashTime);
            _mat.SetColor(hitColorTagID, Color.black);
        }


        public void Dying()
        {
            if (_dying != null)
            {
                StopCoroutine(_dying);
            }
            _dying = StartCoroutine(dying());
        }

        public void StopDying()
        {
            StopCoroutine(_dying);
            _mat.SetColor(hitColorTagID, Color.black);
        }

        IEnumerator dying()
        {
            m_isDying = true;
            int times = 0;
            while (m_isDying && times++ < 10)
            {
                _mat.SetColor(hitColorTagID, HitColor);
                yield return new WaitForSeconds(hitFlashTime);
                _mat.SetColor(hitColorTagID, Color.black);
                yield return new WaitForSeconds(hitFlashTime);
            }
        }
    }
}


