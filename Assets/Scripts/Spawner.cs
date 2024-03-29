using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.UI;
using TMPro;

[RequireComponent(typeof(ARTrackedImageManager))]
public class Spawner : MonoBehaviour
{
    private ARTrackedImageManager _imageManager;
    public GameObject[] prefabs;
    public string[] imageNames;
    public TextMeshProUGUI debugText;
    public CanvasGroup infoPanel;
    private Dictionary<int, GameObject> instantiatedPrefabs = new Dictionary<int, GameObject>();
    
    // Awake is called before OnEnable()!
    void Awake()
    {
        _imageManager = GetComponent<ARTrackedImageManager>();
    }

    void Start()
    {
        StartCoroutine(HideInfoPanel());
    }

    void OnEnable()
    {
        _imageManager.trackedImagesChanged += OnImageChange;
    }

    void OnDisable()
    {
        _imageManager.trackedImagesChanged -= OnImageChange;
    }

    IEnumerator HideInfoPanel()
    {
        float counter = 5f;
        while (counter > 1)
        {
            counter -= Time.deltaTime;
            yield return null;
        }
        while (counter > 0)
        {
            counter -= Time.deltaTime;
            infoPanel.GetComponent<CanvasGroup>().alpha = counter;
            yield return null;
        }
        infoPanel.gameObject.SetActive(false);
    }    

    // This function subscribes to trackedImagesChanged and arg are populated by the event
    void OnImageChange(ARTrackedImagesChangedEventArgs eventArgs)
    {
        // Loop through new tracked images that may have been detected, using generic var type
        foreach (var trackedImage in eventArgs.added)
        {
            //Then loop in our string collection for matching names
            for (int i=0; i <imageNames.Length; i++)
            {
                if (imageNames[i] == trackedImage.referenceImage.name)
                {
                    //Then instantiate the corresponding prefab if it hasn't already been spawn
                    if (instantiatedPrefabs.ContainsKey(i) == false)
                    {
                        GameObject GO = GameObject.Instantiate(prefabs[i], trackedImage.transform.position, Quaternion.identity, trackedImage.transform) ;
                        GO.SetActive(true);
                        instantiatedPrefabs.Add(i, GO);
                        debugText.text = GO.name + "spawned!";
                    }
                }
                else
                    debugText.text = "No match...";
            }
        }
        // Loop through removed images
        foreach (var trackedImage in eventArgs.removed)
        {            
            for (int i=0; i <imageNames.Length; i++)
            {
                if (imageNames[i] == trackedImage.referenceImage.name)
                {
                    if (instantiatedPrefabs.Remove(i, out GameObject GO))
                    { 
                        Destroy(GO);
                        debugText.text = GO.name + "destroyed!";
                    }
                }
            }            
        }
        // Loop through updated images
        foreach (var trackedImage in eventArgs.updated)
        {
            for (int i=0; i <imageNames.Length; i++)
            {
                if (imageNames[i] == trackedImage.referenceImage.name)
                {
                    if (instantiatedPrefabs.TryGetValue(i, out GameObject GO))
                    {
                        GO.SetActive(trackedImage.trackingState == TrackingState.Tracking);
                    }
                }
            }
        }
    }
}