using System.Collections;

/// <summary>
/// This class will be a creation of a Vertex in a set. Currently, a vertex is just used as a base class.
/// </summary>
public class Vertex<T> {
	protected T vertexNum;

	public Vertex(T vertexNum)
	{
		this.vertexNum = vertexNum;
	}

	/* If we need to access the vertex num from outside this class, then use this getter.*/
	public T VertexNum { 
		get { return this.vertexNum; }    
	}


}
