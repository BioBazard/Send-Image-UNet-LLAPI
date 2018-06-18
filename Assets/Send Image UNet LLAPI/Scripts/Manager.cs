using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Manager : MonoBehaviour {

    public string imagePath = "Textures";
    public string imageName = "image";
    public string imageExtension = "png";

    protected string fullPath;

    protected void Awake()
    {
        fullPath = Application.streamingAssetsPath + "/" + imagePath + "/" + imageName + "." + imageExtension;
    }
}
