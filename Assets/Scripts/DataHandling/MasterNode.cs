using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Master node - stores everything (besides article title).
/// This will be accessed as a Value from a Key of a data title in a Dictionary class.
/// </summary>
public class MasterNode {

	public const int MAX_LEVELS = 4;

	private List<string> authors;
	private string title;
	private string conference; //extracted from the URL of the article
	private int year;
	private string category;

	private GameObject[] bezierPoints = new GameObject[MAX_LEVELS];

	public MasterNode(string title, int year)
	{
		year = this.year;
		title = this.title;
	}

	/* Constructor without URL and GameObject.
     * (If I don't get any immediate information about the GameObject, I can just use this by default, with the key as the article.)
     */
	public MasterNode(List<string> article_authors, string article_title, int article_year)
	{
		authors = article_authors;
		title = article_title;
		year = article_year;
	}

	public MasterNode(List<string> article_authors, string article_title, int article_year, string categoryOfArticle) {
		authors = article_authors;
		title = article_title;
		year = article_year;
		category = categoryOfArticle;
	}

	public MasterNode(List<string> article_authors, string article_title, string article_conference, int article_year, string categoryOfArticle) {
		authors = article_authors;
		title = article_title;
		conference = article_conference;
		year = article_year;
		category = categoryOfArticle;
	}

	public string Title
	{
		get { return title; }
		set { title = value; }
	}

	public int Year
	{
		get { return year; }
		set { year = value; }
	}

	public string Conference {
		get { return conference; }
	}

	public List<string> Authors {
		get { return authors; }
	}

	/// <summary>
	/// Currently, this category getter returns just a single string notifying what the current category name of the article is.
	/// Eventually, I will have to add in the support of multiple categories for a specific article title.
	/// </summary>
	public string Category
	{
		get { return category; }
	}

	//For the current Number of levels, 
	public GameObject[] MasterNodeGameObjects {
		get { return bezierPoints; }
	}

}
