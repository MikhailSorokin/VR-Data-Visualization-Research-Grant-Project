using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//Wiil contain the Nodes, which will be instantiated throughout the scene
public class NodeGroup
{
	private Dictionary<string, List<string>> categoryNamesToArticles = new Dictionary<string, List<string>>();
	/*Number of Articles*/
	private int numDefaultArticles; //no specific category goes into this count.
	private int numVRARArticles; 
	private int numArtificialIntelligenceArticles; 
	private int numRobotArticles; 
	private int numGraphicsArticles; 
	private int numAlgorithmArticles; 
	private int numBioInformaticsArticles; 
	private int numDataVisArticles; 
	private int numNumericalAnalysisArticles; 
	private int numScientificComputingArticles; 
	private int numProgrammingLanguagesArticles;
	private int numGameDevArticles;
	private int numCyberSecurityArticles;
	private int numMachineLearningArticles;
	private int numNetworkingArticles; 
	private int numDatabasesArticles;

	private int numTotalArticles;

	/*Variables containing information pertinent to finding out an author's information.*/
	private List<string> titles;
	private List<string> urls;
	private List<int> years;
	private string author;
	private int startIndex; //startIndex is to be used in classes which need information to get the latest author information, not repeat his old information

	public NodeGroup(string article_author, List<string> article_titles, List<int> article_years)
    {
        author = article_author;
        titles = article_titles;
		years = article_years;
		startIndex = 0;

		numDefaultArticles = 0;
		numVRARArticles = 0;
		numArtificialIntelligenceArticles = 0;
		numRobotArticles = 0;
		numGraphicsArticles = 0;
		numAlgorithmArticles = 0;
		numBioInformaticsArticles = 0;
		numDataVisArticles = 0;
		numNumericalAnalysisArticles = 0;
		numScientificComputingArticles = 0;
		numProgrammingLanguagesArticles = 0;
		numGameDevArticles = 0;
		numCyberSecurityArticles = 0;
		numMachineLearningArticles = 0;
		numNetworkingArticles = 0;

		categoryNamesToArticles ["Uncategorized"] = new List<string> ();
		categoryNamesToArticles ["VRAR"] = new List<string> ();
		categoryNamesToArticles ["AI"] = new List<string> ();
		categoryNamesToArticles ["Robot"] = new List<string> ();
		categoryNamesToArticles ["Graphics"] = new List<string> ();
		categoryNamesToArticles ["Algorithms"] = new List<string> ();
		categoryNamesToArticles ["Bio Information"] = new List<string>();
		categoryNamesToArticles ["Data Vis"] = new List<string> ();
		categoryNamesToArticles ["Numerical Analysis"] = new List<string> ();
		categoryNamesToArticles ["Scientific Computing"] = new List<string> ();
		categoryNamesToArticles ["Programming Languages"] = new List<string> ();
		categoryNamesToArticles ["Game Dev"] = new List<string> ();
		categoryNamesToArticles ["Cyber Security"] = new List<string> ();
		categoryNamesToArticles ["Machine Learning"] = new List<string> ();
		categoryNamesToArticles ["Networking"] = new List<string> ();
		categoryNamesToArticles ["Database"] = new List<string> ();
    }


	public NodeGroup(string article_author, List<string> article_titles, List<string> article_URLs, List<int> article_years)
    {
        author = article_author;
        titles = article_titles;
        years = article_years;
        urls = article_URLs;
		startIndex = 0;

		numDefaultArticles = 0;
		numVRARArticles = 0;
		numArtificialIntelligenceArticles = 0;
		numRobotArticles = 0;
		numGraphicsArticles = 0;
		numAlgorithmArticles = 0;
		numBioInformaticsArticles = 0;
		numDataVisArticles = 0;
		numNumericalAnalysisArticles = 0;
		numScientificComputingArticles = 0;
		numProgrammingLanguagesArticles = 0;
		numGameDevArticles = 0;
		numCyberSecurityArticles = 0;
		numMachineLearningArticles = 0;
		numNetworkingArticles = 0;
    }

	public string Node_Author
	{
		get { return author; }
	}

    public List<string> Node_Titles
    {
        get { return titles; }
    }

	public List<int> Node_Years
    {
        get { return years; }
    }

	public List<string> Node_URLs
    {
        get { return urls; }
    }

	public int Start_Index {
		get { return startIndex; }
	}

	public void UpdateUncategorizedArticles(string article) {
		/*if (!categoryNamesToArticles.ContainsKey ("Uncategorized")) { 
			categoryNamesToArticles ["Uncategorized"] = new List<string> ();
		}*/
		categoryNamesToArticles["Uncategorized"].Add(article);
	}

	/*Getters and setters for specific article numbers of various categories.*/
	public int DefaultArticles {
		get { return numDefaultArticles; }
		set { numDefaultArticles = value; }
	}

	public void UpdateVRARArticles(string article) {
		categoryNamesToArticles["VRAR"].Add(article);
	}

	public int VRARArticles {
		get { return numVRARArticles; }
		set { numVRARArticles = value; }
	}

	public void UpdateAIArticles(string article) {
		categoryNamesToArticles["AI"].Add(article);
	}

	public int AIArticles {
		get { return numArtificialIntelligenceArticles; }
		set { numArtificialIntelligenceArticles = value; }
	}

	public void UpdateRobotArticles(string article) {
		categoryNamesToArticles["Robot"].Add(article);
	}

	public int RobotArticles {
		get { return numRobotArticles; }
		set { numRobotArticles = value; }
	}

	public void UpdateGraphicArticles(string article) {
		categoryNamesToArticles["Graphics"].Add(article);
	}

	public int GraphicArticles {
		get { return numGraphicsArticles; }
		set { numGraphicsArticles = value; }
	}

	public void UpdateAlgArticles(string article) {
		categoryNamesToArticles["Algorithms"].Add(article);
	}

	public int AlgArticles {
		get { return numAlgorithmArticles; }
		set { numAlgorithmArticles = value; }
	}

	public void UpdateBioArticles(string article) {
		categoryNamesToArticles["Bio Information"].Add(article);
	}

	public int BioArticles {
		get { return numBioInformaticsArticles; }
		set { numBioInformaticsArticles = value; }
	}

	public void UpdateDataVisArticles(string article) {
		categoryNamesToArticles["Data Vis"].Add(article);
	}

	public int DataVisArticles {
		get { return numDataVisArticles; }
		set { numDataVisArticles = value; }
	}

	public void UpdateNumericalAnalysisArticles(string article) {
		categoryNamesToArticles["Numerical Analysis"].Add(article);
	}

	public int NumericalArticles {
		get { return numNumericalAnalysisArticles; }
		set { numNumericalAnalysisArticles = value; }
	}

	public void UpdateScientificComputationArticles(string article) {
		categoryNamesToArticles["Scientific Computing"].Add(article);
	}

	public int ScienceComputationArticles {
		get { return numScientificComputingArticles; }
		set { numScientificComputingArticles = value; }
	}

	public void UpdatePLArticles(string article) {
		categoryNamesToArticles["Programming Languages"].Add(article);
	}

	public int ProgrammingLanguagesArticles {
		get { return numProgrammingLanguagesArticles; }
		set { numProgrammingLanguagesArticles = value; }
	}

	public void UpdateGameDevArticles(string article) {
		categoryNamesToArticles["Game Dev"].Add(article);
	}

	public int GameDevArticles {
		get { return numGameDevArticles; }
		set { numGameDevArticles = value; }
	}

	public void UpdateCyberSecurityArticles(string article) {
		categoryNamesToArticles["Cyber Security"].Add(article);
	}

	public int CybersecurityArticles {
		get { return numCyberSecurityArticles; }
		set { numCyberSecurityArticles = value; }
	}

	public void UpdateMachineLearningArticles(string article) {
		categoryNamesToArticles["Machine Learning"].Add(article);
	}

	public int MachineLearningArticles {
		get { return numMachineLearningArticles; }
		set { numMachineLearningArticles = value; }
	}

	public void UpdateNetworkingArticles(string article) {
		categoryNamesToArticles["Networking"].Add(article);
	}

	public int NetworkingArticles {
		get { return numNetworkingArticles; }
		set { numNetworkingArticles = value; }
	}

	public void UpdateDatabaseArticles(string article) {
		categoryNamesToArticles["Database"].Add(article);
	}

	public Dictionary<string, List<string>> CategorizedArticles() {
		return categoryNamesToArticles;
	}

	public int DatabaseArticles {
		get { return numDatabasesArticles; }
		set { numDatabasesArticles = value; }
	}

	public int TotalArticles {
		get { return numTotalArticles; }
		set { numTotalArticles = value; }
	}

	/*This method updates all of the titles, urls and years of an individual article.*/
	public void UpdateAuthor(string title, int year) {
		titles.Add (title);
		years.Add (year);
		startIndex++;
	}

	public void UpdateAuthor(string title, string url, int year) {
		titles.Add (title);
		urls.Add (url);
		years.Add (year);
		startIndex++;
	}

}
