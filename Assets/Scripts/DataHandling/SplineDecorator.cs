using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.UI;
using System.Linq;
using System.Text.RegularExpressions;

//TODO: Connections are drawn, but they need to be updated now
//I KNOW WHY splines aren't updating! Need to add splines to splineConnList 
//as it is empty in the UpdateSplinePos method
public class SplineDecorator : MonoBehaviour
{
    public BezierSpline spline;
    public BezierSpline spline2;
    public BezierSpline spline3;
    public BezierSpline spline4;
    //
    public Vector3[] oPos1;
    public Quaternion[] oRot1;
    public Vector3[] oPos2;
    public Quaternion[] oRot2;
    public Vector3[] oPos3;
    public Quaternion[] oRot3;
    public Vector3[] oPos4;
    public Quaternion[] oRot4;
    public bool canTransition = true;
    public float speed = .6f;
    public Transform[] rotators;
    //
    public int frequency;
    public int density;
    public string title;

    [System.Serializable]
    public enum DatasetCategory
    {
        Categories,
        Decades,
        Years,
        Conferences,
        Authors,
        Singleton
    }

    public DatasetCategory datasetCategory;
    public Transform[] items;
    public Transform[] elements;
    public Transform[] lineGuide;
    public static GUIHandler masterGUIHandler;
    public bool expanded = false;
    public List<string> DataSetStrings = new List<string>();

    private bool isMoving;
    private Vector3 startLoc;
    private Vector3 endLoc;
    private string[] categories = {
        "Uncategorized", "VR", "A.I.", "Robotics", "Graphics", "Algorithms", "BioInformatics",
        "Data Visualization", "Numerical Analysis", "Scientific Computations", "Programming Languages",
        "Game Development", "Cybersecurity", "Machine Learning", "Networking", "Databases", "Human Computer Interaction"
    };
    private static Color[] colorCategory =
    {
        Color.white, Color.green, Color.magenta, Color.red, Color.blue, Color.yellow, Color.cyan,
        new Color(255f/255f, 175f/255f, 0f, 255f/255f), new Color(255f/255f, 10f/255f, 0f, 255f/255f), new Color(81f/255f, 0f, 255f/255f, 255f/255f), new Color(102f/255f, 255f/255f, 255f/255f, 255f/255f),
        new Color(60f/255f, 255f/255f, 102f/255f, 255f/255f), new Color(33f/255f, 171f/255f, 205f/255f, 255f/255f), new Color(0f, 0.3f, 255f/255f, 255f/255f), new Color(1f, 0.2f, 0f, 255f/255f),
        new Color(0f/255f, 255f/255f, 128f/255f, 255f/255f), new Color(226f/255f, 182f/255f, 49f/255f, 255f/255f)
    };
    private bool propertiesSet = false;
    public bool dimmed = true;
    private int isSphere = 0;
    private bool doOnce = true;
    private bool docking = false;
    private bool dataAdded = false;
    private List<MasterNode> loadReady = null;
    private Transform cube;
    public Color textColor = Color.white;

    //Connection Constant Information
    private const float CONN_SCALE = 0.033f;
    private const int NUM_CONNOBJS = 32;

    Dictionary<char, int> alphabetDict = new Dictionary<char, int>();

    // connector vars
    public Transform connector;
    public List<Transform> connectorSplines;
    public Transform[] connectGuide;
    private Dictionary<int, SplineDecorator> diskSplineDictionary;
    private Dictionary<int[,], bool> connectorIds;
    public Dictionary<string, List<string>> articleAuthors;
    public Dictionary<string, int> articleYears;

    // lookup variables from data points
    public Dictionary<Transform, string> dataLookups;
    public List<BezierSpline> connSplineList = new List<BezierSpline>();
    private string originalLabel = "";

    void Start()
    {
        if (masterGUIHandler != null)
            masterGUIHandler = FindObjectOfType<GUIHandler>();

        char let = 'a';

        for (int i = 1; i < 27; i++)
        {
            alphabetDict.Add(let, i);
            let++;
        }
    }

    private void Awake()
    {
        masterGUIHandler = FindObjectOfType<GUIHandler>();
        //Dim();
        spline = GetComponent<BezierSpline>();
        if (expanded)
        {
            elements = new Transform[frequency];
            Load();
        }
        //startScale = transform.localScale;
        //endScale = Vector3.one * .5f;

    }

    void Update()
    {
        if (datasetCategory != DatasetCategory.Decades && cube != null)
        {
            Vector3 pA = transform.position;
            Vector3 pB = transform.parent.TransformPoint(startLoc);

            float dist = Vector3.Distance(pA, pB);

            cube.localScale = new Vector3(.01f, .01f, dist / transform.parent.lossyScale.x);/// transform.lossyScale.x);

            Vector3 midPoint = pA + (pB - pA) / 2f;

            cube.position = midPoint;
            cube.LookAt(pA);
        }
    }


    private void Load()
    {
        rotators = new Transform[frequency];
        oPos1 = new Vector3[frequency];
        oPos2 = new Vector3[frequency];
        oRot1 = new Quaternion[frequency];
        oRot2 = new Quaternion[frequency];
        oPos3 = new Vector3[frequency];
        oRot3 = new Quaternion[frequency];
        oPos4 = new Vector3[frequency];
        oRot4 = new Quaternion[frequency];

        if (items.Length == 0) { return; }
        float stepSize = 1f / (density);

        for (int p = 0, f = 0; f < density; f++)
        {
            for (int i = 0; i < lineGuide.Length; i++, p++)
            {
                Transform item = Instantiate(lineGuide[i]) as Transform;
                Vector3 position = spline.GetPoint(p * stepSize);
                item.localPosition = position;
                item.LookAt(position + spline.GetDirection(p * stepSize));
                item.parent = transform;
                rotators[f] = item;
            }
        }

        stepSize = 1f / frequency;

        for (int f = 0; f < frequency; f++)
        {
            Vector3 position = spline.GetPoint((float)f * stepSize);
            oRot1[f] = Quaternion.identity;
            //add to rotators array to allow menu to control rotation locally

            oPos1[f] = position;

        }
        if (spline2)
        {
            for (int f = 0; f < frequency; f++)
            {

                Vector3 position = transform.InverseTransformPoint(spline2.GetPoint((float)f * stepSize));
                oRot2[f] = Quaternion.identity;
                //add to rotators array to allow menu to control rotation locally

                oPos2[f] = position;

                /*

                oRot2 [f] = Quaternion.identity;
				//add to rotators array to allow menu to control rotation locally

				oPos2 [f] = new Vector3(0f, 0f, (float) f * .1f);
                */

            }
        }
        if (spline3)
        {
            for (int f = 0; f < frequency; f++)
            {
                Vector3 position = transform.InverseTransformPoint(spline3.GetPoint((float)f * stepSize));
                oRot3[f] = new Quaternion(0f, 270f, 270f, 0f);
                oPos3[f] = position;
            }
        }
    }

    private void AddInDataHelper(MasterNode node,string titleIn, int i)
    {
        elements[i] = Instantiate(items[0]) as Transform;
        SplineDecorator sp = elements[i].GetComponent<SplineDecorator>();
        
        float stepSize = 1f / (frequency);
        Vector3 position = spline.GetPoint((float)i * stepSize);

        elements[i].localPosition = position;
        elements[i].LookAt(position + spline.GetDirection((float)i * stepSize));
        elements[i].parent = transform;

        elements[i].localScale = transform.localScale;
        elements[i].localScale = transform.localScale * .04f;
        if (!propertiesSet)
            DataSetStrings.Add(node.Title);
        sp.AddInData(node);

        TextMesh[] allTextMeshes = elements[i].GetComponentsInChildren<TextMesh>();

        foreach (TextMesh textMesh in allTextMeshes)
        {
            textMesh.text = titleIn;
            textMesh.color = new Color(textColor.r, textColor.g, textColor.b, 0.75f);
            sp.textColor = textColor;
            //sp.cube.GetComponent<Renderer>().material.SetColor("_TintColor", new Color(textColor.r, textColor.g, textColor.b, 0.75f));
        }
    }

    public void AddInData(MasterNode node)
    {
        int yr = node.Year;

        if (expanded)
        {
            if (datasetCategory == DatasetCategory.Decades)
            {
                int startDecade = 1960;
                
                for (int i = 0; i < elements.Length; i++, startDecade += 10)
                {
                    if (elements[i] == null)
                    {
                        if (startDecade == yr - (yr % 10))
                        {
                            string addString = title + ": " + (startDecade).ToString() + " - " + (startDecade + 9).ToString();

                            AddInDataHelper(node, addString, i);
                        }
                        else
                        {
                            continue;
                        }
                    }
                    else if (elements[i].GetComponent<SplineDecorator>().title.CompareTo((yr - (yr % 10)).ToString()) == 0)
                    {
                        if (!propertiesSet)
                            DataSetStrings.Add(node.Title);

                        SplineDecorator sp = elements[i].GetComponent<SplineDecorator>();
                        sp.AddInData(node);
                    }
                }
                
            }
            else if (datasetCategory == DatasetCategory.Years)
            {
                int yearID = yr % 10;
                for (int i = 0; i < elements.Length; i++)
                {
                    if (yearID == i)
                    {
                        if (elements[i] == null)
                        {
                            elements[i] = Instantiate(items[0]) as Transform;
                            string dec = "";

                            if (transform.GetComponent<SplineDecorator>())
                            {
                                dec = yr.ToString();
                                //dec = transform.GetComponent<SplineDecorator>().title.Remove(3) + i.ToString();
                            }
                            elements[i].GetComponent<SplineDecorator>().title = dec;

                            float stepSize = 1f / (frequency);
                            Vector3 position = spline.GetPoint((float)i * stepSize);

                            elements[i].localPosition = position;
                            elements[i].LookAt(position + spline.GetDirection((float)i * stepSize));
                            elements[i].parent = transform;

                            elements[i].localScale = Vector3.one * .02f;
                            SetGameobject(ref node, i, 1);
                            SplineDecorator sp = elements[i].GetComponent<SplineDecorator>();
                            sp.textColor = textColor;
                            if (!propertiesSet)
                                DataSetStrings.Add(node.Title);
                            sp.AddInData(node);

                            TextMesh[] allTextMeshes = elements[i].GetComponentsInChildren<TextMesh>();

                            foreach (TextMesh textMesh in allTextMeshes)
                            {
                                textMesh.text = (yr).ToString();
                            }
                        }
                        else
                        {
                            if (!propertiesSet)
                                DataSetStrings.Add(node.Title);
                            SetGameobject(ref node, i, 1);
                            SplineDecorator sp = elements[i].GetComponent<SplineDecorator>();
                            sp.AddInData(node);
                        }
                    }
                }
                
            }
            //Author datapoint version
            else if (datasetCategory == DatasetCategory.Authors)
            {
                if (DataSetStrings.Count > 0)
                {
                    int r;
                    
                    for (int titleInd = 0; titleInd < node.Authors.Count; titleInd++)
                    {
                        //This gets the first letter of the last name, and
                        //then disperses it among its respective datapoint in the article.
                        string[] fullName = node.Authors[titleInd].ToLower().Split(new System.Char[]{' '});
                        r = fullName[fullName.Length - 1][0] - 'a' + 1;

                        if (r < 0)
                            r = 0;
                        if (r > 25)
                            r = 27;

                        
                        if (elements[r] == null)
                        {
                            string addString = node.Authors[titleInd];
                                    
                            AddInDataHelper(node, addString, r);
                            SetGameobject(ref node, r, 2);
                        }
                        else
                        {
                            if (!propertiesSet)
                                DataSetStrings.Add(node.Authors[titleInd]);
                            SetGameobject(ref node, r, 2);
                            SplineDecorator sp = elements[r].GetComponent<SplineDecorator>();
                            sp.AddInData(node);
                        }
                         

                    }
                    //AddInAuthors();
                }
            }

            else if (datasetCategory == DatasetCategory.Categories)
            {
				Debug.Log ("Title: " + node.Title + ", Year: " + yr);
                string cat = node.Category;
				Debug.Log (cat);
                int colorID = 0;
                
                //Can we switch "categories" to a dictionary?
                for (int i = 0; i < categories.Length; i++)
                {
                    if (categories[i] == cat)
                    {
                        colorID = i;
                        break;
                    }
                }
                Color colorToSetText = colorCategory[colorID];

                for (int i = 0; i < elements.Length; i++)
                {
                    if (elements[i] == null)
                    {
                        elements[i] = Instantiate(items[0]) as Transform;
                        
                        SplineDecorator sp = elements[i].GetComponent<SplineDecorator>();
                        
                        sp.title = cat;

                        if (elements[i].FindChild("Ring_01"))
                        {
                            Transform child = elements[i].FindChild("Ring_01");
                            Material mat = new Material(child.GetComponent<Renderer>().material);
                            mat.color = new Color(0.0f, 0.3f, 1.0f, 0.1f);
                            elements[i].FindChild("Ring_01").GetComponent<Renderer>().material = mat;

                        }
                        float stepSize = 1f / (frequency);
                        Vector3 position = spline.GetPoint((float)i * stepSize);

                        elements[i].localPosition = position;
                        elements[i].LookAt(position + spline.GetDirection((float)i * stepSize));
                        elements[i].parent = transform;
                        elements[i].localScale = Vector3.one * .02f;
                        SetGameobject(ref node, i, 0);

                        oPos1[i] = elements[i].localPosition;
                        oRot1[i] = elements[i].rotation;
                        
                        TextMesh[] allTextMeshes = elements[i].GetComponentsInChildren<TextMesh>();

                        foreach (TextMesh textMesh in allTextMeshes)
                        {
                            textMesh.color = new Color(colorToSetText.r, colorToSetText.g, colorToSetText.b, 0.035f);
                            sp.textColor = colorToSetText;
                            textMesh.text = cat;
                        }

                        //sp.Dim();
                        sp.AddInData(node);
                        break;
                    }
                    else
                    {
                        if (elements[i].GetComponent<SplineDecorator>().title == cat)
                        {
                            SetGameobject(ref node, i, 0);
                            SplineDecorator sp = elements[i].GetComponent<SplineDecorator>();
                            sp.AddInData(node);
                            break;
                        }
                    }
                }
            }
            else if (datasetCategory == DatasetCategory.Singleton)
            {
                /*
                for (int i = 0; i < DataSetStrings.Count; i++)
                {
                    for (int titleInd = 0; titleInd < node.Authors.Count; titleInd++)
                    {
                        if (elements[i] == null)
                        {
                            elements[i] = Instantiate(items[0]) as Transform;
                            string addString = DataSetStrings[i];

                            AddInDataHelper(node, addString, i);
                            SetGameobject(ref node, i, 3);
                        }
                        else
                        {
                            if (!propertiesSet)
                                DataSetStrings.Add(node.Authors[titleInd]);
                             SetGameobject(ref node, i, 3);
                            SplineDecorator sp = elements[i].GetComponent<SplineDecorator>();
                            sp.AddInData(node);
                        }
                    }
                }
                */
            }
            else
            {
                print("Not found: " + datasetCategory + ".");
            }

        }
        else
        {
            if (!propertiesSet)
                DataSetStrings.Add(node.Title);
        }
    }

    private void SetGameobject(ref MasterNode node, int elementIndex, int level)
    {
        node.MasterNodeGameObjects[level] = elements[elementIndex].gameObject;
        //Need to get the author from different locations

		if (node.Authors != null) {
			for (int authorIndex = 0; authorIndex < node.Authors.Count; authorIndex++) {
				AuthorData authorData = DataProcessor.GetADFromAuthor (node.Authors [authorIndex]);

				foreach (string author in node.Authors) {
					if (author != authorData.Author) {
						if (!authorData.goLocations.ContainsKey (node.MasterNodeGameObjects [level]))
							authorData.goLocations [node.MasterNodeGameObjects [level]] = new List<string> ();

						authorData.goLocations [node.MasterNodeGameObjects [level]].Add (author);
					}
				}
			}
		} else if (node.Title != null) {
			/*AuthorData authorData = DataProcessor.GetADFromTitle (node.Authors [authorIndex]);
			authorData.goLocations [node.MasterNodeGameObjects [level]] = new List<string> ();
			authorData.goLocations [node.MasterNodeGameObjects [level]].Add(node.Title);*/
		}
    }

    public void AddInAuthors()
    {
        foreach (Transform ele in elements)
        {
            if (ele != null)
            {
                SplineDecorator sp = ele.GetComponent<SplineDecorator>();
                string authors = "";
                for (int i = 0; i < sp.DataSetStrings.Count; i++)
                {
                    List<string> t = DataProcessor.articleContainerDictionary[sp.DataSetStrings[i]].Authors;

                    for (int j = 0; j < t.Count; j++)
                    {
                        authors += "\n" + t[j];
                    }
                }
                ele.GetChild(0).GetComponent<TextMesh>().text = authors;
            }
        }
    }

    public void HighlightAuthors(string sourceAuthor, List<string> generalList)
    {
        //Originally had debug here - Debug.Log(datasetCategory);
        if (datasetCategory == DatasetCategory.Singleton)
        {
            string source = transform.GetChild(0).GetComponent<TextMesh>().text;

            string searchPattern = sourceAuthor;

            if (transform.GetChild(0).GetComponent<TextMesh>()
                && Regex.Matches(source, searchPattern).Count == 0)
            {
                foreach (string author in generalList)
                {
                    if (transform.GetChild(0).GetComponent<TextMesh>().text.Contains(author))
                    {
                        originalLabel = transform.GetChild(0).GetComponent<TextMesh>().text;
                        //Set color based on what the current color of the datapoint's mesh renderer object is
                        //print(transform.GetComponent<Renderer>().material.GetColor("_TintColor"));
                        transform.GetChild(0).GetComponent<TextMesh>().color = transform.GetComponent<Renderer>().material.GetColor("_TintColor");
                        transform.GetChild(0).GetComponent<TextMesh>().text += author + "\n";
                        //break;
                    }
                }

            }
        }
        else
        {
            foreach (Transform ele in elements)
            {
                if (ele)
                {
                    ele.GetComponent<SplineDecorator>().HighlightAuthors(sourceAuthor, generalList);
                }
            }
        }
    }

    public void HighlightSourceAuthor(string sourceAuthor, Color highlightColor)
    {
        if (datasetCategory == DatasetCategory.Singleton)
        {
            if (transform.GetChild(0).GetComponent<TextMesh>())
            {
                if (transform.GetChild(0).GetComponent<TextMesh>().text.Contains(sourceAuthor))
                {
                    originalLabel = transform.GetChild(0).GetComponent<TextMesh>().text;
                    transform.GetChild(0).GetComponent<TextMesh>().color = highlightColor;
                    transform.GetComponent<Renderer>().material.SetColor("_TintColor", highlightColor);
                }
            }
            else
            {
                foreach (Transform ele in elements)
                {
                    if (ele)
                    {
                        ele.GetComponent<SplineDecorator>().HighlightSourceAuthor(sourceAuthor, highlightColor);
                    }
                }
            }
        }
    }

    public void HighlightAuthors(List<string> sourceList, List<string> destList, string inputCategory)
    {
        if (datasetCategory == DatasetCategory.Singleton)
        {
            if (transform.GetChild(0).GetComponent<TextMesh>())
            {
                foreach (string sourceAuthor in sourceList)
                {
                    if (transform.GetChild(0).GetComponent<TextMesh>().text.Contains(sourceAuthor)
                        && (this.transform.parent.parent.parent.GetComponent<SplineDecorator>().title == inputCategory))
                    {
                        originalLabel = transform.GetChild(0).GetComponent<TextMesh>().text;
                        transform.GetChild(0).GetComponent<TextMesh>().color = Color.yellow;
                        transform.GetChild(0).GetComponent<TextMesh>().text = sourceAuthor;
                        break;
                    }
                }
                foreach (string destAuthor in destList)
                {
                    if (transform.GetChild(0).GetComponent<TextMesh>().text.Contains(destAuthor)
                        && (this.transform.parent.parent.parent.GetComponent<SplineDecorator>().title != inputCategory))
                    {
                        originalLabel = transform.GetChild(0).GetComponent<TextMesh>().text;
                        transform.GetChild(0).GetComponent<TextMesh>().color = Color.red;
                        transform.GetChild(0).GetComponent<TextMesh>().text = destAuthor;
                        break;
                    }
                }
            }
        }
        else
        {
            foreach (Transform ele in elements)
            {
                if (ele)
                {
                    ele.GetComponent<SplineDecorator>().HighlightAuthors(sourceList, destList, inputCategory);
                }
            }
        }
    }

    public void ReverseAuthors()
    {

        if (datasetCategory == DatasetCategory.Singleton)
        {
            if (transform.GetChild(0).GetComponent<TextMesh>())
            {
                if (originalLabel != "")
                {
                    transform.GetChild(0).GetComponent<TextMesh>().color = Color.white;
                    transform.GetChild(0).GetComponent<TextMesh>().text = originalLabel;
                    originalLabel = "";
                }

            }
        }
        else
        {
            foreach (Transform ele in elements)
            {
                if (ele)
                {
                    ele.GetComponent<SplineDecorator>().ReverseAuthors();
                }
            }
        }
    }

    public void AddInAuthorText(List<string> source, List<string> dest)
    {
        //Right now, I want to make the authors in the source List all yellow,
        //while the dest authors List are all red.
        Color inputColor = Color.white;
        foreach (Transform ele in elements)
        {
            if (ele != null)
            {
                SplineDecorator sp = ele.GetComponent<SplineDecorator>();
                string authors = "";
                for (int i = 0; i < sp.DataSetStrings.Count; i++)
                {
                    List<string> t = DataProcessor.articleContainerDictionary[sp.DataSetStrings[i]].Authors;

                    for (int j = 0; j < t.Count; j++)
                    {
                        if (source.Contains(t[j]))
                        {
                            authors = "\n" + t[j];
                            inputColor = Color.red;
                        }
                        else if (dest.Contains(t[j]))
                        {
                            authors = "\n" + t[j];
                            inputColor = Color.yellow;
                        }
                    }
                }
                ele.GetChild(0).GetComponent<TextMesh>().text = authors;
                ele.GetChild(0).GetComponent<TextMesh>().color = inputColor;
            }
        }

    }

    // Populates the data structure at this level with data points contained within the master nodes list
    public void AddInData(List<MasterNode> masterNodes)
    {
        if (isSphere == 0)
        {
            //must always check true, since we are populating an expanded structure
            if (expanded)
            {
                // categories are populated by default at play start
                if (datasetCategory == DatasetCategory.Categories)
                {
                    foreach (MasterNode node in masterNodes)
                    {
                        string cat = node.Category;

                        for (int i = 0; i < elements.Length; i++)
                        {
                            if (elements[i] == null)
                            {
                                elements[i] = Instantiate(items[0]) as Transform;
                                elements[i].GetComponent<SplineDecorator>().title = cat;

                                if (elements[i].FindChild("Ring_01"))
                                {
                                    Transform child = elements[i].FindChild("Ring_01");
                                    Material mat = new Material(child.GetComponent<Renderer>().material);
                                    mat.color = new Color(0.0f, 0.3f, 1.0f, 0.1f);
                                    elements[i].FindChild("Ring_01").GetComponent<Renderer>().material = mat;
                                }

                                float stepSize = 1f / (frequency);
                                Vector3 position = spline.GetPoint((float)i * stepSize);

                                elements[i].localPosition = position;
                                elements[i].LookAt(position + spline.GetDirection((float)i * stepSize));
                                elements[i].parent = transform;
                                elements[i].localScale = Vector3.one;

                                oPos1[i] = elements[i].localPosition;
                                oRot1[i] = elements[i].rotation;

                                SplineDecorator sp = elements[i].GetComponent<SplineDecorator>();
                                sp.textColor = textColor;
                                TextMesh[] allTextMeshes = elements[i].GetComponentsInChildren<TextMesh>();

                                foreach (TextMesh textMesh in allTextMeshes)
                                {
                                    textMesh.text = cat;
                                }
                                //sp.Dim();
                                sp.AddInData(node);
                                break;
                            }
                            else
                            {
                                if (elements[i].GetComponent<SplineDecorator>().title == cat)
                                {
                                    SplineDecorator sp = elements[i].GetComponent<SplineDecorator>();
                                    sp.AddInData(node);
                                    break;
                                }
                            }

                        }
                    }
                }
                else if (datasetCategory == DatasetCategory.Decades)
                {
					Debug.Log ("MasterNode count: " + masterNodes.Count);
                    foreach (MasterNode node in masterNodes)
                    {

                        int startDecade = 1960;
                        int yr = node.Year;
                        if (elements.Length > 0)
                        {
                            for (int i = 0; i < elements.Length; i++, startDecade += 10)
                            {
                                if (elements[i] == null)
                                {
                                    if (startDecade == yr - (yr % 10))
                                    {
                                        elements[i] = Instantiate(items[0]) as Transform;

                                        elements[i].GetComponent<SplineDecorator>().title = startDecade.ToString();

                                        float stepSize = 1f / (frequency);
                                        Vector3 position = spline.GetPoint((float)i * stepSize);

                                        elements[i].localPosition = position;
                                        elements[i].LookAt(position + spline.GetDirection((float)i * stepSize));
                                        elements[i].parent = transform;

                                        elements[i].localScale = transform.localScale;
                                        //elements [i].localScale = scalar.transform.localScale * .02f;

                                        oPos1[i] = elements[i].localPosition;
                                        oRot1[i] = elements[i].rotation;

                                        SplineDecorator sp = elements[i].GetComponent<SplineDecorator>();
                                        sp.textColor = textColor;
                                        if (!propertiesSet)
                                            DataSetStrings.Add(node.Title);
                                        sp.AddInData(node);
                                        //TextMesh[] allTextMeshes = elements[i].GetComponentsInChildren<TextMesh>();
                                        //TODO

                                        if (elements[i].GetChild(0).GetComponent<TextMesh>())
                                        {
                                            elements[i].GetChild(0).GetComponent<TextMesh>().text = (startDecade).ToString();
                                            elements[i].GetChild(1).GetComponent<TextMesh>().text = (startDecade).ToString();
                                        }

                                        /*foreach (TextMesh tm in allTextMeshes)
                                        {
                                            //tm.text = (startDecade).ToString();
                                        }*/

                                    }
                                    else
                                    {
                                        continue;
                                    }
                                }
                                else if (elements[i].GetComponent<SplineDecorator>().title.CompareTo((yr - (yr % 10)).ToString()) == 0)
                                {
                                    if (!propertiesSet)
                                        DataSetStrings.Add(node.Title);
									
                                    SplineDecorator sp = elements[i].GetComponent<SplineDecorator>();
                                    sp.textColor = textColor;
                                    sp.AddInData(node);
                                }
                            }
                        }
                    }
                }
                else
                {
                    /*foreach (MasterNode t in masterNodes) {
                        print (t.Title);
                    }*/
                    foreach (MasterNode node in masterNodes)
                        AddInData(node);
                }

            }
            else
            {
                Debug.LogError("not expanded");
            }
        }
        else
        {
            docking = true;
            loadReady = new List<MasterNode>(masterNodes);
            if (isSphere == 1)
                isSphere = 2;
            RTS();
        }
    }

    public void ExpandDataPoint()
    {
        if (doOnce)
        {
            doOnce = false;

            if (expanded)
            {
                ContractDataPoint();
            }
            else
            {
                if (transform.GetChild(0) && transform.GetChild(0).GetComponent<TextMesh>())
                {
                    transform.GetChild(0).localScale = Vector3.one;
                }

                List<MasterNode> nodeList = new List<MasterNode>();

                foreach (string str in DataSetStrings)
                {
					//TODO: Need to get the articleContainerDictionary to be successfully initialized with key->value pairs
                    nodeList.Add(DataProcessor.articleContainerDictionary[str]);
                }

                Load();

                GetComponent<MeshRenderer>().enabled = false;

                // temporarily set to large size to allow spawn of datapoints at proper scale, then downscale before lerp
                //transform.localScale = transform.localScale * 50f;
                //Vector3 targetScale = transform.localScale;
                if (!propertiesSet)
                {
                    elements = new Transform[frequency];
                    startLoc = transform.localPosition;
                    endLoc = transform.localPosition * 2f;
                    propertiesSet = true;

                    //AddInData (nodeList); //MUST be after proertiesSet = true

                }
                expanded = true;
                //transform.localScale = transform.localScale * .02f;
                StartCoroutine(TransitionExpand(nodeList));
            }
        }
    }

    public void ContractDataPoint()
    {
        if (!isMoving)
        {
            isMoving = true;

            endLoc = transform.localPosition;

            StartCoroutine(TransitionContract());

            expanded = false;
        }
    }

    public IEnumerator TransitionExpand(List<MasterNode> nodeList)
    {
        float t = 0f;
        //VisibleChildren ();
        cube = GameObject.CreatePrimitive(PrimitiveType.Cube).transform;
        cube.GetComponent<Renderer>().material = Resources.Load("Materials/T2", typeof(Material)) as Material;
        cube.GetComponent<Renderer>().material.SetColor("_TintColor", textColor);
        cube.parent = transform.parent;
        //cube.localRotation = transform.localRotation;
        //cube.localPosition = Vector3.Lerp(startLoc, endLoc, .5f);
        //cube.localScale = new Vector3(.01f, .38f, .01f);
        while (t < 1.0f)
        {
            for (int i = 0; i < frequency; i++)
            {
                transform.localPosition = Vector3.Lerp(startLoc, endLoc, t / 1f);
                transform.localScale = Vector3.Lerp(Vector3.one * .015f, Vector3.one * .5f, t / 1f);
                //cube.localScale = Vector3.Lerp(new Vector3(.01f, .0f, .01f), new Vector3(.01f, .38f, .01f), t / 1f);
            }
            t += Time.deltaTime;
            yield return new WaitForFixedUpdate();
        }
        transform.localScale = Vector3.one * .5f;
        isMoving = false;
        //Below line has connectors load every time you expand something
        doOnce = true;
        if (!dataAdded)
        {
            AddInData(nodeList); //MUST be after propertiesSet = true
            dataAdded = true;
            CallAlgorithm();
        }
        EnableElements();
        BrightenChildren();
        
        yield return null;
    }

    //To spread the datapoints across the line
    void SpreadDatapoints()
    {
        float leftMostPos = transform.localPosition.x - transform.localScale.x / 2.0f;

        float[] spreadPoints = new float[26];

        for (int i = 0; i < spreadPoints.Length; i++)
        {
            spreadPoints[i] = leftMostPos + (i / 26.0f);
        }
    }

    public IEnumerator TransitionContract()
    {
        float t = 0f;
        
        while (t < 1.0f)
        {
            for (int i = 0; i < frequency; i++)
            {
                transform.localPosition = Vector3.Lerp(endLoc, startLoc, t / 1f);
                transform.localScale = Vector3.Lerp(Vector3.one * .5f, Vector3.one * .015f, t / 1f);
            }
            t += Time.deltaTime;
            yield return new WaitForFixedUpdate();

        }
        transform.localScale = Vector3.one * .02f;
        if (cube)
            Destroy(cube.gameObject);

        if (transform.GetChild(0) && transform.GetChild(0).GetComponent<TextMesh>())
        {
            transform.GetChild(0).localScale = Vector3.one * 20f;
        }

        transform.GetComponent<MeshRenderer>().enabled = true;
        isMoving = false;
        //Below line has connectors load every time you expand something
        doOnce = true;
        ContractElements();
        yield return null;
    }

    private void ContractElements()
    {
        foreach (Transform ele in elements)
        {
            if (ele != null)
            {
                SplineDecorator sp = ele.GetComponent<SplineDecorator>();
                if (sp && sp.expanded == true)
                {
                    sp.ContractDataPoint();
                }
                sp.GetComponent<Collider>().enabled = false;
            }
        }
    }

    private void EnableElements()
    {
        foreach (Transform ele in elements)
        {
            if (ele != null)
            {
                SplineDecorator sp = ele.GetComponent<SplineDecorator>();

                sp.GetComponent<Collider>().enabled = true;
            }
        }

    }

    public void CallAlgorithm()
    {
        if (masterGUIHandler.isOnArticle && masterGUIHandler.calledConnectionInfo)
        {
            CleanConnections();
            if (masterGUIHandler.inputField.text != null && masterGUIHandler.inputField.text != "")
            {
                GameObject.Find("MasterGUIHandler").GetComponent<GUIHandler>().HighlightConnectionsbyCoauthor(masterGUIHandler.mostRecentInput); //FIXED: Don't hardcode the author, use what is selected from the author.
            }
        }
    }

    public void DimSurroundingVisuals(Transform frame)
    {
        for (int i = 0; i < elements.Length; i++)
        {
            if (elements[i])
            {
                if (elements[i] != frame)
                {
                    elements[i].GetComponent<SplineDecorator>().Dim();
                }
            }
        }
    }

    public void Dim()
    {
        if (!dimmed)
        {
            dimmed = true;
            Material mat;
            if (transform.FindChild("Ring_01"))
            {
                mat = transform.FindChild("Ring_01").GetComponent<Renderer>().material;
                Color StartCol = mat.GetColor("_TintColor");
                
                mat.SetColor("_TintColor", new Color(StartCol.r, StartCol.g, StartCol.b, .001f));
            }

            if (GetComponent<Collider>() && GetComponent<Collider>().GetComponent<Renderer>() &&
                GetComponent<Collider>().GetComponent<Renderer>().material)
            {
                mat = GetComponent<Collider>().GetComponent<Renderer>().material;
                Color col = mat.GetColor("_TintColor");
                mat.SetColor("_TintColor", new Color(col.r, col.g, col.b, .025f));
            }

            if (transform.GetChild(0).GetComponent<TextMesh>())
            {
                Color col = transform.GetChild(0).GetComponent<TextMesh>().color;
                transform.GetChild(0).GetComponent<TextMesh>().color = new Color(col.r, col.g, col.b, 0.035f);
            }
            if (cube)
                cube.GetComponent<Renderer>().material.SetColor("_TintColor", new Color(textColor.r, textColor.g, textColor.b, 0.035f));

            for (int i = 0; i < elements.Length; i++)
            {
                if (elements[i] != null)
                {
                    elements[i].GetComponent<SplineDecorator>().Dim();
                }
            }
        }
    }

    public void Brighten()
    {
        if (dimmed) {
            dimmed = false;
            if (transform.parent && transform.parent.GetComponent<SplineDecorator>().datasetCategory != DatasetCategory.Decades)
            {
                transform.parent.GetComponent<SplineDecorator>().Brighten();
            } else if (transform.parent && transform.parent.GetComponent<SplineDecorator>().datasetCategory == DatasetCategory.Decades)
            {
                transform.parent.GetComponent<SplineDecorator>().DimSurroundingVisuals(transform);
            }

            Material mat;
            if (transform.FindChild("Ring_01"))
            {
                mat = transform.FindChild("Ring_01").GetComponent<Renderer>().material;
                Color StartCol = mat.GetColor("_TintColor");
                mat.SetColor("_TintColor", new Color(StartCol.r, StartCol.g, StartCol.b, 1f));
            }

            if (GetComponent<Collider>() && GetComponent<Collider>().GetComponent<Renderer>() &&
                GetComponent<Collider>().GetComponent<Renderer>().material)
            {
                mat = GetComponent<Collider>().GetComponent<Renderer>().material;
                Color col = mat.GetColor("_TintColor");
                mat.SetColor("_TintColor", new Color(col.r, col.g, col.b, 1f));
            }

            if (transform.GetChild(0).GetComponent<TextMesh>())
            {
                Color col = transform.GetChild(0).GetComponent<TextMesh>().color;
                transform.GetChild(0).GetComponent<TextMesh>().color = new Color(col.r, col.g, col.b, 0.75f);
            }
            if (cube)
                cube.GetComponent<Renderer>().material.SetColor("_TintColor", new Color(textColor.r, textColor.g, textColor.b, 0.75f));

            BrightenChildren();
        }
    }

    public void BrightenChildren()
    {
        for (int i = 0; i < elements.Length; i++)
        {
            if (elements[i] != null)
            {
                elements[i].GetComponent<SplineDecorator>().Brighten();
            }
        }
    }

    public void CleanConnections()
    {
        List<GameObject> CDGs = new List<GameObject>();
        CDGs = (GameObject.FindGameObjectsWithTag("CDG").ToList());

        foreach (GameObject obj in CDGs)
        {
            Destroy(obj);
        }

        foreach (Transform ele in elements)
        {
            if (ele)
            {

                SplineDecorator sp = ele.GetComponent<SplineDecorator>();

                if (sp)
                {
                    sp.CleanConnections();
                }

            }
        }

    }

    public void UpdateSplinePos()
    {
        foreach (BezierSpline item in connSplineList)
        {
            //print(item.gameObject);
            LoadConnectors(item);
        }
    }

    //Need to set the positions here to be internal, going inside the ring of the 

    /// <summary>
    /// This method will be for all articles in the dictionary, as it notifies of a relationship between all articles within the current graph.
    /// It will be used to detect author profile relationships.
    /// </summary>
    public void DrawArticleConnections(GameObject originSelectedAuthorSpot, AuthorData[] coAuthors, Color connectionSplineColor)
    {
        if (originSelectedAuthorSpot == null)
            return;

        GameObject sourceGO = originSelectedAuthorSpot, destGO = null;
        Vector3 sourcePos = Vector3.zero, worldSourcePos = Vector3.zero;
        Vector3 destPos = Vector3.zero, worldDestPos = Vector3.zero;

        //Debug.Log(coAuthors.Length);
        for (int coauthorIndex = 0; coauthorIndex < coAuthors.Length; coauthorIndex++)
        {
            List<GameObject> coauthorGOLocations = (List<GameObject>)coAuthors[coauthorIndex].goLocations.Keys.ToList();
            //Debug.Log("Coauthor GO count: " + coauthorGOLocations.Count);
            for (int coauthorGOLocInd = 0; coauthorGOLocInd < coauthorGOLocations.Count; coauthorGOLocInd++)
            {
                if (!sourceGO.GetComponent<SplineDecorator>().expanded &&
                    !coauthorGOLocations[coauthorGOLocInd].GetComponent<SplineDecorator>().expanded)
                {
                    sourceGO.GetComponent<Renderer>().material.SetColor("_EmissionColor", connectionSplineColor);
                    sourcePos = sourceGO.transform.localPosition;
                    worldSourcePos = sourceGO.transform.TransformPoint(sourcePos);

                    destGO = coauthorGOLocations[coauthorGOLocInd];
                    if (sourceGO != destGO)
                    {
                        destGO.GetComponent<Renderer>().material.SetColor("_EmissionColor ", connectionSplineColor);
                        destPos = destGO.transform.localPosition;
                        worldDestPos = destGO.transform.TransformPoint(destPos);

                        //Putt in connector splines into here
                        Vector3 menuSource = GameObject.FindGameObjectWithTag("Menu").transform.InverseTransformPoint(worldSourcePos);

                        Transform connTrans = Instantiate(connector) as Transform;
                        connTrans.transform.rotation = GameObject.FindGameObjectWithTag("Menu").transform.localRotation;

                        connector.GetComponent<BezierSpline>().source = sourceGO.transform;
                        connector.GetComponent<BezierSpline>().destination = destGO.transform;
                        //Need to add a different parent to this transform
                        BezierSpline connSpline = connTrans.GetComponent<BezierSpline>();

                        connTrans.parent = GameObject.FindGameObjectWithTag("Menu").transform;
                        connTrans.parent.GetComponent<SplineDecorator>().connSplineList.Add(connSpline);

                        Vector3 menuDest =
                        GameObject.FindGameObjectWithTag("Menu").transform.InverseTransformPoint(worldDestPos);

                        connTrans.transform.localPosition = Vector3.zero;

                        if (sourceGO.transform.parent == destGO.transform.parent)
                        {
                            Vector3 centerPos =
                                GameObject.FindGameObjectWithTag("Menu").transform.InverseTransformPoint(sourceGO.transform.parent.TransformPoint(destPos));
                            
                            connSpline.points[0] = menuSource;
                            connSpline.points[1] = centerPos;
                            connSpline.points[2] = centerPos;
                            connSpline.points[3] = menuDest;

                        } else
                        {
                            connSpline.points[0] = menuSource;
                            connSpline.points[1] = menuSource / 2;
                            connSpline.points[2] = menuDest / 2;
                            connSpline.points[3] = menuDest;
                        }

                        /*Vector3 spawnPos = connSpline.GetPoint(0);
                        print(spawnPos);
                        Transform trans = GameObject.CreatePrimitive(PrimitiveType.Sphere).transform;
                        trans.position = spawnPos;
                        trans.localScale = Vector3.one * .03f;*/
                        
                        //Debug.Log("Source Position: " + sourcePos);
                        //Debug.Log("Dest Position: " + destPos);


                        LoadConnectors(connSpline, connectionSplineColor, coAuthors.Length);

                        sourceGO = destGO;
                        destGO = null;
                    }
                }
            }
        }
    }

    private void LoadConnectors(BezierSpline splineConn)
    {
        if (splineConn.source != null && splineConn.destination != null)
        {
            Vector3 worldSourcePos = splineConn.source.position;
            Vector3 worldDestPos = splineConn.destination.position;
            Vector3 menuSource =
                        GameObject.FindGameObjectWithTag("Menu").transform.InverseTransformPoint(worldSourcePos);
            Vector3 menuDest =
                        GameObject.FindGameObjectWithTag("Menu").transform.InverseTransformPoint(worldDestPos);
            //Setting start position

            if (splineConn.source.transform.parent == splineConn.destination.transform.parent)
            {
                Vector3 centerPos =
                    GameObject.FindGameObjectWithTag("Menu").transform.InverseTransformPoint(
                        splineConn.destination.transform.parent.position);
                
                splineConn.points[0] = menuSource;
                splineConn.points[1] = centerPos;
                splineConn.points[2] = centerPos;
                splineConn.points[3] = menuDest;
            }
            else
            {
                splineConn.points[0] = menuSource;
                //Setting halfway points between source and dest
                splineConn.points[1] = menuSource / 2;
                splineConn.points[2] = menuDest / 2;
                //Set end position
                splineConn.points[3] = menuDest;
            }

            float stepSize = 1f / (NUM_CONNOBJS);
            for (int p = 0, f = 0; f < NUM_CONNOBJS; f++)
            {
                for (int i = 0; i < connectGuide.Length; i++, p++)
                {
                    if (splineConn && splineConn.transform.childCount > 0)
                    {
                        Transform item = splineConn.transform.GetChild(f);
                        Vector3 position = splineConn.GetPoint(p * stepSize);
                        //This was localposition before lawl
                        item.transform.position = position;

                        item.transform.LookAt(position + splineConn.GetDirection(p * stepSize));

                        Vector3 vel = splineConn.GetVelocity(p * stepSize);
                        
                        item.localScale = new Vector3(item.transform.localScale.x, item.transform.localScale.y, vel.magnitude * CONN_SCALE * (32.0f/NUM_CONNOBJS));
                    }
                }
            }
        }
    }

    private void LoadConnectors(BezierSpline splineConn, Color splineColor, int coauthorNumScale)
    {
        //Coauthors - Need to scale by coauthorNumScale

        float stepSize = 1f / (NUM_CONNOBJS);
        for (int p = 0, f = 0; f < NUM_CONNOBJS; f++)
        {
            for (int i = 0; i < connectGuide.Length; i++, p++)
            {
                //Why is the "splineConn.transform.childCount" part in the if statement needed?
                //NVM only needed in the LoadConnectors overloaded method that only takes in one param
                //Debug.Log("Child Count: " + splineConn.transform.childCount);
                Transform item = Instantiate(connectGuide[i]);

                item.gameObject.GetComponentInChildren<Renderer>().material.SetColor("_EmissionColor", splineColor);
                splineColor.a = Random.Range(0f, 255f);
                item.gameObject.GetComponentInChildren<Renderer>().material.SetColor("_TintColor", splineColor);

                Vector3 position = splineConn.GetPoint(p * stepSize);
                item.transform.localPosition = position;

                item.transform.LookAt(position + splineConn.GetDirection(p * stepSize));

                item.transform.parent = splineConn.transform;

                Vector3 vel = splineConn.GetVelocity(p * stepSize);
                item.localScale = new Vector3(item.transform.localScale.x * coauthorNumScale, item.transform.localScale.y * coauthorNumScale, vel.magnitude * CONN_SCALE * (32.0f / NUM_CONNOBJS));
            }
        }
    }

    public void Clean()
    {

        if (GetComponent<Renderer>() != null)
        {
            Color endCol = new Color(1f, 1f, 1f, .001f);
            GetComponent<Renderer>().material.SetColor("_TintColor", endCol);
        }

        foreach (Transform ele in elements)
        {
            if (ele)
            {

                SplineDecorator sp = ele.GetComponent<SplineDecorator>();

                if (sp)
                {
                    sp.Clean();
                }

            }
        }
        //Dim();
    }

    private int GetTotalCntVisibleNeighborGOs(List<string> neighborArticles)
    {
        int total = 0;
        foreach (string article in neighborArticles)
        {
            foreach (GameObject go in DataProcessor.articleContainerDictionary[article].MasterNodeGameObjects)
            {
                if (go != null)
                {
                    total += 1;
                }
            }
        }

        return total;
    }

    public void RTS()
    {
        if (isSphere == 0)
        {
            print("to line");
            StartCoroutine(TransitionRingsToCurveLine());
            isSphere = 1;

        }
        else if (isSphere == 1)
        {
            print("to vert line");
            StartCoroutine(TransitionCurveToStraightLine());
            isSphere = 2;
        }
        else
        {
            print("to sphere");
            StartCoroutine(TransitionRingsToSphere());
            isSphere = 0;
        }
    }

    public void RTL()
    {

    }

    private IEnumerator TransitionRingsToSphere()
    {

        if (spline2)
        {

            // make the camera move down slightly
            float t = 0f;
            while (t < 1.0f)
            {
                for (int i = 0; i < frequency; i++)
                {
                    if (elements[i])
                    {
                        elements[i].localPosition = Vector3.Lerp(oPos3[i], oPos1[i], t / speed);
                        elements[i].localRotation = Quaternion.Lerp(oRot3[i], oRot1[i], t / speed);
                    }
                }
                t += Time.deltaTime;
                yield return new WaitForFixedUpdate();
            }
            canTransition = false;
            if (docking)
            {

                AddInData(loadReady);
                //loadReady = null;
                docking = false;
            }
            yield return null;
        }
    }

    private IEnumerator TransitionRingsToCurveLine()
    {
        if (spline2)
        {
            // make the camera move down slightly
            float t = 0f;
            while (t < 1.0f)
            {
                for (int i = 0; i < frequency; i++)
                {
                    if (elements[i])
                    {
                        elements[i].localPosition = Vector3.Lerp(oPos1[i], oPos2[i], t / speed);
                        elements[i].localRotation = Quaternion.Lerp(oRot1[i], oRot2[i], t / speed);
                    }
                }
                t += Time.deltaTime;
                yield return new WaitForFixedUpdate();
            }
            canTransition = false;
            yield return null;
        }
    }

    private IEnumerator TransitionCurveToStraightLine()
    {
        if (spline3)
        {
            // make the camera move down slightly
            float t = 0f;
            while (t < 1.0f)
            {
                for (int i = 0; i < frequency; i++)
                {
                    if (elements[i])
                    {
                        elements[i].localPosition = Vector3.Lerp(oPos2[i], oPos3[i], t / speed);
                        elements[i].localRotation = Quaternion.Lerp(oRot2[i], oRot3[i], t / speed);
                    }
                }
                t += Time.deltaTime;
                yield return new WaitForFixedUpdate();
            }
            canTransition = false;
            yield return null;
        }
    }


}
