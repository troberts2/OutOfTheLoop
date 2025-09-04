using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CosmeticPicker : MonoBehaviour
{
    [Header("General")]
    [SerializeField] private Color panelActiveColor;
    [SerializeField] private Color panelInactiveColor;

    [Header("Hat Panel")]
    [SerializeField] private GameObject hatPanel;
    [SerializeField] private Button hatPanelButton;

    [Header("Shirt Panel")]
    [SerializeField] private GameObject shirtPanel;
    [SerializeField] private Button shirtPanelButton;

    [Header("Trail Panel")]
    [SerializeField] private GameObject trailPanel;
    [SerializeField] private Button trailPanelButton;

    public void HatPanelButton()
    {
        
    }
}
