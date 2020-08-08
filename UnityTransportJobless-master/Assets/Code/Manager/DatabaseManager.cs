using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public static class DatabaseManager
{
    public static string response;
    public readonly static string BaseUrl = "https://studenthome.hku.nl/~geoffrey.hendrikx/kernmodule4/";
    public static int ServerID = 1;
    public static string ServerPassword = "vertelikniet";
    public static string sessionID;
    public static UserData myData;

    public static IEnumerator GetHttp(string url = "url")
    {
        var request = UnityWebRequest.Get(BaseUrl + url);
        {
            yield return request.SendWebRequest();

            //Getting the response.
            if (request.isDone && !request.isHttpError)
            {
                response = request.downloadHandler.text;
            }
        }
    }
}

[System.Serializable]
public struct UserData
{
    public int ID;
    public string Username;
}