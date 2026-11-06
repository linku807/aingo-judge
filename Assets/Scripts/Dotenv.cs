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
                Debug.LogError($".env 파일을 찾을 수 없습니다! 현재 탐색 경로: {path}");
                return;
            }

            var lines = File.ReadAllLines(path);
            foreach (var line in lines)
            {
                if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#")) continue;
                var parts = line.Split(new char[] { '=' }, 2);
                if (parts.Length != 2) continue;

                var key = parts[0].Trim();
                var value = parts[1].Trim().Replace("\"", ""); // 따옴표가 있다면 제거
                envValues[key] = value;
            }
        }

        public static string Get(string key, string defaultValue = "")
        {
            // 만약 .env 파일이 Assets 폴더 내부에 있다면 Path.GetDirectoryName()을 지우고 Application.dataPath만 쓰세요.
            string env_path = System.IO.Path.Combine(UnityEngine.Application.dataPath, "Scripts", ".env");
            
            LoadEnv(env_path);
            return envValues.TryGetValue(key, out var value) ? value : defaultValue;
        }
    }
}
