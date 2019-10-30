using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using System.Security.Cryptography;
using System.Text;

public class PlayAudioController : MonoBehaviour
{
    /// <summary>
    /// 转换文字输入框
    /// </summary>
    public InputField TextToAudioInputField;

    /// <summary>
    /// 音频
    /// </summary>
    public AudioSource audioSource;


    /// <summary>
    /// 请求地址
    /// </summary>
    private string RequestURL = "http://39.104.127.80:8082/GetAudioStreamHandler.ashx?SpeechText={0}";

    /// <summary>
    /// 文件名
    /// </summary>
    string FileName = "";

    /// <summary>
    /// 是否在StreamingAssets/AudioFile中存在
    /// </summary>
    bool alreadyExit = false;


    /// <summary>
    /// 将Text转化为Speech
    /// </summary>
    public void ConvertTextToSpeech()
    {
        if (string.IsNullOrEmpty(TextToAudioInputField.text))
        {
            return;
        }

        FileName = Md5(TextToAudioInputField.text).ToUpper() + ".wav";
        string FilePath = Application.streamingAssetsPath + "/AudioFile/" + FileName;

        alreadyExit = false;
        StartCoroutine(GetWavFileAndPlay(FilePath));
    }


    /// <summary>
    /// 从StreamingAssets/AudioFile文件中加载音频文件并播放
    /// </summary>
    /// <param name="FilePath"></param>
    /// <returns></returns>
    IEnumerator GetWavFileAndPlay(string FilePath)
    {
        UnityWebRequest uwr = UnityWebRequestMultimedia.GetAudioClip(FilePath, AudioType.WAV);
        yield return uwr.SendWebRequest();
        if (uwr.isDone)
        {
            if (uwr.isNetworkError || uwr.isHttpError)
            {
                alreadyExit = false;
                Debug.Log(uwr.error);
            }
            else
            {
                AudioClip audioClip = DownloadHandlerAudioClip.GetContent(uwr);
                
                audioSource.clip = audioClip;
                audioSource.Play();
                alreadyExit = true;
            }
        }

        //判断当前音频文件是否存在
        if (!alreadyExit)
        {
            string text = TextToAudioInputField.text.Replace("\n", "|!|!|");

            StartCoroutine(SaveWavFile(text));
        }

    }

    /// <summary>
    /// 当StreamingAssets/AudioFile文件夹下不存在指定Wav文件时，下载该音频文件并播放
    /// </summary>
    /// <param name="text"></param>
    /// <returns></returns>
    IEnumerator SaveWavFile(string text)
    {
        UnityWebRequest uwr = UnityWebRequestMultimedia.GetAudioClip(string.Format(RequestURL, text),AudioType.WAV);
        yield return uwr.SendWebRequest();
        if (uwr.isNetworkError || uwr.isHttpError)
        {
            Debug.Log(uwr.error);
        }
        else
        {
            //播放下载的Wav音频
            AudioClip audioClip = DownloadHandlerAudioClip.GetContent(uwr);
            audioSource.clip = audioClip;
            audioSource.Play();

            //保存下载的Wav音频文件
            byte[] data = uwr.downloadHandler.data;
            //这里的FileMode.create是创建这个文件,如果文件名存在则覆盖重新创建
            FileStream fs = new FileStream(Application.streamingAssetsPath + "/AudioFile/" + FileName, FileMode.Create);

            fs.Write(data, 0, data.Length);
            //每次读取文件后都要记得关闭文件
            fs.Close();
        }
    }


        /// <summary>
        /// MD5加密，主要根据输入的文字来生成唯一的音频文件名
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string Md5(string value)
        {
            var result = string.Empty;
            if (string.IsNullOrEmpty(value)) return result;
            using (var md5 = MD5.Create())
            {
                result = GetMd5Hash(md5, value);
            }
            return result;
        }

        /// <summary>
        /// MD5生成
        /// </summary>
        /// <param name="md5Hash"></param>
        /// <param name="input"></param>
        /// <returns></returns>
        static string GetMd5Hash(MD5 md5Hash, string input)
        {

            byte[] data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(input));
            var sBuilder = new StringBuilder();
            foreach (byte t in data)
            {
                sBuilder.Append(t.ToString("x2"));
            }
            return sBuilder.ToString();
        }
}
