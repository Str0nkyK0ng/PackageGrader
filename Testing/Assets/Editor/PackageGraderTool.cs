using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UIElements;
public class PackageGraderTool : EditorWindow
{


    string PACKAGE_PATH = "Assets/UnimportedPackages";

    private string currentlySelectedPackage;
    private string packageFolderPath;
    string[] KNOWN_FOLDERS = { "Assets/Scenes", "Assets/UnimportedPackages", "Assets/Editor"};

    private VisualElement m_RightPane;
    private ListView m_leftPane;
    private TextElement m_RightHeader, m_packagePathHeader;
    [MenuItem("Window/PackageGraderTool")]
    public static void ShowExample()
    {
        PackageGraderTool wnd = GetWindow<PackageGraderTool>();
        wnd.titleContent = new GUIContent("Grader Window");
    }


    private Button deleteButton, importButton, openSceneButton;
    public IntegerField sceneIndex;


    public GoogleDriveManager manager;
    public void Awake()
    {
        Debug.Log("Awake");

    }

    public void SelectFolder()
    {
        //Ask which folder to look at for the .unityPackage files
        PACKAGE_PATH = EditorUtility.OpenFolderPanel("Select the folder with the .unitypackages", "", "");


        Debug.Log("PACKAGE_PATH:" + PACKAGE_PATH);
        //Add the package path to the known folders
        KNOWN_FOLDERS[1] = PACKAGE_PATH;
        PopulateLeftPane();

    }
    public void CreateGUI()
    {

        m_packagePathHeader = new TextElement();
        m_packagePathHeader.style.unityTextAlign = TextAnchor.MiddleCenter;
        m_packagePathHeader.style.unityTextOutlineColor = Color.green;
        m_packagePathHeader.style.unityTextOutlineWidth = 2;

        m_packagePathHeader.text = PACKAGE_PATH;


        Button pullItem = new Button();
        pullItem.text = "Download test";
        pullItem.clicked += delegate {
            string[] fileIDs =
            {
                "1w108wHLByuvutZOtlSAY9ZpCdhIXy9VV","1wjt-JwnamAeHhd9jL8sSUaZruG5c8k8O","1hsWUVULJV8Kk8dgqmthiMoVYpt6QKysa",
            };
            DownloadFiles(fileIDs);
        };
        rootVisualElement.Add(pullItem);


        Button selectFolderButton = new Button();
        selectFolderButton.text = "Select Folder";
        selectFolderButton.clicked += SelectFolder;
        rootVisualElement.Add(m_packagePathHeader);
        rootVisualElement.Add(selectFolderButton);

        //Which scene should we load
        sceneIndex = new IntegerField();
        sceneIndex.label = "What scene # should we load:";
        sceneIndex.value = 0;
        rootVisualElement.Add(sceneIndex);

        // Create a two-pane view with the left pane being fixed with
        var splitView = new TwoPaneSplitView(0, 250, TwoPaneSplitViewOrientation.Horizontal);

        // Add the view to the visual tree by adding it as a child to the root element
        rootVisualElement.Add(splitView);

        // A TwoPaneSplitView always needs exactly two child elements
        m_leftPane = new ListView();
        splitView.Add(m_leftPane);
        m_leftPane.fixedItemHeight = 35;


        //The right panel is all the controls
        m_RightPane = new VisualElement();
        splitView.Add(m_RightPane);

        importButton = new Button();
        importButton.clicked += ImportPackage;
        importButton.text = "Import Package";


        deleteButton = new Button();
        deleteButton.clicked += DeletePackage;
        deleteButton.text = "Clear Project";

        openSceneButton = new Button();
        openSceneButton.clicked += OpenSceneInPackage;
        openSceneButton.text = "Open nth Scene in Package";


        m_RightHeader = new TextElement();
        m_RightHeader.style.unityTextAlign = TextAnchor.MiddleCenter;


        m_RightPane.Add(m_RightHeader);


        //Add the three buttons
        m_RightPane.Add(importButton);
        m_RightPane.Add(openSceneButton);
        m_RightPane.Add(deleteButton);



        PopulateLeftPane();



    }
    private void PopulateLeftPane()
    {
        //Update the path header
        m_packagePathHeader.style.unityTextAlign = TextAnchor.MiddleCenter;
        m_packagePathHeader.text = PACKAGE_PATH;

        string[] packages = GetPackages();

        m_leftPane.Clear();

        if (packages.Length == 0)
        {
            Repaint();
            return;
        }

        //The left plane will have all the packages
        m_leftPane.makeItem = () => new Label();
        m_leftPane.bindItem = (item, index) => {
            (item as Label).text = packages[index].Substring(PACKAGE_PATH.Length);
        };
        m_leftPane.itemsSource = packages;
        m_leftPane.selectionChanged += OnPackageSelection;


        Repaint();
    }
    private void OnPackageSelection(IEnumerable<object> selectedItems)
    {
        // Get the selected folder
        currentlySelectedPackage = selectedItems.First() as string;
        m_RightHeader.text = currentlySelectedPackage;
        packageFolderPath = null;
    }
    
 
    public string[] GetPackages()
    {
        if(PACKAGE_PATH == null || PACKAGE_PATH == string.Empty)
        {
            Debug.LogError("No path was specified.");
            return null;
        }
        //Read from the UnimportedPackages folder
        string[] packages = Directory.GetFiles(PACKAGE_PATH, "*.unitypackage", SearchOption.TopDirectoryOnly);
        if (packages.Length == 0) {
            Debug.LogError("No Packages Found in Folder");
        }
        return packages;
        
    }


    private void ImportPackage()
    {
        AssetDatabase.ImportPackage(currentlySelectedPackage, false);
        // Refresh the AssetDatabase after all the changes
        AssetDatabase.Refresh();
    }

    private void DeletePackage()
    {

        DirectoryInfo d = new DirectoryInfo("Assets");

        foreach (var file in d.GetDirectories("*"))
        {
            string name = file.FullName;
            Debug.Log("Considering File:" + name);

            if (!name.Contains("Editor") && !name.Contains("UnimportedPackages"))
            {
                Debug.Log("Deleting:" + name);
                FileUtil.DeleteFileOrDirectory(file.FullName);
                FileUtil.DeleteFileOrDirectory(file.FullName + ".meta");
            }
            AssetDatabase.Refresh();

        }

        foreach (var file in d.GetFiles("*"))
        {
            string name = file.FullName;
            Debug.Log("Considering File:" + name);

            if (!name.Contains("Editor") && !name.Contains("UnimportedPackages")) {
                Debug.Log("Deleting:"+name);
                FileUtil.DeleteFileOrDirectory(file.FullName);
            }
            AssetDatabase.Refresh();

        }


    }

    private void FindPackageDirectory()
    {

        var folders = AssetDatabase.GetSubFolders("Assets");
        foreach (var folder in folders)
        {
            if (!KNOWN_FOLDERS.Contains(folder)) { 
                packageFolderPath = folder;
                Debug.Log("Found the package folder path:"+packageFolderPath);
                return;
            }
        }
        
        Debug.LogError("Cannot get package path, are you sure the package has been imported?");
        packageFolderPath = null;

    }

    private void OpenSceneInPackage()
    {
        FindPackageDirectory();
        var texturePackageNames = Directory.GetFiles(packageFolderPath, "*.unity", SearchOption.AllDirectories);

        //Open the first scene
        if (texturePackageNames.Length > 0)
        {
            EditorSceneManager.OpenScene(texturePackageNames[sceneIndex.value]);
        }
        else
        {
            Debug.LogError("No scenes found in folder " + currentlySelectedPackage);
        }

    }

    public async void DownloadFile(string fileID) {
        manager = new GoogleDriveManager();
        EditorUtility.DisplayProgressBar("Downloading file id:"+fileID, "...", .5f);
        await manager.GetFile(fileID, Application.dataPath);
        EditorUtility.ClearProgressBar();
    }

    public async void DownloadFiles(string[] fileIDs)
    {
        //Batch together our asset editing so unity doesn't import after each one
        AssetDatabase.StartAssetEditing();

        manager = new GoogleDriveManager();

        for (int x = 0; x < fileIDs.Length; x++)
        {
            EditorUtility.DisplayProgressBar("Downloading Files", "Current File Id:"+fileIDs[x], (x*1.0f)/(fileIDs.Length*1.0f));
            await manager.GetFile(fileIDs[x], Application.dataPath);
        }
        EditorUtility.ClearProgressBar();

        //Stop asset editing
        AssetDatabase.StopAssetEditing();


    }
}
