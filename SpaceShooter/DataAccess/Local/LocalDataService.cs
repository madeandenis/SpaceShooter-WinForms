using System.Text.Json;

namespace SpaceShooter.DataAccess.Local
{
    internal class LocalDataService<TKey, TValue>
    {
        protected readonly string dataFilePath;

        public LocalDataService(string fileName)
        {
            dataFilePath = Path.Combine(Directory.GetCurrentDirectory(), $"..\\..\\..\\User\\Data\\{fileName}");
        }

        protected Dictionary<TKey, TValue> LoadData()
        {
            try
            {
                if (!File.Exists(dataFilePath))
                {
                    return new Dictionary<TKey, TValue>();
                }

                File.SetAttributes(dataFilePath, FileAttributes.ReadOnly);
                string jsonString = File.ReadAllText(dataFilePath);
                var data = JsonSerializer.Deserialize<Dictionary<TKey, TValue>>(jsonString);

                return data ?? new Dictionary<TKey, TValue>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading data: {ex.Message}");
                return new Dictionary<TKey, TValue>();
            }
        }

        protected void SaveData(Dictionary<TKey, TValue> data)
        {
            try
            {
                if (!File.Exists(dataFilePath))
                {
                    File.Create(dataFilePath);
                }

                File.SetAttributes(dataFilePath, FileAttributes.Normal);

                string jsonString = JsonSerializer.Serialize(data);
                File.WriteAllText(dataFilePath, jsonString);
                
                File.SetAttributes(dataFilePath, FileAttributes.ReadOnly);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving data: {ex.Message}");
            }
        }
    }
}
