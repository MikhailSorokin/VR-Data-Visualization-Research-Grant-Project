using UnityEngine;
using System.Collections;
[AddComponentMenu("Camera-Control/Mouse drag Orbit with zoom")]
public class DragMouseOrbit : MonoBehaviour
{
    public Transform target;
    public float distance = 5.0f;
    public float xSpeed = 120.0f;
    public float ySpeed = 120.0f;
    public float yMinLimit = -20f;
    public float yMaxLimit = 80f;
    public float distanceMin = .5f;
    public float distanceMax = 15f;
    public float smoothTime = 2f;
    float rotationYAxis = 0.0f;
    float rotationXAxis = 0.0f;
    float velocityX = 0.0f;
    float velocityY = 0.0f;

    // Use this for initialization
    void Start()
    {
        Vector3 angles = transform.eulerAngles;
        rotationYAxis = angles.y;
        rotationXAxis = angles.x;
    }
    void LateUpdate()
    {
        if (target)
        {

            if (Input.GetMouseButton(0))
            {
                velocityX += xSpeed * Input.GetAxis("Mouse X") * distance * Time.smoothDeltaTime;
                velocityY += ySpeed * Input.GetAxis("Mouse Y") * Time.smoothDeltaTime;
            }
            rotationYAxis += velocityX;
            rotationXAxis -= velocityY;
            rotationXAxis = ClampAngle(rotationXAxis, yMinLimit, yMaxLimit);

            Quaternion toRotation = Quaternion.Euler(rotationXAxis, rotationYAxis, 0);
            Quaternion rotation = toRotation;

            //TODO: Implement working scrolling with mouse on GUI
            /*if (ViveControllerInput.Instance.guiSelected)
            {
                RectTransform guiRectTransform = FindObjectOfType<DataVisInputs>().guiParameters.gridImage.GetComponent<RectTransform>();
                Vector3 tempRectTransform = (guiRectTransform.anchoredPosition3D);
                GameObject numberButtonsGO = GameObject.Find("Button (" + buttonCount + ")");
                //FIXED: Don't have a call on GameObject.Find every time this is being called
                if (tempRectTransform.y >= 0 && numberButtonsGO.transform.position.y < bottomGO.transform.position.y
                    && Input.GetAxis("Mouse ScrollWheel") > 0)
                {
                    tempRectTransform.y += Time.deltaTime * Input.GetAxis("Mouse ScrollWheel") * 100;
                }
                else if (tempRectTransform.y > 0 && Input.GetAxis("Mouse ScrollWheel") < 0)
                {
                    tempRectTransform.y += Time.deltaTime * Input.GetAxis("Mouse ScrollWheel") * 100;
                }
                else if (tempRectTransform.y < 0)
                {
                    tempRectTransform.y = 0f;
                }
                guiRectTransform.anchoredPosition3D = tempRectTransform;
                distance = 0;
            }
            else
            {

            }*/
            distance = Mathf.Clamp(distance - Input.GetAxis("Mouse ScrollWheel") * 5, distanceMin, distanceMax);
            RaycastHit hit;
            if (Physics.Linecast(target.position, transform.position, out hit))
            {
                //distance -= hit.distance;
            }
            Vector3 negDistance = new Vector3(0.0f, 0.0f, -distance);
            Vector3 position = rotation * negDistance + target.position;

            transform.rotation = rotation;
            transform.position = position;
            velocityX = Mathf.Lerp(velocityX, 0, Time.deltaTime * smoothTime);
            velocityY = Mathf.Lerp(velocityY, 0, Time.deltaTime * smoothTime);
        }
    }
    public static float ClampAngle(float angle, float min, float max)
    {
        if (angle < -360F)
            angle += 360F;
        if (angle > 360F)
            angle -= 360F;
        return Mathf.Clamp(angle, min, max);
    }
}
