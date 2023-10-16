using UnityEngine.Networking;
using System;
using UnityEngine;
using System.Threading.Tasks;
using System.Collections.Generic;
using Newtonsoft.Json;

public class GoogleDriveManager
{


    public const string TOKEN_URL = "http://magiccircle.games.ucla.edu/tools/gameDownloader/service-account.php?urlSecurityKey=tZw3y5B8DaFdJfMjQmSpVsXuZx4z6B9EbGeKgNjRnTqVtYv2x5A7CaF";
    public const string API_URL = "https://www.googleapis.com/drive/v3/files/";

    private string GetJSONURL(string documentID, string pageName)
    {
        string baseUrl = "https://sheets.googleapis.com/v4/spreadsheets/" + documentID + "/values/" + pageName + "?alt=json&key=AIzaSyBQFLsqxyUdYFL5LU64HbFOmD0MjX3hpTg&callback=parseGameProgrammingProjects";
        return baseUrl;
    }

    public string token;

    public async Task GetFile(string fileReference, string fileName, string destinationPath)
    {
        if (token == null || token == "")
            await GetToken();
        string requestURL = API_URL + fileReference + "?alt=media";
        destinationPath = destinationPath + "/Packages/" + fileName + ".unitypackage";
        Debug.Log("Attempting to get file...");
        using (UnityWebRequest webRequest = new UnityWebRequest(requestURL))
        {
            webRequest.method = UnityWebRequest.kHttpVerbGET;

            DownloadHandlerFile downloadHandlerFile = new DownloadHandlerFile(destinationPath);
            webRequest.downloadHandler = downloadHandlerFile;
            webRequest.SetRequestHeader("Authorization", "Bearer " + token);
            webRequest.SendWebRequest();

            while (!webRequest.isDone || !downloadHandlerFile.isDone)
            {
                await Task.Yield();
            }


            if (webRequest.result != UnityWebRequest.Result.Success)
            {
                Debug.Log("Failed:" + webRequest.error);

            }
            else
            {
                Debug.Log("Completed Download to:" + destinationPath);
            }
        }

    }

    public async Task GetFile(string fileReference, string destinationPath)
    {
        if(token==null || token == "")
            await GetToken();
        string requestURL = API_URL + fileReference + "?alt=media";
        destinationPath = destinationPath + "/Packages/" + fileReference + ".unitypackage";
        Debug.Log("Attempting to get file...");
        using (UnityWebRequest webRequest = new UnityWebRequest(requestURL))
        {
            webRequest.method = UnityWebRequest.kHttpVerbGET;

            DownloadHandlerFile downloadHandlerFile = new DownloadHandlerFile(destinationPath);
            webRequest.downloadHandler = downloadHandlerFile;
            webRequest.SetRequestHeader("Authorization", "Bearer " + token);
            webRequest.SendWebRequest();

            while (!webRequest.isDone || !downloadHandlerFile.isDone)
            {
                await Task.Yield();
            }


            if (webRequest.result != UnityWebRequest.Result.Success)
            {
                Debug.Log("Failed:" + webRequest.error);

            }
            else
            {
                Debug.Log("Completed Download to:" + destinationPath);
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


    public async Task<GoogleSheetsJSONWrapper> GetDocumentJSON(string documentID, string pageName)
    {
        using (UnityWebRequest webRequest = UnityWebRequest.Get(GetJSONURL(documentID, pageName)))
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
                return CleanDocumentJSON(jsonResult);
            }
        }
    }

    public GoogleSheetsJSONWrapper CleanDocumentJSON(string jsonString)
    {
        //The php file adds a little bit on the front of the resul. Remove it
        jsonString = jsonString.Substring(jsonString.IndexOf("Projects(") + "Projects(".Length);

        //Remove the last two characters
        jsonString = jsonString.Substring(0, jsonString.Length - 2);

        //Turn it into our wrapper class
        return new GoogleSheetsJSONWrapper(JsonConvert.DeserializeObject<JSONData>(jsonString).values);
    }
}


[Serializable]
public class JSONData
{
    public string[][] values;
    public string majorDimension;
    public string range;
}