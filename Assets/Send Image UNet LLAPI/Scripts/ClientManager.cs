using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

public class ClientManager : Manager
{
    //public bool compresseImage = true;

    /// <summary>
    /// Button : Send the image to server
    /// </summary>
    public void Button_SendImage()
    {
        StartCoroutine(SendImage());
    }

    IEnumerator SendImage()
    {
        NetworkLowLevelAPI.instance.Connect();

        yield return new WaitForSeconds(0.1f); // Wait to send socket

        if (File.Exists(fullPath))
        {
            Debug.Log(fullPath);
            //if (compresseImage)
            //    SaveFile.ResizeTexture(fullPath, 4);

            yield return new WaitForSeconds(1.0f); // wait resize

            byte[] userImage = File.ReadAllBytes(fullPath);
            //Debug.Log("bytes lenght start : " + userImage.Length);

            int nbSocket = (userImage.Length / SaveFile.IMAGE_LENGTH);

            // Lasts bytes
            int lastsBytes = userImage.Length - SaveFile.IMAGE_LENGTH * nbSocket;

            byte[] newImageBytes;

            for (int i = 0; i < nbSocket; i++)
            {
                newImageBytes = new byte[SaveFile.IMAGE_LENGTH];

                for (int j = 0; j < newImageBytes.Length; j++)
                {
                    newImageBytes[j] = userImage[j + i * SaveFile.IMAGE_LENGTH];
                }

                yield return new WaitForSeconds(0.1f); // Wait to send socket

                if (i == nbSocket - 1 && lastsBytes == 0)
                    NetworkLowLevelAPI.instance.SendSocketMessage(newImageBytes, SaveFile.HEADER_IMAGE_END + ";" + imageName);
                else
                    NetworkLowLevelAPI.instance.SendSocketMessage(newImageBytes, SaveFile.HEADER_IMAGE_UPDATE + ";" + imageName);

                Debug.Log(imageName + " : " + i + "/" + nbSocket);
            }

            if (lastsBytes > 0)
            {
                newImageBytes = new byte[lastsBytes];

                for (int i = 0; i < newImageBytes.Length; i++)
                {
                    newImageBytes[i] = userImage[i + nbSocket * SaveFile.IMAGE_LENGTH];
                }

                yield return new WaitForSeconds(0.1f); // Wait to send socket
                NetworkLowLevelAPI.instance.SendSocketMessage(newImageBytes, SaveFile.HEADER_IMAGE_END + ";" + imageName);

                Debug.Log(imageName + " : " + nbSocket + "/" + nbSocket);
            }
        }
    }
}
