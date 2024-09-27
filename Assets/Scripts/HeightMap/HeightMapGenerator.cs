using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class HeightmapGenerator : MonoBehaviour
{
    [System.Serializable]
    public class Location
    {
        public double latitude;
        public double longitude;
    }

    [System.Serializable]
    public class ElevationRequest
    {
        public Location[] locations;
    }

    [System.Serializable]
    public class ElevationResponse
    {
        public ElevationResult[] results;
    }

    [System.Serializable]
    public class ElevationResult
    {
        public double latitude;
        public double longitude;
        public double elevation;
    }

    public IEnumerator GetElevationData(double lat1, double lon1, double lat2, double lon2, int resolution = 100)
    {
        // Ensure lat1 < lat2 and lon1 < lon2
        double minLat = Mathf.Min((float)lat1, (float)lat2);
        double maxLat = Mathf.Max((float)lat1, (float)lat2);
        double minLon = Mathf.Min((float)lon1, (float)lon2);
        double maxLon = Mathf.Max((float)lon1, (float)lon2);

        // Calculate the number of points based on resolution
        int latPoints = Mathf.FloorToInt((float)((maxLat - minLat) / resolution)) + 1;
        int lonPoints = Mathf.FloorToInt((float)((maxLon - minLon) / resolution)) + 1;

        float[,] heightmap = new float[latPoints, lonPoints];

        for (int i = 0; i < latPoints; i++)
        {
            for (int j = 0; j < lonPoints; j++)
            {
                double lat = Mathf.Lerp((float)minLat, (float)maxLat, (float)i / (latPoints - 1));
                double lon = Mathf.Lerp((float)minLon, (float)maxLon, (float)j / (lonPoints - 1));

                string url = $"https://api.open-meteo.com/v1/elevation?latitude={lat}&longitude={lon}";

                UnityWebRequest request = UnityWebRequest.Get(url);
                yield return request.SendWebRequest();

                if (request.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError($"Error: {request.error} for coordinates {lat}, {lon}");
                }
                else
                {
                    string responseText = request.downloadHandler.text;
                    Debug.Log($"Response for {lat}, {lon}: {responseText}");

                    // Parse the JSON response
                    JsonData jsonData = JsonUtility.FromJson<JsonData>(responseText);
                    heightmap[i, j] = jsonData.elevation;
                }
            }
        }

        CreateTerrainFromHeightmap(heightmap);
    }

    [System.Serializable]
    private class JsonData
    {
        public float elevation;
    }

    private void CreateTerrainFromHeightmap(float[,] heightmap)
    {
        int width = heightmap.GetLength(1);
        int height = heightmap.GetLength(0);

        // Create a new Texture2D
        Texture2D heightmapTexture = new Texture2D(width, height, TextureFormat.ARGB32, false);

        // Find the min and max elevation values for normalization
        float minElevation = float.MaxValue;
        float maxElevation = float.MinValue;
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                minElevation = Mathf.Min(minElevation, heightmap[y, x]);
                maxElevation = Mathf.Max(maxElevation, heightmap[y, x]);
            }
        }

        // Normalize and set pixel values
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                // Normalize the height value to 0-1 range
                float normalizedHeight = Mathf.InverseLerp(minElevation, maxElevation, heightmap[y, x]);

                // Set the pixel color (grayscale)
                heightmapTexture.SetPixel(x, y, new Color(normalizedHeight, normalizedHeight, normalizedHeight, 1));
            }
        }

        // Apply all SetPixel calls
        heightmapTexture.Apply();

        // Save the texture as a PNG file (optional)
        byte[] bytes = heightmapTexture.EncodeToPNG();
        System.IO.File.WriteAllBytes(Application.persistentDataPath + "/HeightmapTexture.png", bytes);

        Debug.Log("Heightmap texture created and saved to Assets folder.");

        // Optionally, you can use the texture in your scene
        // For example, assign it to a material
        // Renderer renderer = GetComponent<Renderer>();
        // renderer.material.mainTexture = heightmapTexture;
    }

    // Example usage
    void Start()
    {
        StartCoroutine(GetElevationData(42, -5, 51, 9, 2000));
    }
}