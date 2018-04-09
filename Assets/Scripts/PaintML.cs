//#define ACTIVE_IMAGECAPTURE

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.UI;
using System;
#if UNITY_UWP
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Media;
using Windows.Graphics.Imaging;
using System.Runtime.InteropServices.WindowsRuntime;
using number;
#endif

public class PaintML : MonoBehaviour {

    public RenderTexture renderTex;
    public Text TextData;

    private Texture2D tex;
    private byte[] bytes = null;
    private int TexWidth, TexHeight;
    private string onnxName = "number.onnx";

    // Use this for initialization
    void Start() {
        TextData.text = "";
        TexWidth = renderTex.width;
        TexHeight = renderTex.height;
        tex = new Texture2D(TexWidth, TexHeight, TextureFormat.RGBA32, false);
        File.Copy(Application.streamingAssetsPath + "\\" + onnxName, Application.persistentDataPath + "\\" + onnxName, true);

#if ACTIVE_IMAGECAPTURE
        StartCoroutine(SaveCaptureImage(2.0f));
#else
#if UNITY_UWP
        Task.Run(async () => {
            await MachineLearningTask();
        });
#endif
#endif
    }

    // Update is called once per frame
    void Update() {
        if (bytes==null)
        {
            RenderTexture.active = renderTex;
            tex.ReadPixels(new Rect(0, 0, TexWidth, TexHeight), 0, 0);
            tex.Apply();
            bytes = tex.GetRawTextureData();
        }
    }

#if UNITY_UWP
    private async Task MachineLearningTask()
    {
        var ModelInput = new NumberModelInput();
        var ModelGen = new NumberModel();
        var ModelOutput = new NumberModelOutput();
        try
        {
            StorageFile modelFile = await ApplicationData.Current.LocalFolder.GetFileAsync(onnxName);
            ModelGen = await NumberModel.CreateNumberModel(modelFile);
        }
        catch (Exception e)
        {
            UnityEngine.WSA.Application.InvokeOnAppThread(() =>
            {
                TextData.text = e.ToString();
            }, true);
        }

        while (true)
        {
            if (bytes != null)
            {
                // Unityのカメラは上下反転しているので入れ替え処理
                var buf = new byte[bytes.Length];
                for (int i = 0; i < TexHeight; i++)
                {
                    for (int j = 0; j < TexWidth; j++)
                    {
                        buf[(TexWidth * (TexHeight - 1 - i) + j) * 4 + 0] = bytes[(TexWidth * i + j) * 4 + 0];
                        buf[(TexWidth * (TexHeight - 1 - i) + j) * 4 + 1] = bytes[(TexWidth * i + j) * 4 + 1];
                        buf[(TexWidth * (TexHeight - 1 - i) + j) * 4 + 2] = bytes[(TexWidth * i + j) * 4 + 2];
                        buf[(TexWidth * (TexHeight - 1 - i) + j) * 4 + 3] = bytes[(TexWidth * i + j) * 4 + 3];
                    }
                }
                try
                {
                    var softwareBitmap = new SoftwareBitmap(BitmapPixelFormat.Rgba8, TexWidth, TexHeight, BitmapAlphaMode.Premultiplied);
                    softwareBitmap.CopyFromBuffer(buf.AsBuffer());
                    softwareBitmap = SoftwareBitmap.Convert(softwareBitmap, BitmapPixelFormat.Bgra8, BitmapAlphaMode.Premultiplied);

                    ModelInput.data = VideoFrame.CreateWithSoftwareBitmap(softwareBitmap);
                    ModelOutput = await ModelGen.EvaluateAsync(ModelInput);

                    float maxProb = 0;
                    string maxIndexName = "";
                    foreach (var item in ModelOutput.loss)
                    {
                        if (item.Value > maxProb)
                        {
                            maxIndexName = item.Key;
                            maxProb = item.Value;
                        }
                    }
                    softwareBitmap.Dispose();
                    bytes = null;
                    buf = null;
                    UnityEngine.WSA.Application.InvokeOnAppThread(() =>
                    {
                        TextData.text = maxIndexName + ":" + maxProb;
                    }, true);
                }
                catch (Exception e)
                {
                    UnityEngine.WSA.Application.InvokeOnAppThread(() =>
                    {
                        TextData.text = e.ToString();
                    }, true);
                }
            }
        }
    }
#endif

    private IEnumerator SaveCaptureImage(float time)
    {
        while (true)
        {
            yield return new WaitForSeconds(time);
            var now = DateTime.Now;
            File.WriteAllBytes(Application.persistentDataPath + "\\" + now.Hour + now.Minute + now.Second + ".png", tex.EncodeToPNG());
            bytes = null;
        }
    }
}
