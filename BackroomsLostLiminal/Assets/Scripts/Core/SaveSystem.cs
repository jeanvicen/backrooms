using UnityEngine;

namespace Backrooms.Core
{
    /// <summary>
    /// Sistema de salvamento de dados do jogo
    /// Gerencia checkpoints, saves manuais e configurações
    /// </summary>
    public static class SaveSystem
    {
        private const string SAVE_FOLDER = "/BackroomsLostLiminal/";
        private const string CHECKPOINT_PREFIX = "checkpoint_";
        private const string MANUAL_SAVE_PREFIX = "save_";
        private const int MAX_MANUAL_SAVES = 3;

        /// <summary>
        /// Salva um checkpoint automático
        /// </summary>
        public static void SaveCheckpoint(string checkpointName, string levelName, float sanity, float health)
        {
            SaveData data = new SaveData
            {
                checkpointName = checkpointName,
                levelName = levelName,
                sanity = sanity,
                health = health,
                stamina = 100f,
                inventoryItems = new string[0]
            };

            string json = JsonUtility.ToJson(data);
            System.IO.File.WriteAllText(GetCheckpointPath(checkpointName), json);
        }

        /// <summary>
        /// Carrega o último checkpoint salvo
        /// </summary>
        public static SaveData LoadLastCheckpoint()
        {
            string[] checkpointFiles = System.IO.Directory.GetFiles(Application.persistentDataPath + SAVE_FOLDER, CHECKPOINT_PREFIX + "*");
            
            if (checkpointFiles.Length > 0)
            {
                string json = System.IO.File.ReadAllText(checkpointFiles[checkpointFiles.Length - 1]);
                return JsonUtility.FromJson<SaveData>(json);
            }

            return null;
        }

        /// <summary>
        /// Salva manualmente em um slot específico (0-2)
        /// </summary>
        public static bool SaveManual(int slot, string saveName, string levelName, float sanity, float health, string[] inventory)
        {
            if (slot < 0 || slot >= MAX_MANUAL_SAVES)
            {
                Debug.LogError("[SaveSystem] Slot de save inválido. Use 0, 1 ou 2.");
                return false;
            }

            SaveData data = new SaveData
            {
                checkpointName = saveName,
                levelName = levelName,
                sanity = sanity,
                health = health,
                stamina = 100f,
                inventoryItems = inventory
            };

            string json = JsonUtility.ToJson(data);
            System.IO.File.WriteAllText(GetManualSavePath(slot), json);
            
            Debug.Log($"[SaveSystem] Jogo salvo manualmente no slot {slot}");
            return true;
        }

        /// <summary>
        /// Carrega um save manual de um slot específico
        /// </summary>
        public static SaveData LoadManualSave(int slot)
        {
            if (slot < 0 || slot >= MAX_MANUAL_SAVES)
            {
                Debug.LogError("[SaveSystem] Slot de save inválido.");
                return null;
            }

            string path = GetManualSavePath(slot);
            
            if (System.IO.File.Exists(path))
            {
                string json = System.IO.File.ReadAllText(path);
                return JsonUtility.FromJson<SaveData>(json);
            }

            return null;
        }

        /// <summary>
        /// Verifica se existe um save no slot especificado
        /// </summary>
        public static bool HasSaveInSlot(int slot)
        {
            return System.IO.File.Exists(GetManualSavePath(slot));
        }

        /// <summary>
        /// Deleta todos os saves (usado em permadeath)
        /// </summary>
        public static void DeleteAllSaves()
        {
            string folderPath = Application.persistentDataPath + SAVE_FOLDER;
            
            if (System.IO.Directory.Exists(folderPath))
            {
                // Deleta checkpoints
                string[] checkpoints = System.IO.Directory.GetFiles(folderPath, CHECKPOINT_PREFIX + "*");
                foreach (string file in checkpoints)
                {
                    System.IO.File.Delete(file);
                }

                // Deleta saves manuais
                string[] manualSaves = System.IO.Directory.GetFiles(folderPath, MANUAL_SAVE_PREFIX + "*");
                foreach (string file in manualSaves)
                {
                    System.IO.File.Delete(file);
                }
            }
        }

        /// <summary>
        /// Deleta um save manual específico
        /// </summary>
        public static void DeleteManualSave(int slot)
        {
            string path = GetManualSavePath(slot);
            
            if (System.IO.File.Exists(path))
            {
                System.IO.File.Delete(path);
                Debug.Log($"[SaveSystem] Save do slot {slot} deletado.");
            }
        }

        /// <summary>
        /// Obtém informações de todos os saves manuais para o menu
        /// </summary>
        public static SaveInfo[] GetAllSaveInfos()
        {
            SaveInfo[] infos = new SaveInfo[MAX_MANUAL_SAVES];

            for (int i = 0; i < MAX_MANUAL_SAVES; i++)
            {
                infos[i] = new SaveInfo
                {
                    slot = i,
                    exists = HasSaveInSlot(i),
                    saveName = "",
                    levelName = "",
                    sanity = 0,
                    health = 0
                };

                if (infos[i].exists)
                {
                    SaveData data = LoadManualSave(i);
                    if (data != null)
                    {
                        infos[i].saveName = data.checkpointName;
                        infos[i].levelName = data.levelName;
                        infos[i].sanity = data.sanity;
                        infos[i].health = data.health;
                    }
                }
            }

            return infos;
        }

        private static string GetCheckpointPath(string checkpointName)
        {
            EnsureFolderExists();
            return Application.persistentDataPath + SAVE_FOLDER + CHECKPOINT_PREFIX + checkpointName + ".json";
        }

        private static string GetManualSavePath(int slot)
        {
            EnsureFolderExists();
            return Application.persistentDataPath + SAVE_FOLDER + MANUAL_SAVE_PREFIX + slot + ".json";
        }

        private static void EnsureFolderExists()
        {
            string folderPath = Application.persistentDataPath + SAVE_FOLDER;
            
            if (!System.IO.Directory.Exists(folderPath))
            {
                System.IO.Directory.CreateDirectory(folderPath);
            }
        }
    }

    /// <summary>
    /// Informações de save para exibição no menu
    /// </summary>
    [System.Serializable]
    public class SaveInfo
    {
        public int slot;
        public bool exists;
        public string saveName;
        public string levelName;
        public float sanity;
        public float health;
    }
}
