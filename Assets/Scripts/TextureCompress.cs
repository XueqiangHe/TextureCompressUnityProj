﻿using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class TextureCompress : MonoBehaviour 
{
    private string toolPath;
    private string pngPath;

    void Start()
    {
        string appData = Path.Combine(Application.dataPath, "Tools");
        switch (Application.platform)
        {
            case RuntimePlatform.WindowsPlayer:
            case RuntimePlatform.WindowsEditor:
                toolPath = Path.Combine(appData, "astcenc-sse2.exe");
                break;
            case RuntimePlatform.OSXPlayer:
            case RuntimePlatform.OSXEditor:
                toolPath = Path.Combine(appData, "astcenc-sse2-arm64");
                break;
        }

        Debug.Log("toolPath: " + toolPath);
        //win: C:\Users\56399\AppData\Roaming\MyTools/astcenc-sse2.exe
        //mac: /Users/hexueqiang/.config/MyTools/astcenc-sse2.exe
        if (!File.Exists(toolPath))
            throw new FileNotFoundException($"Cannot find astc encoder at {Path.GetFullPath(toolPath)}.");

        pngPath = Path.Combine(Application.streamingAssetsPath, "RGBA32.png");
        //pngPath = "E:\\202404\\TextureCompressUnityProj\\Assets\\Textures\\ASTC\\98_OriRGBA32.png";


        ////test
        //string toolFolder = Path.Combine(Application.dataPath, "Tools");
        //string testToolPath = Path.Combine(toolFolder, "astcenc-sse2.exe");
        //Debug.Log(testToolPath);  //E:/202404/TextureCompressUnityProj/build/TextureCompressUnityProj_Data\Tools\astcenc-sse2.exe
        //if (!File.Exists(testToolPath))
        //    Debug.LogError("not exists exe");
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            AstcCompressAsync(pngPath).Forget();
        }
    }

    async UniTaskVoid AstcCompressAsync(string pngPath)
    {
        await AstcCompress(pngPath);
    }

    async UniTask AstcCompress(string path)
    {
        if (!File.Exists(path))
        {
            Debug.LogError("File is not exists: " + path);
            return;
        }

        int extIndex = path.LastIndexOf('.');
        string outputPath = (extIndex != -1) ? path.Substring(0, extIndex) + ".astc" : path + ".astc";

        ProcessStartInfo startInfo = new ProcessStartInfo
        {
            FileName = toolPath,
            Arguments = $" -cl \"{path}\" \"{outputPath}\" 6x6 -medium -yflip",
            UseShellExecute = false,
            RedirectStandardOutput = true,
            CreateNoWindow = true
        };

        using (Process process = new Process { StartInfo = startInfo })
        {
            process.Start();
            await UniTask.WaitUntil(() => process.HasExited); 

            if (process.ExitCode == 0)
            {
                Debug.Log("Texture compressed successfully.");
            }
            else
            {
                Debug.LogError("Texture compression failed.");
            }
        }
    }
}
