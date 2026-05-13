public class SaveManager : MonoBehaviour
{
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
}
