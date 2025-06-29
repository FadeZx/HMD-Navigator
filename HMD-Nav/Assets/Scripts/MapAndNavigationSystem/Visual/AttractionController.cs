using UnityEngine;
using TMPro;
using System.Data; // Don't forget this for TextMeshPro
using UnityEngine.UI;
using System.Collections;

public class AttractionController : MonoBehaviour
{
    [Header("Attraction Visuals")]
    public GameObject mapPin; // Prefab for the attraction
    public Animator pinAnimator;
    public RawImage attractionImageDisplay;  // RawImage component on the object
    public Texture attractionTexture;        // Texture to show in RawImage
    public AudioSource soundEffect;

    [Header("Navigation")]
    public NavNode destinationNode;
    public GameObject confirmPanel; // Panel with the confirmation UI
    public TextMeshProUGUI destinationTitleTMP; // UI text field for displaying the destination name
    public TextMeshProUGUI popupTitleTMP; // UI text field for displaying the destination name
    public GameObject destinationButton; // Button to confirm navigation

    private bool isActivated = false;

    private void Start()
    {

        if (destinationNode == null)
        {
            Debug.LogWarning($"{name} has no destination node assigned.");
            return;
        }

       
        if (confirmPanel != null)
        {
            confirmPanel.SetActive(false);
        }

        if(mapPin != null)
        {
            mapPin.SetActive(false); // Hide the pin initially
        }
        // Set the destination title from the NavNode
        if (destinationTitleTMP != null && destinationNode != null)
        {
            destinationTitleTMP.text = destinationNode.nodeID;
           
        }
        if (popupTitleTMP != null && popupTitleTMP != null)
        {
            popupTitleTMP.text = destinationNode.nodeID;

        }
        

        if (attractionImageDisplay != null && attractionTexture != null)
        {
            attractionImageDisplay.texture = attractionTexture;
            
        }
    }


    public void ConfirmNavigation()
    {
      
    }

    public void CancelNavigation()
    {
        isActivated = false;

        if (pinAnimator != null)
            pinAnimator.SetBool("isActive", false);

        if (mapPin != null)
            mapPin.SetActive(false);

        NavigationPopup navPopup = FindFirstObjectByType<NavigationPopup>();
        if (navPopup != null)
            navPopup.Cancel();

        if (soundEffect != null)
            soundEffect.Play();

        // Delay the panel deactivation by one frame
        StartCoroutine(DeferredCancelPanel());
    }

    private IEnumerator DeferredCancelPanel()
    {
        yield return null; // Wait 1 frame to let hover system settle

        if (confirmPanel != null)
            confirmPanel.SetActive(false);

        if (destinationButton != null)
            destinationButton.SetActive(true);
    }

    public void SetThisDestination()
    {
        isActivated = true;
        NavigationPopup navPopup = FindFirstObjectByType<NavigationPopup>();
        if (navPopup == null)
        {
            Debug.LogError("No NavigationPopup found in scene.");
            return;
        }
        navPopup.SetDestination(destinationNode);
      

        if (mapPin != null)
            mapPin.SetActive(true);

        if (pinAnimator != null)    
            pinAnimator.SetBool("isActive", true);

        if (destinationButton != null)
            destinationButton.SetActive(false);

        if (confirmPanel != null)
            confirmPanel.SetActive(true);

        if (popupTitleTMP != null && popupTitleTMP != null)
        {
            popupTitleTMP.text = destinationNode.nodeID;
        }


        if (attractionImageDisplay != null && attractionTexture != null)
        {
            attractionImageDisplay.texture = attractionTexture;
        }




        if (soundEffect != null)
            soundEffect.Play();

        Debug.Log($"{name} set navigation to {destinationNode.nodeID}");

        
    }
}
