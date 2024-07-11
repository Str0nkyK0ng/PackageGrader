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
        init();
    }

    public void init()
    {
        headers = rawData[0];

        for(int x = 1; x < rawData.Length; x++)
        {

            List<string> data = new List<string>(rawData[x]);
            int offset = 0;
            for(int y = 0; y < data.Count-offset; y++)
            {
                if (data[y] == "" || data[y]==" ")
                {
                    data.RemoveAt(y);
                    offset++;
                    y--;
                }
            }
            rawData[x] = data.ToArray();
        }
        

    }
    public int IndexOfHeader(string header)
    {
        for (int x = 0; x < headers.Length; x++)
        {
            if (headers[x] == header) {
                Debug.Log(header + " is at index " + x);
                return x;
            }
        }
        throw new Exception("String \"" + header + "\" does not exist");
    }

    public List<string[]> FindAllEntries(string header, string searchValue)
    {
        return FindAllEntries(IndexOfHeader(header), searchValue);
    }

    public List<string[]> CompoundFilter(string[] headers, string[] searchValues)
    {
        List<string[]> matches = new List<string[]>();
        foreach (string[] data in rawData)
        {
            bool invalid = false;
           for(int x=0;x<headers.Length;x++)
            {
                string header = headers[x];
                //Get the header index
                int index = IndexOfHeader(header);
                if (data[index] != searchValues[x])
                {
                    invalid = true;
                    break;
                }
            }

            //If it's valid, add it to our matches
            if (!invalid)
            {
                matches.Add(data);
            }
        }
        return matches;
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

    public List<(string,string)> GetDownloadIDs(string header, List<string[]> values, string addtionalHeader)
    {
        Debug.Log("Getting Download Ideas from:" + header + " with additional header " + addtionalHeader);
        int headerIndex = IndexOfHeader(header);
        int additionalheaderIndex = IndexOfHeader(addtionalHeader);

        List<(string,string)> ids = new List<(string,string)>();
        foreach (string[] v in values)
        {
            (string, string) newData = (v[headerIndex], v[additionalheaderIndex]);
            Debug.Log("Info:" + v[headerIndex] + " and " + v[additionalheaderIndex]);
            ids.Add(newData);
        }
        return ids;
    }


}
