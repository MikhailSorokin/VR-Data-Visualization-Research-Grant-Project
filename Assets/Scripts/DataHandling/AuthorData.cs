using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AuthorData {

	public Dictionary<string, int> categoryToNumArticles = new Dictionary<string, int>();
    public Dictionary<GameObject, List<string>> goLocations = new Dictionary<GameObject, List<string>>();

	private string author;
    private int numberOfCoauthors;

	public AuthorData(string author, string category, int typeDatabase) {
		this.author = author;

		/*This is for the DBLP database section.*/
		//TODO: Do something with category
		if (typeDatabase == 0) {
			categoryToNumArticles ["Uncategorized"] = 0;
			categoryToNumArticles ["VR"] = 0;
			categoryToNumArticles ["A.I."] = 0;
			categoryToNumArticles ["Robotics"] = 0;
			categoryToNumArticles ["Graphics"] = 0;
			categoryToNumArticles ["Algorithms"] = 0;
			categoryToNumArticles ["BioInformatics"] = 0;
			categoryToNumArticles ["Data Visualization"] = 0;
			categoryToNumArticles ["Numerical Analysis"] = 0;
			categoryToNumArticles ["Scientific Computations"] = 0;
			categoryToNumArticles ["Programming Languages"] = 0;
			categoryToNumArticles ["Game Development"] = 0;
			categoryToNumArticles ["Cybersecurity"] = 0;
			categoryToNumArticles ["Machine Learning"] = 0;
			categoryToNumArticles ["Networking"] = 0;
			categoryToNumArticles ["Human Computer Interaction"] = 0;
			categoryToNumArticles ["Databases"] = 0;
		} else if (typeDatabase == 1) {
			/*This is for the Movies section.*/
			categoryToNumArticles ["Batman"] = 0;
			categoryToNumArticles ["Superman"] = 0;
			categoryToNumArticles ["Iron Man"] = 0;
		}
	}

	public string Author {
		get { return author; }
	}

	public int RetrieveNum(string category) {
		return categoryToNumArticles [category];
	}

    public int CoauthorNum
    {
        get { return numberOfCoauthors; }
        set { numberOfCoauthors = value; }
    }

    public void UpdateCategory(string category) {
		categoryToNumArticles [category] += 1;
	}
	
}
