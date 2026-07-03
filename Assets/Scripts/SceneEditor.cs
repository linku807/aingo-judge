using UnityEngine;
using Manager;

public class ButtonController : MonoBehaviour
{
    public void EditScene()
    {
        if (GlobalManager.Scene == Scenes.main)
        {
            GlobalManager.Scene = Scenes.byunhosa;
        }
        else if (GlobalManager.Scene == Scenes.byunhosa)
        {
            GlobalManager.Scene = Scenes.gumsa;
        }
        else if (GlobalManager.Scene == Scenes.gumsa)
        {
            GlobalManager.Scene = Scenes.jepanjang;
        }
        else if (GlobalManager.Scene == Scenes.jepanjang)
        {
            GlobalManager.Scene = Scenes.whole;
        }
        else if (GlobalManager.Scene == Scenes.whole)
        {
            GlobalManager.Scene = Scenes.main;
        }
    }
}