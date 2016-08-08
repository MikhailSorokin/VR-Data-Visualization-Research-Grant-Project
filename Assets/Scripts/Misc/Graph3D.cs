using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class Graph3D : MonoBehaviour {

	public Vector3 bounds;
	public Transform[] lineGuide;
	public BezierSpline graphSplines;
	public List<NodeGroup> dataAdded;
	public GameObject spawnObj;

	private Dictionary <string, float> authorAxisPos;
	private Dictionary <string, float> articleStrengthAxisPos;
	private Dictionary <string, float> articleDateAxisPos;

	private Dictionary <int, List<int>> decadeDictionary;
	private Dictionary <string, List<int>> authorDictionary;

	// Use this for initialization
	void Start () {

		authorDictionary = new Dictionary<string, List<int>>();
		decadeDictionary = new Dictionary<int, List<int>>();

		authorAxisPos = new Dictionary <string, float>();
		articleStrengthAxisPos = new Dictionary <string, float>();
		articleDateAxisPos = new Dictionary <string, float>();
		//Represents the values of (< than 1960), (< than 1970), (< than 1980), (< than 1990), (< than 2000), (< than 2010), and (< than 2020)

		
		decadeDictionary.Add(70, new List<int>());
		decadeDictionary.Add(80, new List<int>());
		decadeDictionary.Add(90, new List<int>());
		decadeDictionary.Add(100, new List<int>());
		decadeDictionary.Add(110, new List<int>());
		decadeDictionary.Add(120, new List<int>());

		CreatGraph ();
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	void CreatGraph() {

		Transform guide = Instantiate(lineGuide[0]) as Transform;
		guide.parent = transform;
		guide.localPosition = new Vector3(-bounds.x, -bounds.y, -bounds.z);
		guide.localScale = new Vector3 (1.0f, 1.0f, bounds.z * 20.0f);

		guide = Instantiate(lineGuide[0]) as Transform;
		guide.parent = transform;
		guide.localPosition = new Vector3(-bounds.x, -bounds.y, -bounds.z);
		guide.Rotate (Vector3.left * 90.0f);
		guide.localScale = new Vector3 (1.0f, 1.0f, bounds.z * 20.0f);

		guide = Instantiate(lineGuide[0]) as Transform;
		guide.parent = transform;
		guide.localPosition = new Vector3(-bounds.x, -bounds.y, -bounds.z);
		guide.Rotate (Vector3.up * 90.0f);
		guide.localScale = new Vector3 (1.0f, 1.0f, bounds.z * 20.0f);

		guide = Instantiate(lineGuide[0]) as Transform;
		guide.parent = transform;
		guide.localPosition = new Vector3(-bounds.x, -bounds.y, bounds.z);
		guide.Rotate (Vector3.up * 90.0f);
		guide.localScale = new Vector3 (1.0f, 1.0f, bounds.z * 20.0f);

		guide = Instantiate(lineGuide[0]) as Transform;
		guide.parent = transform;
		guide.localPosition = new Vector3(bounds.x, -bounds.y, -bounds.z);
		guide.Rotate (Vector3.left * 90.0f);
		guide.localScale = new Vector3 (1.0f, 1.0f, bounds.z * 20.0f);

		guide = Instantiate(lineGuide[0]) as Transform;
		guide.parent = transform;
		guide.localPosition = new Vector3(bounds.x, -bounds.y, bounds.z);
		guide.Rotate (Vector3.left * 90.0f);
		guide.localScale = new Vector3 (1.0f, 1.0f, bounds.z * 20.0f);

		guide = Instantiate(lineGuide[0]) as Transform;
		guide.parent = transform;
		guide.localPosition = new Vector3(-bounds.x, -bounds.y, bounds.z);
		guide.Rotate (Vector3.left * 90.0f);
		guide.localScale = new Vector3 (1.0f, 1.0f, bounds.z * 20.0f);

		guide = Instantiate(lineGuide[0]) as Transform;
		guide.parent = transform;
		guide.localPosition = new Vector3(-bounds.x, bounds.y, -bounds.z);
		guide.Rotate (Vector3.up * 90.0f);
		guide.localScale = new Vector3 (1.0f, 1.0f, bounds.z * 20.0f);

		guide = Instantiate(lineGuide[0]) as Transform;
		guide.parent = transform;
		guide.localPosition = new Vector3(-bounds.x, bounds.y, bounds.z);
		guide.Rotate (Vector3.up * 90.0f);
		guide.localScale = new Vector3 (1.0f, 1.0f, bounds.z * 20.0f);

		guide = Instantiate(lineGuide[0]) as Transform;
		guide.parent = transform;
		guide.localPosition = new Vector3(bounds.x, -bounds.y, -bounds.z);
		guide.localScale = new Vector3 (1.0f, 1.0f, bounds.z * 20.0f);

		guide = Instantiate(lineGuide[0]) as Transform;
		guide.parent = transform;
		guide.localPosition = new Vector3(bounds.x, bounds.y, -bounds.z);
		guide.localScale = new Vector3 (1.0f, 1.0f, bounds.z * 20.0f);

		guide = Instantiate(lineGuide[0]) as Transform;
		guide.parent = transform;
		guide.localPosition = new Vector3(-bounds.x, bounds.y, -bounds.z);
		guide.localScale = new Vector3 (1.0f, 1.0f, bounds.z * 20.0f);


	}

	public void addInData ()
	{
		foreach (NodeGroup x in dataAdded)
		{
			//TODO: add the information for the author, year and title 
			string author = x.Node_Author;
			List<string> articles = x.Node_Titles;
			List<int> years = x.Node_Years;
			//years and title should have same amount of data in each var, if not throw an error

			//articleYears [title] = year;
			//articleAuthors [title] = new List<string>(authors);
			//Access the SplineDecoratorAxis somehow - SplineDecoratorAxis.AddData(year);

			if (articles.Count == years.Count) {
				int totalElements = articles.Count;

				//Need to get the most recent index from the author since we don't want to add his previous article titles and years
				for (int nodeGroupIndex = x.Start_Index; nodeGroupIndex < totalElements; nodeGroupIndex++) {
					if (years [nodeGroupIndex] < 1970) {
						decadeDictionary [70].Add (years [nodeGroupIndex]);
					} else if (years [nodeGroupIndex] < 1980) {
						decadeDictionary [80].Add (years [nodeGroupIndex]);
					} else if (years [nodeGroupIndex] < 1990) {
						decadeDictionary [90].Add (years [nodeGroupIndex]);
					} else if (years [nodeGroupIndex] < 2000) {
						decadeDictionary [100].Add (years [nodeGroupIndex]);
					} else if (years [nodeGroupIndex] < 2010) {
						decadeDictionary [110].Add (years [nodeGroupIndex]);
					} else if (years [nodeGroupIndex] < 2020) {
						decadeDictionary [120].Add (years [nodeGroupIndex]);
					} else {
						Debug.LogError ("I see you are from the FUTURE, well ... an article with this year doesn't exist yet, silly!");
					}

					//Debug.Log (authors.Count);
					// adds an author year of the article
					float authorAxis = 0.0f;

					if (authorDictionary.ContainsKey(author))
					{
						authorDictionary[author].Add(years [nodeGroupIndex]);
						authorAxis = authorAxisPos [author];
					} else
					{
						authorDictionary.Add(author, new List<int>());
						authorDictionary[author].Add(years [nodeGroupIndex]);
						print ("Author: " + author + " wrote " + articles[nodeGroupIndex] + ".");
						authorAxis = (Random.value * 2.0f - 1.0f) * bounds.y;
						authorAxisPos [author] = authorAxis;
					}

					float strengthAxis = (Random.value * 2.0f - 1.0f) * bounds.z;
					articleStrengthAxisPos [articles[nodeGroupIndex]] = strengthAxis;

					float yearAxisVal = (((2016 - years[nodeGroupIndex]) / 56.0f) * 2.0f - 1.0f) * bounds.x;
					articleDateAxisPos [articles[nodeGroupIndex]] = yearAxisVal;

					GameObject o = Instantiate(spawnObj);
					o.transform.parent = transform;
					o.transform.localPosition = new Vector3 (yearAxisVal, authorAxisPos [author], strengthAxis);
				}
			} else {
				Debug.LogError ("Articles and years should be the same length. Check EfficientArticleLoader class and see if there are any problems in parsing in the data.");
			}
		}
	}
}
