using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using Unity.Mathematics;
using Unity.PlasticSCM.Editor.WebApi;
using UnityEngine;
using static AStarPathFinder;

public class AStarPathFinder
{
    public class Spot
    {
        public Vector2 locationOnGrid;
        public float G;
        public float H;
        public float F;
        public List<Spot> neighboringSpots;
        public bool bIsWalkable = true;
        public Spot previous = null;

        //public Spot parent;
        //public bool isInLineOfSight;
        //public float parentDirection;

        public Spot(Vector2 locationOnGrid, float G, float H, float F, List<Spot> neighbors, bool bIsWalkable)
        {
            this.locationOnGrid = locationOnGrid;
            this.G = G;
            this.H = H;
            this.F = F;
            this.neighboringSpots = neighbors != null ? neighbors : new List<Spot>();
            this.bIsWalkable = bIsWalkable;
            this.previous = null;

            //this.parent = null;
            //this.isInLineOfSight = false;
            //this.parentDirection = 0;
        }
    }

    //public int nonNullTotalSpot = 0;
    //int width;
    //int height;

    public Texture2D networkTexture;

    public Spot start;
    public Spot end;

    public List<Spot> openSet = new List<Spot>();
    public HashSet<Spot> closedSet = new HashSet<Spot>();

    public Spot[,] spotGrid;
    public List<Spot> path;

    //List<Vector2> FindPath(Vector2 start, Vector2 end)
    //{

    //}

    public bool IsWalkable(Texture2D texture, int x, int y)
    {
        return texture.GetPixel(x, y).r > 0 ? true : false;
    }

    void AddNeighbors(Spot spot, Spot[,] spotGrid)
    {
        spot.neighboringSpots = new List<Spot>();

        for (int offsetX = -1; offsetX <= 1; offsetX++)
        {
            for (int offsetY = -1; offsetY <= 1; offsetY++)
            {

                if (offsetX == 0 && offsetY == 0)
                {
                    continue;
                }

                int sampleX = Mathf.Min(networkTexture.width - 1, Mathf.Max(0, (int)spot.locationOnGrid.x + offsetX));
                int sampleY = Mathf.Min(networkTexture.height - 1, Mathf.Max(0, (int)spot.locationOnGrid.y + offsetY));

                //Debug.Log(sampleX);
                //Debug.Log(spotGrid[sampleX, sampleY].locationOnGrid);
                
                if (spotGrid[sampleX,sampleY] != null)
                {
                    spot.neighboringSpots.Add(spotGrid[sampleX, sampleY]);
                }
                

            }
        }
        //spot.neighboringSpots.Add(spotGrid[(int)spot.locationOnGrid.x + 1, (int)spot.locationOnGrid.y]);
        //spot.neighboringSpots.Add(spotGrid[(int)spot.locationOnGrid.x, (int)spot.locationOnGrid.y + 1]);
        //spot.neighboringSpots.Add(spotGrid[(int)spot.locationOnGrid.x - 1, (int)spot.locationOnGrid.y]);
        //spot.neighboringSpots.Add(spotGrid[(int)spot.locationOnGrid.x, (int)spot.locationOnGrid.y - 1]);
    }

    public float HeuristicDistance(Spot spot1, Spot spot2)
    {
        return Vector2.Distance(spot1.locationOnGrid, spot2.locationOnGrid);

    }

    public float HeuristicEnergyConsuming(Spot spot1, Spot spot2)
    {
        return 0;

    }

    public void ClearPathFinder()
    {
        openSet.Clear();
        closedSet.Clear();
    }

    public void InitializePathFinder()
    {
        
        spotGrid = new Spot[networkTexture.width, networkTexture.height];
        
        for (int i = 0; i < networkTexture.width; i++)
        {
            for (int j = 0; j < networkTexture.height; j++)
            {
                if(IsWalkable(networkTexture, i, j))
                {
                    spotGrid[i, j] = new Spot(
                        new Vector2(i, j),
                        0,
                        0,
                        0,
                        null,
                        true
                    );
                    //nonNullTotalSpot += 1;
                }
                
                //AddNeighbors(spotGrid[i, j], spotGrid);

            }
        }


        //Debug.Log("Spotgrid set");

        foreach (Spot spot in spotGrid)
        {
            if (spot != null)
            {
                AddNeighbors(spot, spotGrid);
                //Debug.Log("Neighbors added at location " + spot.locationOnGrid + " : " + spot.neighboringSpots.Count);
            }
            
        }

        AddNeighbors(start, spotGrid);
        AddNeighbors(end, spotGrid);

        openSet.Add(start);
        path = new List<Spot>();
    }

    public void CalculateShortestPath()
    {
        //Debug.Log("openSetCount = " + openSet.Count);
        if (openSet.Count > 0)
        {
            // Keep calculate path
            // Selection of the current node (winner)
            int winner = 0;
            for (int i = 0; i < openSet.Count; i++)
            {
                if (openSet[i].F < openSet[winner].F)
                {
                    winner = i;
                }

            }
            
            var current = openSet[winner];
            //Debug.Log("Current locationOnGrid = " + openSet[winner].locationOnGrid);
            // Destination reached check
            if (current.locationOnGrid == end.locationOnGrid)
            {
                // End of algorithm

                //var temp = current;
                //path.Add(temp);
                //while (temp.previous != null)
                //{
                //    path.Add(temp.previous);
                //    temp = temp.previous;
                //}

                var temp = current;
                path.Insert(0, temp);
                while (temp.previous != null)
                {
                    path.Insert(0, temp.previous);
                    temp = temp.previous;
                }
            }

            // Node removal and update
            openSet.Remove(current);
            closedSet.Add(current);

            List<Spot> neighborsToAdd = new List<Spot>();
            //Debug.Log("CountNeighboringSpotAtCurrent = " + current.neighboringSpots.Count);
            for (int i = 0; i < current.neighboringSpots.Count; i++)
            {
                var neighbor = current.neighboringSpots[i];
                //Debug.Log("Neighboring spot locationOnGrid of " + i + "e neighbor : " + current.neighboringSpots[i].locationOnGrid);
                if (neighbor == null)
                {
                    continue;
                }

                if (!closedSet.Contains(neighbor) && neighbor.bIsWalkable)
                {
                    float tempG = current.G + HeuristicDistance(current, neighbor);

                    bool newPath = false;

                    if (openSet.Contains(neighbor))
                    {
                        if (tempG < neighbor.G)
                        {
                            neighbor.G = tempG;
                            newPath = true;
                        }
                    }
                    else
                    {
                        neighbor.G = tempG;
                        newPath = true;
                        neighborsToAdd.Add(neighbor);
                    }

                    if (newPath)
                    {
                        neighbor.H = HeuristicDistance(neighbor, end);
                        neighbor.F = neighbor.G + neighbor.H;
                        neighbor.previous = current;
                    }


                }

            }

            openSet.AddRange(neighborsToAdd);
        }
        else
        {
            // No solution found
            // End of algorithm
            Debug.Log("No solution !");
            return;
        }

    }

    //// Start is called before the first frame update
    //void Start()
    //{

    //    InitializePathFinder();
        
    //}

    //// Update is called once per frame
    //void Update()
    //{
    //    CalculateShortestPath();

    //}
}
