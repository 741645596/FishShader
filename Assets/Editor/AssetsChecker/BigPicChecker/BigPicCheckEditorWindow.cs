
using System.Collections.Generic;
using System.Linq;
using EditerUtils;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

/// <summary>
/// 图片检测界面
/// </summary>
public class BigPicCheckEditorWindow : AssetCheckEditorWindowBase<BigPicAssetInfo>
{
    public const string Title = "纹理图片";

    private int _sortIndex = 0;
    private Vector2 _regionMinSize = Vector2.zero;
    private Vector2 _regionMaxSize = Vector2.zero;

    private void _ShowRuleDes()
    {
        const string s_Des = "检查规则：\n" +
            "1、纹理格式建议：ASTC_4x4几乎与原图一致，ASTC_5x5肉眼看不出来，ASTC_6x6仔细看可能会有些模糊。越大的图建议采用越高的纹理压缩\n" +
            "2、图片纹理Alpha通道为1，则AlphaSource建议设置为None\n" +
            "3、纹理过滤模式FilterMode通常为Bilinear，如果开启Mipmap建议设置为Trillinear可以提升表现效果但是耗性能\n" +
            "4、如果不是3D大地图游戏，规定都要关闭Mipmap（unity默认会开启），内存可以节省33%\n" +
            "5、纹理通常不需要开启读写(R&W)选项，这个会导致纹理内存翻倍\n" +
            "推荐纹理设置：带Alpha通道ASTC 5x5，不带Alpha通道ASTC 6x6\n" +
            "备注：所有的全部修复和一键设置按钮只修改当前面板展示内容";
        AssetsCheckUILogic.ShowRuleDes(s_Des);
    }

    private void _ShowCondiation()
    {
        _regionMinSize = EditorGUILayout.Vector2Field("筛选尺寸最小范围，0表示不限制：", _regionMinSize, GUILayout.Width(450));
        _regionMaxSize = EditorGUILayout.Vector2Field("筛选尺寸最大范围，0表示不限制：", _regionMaxSize, GUILayout.Width(450));

        var des = _sortIndex == 0 ? "排序：名称" : "排序：尺寸";
        if (GUILayout.Button(des, GUILayout.Width(100)))
        {
            _sortIndex = _sortIndex == 0 ? 1 : 0;

            Reload();
        }

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("查找", GUILayout.Width(100)))
        {
            Reload();
        }

        EditorGUILayout.Space();
        EditorGUILayout.Space();

        ShowProblemToggle();

        GUI.color = Color.green;
        if (GUILayout.Button("全部修复", GUILayout.Width(100)))
        {
            _FixAllAction();
        }
        GUI.color = Color.white;

        if (GUILayout.Button("一键设置ASTC", GUILayout.Width(100)))
        {
            _SetAllASTC();
        }

        if (GUILayout.Button("推荐纹理设置", GUILayout.Width(100)))
        {
            _RecommonSetting();
        }
        
        EditorGUILayout.EndHorizontal();
    }

    private List<BigPicAssetInfo> _GetInrangeInfos()
    {
        var infos = new List<BigPicAssetInfo>();
        foreach (var info in _assetsInfos)
        {
            var minArea = _regionMinSize.x * _regionMinSize.y;
            var maxArea = _regionMaxSize.x * _regionMaxSize.y;
            var maxArea2 = maxArea == 0 ? float.MaxValue : maxArea;
            if (info.area >= minArea && info.area < maxArea2)
            {
                infos.Add(info);
            }
        }
        return infos;
    }

    private List<BigPicAssetInfo> _GetFilterInfos(List<BigPicAssetInfo> infos)
    {
        if (_isFilter == false)
        {
            return infos;
        }

        var newInfos = new List<BigPicAssetInfo>();
        foreach (var info in infos)
        {
            if (info.IsError())
            {
                newInfos.Add(info);
            }
        }
        return newInfos;
    }

    private List<BigPicAssetInfo> _SortAssetInfo(List<BigPicAssetInfo> showInfos)
    {
        if (_sortIndex == 0)
            showInfos.Sort((a, b) => { return b.area - a.area; });
        else
            showInfos = showInfos.OrderBy((info) => { return info.assetPath; }).ToList();
        return showInfos;
    }

    // 经验总结推荐纹理格式设置：带Alpha通道ASTC 5x5，不带Alpha通道ASTC 6x6
    private void _RecommonSetting()
    {
        FixHelper.FixStep<BigPicAssetInfo>(_showInfos, (info) =>
        {
            info.RecommonFormat();
        },
        (isCancel) =>
        {
            Reload();
        });
    }

    private void _FixAllAction()
    {
        FixHelper.FixStep<BigPicAssetInfo>(_showInfos, (info) =>
        {
            if (info.CanFix() == false)
            {
                return;
            }

            info.Fix();
        },
        (isCancel) =>
        {
            Reload();
        });
    }

    private void _SetAllASTC()
    {
        AssetsCheckUILogic.ShowASTCPopMenu((format) =>
        {
            FixHelper.FixStep<BigPicAssetInfo>(_showInfos, (info) =>
            {
                info.SetTextureFormat(format);
            },
            (isCancel) =>
            {
                Reload();
            });
        });
    }

    private void _SetASTC(BigPicAssetInfo info)
    {
        AssetsCheckUILogic.ShowASTCPopMenu((format) =>
        {
            info.SetTextureFormat(format);
        });
    }

    protected override string OnGetTitle()
    {
        return Title;
    }

    protected override void OnStartCollectAssetInfo(System.Action<List<BigPicAssetInfo>> finishCB)
    {
        BigPicChecker.CollectAssetInfo(finishCB);
    }

    protected override void OnShowCellButton(BigPicAssetInfo info, Rect rect, bool isError)
    {
        // 设置ASTC
        GUI.color = Color.white;
        var newRect = GUILogicHelper.GetButtonRect(rect, 0);
        if (GUI.Button(newRect, "设置ASTC"))
        {
            _SetASTC(info);

            Reload();
        }

        // 修复按钮
        if (isError)
        {
            GUILogicHelper.ShowFourFixBt(rect, 1, () =>
            {
                info.Fix();

                Reload();
            });
        }
    }

    protected override void OnShowTopInfo()
    {
        // 显示规则信息
        _ShowRuleDes();

        // 显示筛选条件
        _ShowCondiation();
    }

    protected override float OnGetTableViewPosY()
    {
        return 255;
    }

    protected override List<BigPicAssetInfo> OnGetShowInfos()
    {
        var newInfos = _GetInrangeInfos();
        var showInfos = _GetFilterInfos(newInfos);

        return _SortAssetInfo(showInfos);
    }
}
