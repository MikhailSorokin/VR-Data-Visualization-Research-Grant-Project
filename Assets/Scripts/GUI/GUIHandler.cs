using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Linq;

/// <summary>
/// This class handles all the operations that one can perform on the GUI.
/// </summary>
public class GUIHandler : MonoBehaviour {

    [Header ("GUIDisplay", order = 1)]
	public GameObject[] GUIs;
    [Header("Buttons", order = 2)]
    public Button confirmDatabaseSelection;
    public Button transitionButton;
	public Button MenuButton;
	public Button SwitchButton;
	public Button SourceButton;
	public Button DestButton;
	public Button DefaultButton;
    [Header("UseCertainGuis", order = 3)]
    public bool useDatabaseGUI = false;
    [Header("Xml Parsing Information", order = 4)]
    public string parentXmlAttribute;
	public string[] childrenXmlAttributes;
    public string[] movieURLS;
	public Text databaseFileName;
    [Header("Essential Connection Information", order = 5)]
	public bool calledConnectionInfo;
	public InputField inputField, categoryInputField;
	public bool isOnArticle;
	public string mostRecentInput = "";

	private XmlLoader xmlLoader;
	private SplineDecorator refToSD;
    private List<string> allAuthorsInConnections = new List<string>();
    private int individualPageCount = 1;

    //added more colors
    private Color[] colorAssortment = {
        new Color(255f/255f, 175f/255f, 0f, 255f/255f), //Light Orange 
        new Color(255f/255f, 10f/255f, 0f, 255f/255f), //Red 
        new Color(0f/255f, 0f, 255f/255f, 255f/255f), //Dark Olive Green,
        new Color(100f/255f, 149f/255f, 237f/255f, 255f/255f), //Cornflower blue
        new Color(178f/255f, 34f/255f, 34f/255f, 255f/255f), //Firebrick
        new Color(119f/255f, 136f/255f, 153f/255f, 255f/255f), //Light Slate Gray
        new Color(81f/255f, 0f, 255f/255f, 255f/255f),
        new Color(102f/255f, 255f/255f, 255f/255f, 255f/255f),
        new Color(60f/255f, 255f/255f, 102f/255f, 255f/255f),
        new Color(33f/255f, 171f/255f, 205f/255f, 255f/255f),
        new Color(0f, 0.3f, 255f/255f, 255f/255f),
        new Color(1f, 0.2f, 0f, 255f/255f),
        new Color(0f/255f, 255f/255f, 128f/255f, 255f/255f),
        new Color(226f/255f, 182f/255f, 49f/255f, 255f/255f)
    };


    void Awake()
	{
		xmlLoader = new XmlLoader();
		refToSD = GameObject.FindGameObjectWithTag ("Menu").GetComponent<SplineDecorator>();
    }

    /*Remove the start method and enable the XML chooser GUI if you want to use the GUI.*/
    void Start() {
        if (!useDatabaseGUI)
        {
            xmlLoader.ReadFile("dblp.xml", parentXmlAttribute, childrenXmlAttributes); //this should be loaded as default
            TransitionToView();
        } else
        {
            confirmDatabaseSelection.GetComponent<Canvas>().worldCamera = Camera.main;
        }
	}

	void Update () {
		confirmDatabaseSelection.onClick.RemoveAllListeners();
        confirmDatabaseSelection.onClick.AddListener(delegate { XmlScriptCall(); });
        transitionButton.onClick.AddListener(delegate { TransitionToView(); });
    }

    public void EnableAlgorithm()
    {
        mostRecentInput = inputField.text;

        refToSD.CleanConnections();
        if (inputField.text != null && inputField.text != "")
        {
            HighlightConnectionsbyCoauthor(inputField.text);
        }
    }

    /// <summary>
    /// This is called from a button press on the "GUI - Database" Gameobject.
    /// It starts the process of reading from a database, either from a local
    /// folder, or online. 
    /// </summary>
	public void XmlScriptCall()
	{
        Camera cam = confirmDatabaseSelection.GetComponent<Camera>();
        string removedString = "";
        if (useDatabaseGUI)
        {
            for (int urlInd = 0; urlInd < movieURLS.Length; urlInd++) {
                do
                {
                    removedString = movieURLS[urlInd].Remove(movieURLS[urlInd].Length - 1);
                    removedString += individualPageCount;
                    Debug.Log("Printing the current page retrieval: " + individualPageCount + ", from the URL: "
                        + removedString);
					xmlLoader.ReadURL("Movie", removedString, parentXmlAttribute, childrenXmlAttributes);
                    individualPageCount++; //pageCount always starts at one
					removedString = movieURLS[urlInd].Remove(movieURLS[urlInd].Length - 1);
					removedString += individualPageCount;
                } while (xmlLoader.DidntReachEmptyPage(removedString));
				individualPageCount = 0;
           }
        }
		SwitchGUIDisplay ();

	}

    public void TransitionToView()
    {
        SwitchGUIDisplay();
    }

    /// <summary>
    /// Set up the switch from the DBLP GUI to the article GUI. If there ends up being more than
    /// 3 GUIs, we can make a more robust system.
    /// </summary>
    private void SwitchGUIDisplay()
	{

        if (useDatabaseGUI)
        {
            GUIs[0].GetComponent<Canvas>().worldCamera = null;
            GUIs[0].gameObject.SetActive(false);
            GUIs[1].GetComponent<Canvas>().worldCamera = null;
            GUIs[1].gameObject.SetActive(false);
        }
        GUIs [2].gameObject.SetActive(true);
		//TODO: Make the DataVisInputs class static and use the mouseControls boolean variable from there.
		if (!DataVisInputs.usingMouseControls) {
            Debug.Log("Found a controller UI Camera.");
			GUIs [2].GetComponent<Canvas> ().worldCamera = GameObject.Find ("UI Camera").GetComponent<Camera> ();
		} else {
			GUIs [2].GetComponent<Canvas> ().worldCamera = GameObject.Find ("MainCamera").GetComponent<Camera> ();
		}
		//ActiveUI = GUIs [1];
	}

	public void SetTitle(string title) {
		inputField.text = title;
	}

    public void HighlightConnectionsbyCoauthor(string inputtedAuthor)
    {
        allAuthorsInConnections.Clear();

        //Need to get all of the coauthors from the inputted author, who may be located
        //on various gameobjects.
        AuthorData selectedAuthorData = DataProcessor.GetADFromAuthor(inputtedAuthor);
        bool foundAtLeastOneCoAuthor = false;
        int colorIndex = 0;
        allAuthorsInConnections.Add(inputtedAuthor);

        foreach (KeyValuePair<GameObject, List<string>> goToAuthors in selectedAuthorData.goLocations)
        {
            //PrintList(goToAuthors.Key, goToAuthors.Value);
            if (goToAuthors.Value.Count > 0)
            {
                foundAtLeastOneCoAuthor = true;
                AuthorData[] coAuthors = DataProcessor.ConvertListToAD(goToAuthors.Value);
                AddCoauthorsToGUI(coAuthors);

                refToSD.DrawArticleConnections(goToAuthors.Key, coAuthors, colorAssortment[colorIndex % colorAssortment.Length]);
                colorIndex++;
            }
        }

        if (!foundAtLeastOneCoAuthor)
        {
            refToSD.Dim();
        }

        List<string> uniqueCoauthors = allAuthorsInConnections.Distinct().ToList();
        UpdateButtons(uniqueCoauthors, 0);
        Color chosenColor = new Color(255f / 255f, 0f / 255f, 153f / 255f, 255f / 255f); //magenta highlight color
        refToSD.HighlightSourceAuthor(inputtedAuthor, chosenColor);

        calledConnectionInfo = true;
    }

    private void AddCoauthorsToGUI(AuthorData[] coauthors)
    {
        for (int authInd = 0; authInd < coauthors.Length; authInd++)
        {
            if (!allAuthorsInConnections.Contains(coauthors[authInd].Author))
                allAuthorsInConnections.Add(coauthors[authInd].Author);
        }
    }

    private void PrintList(GameObject keyGO, List<string> coauthors)
    {
        Debug.Log("GameObject: " + keyGO.name + "\n");
        Debug.Log("Coauthors: ");
        foreach (string coauthor in coauthors)
        {
            Debug.Log(coauthor + " ");
        }
        Debug.Log("DONE");
    }

    //This is for displaying Article or Author titles on the GUI
    private void UpdateButtons(List<string> information, int type)
    {
        DestroyButtons();

        int buttonCount = 0;
        string finalString = "";
        int breakPoint = information.Count;
        Transform gridImage = GameObject.FindGameObjectWithTag("Grid Image").transform;
        for (int i = 0; i < breakPoint; i++)
        {
            buttonCount++;
            Button button;

            button = Instantiate(DefaultButton);

            //TODO: Instead of Integer, have a enum type which refers to either Authors or Articles
            //Type of 0 corresponds to writing Authors and 1 corresponds to the Articles
            if (type == 0)
            {
                finalString += ((i == 0) ? "Author: " : "Coauthor: ") + information[i];
            } 
            else if (type == 1)
            {
                string buildUp = information[i] + " - ";
                foreach (string author in DataProcessor.articleContainerDictionary[information[i]].Authors)
                {
                    buildUp += author + ", ";
                }
                finalString = buildUp.Trim().Remove(buildUp.Length - 2);
                finalString += ".";
            }

            button.transform.GetChild(0).GetComponent<Text>().text = finalString;
            // This will retain local orientation and scale rather than world orientation and scale, which can prevent
				 //common UI scaling issues.
            button.transform.SetParent(gridImage, false); //originally, this was button.transform.parent = gridImage;
            button.transform.localScale = Vector3.one;
            button.transform.name = "Button (" + buttonCount + ")";
            GameObject.Find("MasterInputHandler").GetComponent<DataVisInputs>().ButtonCount = buttonCount;
            finalString = "";
        }

        //If nothing could be found, notify user
        if (breakPoint == 0)
        {
            Button button;
            button = Instantiate(DefaultButton);
            button.transform.GetChild(0).GetComponent<Text>().text = "Could not find any relationships with the current data. Try loading in more.";
            button.transform.SetParent(gridImage, false); //originally, this was button.transform.parent = gridImage;
            button.transform.localScale = Vector3.one;
            button.transform.name = "Button (" + 1 + ")";
            GameObject.Find("MasterInputHandler").GetComponent<DataVisInputs>().ButtonCount = 1;
        }
    }

    public void OnResetPressed()
    {
        DataVisInputs refToDVI = GameObject.Find("MasterInputHandler").GetComponent<DataVisInputs>();

        refToDVI.ResetSphere();
        refToDVI.ResetRotators();
        refToDVI.ResetGridImage();
        //refToSD.ContractElements();
    }

    public void SwitchUI(Text txt) {
		if (isOnArticle) {
			isOnArticle = false;
			txt.text = "Selected Datapoint Information";
            inputField.gameObject.transform.FindChild("Placeholder").GetComponent<Text>().text
                = "<Title of Datapoint here>";

            //GUIs[2].transform.FindChild("InputField Category").gameObject.SetActive(false);
            GUIs[2].transform.FindChild("Reset Button").gameObject.SetActive(true);
            //Reset the line renderer so connections aren't drawn again
            LineRenderer lineRend = refToSD.gameObject.GetComponent<LineRenderer>();
			if (lineRend != null)
				lineRend.SetVertexCount (0);
            refToSD.ReverseAuthors();
			refToSD.Clean ();
            refToSD.CleanConnections();
            SetTitle("");
            DestroyButtons();
        } else {
			isOnArticle = true;
			txt.text = "Coauthor(s) Relationship (Input Wanted Author)";
            inputField.gameObject.transform.FindChild("Placeholder").GetComponent<Text>().text
             = "<Author name>";
            //GUIs[2].transform.FindChild("InputField Category").gameObject.SetActive(true);
            GUIs[2].transform.FindChild("Reset Button").gameObject.SetActive(true);
            SetTitle("");
            DestroyButtons();
        }
	}

	public void DestroyButtons() {

		Transform gridImage = GameObject.FindGameObjectWithTag ("Grid Image").transform;
		List<GameObject> oldButtons = new List<GameObject> ();
		for (int i = 0; i < gridImage.childCount; i++) {
			oldButtons.Add(gridImage.GetChild (i).gameObject);
		}
		foreach (GameObject obj in oldButtons) {
			Destroy (obj);
		}

	}
}
