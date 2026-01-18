using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Script.Serialization;

namespace MudServer.Entity {
    public class QuestLog {
        // Stores IDs of discovered secret exits
        public List<string> DiscoveredExits { get; set; } = new List<string>();
        
        // Stores quest progress. Key is Quest Guid as string.
        public Dictionary<string, QuestProgress> Quests { get; set; } = new Dictionary<string, QuestProgress>();
    }

    public class QuestProgress {
        public bool Completed { get; set; }
        public int State { get; set; } // Current step in the quest
        public Dictionary<string, int> Counters { get; set; } = new Dictionary<string, int>(); // e.g., "GoblinsKilled": 5
    }
}
