using System.Linq;
using System;
using System.IO;
using System.Collections.Generic;
using MudServer.Server;
using MudServer.Util;
using MudServer.Enums;
using MudServer.World;
using MudServer.Interfaces;
using System.Threading;

namespace MudServer.Entity {
    public class PlayerCharacter : BaseMobile {
        public static Dictionary<Guid, PlayerCharacter> Players = new Dictionary<Guid, PlayerCharacter>();
        public static Thread PlayerThread;
        System.Diagnostics.Stopwatch HealTick = new System.Diagnostics.Stopwatch();
        System.Diagnostics.Stopwatch ReviveTick = new System.Diagnostics.Stopwatch();
        int TickDuration = 3000;
        public Connection Conn { get; private set; }

        public QuestLog QuestLog {
            get => Stats.QuestLog;
            set => Stats.QuestLog = value;
        }

        public Coordinate3 Location {
            get => Stats.Location;
            set => Stats.Location = value;
        }

        public bool GodMode { get; set; }
        public bool Admin = true;
        public AccountState AccountState { get; set; }

        public static PlayerCharacter GetPlayerByID(Guid id) {
            return World.World.GetEntity(id) as PlayerCharacter;
        }

        public PlayerCharacter(Connection conn, Stats data) {
            Conn = conn;
            Id = data.Id;
            Name = data.Name;
            Stats = data;
            // Removed coupling to ThisBaseMobile
            Stats.OnZeroHealth += Die;
            Players.Add(Id, this);


            // The player hasn't been initialized yet.
            if (Stats.MaxHealth == 0) {
                Stats.MaxHealth = 15;
                Stats.Health = Stats.MaxHealth;
            }

            if (data.Location == null || !World.World.TryGetRoom(data.Location, out _)) {
                Move(Coordinate3.Zero);
            } else {
                Move(data.Location);
                if (data.Location == Coordinate3.Purgatory) {
                    GameState = GameState.Dead;
                    ReviveTick.Start();
                }
            }

            Conn.Send("Welcome!");
            AccountState = AccountState.Active;
            DisplayVitals();
            HealTick.Start();

            if (PlayerThread == null) {
                PlayerThread = new Thread(RunAllPlayerLogic);
                PlayerThread.Start();
                Console.WriteLine("PlayerThread started.");
            }
        }

        /// <summary>
        /// Runs all persistent logic for all players on a separate thread.
        /// </summary>
        void RunAllPlayerLogic() {
            try {
                while (true) {
                    int currentTick = World.World.CombatTick;
                    lock (Players) {
                        foreach (KeyValuePair<Guid, PlayerCharacter> entry in Players.ToArray()) {
                            PlayerCharacter player = entry.Value;
                            if (player.AccountState == AccountState.Active) {
                                player.ExecuteLogic(currentTick);
                            }
                        }
                    }

                    Thread.Sleep(33);
                }
            } catch (ThreadAbortException) {
                Console.WriteLine("PlayerThread aborted.");
            }
        }

        void ExecuteLogic(int currentTick) {
            if (GameState == GameState.Dead) {
                if (ReviveTick.ElapsedMilliseconds <= 5000) return;
                Stats.Health = Stats.MaxHealth;
                GameState = GameState.Idle;
                Move(Coordinate3.Zero);
                SendToClient("You have been revived! Welcome back to the land of the living.", Color.Green);
                ReviveTick.Reset();

                return;
            }

            switch (GameState) {
                case GameState.Combat: {
                    if (!IsTargetPresent()) {
                        if (GameState == GameState.Combat) {
                            GameState = GameState.Idle;
                            Target = null;
                            SendToClient("*Target lost. Combat disengaged!*", Color.White);
                        }
                        break;
                    }

                    if (LastCombatTick < currentTick) {
                        int attacksPerTick = Stats.GetNumberOfAttacks();
                        int ticksPassed = currentTick - (LastCombatTick == -1 ? currentTick - 1 : LastCombatTick);

                        for (int t = 0; t < ticksPassed; t++) {
                            for (int i = 0; i < attacksPerTick; i++) {
                                if (!IsTargetPresent()) {
                                    if (GameState == GameState.Combat) {
                                        GameState = GameState.Idle;
                                        Target = null;
                                        SendToClient("*Target lost. Combat disengaged!*", Color.White);
                                    }
                                    goto EndCombat;
                                } else {
                                    StrikeTarget(Target);
                                }
                            }
                        }

                        DisplayVitals();
                    EndCombat:
                        LastCombatTick = currentTick;
                    }

                    break;
                }
                default: {
                    if (HealTick.ElapsedMilliseconds <= TickDuration) return;
                
                    if (GameState == GameState.Resting) {
                        int healAmount = (int)(Stats.MaxHealth * 0.10f);
                        if (healAmount < 1) healAmount = 1;
                        Stats.Health += healAmount;
                        SendToClient($"You feel better... (+{healAmount} HP)", Color.Green);

                        if (Stats.Health >= Stats.MaxHealth) {
                            Stats.Health = Stats.MaxHealth;
                            AccountState = AccountState.Active;
                            SendToClient("You are fully rested and stand up.", Color.Cyan);
                            GameState = GameState.Idle;
                        }
                    }

                    HealTick.Restart();
                    break;
                }
            }
        }

        public void Close() {
            lock (Players) {
                Players.Remove(Id);
            }

            Target = null;
            GameState = GameState.Idle;

            if (!World.World.TryGetRoom(Stats.Location, out Room room)) return;

            lock (room.EntitiesHere) {
                room.EntitiesHere.Remove(Id);
            }
        }

        public override void SendToClient(string msg, string colorSequence = "") {
            //TODO: Word wrap this to 80 characters
            Conn.Send(colorSequence + msg + Color.Reset);
        }

        public string WaitForClientReply() {
            try {
                string reply = Conn.Reader.ReadLine();
                if (reply != null) {
                    return reply.Trim();
                } else {
                    // null
                    return reply;
                }
            } catch (IOException) {
                // Player disconnected.
                return null;
            }
        }

        private void Die(Stats data) {
            GameState = GameState.Dead;
            TriggerOnDeath(data);
            Target = null;
            SendToClient("You have been slain!");
            Move(Coordinate3.Purgatory);
            ReviveTick.Start();
        }

        internal void OnDisconnect() {
            Conn.OnDisconnect();
        }
    }
}
