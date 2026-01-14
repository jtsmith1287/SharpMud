import json

rooms = {
    "0 0 0": {
        "Location": {"X": 0, "Y": 0, "Z": 0},
        "Name": "Village Square",
        "Description": "The heart of the village is a bustling open space paved with worn cobblestones. A stone fountain stands at the center, its water sparkling under the sunlight. Paths lead out in all cardinal directions towards various parts of the settlement.",
        "ConnectedRooms": {
            "north": {"X": 0, "Y": 1, "Z": 0},
            "south": {"X": 0, "Y": -1, "Z": 0},
            "east": {"X": 1, "Y": 0, "Z": 0},
            "west": {"X": -1, "Y": 0, "Z": 0},
            "up": {"X": 0, "Y": 0, "Z": 1}
        },
        "SpawnersHere": [],
        "ID": "00000000-0000-0000-0000-000000000001"
    },
    "0 1 0": {
        "Location": {"X": 0, "Y": 1, "Z": 0},
        "Name": "North Gate",
        "Description": "A sturdy wooden gate marks the northern exit of the village, flanked by low stone walls. Guards occasionally pass through here, keeping a watchful eye on the horizon. Beyond the gate, a narrow path disappears into the thick green forest.",
        "ConnectedRooms": {
            "south": {"X": 0, "Y": 0, "Z": 0},
            "north": {"X": 0, "Y": 2, "Z": 0},
            "east": {"X": 1, "Y": 1, "Z": 0},
            "west": {"X": -1, "Y": 1, "Z": 0}
        },
        "SpawnersHere": [],
        "ID": "00000000-0000-0000-0000-000000000002"
    },
    "0 2 0": {
        "Location": {"X": 0, "Y": 2, "Z": 0},
        "Name": "Forest Path",
        "Description": "The air grows cooler as the village sounds fade behind you, replaced by the rustle of leaves and bird songs. Tall oaks arch over the dirt path, casting long shadows across the ground. It is a peaceful but lonely stretch of road leading away from civilization.",
        "ConnectedRooms": {
            "south": {"X": 0, "Y": 1, "Z": 0},
            "north": {"X": 0, "Y": 3, "Z": 0}
        },
        "SpawnersHere": [],
        "ID": "00000000-0000-0000-0000-000000000003"
    },
    "0 3 0": {
        "Location": {"X": 0, "Y": 3, "Z": 0},
        "Name": "Deep Forest",
        "Description": "The trees here are much larger and closer together, blocking out most of the sunlight and creating a permanent twilight. The path is nearly overgrown with tangled roots and thick briars. Every snap of a twig seems unnaturally loud in the oppressive silence of the deep woods.",
        "ConnectedRooms": {
            "south": {"X": 0, "Y": 2, "Z": 0}
        },
        "SpawnersHere": [],
        "ID": "00000000-0000-0000-0000-000000000011"
    },
    "0 -1 0": {
        "Location": {"X": 0, "Y": -1, "Z": 0},
        "Name": "South Gate",
        "Description": "The southern entrance to the village is wide and well-traveled by merchants and farmers. Dust kicks up from the dry road, and the smell of hay lingers in the air. A small guard shack stands to the side, though it currently looks empty.",
        "ConnectedRooms": {
            "north": {"X": 0, "Y": 0, "Z": 0},
            "south": {"X": 0, "Y": -2, "Z": 0},
            "east": {"X": 1, "Y": -1, "Z": 0},
            "west": {"X": -1, "Y": -1, "Z": 0}
        },
        "SpawnersHere": [],
        "ID": "00000000-0000-0000-0000-000000000004"
    },
    "0 -2 0": {
        "Location": {"X": 0, "Y": -2, "Z": 0},
        "Name": "Dusty Road",
        "Description": "This long stretch of road leads south toward the distant hills and neighboring farmlands. The ground is parched and cracked, showing the signs of a dry season. Carts have left deep ruts in the earth, making for a bumpy journey for any traveler.",
        "ConnectedRooms": {
            "north": {"X": 0, "Y": -1, "Z": 0},
            "south": {"X": 0, "Y": -3, "Z": 0}
        },
        "SpawnersHere": [],
        "ID": "00000000-0000-0000-0000-000000000005"
    },
    "0 -3 0": {
        "Location": {"X": 0, "Y": -3, "Z": 0},
        "Name": "Farmland",
        "Description": "Neat rows of crops stretch out toward the horizon, swaying gently in the breeze. A small farmhouse and a weathered barn can be seen in the distance, surrounded by low fences. The soil is dark and rich, a testament to the hard work of the local farmers who tend these fields.",
        "ConnectedRooms": {
            "north": {"X": 0, "Y": -2, "Z": 0}
        },
        "SpawnersHere": [],
        "ID": "00000000-0000-0000-0000-000000000016"
    },
    "1 0 0": {
        "Location": {"X": 1, "Y": 0, "Z": 0},
        "Name": "East Street",
        "Description": "East Street is lined with small cottages and colorful flower boxes beneath their windows. The smell of freshly baked bread wafts from a nearby window, mixing with the scent of woodsmoke. This part of town feels quiet and residential compared to the square.",
        "ConnectedRooms": {
            "west": {"X": 0, "Y": 0, "Z": 0},
            "east": {"X": 2, "Y": 0, "Z": 0}
        },
        "SpawnersHere": [],
        "ID": "00000000-0000-0000-0000-000000000006"
    },
    "2 0 0": {
        "Location": {"X": 2, "Y": 0, "Z": 0},
        "Name": "The Tipsy Tankard Inn",
        "Description": "Warm golden light spills from the windows of the village inn, and the sound of laughter can be heard from within. A heavy oak door leads inside, where travelers find rest and a hot meal. A wooden sign above the door depicts a foaming mug of ale.",
        "ConnectedRooms": {
            "west": {"X": 1, "Y": 0, "Z": 0},
            "down": {"X": 2, "Y": 0, "Z": -1},
            "north": {"X": 2, "Y": 1, "Z": 0}
        },
        "SpawnersHere": [],
        "ID": "00000000-0000-0000-0000-000000000007"
    },
    "2 0 -1": {
        "Location": {"X": 2, "Y": 0, "Z": -1},
        "Name": "Inn Cellar",
        "Description": "The air here is cool and damp, smelling of earth and fermented yeast. Rows of large wooden barrels are stacked against the stone walls, filled with the inn's finest ale. Cobwebs hang from the low wooden rafters, and a faint skittering sound can be heard from the dark corners.",
        "ConnectedRooms": {
            "up": {"X": 2, "Y": 0, "Z": 0}
        },
        "SpawnersHere": [],
        "ID": "00000000-0000-0000-0000-000000000017"
    },
    "2 1 0": {
        "Location": {"X": 2, "Y": 1, "Z": 0},
        "Name": "Bakery",
        "Description": "The delightful aroma of freshly baked bread and sweet pastries is overwhelming in this cozy shop. Flour dusts every surface, and a large stone oven in the back glows with the heat of a fresh batch. Baskets of crusty loaves and trays of colorful tarts are displayed temptingly on the wooden counters.",
        "ConnectedRooms": {
            "south": {"X": 2, "Y": 0, "Z": 0}
        },
        "SpawnersHere": [],
        "ID": "00000000-0000-0000-0000-000000000018"
    },
    "-1 0 0": {
        "Location": {"X": -1, "Y": 0, "Z": 0},
        "Name": "West Street",
        "Description": "The western road is flanked by several workshops and small storefronts. The rhythmic sound of hammering echoes from one of the buildings, signifying a busy day for the local craftsmen. A few chickens roam the edges of the street, pecking at the ground for food.",
        "ConnectedRooms": {
            "east": {"X": 0, "Y": 0, "Z": 0},
            "west": {"X": -2, "Y": 0, "Z": 0}
        },
        "SpawnersHere": [],
        "ID": "00000000-0000-0000-0000-000000000008"
    },
    "-2 0 0": {
        "Location": {"X": -2, "Y": 0, "Z": 0},
        "Name": "The Iron Anvil Smithy",
        "Description": "Heat radiates from the open doors of the blacksmith's forge, and the air is thick with the smell of coal smoke. Tools and horseshoes hang from hooks along the walls, gleaming in the firelight. A large stone anvil sits in the center of the workshop, waiting for the smith's next strike.",
        "ConnectedRooms": {
            "east": {"X": -1, "Y": 0, "Z": 0},
            "south": {"X": -2, "Y": -1, "Z": 0}
        },
        "SpawnersHere": [],
        "ID": "00000000-0000-0000-0000-000000000009"
    },
    "-2 -1 0": {
        "Location": {"X": -2, "Y": -1, "Z": 0},
        "Name": "Fletcher's Shop",
        "Description": "Bunches of seasoned wood and bundles of feathers are scattered across the workbenches in this narrow workshop. The sound of carving knives and the smell of cedar shavings fill the room. Finished bows and quivers of arrows hang from the walls, crafted with precision and care.",
        "ConnectedRooms": {
            "north": {"X": -2, "Y": 0, "Z": 0}
        },
        "SpawnersHere": [],
        "ID": "00000000-0000-0000-0000-000000000019"
    },
    "1 1 0": {
        "Location": {"X": 1, "Y": 1, "Z": 0},
        "Name": "The Gilded Coin General Store",
        "Description": "Shelves packed with various goods line the walls of this cramped but organized shop. The smell of dried herbs and salted meat fills the air. A polished wooden counter sits near the entrance, where the shopkeeper handles transactions with locals and travelers alike.",
        "ConnectedRooms": {
            "west": {"X": 0, "Y": 1, "Z": 0}
        },
        "SpawnersHere": [],
        "ID": "00000000-0000-0000-0000-000000000012"
    },
    "-1 1 0": {
        "Location": {"X": -1, "Y": 1, "Z": 0},
        "Name": "Village Library",
        "Description": "Tall bookshelves reach toward the ceiling, overflowing with dusty tomes and ancient scrolls. The room is quiet, illuminated by flickering candles that cast dancing shadows on the worn rugs. A large oak table in the center is covered with open maps and half-finished manuscripts.",
        "ConnectedRooms": {
            "east": {"X": 0, "Y": 1, "Z": 0}
        },
        "SpawnersHere": [],
        "ID": "00000000-0000-0000-0000-000000000013"
    },
    "1 -1 0": {
        "Location": {"X": 1, "Y": -1, "Z": 0},
        "Name": "Temple of Light",
        "Description": "Sunlight streams through colorful stained-glass windows, painting vibrant patterns on the polished marble floor. A serene atmosphere pervades the hall, accompanied by the faint scent of incense. An altar of white stone stands at the far end, glowing with a soft, comforting radiance.",
        "ConnectedRooms": {
            "west": {"X": 0, "Y": -1, "Z": 0}
        },
        "SpawnersHere": [],
        "ID": "00000000-0000-0000-0000-000000000014"
    },
    "-1 -1 0": {
        "Location": {"X": -1, "Y": -1, "Z": 0},
        "Name": "Guard House",
        "Description": "This austere room serves as the headquarters for the village's small watch. Racks of spears and shields are mounted on the stone walls, and a large map of the surrounding area is pinned to a wooden board. A few simple benches and a heavy desk suggest that the guards spend their shifts here in constant readiness.",
        "ConnectedRooms": {
            "east": {"X": 0, "Y": -1, "Z": 0}
        },
        "SpawnersHere": [],
        "ID": "00000000-0000-0000-0000-000000000015"
    },
    "0 0 1": {
        "Location": {"X": 0, "Y": 0, "Z": 1},
        "Name": "Village Watchtower",
        "Description": "High above the square, this wooden platform provides an excellent view of the surrounding countryside. From here, you can see the rooftops of the village and the winding roads that lead into the distance. A large bronze bell hangs from a beam, used to signal emergencies to the people below.",
        "ConnectedRooms": {
            "down": {"X": 0, "Y": 0, "Z": 0}
        },
        "SpawnersHere": [],
        "ID": "00000000-0000-0000-0000-000000000010"
    },
    "2147483647 2147483647 2147483647": {
        "Location": {"X": 2147483647, "Y": 2147483647, "Z": 2147483647},
        "Name": "Purgatory",
        "Description": "A vast, featureless void that seems to exist outside of time and space. There is no sound here, and no visible horizon in any direction. It is a place for those who have lost their way in the world, waiting for a path to reappear.",
        "ConnectedRooms": {},
        "SpawnersHere": [],
        "ID": "00000000-0000-0000-0000-000000000000"
    }
}

with open('bin/Debug/world.json', 'w') as f:
    json.dump(rooms, f)
