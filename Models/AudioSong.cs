using System;
using System.IO;
using Colossal.IO.AssetDatabase;
using UnityEngine;
using UnityEngine.Networking;

namespace CustomRadio.Models;

public class AudioSong
{
    public string FilePath { get; }
    public string Title { get; set; }
    public string Artist { get; set; }
    public AudioClip Clip { get; set; }

    public AudioSong(string filePath)
    {
        FilePath = filePath;
        LoadAsyncFile();
        UpdateFromMetatags();
    }

    private async void LoadAsyncFile()
    {
        using UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip("file://" + FilePath, AudioType.OGGVORBIS);
        await www.SendWebRequest();
        if (www.result is UnityWebRequest.Result.ConnectionError or UnityWebRequest.Result.ProtocolError)
            Debug.Log(www.error);
        Clip = DownloadHandlerAudioClip.GetContent(www);
        www.Dispose();

        Debug.Log("File loaded: " + Path.GetFileName(FilePath));
    }

    private void UpdateFromMetatags()
    {
        try
        {
            TagLib.File tagFile = TagLib.File.Create(FilePath);
            Title = tagFile.Tag.Title;
            Artist = tagFile.Tag.FirstPerformer;
        }
        catch(Exception e)
        {
            Debug.Log(e.Message);
        }

        Title ??= Path.GetFileNameWithoutExtension(FilePath);
        Artist ??= "";
    }
}