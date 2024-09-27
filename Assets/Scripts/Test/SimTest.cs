using ComputeShaderUtility;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering;

public class SimTest : MonoBehaviour
{
    public ComputeShader computeShader;
    [SerializeField, HideInInspector] private RenderTexture renderTexture;
    [SerializeField, HideInInspector] private RenderTexture tempBuffer;

    private int mainKernel;
    private int applyChangeKernel;

    public int resetAfterIterations = 100;
    public int currentIteration = 0;
    private float time = 0.0f;

    public int width;
    public int height;

    public FilterMode filterMode = FilterMode.Point;
    public GraphicsFormat format = ComputeHelper.defaultGraphicsFormat;

    Vector2 initPos1;
    Vector2 initPos2;

    void Start()
    {
        

        // Initialize the render texture
        ComputeHelper.CreateRenderTexture(ref renderTexture, width, height, filterMode, format);
        ComputeHelper.CreateRenderTexture(ref tempBuffer, width, height, filterMode, format);

        mainKernel = computeShader.FindKernel("CSMain");
        //applyChangeKernel = computeShader.FindKernel("ApplyChanges");

        initPos1 = new Vector2((float)(width / 3.0), (float)(height / 4.0));
        initPos2 = new Vector2((float)(2 * width / 3.0), (float)(height / 4.0));
        computeShader.SetInt("width", width);
        computeShader.SetInt("height", height);
        //computeShader.SetFloat("time", time);
        computeShader.SetVector("initPos1", initPos1);
        computeShader.SetVector("initPos2", initPos2);
        //computeShader.SetTexture(mainKernel, "TempBuffer", tempBuffer);
        // Update the texture using your compute shader
        //computeShader.SetTexture(kernel, "Result", renderTexture);

        transform.GetComponentInChildren<MeshRenderer>().material.mainTexture = renderTexture;
    }

    void Update()
    {
        time += Time.deltaTime;

        computeShader.SetFloat("time", time);

        // Update the texture using your compute shader
        computeShader.SetTexture(mainKernel, "Result", renderTexture);
        computeShader.SetTexture(applyChangeKernel, "TempBuffer", tempBuffer);

        ComputeHelper.Dispatch(computeShader, renderTexture.width, renderTexture.height, 1, kernelIndex : mainKernel);

        //computeShader.SetTexture(applyChangeKernel, "Result", renderTexture);
        //computeShader.SetTexture(applyChangeKernel, "TempBuffer", tempBuffer);
        //ComputeHelper.Dispatch(computeShader, renderTexture.width, renderTexture.height, 1, kernelIndex: applyChangeKernel);


    }

    private void LateUpdate()
    {
        // Increment the iteration counter
        currentIteration++;

        // Check if it's time to reset the texture

        if (currentIteration >= resetAfterIterations)
        {
            ComputeHelper.ClearRenderTexture(renderTexture);
            currentIteration = 0;
            time = 0;

        }

        ComputeHelper.ClearRenderTexture(tempBuffer);
    }

    void OnDestroy()
    {
        renderTexture.Release();
    }

}
