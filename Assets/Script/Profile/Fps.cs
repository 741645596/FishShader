using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Fps : MonoBehaviour
{
	public Text m_FPS;
	float _updateInterval = 1f;//�趨����֡�ʵ�ʱ����Ϊ1��
	float _accum = .0f;//�ۻ�ʱ��
	int _frames = 0;//��_updateIntervalʱ���������˶���֡
	float _timeLeft;

	void Start()
	{
		if (!m_FPS)
		{
			enabled = false;
			return;
		}
		_timeLeft = _updateInterval;
	}

	// Update is called once per frame
	void Update()
	{
		_timeLeft -= Time.deltaTime;
		//Time.timeScale���Կ���Update ��LateUpdate ��ִ���ٶ�,
		//Time.deltaTime��������㣬������һ֡��ʱ��
		//������ɵõ���Ӧ��һ֡���õ�ʱ��
		_accum += Time.timeScale / Time.deltaTime;
		++_frames;//֡��

		if (_timeLeft <= 0)
		{
			float fps = _accum / _frames;
			//Debug.Log(_accum + "__" + _frames);
			string fpsFormat = System.String.Format("{0:F2}FPS", fps);//������λС��
			m_FPS.text = fpsFormat;

			_timeLeft = _updateInterval;
			_accum = .0f;
			_frames = 0;
		}
	}
}
