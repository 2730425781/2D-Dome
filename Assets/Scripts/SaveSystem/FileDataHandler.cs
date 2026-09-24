using System;
using System.IO;
using UnityEngine;

/// <summary>
/// 通用 JSON 文件读写器：把任意可被 JsonUtility 序列化的类型读写到 persistentDataPath 下的单个文件。
///
/// 为什么做成泛型：
/// 存档槽数据（GameData）和设置数据（AudioSettingsData）是两种完全不同的结构，
/// 但"读文件 → 解密 → JsonUtility → 写回"的流程一模一样。
/// 泛型化之后不必为设置再复制一份读写代码，也不必让音量设置被迫挤进 GameData
/// （音量是全局设置，不该跟着存档槽走）。
/// </summary>
/// <typeparam name="T">可被 JsonUtility 序列化的类型（通常是 [Serializable] 类）。</typeparam>
public class FileDataHandler<T>
{
    private string fullPath;
    private bool encryptData;
    private string codeWord = "LMGYS";

    public FileDataHandler(string dataDirPath, string dateFileName, bool encryptData)
    {
        fullPath = Path.Combine(dataDirPath, dateFileName);
        this.encryptData = encryptData;
    }

    /// <summary>该文件是否已存在（供 UI 判断"有/无存档"、或首次运行判断）。</summary>
    public bool Exists() => File.Exists(fullPath);

    public void SaveData(T data)
    {
        if (data == null)
        {
            Debug.LogWarning("要保存的数据为 null，已跳过：" + fullPath);
            return;
        }

        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(fullPath));
            string dataToSave = JsonUtility.ToJson(data, true);

            if (encryptData)
            {
                dataToSave = EncryptDecrypt(dataToSave);
            }

            using (FileStream stream = new FileStream(fullPath, FileMode.Create))
            {
                using (StreamWriter writer = new StreamWriter(stream))
                {
                    writer.Write(dataToSave);
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError("向文件保存数据时出错：" + fullPath + "\n" + e);
        }
    }

    /// <summary>读取数据；文件不存在或解析失败时返回 null（由调用方决定用什么默认值）。</summary>
    public T LoadData()
    {
        T loadData = default;

        if (File.Exists(fullPath))
        {
            try
            {
                string dataToLoad = " ";

                using (FileStream stream = new FileStream(fullPath, FileMode.Open))
                {
                    using (StreamReader reader = new StreamReader(stream))
                    {
                        dataToLoad = reader.ReadToEnd();
                    }
                }

                if (encryptData)
                {
                    dataToLoad = EncryptDecrypt(dataToLoad);
                }

                loadData = JsonUtility.FromJson<T>(dataToLoad);
            }
            catch (Exception e)
            {
                Debug.LogError("从文件转换加载数据时出错：" + fullPath + "\n" + e);
            }
        }

        return loadData;
    }

    public void Delete()
    {
        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
        }
    }

    private string EncryptDecrypt(String data)
    {
        string modifedData = "";

        for (int i = 0; i < data.Length; i++)
        {
            modifedData += (char)(data[i] ^ codeWord[i % codeWord.Length]);
        }

        return modifedData;
    }
}
