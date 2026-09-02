using System;
using UnityEngine;

namespace ClubGamerZone.TowerDefense.Features.Gameplay
{
    public sealed class LocalSaveSlotRepository
    {
        private const string KeyPrefix = "FleetCommandTD.SaveSlot.";

        public SaveSlotData Load(int slotIndex)
        {
            var key = BuildKey(slotIndex);

            if (!PlayerPrefs.HasKey(key))
            {
                return CreateEmpty(slotIndex);
            }

            var json = PlayerPrefs.GetString(key);

            if (string.IsNullOrWhiteSpace(json))
            {
                return CreateEmpty(slotIndex);
            }

            try
            {
                var data = JsonUtility.FromJson<SaveSlotData>(json);
                return data ?? CreateEmpty(slotIndex);
            }
            catch (ArgumentException)
            {
                return CreateEmpty(slotIndex);
            }
        }

        public void Save(SaveSlotData data)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            data.UpdatedAtUtc = DateTime.UtcNow.ToString("O");
            PlayerPrefs.SetString(BuildKey(data.SlotIndex), JsonUtility.ToJson(data));
            PlayerPrefs.Save();
        }

        public void Delete(int slotIndex)
        {
            PlayerPrefs.DeleteKey(BuildKey(slotIndex));
            PlayerPrefs.Save();
        }

        private static SaveSlotData CreateEmpty(int slotIndex)
        {
            return new SaveSlotData
            {
                SlotIndex = slotIndex
            };
        }

        private static string BuildKey(int slotIndex)
        {
            return $"{KeyPrefix}{slotIndex}";
        }
    }
}
