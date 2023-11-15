using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

namespace CustomRadio.MonoBehaviours;

public class MusicLoader : MonoBehaviour
{
    public const string DefaultChannel = "The Second Moon";

    private readonly List<AudioClip> AudioClips = new List<AudioClip>( );
    private readonly string AssemblyLocation = System.Reflection.Assembly.GetExecutingAssembly().Location;
    public static int CurrentIndex { set; get; }
    public static string CurrentChannel { set; get; }

    private void Start()
    {
        DontDestroyOnLoad(this);

        string musicDirectory = Path.Combine(Path.GetDirectoryName(AssemblyLocation), "Music");

        if(Directory.Exists(musicDirectory))
        {
            Debug.Log(musicDirectory + " Found");
            LoadAllAudioClips(musicDirectory);
        }
        else
        {
            Debug.Log("Error: " + musicDirectory + " was not found");
        }
    }

    private void LoadAllAudioClips(string path)
    {
        string[] musicFiles = Directory.GetFiles(path, "*.ogg");

        foreach(string musicFile in musicFiles)
        {
            StartCoroutine(LoadAudioClip(musicFile));
        }

        CurrentIndex = 0;
        CurrentChannel = DefaultChannel;
    }

    private IEnumerator LoadAudioClip(string filePath)
    {
        using UnityWebRequest request = UnityWebRequestMultimedia.GetAudioClip("file://" + filePath, AudioType.OGGVORBIS);
        yield return request.SendWebRequest();

        if(request.result == UnityWebRequest.Result.ConnectionError)
        {
            Debug.LogError("Failed to load AudioClip from: " + filePath);
            Debug.Log(request.error);
        }
        else
        {
            AudioClips.Add(DownloadHandlerAudioClip.GetContent(request));
            Debug.Log("Loaded audio clip: " + filePath);
        }
    }

    public AudioClip GetCurrentClip()
    {
        if(CurrentIndex < 0)
        {
            CurrentIndex = AudioClips.Count - 1;
        }

        if(CurrentIndex > AudioClips.Count - 1)
        {
            CurrentIndex = 0;
        }

        return AudioClips[CurrentIndex];
    }
}