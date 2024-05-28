using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;

public class GeminiRequest : MonoBehaviour
{
    private Root parsedJson = new();
    private TTSResponse parsedTTSJson = new();
    [SerializeField] 
    private TMP_InputField requestInput;
    [SerializeField] 
    private TextMeshProUGUI requestResponse;
    [SerializeField] private AudioSource audioSource;
    
    string ttsURL = "http://localhost:5000/";

    public void SendRequest()
    {
        StartCoroutine(AIRequest($"{{\"contents\":[{{\"parts\":[{{\"text\":\"{requestInput.text}\"}}]}}]}}"));
    }
    
    IEnumerator AIRequest(string requestStr)
    {
        string URL =
            "https://generativelanguage.googleapis.com/v1beta/models/gemini-pro:generateContent?key=AIzaSyBNPsLaqxXee_w-amaH1Ua_GAYdL5siAdM";

        using (UnityWebRequest www = UnityWebRequest.Post(URL, requestStr, "application/json"))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError(www.error);
            }
            else
            {
                Debug.Log(www.downloadHandler.text);
                JsonUtility.FromJsonOverwrite(www.downloadHandler.text, parsedJson);
                requestResponse.text = parsedJson.candidates[0].content.parts[0].text;
                StartCoroutine(TTSRequest($"{{\"text\":\"{parsedJson.candidates[0].content.parts[0].text}\"}}"));
            }
        }
    }

    IEnumerator ClipRequest(string fileName)
    {
        using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(ttsURL + "get_tts?filename=" + fileName, AudioType.MPEG))
        {
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.ConnectionError)
            {
                Debug.Log(www.error);
            }
            else
            {
                AudioClip myClip = DownloadHandlerAudioClip.GetContent(www);
                audioSource.clip = myClip;
            }
        }
    }
    
    IEnumerator TTSRequest(string jsonData)
    {
        using (UnityWebRequest wwwClip = UnityWebRequest.Post(ttsURL + "tts", jsonData, "application/json"))
        {
            yield return wwwClip.SendWebRequest();

            if (wwwClip.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"wwwClip error: {wwwClip.error}");
            }
            else
            {
                JsonUtility.FromJsonOverwrite(wwwClip.downloadHandler.text, parsedTTSJson);
                StartCoroutine(ClipRequest(parsedTTSJson.filename));
            }
        }
    }

    public void PlayAudio()
    {
        audioSource.Play();
    }

}

[Serializable]
public class Candidate
{
    public Content content;
    public string finishReason;
    public int index;
    public List<SafetyRating> safetyRatings;
}
[Serializable]
public class Content
{
    public List<Part> parts ;
    public string role ;
}
[Serializable]
public class Part
{
    public string text ;
}
[Serializable]
public class PromptFeedback
{
    public List<SafetyRating> safetyRatings ;
}
[Serializable]
public class Root
{
    public List<Candidate> candidates ;
    public PromptFeedback promptFeedback ;
}
[Serializable]
public class SafetyRating
{
    public string category ;
    public string probability ;
}

[Serializable]
public class TTSResponse
{
    public string filename;
}

