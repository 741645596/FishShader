//using System;
//using System.Collections;
//using System.Collections.Generic;
//using System.IO;
//using System.Text;
//using UnityEditor;
//using UnityEngine;
//using UnityEngine.UI;

//class ResizeAllSprite
//{
//    static string MODIFY_DIR = Application.dataPath + "/GameData/AppRes/";
//    const float SCALE = 720.0f / 1080.0f;

//    [MenuItem("图片尺寸切换/记录原始尺寸")]
//    static void RecordOrginSize()
//    {
        
//    }

//    [MenuItem("图片尺寸切换/修改所有精灵")]
//    static void OverrideAllTexture()
//    {
//        // List<string> filePaths = new List<string>();
//        string imgtype = "*.jpg|*.JPG|*.PNG|*.png";
//        // string[] img_suffix = { ".bmp", ".jpg", ".gif", ".png" };
//        string[] ImageType = imgtype.Split('|');
//        //string dir = Application.dataPath + "/GameData/AppRes/Activity/Bbgs/Bg";
        
//        //获取d盘中a文件夹下所有的图片路径  
//        string[] dirs = Directory.GetFiles(MODIFY_DIR, "*.png", SearchOption.AllDirectories);
//        for (int j = 0; j < dirs.Length; j++)
//        {
//            string path = dirs[j];
//            path = path.Replace(Application.dataPath + "/", "Assets/");
//            Debug.Log(path);
//            Texture2D srcTex = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
//            if (srcTex != null)
//            {
//                int width = (int)(srcTex.width * SCALE);
//                int height = (int)(srcTex.height * SCALE);
//                if (width > 0 && height > 0)
//                {
//                    Texture2D texture = ReSetTextureSize(srcTex, width, height);
//                    Debug.Log($"{path} 原尺寸{srcTex.width} {srcTex.height} 缩放到 {width} {height}");
//                    SaveTexture(texture, path);
//                }
//            }
//        }
//        AssetDatabase.Refresh();
//        Debug.Log("修改所有精灵成功");
//    }

//    [MenuItem("图片尺寸切换/修改所有预制体")]
//    static void SetNativeSizeAllPrefab()
//    {
        
//        string[] files = Directory.GetFiles(MODIFY_DIR, "*.prefab", SearchOption.AllDirectories);
//        for (int i = 0; i < files.Length; i++)
//        {
//            string path = files[i];
//            path = path.Replace(Application.dataPath + "/", "Assets/");
//            Debug.Log($"修改预制体{path}");
//            ModifyPrefab(path);
//        }
//        AssetDatabase.Refresh();
//        Debug.Log("修改所有鱼制体成功");
//    }

    
//    static void ModifyPrefab(string path)
//    {
//        var go = AssetDatabase.LoadAssetAtPath<GameObject>(path);
//        Transform[] transforms = go.GetComponentsInChildren<Transform>();
//        for (int i = 0; i < transforms.Length; i++)
//        {
//            var pos = transforms[i].localPosition;
//            pos *= SCALE;
//            transforms[i].localPosition = pos;
//        }

//        if (go.GetComponent<Image>() != null)
//        {
//            HandleImage(go.GetComponent<Image>());
//        }

//        Image[] images = go.GetComponentsInChildren<Image>();
//        for (int i = 0; i < images.Length; i++)
//        {
//            var image = images[i];
//            HandleImage(image);
//        }
//        EditorUtility.SetDirty(go);
//        PrefabUtility.SavePrefabAsset(go);
//    }

//    static void HandleImage(Image image)
//    {
//        bool resize = true;
//        if (!image.ImagePath.Contains("GameData"))
//        {
//            resize = false;
//        }
//        if (image.type != Image.Type.Simple)
//        {
//            resize = false;
//        }
//        if (resize)
//        {
//            image.SetNativeSize();
//        }
//    }

//    static void SaveTexture(Texture2D texture, string path)
//    {
//        var bytes = texture.EncodeToPNG();
//        File.WriteAllBytes(path, bytes);
//    }


//    static Texture2D ReSetTextureSize(Texture tex, int width, int height)
//    {

//        var rendTex = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32);

//        rendTex.Create();
//        //设置当前渲染目标，设置为激活渲染目标的texture
//        Graphics.SetRenderTarget(rendTex);
//        //将模型、视图和投影矩阵保存到矩阵堆栈顶部，更改模型、视图或投影矩阵会覆盖当前的渲染矩阵。最好使用GL.PushMatrix和GL.PopMatrix保存和回复这些矩阵
//        GL.PushMatrix();
//        //清除当前的渲染缓冲区 清除深度缓冲区 清除颜色缓冲区 清除时使用的颜色
//        //会清除屏幕或正在绘制到的活动RenderTexture、已清除区域受活动视口限制，此操作可能会改变模型、视图、投影矩阵。
//        //多数情况下，摄像机将已配置为清除屏幕或 RenderTexture，因此无需 手动执行此操作。
//        GL.Clear(true, true, Color.clear);
//        //从矩阵堆栈顶部恢复模型、视图、投影矩阵
//        GL.PopMatrix();

//        var mat = new Material(Shader.Find("Unlit/Transparent"));

//        mat.mainTexture = tex;

//        Graphics.SetRenderTarget(rendTex);

//        GL.PushMatrix();
//        //用于设置正交投影。将正交投影加载到投影矩阵中，将标识加载到 模型和视图矩阵中。
//        GL.LoadOrtho();

//        mat.SetPass(0);
//        //开始绘制3D图元
//        GL.Begin(GL.QUADS);
//        //为所有纹理单位均设置当前纹理坐标（x,y）
//        GL.TexCoord2(0, 0);
//        //提交一个顶点
//        GL.Vertex3(0, 0, 0);

//        GL.TexCoord2(0, 1);

//        GL.Vertex3(0, 1, 0);

//        GL.TexCoord2(1, 1);

//        GL.Vertex3(1, 1, 0);

//        GL.TexCoord2(1, 0);

//        GL.Vertex3(1, 0, 0);
//        //结束绘制3d图元
//        GL.End();

//        GL.PopMatrix();

//        var finalTex = new Texture2D(rendTex.width, rendTex.height, TextureFormat.ARGB32, false);

//        RenderTexture.active = rendTex;
//        //将屏幕像素读取到保存的纹理数据中
//        finalTex.ReadPixels(new Rect(0, 0, finalTex.width, finalTex.height), 0, 0);

//        finalTex.Apply();

//        return finalTex;

//    }

//    public enum ImageType
//    {
//        Null,
//        Png,
//        Jpg,
//        Gif,
//        Bmp
//    }
//    /// <summary>
//    /// 获取图片格式
//    /// </summary>
//    private static ImageType GetImageType(byte[] bytes)
//    {
//        byte[] header = new byte[8];
//        Array.Copy(bytes, header, header.Length);
//        ImageType type = ImageType.Null;
//        //读取图片文件头8个字节
//        //Png图片 8字节：89 50 4E 47 0D 0A 1A 0A   =  [1]:P[2]:N[3]:G
//        if (header[0] == 0x89 && header[1] == 0x50 && header[2] == 0x4E && header[3] == 0x47 &&
//            header[4] == 0x0D && header[5] == 0x0A && header[6] == 0x1A && header[7] == 0x0A)
//        {
//            type = ImageType.Png;
//        }
//        //Jpg图片 2字节：FF D8
//        else if (header[0] == 0xFF && header[1] == 0xD8)
//        {
//            type = ImageType.Jpg;
//        }
//        //Gif图片 6字节：47 49 46 38 39|37 61   =   GIF897a
//        else if (header[0] == 0x47 && header[1] == 0x49 && header[2] == 0x46 && header[3] == 0x38 &&
//            (header[4] == 0x39 || header[4] == 0x37) && header[5] == 0x61)
//        {
//            type = ImageType.Gif;
//        }
//        //Bmp图片 2字节：42 4D
//        else if (header[0] == 0x42 && header[1] == 0x4D)
//        {
//            type = ImageType.Bmp;
//        }
//        return type;
//    }

//    private static byte[] _header = null;
//    private static byte[] _buffer = null;

//    public static void FileInfo(string filePath, out byte[] bytes, out Vector2 size)
//    {
//        size = Vector2.zero;
//        FileStream stream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
//        stream.Seek(0, SeekOrigin.Begin);
//        bytes = new byte[stream.Length];
//        stream.Read(bytes, 0, (int)stream.Length);

//        ImageType imageType = GetImageType(bytes);
//        switch (imageType)
//        {
//            case ImageType.Png:
//                {
//                    stream.Seek(0, SeekOrigin.Begin);
//                    _header = new byte[8];
//                    stream.Read(_header, 0, 8);
//                    stream.Seek(8, SeekOrigin.Current);

//                    _buffer = new byte[8];
//                    stream.Read(_buffer, 0, _buffer.Length);

//                    Array.Reverse(_buffer, 0, 4);
//                    Array.Reverse(_buffer, 4, 4);

//                    size.x = BitConverter.ToInt32(_buffer, 0);
//                    size.y = BitConverter.ToInt32(_buffer, 4);
//                }
//                break;
//            case ImageType.Jpg:
//                {
//                    stream.Seek(0, SeekOrigin.Begin);
//                    _header = new byte[2];
//                    stream.Read(_header, 0, 2);
//                    //段类型
//                    int type = -1;
//                    int ff = -1;
//                    //记录当前读取的位置
//                    long ps = 0;
//                    //逐个遍历所以段，查找SOFO段
//                    do
//                    {
//                        do
//                        {
//                            //每个新段的开始标识为oxff，查找下一个新段
//                            ff = stream.ReadByte();
//                            if (ff < 0) //文件结束
//                            {
//                                return;
//                            }
//                        } while (ff != 0xff);

//                        do
//                        {
//                            //段与段之间有一个或多个oxff间隔，跳过这些oxff之后的字节为段标识
//                            type = stream.ReadByte();
//                        } while (type == 0xff);

//                        //记录当前位置
//                        ps = stream.Position;
//                        switch (type)
//                        {
//                            case 0x00:
//                            case 0x01:
//                            case 0xD0:
//                            case 0xD1:
//                            case 0xD2:
//                            case 0xD3:
//                            case 0xD4:
//                            case 0xD5:
//                            case 0xD6:
//                            case 0xD7:
//                                break;
//                            case 0xc0: //SOF0段（图像基本信息）
//                            case 0xc2: //JFIF格式的 SOF0段
//                                {
//                                    //找到SOFO段，解析宽度和高度信息
//                                    //跳过2个自己长度信息和1个字节的精度信息
//                                    stream.Seek(3, SeekOrigin.Current);

//                                    //高度 占2字节 低位高位互换
//                                    size.y = stream.ReadByte() * 256;
//                                    size.y += stream.ReadByte();
//                                    //宽度 占2字节 低位高位互换
//                                    size.x = stream.ReadByte() * 256;
//                                    size.x += stream.ReadByte();
//                                    return;
//                                }
//                            default: //别的段都跳过
//                                     //获取段长度，直接跳过
//                                ps = stream.ReadByte() * 256;
//                                ps = stream.Position + ps + stream.ReadByte() - 2;
//                                break;
//                        }
//                        if (ps + 1 >= stream.Length) //文件结束
//                        {
//                            return;
//                        }
//                        stream.Position = ps; //移动指针
//                    } while (type != 0xda); // 扫描行开始
//                }
//                break;
//            case ImageType.Gif:
//                {
//                    stream.Seek(0, SeekOrigin.Begin);
//                    _header = new byte[6];
//                    stream.Read(_header, 0, 6);

//                    _buffer = new byte[4];
//                    stream.Read(_buffer, 0, _buffer.Length);

//                    size.x = BitConverter.ToInt16(_buffer, 0);
//                    size.y = BitConverter.ToInt16(_buffer, 2);
//                }
//                break;
//            case ImageType.Bmp:
//                {
//                    stream.Seek(0, SeekOrigin.Begin);
//                    _header = new byte[2];
//                    stream.Read(_header, 0, 2);
//                    //跳过16个字节
//                    stream.Seek(16, SeekOrigin.Current);
//                    //bmp图片的宽度信息保存在第 18-21位 4字节
//                    //bmp图片的高度度信息保存在第 22-25位 4字节
//                    _buffer = new byte[8];
//                    stream.Read(_buffer, 0, _buffer.Length);

//                    size.x = BitConverter.ToInt32(_buffer, 0);
//                    size.y = BitConverter.ToInt32(_buffer, 4);
//                }
//                break;
//            default:
//                break;
//        }

//        stream.Close();
//        stream.Dispose();
//    }
//}
