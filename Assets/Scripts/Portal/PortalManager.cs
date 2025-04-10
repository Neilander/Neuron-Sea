using UnityEngine;
using System.Collections.Generic;

public class PortalManager : MonoBehaviour
{
    [System.Serializable]
    public class PortalPair
    {
        public Portal portalA;
        public Portal portalB;
        [Tooltip("传送门对是否激活")]
        public bool isActive = true;
    }

    [Header("传送门设置")]
    [Tooltip("传送门对列表")]
    public List<PortalPair> portalPairs = new List<PortalPair>();

    private void Start()
    {
        InitializePortals();
    }

    private void InitializePortals()
    {
        foreach (var pair in portalPairs)
        {
            if (pair.portalA != null && pair.portalB != null)
            {
                // 设置传送门目标
                pair.portalA.targetPortal = pair.portalB.transform;
                pair.portalB.targetPortal = pair.portalA.transform;

                // 设置激活状态
                pair.portalA.SetActive(pair.isActive);
                pair.portalB.SetActive(pair.isActive);
            }
            else
            {
                Debug.LogWarning("传送门对配置不完整，请检查设置");
            }
        }
    }

    public void SetPortalPairActive(int index, bool active)
    {
        if (index >= 0 && index < portalPairs.Count)
        {
            var pair = portalPairs[index];
            pair.isActive = active;

            if (pair.portalA != null)
            {
                pair.portalA.SetActive(active);
            }
            if (pair.portalB != null)
            {
                pair.portalB.SetActive(active);
            }
        }
        else
        {
            Debug.LogWarning($"传送门对索引 {index} 超出范围");
        }
    }

    public void SetAllPortalPairsActive(bool active)
    {
        for (int i = 0; i < portalPairs.Count; i++)
        {
            SetPortalPairActive(i, active);
        }
    }
}