using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace RMSreader.Services
{
    class LocalStorage
    {
        public static async Task SaveJsonToLocalStorage(string fileName, string json)
        {
            StorageFolder localStorage = ApplicationData.Current.LocalFolder;
            StorageFile file = await localStorage.CreateFileAsync(fileName, CreationCollisionOption.ReplaceExisting);
            await FileIO.WriteTextAsync(file, json);
        }

        public static async Task<string> GetJsonFromLocalStorage(string fileName)
        {
            try
            {
                StorageFolder localStorage = ApplicationData.Current.LocalFolder;
                StorageFile file = await localStorage.GetFileAsync(fileName);
                return await FileIO.ReadTextAsync(file);
            }
            catch (FileNotFoundException)
            {
                return null;
            }
        }

        public static void SaveSetting(string key, object value)
        {
            ApplicationDataContainer LocalStorage = ApplicationData.Current.LocalSettings;
            if (!LocalStorage.Values.ContainsKey(key))
            {
                LocalStorage.Values.Add(key, value);
            }
            else
            {
                LocalStorage.Values[key] = value;
            }
        }

        public static object GetSetting(string key)
        {
            ApplicationDataContainer LocalStorage = ApplicationData.Current.LocalSettings;
            if (LocalStorage.Values.ContainsKey(key))
            {
                return LocalStorage.Values[key];
            }
            return null;
        }
    }
}
