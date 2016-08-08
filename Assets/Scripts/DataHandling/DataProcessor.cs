using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;
using UnityEngine;

/// <summary>
/// This class handles taking in all information and sending all information.
/// </summary>
public static class DataProcessor {

	public static Dictionary<string, MasterNode> articleContainerDictionary = new Dictionary<string, MasterNode>(); //holding everything you dream of! except for specific Author related things
	public static List<string> uniqueAuthors = new List<string>();
	public static List<AuthorData> allAuthorData = new List<AuthorData>();

	private static List<MasterNode> tempClusterMasterNodes = new List<MasterNode>();
	private static int count = 0;

	public static void ReadAndProcessData(int sizeOfClusterRead)
	{
		IEnumerable<string> node_authors;
		string node_title, node_year, node_conference;
		while (count < sizeOfClusterRead)
		{
			node_conference = XmlFileTrace.ConferenceEnumerator.GetNextXMLAttribute (); //this is extranced from the url
			string patternToMatch = @"db/journals/([a-z]+)/(\w|\W)*";
			Match regexMatch = new Regex (patternToMatch).Match (node_conference);
			node_conference = regexMatch.Groups[1].Value.ToUpper(); //IMPORTANT - First regex match in a group is at index 1, NOT 0

			node_title = XmlFileTrace.TitlesEnumerator.GetNextXMLAttribute();

			node_authors = XmlFileTrace.AuthorsEnumerator.GetNextXMLAttribute();
			List<string> list_node_authors = node_authors.ToList();

			int node_year_int = 0;
			node_year = XmlFileTrace.YearsEnumerator.GetNextXMLAttribute();

			//Convert the string to an int
			if (int.TryParse(node_year, out node_year_int))
			{
				string categoryOfArticle = CurrentArticleCategory (node_title.ToLower ());

				MasterNode masterNode = new MasterNode(list_node_authors, node_title, node_conference, node_year_int, categoryOfArticle); 

				tempClusterMasterNodes.Add(masterNode);
				if (!articleContainerDictionary.ContainsKey (node_title)) {
					articleContainerDictionary [node_title] = masterNode; //there should always be a unique article
				} else
					articleContainerDictionary ["Duplicate: " + node_title] = masterNode;//throw new Exception("There is a duplicate article in this XML. Please get a new XML file! The Article is: " + node_title);

				//This is for getting individual author information, important for knowing how many articles an article has publichsed within a category
				foreach (string author in list_node_authors) {
					if (!uniqueAuthors.Contains (author)) {
						uniqueAuthors.Add (author);
						AuthorData ad = new AuthorData (author, categoryOfArticle);
						ad.UpdateCategory (categoryOfArticle);
                        ad.CoauthorNum += (list_node_authors.Count - 1); //do minus one so we do not include the current author as well
                        allAuthorData.Add (ad);
					} else {
						AuthorData updatedAuthorData = GetADFromAuthor (author);
						updatedAuthorData.UpdateCategory (categoryOfArticle);
                        updatedAuthorData.CoauthorNum += (list_node_authors.Count - 1); //-1 for reason specified above
					}
				}

				categoryOfArticle = null;
				count++; //increment to the next element in the cluster
			}
			else throw new InvalidOperationException("Could not parse the string year to an integer for some reason!");
		}

		count = 0;
	}

	public static AuthorData GetADFromAuthor(string author) {
		foreach (AuthorData aad in allAuthorData) {
			if (aad.Author == author) {
				return aad;
			}
		}
		return null;
	}

	public static List<AuthorData> GetADsFromAuthors(List<string> authors) {
		List<AuthorData> wantedADs = new List<AuthorData>(); 
		foreach(AuthorData aad in allAuthorData) {
			if (authors.Contains(aad.Author)) {
				wantedADs.Add(aad);
			}
		}
		return wantedADs;
	}
	/// <summary>
	/// Helper method to find the current category of a particular article.
	/// </summary>
	/// <returns>The article category associated with the article.</returns>
	/// <param name="lowerCaseTitle"A lower case title of the original inputted article. This is done to match keywords easier.</param>
	private static string CurrentArticleCategory(string lowerCaseTitle) {
		bool addedArticle = false;
		string categoryOfArticle = "";

		/* Everything should be sorted according to how many keywords there are for a specific category.*/

		//Add to the category of Programming Languages.
		if (lowerCaseTitle.Contains ("java") || lowerCaseTitle.Contains ("ruby") || lowerCaseTitle.Contains ("python")
			|| lowerCaseTitle.Contains ("language") || lowerCaseTitle.Contains ("algol") || lowerCaseTitle.Contains ("automata")
			|| lowerCaseTitle.Contains ("prolog") || lowerCaseTitle.Contains ("fortran") || lowerCaseTitle.Contains ("ocaml")
			|| lowerCaseTitle.Contains ("perl") || lowerCaseTitle.Contains ("grammar") || lowerCaseTitle.Contains("finite")
			|| lowerCaseTitle.Contains("context")) {
			categoryOfArticle = "Programming Languages";
			addedArticle = true;
		}

		//Add to the category of Algorithms.
		else if (lowerCaseTitle.Contains ("sort") || lowerCaseTitle.Contains ("algorithm") || lowerCaseTitle.Contains ("tree")
			|| lowerCaseTitle.Contains ("proof") || lowerCaseTitle.Contains ("graph")) {
			categoryOfArticle = "Algorithms";
			addedArticle = true;
		}

		//Add to the category of Numerical Analysis.
		else if (lowerCaseTitle.Contains ("numerical") || lowerCaseTitle.Contains ("math") || lowerCaseTitle.Contains ("algebra")
			|| lowerCaseTitle.Contains ("calculus") || lowerCaseTitle.Contains("integer") || lowerCaseTitle.Contains("power")
			|| lowerCaseTitle.Contains("multi")) {
			categoryOfArticle = "Numerical Analysis";
			addedArticle = true;
		}

		//Add to the category of VR.
		else if (lowerCaseTitle.Contains ("virtual")
			|| lowerCaseTitle.Contains ("oculus")
			|| lowerCaseTitle.Contains ("head mounted")
			|| lowerCaseTitle.Contains ("augmented")) {
			categoryOfArticle = "VR";
			addedArticle = true;
		}

		//Add to the category of H.C.I.
		else if (lowerCaseTitle.Contains ("society") || lowerCaseTitle.Contains("human") || lowerCaseTitle.Contains("unity")) {
			categoryOfArticle = "Human Computer Interaction";
			addedArticle = true;
		}

		//Add to the category of Game Development.
		else if (lowerCaseTitle.Contains (" unity ") || lowerCaseTitle.Contains ("game engine") || lowerCaseTitle.Contains("game")) {
			categoryOfArticle = "Game Development";
			addedArticle = true;
		}

		//Add to the category of Cybersecurity.
		else if (lowerCaseTitle.Contains ("security") || lowerCaseTitle.Contains ("firewall") || lowerCaseTitle.Contains ("protocol")) {
			categoryOfArticle = "Cybersecurity";
			addedArticle = true;
		}

		//Add to the category of Networking.
		else if (lowerCaseTitle.Contains ("network") || lowerCaseTitle.Contains ("packet") || lowerCaseTitle.Contains ("socket")) {
			categoryOfArticle = "Networking";
			addedArticle = true;
		}

		//Add to the category of AI.
		else if (lowerCaseTitle.Contains ("autono") || lowerCaseTitle.Contains ("artificial intelligence")) {
			categoryOfArticle = "A.I.";
			addedArticle = true;
		}

		//Add to the category of Robotics.
		else if (lowerCaseTitle.Contains ("robot") || lowerCaseTitle.Contains ("drone")) {
			categoryOfArticle = "Robotics";
			addedArticle = true;
		} 

		//Add to the category of BioInformatics.
		else if (lowerCaseTitle.Contains ("bio") || lowerCaseTitle.Contains ("cell")) {
			categoryOfArticle = "BioInformatics";
			addedArticle = true;
		}

		//Add to the category of Scientific Computations.
		else if (lowerCaseTitle.Contains ("science") || lowerCaseTitle.Contains ("physic")) {
			categoryOfArticle = "Scientific Computations";
			addedArticle = true;
		}

		//Add to the category of Graphics.
		else if (lowerCaseTitle.Contains ("graphic")) {
			categoryOfArticle = "Graphics";
			addedArticle = true;
		}
			
		//Add to the category of Databases.
		else if (lowerCaseTitle.Contains ("database")) {
			categoryOfArticle = "Databases";
			addedArticle = true;
		}

		//Add to the category of Data Visualization.
		else if (lowerCaseTitle.Contains ("data")) {
			categoryOfArticle = "Data Visualization";
			addedArticle = true;
		}

		//Add to the category of Machine Learning.
		else if (lowerCaseTitle.Contains ("machine")) {
			categoryOfArticle = "Machine Learning";
			addedArticle = true;
		}

		// If no article has been added, go to default case and add an "Uncategorized" category
		if (!addedArticle) {
			categoryOfArticle = "Uncategorized";
			addedArticle = true;
		} /*else {
			throw new Exception ("Was not able to categorize some article, something is wrong!");
		}*/

		return categoryOfArticle;
	}

	/// <summary>
	/// This is just a simple getter for all the masterNodes
	/// </summary>
	/// <returns>Returns the list of all master nodes.</returns>
	public static List<MasterNode> GetAllMasterNodes()
	{
		return tempClusterMasterNodes;
	}

	public static void ClearTempCluster()
	{
		tempClusterMasterNodes.Clear();
	}

	/// <summary>
	/// Returns a list of all the article strings from a single author.
	/// </summary>
	/// <returns>The articles from single author.</returns>
	/// <param name="inputAuthor">Input author.</param>
	public static List<string> AllArticlesFromAuthor(string inputAuthor)
	{
		List<string> articlesFromAuthor = new List<string>();
		try
		{
			//Originally had a list in here, but an articleToMasterNode KeyValue pair should avoid creating more memory
			foreach (KeyValuePair<string, MasterNode> articleToMasterNode in articleContainerDictionary)
			{
				if (articleToMasterNode.Value.Authors.Contains(inputAuthor))
				{
					articlesFromAuthor.Add(articleToMasterNode.Key);
				}
			}
		}
		catch (NullReferenceException e)
		{
			Console.Write(e.StackTrace);
		}

		return articlesFromAuthor;
	}

	/// <summary>
	/// Returns a list of all the article strings from a single author.
	/// </summary>
	/// <returns>The articles from single author.</returns>
	/// <param name="inputAuthor">Input author.</param>
	public static List<string> AllArticlesFromAuthor(string inputAuthor, string category)
	{
		List<string> articlesFromAuthor = new List<string>();
		try
		{
			//Originally had a list in here, but an articleToMasterNode KeyValue pair should avoid creating more memory
			foreach (KeyValuePair<string, MasterNode> articleToMasterNode in articleContainerDictionary)
			{
				if (articleToMasterNode.Value.Authors.Contains(inputAuthor) && articleToMasterNode.Value.Category == category)
				{
					articlesFromAuthor.Add(articleToMasterNode.Key);
				}
			}
		}
		catch (NullReferenceException e)
		{
			Console.Write(e.StackTrace);
		}

		return articlesFromAuthor;
	}

	/// <summary>
	/// This method gets the top five neighbors of the related parameter based on the totalNumber of articles
	/// between the parameter and a specific edge, as well as determining if both vertexes are in the same
	/// category or not.
	/// 
	/// O(n^2) right now, boo!!! 
	/// 
	/// TODO: Fix it so that it is not O(n^2).
	/// 
	/// </summary>
	/// <param name="rootVertex"></param>
	/// <returns></returns>
	public static Dictionary<string[], string[]> GetTopNeighbors(string inputCategory)
	{
		int highestSimilarity = 0;
		//List<string[]> topNeighborsList = new List<string[]>();
		Dictionary<string[], string[]> topAuthorsToArticleDictionary = new Dictionary<string[], string[]>();

		foreach (KeyValuePair<string, MasterNode> articleToMasterNode1 in articleContainerDictionary)
		{
			foreach (string authorOne in articleToMasterNode1.Value.Authors)
			{
				int cntAuthorOne = CountHelper(authorOne, inputCategory);

				//If we know that this author has more than one article, he may be related to another author elsewhere
				if (cntAuthorOne > 1)
				{
					foreach (KeyValuePair<string, MasterNode> articleToMasterNode2 in articleContainerDictionary)
					{
						foreach (string authorTwo in articleToMasterNode2.Value.Authors)
						{
							string[] seeIfEdgeOneExists = new string[2];
							seeIfEdgeOneExists[0] = authorOne;
							seeIfEdgeOneExists[1] = authorTwo;

							string[] seeIfEdgeTwoExists = new string[2];
							seeIfEdgeTwoExists[0] = authorTwo;
							seeIfEdgeTwoExists[1] = authorOne;

							if (authorOne != authorTwo && articleToMasterNode2.Value.Category != inputCategory && articleToMasterNode2.Value.Category != "Uncategorized"
								&& (!topAuthorsToArticleDictionary.Values.Contains(seeIfEdgeOneExists) || !topAuthorsToArticleDictionary.Values.Contains(seeIfEdgeTwoExists))
							)
							{
								int cntAuthorTwo = CountHelper(authorTwo, inputCategory);
								int diff = Mathf.Abs(cntAuthorOne - cntAuthorTwo);
								if (diff == 0 && cntAuthorTwo >= highestSimilarity) {
									Debug.Log ("For the category: " + inputCategory + ", Author One, " + authorOne + ", has made: " + cntAuthorOne + " articles within this category and Author Two, " + authorTwo + 
										", has made: " + cntAuthorTwo + " articles within this category.");
								}
								if (diff == 0 && cntAuthorTwo > highestSimilarity)
								{
									/*We clear the list when we see a higher total of articles in an algorithm category from one */
									topAuthorsToArticleDictionary.Clear();
									highestSimilarity = cntAuthorTwo; //either one, since diff is 0. What we are saying is that if the similar number of articles from another category is
									string[] articleEdge = new string[2];
									articleEdge[0] = articleToMasterNode1.Key;
									articleEdge[1] = articleToMasterNode2.Key;

									string[] authorEdge = new string[2];
									authorEdge[0] = authorOne;
									authorEdge[1] = authorTwo;
									topAuthorsToArticleDictionary [articleEdge] = authorEdge;
								} else if (diff == 0 && cntAuthorTwo == highestSimilarity)
								{
									string[] articleEdge = new string[2];
									articleEdge[0] = articleToMasterNode1.Key;
									articleEdge[1] = articleToMasterNode2.Key;

									string[] authorEdge = new string[2];
									authorEdge[0] = authorOne;
									authorEdge[1] = authorTwo;
									topAuthorsToArticleDictionary [articleEdge] = authorEdge;
								}
							}
						}
					}
				}

			}

		}

		return topAuthorsToArticleDictionary;
	}

	/// <summary>
	/// This method gets the top five neighbors of the related parameter based on the totalNumber of articles
	/// between the parameter and a specific edge, as well as determining if both vertexes are in the same
	/// category or not.
	/// </summary>
	/// <param name="rootVertex"></param>
	/// <returns></returns>
	public static List<string> GetTopFiveNeighbors(string inputAuthor, string inputCategory)
	{
		int MAX_THRESHOLD = 0;
		List<string> topFiveList = new List<string>();
		int cntParam = CountHelper(inputAuthor, inputCategory);
		int neighborCount = 0;

		foreach (KeyValuePair<string, MasterNode> articleToMasterNode in articleContainerDictionary)
		{
			if (neighborCount < 5)
			{
				foreach (string author in articleToMasterNode.Value.Authors)
				{ 
					if (articleToMasterNode.Value.Category != inputCategory && articleToMasterNode.Value.Category != "Uncategorized")
					{
						int cntOfOther = CountHelper(author, inputCategory); //finds an author in a different category, and determines the number of algorithm articles for that author
						int diff = UnityEngine.Mathf.Abs(cntParam - cntOfOther);
						if (diff == MAX_THRESHOLD)
						{
							topFiveList.Add(articleToMasterNode.Key);
							neighborCount++;
						}
					}
				}
			}
			else
			{
				//At this point, we have now found five neighbors and can now exit the loop
				break;
			}
		}

		return topFiveList;
	}

    /// <summary>
    /// Returns a list of all the coauthor's articles strings from a given author.
    /// </summary>
    /// <returns>The articles from single author.</returns>
    /// <param name="inputAuthor">Input author.</param>
    public static List<string> FindCoauthors(string inputAuthor)
    {
        List<string> coauthorsFromAuthor = new List<string>();

        try
        {
            //Originally had a list in here, but an articleToMasterNode KeyValue pair should avoid creating more memory
            foreach (KeyValuePair<string, MasterNode> articleToMasterNode in articleContainerDictionary)
            {
                if (articleToMasterNode.Value.Authors.Contains(inputAuthor))
                {
                    foreach (string author in articleToMasterNode.Value.Authors)
                    {
                        if (author != inputAuthor && !coauthorsFromAuthor.Contains(author))
                        {
                            coauthorsFromAuthor.Add(author);
                        }
                    }
                }
            }
        }
        catch (NullReferenceException e)
        {
            Console.Write(e.StackTrace);
        }

        return coauthorsFromAuthor.Distinct().ToList();
    }

    private static int CountHelper(string author, string category)
	{
		int neighborCount = 0;

		foreach (KeyValuePair<string, MasterNode> articleToMasterNode in articleContainerDictionary)
		{
			if (articleToMasterNode.Value.Authors.Contains(author) && articleToMasterNode.Value.Category == category)
			{
				neighborCount++;
				//Debug.Log ("Author: " + author + ", Article: " + articleToMasterNode.Key + ", Current Count: " + neighborCount);
			}
		}

		return neighborCount;
	}
		



}
