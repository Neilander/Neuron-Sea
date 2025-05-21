using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RightUpControl : MonoBehaviour
{
    public List<GameObject> beingControlled;
    private bool previousState;

    void Start()
    {
        previousState = !ActivityGateCenter.IsStateActiveAny(ActivityState.Story, ActivityState.StartEffectMove);
        SetAllChildrenActive(previousState);
    }

    void Update()
    {
        bool current = !ActivityGateCenter.IsStateActiveAny(ActivityState.Story, ActivityState.StartEffectMove);
        print("现在应该显示"+current);
        if (current != previousState)
        {
            SetAllChildrenActive(current);
            previousState = current;
        }
    }

    void SetAllChildrenActive(bool active)
    {
        foreach (GameObject child in beingControlled)
        {
            child.SetActive(active);
        }
    }
}
