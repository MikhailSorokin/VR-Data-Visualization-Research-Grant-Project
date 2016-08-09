using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.UI;
using System.Linq;


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
        Articles,
        Singleton
    }

    public DatasetCategory datasetCategory;
    public Transform[] items;
    public Transform[] elements;
    public Transform[] lineGuide;
    public static GUIHandler masterGUIHandler;
    public bool expanded = false;
    public List<string> DataSetStrings = new List<string>();
    public const float OG_SIZE = 0.001f;

    private bool isMoving;
    public bool isRotating = false;

    private Vector3 startLoc;
    private Vector3 endLoc;
    private string[] categories = {
        "Uncategorized", "VR", "A.I.", "Robotics", "Graphics", "Algorithms", "BioInformatics",
        "Data Visualization", "Numerical Analysis", "Scientific Computations", "Programming Languages",
        "Game Development", "Cybersecurity", "Machine Learning", "Networking", "Databases", "Human Computer Interaction"
    };

    private Color[] colorCategory =
    {
        Color.white, Color.green, Color.magenta, Color.red, Color.blue, Color.yellow, Color.cyan,
        new Color(255f/255f, 175f/255f, 0f, 255f/255f), new Color(255f/255f, 10f/255f, 0f, 255f/255f), new Color(81f/255f, 0f, 255f/255f, 255f/255f), new Color(102f/255f, 255f/255f, 255f/255f, 255f/255f),
        new Color(60f/255f, 255f/255f, 102f/255f, 255f/255f), new Color(33f/255f, 171f/255f, 205f/255f, 255f/255f), new Color(0f, 0.3f, 255f/255f, 255f/255f), new Color(1f, 0.2f, 0f, 255f/255f),
        new Color(0f/255f, 255f/255f, 128f/255f, 255f/255f), new Color(226f/255f, 182f/255f, 49f/255f, 255f/255f)
    };

    //private Vector3 startScale = Vector3.one * .02f;
    //private Vector3 endScale = Vector3.one * .5f;

    private bool propertiesSet = false;
    private bool dimmed = true;
    private int isSphere = 0;
    private bool doOnce = true;
    private bool docking = false;
    private bool dataAdded = false;
    private bool connectionsDrawn = false;
    private List<MasterNode> loadReady = null;
    //private LineRenderer lineRenderer;
    //private Material lineRendererMat;
    private Transform cube;

    // connector vars
    public Transform connector;
    public List<Transform> connectorSplines;
    public Transform[] connectGuide;
    private Dictionary<int, SplineDecorator> diskSplineDictionary;
    private Dictionary<int[,], bool> connectorIds;
    public Dictionary<string, List<string>> articleAuthors;
    public Dictionary<string, int> articleYears;
    // connector vars

    // lookup variables from data points
    public Dictionary<Transform, string> dataLookups;

    private List<string> algorithmSourceAuthors = new List<string>();
    private List<string> algorithmDestAuthors = new List<string>();
    private List<string> generalAuthors = new List<string>();
    private List<Vector3> allPositions = new List<Vector3>();
    public List<BezierSpline> connSplineList = new List<BezierSpline>();
    private string originalLabel = "";
    private bool coauthorsConnection = false;
    //private GameObject lineRendererContainer;

    void Start()
    {
        // Mike - If needed to use line renderer, comment the next couple of lines out
        // and comment out the variables which are needed to be accessed within.

        //lineRenderer = this.gameObject.AddComponent<LineRenderer>();
        /*if (this.gameObject == GameObject.FindGameObjectWithTag("Menu"))
        {
            lineRendererContainer = new GameObject("LinerendererContainer");
            lineRendererMat = new Material(Shader.Find("Particles/Additive"));
        }*/

        masterGUIHandler = FindObjectOfType<GUIHandler>();
    }

    public void HighlightArticles(string str)
    {
        int[] years = new int[0];
        for (int i = 0; i < years.Length; i++)
        {
            GameObject button = GameObject.FindGameObjectWithTag("Button");
            Text tx = button.GetComponentInChildren<Text>();
            if (tx.text == "Article")
            {
                //print ("button - " + authorArticles [a] [0]);
                //tx.text = authorArticles [a] [0];
            }
        }
    }

    private void Awake()
    {

        Dim();
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
        if (expanded && datasetCategory != DatasetCategory.Decades && cube != null)
        {
            Vector3 pA = transform.position;
            Vector3 pB = transform.parent.TransformPoint(startLoc);

            float dist = Vector3.Distance(pA, pB);
            //if (datasetCategory == DatasetCategory.Articles)
            //	dist *= 2;
            cube.localScale = new Vector3(.01f, .01f, dist * .5f / transform.lossyScale.x);

            Vector3 midPoint = pA + (pB - pA) / 2f;

            cube.position = midPoint;
            cube.LookAt(pA);

        }

        if (isRotating)
        {
            //print("here");
            //UpdateSplinePos();
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

    public void loadTransforms()
    {
        oPos1 = new Vector3[frequency];
        oPos2 = new Vector3[frequency];
        oRot1 = new Quaternion[frequency];
        oRot2 = new Quaternion[frequency];

        for (int i = 0; i < frequency; i++)
        {
            oPos1[i] = new Vector3();
            oPos2[i] = new Vector3();
            oRot1[i] = new Quaternion();
            oRot2[i] = new Quaternion();

        }
    }

    public void Highlight(List<string> articles)
    {

        if (GetComponent<Renderer>())
        {
            GetComponent<Renderer>().material.color = Color.red;
        }
        else
        {
            foreach (Transform ele in elements)
            {
                if (ele)
                    ele.GetComponent<SplineDecorator>().Highlight(articles);
            }
        }

        if (datasetCategory == DatasetCategory.Articles)
        {
            if (articles.Contains(DataSetStrings[0]))
            {
                GetComponent<Renderer>().material.color = Color.red;
            }
        }
        else if (datasetCategory == DatasetCategory.Years)
        {
            foreach (string article in DataSetStrings)
            {
                if (articles.Contains(article))
                {
                    GetComponent<Renderer>().material.color = Color.red;
                }
            }
        }
        else if (datasetCategory == DatasetCategory.Decades)
        {
            foreach (string article in DataSetStrings)
            {
                if (articles.Contains(article))
                {
                    GetComponent<Renderer>().material.color = Color.red;
                }
            }
        }
        else if (datasetCategory == DatasetCategory.Categories)
        {
            foreach (string article in articles)
            {
                foreach (Transform element in elements)
                {
                    if (element.GetComponent<SplineDecorator>().title == DataProcessor.articleContainerDictionary[article].Category)
                    {
                        element.GetComponent<SplineDecorator>().Highlight(articles);
                    }
                }
            }
        }

    }

    private void GetPointLocation(MasterNode[] nodes)
    {

    }

    public void AddInData(MasterNode node)
    {
        int yr = node.Year;

        if (expanded)
        {
            if (datasetCategory == DatasetCategory.Decades)
            {
                int startDecade = 1960;
                //print("there");
                if (elements.Length > 0)
                {
                    for (int i = 0; i < elements.Length; i++, startDecade += 10)
                    {
                        if (elements[i] == null)
                        {
                            if (startDecade == yr - (yr % 10))
                            {
                                elements[i] = Instantiate(items[0]) as Transform;

                                if (elements[i].GetChild(0).GetComponent<TextMesh>())
                                {
                                    elements[i].GetChild(0).GetComponent<TextMesh>().text = title;
                                }
                                elements[i].GetComponent<SplineDecorator>().title = startDecade.ToString();

                                float stepSize = 1f / (frequency);
                                Vector3 position = spline.GetPoint((float)i * stepSize);

                                elements[i].localPosition = position;
                                elements[i].LookAt(position + spline.GetDirection((float)i * stepSize));
                                elements[i].parent = transform;

                                elements[i].localScale = transform.localScale;
                                elements[i].localScale = transform.localScale * .02f;
                                SplineDecorator sp = elements[i].GetComponent<SplineDecorator>();
                                if (!propertiesSet)
                                    DataSetStrings.Add(node.Title);
                                sp.AddInData(node);
                                TextMesh[] allTextMeshes = elements[i].GetComponentsInChildren<TextMesh>();

                                foreach (TextMesh textMesh in allTextMeshes)
                                {
                                    textMesh.text = title + ": " + (startDecade).ToString() + " - " + (startDecade + 9).ToString();
                                }
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
            }
            else if (datasetCategory == DatasetCategory.Years)
            {

                int yearID = yr % 10;

                if (elements.Length > 0)
                {

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
                                    dec = transform.GetComponent<SplineDecorator>().title.Remove(3) + i.ToString();
                                }
                                elements[i].GetComponent<SplineDecorator>().title = dec;

                                float stepSize = 1f / (frequency);
                                Vector3 position = spline.GetPoint((float)i * stepSize);

                                elements[i].localPosition = position;
                                elements[i].LookAt(position + spline.GetDirection((float)i * stepSize));
                                elements[i].parent = transform;

                                elements[i].localScale = Vector3.one * .02f;
                                node.MasterNodeGameObjects[1] = elements[i].gameObject;
                                SplineDecorator sp = elements[i].GetComponent<SplineDecorator>();
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
                                node.MasterNodeGameObjects[1] = elements[i].gameObject;
                                SplineDecorator sp = elements[i].GetComponent<SplineDecorator>();
                                sp.AddInData(node);
                            }
                        }
                    }
                }
            }
            else if (datasetCategory == DatasetCategory.Articles)
            {
                if (DataSetStrings.Count > 0)
                {
                    int r = Random.Range(0, DataSetStrings.Count);

                    for (int i = 0; i < elements.Length; i++)
                    {
                        //print ("i: " + i);
                        if (r == i)
                        {
                            if (elements[i] == null)
                            {
                                elements[i] = Instantiate(items[0]) as Transform;

                                SplineDecorator sp = elements[i].GetComponent<SplineDecorator>();
                                sp.title = DataSetStrings[i];
                                float stepSize = 1f / (DataSetStrings.Count);
                                Vector3 position = spline.GetPoint((float)i * stepSize);

                                elements[i].localPosition = position;
                                elements[i].LookAt(position + spline.GetDirection((float)i * stepSize));
                                elements[i].parent = transform;
                                elements[i].localScale = Vector3.one * .02f;
                                node.MasterNodeGameObjects[2] = elements[i].gameObject;
                                if (!propertiesSet)
                                    DataSetStrings.Add(sp.title);
                                sp.AddInData(node);
                            }
                            else
                            {
                                if (!propertiesSet)
                                    DataSetStrings.Add(node.Title);
                                node.MasterNodeGameObjects[2] = elements[i].gameObject;
                                SplineDecorator sp = elements[i].GetComponent<SplineDecorator>();
                                sp.AddInData(node);
                            }
                        }
                    }
                    AddInAuthors();
                }
            }
            else if (datasetCategory == DatasetCategory.Categories)
            {
                string cat = node.Category;

                int colorID = 0;

                //
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
                //

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
                        elements[i].localScale = Vector3.one * .02f;
                        node.MasterNodeGameObjects[0] = elements[i].gameObject;

                        oPos1[i] = elements[i].localPosition;
                        oRot1[i] = elements[i].rotation;

                        SplineDecorator sp = elements[i].GetComponent<SplineDecorator>();
                        TextMesh[] allTextMeshes = elements[i].GetComponentsInChildren<TextMesh>();

                        foreach (TextMesh textMesh in allTextMeshes)
                        {
                            textMesh.color = colorToSetText;
                            textMesh.text = cat;
                        }

                        sp.Dim();
                        sp.AddInData(node);
                        break;
                    }
                    else
                    {
                        if (elements[i].GetComponent<SplineDecorator>().title == cat)
                        {
                            node.MasterNodeGameObjects[0] = elements[i].gameObject;
                            SplineDecorator sp = elements[i].GetComponent<SplineDecorator>();
                            sp.AddInData(node);
                            break;
                        }
                    }

                }
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

    public void AddInData(int year)
    {

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

    public void HighlightAuthors(List<string> generalList)
    {
        //Originally had debug here - Debug.Log(datasetCategory);
        if (datasetCategory == DatasetCategory.Singleton)
        {
            if (transform.GetChild(0).GetComponent<TextMesh>())
            {
                foreach (string author in generalList)
                {
                    if (transform.GetChild(0).GetComponent<TextMesh>().text.Contains(author))
                    {
                        originalLabel = transform.GetChild(0).GetComponent<TextMesh>().text;
                        //Set color based on what the current color of the datapoint's mesh renderer object is
                        //print(transform.GetComponent<Renderer>().material.GetColor("_TintColor"));
                        transform.GetChild(0).GetComponent<TextMesh>().color = transform.GetComponent<Renderer>().material.GetColor("_TintColor");
                        transform.GetChild(0).GetComponent<TextMesh>().text = author;
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
                    ele.GetComponent<SplineDecorator>().HighlightAuthors(generalList);
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
                                TextMesh[] allTextMeshes = elements[i].GetComponentsInChildren<TextMesh>();

                                foreach (TextMesh textMesh in allTextMeshes)
                                {

                                    textMesh.text = cat;
                                }
                                sp.Dim();
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
                else if (datasetCategory == DatasetCategory.Conferences)
                {
                    foreach (MasterNode node in masterNodes)
                    {
                        string conference = node.Conference;
                        for (int i = 0; i < elements.Length; i++)
                        {
                            if (elements[i] == null)
                            {
                                elements[i] = Instantiate(items[0]) as Transform;
                                elements[i].GetComponent<SplineDecorator>().title = conference;
                                float stepSize = 1f / (frequency);
                                Vector3 position = spline.GetPoint((float)i * stepSize);
                                elements[i].localPosition = position;
                                elements[i].LookAt(position + spline.GetDirection((float)i * stepSize));
                                elements[i].parent = transform;
                                SplineDecorator sp = elements[i].GetComponent<SplineDecorator>();
                                TextMesh[] allTextMeshes = elements[i].GetComponentsInChildren<TextMesh>();
                                foreach (TextMesh textMesh in allTextMeshes)
                                {
                                    textMesh.text = conference;
                                }
                                sp.AddInData(node);
                                break;
                            }
                            else
                            {
                                if (elements[i].GetComponent<SplineDecorator>().title == conference)
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
                                        if (!propertiesSet)
                                            DataSetStrings.Add(node.Title);
                                        sp.AddInData(node);
                                        TextMesh[] allTextMeshes = elements[i].GetComponentsInChildren<TextMesh>();
                                        //TODO

                                        if (elements[i].GetChild(0).GetComponent<TextMesh>())
                                        {
                                            elements[i].GetChild(0).GetComponent<TextMesh>().text = (startDecade).ToString();
                                            elements[i].GetChild(1).GetComponent<TextMesh>().text = (startDecade).ToString();
                                        }

                                        foreach (TextMesh tm in allTextMeshes)
                                        {
                                            //tm.text = (startDecade).ToString();
                                        }

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

            if (datasetCategory == DatasetCategory.Singleton)
            {
                print("singleton");
            }
            else
            {
                if (expanded)
                {
                    ContractDataPoint();
                }
                else
                {
                    //print ("expanded");

                    if (transform.GetChild(0) && transform.GetChild(0).GetComponent<TextMesh>())
                    {
                        transform.GetChild(0).localScale = Vector3.one;
                    }

                    List<MasterNode> nodeList = new List<MasterNode>();

                    foreach (string str in DataSetStrings)
                    {
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

        while (t < 1.0f)
        {
            for (int i = 0; i < frequency; i++)
            {
                transform.localPosition = Vector3.Lerp(startLoc, endLoc, t / 1f);
                transform.localScale = Vector3.Lerp(Vector3.one * .015f, Vector3.one * .5f, t / 1f);
            }
            t += Time.deltaTime;
            yield return new WaitForFixedUpdate();

        }
        transform.localScale = Vector3.one * .5f;
        isMoving = false;
        //Below line has connectors load every time you expand something
        CallAlgorithm();
        doOnce = true;
        if (!dataAdded)
        {
            AddInData(nodeList); //MUST be after proertiesSet = true
            dataAdded = true;
        }
        EnableElements();
        Brighten();

        cube = GameObject.CreatePrimitive(PrimitiveType.Cube).transform;
        cube.GetComponent<Renderer>().material = Resources.Load("Materials/T2", typeof(Material)) as Material;
        cube.parent = transform.parent;
        cube.localRotation = transform.localRotation;
        cube.localPosition = Vector3.Lerp(startLoc, endLoc, .5f);
        cube.localScale = new Vector3(.01f, .38f, .01f);

        yield return null;
    }

    public IEnumerator TransitionContract()
    {
        float t = 0f;
        if (cube)
            Destroy(cube.gameObject);
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


        if (transform.GetChild(0) && transform.GetChild(0).GetComponent<TextMesh>())
        {
            transform.GetChild(0).localScale = Vector3.one * 20f;
        }

        transform.GetComponent<MeshRenderer>().enabled = true;
        isMoving = false;
        //Below line has connectors load every time you expand something
        CallAlgorithm();
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

    private void CallAlgorithm()
    {
        if (masterGUIHandler.isOnArticle)
        {
            if (masterGUIHandler.inputField.text != null && masterGUIHandler.inputField.text != "")
            {
                GameObject.Find("MasterGUIHandler").GetComponent<GUIHandler>().HighlightConnectionsbyCoauthor(masterGUIHandler.inputField.text);
            }
            else
            {
                GameObject.Find("MasterGUIHandler").GetComponent<GUIHandler>().HighlightConnectionsbyCoauthor();
            }
        }
    }

    public IEnumerator TransitionToInvis(Material mat)
    {
        float t = 0f;
        Color start;
        bool tint = false;
        if (mat.HasProperty("_TintColor"))
        {
            start = mat.GetColor("_TintColor");

            if (start.r < 0.1f && start.g > 0.9f && start.b < .2f)
                start = Color.white;
            tint = true;
        }
        else
        {
            start = mat.color;
        }
        dimmed = true;
        start = new Color(start.r, start.g, start.b, .1f);
        Color endCol = new Color(start.r, start.g, start.b, .001f);
        while (t < 1.0f)
        {
            if (tint)
                mat.SetColor("_TintColor", Color.Lerp(start, endCol, t / 1f));
            else
                mat.color = Color.Lerp(start, endCol, t / 1f);
            t += Time.deltaTime * 4f;
            yield return new WaitForFixedUpdate();
        }
        yield return null;
    }

    public IEnumerator TransitionToVis(Material mat)
    {
        float t = 0f;
        Color start;
        bool tint = false;
        if (mat.HasProperty("_TintColor"))
        {
            start = mat.GetColor("_TintColor");
            tint = true;
        }
        else
        {
            start = mat.color;
        }
        start = new Color(start.r, start.g, start.b, .001f);
        Color endCol = new Color(start.r, start.g, start.b, 1f);
        dimmed = false;
        while (t < 1.0f)
        {
            if (tint)
                mat.SetColor("_TintColor", Color.Lerp(start, endCol, t / 1f));
            else
                mat.color = Color.Lerp(start, endCol, t / 1f);
            t += Time.deltaTime * 4f;
            yield return new WaitForFixedUpdate();
        }
        yield return null;
    }

    public IEnumerator TransitionToVis2(Material mat)
    {
        float t = 0f;
        Color start;
        bool tint = false;
        if (mat.HasProperty("_TintColor"))
        {
            start = mat.GetColor("_TintColor");
            tint = true;
        }
        else
        {
            start = mat.color;
        }
        start = new Color(start.r, start.g, start.b, .001f);
        Color endCol = new Color(start.r, start.g, start.b, 1f);
        dimmed = false;
        while (t < 1.0f)
        {
            if (tint)
                mat.SetColor("_TintColor", Color.Lerp(start, endCol, t / 1f));
            else
                mat.color = Color.Lerp(start, endCol, t / 1f);
            t += Time.deltaTime * 4f;
            yield return new WaitForFixedUpdate();
        }
        yield return null;
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
                    print("dimmed: " + elements[i]);
                    if (elements[i].GetComponent<Renderer>())
                    {
                        StartCoroutine(TransitionToInvis(elements[i].GetComponent<Renderer>().material));
                    }
                }
                else
                {
                    elements[i].GetComponent<SplineDecorator>().Brighten();
                }
            }
        }
    }

    public void DimSurroundingVisuals2(Transform frame)
    {
        if (datasetCategory == DatasetCategory.Decades)
        {
            for (int i = 0; i < elements.Length; i++)
            {
                if (elements[i])
                {
                    if (elements[i] != frame)
                    {
                        elements[i].GetComponent<SplineDecorator>().Dim();
                        if (elements[i].GetComponent<Renderer>())
                        {
                            StartCoroutine(TransitionToInvis(elements[i].GetComponent<Renderer>().material));
                        }
                    }
                    else
                    {
                        elements[i].GetComponent<SplineDecorator>().Brighten();
                    }
                }
            }

        }
        else
        {
            transform.parent.GetComponent<SplineDecorator>().DimSurroundingVisuals2(transform.parent);
            Brighten();
        }
    }

    public void Dim()
    {
        if (!dimmed)
        {
            if (transform.FindChild("Ring_01"))
            {
                Material mat = transform.FindChild("Ring_01").GetComponent<Renderer>().material;
                StartCoroutine(TransitionToInvis(mat));
            }
            if (transform.GetComponent<Renderer>())
            {
                Material mat = GetComponent<Renderer>().material;
                StartCoroutine(TransitionToInvis(mat));
            }
            for (int i = 0; i < transform.childCount; i++)
            {
                if (transform.GetChild(i).GetComponent<TextMesh>())
                {
                    Color col = transform.GetChild(i).GetComponent<TextMesh>().color;
                    transform.GetChild(i).GetComponent<TextMesh>().color = new Color(col.r, col.g, col.b, 0.035f);
                }
            }
            for (int i = 0; i < elements.Length; i++)
            {
                if (elements[i])
                {
                    elements[i].GetComponent<SplineDecorator>().Dim();
                }
            }
        }
    }

    public void Brighten()
    {
        if (dimmed)
        {

            if (transform.FindChild("Ring_01"))
            {
                Material mat = transform.FindChild("Ring_01").GetComponent<Renderer>().material;
                StartCoroutine(TransitionToVis(mat));
            }
            if (transform.GetComponent<Renderer>())
            {
                Material mat = GetComponent<Renderer>().material;
                StartCoroutine(TransitionToVis2(mat));

            }
            for (int i = 0; i < transform.childCount; i++)
            {
                if (transform.GetChild(i).GetComponent<TextMesh>())
                {
                    Color col = transform.GetChild(i).GetComponent<TextMesh>().color;
                    transform.GetChild(i).GetComponent<TextMesh>().color = new Color(col.r, col.g, col.b, 0.75f);
                }
            }
        }
        for (int i = 0; i < elements.Length; i++)
        {
            if (elements[i] != null)
            {

                elements[i].GetComponent<SplineDecorator>().Brighten();
            }
        }

    }

    /// <summary>
    /// Make edges connect from one datapoint to another based on all of the co-authors within a 
    /// certain group.
    /// </summary>
    public void GetTopCoauthors()
    {
        /*foreach (LineRenderer lineRenderer in lineRendererContainer.GetComponentsInChildren<LineRenderer>())
        {
            Destroy(lineRenderer);
            Destroy(lineRenderer.gameObject);
        }*/

        Dictionary<int, List<string>> numCoauthorsToCoauthorlist = new Dictionary<int, List<string>>();
        int currentCoauthorCount = 0;
        int MIN_THRESHOLD = 15;
        bool foundAtLeastOneConnection = false;

        foreach (AuthorData ad in DataProcessor.allAuthorData)
        {
            currentCoauthorCount = ad.CoauthorNum; //TODO: update CoauthorNum in DataProcessor class
            if (currentCoauthorCount >= MIN_THRESHOLD)
            {
                if (!numCoauthorsToCoauthorlist.ContainsKey(currentCoauthorCount))
                    numCoauthorsToCoauthorlist[currentCoauthorCount] = new List<string>();
                numCoauthorsToCoauthorlist[currentCoauthorCount].Add(ad.Author);
                generalAuthors.Add(ad.Author);
                foundAtLeastOneConnection = true;
            }
        }

        if (foundAtLeastOneConnection)
        {
            //if (!connectionsDrawn)
            // {
            DrawConnectors(numCoauthorsToCoauthorlist);
            connectionsDrawn = true;
            //} else
            // {
            //CleanConnections();
            //DrawConnectors(numCoauthorsToCoauthorlist);
            //}
        }
        else
        {
            Dim();
        }
    }

    /// <summary>
    /// Make edges connect from one datapoint to another based on all of the co-authors of a
    /// specific author.
    /// </summary>
    public void GetTopCoauthors(string selectedAuthor)
    {
        if (generalAuthors.Count > 0)
        {
            generalAuthors.Clear();
        }

        List<string> allCoauthors = new List<string>();

        allCoauthors.Add(selectedAuthor);
        generalAuthors.Add(selectedAuthor);

        //TODO: Might want to do find all articles in the DrawConnectors(List<string> method) to take in a list of authors
        //Add the main author's articles to the beginning of the list to have connections drawn from his/her
        //articles to all of neighboring coauthors' articles

        //TODO: Algorithm to find all coauthors from a single author
        List<string> coauthorsAdjToAuthor = DataProcessor.FindCoauthors(selectedAuthor);

        foreach (string coauthor in coauthorsAdjToAuthor)
        {
            generalAuthors.Add(selectedAuthor);
            allCoauthors.Add(coauthor);
        }

        //FIXED: Add a condition when one should draw connections and when it shouldn't happen.
        //TODO: Add a better check of distinct elements in an arraylist or array in order to get the length of
        //elements within
        if (allCoauthors.Distinct().ToArray().Length > 1)
        {
            //Debug.Log(allCoauthors[allCoauthors.Count - 1] + " Count: " + allCoauthors.Count);
            DrawConnectors(allCoauthors);
            coauthorsConnection = true;
        }
        else
        {
            Dim();
        }
    }

    /// <summary>
    /// This method will be for all articles in the dictionary, as it notifies of a relationship between all articles within the current graph.
    /// It will be used to detect author profile relationships.
    /// </summary>
    private void DrawConnectors(List<string> knownCoauthors)
    {
        GameObject lastValidGO = null;
        GameObject sourceGO = null;
        Color colorToUse = colorCategory[0];
        int num = 0;
        int coauthorCount = 0;

        for (int sourceAuthorIndex = 0; sourceAuthorIndex < knownCoauthors.Count - 1; sourceAuthorIndex++)
        {
            colorToUse = colorCategory[num % colorCategory.Length];

            List<string> sourceArticles = DataProcessor.AllArticlesFromAuthor(knownCoauthors[sourceAuthorIndex]);

            foreach (string sourceArticle in sourceArticles)
            {
                coauthorCount += DataProcessor.articleContainerDictionary[sourceArticle].Authors.Count;
                GameObject[] sourceDatapoints = DataProcessor.articleContainerDictionary[sourceArticle].MasterNodeGameObjects;

                foreach (GameObject sourceDatapoint in sourceDatapoints)
                {
                    if (sourceDatapoint != null)
                    {
                        lastValidGO = sourceDatapoint;
                        sourceGO = sourceDatapoint;
                    }
                }

                lastValidGO.GetComponent<Renderer>().material.SetColor("_EmissionColor", colorToUse);
                Vector3 sourcePos = lastValidGO.transform.localPosition;
                Vector3 worldSourcePos = lastValidGO.transform.TransformPoint(sourcePos);

                List<string> destArticles = DataProcessor.AllArticlesFromAuthor(knownCoauthors[sourceAuthorIndex + 1]); //this is the adjacent article in the list
                foreach (string destArticle in destArticles)
                {
                    //Problem is that the article is the same when one is trying to draw connections across the article
                    if (sourceArticle != destArticle)
                    {
                        coauthorCount += DataProcessor.articleContainerDictionary[destArticle].Authors.Count;
                        GameObject[] destDataPoints = DataProcessor.articleContainerDictionary[destArticle].MasterNodeGameObjects;

                        foreach (GameObject destDataPoint in destDataPoints)
                        {
                            if (destDataPoint != null)
                            {
                                lastValidGO = destDataPoint;
                            }
                        }

                        if (destDataPoints[0] != null && destDataPoints[0].transform.parent.GetComponent<SplineDecorator>())
                            destDataPoints[0].transform.parent.GetComponent<SplineDecorator>().Brighten();

                        lastValidGO.GetComponent<Renderer>().material.SetColor("_TintColor", colorToUse);
                        Vector3 destPos = lastValidGO.transform.localPosition;
                        Vector3 worldDestPos = lastValidGO.transform.TransformPoint(destPos);

                        Vector3 menuSource =
                        GameObject.FindGameObjectWithTag("Menu").transform.InverseTransformPoint(worldSourcePos);

                        if (!coauthorsConnection)
                        {
                            Transform connTrans = Instantiate(connector) as Transform;
                            connTrans.transform.rotation = GameObject.FindGameObjectWithTag("Menu").transform.localRotation;
                            print("rotation here: " + connTrans.transform.rotation.ToString());
                            connector.GetComponent<BezierSpline>().source = sourceGO.transform;
                            connector.GetComponent<BezierSpline>().destination = lastValidGO.transform;
                            connTrans.parent = GameObject.FindGameObjectWithTag("Menu").transform;
                            connTrans.parent.GetComponent<SplineDecorator>().connSplineList.Add(connTrans.GetComponent<BezierSpline>());

                            BezierSpline connSpline = connTrans.GetComponent<BezierSpline>();

                            Vector3 menuDest =
                            GameObject.FindGameObjectWithTag("Menu").transform.InverseTransformPoint(worldDestPos);

                            connTrans.transform.localPosition = Vector3.zero;
                            connSpline.points[0] = menuSource;
                            Vector3 mp1 = menuSource / 2;
                            connSpline.points[1] = mp1;
                            Vector3 mp2 = menuDest / 2;
                            connSpline.points[2] = mp2;

                            //set end position
                            connSpline.points[3] = menuDest;
                            LoadConnectors(connSpline, colorToUse, 8);
                        }
                        //Debug.Log(coauthorCount);
                    }
                }
                coauthorCount = 0;
                num++;
            }

            //TODO: Have a boolean designated to use for debugging (using Line Renderers) upon initialization for play
            //Line Renderer is only used for debugging, uncomment this and above variable declarations for LineRenderer in 
            //order to use debuggin. 

            /*GameObject childLineRenderer = new GameObject("Coauthorship Group");
            childLineRenderer.transform.parent = lineRendererContainer.transform;
            LineRenderer lineRenderer = childLineRenderer.AddComponent<LineRenderer>();
            lineRenderer.material = lineRendererMat;
            lineRenderer.useWorldSpace = true;
            lineRenderer.SetWidth(OG_SIZE * numCoauthorsToCoauthorlist[num].Count * 1.5F, OG_SIZE * numCoauthorsToCoauthorlist[num].Count * 1.5f);
            lineRenderer.SetVertexCount(allPositions.Count);
            lineRenderer.SetPositions(allPositions.ToArray());
            lineRenderer.SetColors(sameColorForBoth, sameColorForBoth);

            allPositions.Clear();*/
        }

    }

    private void CleanConnections()
    {

        List<GameObject> CDGs = new List<GameObject>();

        for (int i = 0; i < transform.childCount; i++)
        {
            if (transform.GetChild(i).tag == "CDG")
                CDGs.Add(transform.GetChild(i).gameObject);
        }

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

    /// <summary>
    /// This method will be for all articles in the dictionary, as it notifies of a relationship between all articles within the current graph.
    /// It will be used to detect author profile relationships.
    /// </summary>
    private void DrawConnectors(Dictionary<int, List<string>> numCoauthorsToCoauthorlist)
    {
        GameObject lastValidGO = null;
        Color sameColorForBoth = colorCategory[0];
        GameObject sourceGO = null;

        foreach (int num in numCoauthorsToCoauthorlist.Keys)
        {
            sameColorForBoth = colorCategory[num % colorCategory.Length];
            for (int articlePos = 0; articlePos < numCoauthorsToCoauthorlist[num].Count - 1; articlePos++)
            {
                Debug.Log("Author: " + numCoauthorsToCoauthorlist[num][articlePos] +
                ", Adjacent Author: " + numCoauthorsToCoauthorlist[num][articlePos + 1]);
                List<string> sourceArticles = DataProcessor.AllArticlesFromAuthor(numCoauthorsToCoauthorlist[num][articlePos]);

                foreach (string sourceArticle in sourceArticles)
                {
                    GameObject[] sourceDatapoints = DataProcessor.articleContainerDictionary[sourceArticle].MasterNodeGameObjects;

                    foreach (GameObject sourceDatapoint in sourceDatapoints)
                    {
                        if (sourceDatapoint != null)
                        {
                            lastValidGO = sourceDatapoint;
                            sourceGO = sourceDatapoint;
                        }
                    }

                    lastValidGO.GetComponent<Renderer>().material.SetColor("_TintColor", sameColorForBoth);
                    Vector3 sourcePos = lastValidGO.transform.localPosition;
                    Vector3 worldSourcePos = lastValidGO.transform.TransformPoint(sourcePos);

                    List<string> destArticles = DataProcessor.AllArticlesFromAuthor(numCoauthorsToCoauthorlist[num][articlePos + 1]); //this is the adjacent article in the list
                    foreach (string destArticle in destArticles)
                    {

                        if (sourceArticle != destArticle)
                        {
                            GameObject[] destDataPoints = DataProcessor.articleContainerDictionary[destArticle].MasterNodeGameObjects;

                            foreach (GameObject destDataPoint in destDataPoints)
                            {
                                if (destDataPoint != null)
                                {
                                    lastValidGO = destDataPoint;
                                }
                            }

                            if (destDataPoints[0] != null && destDataPoints[0].transform.parent.GetComponent<SplineDecorator>())
                                destDataPoints[0].transform.parent.GetComponent<SplineDecorator>().Brighten();

                            lastValidGO.GetComponent<Renderer>().material.SetColor("_TintColor", sameColorForBoth);
                            Vector3 destPos = lastValidGO.transform.localPosition;
                            Vector3 worldDestPos = lastValidGO.transform.TransformPoint(destPos);
                            
                            //allPositions.Add(worldSourcePos);
                            //allPositions.Add(worldDestPos);

                            Vector3 menuSource =
                                GameObject.FindGameObjectWithTag("Menu").transform.InverseTransformPoint(worldSourcePos);

                            if (!connectionsDrawn)
                            {
                                //connectionsDrawn = true;
                                Transform connTrans = Instantiate(connector) as Transform;
                                connector.GetComponent<BezierSpline>().source = sourceGO.transform;
                                connector.GetComponent<BezierSpline>().destination = lastValidGO.transform;
                                connTrans.parent = GameObject.FindGameObjectWithTag("Menu").transform;
                                connTrans.parent.GetComponent<SplineDecorator>().connSplineList.Add(connTrans.GetComponent<BezierSpline>());
                                BezierSpline connSpline = connTrans.GetComponent<BezierSpline>();

                                Vector3 menuDest =
                                GameObject.FindGameObjectWithTag("Menu").transform.InverseTransformPoint(worldDestPos);

                                connTrans.transform.localPosition = Vector3.zero;
                                connSpline.points[0] = menuSource;
                                Vector3 mp1 = menuSource / 2;
                                connSpline.points[1] = mp1;
                                Vector3 mp2 = menuDest / 2;
                                connSpline.points[2] = mp2;

                                //set end position
                                connSpline.points[3] = menuDest;

                                LoadConnectors(connSpline, sameColorForBoth, 8);
                            }

                        }
                    }
                }
            }
            /*
            GameObject childLineRenderer = new GameObject("Coauthorship Group");
            childLineRenderer.transform.parent = lineRendererContainer.transform;
            LineRenderer lineRenderer = childLineRenderer.AddComponent<LineRenderer>();
            lineRenderer.material = lineRendererMat;
            lineRenderer.useWorldSpace = true;
            lineRenderer.SetWidth(OG_SIZE * numCoauthorsToCoauthorlist[num].Count * 1.5F, OG_SIZE * numCoauthorsToCoauthorlist[num].Count * 1.5f);
            lineRenderer.SetVertexCount(allPositions.Count);
            lineRenderer.SetPositions(allPositions.ToArray());
            lineRenderer.SetColors(sameColorForBoth, sameColorForBoth);

            allPositions.Clear();
            */
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
            splineConn.points[0] = menuSource;
            Vector3 mp1 = menuSource / 2;
            splineConn.points[1] = mp1;
            Vector3 mp2 = menuDest / 2;
            splineConn.points[2] = mp2;

            //set end position
            splineConn.points[3] = menuDest;

            float stepSize = 1f / (8);
            for (int p = 0, f = 0; f < 8; f++)
            {
                for (int i = 0; i < connectGuide.Length; i++, p++)
                {
                    Transform item = splineConn.transform.GetChild(f);
                    item.gameObject.GetComponentInChildren<Renderer>().material.SetColor("_EmissionColor", Color.red);
                    Vector3 position = splineConn.GetPoint(p * stepSize);
                    item.transform.position = position;

                    item.transform.LookAt(position + splineConn.GetDirection(p * stepSize));
                }
            }

        }
    }
    
    private void LoadConnectors(BezierSpline splineConn, Color splineColor, int freq)
    {
        float stepSize = 1f / (freq);
        for (int p = 0, f = 0; f < freq; f++)
        {
            for (int i = 0; i < connectGuide.Length; i++, p++)
            {
                Transform item = Instantiate(connectGuide[i]);
                item.gameObject.GetComponentInChildren<Renderer>().material.SetColor("_EmissionColor", splineColor);
                Vector3 position = splineConn.GetPoint(p * stepSize);
                item.transform.localPosition = position;

                item.transform.LookAt(position + splineConn.GetDirection(p * stepSize));

                item.transform.parent = splineConn.transform;
            }
        }
    }
    
    private void LoadConnectors(BezierSpline splineConn, Color splineColor, int freq, int scaleNumber)
    {
        float stepSize = 1f / (freq);
        for (int p = 0, f = 0; f < freq; f++)
        {
            for (int i = 0; i < connectGuide.Length; i++, p++)
            {
                Transform item = Instantiate(connectGuide[i]);
                item.gameObject.GetComponentInChildren<Renderer>().material.SetColor("_EmissionColor", splineColor);
                Vector3 position = splineConn.GetPoint(p * stepSize);
                item.transform.localPosition = position;

                item.transform.LookAt(position + splineConn.GetDirection(p * stepSize));

                item.transform.parent = splineConn.transform;

                item.transform.localScale = new Vector3(scaleNumber, 1, 1);
            }
        }
    }

    /// <summary>
    /// This method will be for all articles in the dictionary, as it notifies of a relationship between all articles within the current graph.
    /// It will be used to detect author profile relationships.
    /// </summary>
    private void DrawConnectors(List<AuthorData> sourceADs, List<AuthorData> destADs, string inputCategory, string category)
    {
        GameObject lastValidGO = null;

        List<string> sourceAuthors = new List<string>();
        List<string> destAuthors = new List<string>();

        foreach (AuthorData sourceAD in sourceADs)
        {

            List<string> sourceArticles = DataProcessor.AllArticlesFromAuthor(sourceAD.Author, inputCategory);
            foreach (string sourceArticle in sourceArticles)
            {

                GameObject[] sourceDatapoints = DataProcessor.articleContainerDictionary[sourceArticle].MasterNodeGameObjects;

                foreach (AuthorData destAD in destADs)
                {
                    if (sourceAD.Author != destAD.Author)
                    {
                        foreach (GameObject sourceDataPoint in sourceDatapoints)
                        {
                            if (sourceDataPoint != null)
                            {
                                sourceDataPoint.GetComponent<Renderer>().material.SetColor("_TintColor", Color.yellow);
                                lastValidGO = sourceDataPoint;
                            }
                        }
                        //if (sourceDatapoints[0])
                        //if (sourceDatapoints[0].transform.parent.GetComponent<SplineDecorator>())
                        //	sourceDatapoints[0].transform.parent.GetComponent<SplineDecorator>().Brighten ();

                        Vector3 sourcePos = lastValidGO.transform.localPosition;
                        Vector3 worldSourcePos = lastValidGO.transform.TransformPoint(sourcePos);

                        sourceAuthors.Add(sourceAD.Author);
                        destAuthors.Add(destAD.Author);

                        List<string> destArticles = DataProcessor.AllArticlesFromAuthor(destAD.Author, category);
                        foreach (string destArticle in destArticles)
                        {

                            if (sourceArticle != destArticle)
                            {
                                GameObject[] destDataPoints = DataProcessor.articleContainerDictionary[destArticle].MasterNodeGameObjects;

                                foreach (GameObject destDataPoint in destDataPoints)
                                {
                                    if (destDataPoint != null)
                                    {
                                        destDataPoint.GetComponent<Renderer>().material.SetColor("_TintColor", Color.red);
                                        lastValidGO = destDataPoint;
                                    }
                                }
                                if (destDataPoints[0])
                                    if (destDataPoints[0].transform.parent.GetComponent<SplineDecorator>())
                                        destDataPoints[0].transform.parent.GetComponent<SplineDecorator>().Brighten();

                                Vector3 destPos = lastValidGO.transform.localPosition;
                                Vector3 worldDestPos = lastValidGO.transform.TransformPoint(destPos);

                                allPositions.Add(worldSourcePos);
                                allPositions.Add(worldDestPos);
                            }
                        }
                    }
                }
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
        Dim();
    }

    public List<string> SourceAuthors
    {
        get { return this.algorithmSourceAuthors; }
        set { algorithmSourceAuthors = value; }
    }

    public List<string> DestAuthors
    {
        get { return this.algorithmDestAuthors; }
        set { algorithmDestAuthors = value; }
    }

    public List<string> GeneralAuthors
    {
        get { return this.generalAuthors; }
        set { generalAuthors = value; }
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
