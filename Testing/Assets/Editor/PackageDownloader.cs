using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;



public class PackageDownloader: EditorWindow
{


    [MenuItem("Window/PackageDownloader")]
    public static void ShowExample()
    {
        PackageDownloader wnd = GetWindow<PackageDownloader>();
        wnd.titleContent = new GUIContent("Downloader Window");
    }


    public GoogleDriveManager manager;
    public GoogleSheetsJSONWrapper sheetsData;
    public DropdownField headerSelectDropdown;
    public TextField sheetInformation, pageInformation, headerKey;
    public List<string> googleDocHeaders;

    public void Awake()
    {
        filters = new List<(DropdownField, TextField)>();
    }
    private void OnGUI()
    {
        EditorPrefs.SetString("DocumentID", sheetInformation.value);
        EditorPrefs.SetString("PageName", pageInformation.value);
    }


    public List<(DropdownField, TextField)> filters;
    public Foldout group;

    public void RemoveFilter()
    {
        if (filters.Count > 0)
        {
            (DropdownField, TextField) oldest = filters[filters.Count - 1];
            group.Remove(oldest.Item1);
            group.Remove(oldest.Item2);
            filters.Remove(oldest);
        }
    }
    public void AddNewFilter()
    {
        DropdownField dropdown = new DropdownField();
        dropdown.label = "Row Name:";
        group.Add(dropdown);
        if (googleDocHeaders != null)
        {
            dropdown.choices = googleDocHeaders;
        }
        TextField value = new TextField();
        value.label = "Row Value:";
        group.Add(value);

        if (filters == null)
            filters = new List<(DropdownField, TextField)>();

        filters.Add(new(dropdown, value));
    }
    public void CreateGUI()
    {
        sheetInformation = new TextField();
        sheetInformation.label = "Document Id:";
        sheetInformation.value = EditorPrefs.GetString("DocumentID");
        rootVisualElement.Add(sheetInformation);

        pageInformation = new TextField();
        pageInformation.label = "Page Name:";
        pageInformation.value = EditorPrefs.GetString("PageName");

        rootVisualElement.Add(pageInformation);

        Button getData = new Button();
        getData.text = "Get Sheet Data";
        getData.clicked += DownloadJSON;
        rootVisualElement.Add(getData);




        //Once we have the json, apply filters

        group = new Foldout();
        group.text = "Filters";
        group.Add(headerKey);   


        Button newFilter = new Button();
        newFilter.text = "New Filter";
        newFilter.clicked += AddNewFilter;
        group.Add(newFilter);

        Button removeFilter = new Button();
        removeFilter.text = "Remove Filter";
        removeFilter.clicked += RemoveFilter;
        group.Add(removeFilter);

        rootVisualElement.Add(group);




        Button filterButton = new Button();
        filterButton.text = "Apply Filter";
        filterButton.clicked += FilterJSONEntries;
        rootVisualElement.Add(filterButton);



        //What should we call the file
        nameingDropwdown = new DropdownField();
        nameingDropwdown.label = "What should we name the package?:";
        rootVisualElement.Add(nameingDropwdown);
        if (googleDocHeaders != null)
        {
            nameingDropwdown.choices = googleDocHeaders;
        }
        //Now finally, download
        Button downloadButton = new Button();
        downloadButton.text = "Download Matches";
        downloadButton.clicked += DownloadFromMatches;
        rootVisualElement.Add(downloadButton);

        
    }
    DropdownField nameingDropwdown;
    public async void DownloadJSON()
    {

        if (manager == null)
        {
            manager = new GoogleDriveManager();
        }
        sheetsData = await manager.GetDocumentJSON(sheetInformation.value,pageInformation.value);
        googleDocHeaders = sheetsData.headers.ToList();

        if (filters == null)
            filters = new List<(DropdownField, TextField)>();


        foreach ((DropdownField,TextField) filter in filters)
        {
            Debug.Log("Adding it to a new one!");
            filter.Item1.choices = sheetsData.headers.ToList();
        }
        nameingDropwdown.choices = sheetsData.headers.ToList();
    }


    public List<string[]> matches;
    public Label matchLabel;
    public void FilterJSONEntries()
    {
        List<string> headers = new List<string>();
        List<string> values = new List<string>();

        foreach ((DropdownField,TextField) filter in filters)
        {
            headers.Add(filter.Item1.value);
            values.Add(filter.Item2.value);
        }
        matches = sheetsData.CompoundFilter(headers.ToArray(), values.ToArray());

        //Double check / purge any blank entries
        for (int x = 0; x < matches.Count; x++)
        {
            matches[x] = matches[x].Where(s => !string.IsNullOrEmpty(s)).ToArray();
        }


        //now lets pring them for my own sake
        for (int x = 0; x < matches.Count; x++)
        {
            string info = "";
            for(int y=0;y< matches[x].Length; y++)
            {
                info += "\n[" + y + "]\"" + matches[x][y] + "\"";
            }
            Debug.Log(info);
        }

        if (matchLabel == null) {
            matchLabel = new Label(matches.Count + " matches found");
            rootVisualElement.Add(matchLabel);
        }
        else
            matchLabel.text= matches.Count + " matches found";

    }

    public async void DownloadFromMatches()
    {
        List<(string,string)> ids = sheetsData.GetDownloadIDs("Unitypackage of your entire project including everything listed above", matches,nameingDropwdown.value);

        //Batch together our asset editing so unity doesn't import after each one
        AssetDatabase.StartAssetEditing();

        if (manager == null)
        {
            manager = new GoogleDriveManager();
        }

        try
        {
            for (int x = 0; x < ids.Count; x++)
            {


    
                string fileId = ids[x].Item1;
                Debug.Log("File Id:"+fileId);
                fileId=fileId.Split("=")[1];
                EditorUtility.DisplayProgressBar("Downloading Files", "Current Package:" + ids[x].Item2, (x * 1.0f) / (ids.Count * 1.0f));
                await manager.GetFile(fileId, ids[x].Item2, Application.dataPath);
            }
            EditorUtility.ClearProgressBar();
            AssetDatabase.StopAssetEditing();
        }
        catch
        {
            AssetDatabase.StopAssetEditing();

        }


        //Stop asset editing
    }
    

}
