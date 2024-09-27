using ComputeShaderUtility;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class LandCoverTest : MonoBehaviour
{
    [SerializeField] private ComputeShader computeShader = null;
    [SerializeField, HideInInspector] private RenderTexture renderTexture = null;

    public ComputeBuffer OriginalLandColorMapBuffer;

    public ComputeBuffer LandSettingsBuffer;

    public LandFieldSettings landFieldSettings;

    public int width;
    public int height;

    int kernel;

    public int seed;

    Land[] landsSettings;


    Texture2D GetTexture2D(RenderTexture rTex)
    {
        Texture2D tex = new Texture2D(rTex.width, rTex.height, TextureFormat.RGBAFloat, false);
        
        // ReadPixels looks at the active RenderTexture.
        RenderTexture.active = new RenderTexture(rTex.width, rTex.height, 0);
        Graphics.Blit(rTex, RenderTexture.active);
        tex.ReadPixels(new Rect(0, 0, rTex.width, rTex.height), 0, 0);
        tex.Apply();
        return tex;
    }

    Land GetDataAtPixel(Land[] lands, int x, int y)
    {
        return lands[x + width * (y - 1)];
    }

    // Start is called before the first frame update
    private void Start()
    {
        
        if (landFieldSettings.landsSettings.Length > 0)
        {
            landsSettings = new Land[landFieldSettings.landsSettings.Length];

            for (int i = 0; i < landsSettings.Length; i++)
            {

                landsSettings[i] = new Land()
                {
                    affordanceIndex = landFieldSettings.landsSettings[i].landResistanceIndex,
                    appearanceThreshold = landFieldSettings.landsSettings[i].appearanceThreshold,
                    landXTh = landFieldSettings.landsSettings[i].landXTh,
                    landColor = landFieldSettings.landsSettings[i].landColor
                };
            }

        }
        

        if (renderTexture == null)
        {
            renderTexture = new RenderTexture(width, height, 16, RenderTextureFormat.ARGB32);
            renderTexture.enableRandomWrite = true;
            renderTexture.Create();
        }

        kernel = computeShader.FindKernel("OriginalLandFieldUpdate");

        ComputeHelper.CreateStructuredBuffer(ref OriginalLandColorMapBuffer, landFieldSettings.octaves);
        computeShader.SetBuffer(kernel, "octaves", OriginalLandColorMapBuffer);

        ComputeHelper.CreateStructuredBuffer(ref LandSettingsBuffer, landsSettings);
        computeShader.SetBuffer(kernel, "landsSettings", LandSettingsBuffer);

        computeShader.SetInt("seed", seed);
        computeShader.SetFloat("xOffset", landFieldSettings.XOffset);
        computeShader.SetFloat("yOffset", landFieldSettings.YOffset);
        computeShader.SetInt("octaveCount", landFieldSettings.octaves.Length);
        computeShader.SetInt("landCount", landFieldSettings.landsSettings.Length);
        computeShader.SetInt("width", width);
        computeShader.SetInt("height", height);

        computeShader.SetTexture(kernel, "OriginalLandColorMap", renderTexture);
        //computeShader.Dispatch(kernel, renderTexture.width / 8, renderTexture.height / 8, 1);
    }

    private void Update()
    {
        for (int i = 0; i < landsSettings.Length; i++)
        {

            landsSettings[i] = new Land()
            {
                affordanceIndex = landFieldSettings.landsSettings[i].landResistanceIndex,
                appearanceThreshold = landFieldSettings.landsSettings[i].appearanceThreshold,
                landXTh = landFieldSettings.landsSettings[i].landXTh,
                landColor = landFieldSettings.landsSettings[i].landColor
            };
        }

        ComputeHelper.CreateStructuredBuffer(ref OriginalLandColorMapBuffer, landFieldSettings.octaves);
        computeShader.SetBuffer(kernel, "octaves", OriginalLandColorMapBuffer);

        ComputeHelper.CreateStructuredBuffer(ref LandSettingsBuffer, landsSettings);
        computeShader.SetBuffer(kernel, "landsSettings", LandSettingsBuffer);

        computeShader.SetInt("seed", seed);
        computeShader.SetFloat("xOffset", landFieldSettings.XOffset);
        computeShader.SetFloat("yOffset", landFieldSettings.YOffset);
        computeShader.SetInt("octaveCount", landFieldSettings.octaves.Length);
        computeShader.SetInt("landCount", landFieldSettings.landsSettings.Length);
        computeShader.SetInt("width", width);
        computeShader.SetInt("height", height);

        computeShader.SetTexture(kernel, "OriginalLandColorMap", renderTexture);
        
        ComputeHelper.Dispatch(computeShader, renderTexture.width, renderTexture.height, 1, kernel);

        // Read data from the OriginalIndexLandMapBuffer
        //storedLandsData = new Land[width * height];
        //OriginalLandDataMapBuffer.GetData(storedLandsData);

        //Debug.Log(GetDataAtPixel(storedLandsData, 500, 400).landResistanceIndex);
    }

    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        Graphics.Blit(renderTexture, destination);
    }

    private void OnDestroy()
    {

        ComputeHelper.Release(LandSettingsBuffer, OriginalLandColorMapBuffer);

    }
}
