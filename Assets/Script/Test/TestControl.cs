using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TestControl : MonoBehaviour
{
    // Start is called before the first frame update
    public Button btnCtrlGoldFish;
    public Button btnCtrlSmallFish;
    public Button btnCtrlbg;
    public Button btnProfile;
    public Button btnShadowLod;
    public Button btnPSLod;
    public Button btnXuliezhen;

    public GameObject bgNode;
    public GameObject GoldFishNode;
    public GameObject SmallFishNode;
    public GameObject profileNode;
    public GameObject XuliezhenNode;

    public List<GameObject> listAllPt = new List<GameObject>();
    private bool isShowPs = true;


    void Awake()
    {
        GetAllPt();
    }

    void Start()
    {
        btnCtrlGoldFish.onClick.AddListener(OnClickBtnGoldFish);
        btnCtrlSmallFish.onClick.AddListener(OnClickBtnSmallFish);
        btnCtrlbg.onClick.AddListener(OnClickBtnbg);
        btnProfile.onClick.AddListener(OnClickProfile);
        btnShadowLod.onClick.AddListener(OnShaderLod);
        btnPSLod.onClick.AddListener(OnClickps);
        btnXuliezhen.onClick.AddListener(OnClickxuliezhen);
    }

    private void GetAllPt()
    {
        foreach (ParticleSystem ps in GetComponentsInChildren<ParticleSystem>())
        {
            listAllPt.Add(ps.gameObject);
        }

        foreach (ParticleSystem ps in bgNode.GetComponentsInChildren<ParticleSystem>())
        {
            listAllPt.Add(ps.gameObject);
        }
        Text text= btnPSLod.gameObject.GetComponentInChildren<Text>();
        text.text += "num:"+listAllPt.Count.ToString();
    }
    // Update is called once per frame
    public void OnClickxuliezhen()
    {
        XuliezhenNode.SetActive(!XuliezhenNode.activeSelf);
    }
    public void OnClickps()
    {
        isShowPs = !isShowPs;
        foreach (GameObject ps in listAllPt)
        {
            ps.SetActive(isShowPs);
        }
    }
    public void OnShaderLod()
    {
        if (Shader.globalMaximumLOD != 300)
        {
            Shader.globalMaximumLOD = 300;
        }
        else
        {
            Shader.globalMaximumLOD = 150;
        }
    }
    public void OnClickProfile()
    {
        profileNode.SetActive(!profileNode.activeSelf);
    }
    public void OnClickBtnbg()
    {
        bgNode.SetActive(!bgNode.activeSelf);
    }
    public void OnClickBtnGoldFish()
    {
        GoldFishNode.SetActive(!GoldFishNode.activeSelf);
    }

    public void OnClickBtnSmallFish()
    {
        SmallFishNode.SetActive(!SmallFishNode.activeSelf);
    }
    void OnDestroy()
    {
        btnCtrlGoldFish.onClick.RemoveListener(OnClickBtnGoldFish);
        btnCtrlSmallFish.onClick.RemoveListener(OnClickBtnSmallFish);
        btnCtrlbg.onClick.RemoveListener(OnClickBtnbg);
        btnProfile.onClick.RemoveListener(OnClickProfile);
        btnXuliezhen.onClick.RemoveListener(OnClickxuliezhen);
        listAllPt.Clear();
    }
}
