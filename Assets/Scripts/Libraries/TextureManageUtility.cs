using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using static HotspotDistribSettings;

public static class TextureManageUtility
{

    public static Texture2D ConvertToTexture2D(RenderTexture rTex, TextureFormat textFormat)
    {
        Texture2D tex = new Texture2D(rTex.width, rTex.height, textFormat, false);
        // ReadPixels looks at the active RenderTexture.
        RenderTexture.active = new RenderTexture(rTex.width, rTex.height, 0);
        Graphics.Blit(rTex, RenderTexture.active);
        tex.ReadPixels(new Rect(0, 0, rTex.width, rTex.height), 0, 0);
        tex.Apply();
        RenderTexture.active = null;
        return tex;
    }

    public static void SaveTexture(Texture2D textureToSave, string fileName)
    {
        byte[] textureBytes = textureToSave.EncodeToPNG(); // Convert texture to PNG format

        // Specify the file path where you want to save the texture
        string filePath = Path.Combine(Application.persistentDataPath, fileName + ".png");

        // Write the texture bytes to a file
        File.WriteAllBytes(filePath, textureBytes);

        Debug.Log("Texture saved to: " + filePath);
    }

    public static void SaveTexture(Texture2D textureToSave, string fileName, string pathName, string absDataPath = "C:/Users/CRL-louis/Documents/TDU/Research/LastSimData")
    {
        byte[] textureBytes = textureToSave.EncodeToPNG(); // Convert texture to PNG format

        // Specify the file path where you want to save the texture
        string filePath = Path.Combine(absDataPath + "/" + pathName, fileName + ".png");

        // Write the texture bytes to a file
        File.WriteAllBytes(filePath, textureBytes);

        Debug.Log("Texture saved to: " + filePath);
    }

    public static Texture2D LoadTexture(string fileName, int width, int height, string absDataPath = "C:/Users/CRL-louis/Documents/TDU/Research/LastSimData")
    {
        string filePath = Path.Combine(absDataPath, fileName + ".png");

        if (File.Exists(filePath))
        {
            byte[] fileData = File.ReadAllBytes(filePath);
            Texture2D loadedTexture = new Texture2D(width, height); // Create a new texture
            loadedTexture.LoadImage(fileData); // Load the saved image file into the texture
            return loadedTexture;
        }
        else
        {
            Debug.LogError("Texture file not found at path: " + filePath);
            return null;
        }
    }



    public static void SaveResultingTextureFromSimulation(RenderTexture renderTexture, string simTextureFileName)
    {

        if (renderTexture != null)
        {
            Texture2D tempTexture = ConvertToTexture2D(renderTexture, TextureFormat.R8);

            //// Create a new Texture2D with the same dimensions as your RenderTexture to hold the contents of the RenderTexture
            //Texture2D tempTexture = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.R8, false);

            //// Set the RenderTexture as the active render target
            //RenderTexture.active = renderTexture;

            //// Read the pixels from the RenderTexture into the Texture2D
            //tempTexture.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);

            //// Apply changes and release the RenderTexture
            //tempTexture.Apply();
            //RenderTexture.active = null; // Reset the active render target

            SaveTexture(tempTexture, simTextureFileName);
            // Remember to destroy the temporary texture to free up memory
            Object.Destroy(tempTexture);

        }
        else
        {
            Debug.LogWarning("RenderTexture or file path is invalid.");
        }
    }
}
