using LDtkUnity;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Cinemachine.DocumentationSortingAttribute;

public class leveldata : MonoBehaviour, ILDtkImportedLevel
{
    [Header("�ؿ��߽�")]
    public Rect levelBound;
    public void OnLDtkImportLevel(LDtkUnity.Level level)
    {
        levelBound = level.UnityWorldSpaceBounds(WorldLayout.GridVania, 36);
    }
}
