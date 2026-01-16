import json
import os
import sys

def load_json(file_path):
    if not os.path.exists(file_path):
        print(f"Error: File not found at {file_path}")
        return None
    try:
        with open(file_path, 'r', encoding='utf-8-sig') as f:
            return json.load(f)
    except json.JSONDecodeError as e:
        print(f"Error: Failed to parse JSON in {file_path}: {e}")
        return None

DIRECTION_OFFSETS = {
    "north": (0, 1, 0),
    "south": (0, -1, 0),
    "east": (1, 0, 0),
    "west": (-1, 0, 0),
    "up": (0, 0, 1),
    "down": (0, 0, -1)
}

def validate_maps(master_path):
    print(f"Validating master map file: {master_path}")
    master_data = load_json(master_path)
    if not master_data:
        return False

    map_files = master_data.get("Maps", [])
    if not map_files:
        print("Warning: No map files listed in master file.")
        return True

    base_dir = os.path.dirname(master_path)
    all_rooms = {}
    is_valid = True
    
    coordinates_seen = {}
    ids_seen = {}

    for map_file in map_files:
        map_path = os.path.join(base_dir, map_file)
        print(f"  Validating map chunk: {map_file}...")
        world_data = load_json(map_path)
        if world_data is None:
            is_valid = False
            continue

        for key, room in world_data.items():
            # Check key format
            try:
                kx, ky, kz = map(int, key.split())
            except ValueError:
                print(f"  Error in {map_file}: Invalid coordinate key format '{key}'.")
                is_valid = False

            # Check Location object
            loc = room.get("Location")
            if not loc:
                print(f"  Error in {map_file}: Room '{key}' is missing 'Location'.")
                is_valid = False
            else:
                x, y, z = loc.get("X"), loc.get("Y"), loc.get("Z")
                coord_tuple = (x, y, z)
                
                if (x, y, z) != (kx, ky, kz):
                    print(f"  Error in {map_file}: Room key '{key}' does not match Location values ({x}, {y}, {z}).")
                    is_valid = False

                if coord_tuple in coordinates_seen:
                    print(f"  Error: Duplicate coordinate found! '{key}' in {map_file} and '{coordinates_seen[coord_tuple]['key']}' in {coordinates_seen[coord_tuple]['file']} share {coord_tuple}.")
                    is_valid = False
                else:
                    coordinates_seen[coord_tuple] = {"key": key, "file": map_file}
                    
            # Check for duplicate IDs
            room_id = room.get("ID")
            if not room_id:
                print(f"  Error in {map_file}: Room '{key}' is missing 'ID'.")
                is_valid = False
            elif room_id in ids_seen:
                print(f"  Error: Duplicate ID found! Room '{key}' in {map_file} shares ID '{room_id}' with room '{ids_seen[room_id]['key']}' in {ids_seen[room_id]['file']}.")
                is_valid = False
            else:
                ids_seen[room_id] = {"key": key, "file": map_file}
            
            all_rooms[key] = {"room": room, "file": map_file}

    # Check connections and directions across all rooms
    print("  Checking connections and directions...")
    for key, data in all_rooms.items():
        room = data["room"]
        map_file = data["file"]
        loc = room["Location"]
        x, y, z = loc["X"], loc["Y"], loc["Z"]
        
        # Check normal connections
        connections = room.get("ConnectedRooms", {})
        for direction, target_loc in connections.items():
            tx, ty, tz = target_loc.get("X"), target_loc.get("Y"), target_loc.get("Z")
            target_key = f"{tx} {ty} {tz}"
            
            # 1. Check if target exists
            if target_key not in all_rooms:
                print(f"  Error: Room '{key}' in {map_file} has a '{direction}' exit to non-existent room '{target_key}'.")
                is_valid = False
                continue

            # 2. Check if direction is correct
            if direction in DIRECTION_OFFSETS:
                dx, dy, dz = DIRECTION_OFFSETS[direction]
                if (x + dx != tx) or (y + dy != ty) or (z + dz != tz):
                    print(f"  Error: Room '{key}' in {map_file} has '{direction}' exit to '{target_key}', but coordinates don't match (expected {x+dx} {y+dy} {z+dz}).")
                    is_valid = False
            else:
                print(f"  Warning: Room '{key}' in {map_file} has unknown direction '{direction}'.")

        # 3. Check for asymmetrical connections
        for direction, target_loc in connections.items():
            tx, ty, tz = target_loc.get("X"), target_loc.get("Y"), target_loc.get("Z")
            target_key = f"{tx} {ty} {tz}"
            if target_key in all_rooms:
                target_room = all_rooms[target_key]["room"]
                target_connections = target_room.get("ConnectedRooms", {})
                
                # Find the reverse direction
                reverse_dir = None
                for d, offset in DIRECTION_OFFSETS.items():
                    if (tx + offset[0] == x) and (ty + offset[1] == y) and (tz + offset[2] == z):
                        reverse_dir = d
                        break
                
                if reverse_dir:
                    found_back = False
                    for d_back, loc_back in target_connections.items():
                        if loc_back.get("X") == x and loc_back.get("Y") == y and loc_back.get("Z") == z:
                            found_back = True
                            break
                    if not found_back:
                        print(f"  Warning: Asymmetrical connection! Room '{key}' leads {direction} to '{target_key}', but '{target_key}' has no exit back to '{key}'.")

        # Check entry room and invisible connections
        if room.get("IsEntryRoom"):
            inv_connections = room.get("InvisibleConnections", {})
            if not inv_connections:
                print(f"  Warning: Room '{key}' in {map_file} is marked as an Entry Room but has no InvisibleConnections.")
            
            for direction, target_loc in inv_connections.items():
                tx, ty, tz = target_loc.get("X"), target_loc.get("Y"), target_loc.get("Z")
                target_key = f"{tx} {ty} {tz}"
                if target_key not in all_rooms:
                    print(f"  Error: Entry Room '{key}' in {map_file} has an invisible '{direction}' exit to non-existent room '{target_key}'.")
                    is_valid = False

    if is_valid:
        print("Validation successful! All chunks are consistent.")
    else:
        print("Validation failed with errors.")

    return is_valid, all_rooms

def resolve_asymmetrical_connections(all_rooms):
    print("\nResolving asymmetrical connections...")
    modified_files = set()
    
    for key, data in all_rooms.items():
        room = data["room"]
        file = data["file"]
        loc = room["Location"]
        x, y, z = loc["X"], loc["Y"], loc["Z"]
        
        connections = room.get("ConnectedRooms", {})
        to_delete = []
        
        for direction, target_loc in list(connections.items()):
            tx, ty, tz = target_loc.get("X"), target_loc.get("Y"), target_loc.get("Z")
            target_key = f"{tx} {ty} {tz}"
            
            if target_key in all_rooms:
                target_data = all_rooms[target_key]
                target_room = target_data["room"]
                target_file = target_data["file"]
                target_connections = target_room.get("ConnectedRooms", {})
                
                # Check if there is a back connection
                found_back = False
                for d_back, loc_back in target_connections.items():
                    if loc_back.get("X") == x and loc_back.get("Y") == y and loc_back.get("Z") == z:
                        found_back = True
                        break
                
                if not found_back:
                    print(f"\nOne-way connection found: '{key}' ({room['Name']}) --{direction}--> '{target_key}' ({target_room['Name']})")
                    if "--fix" in sys.argv:
                        choice = "a" # Default to add back-connection if --fix is used and we are non-interactive
                    else:
                        try:
                            choice = input(f"Choose action: [a]dd back-connection to {target_room['Name']}, [d]elete exit from {room['Name']}, [s]kip: ").lower()
                        except EOFError:
                            choice = "s"
                    
                    if choice == 'a':
                        # Find reverse direction
                        rev_dir = None
                        for d, offset in DIRECTION_OFFSETS.items():
                            if (tx + offset[0] == x) and (ty + offset[1] == y) and (tz + offset[2] == z):
                                rev_dir = d
                                break
                        
                        if rev_dir:
                            target_connections[rev_dir] = {"X": x, "Y": y, "Z": z}
                            modified_files.add(target_file)
                            print(f"Added {rev_dir} exit to {target_room['Name']} pointing to {key}.")
                        else:
                            print(f"Error: Could not determine reverse direction for {direction}!")
                    elif choice == 'd':
                        to_delete.append(direction)
                        modified_files.add(file)
                        print(f"Deleted {direction} exit from {room['Name']}.")
        
        for direction in to_delete:
            del connections[direction]

    if modified_files:
        print("\nSaving changes...")
        # Group rooms by file
        rooms_by_file = {}
        for k, d in all_rooms.items():
            f = d["file"]
            if f not in rooms_by_file:
                rooms_by_file[f] = {}
            rooms_by_file[f][k] = d["room"]
            
        base_dir = os.path.dirname(os.path.join("bin", "Debug", "maps", "maps.json")) # Default base dir
        for f in modified_files:
            map_path = os.path.join(base_dir, f)
            with open(map_path, 'w', encoding='utf-8') as out:
                json.dump(rooms_by_file[f], out, indent=2)
            print(f"Saved {map_path}")

if __name__ == "__main__":
    path = os.path.join("bin", "Debug", "maps", "maps.json")
    if len(sys.argv) > 1 and not sys.argv[1].startswith("--"):
        path = sys.argv[1]
    
    success, all_rooms = validate_maps(path)
    if all_rooms:
        resolve_asymmetrical_connections(all_rooms)
    
    if not success:
        sys.exit(1)
