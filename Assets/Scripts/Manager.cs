using UnityEngine;
using System;

namespace Manager
{
    // 카메라 구도를 편하게 관리하고 확인하기 위한 열거형
    public enum Scenes
    {
        main,
        whole,
        gumsa,
        byunhosa,
        jepanjang,
    }

    public static class GlobalManager
    {
        private static Scenes scene = Scenes.main; // 카메라 구도 선언 및 기본값 세팅(main)

        public static event Action<Scenes> OnSceneChanged; // 카메라 구도 변경을 감지 하는 이벤트 선언

        public static Scenes Scene
        {
            get => scene;
            set
            {
                if (scene == value) return;
                scene = value;

                OnSceneChanged?.Invoke(scene);
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Initialize()
        {
            scene = Scenes.main;
            
        }
    }
}