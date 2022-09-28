using System.Collections;
using System.Collections.Generic;
using UnityAtoms.BaseAtoms;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(ScrollRect))]
public class MenuScroll : MonoBehaviour
{
    [Header("refs")]
    [Tooltip("the current page")]
    [SerializeField] IntVariable m_CurrentPage;

    /// the scrollrect this menu controls
    ScrollRect m_ScrollRect;

    /// the current scroll
    float m_Scroll;


    // -- lifecycle --
    void Awake()
    {
        m_ScrollRect = GetComponent<ScrollRect>();
        m_CurrentPage.Changed.Register(OnPageChanged);
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void OnPageChanged(int page) {

        // unity's scroll rect goes from the top of the first object to the bottom of the last
        // so for this number it seems like there's one fewer page
        m_Scroll = page / ((float)PageCount -1);

        // unity's scroll rect goes from 1 to 0, so we invert it here
        m_ScrollRect.verticalNormalizedPosition = 1.0f - m_Scroll;
    }

    // queries
    /// the number of pages the menu contains
    private int PageCount {
        get => m_ScrollRect.content.childCount;
    }
}
