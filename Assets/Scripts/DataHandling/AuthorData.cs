using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AuthorData {

	public Dictionary<string, int> categoryToNumArticles = new Dictionary<string, int>();
	private string author;
    private int numberOfCoauthors;

	public AuthorData(string author, string category) {
		this.author = author;

		//TODO: Do something with category
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
