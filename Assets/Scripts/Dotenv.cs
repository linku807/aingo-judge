using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Dotenv
{
    public class EnvLoader
    {
        private static Dictionary<string, string> envValues = new Dictionary<string, string>();

        public static void LoadEnv(string path)
        {
            envValues.Clear();

            if (!File.Exists(path))
            {
                // 🔍 디버깅용: 파일이 없으면 유니티 콘솔에 실제 탐색한 경로를 출력해 줍니다.
                Debug.LogError($".env 파일을 찾을 수 없습니다! 현재 탐색 경로: {path}");
                return;
            }

            var lines = File.ReadAllLines(path);
            foreach (var line in lines)
            {
                if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#")) continue;

                // 🛠️ 수정 포인트 2: Split('=') 대신 Split('=', 2)를 사용합니다.
                // API Key 내부에 '=' 문자가 포함되어 있어도 정상적으로 자르기 위함입니다.
                var parts = line.Split(new char[] { '=' }, 2);
                if (parts.Length != 2) continue;

                var key = parts[0].Trim();
                var value = parts[1].Trim().Replace("\"", ""); // 따옴표가 있다면 제거
                envValues[key] = value;
            }
        }

        public static string Get(string key, string defaultValue = "")
        {
            // 🛠️ 수정 포인트 1: Application.dataPath(Assets폴더)의 부모 폴더(최상위 폴더)로 경로 지정
            // 만약 .env 파일이 Assets 폴더 내부에 있다면 Path.GetDirectoryName()을 지우고 Application.dataPath만 쓰세요.
            string env_path = System.IO.Path.Combine(UnityEngine.Application.dataPath, "Scripts", ".env");//Path.Combine(Path.GetDirectoryName(Application.dataPath), ".env");
            
            LoadEnv(env_path);
            return envValues.TryGetValue(key, out var value) ? value : defaultValue;
        }
    }
}
