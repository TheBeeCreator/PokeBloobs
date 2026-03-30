using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokeBloobs.Classes
{
    internal class DictionaryEvos
    {
        public static Dictionary<string, List<string[]>> evolutionChains = new Dictionary<string, List<string[]>>
        {
            // Gen 1
            { "Bulbasaur", new List<string[]> { new[] { "Bulbasaur", "Ivysaur", "Venusaur" } } },
            { "Charmander", new List<string[]> { new[] { "Charmander", "Charmeleon", "Charizard" } } },
            { "Squirtle", new List<string[]> { new[] { "Squirtle", "Wartortle", "Blastoise" } } },
            { "Caterpie", new List<string[]> { new[] { "Caterpie", "Metapod", "Butterfree" } } },
            { "Weedle", new List<string[]> { new[] { "Weedle", "Kakuna", "Beedrill" } } },
            { "Pidgey", new List<string[]> { new[] { "Pidgey", "Pidgeotto", "Pidgeot" } } },
            { "Rattata", new List<string[]> { new[] { "Rattata", "Raticate" } } },
            { "Spearow", new List<string[]> { new[] { "Spearow", "Fearow" } } },
            { "Ekans", new List<string[]> { new[] { "Ekans", "Arbok" } } },
            { "Pikachu", new List<string[]> { new[] { "Pichu", "Pikachu", "Raichu" } } }, // includes Pichu
            { "Sandshrew", new List<string[]> { new[] { "Sandshrew", "Sandslash" } } },
            { "Nidoran♀", new List<string[]> { new[] { "Nidoran♀", "Nidorina", "Nidoqueen" } } },
            { "Nidoran♂", new List<string[]> { new[] { "Nidoran♂", "Nidorino", "Nidoking" } } },
            { "Clefairy", new List<string[]> { new[] { "Cleffa", "Clefairy", "Clefable" } } },
            { "Vulpix", new List<string[]> { new[] { "Vulpix", "Ninetales" } } },
            { "Jigglypuff", new List<string[]> { new[] { "Igglybuff", "Jigglypuff", "Wigglytuff" } } },
            { "Zubat", new List<string[]> { new[] { "Zubat", "Golbat", "Crobat" } } },
            { "Oddish", new List<string[]> {
                new[] { "Oddish", "Gloom", "Vileplume" },
                new[] { "Oddish", "Gloom", "Bellossom" } // Gen 2 alternate evo
            }},
            { "Paras", new List<string[]> { new[] { "Paras", "Parasect" } } },
            { "Venonat", new List<string[]> { new[] { "Venonat", "Venomoth" } } },
            { "Diglett", new List<string[]> { new[] { "Diglett", "Dugtrio" } } },
            { "Meowth", new List<string[]> { new[] { "Meowth", "Persian" } } },
            { "Psyduck", new List<string[]> { new[] { "Psyduck", "Golduck" } } },
            { "Mankey", new List<string[]> { new[] { "Mankey", "Primeape" } } },
            { "Growlithe", new List<string[]> { new[] { "Growlithe", "Arcanine" } } },
            { "Poliwag", new List<string[]> { new[] { "Poliwag", "Poliwhirl", "Poliwrath" }, new[] { "Poliwag", "Poliwhirl", "Politoed" } } },
            { "Abra", new List<string[]> { new[] { "Abra", "Kadabra", "Alakazam" } } },
            { "Machop", new List<string[]> { new[] { "Machop", "Machoke", "Machamp" } } },
            { "Bellsprout", new List<string[]> { new[] { "Bellsprout", "Weepinbell", "Victreebel" } } },
            { "Geodude", new List<string[]> { new[] { "Geodude", "Graveler", "Golem" } } },
            { "Slowpoke", new List<string[]> { new[] { "Slowpoke", "Slowbro" }, new[] { "Slowpoke", "Slowking" } } },
            { "Magnemite", new List<string[]> { new[] { "Magnemite", "Magneton", "Magnezone" } } },
            { "Farfetch'd", new List<string[]> { new[] { "Farfetch'd" } } },
            { "Seel", new List<string[]> { new[] { "Seel", "Dewgong" } } },
            { "Grimer", new List<string[]> { new[] { "Grimer", "Muk" } } },
            { "Shellder", new List<string[]> { new[] { "Shellder", "Cloyster" } } },
            { "Gastly", new List<string[]> { new[] { "Gastly", "Haunter", "Gengar" } } },
            { "Onix", new List<string[]> { new[] { "Onix", "Steelix" } } },
            { "Drowzee", new List<string[]> { new[] { "Drowzee", "Hypno" } } },
            { "Krabby", new List<string[]> { new[] { "Krabby", "Kingler" } } },
            { "Voltorb", new List<string[]> { new[] { "Voltorb", "Electrode" } } },
            { "Exeggcute", new List<string[]> { new[] { "Exeggcute", "Exeggutor" } } },
            { "Cubone", new List<string[]> { new[] { "Cubone", "Marowak" } } },
            { "Lickitung", new List<string[]> { new[] { "Lickitung", "Lickilicky" } } },
            { "Koffing", new List<string[]> { new[] { "Koffing", "Weezing" } } },
            { "Rhyhorn", new List<string[]> { new[] { "Rhyhorn", "Rhydon", "Rhyperior" } } },
            { "Tangela", new List<string[]> { new[] { "Tangela", "Tangrowth" } } },
            { "Horsea", new List<string[]> { new[] { "Horsea", "Seadra", "Kingdra" } } },
            { "Goldeen", new List<string[]> { new[] { "Goldeen", "Seaking" } } },
            { "Staryu", new List<string[]> { new[] { "Staryu", "Starmie" } } },
            { "Eevee", new List<string[]> {
                new[] { "Eevee", "Vaporeon" },
                new[] { "Eevee", "Jolteon" },
                new[] { "Eevee", "Flareon" },
                new[] { "Eevee", "Espeon" },
                new[] { "Eevee", "Umbreon" },
                new[] { "Eevee", "Leafeon" },
                new[] { "Eevee", "Glaceon" },
                new[] { "Eevee", "Sylveon" }
            }},
            { "Porygon", new List<string[]> { new[] { "Porygon", "Porygon2", "Porygon-Z" } } },
            { "Omanyte", new List<string[]> { new[] { "Omanyte", "Omastar" } } },
            { "Kabuto", new List<string[]> { new[] { "Kabuto", "Kabutops" } } },
            { "Dratini", new List<string[]> { new[] { "Dratini", "Dragonair", "Dragonite" } } },

            //Gen 2
            // Starter Pokémon
            { "Chikorita", new List<string[]> { new[] { "Chikorita", "Bayleef", "Meganium" } } },
            { "Cyndaquil", new List<string[]> { new[] { "Cyndaquil", "Quilava", "Typhlosion" } } },
            { "Totodile", new List<string[]> { new[] { "Totodile", "Croconaw", "Feraligatr" } } },

            // Early-route Pokémon
            { "Sentret", new List<string[]> { new[] { "Sentret", "Furret" } } },
            { "Hoothoot", new List<string[]> { new[] { "Hoothoot", "Noctowl" } } },
            { "Ledyba", new List<string[]> { new[] { "Ledyba", "Ledian" } } },
            { "Spinarak", new List<string[]> { new[] { "Spinarak", "Ariados" } } },
            { "Chinchou", new List<string[]> { new[] { "Chinchou", "Lanturn" } } },

            // Fossil Pokémon
            { "Togepi", new List<string[]> { new[] { "Togepi", "Togetic", "Togekiss" } } },
            { "Natu", new List<string[]> { new[] { "Natu", "Xatu" } } },

            // Common Pokémon with branching evolutions
            { "Mareep", new List<string[]> { new[] { "Mareep", "Flaaffy", "Ampharos" } } },
            { "Marill", new List<string[]> { new[] { "Marill", "Azumarill" } } },
            { "Wooper", new List<string[]> { new[] { "Wooper", "Quagsire" } } },
            { "Espeon", new List<string[]> { new[] { "Eevee", "Espeon" } } },
            { "Umbreon", new List<string[]> { new[] { "Eevee", "Umbreon" } } },
            { "Slowking", new List<string[]> { new[] { "Slowpoke", "Slowking" } } },
            { "Bellossom", new List<string[]> { new[] { "Oddish", "Gloom", "Bellossom" } } },
            { "Politoed", new List<string[]> { new[] { "Poliwag", "Poliwhirl", "Politoed" } } },
            { "Steelix", new List<string[]> { new[] { "Onix", "Steelix" } } },
            { "Scizor", new List<string[]> { new[] { "Scyther", "Scizor" } } },
            { "Heracross", new List<string[]> { new[] { "Heracross" } } }, // standalone

            // Other notable Pokémon
            { "Tyranitar", new List<string[]> { new[] { "Larvitar", "Pupitar", "Tyranitar" } } },
            { "Kingdra", new List<string[]> { new[] { "Horsea", "Seadra", "Kingdra" } } },
            { "Donphan", new List<string[]> { new[] { "Phanpy", "Donphan" } } },
            { "Porygon2", new List<string[]> { new[] { "Porygon", "Porygon2", "Porygon-Z" } } },

            // Tyrogue evolutions
            { "Hitmonlee", new List<string[]> { new[] { "Tyrogue", "Hitmonlee" } } },
            { "Hitmonchan", new List<string[]> { new[] { "Tyrogue", "Hitmonchan" } } },
            { "Hitmontop", new List<string[]> { new[] { "Tyrogue", "Hitmontop" } } },

            // Legendaries / standalones
            { "Raikou", new List<string[]> { new[] { "Raikou" } } },
            { "Entei", new List<string[]> { new[] { "Entei" } } },
            { "Suicune", new List<string[]> { new[] { "Suicune" } } },
            { "Lugia", new List<string[]> { new[] { "Lugia" } } },
            { "Ho-Oh", new List<string[]> { new[] { "Ho-Oh" } } },
            { "Celebi", new List<string[]> { new[] { "Celebi" } } },

            // Unown variants grouped
            { "Unown", new List<string[]> {
                new[] { "Unown-A" }, new[] { "Unown-B" }, new[] { "Unown-C" }, new[] { "Unown-D" },
                new[] { "Unown-E" }, new[] { "Unown-F" }, new[] { "Unown-G" }, new[] { "Unown-H" },
                new[] { "Unown-I" }, new[] { "Unown-J" }, new[] { "Unown-K" }, new[] { "Unown-L" },
                new[] { "Unown-M" }, new[] { "Unown-N" }, new[] { "Unown-O" }, new[] { "Unown-P" },
                new[] { "Unown-Q" }, new[] { "Unown-R" }, new[] { "Unown-S" }, new[] { "Unown-T" },
                new[] { "Unown-U" }, new[] { "Unown-V" }, new[] { "Unown-W" }, new[] { "Unown-X" },
                new[] { "Unown-Y" }, new[] { "Unown-Z" }
            }},

            // Gen 3
            { "Treecko", new List<string[]> { new[] { "Treecko", "Grovyle", "Sceptile" } } },
            { "Torchic", new List<string[]> { new[] { "Torchic", "Combusken", "Blaziken" } } },
            { "Mudkip", new List<string[]> { new[] { "Mudkip", "Marshtomp", "Swampert" } } },
            { "Poochyena", new List<string[]> { new[] { "Poochyena", "Mightyena" } } },
            { "Zigzagoon", new List<string[]> { new[] { "Zigzagoon", "Linoone" } } },
            { "Lotad", new List<string[]> { new[] { "Lotad", "Lombre", "Ludicolo" } } },
            { "Seedot", new List<string[]> { new[] { "Seedot", "Nuzleaf", "Shiftry" } } },
            { "Ralts", new List<string[]> { new[] { "Ralts", "Kirlia", "Gardevoir" }, new[] { "Ralts", "Kirlia", "Gallade" } } },
            { "Surskit", new List<string[]> { new[] { "Surskit", "Masquerain" } } },
            { "Shroomish", new List<string[]> { new[] { "Shroomish", "Breloom" } } },
            { "Slakoth", new List<string[]> { new[] { "Slakoth", "Vigoroth", "Slaking" } } },
            { "Nincada", new List<string[]> { new[] { "Nincada", "Ninjask", "Shedinja" } } },
            { "Whismur", new List<string[]> { new[] { "Whismur", "Loudred", "Exploud" } } },
            { "Makuhita", new List<string[]> { new[] { "Makuhita", "Hariyama" } } },
            { "Geodude (Hoenn)", new List<string[]> { new[] { "Geodude", "Graveler", "Golem" } } },
            { "Nosepass", new List<string[]> { new[] { "Nosepass", "Probopass" } } },
            { "Skitty", new List<string[]> { new[] { "Skitty", "Delcatty" } } },
            { "Meditite", new List<string[]> { new[] { "Meditite", "Medicham" } } },

            // Standalone Pokémon
            { "Regirock", new List<string[]> { new[] { "Regirock" } } },
            { "Regice", new List<string[]> { new[] { "Regice" } } },
            { "Registeel", new List<string[]> { new[] { "Registeel" } } },
            { "Latias", new List<string[]> { new[] { "Latias" } } },
            { "Latios", new List<string[]> { new[] { "Latios" } } },
            { "Kyogre", new List<string[]> { new[] { "Kyogre" } } },
            { "Groudon", new List<string[]> { new[] { "Groudon" } } },
            { "Rayquaza", new List<string[]> { new[] { "Rayquaza" } } },

            // Gen 4
            { "Turtwig", new List<string[]> { new[] { "Turtwig", "Grotle", "Torterra" } } },
            { "Chimchar", new List<string[]> { new[] { "Chimchar", "Monferno", "Infernape" } } },
            { "Piplup", new List<string[]> { new[] { "Piplup", "Prinplup", "Empoleon" } } },
            { "Starly", new List<string[]> { new[] { "Starly", "Staravia", "Staraptor" } } },
            { "Bidoof", new List<string[]> { new[] { "Bidoof", "Bibarel" } } },
            { "Kricketot", new List<string[]> { new[] { "Kricketot", "Kricketune" } } },
            { "Shinx", new List<string[]> { new[] { "Shinx", "Luxio", "Luxray" } } },
            { "Budew", new List<string[]> { new[] { "Budew", "Roselia", "Roserade" } } },
            { "Cranidos", new List<string[]> { new[] { "Cranidos", "Rampardos" } } },
            { "Shieldon", new List<string[]> { new[] { "Shieldon", "Bastiodon" } } },
            { "Burmy", new List<string[]> { new[] { "Burmy", "Wormadam", "Mothim" } } },
            { "Combee", new List<string[]> { new[] { "Combee", "Vespiquen" } } },
            { "Pachirisu", new List<string[]> { new[] { "Pachirisu" } } },
            { "Buizel", new List<string[]> { new[] { "Buizel", "Floatzel" } } },
            { "Cherubi", new List<string[]> { new[] { "Cherubi", "Cherrim" } } },
            { "Shellos", new List<string[]> { new[] { "Shellos", "Gastrodon" } } },
            { "Drifloon", new List<string[]> { new[] { "Drifloon", "Drifblim" } } },

            // Standalone Pokémon
            { "Uxie", new List<string[]> { new[] { "Uxie" } } },
            { "Mesprit", new List<string[]> { new[] { "Mesprit" } } },
            { "Azelf", new List<string[]> { new[] { "Azelf" } } },
            { "Dialga", new List<string[]> { new[] { "Dialga" } } },
            { "Palkia", new List<string[]> { new[] { "Palkia" } } },
            { "Heatran", new List<string[]> { new[] { "Heatran" } } },
            { "Regigigas", new List<string[]> { new[] { "Regigigas" } } },
            { "Giratina", new List<string[]> { new[] { "Giratina" } } },
            { "Cresselia", new List<string[]> { new[] { "Cresselia" } } },

            //Rotom grouped forms
            { "Rotom", new List<string[]> {
                new[] { "Rotom" },
                new[] { "Rotom Heat" },
                new[] { "Rotom Wash" },
                new[] { "Rotom Frost" },
                new[] { "Rotom Fan" },
                new[] { "Rotom Mow" }
            }},

            // Gen 5
            { "Snivy", new List<string[]> { new[] { "Snivy", "Servine", "Serperior" } } },
            { "Tepig", new List<string[]> { new[] { "Tepig", "Pignite", "Emboar" } } },
            { "Oshawott", new List<string[]> { new[] { "Oshawott", "Dewott", "Samurott" } } },
            { "Patrat", new List<string[]> { new[] { "Patrat", "Watchog" } } },
            { "Lillipup", new List<string[]> { new[] { "Lillipup", "Herdier", "Stoutland" } } },
            { "Purrloin", new List<string[]> { new[] { "Purrloin", "Liepard" } } },
            { "Pansage", new List<string[]> { new[] { "Pansage", "Simisage" } } },
            { "Pansear", new List<string[]> { new[] { "Pansear", "Simisear" } } },
            { "Panpour", new List<string[]> { new[] { "Panpour", "Simipour" } } },
            { "Munna", new List<string[]> { new[] { "Munna", "Musharna" } } },
            { "Pidove", new List<string[]> { new[] { "Pidove", "Tranquill", "Unfezant" } } },
            { "Blitzle", new List<string[]> { new[] { "Blitzle", "Zebstrika" } } },
            { "Roggenrola", new List<string[]> { new[] { "Roggenrola", "Boldore", "Gigalith" } } },
            { "Woobat", new List<string[]> { new[] { "Woobat", "Swoobat" } } },
            { "Drilbur", new List<string[]> { new[] { "Drilbur", "Excadrill" } } },
            { "Audino", new List<string[]> { new[] { "Audino" } } },

            // Standalone / Legendary Pokémon
            { "Victini", new List<string[]> { new[] { "Victini" } } },
            { "Cobalion", new List<string[]> { new[] { "Cobalion" } } },
            { "Terrakion", new List<string[]> { new[] { "Terrakion" } } },
            { "Virizion", new List<string[]> { new[] { "Virizion" } } },
            { "Tornadus", new List<string[]> { new[] { "Tornadus" } } },
            { "Thundurus", new List<string[]> { new[] { "Thundurus" } } },
            { "Reshiram", new List<string[]> { new[] { "Reshiram" } } },
            { "Zekrom", new List<string[]> { new[] { "Zekrom" } } },
            { "Landorus", new List<string[]> { new[] { "Landorus" } } },
            { "Kyurem", new List<string[]> { new[] { "Kyurem" } } },
            { "Keldeo", new List<string[]> { new[] { "Keldeo" } } },
            { "Meloetta", new List<string[]> { new[] { "Meloetta" } } },
            { "Genesect", new List<string[]> { new[] { "Genesect" } } },

            // Gen 6
            { "Chespin", new List<string[]> { new[] { "Chespin", "Quilladin", "Chesnaught" } } },
            { "Fennekin", new List<string[]> { new[] { "Fennekin", "Braixen", "Delphox" } } },
            { "Froakie", new List<string[]> { new[] { "Froakie", "Frogadier", "Greninja" } } },
            { "Bunnelby", new List<string[]> { new[] { "Bunnelby", "Diggersby" } } },
            { "Fletchling", new List<string[]> { new[] { "Fletchling", "Fletchinder", "Talonflame" } } },
            { "Scatterbug", new List<string[]> { new[] { "Scatterbug", "Spewpa", "Vivillon" } } },
            { "Litleo", new List<string[]> { new[] { "Litleo", "Pyroar" } } },
            { "Flabébé", new List<string[]> { new[] { "Flabébé", "Floette", "Florges" } } },
            { "Skiddo", new List<string[]> { new[] { "Skiddo", "Gogoat" } } },
            { "Pancham", new List<string[]> { new[] { "Pancham", "Pangoro" } } },

            // Standalone / Legendary Pokémon
            { "Xerneas", new List<string[]> { new[] { "Xerneas" } } },
            { "Yveltal", new List<string[]> { new[] { "Yveltal" } } },
            { "Zygarde", new List<string[]> { new[] { "Zygarde" } } },
            { "Diancie", new List<string[]> { new[] { "Diancie" } } },
            { "Hoopa", new List<string[]> { new[] { "Hoopa" } } },
            { "Volcanion", new List<string[]> { new[] { "Volcanion" } } },

            // Gen 7
            { "Rowlet", new List<string[]> { new[] { "Rowlet", "Dartrix", "Decidueye" } } },
            { "Litten", new List<string[]> { new[] { "Litten", "Torracat", "Incineroar" } } },
            { "Popplio", new List<string[]> { new[] { "Popplio", "Brionne", "Primarina" } } },
            { "Pikipek", new List<string[]> { new[] { "Pikipek", "Trumbeak", "Toucannon" } } },
            { "Yungoos", new List<string[]> { new[] { "Yungoos", "Gumshoos" } } },
            { "Grubbin", new List<string[]> { new[] { "Grubbin", "Charjabug", "Vikavolt" } } },
            { "Crabrawler", new List<string[]> { new[] { "Crabrawler", "Crabominable" } } },
            { "Oricorio", new List<string[]> {
                new[] { "Oricorio Baile" }, new[] { "Oricorio Pom-Pom" },
                new[] { "Oricorio Pa'u" }, new[] { "Oricorio Sensu" }
            }},
            { "Cutiefly", new List<string[]> { new[] { "Cutiefly", "Ribombee" } } },
            { "Rockruff", new List<string[]> { new[] { "Rockruff", "Lycanroc Midday" }, new[] { "Rockruff", "Lycanroc Midnight" }, new[] { "Rockruff", "Lycanroc Dusk" } } },
            { "Wishiwashi", new List<string[]> { new[] { "Wishiwashi Solo" }, new[] { "Wishiwashi School" } } },
            { "Mareanie", new List<string[]> { new[] { "Mareanie", "Toxapex" } } },
            { "Mudbray", new List<string[]> { new[] { "Mudbray", "Mudsdale" } } },
            { "Dewpider", new List<string[]> { new[] { "Dewpider", "Araquanid" } } },
            { "Fomantis", new List<string[]> { new[] { "Fomantis", "Lurantis" } } },
            { "Morelull", new List<string[]> { new[] { "Morelull", "Shiinotic" } } },
            { "Comfey", new List<string[]> { new[] { "Comfey" } } },

            // Legendary / Standalone Pokémon
            { "Tapu Koko", new List<string[]> { new[] { "Tapu Koko" } } },
            { "Tapu Lele", new List<string[]> { new[] { "Tapu Lele" } } },
            { "Tapu Bulu", new List<string[]> { new[] { "Tapu Bulu" } } },
            { "Tapu Fini", new List<string[]> { new[] { "Tapu Fini" } } },
            { "Cosmog", new List<string[]> { new[] { "Cosmog", "Cosmoem", "Solgaleo" }, new[] { "Cosmog", "Cosmoem", "Lunala" } } },
            { "Nihilego", new List<string[]> { new[] { "Nihilego" } } },
            { "Buzzwole", new List<string[]> { new[] { "Buzzwole" } } },
            { "Pheromosa", new List<string[]> { new[] { "Pheromosa" } } },
            { "Xurkitree", new List<string[]> { new[] { "Xurkitree" } } },
            { "Celesteela", new List<string[]> { new[] { "Celesteela" } } },
            { "Kartana", new List<string[]> { new[] { "Kartana" } } },
            { "Guzzlord", new List<string[]> { new[] { "Guzzlord" } } },
            { "Necrozma", new List<string[]> { new[] { "Necrozma" } } },
            { "Magearna", new List<string[]> { new[] { "Magearna" } } },
            { "Marshadow", new List<string[]> { new[] { "Marshadow" } } },

            // Gen 8
            { "Grookey", new List<string[]> { new[] { "Grookey", "Thwackey", "Rillaboom" } } },
            { "Scorbunny", new List<string[]> { new[] { "Scorbunny", "Raboot", "Cinderace" } } },
            { "Sobble", new List<string[]> { new[] { "Sobble", "Drizzile", "Inteleon" } } },
            { "Rookidee", new List<string[]> { new[] { "Rookidee", "Corvisquire", "Corviknight" } } },
            { "Nickit", new List<string[]> { new[] { "Nickit", "Thievul" } } },
            { "Gossifleur", new List<string[]> { new[] { "Gossifleur", "Eldegoss" } } },
            { "Wooloo", new List<string[]> { new[] { "Wooloo", "Dubwool" } } },
            { "Chewtle", new List<string[]> { new[] { "Chewtle", "Drednaw" } } },
            { "Yamper", new List<string[]> { new[] { "Yamper", "Boltund" } } },
            { "Rolycoly", new List<string[]> { new[] { "Rolycoly", "Carkol", "Coalossal" } } },
            { "Applin", new List<string[]> { new[] { "Applin", "Flapple" }, new[] { "Applin", "Appletun" } } },
            { "Silicobra", new List<string[]> { new[] { "Silicobra", "Sandaconda" } } },
            { "Indeedee", new List<string[]> { new[] { "Indeedee Male" }, new[] { "Indeedee Female" } } },

            // Legendary / Standalone Pokémon
            { "Zacian", new List<string[]> { new[] { "Zacian" } } },
            { "Zamazenta", new List<string[]> { new[] { "Zamazenta" } } },
            { "Eternatus", new List<string[]> { new[] { "Eternatus" } } },
            { "Kubfu", new List<string[]> { new[] { "Kubfu", "Urshifu Single Strike", "Urshifu Rapid Strike" } } },
            { "Zarude", new List<string[]> { new[] { "Zarude" } } },

            // Gen 9
            { "Sprigatito", new List<string[]> { new[] { "Sprigatito", "Floragato", "Meowscarada" } } },
            { "Fuecoco", new List<string[]> { new[] { "Fuecoco", "Crocalor", "Skeledirge" } } },
            { "Quaxly", new List<string[]> { new[] { "Quaxly", "Quaxwell", "Quaquaval" } } },
            { "Lechonk", new List<string[]> { new[] { "Lechonk", "Oinkologne" } } },
            { "Smoliv", new List<string[]> { new[] { "Smoliv", "Dolliv", "Arboliva" } } },
            { "Pawmi", new List<string[]> { new[] { "Pawmi", "Pawmo", "Pawmot" } } },
            { "Fidough", new List<string[]> { new[] { "Fidough", "Dachsbun" } } },
            { "Tandemaus", new List<string[]> { new[] { "Tandemaus", "Maushold" } } },
            { "Cetitan", new List<string[]> { new[] { "Cetitan" } } },
            { "Veluza", new List<string[]> { new[] { "Veluza" } } },
            { "Arboliva", new List<string[]> { new[] { "Arboliva" } } },
            { "Finizen", new List<string[]> { new[] { "Finizen", "Palafin" } } },
            { "Dudunsparce", new List<string[]> { new[] { "Dudunsparce" } } },
            { "Kingambit", new List<string[]> { new[] { "Kingambit" } } },
            { "Iron Treads", new List<string[]> { new[] { "Iron Treads" } } },
            { "Iron Valiant", new List<string[]> { new[] { "Iron Valiant" } } },
            { "Iron Bundle", new List<string[]> { new[] { "Iron Bundle" } } },
            { "Iron Hands", new List<string[]> { new[] { "Iron Hands" } } },
            { "Iron Jugulis", new List<string[]> { new[] { "Iron Jugulis" } } },
            { "Iron Moth", new List<string[]> { new[] { "Iron Moth" } } },
            { "Iron Thorns", new List<string[]> { new[] { "Iron Thorns" } } },

            // Legendary / Standalone Pokémon
            { "Koraidon", new List<string[]> { new[] { "Koraidon" } } },
            { "Miraidon", new List<string[]> { new[] { "Miraidon" } } },
            { "Wo-Chien", new List<string[]> { new[] { "Wo-Chien" } } },
            { "Chien-Pao", new List<string[]> { new[] { "Chien-Pao" } } },
            { "Ting-Lu", new List<string[]> { new[] { "Ting-Lu" } } },
            { "Chi-Yu", new List<string[]> { new[] { "Chi-Yu" } } },

            //Babies
            // Gen 2 Baby Pokémon
            { "Pichu", new List<string[]> { new[] { "Pichu", "Pikachu", "Raichu" } } },
            { "Cleffa", new List<string[]> { new[] { "Cleffa", "Clefairy", "Clefable" } } },
            { "Igglybuff", new List<string[]> { new[] { "Igglybuff", "Jigglypuff", "Wigglytuff" } } },
            { "Tyrogue", new List<string[]> {
                new[] { "Tyrogue", "Hitmonlee" },
                new[] { "Tyrogue", "Hitmonchan" },
                new[] { "Tyrogue", "Hitmontop" }
            } },

            // Gen 3 Baby Pokémon
            { "Azurill", new List<string[]> { new[] { "Azurill", "Marill", "Azumarill" } } },
            { "Wynaut", new List<string[]> { new[] { "Wynaut", "Wobbuffet" } } },

            // Gen 5 Baby Pokémon
            { "Munchlax", new List<string[]> { new[] { "Munchlax", "Snorlax" } } }
        };
    }
}
