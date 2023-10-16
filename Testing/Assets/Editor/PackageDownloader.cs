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


    public void Awake()
    {
        
    }
    private void OnGUI()
    {
        EditorPrefs.SetString("DocumentID", sheetInformation.value);
        EditorPrefs.SetString("PageName", pageInformation.value);

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




        //Once we have the json
        headerSelectDropdown = new DropdownField();
        headerSelectDropdown.label = "Row Name:";
        rootVisualElement.Add(headerSelectDropdown);

        headerKey = new TextField();
        headerKey.label = "Row Value:";
        rootVisualElement.Add(headerKey);

        Button filterButton = new Button();
        filterButton.text = "Filter";
        filterButton.clicked += FilterJSONEntries;
        rootVisualElement.Add(filterButton);


        //Now finally, download
        Button downloadButton = new Button();
        downloadButton.text = "Download Matches";
        downloadButton.clicked += DownloadFromMatches;
        rootVisualElement.Add(downloadButton);

        
    }

    public async void DownloadJSON()
    {

        if (manager == null)
        {
            manager = new GoogleDriveManager();
        }
        sheetsData = await manager.GetDocumentJSON(sheetInformation.value,pageInformation.value);
        headerSelectDropdown.choices = sheetsData.headers.ToList();
    }


    public List<string[]> matches;
    public void FilterJSONEntries()
    {
        matches = sheetsData.FindAllEntries(headerSelectDropdown.value,headerKey.value);

        Label label = new Label(matches.Count + " matches found");
        rootVisualElement.Add(label);
    }

    public async void DownloadFromMatches()
    {
        List<string> ids = sheetsData.GetDownloadIDs("Unitypackage of your entire project including everything listed above", matches);

        //Batch together our asset editing so unity doesn't import after each one
        AssetDatabase.StartAssetEditing();

        if (manager == null)
        {
            manager = new GoogleDriveManager();
        }

        for (int x = 0; x < ids.Count; x++)
        {
            string fileId = ids[x].Split("=")[1];
            EditorUtility.DisplayProgressBar("Downloading Files", "Current File Id:" + fileId, (x * 1.0f) / (ids.Count * 1.0f));
            await manager.GetFile(fileId, Application.dataPath);
        }
        EditorUtility.ClearProgressBar();

        //Stop asset editing
        AssetDatabase.StopAssetEditing();
    }
    

}
