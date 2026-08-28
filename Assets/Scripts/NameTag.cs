using UnityEngine;
using TMPro;
using System.Collections.Generic;
using Manager;

public class nametag : MonoBehaviour
{   
    [SerializeField] private TextMeshProUGUI name;

    Dictionary<string, string> names = new Dictionary<string, string>()
    {
        {"main","최피고"},
        {"whole", "모두"},
        {"gumsa","박검사"},
        {"byunhosa", "김변호"},
        {"jepanjang", "이재판"}
    };

    void OnEnable() => GlobalManager.OnSceneChanged += OnSceneChanged;
    void OnDisable() => GlobalManager.OnSceneChanged -= OnSceneChanged;
    //카메라 구도 변경 감지

    void OnSceneChanged(Scenes newScene)
    {
        name.text = names[newScene.ToString()];
    }
}
