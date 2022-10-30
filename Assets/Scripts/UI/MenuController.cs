using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System;

public class MenuController : MonoBehaviour
{

    [SerializeField] GameObject menu;

    List<Text> menuItems;
    public event Action<int> onMenuSelected;
    public event Action onBack;

    int selectedItem = 0;

    private void Awake()
    {
        menuItems = menu.GetComponentsInChildren<Text>().ToList();
    }

    public void OpenMenu()
    {
        menu.SetActive(true);
        UpdateItemSelection();
    }

    public void CloseMenu()
    {
        menu.SetActive(false);
    }

    
    public void HandleUpdate()
    {
        int prevSelection = selectedItem;
        if (Input.GetKeyUp(KeyCode.DownArrow))
        {
            ++selectedItem;
        }
        else
        {
            if (Input.GetKeyUp(KeyCode.UpArrow))
            {
                --selectedItem;
            }
        }

        selectedItem = Mathf.Clamp(selectedItem, 0, menuItems.Count - 1);
        
        if(prevSelection != selectedItem)
        {
            UpdateItemSelection();
        }

        if (Input.GetKeyUp(KeyCode.Return))
        {
            onMenuSelected?.Invoke(selectedItem);
            CloseMenu();
        }
        else
        {
            if (Input.GetKeyUp(KeyCode.Escape))
            {
                onBack?.Invoke();
                CloseMenu();
            }
        }

    }

    void UpdateItemSelection()
    {
        for(int i = 0; i < menuItems.Count; i++)
        {
            if(i== selectedItem)
            {
                menuItems[i].color = GlobalSetting.i.HighlightedColor;
            }
            else
            {
                menuItems[i].color = Color.black;
            }
        }
    }

}
