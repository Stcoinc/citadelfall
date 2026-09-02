using System;

namespace ClubGamerZone.TowerDefense.Application.Gameplay
{
    public sealed class MissionRuntimeState
    {
        private readonly int _totalWaves;
        private int _completedWaves;
        private int _activeEnemies;

        public event Action<int, int> CurrencyChanged;

        public MissionRuntimeState(int startingScrap, int baseHealth, int totalWaves, int startingCoins = 0, int startingGems = 0)
        {
            if (startingScrap < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(startingScrap));
            }

            if (startingCoins < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(startingCoins));
            }

            if (startingGems < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(startingGems));
            }

            if (baseHealth <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(baseHealth));
            }

            if (totalWaves <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(totalWaves));
            }

            Scrap = startingScrap;
            Coins = startingCoins;
            Gems = startingGems;
            BaseHealth = baseHealth;
            _totalWaves = totalWaves;
            Outcome = MissionOutcome.Running;
        }

        public int Scrap { get; private set; }

        public int Coins { get; private set; }

        public int Gems { get; private set; }

        public int BaseHealth { get; private set; }

        public int CompletedWaves => _completedWaves;

        public int TotalWaves => _totalWaves;

        public int ActiveEnemies => _activeEnemies;

        public MissionOutcome Outcome { get; private set; }

        public bool TrySpendScrap(int amount)
        {
            if (amount < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(amount));
            }

            if (Outcome != MissionOutcome.Running || Scrap < amount)
            {
                return false;
            }

            Scrap -= amount;
            if (amount > 0)
            {
                CurrencyChanged?.Invoke(Scrap, Coins);
            }
            return true;
        }

        public bool TrySpendCoins(int amount)
        {
            if (amount < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(amount));
            }

            if (Outcome != MissionOutcome.Running || Coins < amount)
            {
                return false;
            }

            Coins -= amount;
            if (amount > 0)
            {
                CurrencyChanged?.Invoke(Scrap, Coins);
            }
            return true;
        }

        public bool TrySpendGems(int amount)
        {
            if (amount < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(amount));
            }

            if (Outcome != MissionOutcome.Running || Gems < amount)
            {
                return false;
            }

            Gems -= amount;
            return true;
        }

        public void AddScrap(int amount)
        {
            if (amount < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(amount));
            }

            if (Outcome != MissionOutcome.Running)
            {
                return;
            }

            Scrap += amount;
            if (amount > 0)
            {
                CurrencyChanged?.Invoke(Scrap, Coins);
            }
        }

        public void DamageBase(int amount)
        {
            if (amount < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(amount));
            }

            if (Outcome != MissionOutcome.Running)
            {
                return;
            }

            BaseHealth = Math.Max(0, BaseHealth - amount);

            if (BaseHealth == 0)
            {
                Outcome = MissionOutcome.Defeat;
            }
        }

        public void RegisterEnemySpawned()
        {
            if (Outcome != MissionOutcome.Running)
            {
                return;
            }

            _activeEnemies++;
        }

        public void RegisterEnemyResolved()
        {
            if (_activeEnemies > 0)
            {
                _activeEnemies--;
            }

            TryCompleteMission();
        }

        public void CompleteWave()
        {
            if (Outcome != MissionOutcome.Running)
            {
                return;
            }

            if (_completedWaves < _totalWaves)
            {
                _completedWaves++;
            }

            TryCompleteMission();
        }

        private void TryCompleteMission()
        {
            if (Outcome == MissionOutcome.Running && _completedWaves >= _totalWaves && _activeEnemies == 0)
            {
                Outcome = MissionOutcome.Victory;
            }
        }
    }
}
