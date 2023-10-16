using UnityEngine;
using UnityEditor;
using System;
using UnityEngine.Networking;
using System.Collections.Generic;

//This class is meant to make searching the JSON data in google sheets easier
//Unity's native JSON functions are quite awful, even with an additional library
//This class is meant to make that easier

public class GoogleSheetsJSONWrapper
{
    public string[][] rawData;


    public string[] headers;

    public GoogleSheetsJSONWrapper(string[][] data)
    {
        rawData = data;
        initHeaders();
    }

    public void initHeaders()
    {
        headers = rawData[0];

    }
    public int IndexOfHeader(string header)
    {
        for (int x = 0; x < headers.Length; x++)
        {
            if (headers[x] == header)
                return x;
        }
        throw new Exception("String \"" + header + "\" does not exist");
    }

    public List<string[]> FindAllEntries(string header, string searchValue)
    {
        return FindAllEntries(IndexOfHeader(header), searchValue);
    }

    public List<string[]> FindAllEntries(int headerIndex, string searchValue)
    {
        List<string[]> matches = new List<string[]>();
        foreach (string[] data in rawData)
        {
            if (data[headerIndex] == searchValue)
            {
                matches.Add(data);
            }
        }
        return matches;
    }

    public List<string> GetDownloadIDs(int headerIndex, List<string[]> values)
    {
        List<string> ids = new List<string>();
        foreach(string[] v in values)
        {
            ids.Add(v[headerIndex]);
        }
        return ids;
    }

    public List<string> GetDownloadIDs(string header, List<string[]> values)
    {
        int headerIndex = IndexOfHeader(header);
        List<string> ids = new List<string>();
        foreach (string[] v in values)
        {
            ids.Add(v[headerIndex]);
        }
        return ids;
    }


}
