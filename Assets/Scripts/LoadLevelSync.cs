using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class LoadLevelSync : MonoBehaviour {

    private Valve.VR.EVRButtonId gripButton = Valve.VR.EVRButtonId.k_EButton_Grip;

    //Vive controller
    private SteamVR_Controller.Device controller { get { return SteamVR_Controller.Input((int)trackedObject.index); } }
    private SteamVR_TrackedObject trackedObject;

    public int levelToLoad;
    public TextMesh loadingText;

    int loadProgress = 0;

    void Start() {
        trackedObject = GetComponent<SteamVR_TrackedObject>();
    }

    void Update()
    {
        if (controller != null && controller.GetPressDown(gripButton)) {
                StartCoroutine("LoadLevelAsync");
        }
    }

    IEnumerator LoadLevelAsync()
    {
        loadingText.text = "Loading Process: \n" + "\t" + loadProgress + "%";
        AsyncOperation async = SceneManager.LoadSceneAsync(levelToLoad);
        while (!async.isDone)
        {
            loadProgress = (int)(async.progress * 100);
            loadingText.text = "Loading Process: \n" + "\t" + loadProgress + "%";
            yield return null;
        }
        
    }

}
