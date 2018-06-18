using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class ServerManager : Manager
{
    public Text debugText;
    
    public void SaveNewImage (byte[] image)
    {
        File.WriteAllBytes(fullPath, image);

        debugText.text = "Image save : "+ fullPath;

        Debug.Log("Save Picture Done : " + fullPath);
    }
}
