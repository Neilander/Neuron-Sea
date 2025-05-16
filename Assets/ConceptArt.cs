using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConceptArt : MonoBehaviour
{
    [SerializeField] private Transform imgParent; // ¸¸ÎïÌå
    public void ShowPic(int index)
    {
        Debug.Log("ShowPic called with index: " + index);
        gameObject.SetActive(true);
        // Hide all images
        foreach (Transform child in imgParent)
        {
            child.gameObject.SetActive(false);
        }
        // Show the selected image
        if (index >= 0 && index < imgParent.childCount)
        {
            imgParent.GetChild(index).gameObject.SetActive(true);
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            GetComponent<ClickAndExit>().Exit();
        }
    }
}
