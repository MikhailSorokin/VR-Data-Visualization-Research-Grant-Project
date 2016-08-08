using System.Collections;
using System.Collections.Generic;

public class Edge<T> {

	private T source;
	private T dest;

	public Edge(T sourceVertex, T destVertex)
	{
		source = sourceVertex;
		dest = destVertex;
	}

	/* If we need to access the edge source from outside this class, then use this getter.*/
	public T VertexSource { 
		get { return source; }    
	}

	public T VertexDest {
		get { return dest; }
	}

}
