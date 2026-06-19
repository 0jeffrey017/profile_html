using System.Collections.Generic;
using System.IO;
using UnityEngine;

/*
 * Space Cat — JSONによるセーブ／ロードシステム（担当：全体設計・セーブ機能）
 *
 * 難易度が高く一画面でのクリアが困難なため、進行度の中断・再開を可能にする
 * セーブ機能を実装。プレイヤー座標・獲得アイテム・クリアフラグをシリアライズし、
 * persistentDataPath 配下に JSON として永続化する。
 * 入出力はジェネリックメソッド化し、どのデータ型でも再利用できる設計とした。
 */

/// <summary>保存対象のゲーム進行データ（[Serializable] で JSON 化可能にする）。</summary>
[System.Serializable]
public class GameData
{
    public Vector2 PlayerPostion;       // 中断時のプレイヤー座標
    public List<ItemData> items;        // 各装備の獲得状況
    public bool isGameCleared;
    public bool isGameStarted;
}

[System.Serializable]
public class ItemData
{
    public string equipmentId;
    public string displayName;
    public bool isCollected;
}

public class SaveManager : MonoBehaviour
{
    /// <summary>
    /// 任意の型 T を JSON 文字列へ変換してファイルへ書き出す汎用セーブ。
    /// 型を限定しないことで、プレイヤーデータ・設定など用途を問わず再利用できる。
    /// </summary>
    public void SaveDataToJson<T>(string path, T data)
    {
        var json = JsonUtility.ToJson(data, prettyPrint: true);
        if (!File.Exists(path)) File.Create(path).Close();

        using StreamWriter writer = new StreamWriter(path);
        writer.Write(json);
        Debug.Log("save success");
    }

    /// <summary>JSON ファイルを読み込み、型 T として復元する汎用ロード。</summary>
    public T LoadDataFromJson<T>(string path)
    {
        if (!File.Exists(path)) return default; // 未保存なら既定値を返す

        using StreamReader reader = new StreamReader(path);
        var json = reader.ReadToEnd();
        return JsonUtility.FromJson<T>(json);
    }

    /// <summary>
    /// 現在の進行データを保存する。保存先ディレクトリが無ければ自動生成し、
    /// プラットフォーム非依存の persistentDataPath を用いる。
    /// </summary>
    public void SavePlayerData(GameData currentData)
    {
        string dir = Application.persistentDataPath + "/SaveData";
        if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

        string path = dir + "/PlayerData.json";
        SaveDataToJson(path, currentData);
    }
}
