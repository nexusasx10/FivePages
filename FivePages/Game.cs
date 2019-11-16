using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FivePages
{
    public class Game
    {
        public readonly string Name;
        public readonly Vector Size;
        public Map Map;
        public Brush Background;

        public Game(string name, int width, int height)
        {
            Name = name;
            Map = new Map(width, height);
            Size = new Vector(width * Entity.StandardSize.X, height * Entity.StandardSize.Y);
        }
    }

    public class Level1 : Game
    {
        public Level1(string name, int width, int height) : base(name, width, height)
        {
            Background = Brushes.LightSkyBlue;
            var levelFile = File.ReadLines("levels/level1.txt");
            var str = 0;
            var goldNumber = 1;
            var boxNumber = 1;
            foreach (var line in levelFile)
            {
                var sym = 0;                
                foreach (var block in line)
                {
                    switch (block)
                    {
                        case 'w':
                            Map.Add(new Wall("Wall", Wall.WallType.Brick), sym, str, Cell.Layer.Block);
                            break;
                        case 's':
                            Map.Add(new Doorway("Enter", Doorway.DoorwayType.Enter), sym, str, Cell.Layer.Block);
                            break;
                        case 'o':
                            Map.Add(new Doorway("Exit", Doorway.DoorwayType.Exit), sym, str, Cell.Layer.Block);
                            break;
                        case 'l':
                            Map.Add(new Ladder("Ladder", Ladder.LadderType.Metal), sym, str, Cell.Layer.Block);
                            break;
                        case 'g':
                            Map.Add(new Gold("Gold" + goldNumber, Gold.GoldType.Usual), sym, str, Cell.Layer.Gold);
                            goldNumber++;
                            break;
                        case 'a':
                            Map.Add(new Gold("Gold", Gold.GoldType.Secret), sym, str, Cell.Layer.Gold);
                            Map.Add(new Box("Box", Box.BoxType.Wooden), sym, str, Cell.Layer.Block);
                            break;
                        case 'b':
                            Map.Add(new Box("Box" + boxNumber, Box.BoxType.Wooden), sym, str, Cell.Layer.Block);
                            boxNumber++;
                            break;
                    }
                    sym++;
                }
                str++;
            }            
            Map.Add(new Player("Player", new Vector(40, 600)));
            Map.Add(new Enemy("Zombie1", new Vector(240, 480), Enemy.EnemyType.Zombie));
            Map.Add(new Enemy("Zombie2", new Vector(240, 240), Enemy.EnemyType.Zombie));
            Map.Add(new Enemy("Zombie3", new Vector(480, 300), Enemy.EnemyType.Zombie));
            Map.Add(new Enemy("Zombie4", new Vector(760, 60), Enemy.EnemyType.Zombie));
            Map.Add(new Enemy("Zombie5", new Vector(880, 60), Enemy.EnemyType.Zombie));
            Map.Add(new Enemy("Zombie6", new Vector(760, 540), Enemy.EnemyType.Zombie));
            Map.Add(new Enemy("Zombie7", new Vector(880, 540), Enemy.EnemyType.Zombie));
        }
    }

    public class Level2 : Game
    {
        public Level2(string name, int width, int height) : base(name, width, height)
        {

        }
    }

    public class Level3 : Game
    {
        public Level3(string name, int width, int height) : base(name, width, height)
        {

        }
    }
}
