//====================================================================================
//
// Purpose: Provide basic touch detection of controller to interactable objects
//
// This script must be attached to a Controller within the [CameraRig] Prefab
// Modified heavily, 
// Credit goes to: Mikhail Sorokin
//
//====================================================================================

using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class SteamVR_InteractTouch : MonoBehaviour
{

    [Header("Collider Properties", order = 1)]
    public float colliderRadius = 0.05f;
    public Vector3 centerOffsetVector = new Vector3(0.005f, -0.03f, 0.01f);
    public bool triggerStatus = true;

    [Header("Controller Vibration Properties", order = 2)]
    public int cooldown = 3;
    public ushort hapticFeedbackStrength = 200;

    [Header("Other References", order = 3)]
    public DataVisInputs dataVisInputs;
    public GUIHandler masterGUIHandler;

    private int buttonCount = 0;
    private float timePassed;
    private bool hasEnableExpanded;

    void Start()
    {
        if (GetComponent<SteamVR_ControllerActions>() == null)
        {
            Debug.LogError("SteamVR_InteractTouch is required to be attached to a SteamVR Controller that has the SteamVR_ControllerActions script attached to it");
            return;
        }

        //Create trigger box collider for controller
        SphereCollider collider = this.gameObject.AddComponent<SphereCollider>();
        collider.radius = colliderRadius;
        collider.center = centerOffsetVector;
        collider.isTrigger = triggerStatus;
    }

    void OnTriggerStay(Collider collider)
    {
        timePassed += Time.deltaTime;
        if (collider.tag == "Datapoint" && timePassed <= cooldown)
        {
            /*if (this.transform.name == "Controller (left)")
            {
                GameObject.Find("Controller (right)").GetComponent<Collider>().enabled = false;
            }
            if (this.transform.name == "Controller (right)")
            {
                GameObject.Find("Controller (left)").GetComponent<Collider>().enabled = false;
            }*/
            dataVisInputs.DoRigidbodyFreeze();
            collider.GetComponent<Renderer>().material.SetColor("_TintColor", Color.green);
            GetComponent<SteamVR_ControllerActions>().TriggerHapticPulse(cooldown, hapticFeedbackStrength); //first parameter is duration (in seconds), second is pressure applied. Max is about 3000

            if (!hasEnableExpanded)
            {
                EnableForExpand(collider);
                hasEnableExpanded = true;
            }
        }
    }

    void OnTriggerExit(Collider collider)
    {
        if (collider.tag == "Datapoint" && !masterGUIHandler.isOnArticle)
        {
            /*GameObject.Find("Controller (left)").GetComponent<Collider>().enabled = true;
            GameObject.Find("Controller (right)").GetComponent<Collider>().enabled = true;*/
            collider.GetComponent<Renderer>().material.SetColor("_TintColor", Color.white);
            dataVisInputs.AllowExpansion(null, null);
            dataVisInputs.ButtonCount = buttonCount;
            timePassed = 0;
            hasEnableExpanded = false;
        }
        else if (collider.tag == "Datapoint")
        {
            collider.GetComponent<Renderer>().material.SetColor("_TintColor", Color.white);
            dataVisInputs.AllowExpansion(null, null);
            dataVisInputs.ButtonCount = buttonCount;
            timePassed = 0;
            hasEnableExpanded = false;
        }

    }

    private void EnableForExpand(Collider collider)
    {
        if (collider.transform.parent.parent)
        {
            collider.transform.parent.parent.GetComponent<SplineDecorator>().DimSurroundingVisuals2(collider.transform.parent);
        }
        else
        {
            print("not found");
        }

        //The title of the GUI will be updated only when the user is not on the "Input Author" section.
        if (!masterGUIHandler.isOnArticle)
        {
            string titleStr;
            string catStr;
            if (collider.transform.parent)
            {
                if (collider.GetComponent<SplineDecorator>().datasetCategory == SplineDecorator.DatasetCategory.Articles)
                {
                    titleStr = collider.GetComponent<SplineDecorator>().title;
                    catStr = collider.transform.parent.parent.GetComponent<SplineDecorator>().title;
                }
                else if (collider.GetComponent<SplineDecorator>().datasetCategory == SplineDecorator.DatasetCategory.Singleton)
                {
                    titleStr = collider.GetComponent<SplineDecorator>().title;
                    catStr = "Article";
                }
                else if (collider.GetComponent<SplineDecorator>().datasetCategory == SplineDecorator.DatasetCategory.Years)
                {
                    titleStr = collider.GetComponent<SplineDecorator>().title;
                    catStr = collider.transform.parent.GetComponent<SplineDecorator>().title;
                }
                else
                {
                    titleStr = "err";
                    catStr = "cat err";
                }
            }
            else
            {
                titleStr = collider.transform.GetComponent<SplineDecorator>().title;
                catStr = "DBPL";
            }

            Transform gridImage = GameObject.FindGameObjectWithTag("Grid Image").transform;

            int breakPoint = collider.GetComponent<SplineDecorator>().DataSetStrings.Count;

            Button button = null;
            buttonCount = 0;
            dataVisInputs.ResetGridImage();

            masterGUIHandler.DestroyButtons();

            for (int i = 0; i < breakPoint; i++)
            {
                buttonCount++;

                button = Instantiate(masterGUIHandler.MenuButton);
                button.transform.GetChild(0).GetComponent<Text>().text = collider.GetComponent<SplineDecorator>().DataSetStrings[i];
                /* This will retain local orientation and scale rather than world orientation and scale, which can prevent
                 * common UI scaling issues.*/
                button.transform.SetParent(gridImage, false); //originally, this was button.transform.parent = gridImage;
                button.transform.localScale = Vector3.one;
                button.transform.name = "Button (" + buttonCount + ")";
            }

            masterGUIHandler.SetTitle(catStr + " - " + titleStr);
        }

        //FIXED: Don't pass in buttonCount here as it can just be done from the ButtonCount getter/setter
        dataVisInputs.AllowExpansion(collider.gameObject, this.gameObject.GetComponent<SteamVR_TrackedController>());
        dataVisInputs.ButtonCount = buttonCount;
    }

}
