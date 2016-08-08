using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq; //for the intersection method

/// <summary>
/// This class creates a graph with vertices and edges, which will contain edges going from one vertex to another.
/// Then, once a graph is constructed, certain functions within Graph can be used to find relationships within
/// all sorts of categories, such as which authors are publishing the same number of articles. It can also find outcasts,
/// which means finding out, from searching two specific authors, which set of vertices are completely different from this
/// set of vertices.
/// </summary>
/// <typeparam name="AnyCategory"></typeparam>
public class Graph {

	private ArticleVertex<int>[] vertexArray;
	private List<Edge<ArticleVertex<int>>> edgesList;
	private static int vertexID;

	public Graph()
	{
		vertexArray = new ArticleVertex<int>[1];
		vertexID = 1;

		//initialize edgesList
		edgesList = new List<Edge<ArticleVertex<int>>>();

		//TODO: Need to grab the connections that galen uses and initialize the edgesList variable in order to build a relationship 
		//between certain ArticleVertices.
	}

	/*-----------------------EVERYTHING TO DO WITH ADDING VERTEXES AND EDGES--------------------------------------*/
	/* Take in a HashMap of authors (as keys) to a list of his/her years (as values) in order to be able to construct a graph of edges and vertices. */
	public void ParseInformation (Dictionary<string, List<int>> authorsToYears) {
		foreach (KeyValuePair<string,List<int>> kv in authorsToYears) {
			AddVertex (kv.Key, kv.Value);
		}

	}

	/* Adds an ArticleVertex - which is just a Vertex containing information about a specific article. */
	public void AddVertex(string author, List<int> years) {
		foreach (int year in years) {
			ArticleVertex<int> article = new ArticleVertex<int> (vertexID, author, year);
			vertexArray [vertexID - 1] = article; 
			Array.Resize (ref vertexArray, vertexArray.Length + 1);
			vertexID++;
		}
	}

	public ArticleVertex<int> FindVertex (string author, int year) {
		foreach (ArticleVertex<int> av in vertexArray) {
			if (av.Author == author && av.Year == year) {
				return av;
			}
		}

		//Didn't find anything
		return null;
	}


	/* This method is called after all the vertices have been added.*/
	public void AddEdge (string author, int startYear, int endYear) {
		ArticleVertex<int> articleStart = FindVertex (author, startYear);
		ArticleVertex<int> articleEnd = FindVertex (author, endYear);

		if (articleStart != null && articleEnd != null) {
			edgesList.Add (new Edge<ArticleVertex<int>> (articleStart, articleEnd));
		}
	}

	public List<ArticleVertex<int>> GetNeighbors (ArticleVertex<int> rootVertex) {
		List<ArticleVertex<int>> neighbors = new List<ArticleVertex<int>>();

		foreach (Edge<ArticleVertex<int>> edge in edgesList) {
			if (edge.VertexSource == rootVertex) {
				neighbors.Add(edge.VertexDest);
			}
		}

		return neighbors;
	}

	/*--------------------------------------------------------------------------------------------------------------*/
	/*--------------------------------------------------DEBUGGIN METHODS--------------------------------------------*/
	/* These methods are useful for debuggin. */
	public ArticleVertex<int>[] GetCurrentVertices() {
		return vertexArray;
	}

	public List<Edge<ArticleVertex<int>>> GetCurrentEdges() {
		return edgesList;
	}


	/*--------------------------------------------------------------------------------------------------------------*/
	/*------------------------------------------Dijkstra BFS Stuff WOW ;)-------------------------------------------*/
	/* Find the SHORTEST length between the two synset IDs parameters, which would be specified by the user. */
	public int Length(ArticleVertex<int> articleOne, ArticleVertex<int> articleTwo)
	{
		int overallShortestLength = -1;

		if (Get_Ancestor_Or_Length_Helper(articleOne, articleTwo, 0) != -1)
		{
			overallShortestLength = Get_Ancestor_Or_Length_Helper(articleOne, articleTwo, 0);
		}

		return overallShortestLength;
	}

	public int Ancestor(ArticleVertex<int> articleOne, ArticleVertex<int> articleTwo)
	{
		int overallShortestAncestorID = -1;

		if (Get_Ancestor_Or_Length_Helper(articleOne, articleTwo, 1) != -1)
		{
			overallShortestAncestorID = Get_Ancestor_Or_Length_Helper(articleOne, articleTwo, 1);
		}

		return overallShortestAncestorID;
	}

	private int Get_Ancestor_Or_Length_Helper(ArticleVertex<int> articleOne, ArticleVertex<int> articleTwo, int mode)
	{
		int returnVal = -1;

		if (mode == 0) {
			returnVal = Length_Helper (articleOne, articleTwo);
		} else if (mode == 1) {
			returnVal = Ancestor_Helper (articleOne, articleTwo);
		}
		//TODO - add a BFS search for the path between synsetID1 and synsetID2
		return returnVal;
	}

	/* This is a BFS search algorithm which looks at the two parameters and returns all of the 
	 * costs associated with the distance between two neighbors.
	 */
	private int Length_Helper(ArticleVertex<int> articleOne, ArticleVertex<int> articleTwo) {
		int totalMinDistance = -1;
		List<ArticleVertex<int>> neighbors = new List<ArticleVertex<int>> ();
		List<ArticleVertex<int>> firstParents = new List<ArticleVertex<int>> ();

		/* A Dictionary is preffered over a HashTable because a dictionary allows for generic type.
		*  this allows for static typing and no boxing needing to be done on it.
		*/
		Dictionary<ArticleVertex<int>, int> currDistances = new Dictionary<ArticleVertex<int>, int> ();
		Queue<ArticleVertex<int>> lengthQueue = new Queue<ArticleVertex<int>>();

		lengthQueue.Enqueue (articleOne);
		firstParents.Add (articleOne);
		currDistances [articleOne] = 0;

		while (lengthQueue.Count != 0) {
			ArticleVertex<int> currentElem = lengthQueue.Dequeue ();

			/* Process all the neighbors of the current element in the hypernym hashmap, and
			 * updates the distance of the current element being processed  
			 */
			neighbors = GetNeighbors(currentElem);

			for (int neighborInd = 0; neighborInd < neighbors.Count; neighborInd++) {
				ArticleVertex<int> adjacentElement = neighbors [neighborInd];

				if (currDistances [adjacentElement] == 0) {
					currDistances [adjacentElement] = currDistances [currentElem] + 1;
					firstParents.Add (adjacentElement);
					lengthQueue.Enqueue (adjacentElement);
				}
			}

			neighbors.Clear();
		}
			
		List<ArticleVertex<int>> secondParents = new List<ArticleVertex<int>> ();

		/* A Dictionary is preffered over a HashTable because a dictionary allows for generic type.
		*  this allows for static typing and no boxing needing to be done on it.
		*/
		Dictionary<ArticleVertex<int>, int> currDistancesTwo = new Dictionary<ArticleVertex<int>, int> ();

		lengthQueue.Enqueue (articleTwo);
		secondParents.Add (articleTwo);
		currDistancesTwo [articleTwo] = 0;

		while (lengthQueue.Count != 0) {
			ArticleVertex<int> currentElem = lengthQueue.Dequeue ();

			/* Process all the neighbors of the current element in the hypernym hashmap, and
			 * updates the distance of the current element being processed  
			 */
			neighbors = GetNeighbors(currentElem);

			for (int neighborInd = 0; neighborInd < neighbors.Count; neighborInd++) {
				ArticleVertex<int> adjacentElement = neighbors [neighborInd];

				if (currDistancesTwo [adjacentElement] == 0) {
					currDistances [adjacentElement] = currDistancesTwo [currentElem] + 1;
					secondParents.Add (adjacentElement);
					lengthQueue.Enqueue (adjacentElement);
				}
			}

			neighbors.Clear();
		}

		List<ArticleVertex<int>> intersected = (List<ArticleVertex<int>>) firstParents.Intersect (secondParents);
		//Need to process all intersected parents
		if (intersected != null) {
			totalMinDistance = currDistances [intersected[0]] + currDistancesTwo [intersected[0]];
			for (int distPos = 0; distPos < intersected.Count; distPos++) {
				int totalDistance = currDistances [intersected [distPos]] + currDistancesTwo [intersected [distPos] ];
				if (totalDistance < totalMinDistance) {
					totalMinDistance = totalDistance;
				}
			}
		}

		return totalMinDistance;
	}
		

	private int Ancestor_Helper(ArticleVertex<int> articleOne, ArticleVertex<int> articleTwo) {
		return -1;
	}


	/* Based on the two inputs (and category), this method should return an array of the elements 
     * which are most closely related to each other. */
	public void MostSimilar(int a, int b)
	{

	}

	/* Based on the two inputs, this method should return an array of the elements 
     * which are the furthest distance away from these two inputs. */
	public void Outcast(int a, int b) 
	{

	}
	/*--------------------------------------------------------------------------------------------------------------*/

}
