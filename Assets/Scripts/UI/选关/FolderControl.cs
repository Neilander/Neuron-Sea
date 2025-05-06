using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FolderControl : MonoBehaviour
{


    public GameObject levels; // Reference to the Levels GameObject

    private bool isExpanded = false;

    public void ToggleLevels(){
        isExpanded = !isExpanded;
        levels.SetActive(isExpanded);
    }
}
