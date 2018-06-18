using System.IO;
using UnityEngine;

public static class SaveFile {
    
    public static string HEADER_IMAGE_UPDATE = "up_img";
    public static string HEADER_IMAGE_END = "end_img";

    /// <summary>
    /// Network : Image bytes lenght (by packet)
    /// </summary>
    public static int IMAGE_LENGTH = 1000;

    /// <summary>
    /// Network : Header bytes lenght (by packet)
    /// </summary>
    public static int HEADER_LENGTH = 402;

    /// <summary>
    /// Return a Texture2D with file path
    /// </summary>
    /// <param name="path">Texture file path</param>
    public static Texture2D GetTextureFromFilePath(string path)
    {
        if (!File.Exists(path))
            return null;

        byte[] data = File.ReadAllBytes(path);

        return GetTextureFromBytes(data);
    }

    /// <summary>
    /// Return a Texture2D with bytes file
    /// </summary>
    /// <param name="path">Bytes file</param>
    public static Texture2D GetTextureFromBytes(byte[] data)
    {
        Texture2D texture = new Texture2D(2, 2, TextureFormat.ARGB32, false);
        texture.LoadImage(data);

        return texture;
    }

    /// <summary>
    /// Resize a Texture at the file path
    /// </summary>
    /// <param name="path">Texture file</param>
    /// <param name="resize">This is the divided ratio of the Texture</param>
    public static void ResizeTexture(string path, int resize)
    {
        Texture2D oldTex = GetTextureFromFilePath(path);
        Texture2D newTex = new Texture2D(oldTex.width/resize, oldTex.height/resize, TextureFormat.ARGB32, false);

        for (int w = 0; w < newTex.width; w++)
        {
            for (int h = 0; h < newTex.height; h++)
            {
                Color pixel = oldTex.GetPixel(w * resize, h * resize);
                newTex.SetPixel(w, h, pixel);
            }
        }

        byte[] data = newTex.EncodeToPNG();
        File.WriteAllBytes(path, data);

        Debug.Log("Resize Picture Done : " + path);
    }
}
