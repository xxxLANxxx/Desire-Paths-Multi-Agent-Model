using ComputeShaderUtility;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomGPUTest : MonoBehaviour
{
    [SerializeField] private ComputeShader computeShader = null;
    [SerializeField] private RenderTexture renderTexture = null;

    [SerializeField] private Vector2Int resolution = new Vector2Int(512, 256);

    int kernel = 0;

    // Start is called before the first frame update
    private void Start()
    {
        if (renderTexture == null)
        {
            renderTexture = new RenderTexture(resolution.x, resolution.y, 16, RenderTextureFormat.ARGB32);
            renderTexture.enableRandomWrite = true;
            renderTexture.Create();
        }

        kernel = computeShader.FindKernel("RandomGPUTest");
        
        
        computeShader.SetVector("resolution", new Vector4(resolution.x, resolution.y));
        computeShader.SetFloat("time", Time.time);
        computeShader.SetTexture(kernel, "Result", renderTexture);
        //computeShader.Dispatch(kernel, renderTexture.width / 8, renderTexture.height, 1);
    }

    private void Update()
    {

        computeShader.SetFloat("time", Time.time);
        //computeShader.Dispatch(kernel, renderTexture.width, renderTexture.height, 1);
        ComputeHelper.Dispatch(computeShader, renderTexture.width, renderTexture.height, 1, kernel);
    }

    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        Graphics.Blit(renderTexture, destination);
    }
}

