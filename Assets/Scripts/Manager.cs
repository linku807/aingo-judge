using UnityEngine;
using System;

namespace Manager
{
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
        private static Scenes scene = Scenes.main;

        public static event Action<Scenes> OnSceneChanged;

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
            Debug.Log("global listener ready");
        }
    }
}