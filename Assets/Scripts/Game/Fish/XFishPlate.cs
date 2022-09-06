using System;
using UnityEngine;

// 组合鱼转盘
public class XFishPlate : MonoBehaviour
{
    [System.Serializable]
    public class XFishPlateNode
    {
        public float speed;
        public Transform tf;
    }
    [SerializeField]
    public XFishPlateNode[] PlateNodes;
    bool m_Rotate;

    private void Start()
    {
        m_Rotate = true;
        var com = GetComponent<XFish>();
        if (com != null)
        {
            com.SetFishPlate(this);
        }
    }

    public void SetNeedRotate(bool bo)
    {
        m_Rotate = bo;
    }

    public void Update()
    {
        if (m_Rotate)
        {
            Vector3 vec = Vector3.zero;
            for (int i = 0; i < PlateNodes.Length; i++)
            {
                var node = PlateNodes[i];
                vec.z = node.speed * Time.deltaTime;
                node.tf.Rotate(vec);
            }
        }
    }
}
