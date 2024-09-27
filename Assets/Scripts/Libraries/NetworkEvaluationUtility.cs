using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static HotspotDistribSettings;
using static Simulation;

public class NetworkEvaluationUtility
{
    public static void AddUnidirectedSitePairs(ref List<SitePair> sitePairsList, HotspotDistribSettings distribSettings)
    {

        for (int i = 0; i < distribSettings.hotspotsSettings.Length; i++)
        {
            for (int j = 0; j < distribSettings.hotspotsSettings.Length; j++)
            {
                if (i != j)
                {
                    AddUnidirectionalPair(new SitePair { site1 = i, site2 = j }, ref sitePairsList);
                }
            }
        }
    }

    public static void AddBidirectedSitePairs(ref List<SitePair> sitePairsList, HotspotDistribSettings distribSettings)
    {

        for (int i = 0; i < distribSettings.hotspotsSettings.Length; i++)
        {
            for (int j = 0; j < distribSettings.hotspotsSettings.Length; j++)
            {
                if (i != j)
                {
                    AddBidirectionalPair(new SitePair { site1 = i, site2 = j }, ref sitePairsList);
                }
            }
        }
    }

    public static bool UnidirectionalPairExists(SitePair pairToCheck, List<SitePair> sitePairs)
    {
        if (sitePairs != null)
        {
            foreach (SitePair pair in sitePairs)
            {
                if ((pairToCheck.site1 == pair.site1 && pairToCheck.site2 == pair.site2))
                {

                    return true;

                }
            }
        }
        return false;
    }

    public static void AddUnidirectionalPair(SitePair newPair, ref List<SitePair> pairsList)
    {
        if (!UnidirectionalPairExists(newPair, pairsList))
        {
            pairsList.Add(newPair); // Append the new pair
        }
    }

    public static bool BidirectionalPairExists(SitePair pairToCheck, List<SitePair> sitePairs)
    {
        if (sitePairs != null)
        {
            foreach (SitePair pair in sitePairs)
            {
                if ((pairToCheck.site1 == pair.site1 || pairToCheck.site1 == pair.site2))
                {
                    if (pairToCheck.site2 == pair.site1 || pairToCheck.site2 == pair.site2)
                    {
                        return true;
                    }
                }
            }
        }
        return false;
    }

    public static void AddBidirectionalPair(SitePair newPair, ref List<SitePair> pairsList)
    {
        if (!BidirectionalPairExists(newPair, pairsList))
        {
            pairsList.Add(newPair); // Append the new pair
        }
    }

    //public static void EvaluateNetworkEfficiency(
    //    Texture2D finalNetworkMap,
    //    List<SitePair> sitePairs,
    //    Hotspot[] hotspots,
    //    GeneralSettings simSettings,
    //    DataFiles dataFiles,
    //    float[] geodesicDistancesForPairs)
    //{
    //    AStarPathFinder aStarPathFinder = new AStarPathFinder();

    //    // Initialize parameters of AStar path finder
    //    aStarPathFinder.networkTexture = finalNetworkMap;

    //    for (int i = 0; i < sitePairs.Count; i++)
    //    {
    //        aStarPathFinder.start = new AStarPathFinder.Spot(
    //            MathUtility.ConvertRelativeToAbsoluteLocationOnGrid(hotspots[sitePairs[i].site1].location, simSettings.width, simSettings.height),
    //            0,
    //            0,
    //            0,
    //            null,
    //            true
    //            );
    //        aStarPathFinder.end = new AStarPathFinder.Spot(
    //            MathUtility.ConvertRelativeToAbsoluteLocationOnGrid(hotspots[sitePairs[i].site2].location, simSettings.width, simSettings.height),
    //            0,
    //            0,
    //            0,
    //            null,
    //            true
    //            );

    //        aStarPathFinder.InitializePathFinder();

    //        while (aStarPathFinder.openSet.Count > 0)
    //        {
    //            aStarPathFinder.CalculateShortestPath();
    //        };

    //        //float distStartToEnd = 0;

    //        //for(int j = 0; j < aStarPathFinder.path.Count; j++)
    //        //{
    //        //    distStartToEnd += aStarPathFinder.path[i].G;
    //        //}

    //        float distStartToEnd = aStarPathFinder.path[aStarPathFinder.path.Count - 1].G;

    //        dataFiles.evaluateNetworkDataFile.WriteLine(
    //            sitePairs[i].site1 + "," +
    //            sitePairs[i].site2 + "," +
    //            geodesicDistancesForPairs[i] + "," +
    //            distStartToEnd + "," +
    //            geodesicDistancesForPairs[i] / distStartToEnd + "," +
    //            aStarPathFinder.path.Count
    //        );

    //        aStarPathFinder.ClearPathFinder();

    //    }

    //    Debug.Log("Network Evaluation Finished !");

    //}

    public static void EvaluateNetworkCost(
       Texture2D networkMap,
       List<SitePair> sitePairs,
       Hotspot[] hotspots,
       GeneralSettings simSettings,
       DataFiles dataFiles,
       float[] geodesicDistancesForPairs)
    {





    }
}
