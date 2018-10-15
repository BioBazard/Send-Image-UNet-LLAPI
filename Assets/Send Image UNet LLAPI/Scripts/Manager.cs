using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Manager : MonoBehaviour {

    public string imagePath = "Textures";
    public string imageName = "image";
    public string imageExtension = "png";

    protected string fullPath;

    protected void Awake()
    {
        string dirPath = Application.streamingAssetsPath + "/" + imagePath;
        Directory.CreateDirectory(dirPath);

        fullPath = dirPath + "/" + imageName + "." + imageExtension;
    }
}
