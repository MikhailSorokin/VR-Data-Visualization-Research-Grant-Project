using System;

[Serializable()]
public class EmptyPageException : Exception {
    public EmptyPageException() : base() { }
    public EmptyPageException(string message) : base(message) { }
	
}
