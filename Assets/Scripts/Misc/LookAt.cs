using UnityEngine;
using System.Collections;

public class LookAt : MonoBehaviour {

    private Camera player;

	// Use this for initialization
	void Start () {
        player = Camera.main;
	}
	
	// Update is called once per frame
	void Update () {
		transform.LookAt(2 * transform.position - player.transform.position);

	}
}
