using System.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using GameCore.Util;

namespace GameCore {
public class BaseMobile {
    public Guid ID;
    public string Name;
    public Stats Stats;
    public GameState GameState = GameState.Idle;
    public bool Hidden = false;
    protected Random Rnd = new Random();
    public BaseMobile Target;
    public int LastCombatTick = -1;
    protected Stopwatch RoundTimer = new Stopwatch();

    /// <summary>
    /// Do not add to this event directly. Use OnDeath.
    /// </summary>
    event Action<BaseMobile> OnDeathEvent = delegate { };

    public event Action<BaseMobile> OnDeath {
        add {
            //Prevent double subscription
            OnDeathEvent -= value;
            OnDeathEvent += value;
        }
        remove {
            OnDeathEvent -= value;
        }
    }

    /// <summary>
    /// Generates and sets this instance's Guid.
    /// </summary>
    /// <returns>The generated Guid.</returns>
    public Guid GenerateID() {
        ID = Guid.NewGuid();
        Stats.Id = ID;
        return ID;
    }

    public void TriggerOnDeath(Stats data) {
        if (data.Id == Stats.Id) {
            OnDeathEvent(this);
        }
    }

    public virtual void Move(Coordinate3 location) {
        Room oldRoom = World.GetRoom(Stats.Location);
        Room newRoom = World.GetRoom(location);

        if (newRoom == null) {
            // If we can't move to the new room, just stay where we are.
            // We might want to notify the mobile/player.
            SendToClient("You can't go that way.", Color.Red);
            return;
        }

        var thisPlayer = PlayerEntity.GetPlayerByID(Stats.Id);
        if (thisPlayer != null && thisPlayer.GameState == GameState.Resting) {
            thisPlayer.AccountState = AccountState.Active;
            thisPlayer.SendToClient("You stop resting as you move.", Color.Cyan);
        }

        if (Stats.Location == location) {
            BroadcastLocal(Name + " has arrived.", Color.Yellow);
            lock (newRoom.EntitiesHere) {
                try {
                    // In case we're already here, we don't want to add a duplicate of ourselves.
                    newRoom.EntitiesHere.Remove(ID);
                } catch (InvalidOperationException) { }

                newRoom.EntitiesHere.Add(ID);
            }

            if (thisPlayer != null)
                Actions.ActionCalls["look"](thisPlayer, new string[1]);
            return;
        }

        if (oldRoom != null) {
            lock (oldRoom.EntitiesHere) {
                oldRoom.EntitiesHere.Remove(ID);
            }

            if (!Hidden) {
                BroadcastLocal(
                    Name + " has left to the " + oldRoom.GetDirection(newRoom.Location),
                    Color.Yellow
                );
            }
        }

        Stats.Location = newRoom.Location;
        if (oldRoom != null) {
            if (!Hidden) {
                BroadcastLocal(
                    Name + " has arrived from the " +
                    newRoom.GetDirection(oldRoom.Location), Color.Yellow
                );
                SendToClient(
                    "You move to the " + oldRoom.GetDirection(newRoom.Location),
                    Color.White
                );
            } else {
                SendToClient(
                    "You sneak to the " + oldRoom.GetDirection(newRoom.Location),
                    Color.Magenta
                );
            }
        } else {
            if (!Hidden) {
                BroadcastLocal(Name + " has arrived.", Color.Yellow);
            }
        }

        lock (newRoom.EntitiesHere) {
            if (!newRoom.EntitiesHere.Contains(ID)) {
                newRoom.EntitiesHere.Add(ID);
            }
        }

        if (thisPlayer != null) {
            Actions.ActionCalls["look"](thisPlayer, new string[1]);
            Actions.ShowMap(thisPlayer, new string[1]);
        }
    }

    public virtual void SendToClient(string msg, string colorSequence = "") {
        PlayerEntity player;
        if (PlayerEntity.Players.TryGetValue(ID, out player)) {
            player.SendToClient(msg, colorSequence);
        }
    }

    public void BroadcastLocal(string msg, string colorSequence = "", params Guid[] ignore) {
        PlayerEntity player;
        Room room = World.GetRoom(Stats.Location);
        if (room != null) {
            Guid[] entities;
            lock (room.EntitiesHere) {
                entities = room.EntitiesHere.ToArray();
            }

            for (int i = 0; i < entities.Length; i++) {
                if (PlayerEntity.Players.TryGetValue(entities[i], out player)) {
                    if (player.ID == ID || Contains<Guid>(ignore, player.ID)) {
                        continue;
                    }

                    player.SendToClient(msg, colorSequence);
                }
            }
        }
    }

    protected bool Contains<T>(T[] a, T b) {
        for (int i = 0; i < a.Length; i++) {
            if (a[i].Equals(b))
                return true;
        }

        return false;
    }

    protected void StrikeTarget(BaseMobile target) {
        int dodgeVal = Rnd.Next(1, 101);
        if (dodgeVal >= 108 - Math.Sqrt((double)target.Stats.Dex * 7)) {
            BroadcastLocal($"{target.Name} dodged {Name}'s attack!", Color.RedD, target.ID);
            target.SendToClient($"You dodged {Name}'s attack!", Color.RedD);
            SendToClient($"{target.Name} dodged your attack!", Color.RedD);
        } else if (false) {
            // other possibilities for no damage TBD
        } else if (target.Stats.Health > 0 && target.GameState != GameState.Dead) {
            int dmg = Rnd.Next((int)(Stats.Str / 4f), (int)(Stats.Str / 3f));
            if (Hidden) {
                Hidden = false;
                SendToClient($"You surprise attack {target.Name}!", Color.White);
                dmg *= 2;
            }

            target.OnDeath += OnDeathEventReceiver;
            if (target.GameState != GameState.Combat || target.Target == null ||
                target.Target.GameState == GameState.Dead ||
                (target.Target.Stats != null && target.Target.Stats.Health <= 0)) {
                target.Target = this;
                target.GameState = GameState.Combat;
                target.LastCombatTick = World.CombatTick;
                target.SendToClient($"{Name} attacked you! You're now in combat!", Color.Red);
            }

            BroadcastLocal($"{target.Name} was struck by {Name} for {dmg} damage!", Color.Red, target.ID);
            target.SendToClient(
                Color.Red + $"{Name} struck {Color.Yellow + "*" + Color.White + "You " + Color.Red}for {dmg} damage!"
            );
            SendToClient(
                Color.White + "You " + Color.Red +
                $"struck {target.Name} for {dmg} damage!", Color.Red
            );
            target.Stats.Health -= dmg;
        } else {
            Target = null;
            GameState = GameState.Idle;
            SendToClient("* Combat disengaged *", Color.White);
        }
    }

    public bool IsTargetPresent() {
        if (Target == null || Target.Stats == null || Target.GameState == GameState.Dead ||
            Target.Stats.Health <= 0) return false;
        Room room = World.GetRoom(Stats.Location);
        if (room == null) return false;

        // Basic location check
        if (Target.Stats.Location != Stats.Location) return false;

        // Precise room presence check
        lock (room.EntitiesHere) {
            return room.EntitiesHere.Contains(Target.ID);
        }
    }

    void OnDeathEventReceiver(BaseMobile killed) {
        // If this mobile is dead, it can't get experience now can it?
        if (GameState == GameState.Dead) {
            return;
        }

        int distance = (Stats.Location - killed.Stats.Location).Max();
        // If targetting something far away and it dies, ignore it.
        if (distance > 3 && Target.Stats.Id == killed.Stats.Id) {
            GameState = GameState.Idle;
            Target = null;
            return;
        }

        // Only untarget and break combat if we're targetting what died and grant bonus experience.
        if (Target.Stats.Id == killed.Stats.Id) {
            SendToClient(
                "You killed " + killed.Name + "! You've gained 5 bonus experience.", Color.Cyan
            );
            Stats.Exp += 5;
            GameState = GameState.Idle;
            Target = null;
        }

        int exp = killed.Stats.GrantExperience();
        SendToClient("You've gained " + exp + " experience!", Color.Cyan);
        Stats.Exp += exp;
        // Target is dead, so unsubscribe to prevent any weird chain event triggers.
        killed.OnDeath -= OnDeathEventReceiver;
        SendToClient("* Combat disengaged *", Color.White);
    }

    public void DisplayVitals() {
        int barWidth = 20;
        string vitalsColor = Color.Green;
        string enemyHealth = "";
        int barCount;
        string bars;
        string spaces;

        // A yellow '+' to indicate the player can allocate a new stat point.
        string levelUpIndicator = Stats.StatAllocationNeeded ? Color.Yellow + " + " : "";
        string enemyHealthColor = Color.Green;
        if ((float)Stats.Health / Stats.MaxHealth < 0.33f)
            vitalsColor = Color.Red;
        if (Target != null) {
            barCount = (int)(barWidth * (float)Target.Stats.Health / Target.Stats.MaxHealth);
            if ((float)Target.Stats.Health / Target.Stats.MaxHealth < 0.33f)
                enemyHealthColor = Color.Red;
            bars = enemyHealthColor + new string('#', barCount);
            spaces = new string(' ', 20 - barCount);
            enemyHealth
                = $"{Color.Red}{Target.Name}: {Target.Stats.Health} {Color.White}[{bars}{spaces} {Color.White}] {enemyHealthColor}{Target.Stats.MaxHealth}";
        }

        barCount = (int)(barWidth * (float)Stats.Health / Stats.MaxHealth);
        bars = vitalsColor + new string('#', barCount);
        spaces = new string(' ', 20 - barCount);
        string healthBar
            = $"{vitalsColor}{Stats.Health} {Color.White}[{bars}{spaces}{Color.White}] {vitalsColor}{Stats.MaxHealth}";

        SendToClient(
            $"\n{Color.White}-- HP: {healthBar} -- {enemyHealthColor}{enemyHealth}{levelUpIndicator}\n", vitalsColor
        );
    }
}
}
