using ComputeShaderUtility;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using static HotspotDistribSettings;

public class HotspotDistribTest : MonoBehaviour
{
    public ComputeShader hotspotDistribCS;

    public int width;
    public int height;

    public HotspotDistribSettings distribSettings;

    int hotspotUpdateKernel;
    int simpleDistribPatchKernel;
    int visitFreqPatchKernel;

    [Header("Display Settings")]
    public FilterMode filterMode = FilterMode.Point;
    public GraphicsFormat format = ComputeHelper.defaultGraphicsFormat;

    [SerializeField, HideInInspector] private RenderTexture hotspotDistribMap;
    [SerializeField, HideInInspector] private RenderTexture simpleDistribPatchMap;
    [SerializeField, HideInInspector] private RenderTexture visitFreqPatchMap;

    public ComputeBuffer hotspotsBuffer;

    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {

        ComputeHelper.CreateRenderTexture(ref hotspotDistribMap, width, height, filterMode, format);
        ComputeHelper.CreateRenderTexture(ref visitFreqPatchMap, width, height, filterMode, format);
        ComputeHelper.CreateRenderTexture(ref simpleDistribPatchMap, width, height, filterMode, format);

        hotspotUpdateKernel = hotspotDistribCS.FindKernel("HotspotDistribUpdate");
        simpleDistribPatchKernel = hotspotDistribCS.FindKernel("SimpleDistribPatch");
        visitFreqPatchKernel = hotspotDistribCS.FindKernel("VisitFreqPatch");

        hotspotDistribCS.SetTexture(hotspotUpdateKernel, "HotspotDistribMap", hotspotDistribMap);
        hotspotDistribCS.SetTexture(simpleDistribPatchKernel, "SimplePatchMap", simpleDistribPatchMap);
        hotspotDistribCS.SetTexture(visitFreqPatchKernel, "VisitFreqPatchMap", visitFreqPatchMap);


        Hotspot[] hotspots = new Hotspot[distribSettings.hotspotsSettings.Length];
        for (int i = 0; i < hotspots.Length; i++)
        {
            hotspots[i] = new Hotspot
            (
                distribSettings.hotspotsSettings[i].location * new Vector2(width, height),
                distribSettings.hotspotsSettings[i].visitFreq,
                distribSettings.hotspotsSettings[i].attractiveness
            );
        }

        ComputeHelper.CreateAndSetBuffer<Hotspot>(ref hotspotsBuffer, hotspots, hotspotDistribCS, "hotspots", hotspotUpdateKernel);
        hotspotDistribCS.SetBuffer(simpleDistribPatchKernel, "hotspots", hotspotsBuffer);
        hotspotDistribCS.SetBuffer(visitFreqPatchKernel, "hotspots", hotspotsBuffer);

        hotspotDistribCS.SetInt("width", width);
        hotspotDistribCS.SetInt("height", height);
        hotspotDistribCS.SetInt("numHotspots", hotspots.Length);
        hotspotDistribCS.SetFloat("scaleFactor", distribSettings.scaleFactor);

        ComputeHelper.Dispatch(hotspotDistribCS, hotspots.Length, 1, 1, kernelIndex: hotspotUpdateKernel);
        ComputeHelper.Dispatch(hotspotDistribCS, width, height, 1, kernelIndex: simpleDistribPatchKernel);
        ComputeHelper.Dispatch(hotspotDistribCS, width, height, 1, kernelIndex: visitFreqPatchKernel);
        ComputeHelper.CopyRenderTexture(visitFreqPatchMap, hotspotDistribMap);

        //Graphics.Blit(simpleDistribPatchMap, destination);
        //Graphics.Blit(hotspotDistribMap, destination);
        Graphics.Blit(visitFreqPatchMap, destination);
    }

    private void OnDestroy()
    {
        ComputeHelper.Release(hotspotsBuffer);
    }
}

