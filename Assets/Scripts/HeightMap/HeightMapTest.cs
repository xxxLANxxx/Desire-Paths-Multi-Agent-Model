using ComputeShaderUtility;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using static HeightMapSettings;
using static HotspotDistribSettings;
using static SlimeSettings;

public class HeightMapTest : MonoBehaviour
{
    [SerializeField] private ComputeShader heightMapCS;


    [SerializeField, HideInInspector] private RenderTexture heightMap;

    public ComputeBuffer heightMapBuffer;

    public HeightMapSettings heightMapSettings;

    public int width;
    public int height;

    public int seed;

    int heightMapKernel;

    [Header("Display Settings")]
    public FilterMode filterMode = FilterMode.Point;
    public GraphicsFormat format = ComputeHelper.defaultGraphicsFormat;

    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        ComputeHelper.CreateRenderTexture(ref heightMap, width, height, filterMode, format);

        heightMapKernel = heightMapCS.FindKernel("OriginalHeightMapUpdate");

        heightMapCS.SetTexture(heightMapKernel, "OriginalHeightMap", heightMap);


        ComputeHelper.CreateStructuredBuffer(ref heightMapBuffer, heightMapSettings.octaves);
        heightMapCS.SetBuffer(heightMapKernel, "octaves", heightMapBuffer);

        heightMapCS.SetInt("octaveCount", heightMapSettings.octaves.Length);
        heightMapCS.SetInt("width", width);
        heightMapCS.SetInt("height", height);
        heightMapCS.SetInt("seed", seed);
        heightMapCS.SetFloat("xOffset", heightMapSettings.XOffset);
        heightMapCS.SetFloat("yOffset", heightMapSettings.YOffset);

        ComputeHelper.Dispatch(heightMapCS, heightMap.width, heightMap.height, 1, kernelIndex: heightMapKernel);

        Graphics.Blit(heightMap, destination);
    }

    private void OnDestroy()
    {
        ComputeHelper.Release(heightMapBuffer);
    }
}
