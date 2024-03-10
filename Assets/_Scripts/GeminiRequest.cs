using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;

public class GeminiRequest : MonoBehaviour
{
    private Root parsedJson = new();
    
    [SerializeField] 
    private TMP_InputField requestInput;
    [SerializeField] 
    private TextMeshProUGUI requestResponse;
    [SerializeField] private AudioSource audioSource;
    public void SendRequest()
    {
        StartCoroutine(AIRequest($"{{\"contents\":[{{\"parts\":[{{\"text\":\"{requestInput.text}\"}}]}}]}}"));
    }

    private void Start()
    {
        StartCoroutine(AIRequest($"{{\"contents\":[{{\"parts\":[{{\"text\":\"Türkçe çok kısa bir cümle kurar mısın\"}}]}}]}}"));
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
                StartCoroutine(ClipRequest($"{{\"text\":\"{parsedJson.candidates[0].content.parts[0].text}\"}}"));
            }
        }
    }

    IEnumerator ClipRequest(string jsonData)
    {
        string ttsURL = "http://localhost:5000/tts";
        Debug.Log(jsonData);
        using (UnityWebRequest wwwClip = UnityWebRequest.Post(ttsURL, jsonData, "application/json"))
        {
            yield return wwwClip.SendWebRequest();

            if (wwwClip.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"wwwClip error: {wwwClip.error}");
            }
            else
            {
                byte[] audioBytes = wwwClip.downloadHandler.data;
                float[] f = ConvertByteToFloat(audioBytes);
                AudioClip audioClip = AudioClip.Create("testSound", f.Length, 1, 44100, false, false);
                audioClip.SetData(f, 0);
                audioSource.clip = audioClip;
                audioSource.Play();
            }
        }
    }
    
    private float[] ConvertByteToFloat(byte[] array) 
    {
        float[] floatArr = new float[array.Length / 4];
        for (int i = 0; i < floatArr.Length; i++) 
        {
            if (BitConverter.IsLittleEndian) 
                Array.Reverse(array, i * 4, 4);
            floatArr[i] = BitConverter.ToSingle(array, i*4) / 0x80000000;
        }
        return floatArr;
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

