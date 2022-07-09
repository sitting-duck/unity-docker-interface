using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Diagnostics;
using System.IO;

public class Docker : MonoBehaviour
{

    private Process process = null;
    private const string conainerName = "containerName";    

    #if UNITY_STANDALONE_WIN
    private static readonly string shell = @"cmd.exe";
    private static readonly string command = @"/C docker run --rm -v {0} -t {1}";

    private static readonly string volumeDir = @"\containerMountDir";

    private void Awake()
    {
        PressResimulateEvent.Instance.AddListener(runDocker);
        UnityEngine.Debug.Log("Persistent Data Path: " + Application.persistentDataPath + volumeDir);
    }

    public void runDocker()
  {    
    if(process != null) {
      UnityEngine.Debug.LogWarning("Docker process still running. ");
      return;
    }

    // Create mount directory for the docker container to mount with the -v flag.    
    if (!Directory.Exists(Application.persistentDataPath + volumeDir)) {
      Directory.CreateDirectory(Application.persistentDataPath + volumeDir);
    }

    // delete all files from the last run
    DirectoryInfo directory = new DirectoryInfo(Application.persistentDataPath + volumeDir);
    foreach(FileInfo file in directory.GetFiles()) {
      file.Delete();
    }   

    UnityEngine.Debug.LogWarning("Spinning up Docker Container");
    ProcessStartInfo startInfo = new ProcessStartInfo(shell);

    string mountPath = Application.persistentDataPath + volumeDir + ":" + volumeDir;

    startInfo.Arguments = string.Format(command, mountPath, conainerName);
    startInfo.StandardOutputEncoding = System.Text.Encoding.UTF8;
    startInfo.StandardOutputEncoding = System.Text.Encoding.UTF8;
    startInfo.RedirectStandardOutput = true;
    startInfo.RedirectStandardError = true;
    startInfo.UseShellExecute = false;
    startInfo.CreateNoWindow = true;

    process = new Process();
    process.StartInfo = startInfo;
    process.OutputDataReceived += OutputDataReceived;
    process.ErrorDataReceived += ErrorDataReceived;

    process.Start();
    process.BeginOutputReadLine();
    process.BeginErrorReadLine();

    StartCoroutine(WaitForProcessToEnd());
  }

  private IEnumerator WaitForProcessToEnd()
  {
    while (!process.HasExited) {
      yield return null;
    }

    OnProcessExit();
  }

  public void StopSimulation()
  {
    if (process != null) {
      process.Dispose();
      process = null;
    }
  }

  private void OnProcessExit()
  {
    UnityEngine.Debug.LogWarning("*****Process Exited*****");
    process.Dispose();
    process = null;
  }

  private static void OutputDataReceived(object sender, DataReceivedEventArgs args)
  {
    if (args.Data != null) {
      UnityEngine.Debug.Log(args.Data);
    }
  }

  private static void ErrorDataReceived(object sender, DataReceivedEventArgs args)
  {
    if (args.Data != null) {
      UnityEngine.Debug.LogError(args.Data);
    }
  }

}
