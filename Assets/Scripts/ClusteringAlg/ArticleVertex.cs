using System.Collections;
using System.Collections.Generic;

/// <summary>
/// This class is used to specifically access article vertex information.
/// </summary>
public class ArticleVertex<T> : Vertex<T> {

	private List<string> node_Authors;
	private string node_Author;
	private string node_Title;
	private string node_URL;
	private int node_Year;

	public ArticleVertex(T artVertexNum, List<string> node_Author) : base(artVertexNum)
	{
		this.node_Authors = node_Author;
	}

	/* This constructor will be used for the HashMap connecting an author to a year.*/
	public ArticleVertex(T artVertexNum, string node_Author, int node_Year) : base(artVertexNum)
	{
		this.node_Author = node_Author;
		this.node_Year = node_Year;
	}

	public ArticleVertex(T artVertexNum, List<string> node_Author, string node_Title, string node_URL, int node_Year) : base(artVertexNum)
	{
		this.node_Authors = node_Author;
		this.node_Title = node_Title;
		this.node_URL = node_URL;
		this.node_Year = node_Year;
	}

	/* TODO - There can be more than one author for an article, so will eventually have to change this to an 
     * array of authors.
     */
	public string Author
	{
		get { return node_Author; }
	}

	public string Conference {
		get { return node_URL; }
	}

	public List<string> Authors
	{
		get { return node_Authors; }
	}

	public string Title
	{
		get { return node_Title; }
	}

	public int Year
	{
		get { return node_Year; }
	}


}
