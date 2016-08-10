using System.Collections;
using UnityEngine;

public class Edge {

    private string articleSource;
    private string articleDest;
    private Transform scaleVector = null;

	public Edge(string articleSourcePoint, string articleDestPoint)
    {
        articleSource = articleSourcePoint;
        articleDest = articleDestPoint;
    }

    public string ArticleSource {
        get { return articleSource; }
    }

    public string ArticleDest
    {
        get { return articleDest; }
    }

    public Transform ScaleVectorTransform
    {
        get { return scaleVector; }
        set { scaleVector = value; }
    }

}
