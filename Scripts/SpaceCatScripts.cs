using System.Collections;
public class SaveManager : MonoBehaviour
{
    private GameData _currentGameData = null;
    public GameData CurrentGameData
    {
        get
        {
            _currentGameData ??= GetEnmtyPlayerGameData();

            return _currentGameData;
        }
        set => _currentGameData = value;
    }
    private string _currentPlayerName = null;
    public string CurrentPlayerName
    {
        get
        {
            _currentPlayerName ??= "Admin";

            return _currentPlayerName;
        }
        set => _currentPlayerName = value;
    }

    public void SaveDataToJson<T>(string path, T data)
    {
        var s = JsonUtility.ToJson(data, true);
        if (!File.Exists(path))
        {
            File.Create(path).Close();
        }
        using StreamWriter writer = new StreamWriter(path);
        writer.Write(s);
        writer.Close();
        Debug.Log($"save success");
    }

    public T LoadDataFromJson<T>(string path)
    {
        if (File.Exists(path))
        {
            using StreamReader reader = new StreamReader(path);
            var s = reader.ReadToEnd();
            var data = JsonUtility.FromJson<T>(s);
            reader.Close();
            return data;
        }
        return default;
    }

    public bool CreateNewPlayer(string playerName)
    {
        var dir = Application.persistentDataPath + "/SaveData";
        if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

        var path = dir + "/PlayerList.data";

        var playerList = LoadDataFromJson<PlayerListData>(path);
        playerList ??= new PlayerListData();

        var playerDict = new Dictionary<string, GameData>();

        foreach (var entry in playerList.players)
        {
            if (playerDict.ContainsKey(entry.playerName)) continue;
            playerDict[entry.playerName] = entry.data;
        }

        if (playerDict.ContainsKey(playerName))return false;

        playerDict[playerName] = GetEnmtyPlayerGameData();

        var newPlayer = new PlayerEntry()
        {
            playerName = playerName,
            data = playerDict[playerName],
        };

        playerList.players.Add(newPlayer);
        SaveDataToJson(path, playerList);

        _currentGameData = playerDict[playerName];
        _currentPlayerName = playerName;
        return true;
    }

    public Dictionary<string, GameData> LoadPlayers()
    {
        var dir = Application.persistentDataPath + "/SaveData";
        if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

        var path = dir + "/PlayerList.data";

        var playerList = LoadDataFromJson<PlayerListData>(path);
        playerList ??= new PlayerListData();

        var playerDict = new Dictionary<string, GameData>();

        foreach (var entry in playerList.players)
        {
            if (playerDict.ContainsKey(entry.playerName)) continue;
            playerDict[entry.playerName] = entry.data;
        }
        return playerDict;
    }

    public void DeletePlayer(string playerName)
    {
        var dir = Application.persistentDataPath + "/SaveData";
        if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

        var path = dir + "/PlayerList.data";

        var playerList = LoadDataFromJson<PlayerListData>(path);
        playerList ??= new PlayerListData();

        playerList.players.RemoveAll(p => p.playerName.Equals(playerName));

        SaveDataToJson(path, playerList);
    }

    public void SavePlayerData()
    {
        string dir = Application.persistentDataPath + "/SaveData";
        if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

        string path = dir + "/PlayerList.data";

        PlayerListData playerList = LoadDataFromJson<PlayerListData>(path);
        playerList ??= new PlayerListData();
        foreach (var v in playerList.players)
        {
            if (v.playerName.Equals(CurrentPlayerName))
            {
                v.data = _currentGameData;
            }
        }
        SaveDataToJson(path, playerList);
    }

    public void ResetPlayerData()
    {
        _currentGameData = GetEnmtyPlayerGameData();
        SavePlayerData();
    }

    private GameData GetEnmtyPlayerGameData()
    {
        GameData gameData;
        TextAsset data = Resources.Load<TextAsset>("Data/GameData");
        if (data == null)
        {
            Debug.LogError("Data/GameData not finded in Resources");
            return null;
        }
        gameData = JsonUtility.FromJson<GameData>(data.text);

        TextAsset itemData = Resources.Load<TextAsset>("Data/ItemData");
        if (itemData == null)
        {
            Debug.LogError("not find ItemData.json,in Resources/Data/");
            return null;
        }
        ItemListWrapper itemDataList = JsonUtility.FromJson<ItemListWrapper>(itemData.text);
        if (itemDataList == null)
        {
            Debug.LogError("json formater fail");
            return null;
        }
        else
        {
            var itemDatas = itemDataList.items;
            gameData.items = itemDatas;
        }

        Debug.Log(JsonUtility.ToJson(gameData, true));
        return gameData;
    }
}
