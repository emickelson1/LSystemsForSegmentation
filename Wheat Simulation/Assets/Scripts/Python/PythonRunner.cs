using System.Diagnostics;
using UnityEngine;
using System.IO;
public class PythonRunner : MonoBehaviour
{
    public void RunPythonScript(string argument)
    {
        DirectoryManager directoryManager = FindObjectOfType<DirectoryManager>();
        string pythonPath = directoryManager.currentProfile.pythonPath;
        string scriptPath = "Assets/Scripts/Python/updatejson.py";

        string args = $"\"{scriptPath}\" \"{argument}\"";

        ProcessStartInfo startInfo = new ProcessStartInfo
        {
            FileName = pythonPath,
            Arguments = args,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using (Process process = new Process { StartInfo = startInfo })
        {
            process.Start();
            string output = process.StandardOutput.ReadToEnd();
            string error = process.StandardError.ReadToEnd();
            process.WaitForExit();

            if (!string.IsNullOrEmpty(output))
            {
                UnityEngine.Debug.Log($"[Python Output] {output}");
            }

            if (!string.IsNullOrEmpty(error))
            {
                UnityEngine.Debug.Log($"[Python Error] {error}");
            }
        }
    }
}
