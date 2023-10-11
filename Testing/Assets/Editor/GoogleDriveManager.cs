using UnityEngine.Networking;
using System;
using UnityEngine;
using System.Threading.Tasks;
using System.Collections.Generic;

public class GoogleDriveManager{

    public const string TOKEN_URL = "http://magiccircle.games.ucla.edu/tools/gameDownloader/service-account.php?urlSecurityKey=tZw3y5B8DaFdJfMjQmSpVsXuZx4z6B9EbGeKgNjRnTqVtYv2x5A7CaF";
    public const string API_URL = "https://www.googleapis.com/drive/v3/files/";
    public string token;


    public async Task GetFile(string fileReference, string destinationPath) {
        await GetToken();
        string requestURL = API_URL + fileReference+ "?alt=media";
        destinationPath = destinationPath + "/Packages/" + fileReference + ".unitypackage";
        Debug.Log("Attempting to get file...");
        using (UnityWebRequest webRequest = new UnityWebRequest(requestURL))
        {
            webRequest.method = UnityWebRequest.kHttpVerbGET;

            DownloadHandlerFile downloadHandlerFile = new DownloadHandlerFile(destinationPath);
            webRequest.downloadHandler = downloadHandlerFile;
            webRequest.SetRequestHeader("Authorization", "Bearer "+token);
            webRequest.SendWebRequest();

            while (!webRequest.isDone || !downloadHandlerFile.isDone)
            {
                await Task.Yield();
            }


            if (webRequest.result != UnityWebRequest.Result.Success)
            {
                Debug.Log("Failed:"+webRequest.error);
                
            }
            else
            {
                Debug.Log("Completed Download to:"+destinationPath);
            }
        }

    }

    public async Task<string> GetToken()
    {
        Debug.Log("Attempting to get token");
        using (UnityWebRequest webRequest = UnityWebRequest.Get(TOKEN_URL))
        {

            webRequest.SendWebRequest();
            while (!webRequest.isDone)
            {
                await Task.Yield();
            }



            if ((int)webRequest.result > 1)
            {
                Debug.Log(webRequest.error);
                return null;
            }
            else
            {
                string jsonResult = webRequest.downloadHandler.text;
                token = jsonResult;
                Debug.Log("Got token:" + token);
                return jsonResult;
            }
        }
    }
}