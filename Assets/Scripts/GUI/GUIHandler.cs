using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Linq;

/// <summary>
/// This class handles all the operations that one can perform on the GUI.
/// </summary>
public class GUIHandler : MonoBehaviour {

	public GameObject[] GUIs; 
	public Button confirmDatabaseSelection;
    public Button transitionButton;
	public Button MenuButton;
	public Button SwitchButton;
	public Button SourceButton;
	public Button DestButton;
	public Button DefaultButton;

	public string parentXmlAttribute;
	public string[] childrenXmlAttributes;
	public Text databaseFileName;
	public bool calledConnectionInfo;
	public InputField inputField, categoryInputField;
	public bool isOnArticle;
	public string mostRecentInput = "";

	private XmlLoader xmlLoader;
	private SplineDecorator refToSD;
	private bool SearchEnabled = false;

	void Awake()
	{
		xmlLoader = new XmlLoader();
		refToSD = GameObject.FindGameObjectWithTag ("Menu").GetComponent<SplineDecorator>();
	}

	/*Remove the start method and enable the XML chooser GUI if you want to use the GUI.*/
	void Start() {
		xmlLoader.ReadFile ("dblp.xml", parentXmlAttribute, childrenXmlAttributes);
	}

	void Update () {
		confirmDatabaseSelection.onClick.RemoveAllListeners();
        //confirmDatabaseSelection.onClick.AddListener(delegate { XmlScriptCall(); });
        transitionButton.onClick.AddListener(delegate { TransitionToView(); });
    }

    public void EnableAlgorithm()
    {
        mostRecentInput = inputField.text;
        SearchEnabled = true;

        if (inputField.text != null && inputField.text != "")
        {
            HighlightConnectionsbyCoauthor(inputField.text);
        }
        else
        {
            HighlightConnectionsbyCoauthor();
        }
    }

	private void XmlScriptCall()
	{
		xmlLoader.ReadFile(databaseFileName.text + ".xml", parentXmlAttribute, childrenXmlAttributes);
		SwitchGUIDisplay();
	}

    private void TransitionToView()
    {
        SwitchGUIDisplay();
    }

    /// <summary>
    /// Set up the switch from the DBLP GUI to the article GUI. If there ends up being more than
    /// 3 GUIs, we can make a more robust system.
    /// </summary>
    private void SwitchGUIDisplay()
	{
		GUIs [0].GetComponent<Canvas> ().worldCamera = null;
		GUIs [0].gameObject.SetActive(false);
        GUIs [1].GetComponent<Canvas>().worldCamera = null;
        GUIs [1].gameObject.SetActive(false);
        GUIs [2].gameObject.SetActive(true);
		GUIs [2].GetComponent<Canvas> ().worldCamera = GameObject.Find ("Controller UI Camera").GetComponent<Camera>();
		//ActiveUI = GUIs [1];
	}

	public void SetTitle(string title) {
		inputField.text = title;
	}

    public void HighlightConnectionsbyCoauthor()
    {
        //Debug.Log("Calling connections with no author");
        refToSD.GetTopCoauthors();
        List<string> uniqueGeneralAuthors = refToSD.GeneralAuthors.Distinct().ToList();
        UpdateButtons(uniqueGeneralAuthors);

        refToSD.HighlightAuthors(uniqueGeneralAuthors);

        calledConnectionInfo = true;
    }

    public void HighlightConnectionsbyCoauthor(string inputtedAuthor)
    {
        //Debug.Log("Calling connections with a parameter author.");
        refToSD.GetTopCoauthors(inputtedAuthor);

        List<string> uniqueGeneralAuthors = refToSD.GeneralAuthors.Distinct().ToList();
        UpdateButtons(uniqueGeneralAuthors);

        refToSD.HighlightAuthors(uniqueGeneralAuthors);

        calledConnectionInfo = true;
    }

    private void UpdateButtons(List<string> uniqueGeneralAuthors)
    {
        DestroyButtons();

        int buttonCount = 0;

        int breakPoint = uniqueGeneralAuthors.Count;
        Transform gridImage = GameObject.FindGameObjectWithTag("Grid Image").transform;
        for (int i = 0; i < breakPoint; i++)
        {
            buttonCount++;
            Button button;

            button = Instantiate(DefaultButton);
            button.transform.GetChild(0).GetComponent<Text>().text = uniqueGeneralAuthors[i];
            /* This will retain local orientation and scale rather than world orientation and scale, which can prevent
				 * common UI scaling issues.*/
            button.transform.SetParent(gridImage, false); //originally, this was button.transform.parent = gridImage;
            button.transform.localScale = Vector3.one;
            button.transform.name = "Button (" + buttonCount + ")";
            GameObject.Find("MasterInputHandler").GetComponent<DataVisInputs>().ButtonCount = buttonCount;
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
    }

    public void SwitchUI(Text txt) {
		if (isOnArticle) {
			isOnArticle = false;
			txt.text = "Articles by Category";
            GUIs[2].transform.FindChild("InputField Category").gameObject.SetActive(false);
            GUIs[2].transform.FindChild("Reset Button").gameObject.SetActive(true);
            //Reset the line renderer so connections aren't drawn again
            LineRenderer lineRend = refToSD.gameObject.GetComponent<LineRenderer>();
			if (lineRend != null)
				lineRend.SetVertexCount (0);
            refToSD.ReverseAuthors();
			refToSD.Clean ();
		} else {
			isOnArticle = true;
			txt.text = "Authors";
            GUIs[2].transform.FindChild("InputField Category").gameObject.SetActive(true);
            GUIs[2].transform.FindChild("Reset Button").gameObject.SetActive(false);
            inputField.text = mostRecentInput;

            if (SearchEnabled && inputField.text != null && inputField.text != "")
            {
                HighlightConnectionsbyCoauthor(inputField.text);
            }
            else if (SearchEnabled)
            {
                HighlightConnectionsbyCoauthor();
            }
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
