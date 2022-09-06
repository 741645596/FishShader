using System.Collections.Generic;
using UnityEngine;

public class RendererDepth : MonoBehaviour
{
    private int _oldSortOrder = 0;

	private List<Renderer> _rendererList = new List<Renderer>();

	public int SortingOrder;

	private void Start()
	{

	}

#if UNITY_EDITOR
    private void Update()
    {
        if (_oldSortOrder != SortingOrder)
        {
            _oldSortOrder = SortingOrder;
            SetSortingOrder(SortingOrder);
        }
    }
#endif

    private void GetTarget()
	{
		if (_rendererList.Count == 0)
		{
			GetComponentsInChildren(_rendererList);
		}
	}

	public void SetSortingOrder(int order)
	{
        //Debug.LogError(gameObject.name + " " + order.ToString() + " " + SortingOrder.ToString());
		GetTarget();
		SortingOrder = order;
		if (_rendererList.Count > 0)
		{
			foreach (var renderer in _rendererList)
			{
				renderer.sortingOrder = order;
                //Debug.Log($"Set ParticleSystemRenderer sortingOrder {order}");
			}
		}
	}

    public void AddSortingOrder(int add)
    {
        SortingOrder += add;
        SetSortingOrder(SortingOrder);
    }
}
