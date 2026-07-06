using UnityEngine;
using Manager;
using Unity.Cinemachine;

public class MovingCamera : MonoBehaviour
{
    private Scenes self = Scenes.main; // 자기 자신의 구도 선언 및 기본값 지정
    private CinemachineCamera virtualCamera; //시네머신 카메라 선언
    void OnEnable() => GlobalManager.OnSceneChanged += OnSceneChanged;
    void OnDisable() => GlobalManager.OnSceneChanged -= OnSceneChanged;
    // 장면 변환 감지 선언
    void Start()
    {
        virtualCamera = GetComponent<CinemachineCamera>(); // 실제 시네머신 카메라 객체 대입

        if(this.gameObject.name == "main camera") self = Scenes.main;
        else if(this.gameObject.name == "right camera") self = Scenes.byunhosa;
        else if(this.gameObject.name == "top camera") self = Scenes.jepanjang;
        else if(this.gameObject.name == "left camera") self = Scenes.gumsa;
        else if(this.gameObject.name == "whole camera") self = Scenes.whole;
        // 자기 자신 찾아 지정
    }

    // Update is called once per frame
    void OnSceneChanged(Scenes newScene)
    {
        if (newScene == self)
        {
            virtualCamera.Priority = 1; // 시네 머신 우선 순위를 높여 카메라 위치 변환
        }else{
            virtualCamera.Priority = 0;
        }
    }
}
