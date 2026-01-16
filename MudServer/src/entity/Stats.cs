using System;
using System.Web.Script.Serialization;
using GameCore.Util;
using ServerCore;

namespace GameCore {
public class Stats {
    [ScriptIgnore]
    public BaseMobile ThisBaseMobile;
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
            if (ThisBaseMobile != null) {
                ThisBaseMobile.Target = null;
                ThisBaseMobile.GameState = GameState.Dead;
            }

            OnZeroHealthEvent(this);
            try {
                if (ThisBaseMobile != null) {
                    ThisBaseMobile.TriggerOnDeath(this);
                }
            } catch (NullReferenceException) { }
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
        while (_exp >= ExpToNextLevel) {
            _str++;
            _dex++;
            _int++;
            MaxHealth += Math.Min(1, (int)Math.Ceiling(Con / 2f));
            Level++;
            ExpToNextLevel += (int)(ExpToNextLevel * 1.5f);

            if (!(ThisBaseMobile is PlayerEntity)) continue;

            //StatAllocationNeeded = true;
            ThisBaseMobile.SendToClient($"You are now level {Level}!", Color.Cyan);
            ThisBaseMobile.SendToClient(
                $"Your maximum health has increased by {MaxHealth - prevMaxHealth}.", Color.Cyan
            );
        }
    }

    public Stats ShallowCopy() {
        return (Stats)this.MemberwiseClone();
    }
}
}
