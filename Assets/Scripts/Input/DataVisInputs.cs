﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using HandControlllerInputs;
using UnityEngine.UI;

//TODO: Connections are drawn, but they need to be updated now
public class DataVisInputs : MonoBehaviour
{

    [System.Serializable]
    public struct DataReadParameters
    {
        public int sizeOfClusterRead;
        public SplineDecorator[] rotators;
    }

    [System.Serializable]
    public struct GUIParameters
    {
        public GameObject bottomGO;
        public GameObject gridImage;
        public Color originalColor;
    }

    [System.Serializable]
    public struct TransitionParameters
    {
        public TextMesh[] textMeshes;
        public float speed;
        public Vector3[] oPos1, oPos2;
        public Quaternion[] oRot1, oRot2;
        public bool canTransition;
    }

    public DataReadParameters dataReadParameters;
    public GUIParameters guiParameters;
    public TransitionParameters transitionParameters;
    public float rotationSpeed = 500.0f;
    public bool usingMouseControls = false;
    public bool menuWanted = false;

    //Private Variables
    private static GameObject colliderGameObject;
    private static GameObject touchedObject;
    private static GameObject dataPointToUse;
    private float distance;
    private SteamVR_TrackedController contrToUse;
    private SteamVR_TrackedObject trackedObj;
    private int buttonCount = 1;

    private GUIHandler masterGUIHandler;
    private bool transitionHappening;
    private bool waitingOh;
    private bool menuOnL = true;
    private bool menuOnR = true;
    private bool menuBool = true;
    private bool transitionedInvis;
    private bool doneYet;
    private const string HEX_STRING_WITH_ALPHA = "0x48FF00FF";
    private int times = 0;

    public Transform leftLine;
    public Transform rightLine;
    public Transform centerTitle;

    //This is when both triggers are pressed
    private float controllerDistance = 0.0f;
    private Vector3 lScale = Vector3.zero;
    private float timePassed;
    private Transform objectHit;

    private float bothControllerDistance = 0.0f;
    private Vector3 leftControllerScale = Vector3.zero;
    private Vector3 rightControllerScale = Vector3.zero;

    //This is for the double controller button GUI
    public float desiredLeftScaleX;
    public float desiredRightScaleX;
    public float speed = 1.0f;
    private float leftAnchorPosX;
    private float leftOriginalScaleX;
    private float leftEndScaleX;
    private float rightAnchorPosX;
    private float rightOriginalScaleX;
    private float rightEndScaleX;
    private Camera cam;

    void Start()
    {
        transitionParameters.oPos1 = ControllerManager.refToMainSplineGO.GetComponent<SplineDecorator>().oPos1;
        transitionParameters.oPos2 = ControllerManager.refToMainSplineGO.GetComponent<SplineDecorator>().oPos2;
        transitionParameters.oRot1 = ControllerManager.refToMainSplineGO.GetComponent<SplineDecorator>().oRot1;
        transitionParameters.oRot2 = ControllerManager.refToMainSplineGO.GetComponent<SplineDecorator>().oRot2;

        //Call all the controller events here
        ControllerManager.Intro += new DoubleControllerInteractionEventHandler(DoSpawnControllerMenuGUIs);
        ControllerManager.RigidbodyFreeze += new DoubleControllerInteractionEventHandler(DoRigidbodyFreeze);
        //ControllerManager.DoubleControllerGUI += new DoubleControllerInteractionEventHandler(SetUpDoubleControllerGUI);

        ControllerManager.DoubleTouchpadPressed += new DoubleControllerInteractionEventHandler(TransitionThere);
        ControllerManager.DoubleTouchpadReleased += new DoubleControllerInteractionEventHandler(NoTransitions);

        ControllerManager.DoubleTriggerPressed += new DoubleControllerInteractionEventHandler(ScaleUp);
        ControllerManager.DoubleTriggerReleased += new DoubleControllerInteractionEventHandler(StopScale);

        ControllerManager.TriggerPressed += new ControllerInteractionEventHandler(DoTriggerPressed);
        ControllerManager.TriggerReleased += new ControllerInteractionEventHandler(DoTriggerReleased);

        ControllerManager.GripPressed += new ControllerInteractionEventHandler(DoGripPressed);
        ControllerManager.MapGripToKeyPress += new KeyboardInteractionEventHandler(DoGripPressed);
        ControllerManager.GripReleased += new ControllerInteractionEventHandler(DoGripReleased);
        ControllerManager.MapGripToKeyRelease += new KeyboardInteractionEventHandler(DoGripReleased);

        ControllerManager.TouchpadPressed += new ControllerInteractionEventHandler(DoTouchpadPressed);
        ControllerManager.MapTouchpadPressToKeyPress += new KeyboardInteractionEventHandler(DoTouchpadPressed);
        ControllerManager.TouchpadReleased += new ControllerInteractionEventHandler(DoTouchpadReleased);
        ControllerManager.MapTouchpadReleaseToKeyRelease += new KeyboardInteractionEventHandler(DoTouchpadReleased);

        ControllerManager.MenuReleased += new ControllerInteractionEventHandler(DoMenuReleased);

        ControllerManager.MapTouchpadAxisChangedToArrowKeys += new MultiKeyInteractionEventHandler(DoChangeOnKeyboardInput);
        ControllerManager.TouchpadAxisChanged += new ControllerInteractionEventHandler(DoTouchPadAxisChanged);

        masterGUIHandler = (GUIHandler)FindObjectOfType(typeof(GUIHandler));

        GameObject cam3D = GameObject.Find("MainCamera");
        GameObject vrHeadset = GameObject.FindGameObjectWithTag("VRHeadsetCam");

        if (usingMouseControls)
        {
            if (cam3D == null)
            {
                Debug.LogError("Could not find the camera to use for the mouse interactivity and controls.");
                return;
            }

            if (cam3D != null)
            {
                cam3D.SetActive(true);
                cam = cam3D.GetComponent<Camera>();
            }

            if (vrHeadset != null)
            {
                vrHeadset.SetActive(false);
                Destroy(GameObject.Find("[SteamVR]").gameObject);
                Destroy(GameObject.Find("Controller UI Camera"));
                Destroy(GameObject.Find("Cursor 0"));
                Destroy(GameObject.Find("Cursor 1"));
            }

        }
        else
        {
            if (vrHeadset == null)
            {
                Debug.LogError("Could not find the camera to use for virtual reality interactivity and controls.");
                return;
            }

            if (vrHeadset != null)
            {
                vrHeadset.SetActive(true);
                cam = vrHeadset.GetComponent<Camera>();
            }

            if (cam3D != null)
            {
                cam3D.SetActive(false);
            }
        }

        ClearConsole();

    }

    static void ClearConsole()
    {
        // This simply does "LogEntries.Clear()" the long way:
        var logEntries = System.Type.GetType("UnityEditorInternal.LogEntries,UnityEditor.dll");
        var clearMethod = logEntries.GetMethod("Clear", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);
        clearMethod.Invoke(null, null);
    }

    void Update()
    {
        if (usingMouseControls) 
            UseMouseEvents();

        foreach (KeyValuePair<string, MasterNode> articleToMasterNode in DataProcessor.articleContainerDictionary)
        {
            for (int levelIndex = 0; levelIndex < articleToMasterNode.Value.MasterNodeGameObjects.Length; levelIndex++)
            {
                if (articleToMasterNode.Value.MasterNodeGameObjects[levelIndex] != null)
                {
                    Transform[] activeGOs = articleToMasterNode.Value.MasterNodeGameObjects[levelIndex].GetComponent<SplineDecorator>().elements;
                    foreach (Transform checkActiveGO in activeGOs)
                    {
                        if (checkActiveGO != null && articleToMasterNode.Value.MasterNodeGameObjects[levelIndex] == checkActiveGO.gameObject)
                        {
                            articleToMasterNode.Value.MasterNodeGameObjects[levelIndex] = checkActiveGO.gameObject;
                        }
                    }
                }
            }
        }
        //Need to Update all AuthorData GO positions
        foreach (AuthorData authorData in DataProcessor.allAuthorData)
        {
            foreach (KeyValuePair<GameObject, List<string>> gameobjectToCoauthorList in authorData.goLocations)
            {
                if (gameobjectToCoauthorList.Key != null)
                {
                    Transform[] activeGOs = gameobjectToCoauthorList.Key.GetComponent<SplineDecorator>().elements;
                    foreach (Transform checkActiveGO in activeGOs)
                    {
                       
                        if (checkActiveGO != null && gameobjectToCoauthorList.Key == checkActiveGO.gameObject)
                        {
                            ChangeKey(authorData.goLocations, gameobjectToCoauthorList.Key, checkActiveGO.gameObject);

                        }
                    }
                }
            }
        }

        ControllerManager.refToMainSplineGO.GetComponent<SplineDecorator>().UpdateSplinePos();
    }

    private void UseMouseEvents()
    {
        RaycastHit hitInfo = new RaycastHit();
        bool hit = Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hitInfo);

        if (hit)
        {
            if (hitInfo.transform.gameObject.tag == "Datapoint")
            {
                if (hitInfo.transform.GetComponent<SplineDecorator>())
                {

                    hitInfo.transform.parent.parent.GetComponent<SplineDecorator>().DimSurroundingVisuals(hitInfo.transform.parent);
                }
            }
        }

        if (Input.GetMouseButtonDown(1))
        {
            if (cam != null) 
                cam.transform.position += cam.transform.right;
        }

        if (hit)
        {
            if (hitInfo.transform.gameObject.tag == "Datapoint")
            {
                hitInfo.transform.gameObject.GetComponent<Renderer>().material.SetColor("_TintColor", Color.green);
                EnableForExpand(hitInfo.transform);
                objectHit = hitInfo.transform;
            }
        }
        else
        {
            if (objectHit)
            {
                objectHit.GetComponent<Renderer>().material.SetColor("_TintColor", Color.white);
                objectHit = null;
            }
        }

        if (Input.GetMouseButtonDown(0) && Input.GetAxis("Mouse X") == 0 && Input.GetAxis("Mouse Y") == 0)
        {

            if (hit)
            {
                Debug.Log("Hit " + hitInfo.transform.gameObject.name);
                if (hitInfo.transform.gameObject.tag == "Datapoint")
                {
                    //DoRigidbodyFreeze();
                    if (hitInfo.transform.GetComponent<SplineDecorator>())
                        hitInfo.transform.GetComponent<SplineDecorator>().ExpandDataPoint();
                }
                else if (hitInfo.transform.GetComponent<TextMesh>())
                {
                    print("here");
                    hitInfo.transform.parent.GetComponent<SplineDecorator>().ExpandDataPoint();
                }
                else
                {
                    Debug.Log("nopz");
                }
            }
        }
    }

    private bool ChangeKey(Dictionary<GameObject, List<string>> dict,
                                           GameObject oldKey, GameObject newKey)
    {
        List<string> value;
        Debug.Log(dict.TryGetValue(oldKey, out value));
        if (!dict.TryGetValue(oldKey, out value))
            return false;

        dict.Remove(oldKey);  // do not change order
        dict[newKey] = value;  // or dict.Add(newKey, value) depending on ur comfort
        return true;
    }

    private static Color hexToColor(string hex)
    {
        hex = hex.Replace("0x", "");//in case the string is formatted 0xFFFFFF
        hex = hex.Replace("#", "");//in case the string is formatted #FFFFFF
        byte a = 255;//assume fully visible unless specified in hex
        byte r = byte.Parse(hex.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
        byte g = byte.Parse(hex.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
        byte b = byte.Parse(hex.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);
        //Only use alpha if the string has enough characters
        if (hex.Length == 8)
        {
            a = byte.Parse(hex.Substring(6, 2), System.Globalization.NumberStyles.HexNumber);
        }
        return new Color32(r, g, b, a);
    }

    void SetUpDoubleControllerGUI()
    {
        if (ControllerManager.leftController != null && ControllerManager.rightController != null
            && ControllerManager.leftController.isActiveAndEnabled 
            && ControllerManager.rightController.isActiveAndEnabled) {

            Vector3 pA = ControllerManager.leftController.transform.position;
            Vector3 pB = ControllerManager.rightController.transform.position;

            Vector3 midPoint = pA + (pB - pA) / 2f;

            centerTitle.position = midPoint;

            float distance = Mathf.Abs((ControllerManager.left_controller.transform.pos
            - ControllerManager.right_controller.transform.pos).magnitude);

            if (bothControllerDistance == 0.0f)
            {
                bothControllerDistance = distance;
                leftControllerScale = leftLine.localScale;
                rightControllerScale = rightLine.localScale;
            }

            if (bothControllerDistance != 0.0f)
            {
                desiredLeftScaleX = leftControllerScale.x * (distance / bothControllerDistance);
                desiredRightScaleX = rightControllerScale.x * (distance / bothControllerDistance);
            }

            if (!doneYet)
            {
                leftLine.SetParent(ControllerManager.leftController.transform, false);
                leftLine.transform.localPosition = new Vector3(0f, 0f, 0.15f);
                leftAnchorPosX = leftLine.localPosition.x - leftLine.localScale.x;
                leftOriginalScaleX = leftLine.localScale.x;
                rightLine.SetParent(ControllerManager.rightController.transform, false);
                rightLine.transform.localPosition = new Vector3(0f, 0f, 0.15f);
                rightAnchorPosX = rightLine.localPosition.x - rightLine.localScale.x;
                rightOriginalScaleX = rightLine.localScale.x;
                doneYet = true;
            }

            Vector3 tempLeftScale = leftLine.localScale;

            float leftStartX = (tempLeftScale.x < desiredLeftScaleX) ? tempLeftScale.x : desiredLeftScaleX;
            float leftEndX = Mathf.Abs(midPoint.x) - 0.5f;
            tempLeftScale.x = Mathf.MoveTowards(leftStartX, leftEndX, Time.deltaTime * speed);
            leftLine.localScale = tempLeftScale;

            Vector3 tempLeftPosVector = leftLine.localPosition;
            tempLeftPosVector.x = leftAnchorPosX + (leftLine.localScale.x + leftOriginalScaleX) / 2.0f;
            leftLine.localPosition = tempLeftPosVector;

            Vector3 tempRightScale = rightLine.localScale;

            float rightStartX = (tempRightScale.x < desiredRightScaleX) ? tempRightScale.x : desiredRightScaleX;
            float rightEndX = midPoint.x + 0.5f;
            //This midpoint has to be negative while the left midpoint has to be a positive value
            tempRightScale.x = Mathf.MoveTowards(rightStartX, rightEndX, Time.deltaTime * speed);
            rightLine.localScale = tempRightScale;

            Vector3 tempRightPosVector = rightLine.localPosition;
            tempRightPosVector.x = rightAnchorPosX + (rightLine.localScale.x + rightOriginalScaleX) / 2.0f;
            rightLine.localPosition = tempRightPosVector;

            //SetUpLineCube(leftLine, midPoint.x, desiredScaleX, leftAnchorPosX, leftOriginalScaleX, ControllerManager.left_controller.index);
            //SetUpLineCube(rightLine, midPoint.x, -desiredScaleX, rightAnchorPosX, rightOriginalScaleX, ControllerManager.right_controller.index);
        }
    }

    private void SetUpLineCube(Transform line, float midPointX, float desScaleX, float anchorPosX, float ogScaleX, uint contrIndex)
    {
        //Left Line Scale
        //var device = SteamVR_Controller.Input((int)contrIndex);
        //float contrSpd = device.velocity.magnitude;

        Vector3 tempScale = line.localScale;

        float startX = (tempScale.x < desScaleX) ? tempScale.x : desScaleX;
        //float endX = (tempScale.x < desScaleX) ? desScaleX : ogScaleX;

        tempScale.x = Mathf.MoveTowards(startX, midPointX, Time.deltaTime * speed);
        line.localScale = tempScale;

        Vector3 tempPosVector = line.localPosition;
        tempPosVector.x = anchorPosX + (line.localScale.x + ogScaleX) / 2.0f;
        line.localPosition = tempPosVector;
    }

    void ScaleUp()
    {
        float distance = Mathf.Abs((ControllerManager.left_controller.transform.pos
            - ControllerManager.right_controller.transform.pos).magnitude);

        if (controllerDistance == 0.0f)
        {
            controllerDistance = distance;
            lScale = ControllerManager.refToMainSplineGO.transform.localScale;
        }

        if (controllerDistance != 0.0f)
        {
            ControllerManager.refToMainSplineGO.transform.localScale = lScale * (distance / controllerDistance);
        }
    }

    void StopScale()
    {
        controllerDistance = 0.0f;
    }

    void TransitionThere()
    {
        if (times == 0)
        {
            times++;
            SplineDecorator sd = ControllerManager.refToMainSplineGO.GetComponent<SplineDecorator>();
            if (GetComponent<DataVisInputs>().transitionParameters.canTransition)
            {
                sd.RTL();
            }
            else
            {
                sd.RTS();
            }
        }
    }

    void NoTransitions()
    {
        times = 0;
    }

    void DoSpawnControllerMenuGUIs()
    {
        if (menuWanted && !transitionedInvis)
        {
            Color originalColor = hexToColor(HEX_STRING_WITH_ALPHA);

            ControllerManager.leftController.transform.FindChild("Button Key").GetComponentInChildren<MeshFilter>().
            gameObject.GetComponent<MeshRenderer>().sharedMaterial.SetColor("_TintColor", originalColor);

            ControllerManager.rightController.transform.FindChild("Button Key").GetComponentInChildren<MeshFilter>().
            gameObject.GetComponent<MeshRenderer>().sharedMaterial.SetColor("_TintColor", originalColor);

            transitionedInvis = true;
        } else if (!menuWanted)
        {
            //TODO: Not duplicate almost same code twice... Just lazy for now :)
            GameObject menuHolderLeft = GameObject.Find("Controller (left)").transform.Find("Button Key").gameObject;

            MeshRenderer[] textInformationL = menuHolderLeft.GetComponentsInChildren<MeshRenderer>();
            for (int textID = 0; textID < textInformationL.Length; textID++)
            {
                textInformationL[textID].gameObject.SetActive(false);
            }

            GameObject menuHolderRight = GameObject.Find("Controller (right)").transform.Find("Button Key").gameObject;

            MeshRenderer[] textInformationR = menuHolderRight.GetComponentsInChildren<MeshRenderer>();
            for (int textID = 0; textID < textInformationR.Length; textID++)
            {
                textInformationR[textID].gameObject.SetActive(false);
            }
        }
    }

    public void DoRigidbodyFreeze()
    {
        if (ControllerManager.refToMainSplineGO.GetComponent<Rigidbody>() != null)
        {
            ControllerManager.refToMainSplineGO.GetComponent<Rigidbody>().constraints
                = RigidbodyConstraints.FreezeAll;
        }
        foreach (Rigidbody rigidbody in ControllerManager.refToMainSplineGO.GetComponentsInChildren<Rigidbody>())
        {
            rigidbody.constraints = RigidbodyConstraints.FreezeAll;
        }
    }

    void DoGripPressed()
    {
        ResetRotators();
        ResetGridImage();
        DataProcessor.ReadAndProcessData(dataReadParameters.sizeOfClusterRead);
        ControllerManager.refToMainSplineGO.GetComponent<SplineDecorator>().AddInData(DataProcessor.GetAllMasterNodes());
        DataProcessor.ClearTempCluster();

        if (masterGUIHandler.calledConnectionInfo && masterGUIHandler.isOnArticle)
        {
            foreach (KeyValuePair<string, MasterNode> articleToMasterNode in DataProcessor.articleContainerDictionary)
            {
                for (int levelIndex = 0; levelIndex < articleToMasterNode.Value.MasterNodeGameObjects.Length; levelIndex++)
                {
                    if (articleToMasterNode.Value.MasterNodeGameObjects[levelIndex] != null)
                    {
                        Transform[] activeGOs = articleToMasterNode.Value.MasterNodeGameObjects[levelIndex].GetComponent<SplineDecorator>().elements;
                        foreach (Transform checkActiveGO in activeGOs)
                        {
                            if (checkActiveGO != null && articleToMasterNode.Value.MasterNodeGameObjects[levelIndex] == checkActiveGO.gameObject)
                            {
                                articleToMasterNode.Value.MasterNodeGameObjects[levelIndex] = checkActiveGO.gameObject;
                            }
                        }
                    }
                }
            }

            ControllerManager.refToMainSplineGO.GetComponent<SplineDecorator>().CallAlgorithm();
            //What I can do is access the positions of the line renderer component on of the sphere and just update those positions with the current GO positions on the sphere.
            //This way, we don't have to call the algorithm every time, which may take more time then needed

        }
    }

    void DoGripPressed(ControllerInteractionEventArgs e)
    {
        ResetRotators();
        ResetGridImage();
        DataProcessor.ReadAndProcessData(dataReadParameters.sizeOfClusterRead);
        ControllerManager.refToMainSplineGO.GetComponent<SplineDecorator>().AddInData(DataProcessor.GetAllMasterNodes());
        DataProcessor.ClearTempCluster();

        if (masterGUIHandler.calledConnectionInfo && masterGUIHandler.isOnArticle)
        {
            foreach (KeyValuePair<string, MasterNode> articleToMasterNode in DataProcessor.articleContainerDictionary)
            {
                for (int levelIndex = 0; levelIndex < articleToMasterNode.Value.MasterNodeGameObjects.Length; levelIndex++)
                {
                    if (articleToMasterNode.Value.MasterNodeGameObjects[levelIndex] != null)
                    {
                        Transform[] activeGOs = articleToMasterNode.Value.MasterNodeGameObjects[levelIndex].GetComponent<SplineDecorator>().elements;
                        foreach (Transform checkActiveGO in activeGOs)
                        {
                            if (checkActiveGO != null && articleToMasterNode.Value.MasterNodeGameObjects[levelIndex] == checkActiveGO.gameObject)
                            {
                                articleToMasterNode.Value.MasterNodeGameObjects[levelIndex] = checkActiveGO.gameObject;
                            }
                        }
                    }
                }
            }

            ControllerManager.refToMainSplineGO.GetComponent<SplineDecorator>().CallAlgorithm();
            //What I can do is access the positions of the line renderer component on of the sphere and just update those positions with the current GO positions on the sphere.
            //This way, we don't have to call the algorithm every time, which may take more time then needed

        }
    }

    void DoGripReleased()
    {
    }

    void DoGripReleased(ControllerInteractionEventArgs e)
    {
    }

    bool calledLoadin = false;

    void DoTouchpadPressed()
    {
        if (masterGUIHandler.isOnArticle && !calledLoadin)
        {
            masterGUIHandler.EnableAlgorithm();
            calledLoadin = true;
        }
    }

    void DoTouchpadPressed(ControllerInteractionEventArgs e)
    {
        if (masterGUIHandler.isOnArticle && !calledLoadin && SteamVR_InteractTouch.controllerOnDatapoint)
        {
            masterGUIHandler.EnableAlgorithm(); //this method should call method inside the Dataprocessor class
            calledLoadin = true;
        }
    }

    void DoTouchpadReleased()
    {
        calledLoadin = false;
    }

    void DoTouchpadReleased(ControllerInteractionEventArgs e)
    {
        calledLoadin = false;
    }

    void DoTriggerPressed(ControllerInteractionEventArgs e)
    {
        if (dataPointToUse != null && contrToUse != null && contrToUse.controllerIndex == e.controllerIndex)
        {
            dataPointToUse.GetComponent<SplineDecorator>().ExpandDataPoint();
        }
    }

    void DoTriggerReleased(ControllerInteractionEventArgs e)
    {
        contrToUse = null;
        dataPointToUse = null;
    }

    void DoMenuReleased(ControllerInteractionEventArgs e)
    {
        ToggleMenu(e);
    }

    void DoChangeOnKeyboardInput(KeyboardInteractionEventArgs e)
    {
        dataReadParameters.rotators = ControllerManager.refToMainSplineGO.GetComponentsInChildren<SplineDecorator>();

        //Reason we start at 1 is because 0 is the parent gameobject
        for (int i = 1; i < dataReadParameters.rotators.Length; i++)
        {
            dataReadParameters.rotators[i].transform.Rotate(0, 0, rotationSpeed * e.direction * Time.deltaTime);
        }

        if (masterGUIHandler.calledConnectionInfo && masterGUIHandler.isOnArticle)
        {
            foreach (KeyValuePair<string, MasterNode> articleToMasterNode in DataProcessor.articleContainerDictionary)
            {
                for (int levelIndex = 0; levelIndex < articleToMasterNode.Value.MasterNodeGameObjects.Length; levelIndex++)
                {
                    if (articleToMasterNode.Value.MasterNodeGameObjects[levelIndex] != null)
                    {
                        Transform[] activeGOs = articleToMasterNode.Value.MasterNodeGameObjects[levelIndex].GetComponent<SplineDecorator>().elements;
                        foreach (Transform checkActiveGO in activeGOs)
                        {
                            if (checkActiveGO != null && articleToMasterNode.Value.MasterNodeGameObjects[levelIndex] == checkActiveGO.gameObject)
                            {
                                articleToMasterNode.Value.MasterNodeGameObjects[levelIndex] = checkActiveGO.gameObject;
                            }
                        }
                    }
                }
            }

            ControllerManager.refToMainSplineGO.GetComponent<SplineDecorator>().CallAlgorithm();
        }

    }

    void DoTouchPadAxisChanged(ControllerInteractionEventArgs e)
    {
        //Once TouchPadAxis has been changed and laser pointer is on GUI, allow a user to navigate through the GUI and scroll through all of the articles
        //Also, when a user is on the GUI, the sphere cannot be manipulated.
        if (ControllerManager.left_controller != null && ControllerManager.right_controller != null)
        {
            if (e.controllerIndex == ControllerManager.right_controller.index && ViveControllerInput.Instance.guiSelected)
            {
                RectTransform guiRectTransform = guiParameters.gridImage.GetComponent<RectTransform>();
                Vector3 tempRectTransform = (guiRectTransform.anchoredPosition3D);
                GameObject numberButtonsGO = GameObject.Find("Button (" + buttonCount + ")");
                //FIXED: Don't have a call on GameObject.Find every time this is being called
                if (tempRectTransform.y >= 0 && numberButtonsGO != null && numberButtonsGO.transform.position.y < guiParameters.bottomGO.transform.position.y
                    && e.touchpadAxis.normalized.y > 0)
                {
                    tempRectTransform.y += Time.deltaTime * e.touchpadAxis.normalized.y * 100;
                }
                else if (tempRectTransform.y > 0 && e.touchpadAxis.normalized.y < 0)
                {
                    tempRectTransform.y += Time.deltaTime * e.touchpadAxis.normalized.y * 100;
                }
                else if (tempRectTransform.y < 0)
                {
                    tempRectTransform.y = 0f;
                }
                guiRectTransform.anchoredPosition3D = tempRectTransform;
            }
            else if (e.controllerIndex == ControllerManager.left_controller.index && !ControllerManager.leftController.GetComponent<SteamVR_LaserPointer>().moving &&
              !ControllerManager.rightController.GetComponent<SteamVR_LaserPointer>().moving)
            {
                dataReadParameters.rotators = ControllerManager.refToMainSplineGO.GetComponentsInChildren<SplineDecorator>();
                if (e.touchpadAxis.normalized.y != 0)
                {
                    //Reason we start at 1 is because 0 is the parent gameobject
                    for (int i = 1; i < dataReadParameters.rotators.Length; i++)
                    {
                        dataReadParameters.rotators[i].transform.Rotate(0, 0, .6f * e.touchpadAxis.normalized.y);
                    }

                    if (masterGUIHandler.calledConnectionInfo && masterGUIHandler.isOnArticle)
                    {
                        foreach (KeyValuePair<string, MasterNode> articleToMasterNode in DataProcessor.articleContainerDictionary)
                        {
                            for (int levelIndex = 0; levelIndex < articleToMasterNode.Value.MasterNodeGameObjects.Length; levelIndex++)
                            {
                                if (articleToMasterNode.Value.MasterNodeGameObjects[levelIndex] != null)
                                {
                                    Transform[] activeGOs = articleToMasterNode.Value.MasterNodeGameObjects[levelIndex].GetComponent<SplineDecorator>().elements;
                                    foreach (Transform checkActiveGO in activeGOs)
                                    {
                                        if (checkActiveGO != null && articleToMasterNode.Value.MasterNodeGameObjects[levelIndex] == checkActiveGO.gameObject)
                                        {
                                            articleToMasterNode.Value.MasterNodeGameObjects[levelIndex] = checkActiveGO.gameObject;
                                        }
                                    }
                                }
                            }
                        }

                        //masterGUIHandler.HighlightConnectionsbyCoauthor();
                    }
                }
            }
        }
    }

    /*----------------------------------------------------Everything below is a helper function, used in this class, and others.-------------------------------------------------*/
    private void EnableForExpand(Transform hitObj)
    {
        if (hitObj.parent.parent)
        {
            hitObj.parent.parent.GetComponent<SplineDecorator>().DimSurroundingVisuals(hitObj.parent);
        }
        else
        {
            print("not found");
        }
        string titleStr;
        string catStr;
        if (hitObj.parent)
        {
            if (hitObj.GetComponent<SplineDecorator>().datasetCategory == SplineDecorator.DatasetCategory.Categories)
            {
                titleStr = hitObj.GetComponent<SplineDecorator>().title;
                catStr = hitObj.transform.parent.parent.GetComponent<SplineDecorator>().title;
            }
            else if (hitObj.GetComponent<SplineDecorator>().datasetCategory == SplineDecorator.DatasetCategory.Singleton)
            {
                titleStr = hitObj.GetComponent<SplineDecorator>().title;
                catStr = "Article";
            }
            else if (hitObj.GetComponent<SplineDecorator>().datasetCategory == SplineDecorator.DatasetCategory.Years)
            {
                titleStr = hitObj.GetComponent<SplineDecorator>().title;
                catStr = hitObj.transform.parent.GetComponent<SplineDecorator>().title;
            }
            else
            {
                titleStr = "err";
                catStr = "cat err";
            }
        }
        else
        {
            titleStr = hitObj.GetComponent<SplineDecorator>().title;
            catStr = "DBPL";
        }
        //string str = collider.GetComponent<SplineDecorator> ().DataSetStrings [0];

        Transform gridImage = GameObject.FindGameObjectWithTag("Grid Image").transform;

        int breakPoint = hitObj.GetComponent<SplineDecorator>().DataSetStrings.Count;

        Button button = null;
        buttonCount = 0;
        ResetGridImage();

        masterGUIHandler.DestroyButtons();

        for (int i = 0; i < breakPoint; i++)
        {
            buttonCount++;

            button = Instantiate(masterGUIHandler.MenuButton);
            button.transform.GetChild(0).GetComponent<Text>().text = hitObj.GetComponent<SplineDecorator>().DataSetStrings[i];
            /* This will retain local orientation and scale rather than world orientation and scale, which can prevent
             * common UI scaling issues.*/
            button.transform.SetParent(gridImage, false); //originally, this was button.transform.parent = gridImage;
            button.transform.localScale = Vector3.one;
            button.transform.name = "Button (" + buttonCount + ")";

        }

        masterGUIHandler.SetTitle(catStr + " - " + titleStr);

        //TODO: Don't pass in buttonCount here as it can just be done from the ButtonCount getter/setter
        AllowExpansion(hitObj.gameObject, this.gameObject.GetComponent<SteamVR_TrackedController>());
        ButtonCount = buttonCount;
    }

    public int ButtonCount
    {
        get { return buttonCount; }
        set { buttonCount = value; }
    }

    public void AllowExpansion(GameObject dot, SteamVR_TrackedController contrObject)
    {
        dataPointToUse = dot;
        contrToUse = contrObject;
    }

    public void ResetRotators()
    {
        dataReadParameters.rotators = ControllerManager.refToMainSplineGO.GetComponentsInChildren<SplineDecorator>();

        //Reason we start at 1 is because 0 is the parent gameobject
        for (int i = 1; i < dataReadParameters.rotators.Length; i++)
        {
            Quaternion tempRot = dataReadParameters.rotators[i].transform.localRotation;
            Vector3 tempEuler = tempRot.eulerAngles;
            tempEuler.z = 0f;
            tempRot.eulerAngles = tempEuler;
            dataReadParameters.rotators[i].transform.localRotation = tempRot;
        }
    }

    public void ResetGridImage()
    {
        RectTransform guiRectTransform = guiParameters.gridImage.GetComponent<RectTransform>();
        guiRectTransform.anchoredPosition3D = Vector3.zero;
    }

    public void ResetSphere()
    {
        ControllerManager.refToMainSplineGO.transform.localPosition = Vector3.zero;
        ControllerManager.refToMainSplineGO.transform.rotation = Quaternion.identity;
        ControllerManager.refToMainSplineGO.transform.localScale = Vector3.one;
    }

    private void ToggleMenu(ControllerInteractionEventArgs e)
    {
        if (!transitionHappening)
        {
            menuBool = (e.controllerIndex == ControllerManager.left_controller.index) ? menuOnL : menuOnR;
            if (menuBool)
            {
                if (e.controllerIndex == ControllerManager.left_controller.index)
                {
                    //leftLine.gameObject.SetActive(false);
                    //rightLine.gameObject.SetActive(false);
                    StartCoroutine(FadeOut(0.3f, true));
                }
                else
                {
                    StartCoroutine(FadeOut(0.3f, false));
                }
            }
            else if (!menuBool)
            {
                if (e.controllerIndex == ControllerManager.left_controller.index)
                {
                    //leftLine.gameObject.SetActive(true);
                    StartCoroutine(FadeIn(0.3f, true));
                }
                else
                {
                    //rightLine.gameObject.SetActive(true);
                    StartCoroutine(FadeIn(0.3f, false));
                }
            }
        }
    }

    IEnumerator FadeOut(float secs, bool isLeft)
    {

        SteamVR_TrackedObject controller = (isLeft) ? ControllerManager.leftController :
            ControllerManager.rightController;
        transitionHappening = true;

        string loadResource = (isLeft) ? "Materials/Unlit UI L" : "Materials/Unlit UI R";

        Material mat = Resources.Load(loadResource, typeof(Material)) as Material;
        //controller.transform.FindChild("Button Key").GetChild(0).GetComponent<Renderer>().material;

        float t = 0f;
        Color start;
        bool tint = false;
        if (mat.HasProperty("_TintColor"))
        {
            start = mat.GetColor("_TintColor");

            if (start.r < 0.1f && start.g > 0.9f && start.b < .2f)
                start = Color.white;
            tint = true;
        }
        else
        {
            start = mat.color;
        }
        start = new Color(start.r, start.g, start.b, .3f);
        Color endCol = new Color(start.r, start.g, start.b, 0f);
        while (t < 1.0f)
        {
            foreach (TextMesh tm in controller.transform.FindChild("Button Key").GetComponentsInChildren<TextMesh>())
            {
                tm.color = Color.Lerp(guiParameters.originalColor, Color.clear, t);
            }

            if (tint)
                mat.SetColor("_TintColor", Color.Lerp(start, endCol, t / 1f));
            else
                mat.color = Color.Lerp(start, endCol, t / 1f);
            t += Time.deltaTime * 4f;
            yield return new WaitForFixedUpdate();
        }
        mat.SetColor("_TintColor", endCol);
        transitionHappening = false;
        if (isLeft)
            menuOnL = false;
        else
            menuOnR = false;

        yield return null;
    }

    IEnumerator FadeIn(float secs, bool isLeft)
    {

        SteamVR_TrackedObject controller = (isLeft) ? ControllerManager.leftController :
            ControllerManager.rightController;
        transitionHappening = true;

        string loadResource = (isLeft) ? "Materials/Unlit UI L" : "Materials/Unlit UI R";

        Material mat = Resources.Load(loadResource, typeof(Material)) as Material;
        //controller.transform.FindChild("Button Key").GetChild(0).GetComponent<Renderer>().material;

        float t = 0f;
        Color start;
        bool tint = false;
        if (mat.HasProperty("_TintColor"))
        {
            start = mat.GetColor("_TintColor");
            tint = true;
        }
        else
        {
            start = mat.color;
        }
        start = new Color(start.r, start.g, start.b, 0f);
        Color endCol = new Color(start.r, start.g, start.b, 1f);

        while (t < 1.0f)
        {
            foreach (TextMesh tm in controller.transform.FindChild("Button Key").GetComponentsInChildren<TextMesh>())
            {
                tm.color = Color.Lerp(Color.clear, guiParameters.originalColor, t);
            }
            if (tint)
                mat.SetColor("_TintColor", Color.Lerp(start, endCol, t / 1f));
            else
                mat.color = Color.Lerp(start, endCol, t / 1f);
            t += Time.deltaTime * 4f;
            yield return new WaitForFixedUpdate();
        }
        mat.SetColor("_TintColor", endCol);
        transitionHappening = false;
        if (isLeft)
            menuOnL = true;
        else
            menuOnR = true;
        yield return null;
    }


}
