using UnityEngine;
using System.Collections;

public class EnableSingleControllerCollider : MonoBehaviour {

    public Collider left;
    public Collider right;
    public Transform model;

    // Use this for initialization
    void Start () {
        right.enabled = false;
	}
	
	// Update is called once per frame
	void Update () {
	    if ( Vector3.Distance(left.transform.position, model.transform.position) < Vector3.Distance(right.transform.position, model.transform.position))
        {
            if (!left.enabled)
            {
                left.enabled = true;
                right.enabled = false;
            }
        } else
        {
            if (!right.enabled)
            {
                left.enabled = false;
                right.enabled = true;
            }
        }
	}
}
