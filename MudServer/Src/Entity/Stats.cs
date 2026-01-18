using System;
using System.Web.Script.Serialization;
using MudServer.World;
using MudServer.Server;
using MudServer.Util;

namespace MudServer.Entity {
    public class Stats {
        public string Name;
        public int Level;
        public Coordinate3 Location;
        public Guid Id;
        private int _health;
        public int MaxHealth;

        public int Health {
            get => _health;
            set {
                // If this object is dead, we can't make it more dead.
                if (_health == 0 && value < 0) {
                    return;
                }

                _health = value;
                if (_health > MaxHealth)
                    _health = MaxHealth;

                if (_health > 0) return;

                _health = 0;
                OnZeroHealthEvent(this);
            }
        }

        private int _str = 10;
        public int BonusStr;

        public int Str {
            get => _str + BonusStr;
            set => _str = value;
        }

        private int _dex = 10;
        public int BonusDex;

        public int Dex {
            get => _dex + BonusDex;
            set => _dex = value;
        }

        private int _int = 10;
        public int BonusInt;

        public int Int {
            get => _int + BonusInt;
            set => _int = value;
        }

        private int _con = 10;
        public int BonusCon;

        public int Con {
            get => _con + BonusCon;
            set => _con = value;
        }

        public int BonusSpeed;

        public int Speed => (int)Math.Ceiling((Dex / 3.0) + BonusSpeed);

        public int GetNumberOfAttacks() {
            if (Speed <= 0) return 0;
            if (Speed < 10) return 1;
            return (int)Math.Floor(Math.Log(Speed / 10.0) / Math.Log(1.5)) + 1;
        }

        public int ExpToNextLevel = 100;
        private int _exp = 0;
        public bool StatAllocationNeeded = false;

        public int Exp {
            get => _exp;
            set {
                _exp = value;
                if (_exp >= ExpToNextLevel) {
                    LevelUp();
                }
            }
        }

        [field: ScriptIgnore]
        event Action<Stats> OnZeroHealthEvent = delegate { };

        public event Action<Stats> OnZeroHealth {
            add {
                //Prevent double subscription
                OnZeroHealthEvent -= value;
                OnZeroHealthEvent += value;
            }
            remove {
                OnZeroHealthEvent -= value;
            }
        }

        [field: ScriptIgnore]
        public event Action<Stats, int> OnLevelUp = delegate { };

        public QuestLog QuestLog = new QuestLog();

        public Stats() { }

        public Stats(string username, Guid id) {
            Name = username;
            Id = id;
            Level = 1;
            DataManager.IdDataPairs.Add(id, this);
        }

        internal int GrantExperience() {
            return (int)(_str + _dex + _int + (int)((float)MaxHealth / 3));
        }

        private void LevelUp() {
            int prevMaxHealth = MaxHealth;
            int levelsGained = 0;
            while (_exp >= ExpToNextLevel) {
                _str++;
                _dex++;
                _int++;
                MaxHealth += Math.Min(1, (int)Math.Ceiling(Con / 2f));
                Level++;
                levelsGained++;
                ExpToNextLevel += (int)(ExpToNextLevel * 1.5f);
            }

            if (levelsGained > 0) {
                OnLevelUp(this, prevMaxHealth);
            }
        }

        public Stats ShallowCopy() {
            return (Stats)this.MemberwiseClone();
        }
    }
}
