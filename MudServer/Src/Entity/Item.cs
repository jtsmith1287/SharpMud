using System;
using System.Collections.Generic;
using MudServer.Interfaces;
using MudServer.Actions;
using System.Linq;

namespace MudServer.Entity {
    public class Item : Entity, IUsable, ITakeable {
        public ItemDef Def { get; set; }
        public List<IItemAction> OnUseActions { get; set; } = new List<IItemAction>();

        public Item() {
            Id = Guid.NewGuid();
            if (!World.World.Items.ContainsKey(Id)) {
                World.World.Items.Add(Id, this);
            }
        }

        public Item(ItemDef def) : this() {
            Def = def;
            Name = def.Name;
            Description = def.Description;
            Hidden = def.Hidden;
            Secret = def.Secret;

            if (def.OnUse != null) {
                foreach (var actionDef in def.OnUse) {
                    if (actionDef.ActionType == "RevealItemAction") {
                        OnUseActions.Add(new RevealItemAction { TargetId = actionDef.TargetId });
                    } else if (actionDef.ActionType == "SpawnCreatureAction") {
                        OnUseActions.Add(new SpawnCreatureAction { CreatureId = actionDef.CreatureId });
                    }
                }
            }
        }

        public void Use(IActor actor) {
            foreach (IItemAction action in OnUseActions) {
                action.Execute(actor, this);
            }
        }

        public void Take(IActor actor) {
            // Implementation for taking the item
        }
    }
}
