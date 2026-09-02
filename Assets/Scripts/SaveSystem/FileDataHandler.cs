using System;
using System.IO;
using UnityEngine;

public class FileDataHandler
{
    private string fullPath;
    private bool encryptData;
    private string codeWord = "LMGYS";

    public FileDataHandler(string dataDirPath, string dateFileName, bool encryptData)
    {
        fullPath = Path.Combine(dataDirPath, dateFileName);
        this.encryptData = encryptData;
    }

    public void SaveData(GameData gameData)
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(fullPath));
            string dataToSave = JsonUtility.ToJson(gameData, true);

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

    public GameData LoadData()
    {
        GameData loadData = null;

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

                loadData = JsonUtility.FromJson<GameData>(dataToLoad);
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
