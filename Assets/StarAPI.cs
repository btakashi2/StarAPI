using UnityEngine;
using Cysharp.Threading.Tasks;
using UnityEngine.Networking;
using Newtonsoft.Json;
using UnityEngine.UI;
using System.Collections.Generic;
using System;

public class StarAPI : MonoBehaviour
{
    //string BASE_URL = "https://livlog.xyz/hoshimiru/constellation";

    //private const string LAT = "35.709026";
    //private const string LNG = "139.731992";

    [SerializeField] private List<RawImage> _targetImages;

    public Text debugText;

    async void Start()
    {
        string requestAPI = "https://livlog.xyz/hoshimiru/constellation?lat=35.6581&lng=139.7414&date=2020-01-15&hour=20&min=00";

        debugText.text = "API接続開始 \n";
        await GetStarImages(requestAPI);
    }

    async UniTask GetStarImages(string url)
    {
        UnityWebRequest request = UnityWebRequest.Get(url);
        await request.SendWebRequest();

        debugText.text += "リクエスト開始 \n";
        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.Log(request.error);
            debugText.text += "リクエストに失敗 \n";
        }
        else
        {
            debugText.text += "リクエストに成功 \n";
            // JSONデータから画像URLを取得
            string jsonData = request.downloadHandler.text;
            debugText.text += "Json取得完了 \n";
            try
            {
                // Jsonの取得には下記で紹介されているcom.unity.nuget.newtonsoft-jsonをインストールすること
                // https://www.create-forever.games/unity-json-net/
                var starData = JsonConvert.DeserializeObject<Root>(jsonData);
                debugText.text += "Json変換完了 \n";
                // 先頭の3つの要素からstarImageのURLを取得
                for (int i = 0; i < Mathf.Min(_targetImages.Count, starData.result.Count); i++)
                {
                    string starImageUrl = starData.result[i].starImage;
                    debugText.text += (i + 1) + ":画像URL取得 \n";

                    // 取得したURLから画像をダウンロード
                    UnityWebRequest imageRequest = UnityWebRequestTexture.GetTexture(starImageUrl);
                    await imageRequest.SendWebRequest();

                    if (imageRequest.result != UnityWebRequest.Result.Success)
                    {
                        Debug.Log(imageRequest.error);
                        debugText.text += (i + 1) + ":画像取得失敗 \n";
                    }
                    else
                    {
                        // テクスチャを設定
                        _targetImages[i].texture = ((DownloadHandlerTexture)imageRequest.downloadHandler).texture;
                        debugText.text += (i + 1) + ":画像取得成功 \n";

                        // 配置する球面座標（r, θ, φ）
                        float r = 100f; // 固定値
                        float theta = 90-(float)starData.result[i].altitudeNum; // 仰角
                        float phi = 90-(float)starData.result[i].directionNum; // 方角

                        // 球面座標を直交座標に変換
                        float x = r * Mathf.Sin(theta * Mathf.Deg2Rad) * Mathf.Cos(phi * Mathf.Deg2Rad);
                        float y = r * Mathf.Cos(theta * Mathf.Deg2Rad);
                        float z = r * Mathf.Sin(theta * Mathf.Deg2Rad) * Mathf.Sin(phi * Mathf.Deg2Rad);

                        // RawImage を配置
                        _targetImages[i].transform.position = new Vector3(x, y, z);
                        _targetImages[i].transform.localScale = new Vector3(30, 30, 30);
                        _targetImages[i].transform.LookAt(Vector3.zero);
                    }
                }
            }
            catch (Exception ex)
            {
                debugText.text += "Json変換エラー: " + ex.Message + "\n";
                // エラーが発生した場合の処理...
            }
        }
    }
}


[Serializable]
public class Result
{
    public string altitude { get; set; }
    public double altitudeNum { get; set; }
    public string confirmed { get; set; }
    public string content { get; set; }
    public string direction { get; set; }
    public double directionNum { get; set; }
    public string drowing { get; set; }
    public string eclipticalFlag { get; set; }
    public string enName { get; set; }
    public string id { get; set; }
    public string jpName { get; set; }
    public string origin { get; set; }
    public string ptolemyFlag { get; set; }
    public string roughly { get; set; }
    public string ryaku { get; set; }
    public string season { get; set; }
    public string starIcon { get; set; }
    public string starImage { get; set; }
}

[Serializable]
public class Root
{
    public List<Result> result { get; set; }
    public int status { get; set; }
}