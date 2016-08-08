using UnityEngine;
using System.Collections;

public class RelativeScaleTest : MonoBehaviour {

    public float desiredScaleX = 7.0f;
    public float speed = 15.0f;

    private float anchorPosX;
    private float originalScaleX;

    void Start()
    {
        anchorPosX = transform.position.x - transform.localScale.x;
        originalScaleX = transform.localScale.x;
    }

    void Update()
    {
        Vector3 tempPosScale = transform.localScale;
        tempPosScale.x = Mathf.MoveTowards(transform.localScale.x, desiredScaleX, speed * Time.deltaTime); ;
        transform.localScale = tempPosScale;

        Vector3 tempPosVector = transform.position;
        Debug.Log(tempPosVector.x);
        tempPosVector.x = anchorPosX + (transform.localScale.x + originalScaleX) / 2.0f;
        transform.position = tempPosVector;
    }

}
