using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Servants_of_Arcana
{
    public class JsonDataManager
    {
        private static JsonSerializerSettings options;
        private static string entityPath { get; set; }
        private static string tablePath { get; set; }
        public JsonDataManager() 
        {
            options = new JsonSerializerSettings()
            {
                TypeNameHandling = TypeNameHandling.All,
                Formatting = Formatting.Indented,
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            };
            entityPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "JsonData/EntityData");
            tablePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "JsonData/TableData");

            if (!Directory.Exists(entityPath)) Directory.CreateDirectory(entityPath);
            if (!Directory.Exists(tablePath)) Directory.CreateDirectory(tablePath);
        }
        public static List<Entity> PullAllEntities()
        {
            List<Entity> pullData = new List<Entity>();
            foreach (string filePath in Directory.GetFiles(entityPath)) { JsonConvert.DeserializeObject<Entity>(File.ReadAllText(filePath), options); }
            return pullData;
        }
        public static List<RandomTable> PullAllTables()
        {
            List<RandomTable> pullData = new List<RandomTable>();
            foreach (string filePath in Directory.GetFiles(tablePath))
            {
                RandomTable table = JsonConvert.DeserializeObject<RandomTable>(File.ReadAllText(filePath), options);
                pullData.Add(table);
            }
            return pullData;
        }
        public static Entity ReturnEntity(string name)
        {
            string pullData = File.ReadAllText(Path.Combine(entityPath, name + ".json"));
            Entity entity = new Entity(JsonConvert.DeserializeObject<Entity>(pullData, options).components);

            entity.SetDelegates();
            if (entity.GetComponent<InventoryComponent>() != null)
            {
                foreach (Entity item in entity.GetComponent<InventoryComponent>().items)
                {
                    item.SetDelegates();
                }
                foreach (EquipmentSlot slot in entity.GetComponent<InventoryComponent>().equipment)
                {
                    if (slot.item != null)
                    {
                        slot.item.SetDelegates();
                    }
                }
            }

            return entity;
        }
        public static void SaveEntity(Entity entity, string name)
        {
            //entity.ClearImbeddedComponents();
            if (!Directory.Exists(entityPath)) Directory.CreateDirectory(entityPath);
            File.WriteAllText(Path.Combine(entityPath, name + ".json"), JsonConvert.SerializeObject(entity, options));
        }
        public static void SaveTable(RandomTable table)
        {
            if (!Directory.Exists(tablePath)) Directory.CreateDirectory(tablePath);
            File.WriteAllText(Path.Combine(tablePath, table.name + ".json"), JsonConvert.SerializeObject(table, options));
        }
    }
    public class RandomTableManager
    {
        public static Dictionary<string, RandomTable> spawnTables = new Dictionary<string, RandomTable>();
        public RandomTableManager()
        {
            spawnTables.Clear();
            foreach (RandomTable table in JsonDataManager.PullAllTables())
            { spawnTables.Add(table.name, table); }
        }
        public static void CreateNew(string tableName, Dictionary<int, string> tableDictionary)
        {
            JsonDataManager.SaveTable(new RandomTable(tableName, tableDictionary));
        }
        public static string RetrieveRandom(string table, int modifier, bool useSeed)
        {
            if (spawnTables.ContainsKey(table))
            {
                if (modifier == 0) { modifier = 1; }
                if (useSeed) { return spawnTables[table].table[Program.dungeonGenerator.seed.Next(1, 21) * modifier]; }
                else { return spawnTables[table].table[Program.random.Next(1, 21) * modifier]; }
            }
            else { throw new Exception("Referenced table does not exist"); }
        }
    }
    public class RandomTable
    {
        public string name { get; set; }
        public Dictionary<int, string> table = new Dictionary<int, string>();
        public RandomTable(string name, Dictionary<int, string> table) { this.name = name; this.table = table; }
    }
}
