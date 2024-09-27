using ComputeShaderUtility;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using static HotspotDistribTest;
using static HotspotDistribSettings;
using UnityEngine.Experimental.Rendering;
using System.ComponentModel;
using Unity.Collections;
using System.Linq;
using Unity.VisualScripting;
using static ClearShaderCache;
using UnityEditor;
using static UnityEngine.UIElements.UxmlAttributeDescription;

public class Simulation : MonoBehaviour
{
    //====================================================================================================================================================
    ////////////////////////////////////////////////////////////// Variables //////////////////////////////////////////////////////////////////////
    //====================================================================================================================================================



    ///////////////////////////////////////////////////// Enumerations /////////////////////////////////////////////////////////////////

    public enum DirectionalPathMode { Unidirectional, Bidirectional }
    public enum HeightMapDisplayMode { Detailed, ColorScaledTopography, EdgeDetection };

    public enum HeightMapGenerationMode { Random, Custom, Import };

    public enum HeightMapTopographyMode { Uniform, ColorScale };

    public enum HotspotDistributionMode { Random, Custom, Import };

    //////////////////////////////////////////////// Compute Shaders //////////////////////////////////////////////////////////////

    public ComputeShader computeShader;
    public ComputeShader drawAgentsCS;
    public ComputeShader hotspotDistribCS;
    public ComputeShader heightMapCS;
    public ComputeShader heightMapTopographyDisplayCS;
    public ComputeShader originalLandFieldCS;
    //public ComputeShader simulationDisplayCS;

    //public ComputeShader networkEvaluationCS;


    /////////////////////////////////////////////////// Kernels ////////////////////////////////////////////////////////////////////

    private int updateAgentKernel;
    private int displayAllKernel;

    private int agentPresenceKernel;
    private int agentImpactKernel;
    private int erosionKernel;

    private int transportsAffordanceKernel;
    private int agentTransportKernel;

    private int hotspotUpdateKernel;
    private int simpleDistribPatchKernel;
    private int visitFreqPatchKernel;

    private int originalHeightMapUpdateKernel;
    private int heightMapTopographicalUpdateKernel;
    private int heightMapColorScaledUpdateKernel;
    private int heightMapEdgeDetectionUpdateKernel;

    private int currentHeightMapUpdateKernel;

    private int originalLandFieldUpdateKernel;

    private int environmentUpdateKernel;

    private int initiateInfraCostEvalKernel;
    private int initiateAgentsCostEvalKernel;
    private int evaluateNetworkCostKernel;
    private int evaluateAgentsCostKernel;

    private int initConvTrailDiffKernel;
    private int convTrailDiffUpdateKernel;


    ////////////////////////////////////////////////// Other Settings ////////////////////////////////////////////////////////////////

    [Header("Simulation Settings")]
    public GeneralSettings generalSettings;
    //public TrailSettings trailSettings;
    public SlimeSettings slimeSettings;
    public TransportAffordanceSettings transportAffordanceSettings;
    public HotspotDistribSettings distribSettings;
    public HeightMapSettings heightMapSettings;
    public LandFieldSettings landFieldSettings;

    [Range(0, 100)] public int seed = 10;
    [Range(1, 80000)] public int numItPerGeneration;
    public int maxGeneration = 150;
    public bool runSimInBackground = false;
    public bool activateRendering = true;

    [Header("Environment Settings")]
    [Range(2, 50)] public int numHeightLayers = 2;
    [Range(2, 50)] public int customNumHotspots = 2;
    [Range(0, 5)] public int diffuseSize = 2;

    [Header("Display Settings")]
    public ShowMode showMode = ShowMode.ShowAll;
    public FilterMode filterMode = FilterMode.Point;
    public GraphicsFormat format = ComputeHelper.defaultGraphicsFormat;
    public HeightMapDisplayMode heightMapDisplayMode = HeightMapDisplayMode.Detailed;
    public HeightMapTopographyMode heightMapTopographyMode = HeightMapTopographyMode.Uniform;
    public HotspotDistributionMode hotspotDistributionMode = HotspotDistributionMode.Random;

    public Color lowestHeightColor;
    public Color highestHeightColor;
    public Color customTopographyColor;
    public bool heightmapEdgeDetectionLayer = false;
    bool bColorScaledTopographyMode;

    //public Color lowestAffColor;
    //public Color highestAffColor;

    ///////////////////////////////////////////////////// RenderTextures //////////////////////////////////////////////////////////

    [SerializeField, HideInInspector] private RenderTexture agentPresenceMap;
    [SerializeField, HideInInspector] private RenderTexture agentImpactMap;
    [SerializeField, HideInInspector] private RenderTexture erosionMap;
    [SerializeField, HideInInspector] private RenderTexture trailMap;

    [SerializeField, HideInInspector] private RenderTexture transportsAffordanceMap;
    [SerializeField, HideInInspector] private RenderTexture agentTransportMap;

    [SerializeField, HideInInspector] private RenderTexture hotspotDistribMap;
    [SerializeField, HideInInspector] private RenderTexture simpleDistribPatchMap;
    [SerializeField, HideInInspector] private RenderTexture visitFreqPatchMap;

    [SerializeField, HideInInspector] private RenderTexture originalHeightMap;
    [SerializeField, HideInInspector] private RenderTexture currentHeightMap;
    [SerializeField, HideInInspector] private RenderTexture heightMapColorScaledMap;
    [SerializeField, HideInInspector] private RenderTexture heightMapTopographicalMap;
    [SerializeField, HideInInspector] private RenderTexture heightMapEdgeDetectionMap;

    [SerializeField, HideInInspector] private RenderTexture originalLandFieldMap;
    [SerializeField, HideInInspector] private RenderTexture environmentDisplayAllMap;
    [SerializeField, HideInInspector] private RenderTexture displayAllMap;

    Texture2D affordanceNetwork;

    ///////////////////////////////////////////////////// Compute Buffers ///////////////////////////////////////////////////////

    ComputeBuffer agentsBuffer;
    ComputeBuffer slimeSettingsBuffer;
    ComputeBuffer generalSettingsBuffer;
    ComputeBuffer transportAffordanceSettingsBuffer;
    ComputeBuffer hotspotsBuffer;

    ComputeBuffer originalHeightMapOctavesBuffer;
    ComputeBuffer currentHeightMapBuffer;
    ComputeBuffer heightMapTopographyColorScaleBuffer;

    ComputeBuffer originalLandOctavesBuffer;
    ComputeBuffer landsSettingsBuffer;

    ComputeBuffer debugAgentBuffer;
    ComputeBuffer costAgentsEvalCountBuffer;
    ComputeBuffer costInfraEvalCountBuffer;
    ComputeBuffer convergenceTrailDiffBuffer;

    //////////////////////////////////////////////// Structure List /////////////////////////////////////////////////////////////////

    Color[] heightGradColor;
    //Color[] netAffGradColor;

    Land[] landsSettings;

    Hotspot[] hotspots;

    Agent[] agents;
    Agent[] debugStoredData;

    List<SitePair> sitePairs;

    float[] geodesicDistancesForPairs;

    Octave[] heightMapOctaves;
    Octave[] landMapOctaves;


    int[] costAgentsEvalData;
    int[] costInfraEvalData;

    int[] costAgentsEvalCountStoredData;
    int[] costInfraEvalCountStoredData;

    int[] convTrailPixCount;
    int[] convTrailPixCountStoredData;

    //TransportAffordanceSettings.TransportMode[] transportModesDebugData;

    //////////////////////////////////////////////// Evaluation Datas /////////////////////////////////////////////////////////////

    [Header("Evaluation Settings")]
    public DirectionalPathMode directionalSitePairsMode = DirectionalPathMode.Bidirectional;
    public bool multiScreenMode = false;

    int countTotalLength;
    float useEnergyPerGen;
    float infraEnergyPerGen;

    bool endSimulation = false;

    [Header("Realtime Data")]
    public int currentGenerationNum = 0;

    [HideInInspector]
    public int numIt = 0;


    /////////////////////////////////////////////////// Data Files ///////////////////////////////////////////////////////////////

    [Header("Data Files Settings")]
    [HideInInspector] public PathSelector pathSelector;
    public string networkGensFolder = "NetworkGenerations";
    string fileNamesDataFileName = "fileNamesDataFile";


    public DataFiles dataFiles;
    public string agentDistribDataFileName;
    public string networkDistribDataFileName;
    public string countTotCostTMDataFileName;


    //====================================================================================================================================================
    ////////////////////////////////////////////////////////////////// Functions //////////////////////////////////////////////////////////////////////
    //====================================================================================================================================================

    void CleanUpFolder(string path)
    {
        string simDataPath = pathSelector.absoluteDataPath + "/" + path;
        // Ensure the folder exists before attempting to clean it
        if (Directory.Exists(simDataPath))
        {
            // Delete all files in the folder
            string[] files = Directory.GetFiles(simDataPath);
            foreach (string file in files)
            {
                File.Delete(file);
            }

            // Delete all subdirectories in the folder
            string[] subDirectories = Directory.GetDirectories(simDataPath);
            foreach (string subDirectory in subDirectories)
            {
                Directory.Delete(subDirectory, true); // 'true' deletes the subdirectory and its contents
            }

            Debug.Log("Folder cleaned: " + simDataPath);
        }
        //else
        //{
        //    Debug.LogWarning("The specified folder does not exist: " + simDataPath);
        //}
    }

    void InitializeSimDataPath(string fileName)
    {
        StreamWriter fileNamesDataFile = new StreamWriter(pathSelector.absoluteDataPath + "/" + fileName + ".csv", false);
        fileNamesDataFile.WriteLine("Name of files :");
        fileNamesDataFile.WriteLine(agentDistribDataFileName);
        fileNamesDataFile.WriteLine(networkDistribDataFileName);
        fileNamesDataFile.WriteLine(countTotCostTMDataFileName);
        fileNamesDataFile.Close();

        if (!Directory.Exists(pathSelector.absoluteDataPath + "/" + networkGensFolder))
        {
            Directory.CreateDirectory(pathSelector.absoluteDataPath + "/" + networkGensFolder);
        }

        dataFiles = new DataFiles()
        {
            countAgentTMDataFile = new StreamWriter(pathSelector.absoluteDataPath + "/" + agentDistribDataFileName + ".csv", false),
            countInfraTMDataFile = new StreamWriter(pathSelector.absoluteDataPath + "/" + networkDistribDataFileName + ".csv", false),
            countTotCostTMDataFile = new StreamWriter(pathSelector.absoluteDataPath + "/" + countTotCostTMDataFileName + ".csv", false),
        };

        dataFiles.countAgentTMDataFile.WriteLine(
            "GenNum" + "," +
            "TM" + "," +
            "NumAgentPerTM"
        );

        dataFiles.countInfraTMDataFile.WriteLine(
            "GenNum" + "," +
            "TM" + "," +
            "NumInfraPerTM"
        );

        dataFiles.countTotCostTMDataFile.WriteLine(
            "GenNum" + "," +
            "CompleteTravelNum" + "," +
            "ConvergenceRate" + "," +
            "TotCostInfraPerTM" + "," +
            "TotCostUsePerTM" + "," +
            "TotCostPerTM"
        );

    }

    void InitGeneration()
    {
        // Initialize the random number generator with the seed
        Random.InitState(seed);

        // Initialize agents
        agents = new Agent[generalSettings.numAgents];

        for (int i = 0; i < generalSettings.numAgents; i++)
        {

            Vector2 centre = new Vector2(generalSettings.width / 2, generalSettings.height / 2);
            Vector2 startPos = Vector2.zero;
            int randomTargetIndex = -1;
            int randomOriginIndex = -1;

            float randomAngle = Random.value * 2 * Mathf.PI;
            float angle = 0;

            randomOriginIndex = Random.Range(0, hotspots.Length);
            randomTargetIndex = GenerateBiasedRandomIntExcluding(hotspots, hotspots.Length, Random.value, randomOriginIndex);


            if (generalSettings.spawnMode == SpawnMode.Point)
            {
                startPos = centre;
            }
            else if (generalSettings.spawnMode == SpawnMode.Random)
            {
                startPos = new Vector2(Random.Range(0, generalSettings.width), Random.Range(0, generalSettings.height));
            }
            else if (generalSettings.spawnMode == SpawnMode.InwardCircle)
            {
                startPos = centre + Random.insideUnitCircle * generalSettings.height * 0.5f;
            }
            else if (generalSettings.spawnMode == SpawnMode.RandomCircle)
            {
                startPos = centre + Random.insideUnitCircle * generalSettings.height * 0.2f;
            }
            else if (generalSettings.spawnMode == SpawnMode.RandomSite)
            {
                startPos = hotspots[randomOriginIndex].location;
            }

            Vector2 hotspotTargetLocation = hotspots[randomTargetIndex].location;

            angle = Vector2.Angle(hotspotTargetLocation - startPos, Vector2.zero) * Mathf.PI / 180.0f;

            Vector3Int speciesMask;
            int speciesIndex = 0;
            int numSpecies = slimeSettings.speciesSettings.Length;

            if (numSpecies == 1)
            {
                speciesMask = Vector3Int.one;
            }
            else
            {
                int species = Random.Range(1, numSpecies + 1);
                speciesIndex = species - 1;
                speciesMask = new Vector3Int((species == 1) ? 1 : 0, (species == 2) ? 1 : 0, (species == 3) ? 1 : 0);
            }


            agents[i] = new Agent(
                startPos,
                angle,
                speciesMask,
                0,
                speciesIndex,

                randomTargetIndex,
                randomOriginIndex,
                0,
                0,
                0
            );

        }

        ComputeHelper.CreateAndSetBuffer<Agent>(ref agentsBuffer, agents, computeShader, "agents", updateAgentKernel);

    }

    float PowLaw(Hotspot[] hotspots, int originIndex, int indexHotspotToEvaluate)
    {
        float hotspotDist = Vector2.Distance(hotspots[originIndex].location, hotspots[indexHotspotToEvaluate].location);
        return hotspots[indexHotspotToEvaluate].attractiveness / Mathf.Pow(hotspotDist * hotspots[indexHotspotToEvaluate].visitFreq, 2);
    }

    int GenerateBiasedRandomIntExcluding(Hotspot[] hotspots, int numHotspots, float randomFloat, int indexToExclude)
    {
        float totalWeight = 0.0f;
        float cumulWeightUntilExcludeIndexMinus1 = 0.0f;
        float cumulWeightUntilExcludeIndex = 0.0f;

        for (int k = 0; k < numHotspots; k++)
        {
            float selectionWeight = PowLaw(hotspots, indexToExclude, k);

            if (k <= indexToExclude - 1
                || (indexToExclude != 0 && k == 0)
                )
            {
                cumulWeightUntilExcludeIndexMinus1 += selectionWeight;
            }


            if (k <= indexToExclude)
            {
                cumulWeightUntilExcludeIndex += selectionWeight;
            }


            totalWeight += selectionWeight;

        }

        float randCumulWeight = randomFloat * totalWeight;


        //Debug.Log("Rand Cumul Weight : " + randCumulWeight);

        if ((randCumulWeight > cumulWeightUntilExcludeIndexMinus1 && randCumulWeight <= cumulWeightUntilExcludeIndex)
            )
        {
            //Debug.Log("Exclude Index Case ! ");
            if (indexToExclude == numHotspots - 1)
            {
                return 0;
            }

            return indexToExclude + 1;

        }
        else
        {
            //Debug.Log("Normal Case ! ");
            float cumulWeight = 0;
            for (int j = 0; j < numHotspots; j++)
            {
                cumulWeight += PowLaw(hotspots, indexToExclude, j);

                if (cumulWeight >= randCumulWeight)
                {
                    return j;
                }
            }
        }

        return -1;
    }

    void Init()
    {
        // Create render textures
        ComputeHelper.CreateRenderTexture(ref agentPresenceMap, generalSettings.width, generalSettings.height, filterMode, format);
        ComputeHelper.CreateRenderTexture(ref agentImpactMap, generalSettings.width, generalSettings.height, filterMode, format);
        ComputeHelper.CreateRenderTexture(ref erosionMap, generalSettings.width, generalSettings.height, filterMode, format);
        ComputeHelper.CreateRenderTexture(ref trailMap, generalSettings.width, generalSettings.height, filterMode, format);

        ComputeHelper.CreateRenderTexture(ref transportsAffordanceMap, generalSettings.width, generalSettings.height, filterMode, format);
        ComputeHelper.CreateRenderTexture(ref agentTransportMap, generalSettings.width, generalSettings.height, filterMode, format);

        ComputeHelper.CreateRenderTexture(ref displayAllMap, generalSettings.width, generalSettings.height, filterMode, format);

        ComputeHelper.CreateRenderTexture(ref hotspotDistribMap, generalSettings.width, generalSettings.height, filterMode, format);
        ComputeHelper.CreateRenderTexture(ref visitFreqPatchMap, generalSettings.width, generalSettings.height, filterMode, format);
        ComputeHelper.CreateRenderTexture(ref simpleDistribPatchMap, generalSettings.width, generalSettings.height, filterMode, format);

        ComputeHelper.CreateRenderTexture(ref originalHeightMap, generalSettings.width, generalSettings.height, filterMode, format);
        ComputeHelper.CreateRenderTexture(ref currentHeightMap, generalSettings.width, generalSettings.height, filterMode, format);

        ComputeHelper.CreateRenderTexture(ref heightMapColorScaledMap, generalSettings.width, generalSettings.height, filterMode, format);
        ComputeHelper.CreateRenderTexture(ref heightMapTopographicalMap, generalSettings.width, generalSettings.height, filterMode, format);
        ComputeHelper.CreateRenderTexture(ref heightMapEdgeDetectionMap, generalSettings.width, generalSettings.height, filterMode, format);

        ComputeHelper.CreateRenderTexture(ref originalLandFieldMap, generalSettings.width, generalSettings.height, filterMode, format);

        ComputeHelper.CreateRenderTexture(ref environmentDisplayAllMap, generalSettings.width, generalSettings.height, filterMode, format);


        // Assign kernels

        updateAgentKernel = computeShader.FindKernel("UpdateAgent");

        agentPresenceKernel = drawAgentsCS.FindKernel("DrawAgentPresence");
        agentImpactKernel = drawAgentsCS.FindKernel("DrawAgentImpact");
        erosionKernel = drawAgentsCS.FindKernel("DrawErosion");

        //trailKernel = drawAgentsCS.FindKernel("SetTrail");

        transportsAffordanceKernel = drawAgentsCS.FindKernel("DrawAffordance");
        agentTransportKernel = drawAgentsCS.FindKernel("DrawAgentTransport");

        displayAllKernel = drawAgentsCS.FindKernel("DisplayAll");

        initiateInfraCostEvalKernel = drawAgentsCS.FindKernel("InitiateInfraCostEvaluation");
        initiateAgentsCostEvalKernel = drawAgentsCS.FindKernel("InitiateAgentsCostEvaluation");
        evaluateNetworkCostKernel = drawAgentsCS.FindKernel("EvalInfraCost");
        evaluateAgentsCostKernel = drawAgentsCS.FindKernel("EvalAgentsCost");

        hotspotUpdateKernel = hotspotDistribCS.FindKernel("HotspotDistribUpdate");
        simpleDistribPatchKernel = hotspotDistribCS.FindKernel("SimpleDistribPatch");
        visitFreqPatchKernel = hotspotDistribCS.FindKernel("VisitFreqPatch");

        originalHeightMapUpdateKernel = heightMapCS.FindKernel("OriginalHeightMapUpdate");
        currentHeightMapUpdateKernel = heightMapCS.FindKernel("CurrentHeightMapUpdate");

        heightMapTopographicalUpdateKernel = heightMapTopographyDisplayCS.FindKernel("HeightMapTopographyUpdate");
        heightMapColorScaledUpdateKernel = heightMapTopographyDisplayCS.FindKernel("HeightMapColorScaledUpdate");
        heightMapEdgeDetectionUpdateKernel = heightMapTopographyDisplayCS.FindKernel("HeightMapEdgeDetectionUpdate");

        originalLandFieldUpdateKernel = originalLandFieldCS.FindKernel("OriginalLandFieldUpdate");

        //environmentUpdateKernel = simulationDisplayCS.FindKernel("EnvironmentDisplayUpdate");

        initConvTrailDiffKernel = drawAgentsCS.FindKernel("InitConvergenceTrailDiff");
        convTrailDiffUpdateKernel = drawAgentsCS.FindKernel("ConvergenceTrailDiff");

        // Assign textures
        computeShader.SetTexture(updateAgentKernel, "AgentImpactMap", agentImpactMap);
        computeShader.SetTexture(updateAgentKernel, "TrailMap", trailMap);
        computeShader.SetTexture(updateAgentKernel, "SimpleDistribPatchMap", simpleDistribPatchMap);
        computeShader.SetTexture(updateAgentKernel, "OriginalHeightMap", originalHeightMap);
        computeShader.SetTexture(updateAgentKernel, "OriginalLandFieldMap", originalLandFieldMap);


        drawAgentsCS.SetTexture(agentPresenceKernel, "AgentPresenceMap", agentPresenceMap);
        drawAgentsCS.SetTexture(agentImpactKernel, "AgentImpactMap", agentImpactMap);

        //drawAgentsCS.SetTexture(erosionKernel, "AgentImpactMap", agentImpactMap);
        drawAgentsCS.SetTexture(erosionKernel, "ErosionMap", erosionMap);

        //drawAgentsCS.SetTexture(trailKernel, "ErosionMap", erosionMap);
        //drawAgentsCS.SetTexture(trailKernel, "TrailMap", trailMap);

        //drawAgentsCS.SetTexture(diffuseImpactKernel, "AgentImpactMap", agentImpactMap);
        //drawAgentsCS.SetTexture(diffuseImpactKernel, "TrailMap", trailMap);

        //drawAgentsCS.SetTexture(finalNetworkKernel, "TrailMap", trailMap);
        //drawAgentsCS.SetTexture(finalNetworkKernel, "FinalNetworkMap", finalNetworkMap);

        //drawAgentsCS.SetTexture(transportsAffordanceKernel, "TrailMap", trailMap);
        //drawAgentsCS.SetTexture(transportsAffordanceKernel, "TransportsAffordanceMap", transportsAffordanceMap);

        //drawAgentsCS.SetTexture(agentTransportKernel, "TransportsAffordanceMap", transportsAffordanceMap);
        //drawAgentsCS.SetTexture(agentTransportKernel, "AgentTransportMap", agentTransportMap);


        //drawAgentsCS.SetTexture(displayAllKernel, "displayAllMap", displayAllMap);
        drawAgentsCS.SetTexture(displayAllKernel, "TrailMap", trailMap);
        drawAgentsCS.SetTexture(displayAllKernel, "SimpleDistribPatchLayerMap", simpleDistribPatchMap);
        drawAgentsCS.SetTexture(displayAllKernel, "EnvironmentLayerMap", environmentDisplayAllMap);
        drawAgentsCS.SetTexture(displayAllKernel, "DisplayAllMap", displayAllMap);

        hotspotDistribCS.SetTexture(hotspotUpdateKernel, "HotspotDistribMap", hotspotDistribMap);
        hotspotDistribCS.SetTexture(simpleDistribPatchKernel, "SimplePatchMap", simpleDistribPatchMap);
        hotspotDistribCS.SetTexture(visitFreqPatchKernel, "VisitFreqPatchMap", visitFreqPatchMap);


        heightMapCS.SetTexture(originalHeightMapUpdateKernel, "OriginalHeightMap", originalHeightMap);

        heightMapCS.SetTexture(currentHeightMapUpdateKernel, "OriginalHeightMap", originalHeightMap);
        heightMapCS.SetTexture(currentHeightMapUpdateKernel, "TrailMap", trailMap);
        heightMapCS.SetTexture(currentHeightMapUpdateKernel, "CurrentHeightMap", currentHeightMap);

        heightMapTopographyDisplayCS.SetTexture(heightMapTopographicalUpdateKernel, "OriginalHeightMap", originalHeightMap);
        heightMapTopographyDisplayCS.SetTexture(heightMapTopographicalUpdateKernel, "TopographyMap", heightMapTopographicalMap);

        heightMapTopographyDisplayCS.SetTexture(heightMapColorScaledUpdateKernel, "OriginalHeightMap", originalHeightMap);
        heightMapTopographyDisplayCS.SetTexture(heightMapColorScaledUpdateKernel, "TopographyMap", heightMapTopographicalMap);
        heightMapTopographyDisplayCS.SetTexture(heightMapColorScaledUpdateKernel, "ColorScaledMap", heightMapColorScaledMap);

        heightMapTopographyDisplayCS.SetTexture(heightMapEdgeDetectionUpdateKernel, "TopographyMap", heightMapTopographicalMap);
        heightMapTopographyDisplayCS.SetTexture(heightMapEdgeDetectionUpdateKernel, "EdgeDetectionMap", heightMapEdgeDetectionMap);


        originalLandFieldCS.SetTexture(originalLandFieldUpdateKernel, "OriginalLandFieldMap", originalLandFieldMap);

        //simulationDisplayCS.SetTexture(environmentUpdateKernel, "LandFieldLayerMap", originalLandFieldMap);
        //simulationDisplayCS.SetTexture(environmentUpdateKernel, "HeightMapEdgeDetectionLayerMap", heightMapEdgeDetectionMap);
        //simulationDisplayCS.SetTexture(environmentUpdateKernel, "EnvironmentdisplayAllMap", environmentDisplayAllMap);

        // Initialize the random number generator with the seed
        Random.InitState(seed);

        // Initialize octaves for random noise generation
        heightMapOctaves = new Octave[heightMapSettings.octaves.Length];
        for (int i = 0; i < heightMapOctaves.Length; i++)
        {
            heightMapOctaves[i] = new Octave(

                heightMapSettings.octaves[i].frequency,
                heightMapSettings.octaves[i].amplitude
            );

        }

        landMapOctaves = new Octave[landFieldSettings.octaves.Length];
        for (int i = 0; i < landMapOctaves.Length; i++)
        {
            landMapOctaves[i] = new Octave(

                landFieldSettings.octaves[i].frequency,
                landFieldSettings.octaves[i].amplitude
            );
        }


        // Initialize lands settings
        landsSettings = new Land[landFieldSettings.landsSettings.Length];

        for (int i = 0; i < landsSettings.Length; i++)
        {

            landsSettings[i] = new Land(
                landFieldSettings.landsSettings[i].landResistanceIndex,
                landFieldSettings.landsSettings[i].appearanceThreshold,
                landFieldSettings.landsSettings[i].landXTh,
                landFieldSettings.landsSettings[i].landColor
            );
        }

        // Initialize hotspots
        if (hotspotDistributionMode == HotspotDistributionMode.Custom)
        {
            hotspots = new Hotspot[distribSettings.hotspotsSettings.Length];

            for (int i = 0; i < hotspots.Length; i++)
            {
                hotspots[i] = new Hotspot(

                    distribSettings.hotspotsSettings[i].location * new Vector2(generalSettings.width, generalSettings.height),
                    distribSettings.hotspotsSettings[i].visitFreq,
                    distribSettings.hotspotsSettings[i].attractiveness
                );

            }
        }
        else if (hotspotDistributionMode == HotspotDistributionMode.Random)
        {
            hotspots = new Hotspot[customNumHotspots];

            for (int i = 0; i < hotspots.Length; i++)
            {
                hotspots[i] = new Hotspot(

                    new Vector2(Random.Range(0.05f * generalSettings.width, 0.95f * generalSettings.width), Random.Range(0.05f * generalSettings.height, 0.95f * generalSettings.height)),
                    1,
                    1
                );

            }
        }


        sitePairs = new List<SitePair>();

        if (directionalSitePairsMode == DirectionalPathMode.Unidirectional)
        {
            NetworkEvaluationUtility.AddUnidirectedSitePairs(ref sitePairs, distribSettings);
        }
        else if (directionalSitePairsMode == DirectionalPathMode.Bidirectional)
        {
            NetworkEvaluationUtility.AddBidirectedSitePairs(ref sitePairs, distribSettings);
        }

        // Calculate distance on grid for all pairs
        geodesicDistancesForPairs = new float[sitePairs.Count];

        for (int i = 0; i < geodesicDistancesForPairs.Length; i++)
        {
            Vector2 site1PixelLocation = MathUtility.ConvertRelativeToAbsoluteLocationOnGrid(hotspots[sitePairs[i].site1].location, generalSettings.width, generalSettings.height);
            Vector2 site2PixelLocation = MathUtility.ConvertRelativeToAbsoluteLocationOnGrid(hotspots[sitePairs[i].site2].location, generalSettings.width, generalSettings.height);

            int x = (int)Mathf.Abs(site1PixelLocation.x - site2PixelLocation.x);
            int y = (int)Mathf.Abs(site1PixelLocation.y - site2PixelLocation.y);

            int minXY = Mathf.Min(x, y);
            int maxXY = Mathf.Max(x, y);

            geodesicDistancesForPairs[i] = minXY * Mathf.Sqrt(2) + maxXY - minXY;

            //geodesicDistancesForPairs[i] = Mathf.Sqrt(Mathf.Pow(x, 2) + Mathf.Pow(y, 2));
        }

        InitGeneration();

        currentGenerationNum = 0;

        debugStoredData = new Agent[generalSettings.numAgents];

        convTrailPixCount = new int[1];
        convTrailPixCountStoredData = new int[1];

        costAgentsEvalData = new int[transportAffordanceSettings.transportModes.Length];
        costInfraEvalData = new int[transportAffordanceSettings.transportModes.Length];

        costAgentsEvalCountStoredData = new int[transportAffordanceSettings.transportModes.Length];
        costInfraEvalCountStoredData = new int[transportAffordanceSettings.transportModes.Length];

        drawAgentsCS.SetBuffer(agentPresenceKernel, "agents", agentsBuffer);
        drawAgentsCS.SetBuffer(agentImpactKernel, "agents", agentsBuffer);
        //drawAgentsCS.SetBuffer(erosionKernel, "agents", agentsBuffer);
        drawAgentsCS.SetBuffer(agentTransportKernel, "agents", agentsBuffer);
        drawAgentsCS.SetBuffer(evaluateAgentsCostKernel, "agents", agentsBuffer);

        ComputeHelper.CreateAndSetBuffer<Hotspot>(ref hotspotsBuffer, hotspots, hotspotDistribCS, "hotspots", hotspotUpdateKernel);
        hotspotDistribCS.SetBuffer(simpleDistribPatchKernel, "hotspots", hotspotsBuffer);
        hotspotDistribCS.SetBuffer(visitFreqPatchKernel, "hotspots", hotspotsBuffer);

        computeShader.SetBuffer(updateAgentKernel, "hotspots", hotspotsBuffer);

        ComputeHelper.CreateAndSetBuffer<Land>(ref landsSettingsBuffer, landsSettings, originalLandFieldCS, "landsSettings", originalLandFieldUpdateKernel);

        computeShader.SetBuffer(updateAgentKernel, "landsSettings", landsSettingsBuffer);


        ComputeHelper.CreateAndSetBuffer<Octave>(ref originalLandOctavesBuffer, landMapOctaves, originalLandFieldCS, "octaves", originalLandFieldUpdateKernel);

        ComputeHelper.CreateAndSetBuffer<Octave>(ref originalHeightMapOctavesBuffer, heightMapOctaves, heightMapCS, "octaves", originalHeightMapUpdateKernel);

        // Compute Shader
        computeShader.SetInt("numAgents", generalSettings.numAgents);
        computeShader.SetInt("numHotspots", hotspots.Length);
        computeShader.SetInt("numLandsSettings", landsSettings.Length);
        computeShader.SetFloat("deltaTime", Time.fixedDeltaTime);
        computeShader.SetFloat("time", Time.fixedTime);
        computeShader.SetInt("seed", seed);
        computeShader.SetInt("width", generalSettings.width);
        computeShader.SetInt("height", generalSettings.height);

        // Draw Agents Compute Shader

        //drawAgentsCS.SetBuffer(agentImpactKernel, "agents", agentsBuffer);

        drawAgentsCS.SetInt("numAgents", generalSettings.numAgents);
        drawAgentsCS.SetInt("width", generalSettings.width);
        drawAgentsCS.SetInt("height", generalSettings.height);
        drawAgentsCS.SetInt("numTM", transportAffordanceSettings.transportModes.Length);
        drawAgentsCS.SetInt("diffuseSize", diffuseSize);
        drawAgentsCS.SetVector("displayColor", distribSettings.displayColor);

        // Hotspots Configuration Compute Shader

        hotspotDistribCS.SetInt("width", generalSettings.width);
        hotspotDistribCS.SetInt("height", generalSettings.height);
        hotspotDistribCS.SetFloat("scaleFactor", distribSettings.scaleFactor);
        hotspotDistribCS.SetInt("numHotspots", hotspots.Length);

        // Height Map Configuration

        heightMapCS.SetInt("octaveCount", heightMapSettings.octaves.Length);
        heightMapCS.SetInt("height", generalSettings.height);
        heightMapCS.SetInt("width", generalSettings.width);
        heightMapCS.SetInt("seed", seed);
        heightMapCS.SetFloat("xOffset", heightMapSettings.XOffset);
        heightMapCS.SetFloat("yOffset", heightMapSettings.YOffset);

        ColorHSV lowestHeightColorHSV = ColorExtension.ToHSV(lowestHeightColor);
        ColorHSV highestHeightColorHSV = ColorExtension.ToHSV(highestHeightColor);

        heightGradColor = new Color[numHeightLayers];

        for (int i = 0; i < numHeightLayers; i++)
        {
            heightGradColor[i] = ColorUtility.LerpHSV(lowestHeightColorHSV, highestHeightColorHSV, (float)i / (float)numHeightLayers).ToRGB();
        }

        //ColorHSV lowestAffColorHSV = ColorExtension.ToHSV(lowestHeightColor);
        //ColorHSV highestAffColorHSV = ColorExtension.ToHSV(highestHeightColor);

        //netAffGradColor = new Color[transportAffordanceSettings.transportModes.Length];

        //for (int i = 0; i < transportAffordanceSettings.transportModes.Length; i++)
        //{
        //    //netAffGradColor[i] = ColorUtility.LerpHSV(lowestAffColorHSV, highestAffColorHSV, (float)i / (float)numNetworkAffordance).ToRGB();
        //    netAffGradColor[i] = transportAffordanceSettings.transportModes[i].affColor;
        //}

        if (heightMapTopographyMode == HeightMapTopographyMode.ColorScale)
        {
            bColorScaledTopographyMode = true;
        }
        else
        {
            bColorScaledTopographyMode = false;
        }

        heightMapTopographyDisplayCS.SetBool("bColorScaledTopographyMode", bColorScaledTopographyMode);
        heightMapTopographyDisplayCS.SetInt("height", generalSettings.height);
        heightMapTopographyDisplayCS.SetInt("width", generalSettings.width);
        heightMapTopographyDisplayCS.SetInt("numHeightLayers", numHeightLayers);
        heightMapTopographyDisplayCS.SetVector("customTopographyColor", customTopographyColor);

        // Land Field Map Configuration

        originalLandFieldCS.SetInt("height", generalSettings.height);
        originalLandFieldCS.SetInt("width", generalSettings.width);
        originalLandFieldCS.SetInt("seed", seed);
        originalLandFieldCS.SetFloat("xOffset", landFieldSettings.XOffset);
        originalLandFieldCS.SetFloat("yOffset", landFieldSettings.YOffset);
        originalLandFieldCS.SetInt("landCount", landsSettings.Length);
        originalLandFieldCS.SetInt("octaveCount", landFieldSettings.octaves.Length);

        //simulationDisplayCS.SetInt("height", generalSettings.height);
        //simulationDisplayCS.SetInt("width", generalSettings.width);
        //simulationDisplayCS.SetBool("bDisplayHeightmapEdgeDetectionLayer", displayHeightmapEdgeDetectionLayer);


    }

    void UpdateAgents(Agent[] agentsData)
    {
        // Initialize the random number generator with the seed
        Random.InitState(seed);

        // Initialize agents
        agents = new Agent[generalSettings.numAgents];

        for (int i = 0; i < generalSettings.numAgents; i++)
        {

            Vector2 centre = new Vector2(generalSettings.width / 2, generalSettings.height / 2);
            Vector2 startPos = Vector2.zero;
            int randomTargetIndex = -1;
            int randomOriginIndex = -1;

            float randomAngle = Random.value * 2 * Mathf.PI;
            float angle = 0;

            randomOriginIndex = Random.Range(0, hotspots.Length);
            randomTargetIndex = GenerateBiasedRandomIntExcluding(hotspots, hotspots.Length, Random.value, randomOriginIndex);


            if (generalSettings.spawnMode == SpawnMode.Point)
            {
                startPos = centre;
            }
            else if (generalSettings.spawnMode == SpawnMode.Random)
            {
                startPos = new Vector2(Random.Range(0, generalSettings.width), Random.Range(0, generalSettings.height));
            }
            else if (generalSettings.spawnMode == SpawnMode.InwardCircle)
            {
                startPos = centre + Random.insideUnitCircle * generalSettings.height * 0.5f;
            }
            else if (generalSettings.spawnMode == SpawnMode.RandomCircle)
            {
                startPos = centre + Random.insideUnitCircle * generalSettings.height * 0.2f;
            }
            else if (generalSettings.spawnMode == SpawnMode.RandomSite)
            {
                startPos = hotspots[randomOriginIndex].location;
            }

            Vector2 hotspotTargetLocation = hotspots[randomTargetIndex].location;

            angle = Vector2.Angle(hotspotTargetLocation - startPos, Vector2.zero) * Mathf.PI / 180.0f;

            Vector3Int speciesMask;
            int speciesIndex = 0;
            int numSpecies = slimeSettings.speciesSettings.Length;

            if (numSpecies == 1)
            {
                speciesMask = Vector3Int.one;
            }
            else
            {
                int species = Random.Range(1, numSpecies + 1);
                speciesIndex = species - 1;
                speciesMask = new Vector3Int((species == 1) ? 1 : 0, (species == 2) ? 1 : 0, (species == 3) ? 1 : 0);
            }


            agents[i] = new Agent(
                startPos,
                angle,
                speciesMask,
                0,
                speciesIndex,

                randomTargetIndex,
                randomOriginIndex,
                0,
                0,
                agentsData[i].completeTravels
            );

        }

        ComputeHelper.CreateAndSetBuffer<Agent>(ref agentsBuffer, agents, computeShader, "agents", updateAgentKernel);

    }

    void RunSimulation()
    {
        // Assign settings

        var speciesSettings = slimeSettings.speciesSettings;

        ComputeHelper.CreateStructuredBuffer(ref slimeSettingsBuffer, speciesSettings);

        ComputeHelper.CreateStructuredBuffer(ref landsSettingsBuffer, landsSettings);

        //ComputeHelper.CreateAndSetBuffer<int>(ref completeTravelsBuffer, completeTravels, computeShader, "completeTravelsPerGen", updateAgentKernel);
        //var completeTravelsVar = completeTravels;

        //ComputeHelper.CreateStructuredBuffer(ref completeTravelsBuffer, completeTravelsVar);

        var transportsSettings = transportAffordanceSettings.transportModes;

        ComputeHelper.CreateStructuredBuffer(ref transportAffordanceSettingsBuffer, transportsSettings);

        computeShader.SetFloat("deltaTime", Time.fixedDeltaTime);
        computeShader.SetFloat("time", Time.fixedTime);
        //computeShader.SetFloat("proportionAgentSpeedWhenLost", slimeSettings.proportionAgentSpeedWhenLost);

        computeShader.SetInt("numLandsSettings", landsSettings.Length);

        computeShader.SetInt("width", generalSettings.width);
        computeShader.SetInt("height", generalSettings.height);
        computeShader.SetInt("numAgents", generalSettings.numAgents);
        computeShader.SetInt("seed", seed);
        computeShader.SetInt("numHotspots", hotspots.Length);

        computeShader.SetBuffer(updateAgentKernel, "agents", agentsBuffer);
        computeShader.SetBuffer(updateAgentKernel, "speciesSettings", slimeSettingsBuffer);
        computeShader.SetBuffer(updateAgentKernel, "landsSettings", landsSettingsBuffer);
        //computeShader.SetBuffer(updateAgentKernel, "completeTravelsPerGen", completeTravelsBuffer);
        computeShader.SetBuffer(updateAgentKernel, "transportModes", transportAffordanceSettingsBuffer);

        //computeShader.SetTexture(updateAgentKernel, "TrailMap", trailMap);
        computeShader.SetTexture(updateAgentKernel, "AgentImpactMap", agentImpactMap);
        computeShader.SetTexture(updateAgentKernel, "OriginalHeightMap", originalHeightMap);
        computeShader.SetTexture(updateAgentKernel, "OriginalLandFieldMap", originalLandFieldMap);

        // Draw Agents Configuration

        drawAgentsCS.SetBuffer(agentPresenceKernel, "agents", agentsBuffer);
        drawAgentsCS.SetBuffer(agentPresenceKernel, "speciesSettings", slimeSettingsBuffer);

        drawAgentsCS.SetBuffer(agentImpactKernel, "agents", agentsBuffer);
        drawAgentsCS.SetBuffer(agentImpactKernel, "speciesSettings", slimeSettingsBuffer);

        drawAgentsCS.SetBuffer(erosionKernel, "agents", agentsBuffer);
        //drawAgentsCS.SetBuffer(finalNetworkKernel, "transportModes", transportAffordanceSettingsBuffer);



        drawAgentsCS.SetBuffer(agentTransportKernel, "agents", agentsBuffer);
        drawAgentsCS.SetBuffer(agentTransportKernel, "speciesSettings", slimeSettingsBuffer);

        // Affordance Configuration

        //ColorHSV lowestAffColorHSV = ColorExtension.ToHSV(lowestHeightColor);
        //ColorHSV highestAffColorHSV = ColorExtension.ToHSV(highestHeightColor);

        //netAffGradColor = new Color[transportAffordanceSettings.transportModes.Length];

        //for (int i = 0; i < transportAffordanceSettings.transportModes.Length; i++)
        //{
        //    //netAffGradColor[i] = ColorUtility.LerpHSV(lowestAffColorHSV, highestAffColorHSV, (float)i / (float)numNetworkAffordance).ToRGB();
        //    netAffGradColor[i] = transportAffordanceSettings.transportModes[i].affColor;
        //}

        //var evalSettings = netAffGradColor;

        drawAgentsCS.SetBuffer(evaluateNetworkCostKernel, "transportModes", transportAffordanceSettingsBuffer);

        drawAgentsCS.SetBuffer(displayAllKernel, "speciesSettings", slimeSettingsBuffer);

        drawAgentsCS.SetInt("numSpecies", speciesSettings.Length);
        drawAgentsCS.SetInt("numAffGrad", transportAffordanceSettings.transportModes.Length);
        drawAgentsCS.SetInt("numAgents", generalSettings.numAgents);
        drawAgentsCS.SetInt("width", generalSettings.width);
        drawAgentsCS.SetInt("height", generalSettings.height);
        drawAgentsCS.SetInt("diffuseSize", diffuseSize);
        //drawAgentsCS.SetInt("numItPerGen", numItPerGeneration);
        //drawAgentsCS.SetInt("numIt", numIt);

        drawAgentsCS.SetVector("displayColor", distribSettings.displayColor);

        drawAgentsCS.SetTexture(agentPresenceKernel, "AgentPresenceMap", agentPresenceMap);
        drawAgentsCS.SetTexture(agentImpactKernel, "AgentImpactMap", agentImpactMap);

        //drawAgentsCS.SetTexture(erosionKernel, "AgentImpactMap", agentImpactMap);
        drawAgentsCS.SetTexture(erosionKernel, "ErosionMap", erosionMap);

        //drawAgentsCS.SetTexture(trailKernel, "ErosionMap", erosionMap);
        //drawAgentsCS.SetTexture(trailKernel, "TrailMap", trailMap);

        //drawAgentsCS.SetTexture(diffuseImpactKernel, "AgentImpactMap", agentImpactMap);
        //drawAgentsCS.SetTexture(diffuseImpactKernel, "TrailMap", trailMap);

        //drawAgentsCS.SetTexture(finalNetworkKernel, "TrailMap", trailMap);
        //drawAgentsCS.SetTexture(finalNetworkKernel, "FinalNetworkMap", finalNetworkMap);

        //drawAgentsCS.SetTexture(transportsAffordanceKernel, "TrailMap", trailMap);
        //drawAgentsCS.SetTexture(transportsAffordanceKernel, "TransportsAffordanceMap", transportsAffordanceMap);

        drawAgentsCS.SetBuffer(transportsAffordanceKernel, "transportModes", transportAffordanceSettingsBuffer);

        drawAgentsCS.SetTexture(agentTransportKernel, "TransportAffordanceMap", transportsAffordanceMap);
        drawAgentsCS.SetTexture(agentTransportKernel, "AgentTransportMap", agentTransportMap);

        drawAgentsCS.SetTexture(displayAllKernel, "SimpleDistribPatchLayerMap", simpleDistribPatchMap);
        drawAgentsCS.SetTexture(displayAllKernel, "TrailMap", trailMap);
        drawAgentsCS.SetTexture(displayAllKernel, "EnvironmentLayerMap", environmentDisplayAllMap);
        drawAgentsCS.SetTexture(displayAllKernel, "DisplayAllMap", displayAllMap);

        // Hotspots Configuration

        if (hotspotDistributionMode == HotspotDistributionMode.Custom)
        {
            hotspots = new Hotspot[distribSettings.hotspotsSettings.Length];

            for (int i = 0; i < hotspots.Length; i++)
            {
                hotspots[i] = new Hotspot(

                    distribSettings.hotspotsSettings[i].location * new Vector2(generalSettings.width, generalSettings.height),
                    distribSettings.hotspotsSettings[i].visitFreq,
                    distribSettings.hotspotsSettings[i].attractiveness
                );

            }
        }


        ComputeHelper.CreateAndSetBuffer<Hotspot>(ref hotspotsBuffer, hotspots, hotspotDistribCS, "hotspots", hotspotUpdateKernel);

        //var hotspotsSet = distribSettings.hotspotsSettings;

        //ComputeHelper.CreateStructuredBuffer(ref hotspotsBuffer, hotspotsSet);
        //hotspotDistribCS.SetBuffer(hotspotUpdateKernel, "hotspots", hotspotsBuffer);
        hotspotDistribCS.SetBuffer(simpleDistribPatchKernel, "hotspots", hotspotsBuffer);
        hotspotDistribCS.SetBuffer(visitFreqPatchKernel, "hotspots", hotspotsBuffer);


        hotspotDistribCS.SetInt("width", generalSettings.width);
        hotspotDistribCS.SetInt("height", generalSettings.height);
        hotspotDistribCS.SetFloat("scaleFactor", distribSettings.scaleFactor);
        hotspotDistribCS.SetInt("numHotspots", hotspots.Length);

        hotspotDistribCS.SetTexture(hotspotUpdateKernel, "HotspotDistribMap", hotspotDistribMap);
        hotspotDistribCS.SetTexture(simpleDistribPatchKernel, "SimplePatchMap", simpleDistribPatchMap);
        hotspotDistribCS.SetTexture(visitFreqPatchKernel, "VisitFreqPatchMap", visitFreqPatchMap);

        // Height Map Configuration

        for (int i = 0; i < heightMapOctaves.Length; i++)
        {
            heightMapOctaves[i] = new Octave(

                heightMapSettings.octaves[i].frequency,
                heightMapSettings.octaves[i].amplitude
            );

        }

        ComputeHelper.CreateAndSetBuffer<Octave>(ref originalHeightMapOctavesBuffer, heightMapOctaves, heightMapCS, "octaves", originalHeightMapUpdateKernel);

        //heightMapCS.SetBuffer(originalHeightMapUpdateKernel, "octaves", originalHeightMapOctavesBuffer);
        heightMapCS.SetInt("width", generalSettings.width);
        heightMapCS.SetInt("height", generalSettings.height);
        heightMapCS.SetInt("seed", seed);
        heightMapCS.SetFloat("xOffset", heightMapSettings.XOffset);
        heightMapCS.SetFloat("yOffset", heightMapSettings.YOffset);
        heightMapCS.SetInt("octaveCount", heightMapSettings.octaves.Length);
        heightMapCS.SetTexture(originalHeightMapUpdateKernel, "OriginalHeightMap", originalHeightMap);

        heightMapCS.SetTexture(currentHeightMapUpdateKernel, "OriginalHeightMap", originalHeightMap);
        heightMapCS.SetTexture(currentHeightMapUpdateKernel, "TrailMap", trailMap);
        heightMapCS.SetTexture(currentHeightMapUpdateKernel, "CurrentHeightMap", currentHeightMap);

        if (heightMapTopographyMode == HeightMapTopographyMode.ColorScale)
        {
            bColorScaledTopographyMode = true;
        }
        else
        {
            bColorScaledTopographyMode = false;
        }

        heightMapTopographyDisplayCS.SetBool("bColorScaledTopographyMode", bColorScaledTopographyMode);
        heightMapTopographyDisplayCS.SetVector("customTopographyColor", customTopographyColor);
        heightMapTopographyDisplayCS.SetInt("height", generalSettings.height);
        heightMapTopographyDisplayCS.SetInt("width", generalSettings.width);
        heightMapTopographyDisplayCS.SetInt("numHeightLayers", numHeightLayers);


        ColorHSV lowestHeightColorHSV = ColorExtension.ToHSV(lowestHeightColor);
        ColorHSV highestHeightColorHSV = ColorExtension.ToHSV(highestHeightColor);

        heightGradColor = new Color[numHeightLayers];

        for (int i = 0; i < numHeightLayers; i++)
        {
            heightGradColor[i] = ColorUtility.LerpHSV(lowestHeightColorHSV, highestHeightColorHSV, (float)i / (float)numHeightLayers).ToRGB();
        }



        var affordanceHeightColorScale = heightGradColor;

        ComputeHelper.CreateStructuredBuffer(ref heightMapTopographyColorScaleBuffer, affordanceHeightColorScale);
        heightMapTopographyDisplayCS.SetBuffer(heightMapTopographicalUpdateKernel, "affordanceScale", heightMapTopographyColorScaleBuffer);
        heightMapTopographyDisplayCS.SetBuffer(heightMapColorScaledUpdateKernel, "affordanceScale", heightMapTopographyColorScaleBuffer);
        heightMapTopographyDisplayCS.SetBuffer(heightMapEdgeDetectionUpdateKernel, "affordanceScale", heightMapTopographyColorScaleBuffer);

        heightMapTopographyDisplayCS.SetTexture(heightMapTopographicalUpdateKernel, "OriginalHeightMap", originalHeightMap);
        heightMapTopographyDisplayCS.SetTexture(heightMapTopographicalUpdateKernel, "TopographyMap", heightMapTopographicalMap);

        heightMapTopographyDisplayCS.SetTexture(heightMapColorScaledUpdateKernel, "OriginalHeightMap", originalHeightMap);
        heightMapTopographyDisplayCS.SetTexture(heightMapColorScaledUpdateKernel, "TopographyMap", heightMapTopographicalMap);
        heightMapTopographyDisplayCS.SetTexture(heightMapColorScaledUpdateKernel, "ColorScaledMap", heightMapColorScaledMap);

        heightMapTopographyDisplayCS.SetTexture(heightMapEdgeDetectionUpdateKernel, "OriginalHeightMap", originalHeightMap);
        heightMapTopographyDisplayCS.SetTexture(heightMapEdgeDetectionUpdateKernel, "TopographyMap", heightMapTopographicalMap);
        heightMapTopographyDisplayCS.SetTexture(heightMapEdgeDetectionUpdateKernel, "EdgeDetectionMap", heightMapEdgeDetectionMap);


        // Land Field Configuration

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

        ComputeHelper.CreateStructuredBuffer(ref originalLandOctavesBuffer, landFieldSettings.octaves);

        originalLandFieldCS.SetBuffer(originalLandFieldUpdateKernel, "landsSettings", landsSettingsBuffer);
        originalLandFieldCS.SetBuffer(originalLandFieldUpdateKernel, "octaves", originalLandOctavesBuffer);

        originalLandFieldCS.SetFloat("xOffset", landFieldSettings.XOffset);
        originalLandFieldCS.SetFloat("yOffset", landFieldSettings.YOffset);
        originalLandFieldCS.SetInt("octaveCount", landFieldSettings.octaves.Length);
        originalLandFieldCS.SetInt("landCount", landFieldSettings.landsSettings.Length);
        originalLandFieldCS.SetInt("width", generalSettings.width);
        originalLandFieldCS.SetInt("height", generalSettings.height);
        originalLandFieldCS.SetInt("seed", seed);
        originalLandFieldCS.SetTexture(originalLandFieldUpdateKernel, "OriginalLandFieldMap", originalLandFieldMap);

        //simulationDisplayCS.SetInt("height", generalSettings.height);
        //simulationDisplayCS.SetInt("width", generalSettings.width);
        //simulationDisplayCS.SetBool("bDisplayHeightmapEdgeDetectionLayer", displayHeightmapEdgeDetectionLayer);
        //simulationDisplayCS.SetTexture(environmentUpdateKernel, "LandFieldLayerMap", originalLandFieldMap);
        //simulationDisplayCS.SetTexture(environmentUpdateKernel, "HeightMapEdgeDetectionLayerMap", heightMapEdgeDetectionMap);
        //simulationDisplayCS.SetTexture(environmentUpdateKernel, "EnvironmentDisplayAllMap", environmentDisplayAllMap);

        ComputeHelper.Dispatch(computeShader, generalSettings.numAgents, 1, 1, kernelIndex: updateAgentKernel);


        //ComputeHelper.Dispatch(drawAgentsCS, generalSettings.width, generalSettings.height, 1, kernelIndex: diffuseImpactKernel);

        //ComputeHelper.CopyRenderTexture(trailMap, agentImpactMap);

        ComputeHelper.Dispatch(hotspotDistribCS, hotspots.Length, 1, 1, kernelIndex: hotspotUpdateKernel);
        ComputeHelper.Dispatch(hotspotDistribCS, generalSettings.width, generalSettings.height, 1, kernelIndex: simpleDistribPatchKernel);
        ComputeHelper.Dispatch(hotspotDistribCS, generalSettings.width, generalSettings.height, 1, kernelIndex: visitFreqPatchKernel);
        //ComputeHelper.CopyRenderTexture(hotspotDistribMap, visitFreqPatchMap);
        ComputeHelper.CopyRenderTexture(visitFreqPatchMap, hotspotDistribMap);

        //ComputeHelper.CopyRenderTexture(agentImpactMap, erosionMap);

        ComputeHelper.Dispatch(drawAgentsCS, generalSettings.numAgents, 1, 1, kernelIndex: agentPresenceKernel);

        ComputeHelper.Dispatch(drawAgentsCS, generalSettings.numAgents, 1, 1, kernelIndex: agentImpactKernel);

        //ComputeHelper.CopyRenderTexture(erosionMap, agentImpactMap);

        //ComputeHelper.Dispatch(drawAgentsCS, generalSettings.width, generalSettings.height, 1, kernelIndex: erosionKernel);
        ComputeHelper.Dispatch(drawAgentsCS, generalSettings.numAgents, 1, 1, kernelIndex: erosionKernel);


        //ComputeHelper.Dispatch(drawAgentsCS, generalSettings.width, generalSettings.height, 1, kernelIndex: finalNetworkKernel);

        ComputeHelper.Dispatch(heightMapCS, generalSettings.width, generalSettings.height, 1, kernelIndex: originalHeightMapUpdateKernel);
        ComputeHelper.Dispatch(heightMapCS, generalSettings.width, generalSettings.height, 1, kernelIndex: currentHeightMapUpdateKernel);

        ComputeHelper.Dispatch(heightMapTopographyDisplayCS, generalSettings.width, generalSettings.height, 1, kernelIndex: heightMapColorScaledUpdateKernel);
        ComputeHelper.Dispatch(heightMapTopographyDisplayCS, generalSettings.width, generalSettings.height, 1, kernelIndex: heightMapTopographicalUpdateKernel);
        ComputeHelper.Dispatch(heightMapTopographyDisplayCS, generalSettings.width, generalSettings.height, 1, kernelIndex: heightMapEdgeDetectionUpdateKernel);

        ComputeHelper.Dispatch(originalLandFieldCS, generalSettings.width, generalSettings.height, 1, kernelIndex: originalLandFieldUpdateKernel);

        //ComputeHelper.Dispatch(simulationDisplayCS, generalSettings.width, generalSettings.height, 1, kernelIndex: environmentUpdateKernel);

        ComputeHelper.Dispatch(drawAgentsCS, generalSettings.numAgents, 1, 1, kernelIndex: displayAllKernel);



    }


    //====================================================================================================================================================
    //////////////////////////////////////////////////////////////////// Unity Main ///////////////////////////////////////////////////////////////////////////
    //====================================================================================================================================================


    protected virtual void Start()
    {
        Application.runInBackground = runSimInBackground;

        CleanUpFolder(networkGensFolder);

        InitializeSimDataPath(fileNamesDataFileName);

        Init();

        if (!multiScreenMode)
        {
            transform.GetComponentInChildren<MeshRenderer>().material.mainTexture = displayAllMap;
        }
        else
        {
            Transform visitFreqChildTransform = transform.Find("VisitFreqHotspot");
            visitFreqChildTransform.GetComponent<MeshRenderer>().material.mainTexture = visitFreqPatchMap;

            Transform affordanceTransform = transform.Find("Affordance");
            affordanceTransform.GetComponent<MeshRenderer>().material.mainTexture = transportsAffordanceMap;

            //Transform finalNetworkTransform = transform.Find("VisitFreqHotspot");
            //finalNetworkTransform.GetComponent<MeshRenderer>().material.mainTexture = finalNetworkMap;
        }
    }
   

    void FixedUpdate()
    {
        if (!endSimulation && currentGenerationNum <= maxGeneration)
        {
            Camera[] cameras = Camera.allCameras;
            foreach (Camera cam in cameras)
            {
                cam.enabled = activateRendering;
            }

            foreach (Renderer renderer in FindObjectsByType<Renderer>(FindObjectsSortMode.None))
            {
                renderer.enabled = activateRendering;
            }

            for (int i = 0; i < generalSettings.stepsPerFrame; i++)
            {
                RunSimulation();
                //numIt++;
            }
            numIt += generalSettings.stepsPerFrame;
        }
        else
        {

            Debug.Log("Maximum iterations has been reached or simulation has been ended manually !");

            // This will stop the play mode in the Unity Editor
#if UNITY_EDITOR
            EditorApplication.isPlaying = false;
#else
    // For builds, you might want to add code to gracefully exit or halt simulation logic
#endif
        }
    }

    void LateUpdate()
    {
        // If reach end of current generation
        if (numIt >= numItPerGeneration)
        {
            // At the end of each generation, the resulting network is evaluated and data are stored
            numIt = 0;
            currentGenerationNum++;

            // Save the resulting network with afforded transport modes
            ComputeHelper.ClearRenderTexture(transportsAffordanceMap);
            drawAgentsCS.SetTexture(transportsAffordanceKernel, "TrailMap", trailMap);
            drawAgentsCS.SetTexture(transportsAffordanceKernel, "TransportsAffordanceMap", transportsAffordanceMap);

            ComputeHelper.Dispatch(drawAgentsCS, generalSettings.width, generalSettings.height, 1, kernelIndex: transportsAffordanceKernel);

            affordanceNetwork = TextureManageUtility.ConvertToTexture2D(transportsAffordanceMap, TextureFormat.RGB565);
            TextureManageUtility.SaveTexture(affordanceNetwork, "affordanceNetworkForGen_" + currentGenerationNum, networkGensFolder);


            // Start image processing to evaluate transport mode distribution

            //var costAgentsEvalVar = costAgentsEvalData;
            var costInfraEvalVar = costInfraEvalData;

            //ComputeHelper.CreateStructuredBuffer(ref costAgentsEvalCountBuffer, costAgentsEvalVar);
            //drawAgentsCS.SetBuffer(initiateAgentsCostEvalKernel, "costAgentsEval", costAgentsEvalCountBuffer);

            ComputeHelper.CreateStructuredBuffer(ref costInfraEvalCountBuffer, costInfraEvalVar);
            drawAgentsCS.SetBuffer(initiateInfraCostEvalKernel, "costInfraEval", costInfraEvalCountBuffer);

            ComputeHelper.Dispatch(drawAgentsCS, 1, 1, 1, kernelIndex: initiateInfraCostEvalKernel);

            drawAgentsCS.SetTexture(evaluateNetworkCostKernel, "TransportsAffordanceMap", transportsAffordanceMap);
            drawAgentsCS.SetBuffer(evaluateNetworkCostKernel, "transportModes", transportAffordanceSettingsBuffer);
            drawAgentsCS.SetBuffer(evaluateNetworkCostKernel, "costInfraEval", costInfraEvalCountBuffer);

            //drawAgentsCS.SetBuffer(evaluateAgentsCostKernel, "agents", agentsBuffer);
            //drawAgentsCS.SetBuffer(evaluateAgentsCostKernel, "costAgentsEval", costAgentsEvalCountBuffer);

            ComputeHelper.Dispatch(drawAgentsCS, generalSettings.width, generalSettings.height, 1, kernelIndex: evaluateNetworkCostKernel);

            // Get data and store in excel file

            agentsBuffer.GetData(debugStoredData);

            //costAgentsEvalCountBuffer.GetData(costAgentsEvalCountStoredData);
            costInfraEvalCountBuffer.GetData(costInfraEvalCountStoredData);

            // Store Infrastructure Evaluation and Use Evaluation Data

            for (int j = 0; j < transportAffordanceSettings.transportModes.Length; j++)
            {
                dataFiles.countInfraTMDataFile.WriteLine(
                    currentGenerationNum + "," +
                    j + "," +
                    costInfraEvalCountStoredData[j]
                );
            }

            var convTrailDiffVar = convTrailPixCount;

            ComputeHelper.CreateStructuredBuffer(ref convergenceTrailDiffBuffer, convTrailDiffVar);
            drawAgentsCS.SetBuffer(initConvTrailDiffKernel, "convTrailDiffBuffer", convergenceTrailDiffBuffer);

            ComputeHelper.Dispatch(drawAgentsCS, 1, 1, 1, kernelIndex: initConvTrailDiffKernel);

            drawAgentsCS.SetTexture(convTrailDiffUpdateKernel, "PreviousTrailMap", trailMap);
            drawAgentsCS.SetTexture(convTrailDiffUpdateKernel, "CurrentTrailMap", erosionMap);

            drawAgentsCS.SetBuffer(convTrailDiffUpdateKernel, "convTrailDiffBuffer", convergenceTrailDiffBuffer);

            ComputeHelper.Dispatch(drawAgentsCS, generalSettings.width, generalSettings.height, 1, kernelIndex: convTrailDiffUpdateKernel);

            convergenceTrailDiffBuffer.GetData(convTrailPixCountStoredData);

            // Store Number of Travels completed for each generation

            int totTravels = 0;
            for (int k = 0; k < debugStoredData.Length; k++)
            {
                totTravels += debugStoredData[k].completeTravels;
            }

            float convergencePercent = (float.IsNaN(convTrailPixCountStoredData[0] / (float)(costInfraEvalCountStoredData.Sum()))) ? 0 : (float)convTrailPixCountStoredData[0] / (float)(costInfraEvalCountStoredData.Sum());

            dataFiles.countTotCostTMDataFile.WriteLine(
                currentGenerationNum + "," +
                totTravels + "," +
                convergencePercent + "," +
                costInfraEvalCountStoredData.Sum()
            );

            // After evaluation, reinitialize for next generation

            UpdateAgents(debugStoredData);

            ComputeHelper.ClearRenderTexture(trailMap);

            ComputeHelper.CopyRenderTexture(erosionMap, trailMap);

            ComputeHelper.ClearRenderTexture(erosionMap);

        }

        // Plot textures at runtime management

        if (showMode == ShowMode.ShowAgentsOnly)
        {
            ComputeHelper.ClearRenderTexture(displayAllMap);

            drawAgentsCS.SetTexture(agentPresenceKernel, "AgentPresenceMap", displayAllMap);

            ComputeHelper.Dispatch(drawAgentsCS, generalSettings.numAgents, 1, 1, kernelIndex: agentPresenceKernel);


        }
        else if (showMode == ShowMode.ShowHotspotsOnly)
        {

            ComputeHelper.ClearRenderTexture(displayAllMap);

            hotspotDistribCS.SetTexture(visitFreqPatchKernel, "VisitFreqPatchMap", displayAllMap);

            ComputeHelper.Dispatch(hotspotDistribCS, generalSettings.width, generalSettings.height, 1, kernelIndex: visitFreqPatchKernel);


        }
        else if (showMode == ShowMode.ShowCurrentErosion)
        {
            ComputeHelper.ClearRenderTexture(displayAllMap);

            ComputeHelper.CopyRenderTexture(erosionMap, displayAllMap);
        }

        else if (showMode == ShowMode.ShowCurrentTrail)
        {
            ComputeHelper.ClearRenderTexture(displayAllMap);

            ComputeHelper.CopyRenderTexture(trailMap, displayAllMap);
        }


        else if (showMode == ShowMode.ShowAffordanceNetwork)
        {
            ComputeHelper.ClearRenderTexture(displayAllMap);

            drawAgentsCS.SetTexture(transportsAffordanceKernel, "TransportsAffordanceMap", displayAllMap);

            ComputeHelper.Dispatch(drawAgentsCS, generalSettings.width, generalSettings.height, 1, kernelIndex: transportsAffordanceKernel);
        }

        else if (showMode == ShowMode.ShowAgentTransports)
        {
            ComputeHelper.ClearRenderTexture(displayAllMap);

            drawAgentsCS.SetTexture(agentTransportKernel, "AgentTransportMap", displayAllMap);

            ComputeHelper.Dispatch(drawAgentsCS, generalSettings.numAgents, 1, 1, kernelIndex: agentTransportKernel);
        }

        else if (showMode == ShowMode.ShowOriginalHeightMapOnly)
        {
            ComputeHelper.ClearRenderTexture(displayAllMap);

            if (heightMapDisplayMode == HeightMapDisplayMode.Detailed)
            {
                heightMapCS.SetTexture(originalHeightMapUpdateKernel, "OriginalHeightMap", displayAllMap);

                ComputeHelper.Dispatch(heightMapCS, generalSettings.width, generalSettings.height, 1, kernelIndex: originalHeightMapUpdateKernel);
            }

            else if (heightMapDisplayMode == HeightMapDisplayMode.ColorScaledTopography)
            {
                heightMapTopographyDisplayCS.SetTexture(heightMapColorScaledUpdateKernel, "ColorScaledMap", displayAllMap);

                ComputeHelper.Dispatch(heightMapTopographyDisplayCS, generalSettings.width, generalSettings.height, 1, kernelIndex: heightMapColorScaledUpdateKernel);
            }

            else if (heightMapDisplayMode == HeightMapDisplayMode.EdgeDetection)
            {
                heightMapTopographyDisplayCS.SetTexture(heightMapEdgeDetectionUpdateKernel, "EdgeDetectionMap", displayAllMap);

                ComputeHelper.Dispatch(heightMapTopographyDisplayCS, generalSettings.width, generalSettings.height, 1, kernelIndex: heightMapEdgeDetectionUpdateKernel);
            }


        }

        else if (showMode == ShowMode.ShowOriginalLandFieldOnly)
        {
            ComputeHelper.ClearRenderTexture(displayAllMap);

            originalLandFieldCS.SetTexture(originalLandFieldUpdateKernel, "OriginalLandFieldMap", displayAllMap);
            ComputeHelper.Dispatch(originalLandFieldCS, generalSettings.width, generalSettings.height, 1, kernelIndex: originalLandFieldUpdateKernel);
        }

        else
        {
            ComputeHelper.ClearRenderTexture(displayAllMap);

            drawAgentsCS.SetTexture(displayAllKernel, "DisplayAllMap", displayAllMap);
            ComputeHelper.Dispatch(drawAgentsCS, generalSettings.width, generalSettings.height, 1, kernelIndex: displayAllKernel);
        }

    }

    void OnDestroy()
    {
        //====================================================================================================================================================
        ////////////////////////////////////////////////////////////// Save Textures //////////////////////////////////////////////////////////////////////
        //====================================================================================================================================================

        Texture2D finalNetwork = TextureManageUtility.ConvertToTexture2D(trailMap, TextureFormat.ARGB32);
        TextureManageUtility.SaveTexture(finalNetwork, "finalNetwork");

        Texture2D hotspotsVisitFreqPatch = TextureManageUtility.ConvertToTexture2D(visitFreqPatchMap, TextureFormat.ARGB32);
        TextureManageUtility.SaveTexture(hotspotsVisitFreqPatch, "hotspotsVisitFreq");

        Texture2D originalHeightmapDetailedDisplay = TextureManageUtility.ConvertToTexture2D(originalHeightMap, TextureFormat.ARGB32);
        TextureManageUtility.SaveTexture(originalHeightmapDetailedDisplay, "originalHeightmap_Detailed");

        Texture2D originalHeightmapTopographyDisplay = TextureManageUtility.ConvertToTexture2D(heightMapTopographicalMap, TextureFormat.ARGB32);
        TextureManageUtility.SaveTexture(originalHeightmapTopographyDisplay, "originalHeightmap_Topography");

        Texture2D originalHeightmapEdgeDetection = TextureManageUtility.ConvertToTexture2D(heightMapEdgeDetectionMap, TextureFormat.ARGB32);
        TextureManageUtility.SaveTexture(originalHeightmapEdgeDetection, "originalHeightmap_EdgeDetection");

        Texture2D originalHeightmapColorScaleDisplay = TextureManageUtility.ConvertToTexture2D(heightMapColorScaledMap, TextureFormat.ARGB32);
        TextureManageUtility.SaveTexture(originalHeightmapColorScaleDisplay, "originalHeightmap_ColorScale");


        //====================================================================================================================================================
        ////////////////////////////////////////////////////////////// Release Buffers //////////////////////////////////////////////////////////////////////
        //====================================================================================================================================================


        ComputeHelper.Release(
            agentsBuffer,
            slimeSettingsBuffer,
            generalSettingsBuffer,
            transportAffordanceSettingsBuffer,
            hotspotsBuffer,

            originalHeightMapOctavesBuffer,
            currentHeightMapBuffer,

            originalLandOctavesBuffer,
            landsSettingsBuffer
        );

        ComputeHelper.Release(
            agentPresenceMap,
            agentImpactMap,
            erosionMap,
            trailMap,

            transportsAffordanceMap,
            agentTransportMap,

            hotspotDistribMap,
            simpleDistribPatchMap,
            visitFreqPatchMap,

            originalHeightMap,
            currentHeightMap,
            heightMapColorScaledMap,
            heightMapTopographicalMap,
            heightMapEdgeDetectionMap,

            originalLandFieldMap,
            environmentDisplayAllMap,
            displayAllMap
        );


        //====================================================================================================================================================
        ////////////////////////////////////////////////////////////// Close Files with Saved Data /////////////////////////////////////////////////////////
        //====================================================================================================================================================

        dataFiles.countAgentTMDataFile.Close();
        dataFiles.countInfraTMDataFile.Close();
        dataFiles.countTotCostTMDataFile.Close();

        //ClearShaderCache_Command();
    }

}
