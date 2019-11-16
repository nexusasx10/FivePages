using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace FivePages
{
    public class Entity
    {
        public string Name;
        
        public Vector Size { get; set; }

        public bool Permeability;

        public static Vector StandardSize = new Vector(40, 60);

        protected Dictionary<string, Tuple<List<Image>, double>> Textures = new Dictionary<string, Tuple<List<Image>, double>>();

        public Image TextureGetter(string name, double period = 0.1)
        {
            if (Textures[name].Item2 < Textures[name].Item1.Count - 1)
                Textures[name] = Tuple.Create(Textures[name].Item1, Textures[name].Item2 + period);
            else
                Textures[name] = Tuple.Create(Textures[name].Item1, 0.0);
            return Textures[name].Item1[Convert.ToInt32(Textures[name].Item2)];
        }

        public virtual Image GetTexture(GameForm form)
        {
            return Image.FromFile("images/standard.png");
        }

        public virtual void TryDestroy(GameForm form)
        {

        }
    }

    public class MovableEntity : Entity
    {
        public enum Direction
        {
            Left,
            Right,
            Up,
            Down
        }
        public Direction direction;

        public Vector Position { get; set; }
        public Vector Speed { get; set; }

        public bool IsDead;

        public virtual void Update(GameForm form)
        {
            form.Invalidate();
        }

        public int Column { get { return Convert.ToInt32(Position.X / StandardSize.X); } }
        public int Row { get { return Convert.ToInt32(Position.Y / StandardSize.Y); } }

        public Vector Spacing { get { return new Vector(Column * StandardSize.X, Row * StandardSize.Y) - Position; } }
    }

    public class Player : MovableEntity
    {
        public HashSet<Action> actions = new HashSet<Action>();
        
        public enum Action
        {
            Stand,
            Jump,
            Run,
            Climb,
            Shot
        }

        public Player(string name, Vector position)
        {
            Name = name;
            Position = position;
            Size = StandardSize;
            Speed = Vector.Zero;
            Textures["standing"] = Tuple.Create(new List<Image> {
                Image.FromFile("images/PlayerStanding.png")
            }, 0.0);
            Textures["running"] = Tuple.Create(new List<Image> {
                Image.FromFile("images/PlayerRunning1.png"),
                Image.FromFile("images/PlayerRunning2.png"),
                Image.FromFile("images/PlayerRunning3.png"),
                Image.FromFile("images/PlayerRunning4.png")
            }, 0.0);
            Textures["climbing"] = Tuple.Create(new List<Image> {
                Image.FromFile("images/PlayerClimbing1.png"),
                Image.FromFile("images/PlayerClimbing2.png")
            }, 0.0);
            Textures["jumping"] = Tuple.Create(new List<Image> {
                Image.FromFile("images/PlayerJumping.png")
            }, 0.0);
            actions.Add(Action.Stand);
        }

        public void Destroy()
        {
            IsDead = true;
        }

        public override Image GetTexture(GameForm form)
        {
            Image texture;
            if (actions.Contains(Action.Jump))
                texture = TextureGetter("jumping");
            else if (actions.Contains(Action.Run))
                texture = TextureGetter("running", 0.2);
            else if (actions.Contains(Action.Climb))
            {
                if (direction == Direction.Up || direction == Direction.Down)
                    texture = TextureGetter("climbing", 0.05);
                else
                    texture = TextureGetter("climbing", 0.0001);
            }
            else
                texture = TextureGetter("standing");
            if (direction == Direction.Left)
            {
                texture = (Image)texture.Clone();
                texture.RotateFlip(RotateFlipType.RotateNoneFlipX);
            }
            return texture;
        }

        public override void Update(GameForm form)
        {  
            if (Speed.Length > 0)
                Speed -= Speed.Normalize() * 1.5;

            if (Speed.Length > 8)
                Speed = Speed.Normalize() * 8;
            if (form.GameSpace.Map[Column, Row].Block is Ladder)
            {
                if (Spacing.Length < 10)
                        Speed = Vector.Zero;
                if (actions.Contains(Action.Climb))
                {
                    
                    if (direction == Direction.Up)
                        if (Speed.Y > -4)
                            Speed += new Vector(0, -4);
                    if (direction == Direction.Down)
                        if (Speed.Y < 4)
                            Speed += new Vector(0, 4);
                    if (direction == Direction.Left)
                        if (Speed.X > -4)
                            Speed += new Vector(-4, 0);
                    if (direction == Direction.Right)
                        if (Speed.X < 4)
                            Speed += new Vector(4, 0);
                }
            }
            else
            {
                Speed += new Vector(0, 3);
                if (actions.Contains(Action.Run))
                {
                    if (direction == Direction.Left)
                        if (Speed.X > -4)
                            Speed += new Vector(-4, 0);
                    if (direction == Direction.Right)
                        if (Speed.X < 4)
                            Speed += new Vector(4, 0);
                }
                if (actions.Contains(Action.Jump))
                {
                    if (form.GameSpace.Map[Column, Row].Bottom != null)
                        if (!form.GameSpace.Map[Column, Row].Bottom.IsPermeable && Spacing.Y < 3)
                        {
                            Jump();                            
                        }
                }
            }
            Move(form);
            if (Row == 11)
                Destroy();
            base.Update(form);
        }

        public void Jump()
        {
            Speed += Speed;
            Speed -= new Vector(0, 80);
        }

        public void Move(GameForm form)
        {
            if (Speed.X > 0)
            {
                if (form.GameSpace.Map[Column, Row].Right != null)
                    if (form.GameSpace.Map[Column, Row].Right.IsPermeable || Spacing.X > 0)
                    {
                        Position += new Vector(Speed.X, 0);
                    }
            }

            if (Speed.X < 0)
            {
                if (form.GameSpace.Map[Column, Row].Left != null)
                    if (form.GameSpace.Map[Column, Row].Left.IsPermeable || Spacing.X < 0)
                    {
                        Position += new Vector(Speed.X, 0);
                    }
            }

            if (Speed.Y > 0)
            {
                if (form.GameSpace.Map[Column, Row].Bottom != null)
                    if (form.GameSpace.Map[Column, Row].Bottom.IsPermeable || Spacing.Y > 0)
                    {
                        Position += new Vector(0, Speed.Y);
                    }
            }

            if (Speed.Y < 0)
            {
                if (form.GameSpace.Map[Column, Row].Top != null)
                    if (form.GameSpace.Map[Column, Row].Top.IsPermeable || Spacing.Y < 0)
                    {
                        Position += new Vector(0, Speed.Y);
                    }
            }
        }
    }

    public class Enemy : MovableEntity
    {

        public EnemyType Type;

        public enum EnemyType
        {
            Zombie,
            Sceleton
        }

        public override void TryDestroy(GameForm form)
        {
            var player = (Player)form.GameSpace.Map["Player"];
            if (player.Column == Column && player.Row == Row)
            {
                if (!IsDead)
                    player.Destroy();
            }

            if (player.Column == Column + 1 && player.Row == Row && player.direction == Direction.Left && player.actions.Contains(Player.Action.Shot))
            {
                IsDead = true;
            }

            if (player.Column == Column - 1 && player.Row == Row && player.direction == Direction.Right && player.actions.Contains(Player.Action.Shot))
            {
                IsDead = true;
            }

            if (player.Column == Column && player.Row == Row - 1 && player.Spacing.Length < 20)
            {
                IsDead = true;
                player.Jump();
            }
        }

        public Enemy(string name, Vector position, EnemyType type)
        {
            Name = name;
            Position = position;
            Size = StandardSize;
            Type = type;
            direction = Direction.Left;
            IsDead = false;
            Textures["zombie"] = Tuple.Create(new List<Image> {
                Image.FromFile("images/1/Zombie1.png"),
                Image.FromFile("images/1/Zombie2.png")
            }, 0.0);
            Textures["deadZombie"] = Tuple.Create(new List<Image> {
                Image.FromFile("images/1/DeadZombie.png")
            }, 0.0);
            Textures["sceleton"] = Tuple.Create(new List<Image> {
                Image.FromFile("images/2/Sceleton1.png"),
                Image.FromFile("images/2/Sceleton2.png"),
                Image.FromFile("images/2/Sceleton3.png")
            }, 0.0);
            Textures["deadSceleton"] = Tuple.Create(new List<Image> {
                Image.FromFile("images/2/DeadSceleton.png")
            }, 0.0);
        }

        public override Image GetTexture(GameForm form)
        {
            Image texture;
            if (Type == EnemyType.Zombie)
            {
                if (IsDead)
                    texture = TextureGetter("deadZombie");
                else
                    texture = TextureGetter("zombie", 0.05);
            }
            else
            {
                if (IsDead)
                    texture = TextureGetter("deadSceleton");
                else
                    texture = TextureGetter("sceleton", 0.15);
            }
            if (direction == Direction.Right)
            {
                texture = (Image)texture.Clone();
                texture.RotateFlip(RotateFlipType.RotateNoneFlipX);
            }
            return texture;
        }

        public override void Update(GameForm form)
        {
            if (!IsDead)
            {
                if (direction == Direction.Left)
                {
                    if (form.GameSpace.Map[Column, Row].Left != null)
                    {
                        if ((!form.GameSpace.Map[Column, Row].Left.IsPermeable) ||
                            ((form.GameSpace.Map[Column, Row].Left.Bottom != null) &&
                            (form.GameSpace.Map[Column, Row].Left.Bottom.IsPermeable)))
                                if (Spacing.Length < 5)
                                    direction = Direction.Right;
                    }
                    Position += new Vector(-1, 0);
                }
                else
                {
                    if (form.GameSpace.Map[Column, Row].Right != null)
                    {
                        if ((!form.GameSpace.Map[Column, Row].Right.IsPermeable) ||
                            ((form.GameSpace.Map[Column, Row].Right.Bottom != null) &&
                            (form.GameSpace.Map[Column, Row].Right.Bottom.IsPermeable)))
                                if (Spacing.Length < 5)
                                    direction = Direction.Left;
                    }
                    Position += new Vector(1, 0);
                }
            }
            base.Update(form);
        }
    }

    public class Doorway : Entity
    {
        public DoorwayType Type;

        public enum DoorwayType
        {
            Enter,
            Exit
        }

        public Doorway(string name, DoorwayType type)
        {
            Permeability = true;
            Name = name;
            Size = StandardSize;
            Type = type;
            Textures["enter"] = Tuple.Create(new List<Image> {
                Image.FromFile("images/Enter1.png"),
                Image.FromFile("images/Enter2.png")
            }, 0.0);
            Textures["exit"] = Tuple.Create(new List<Image> {
                Image.FromFile("images/Exit1.png"),
                Image.FromFile("images/Exit2.png")
            }, 0.0);
        }

        public override Image GetTexture(GameForm form)
        {
            if (Type == DoorwayType.Enter)
                return TextureGetter("enter", 0.01);
            else
                return TextureGetter("exit", 0.01);
        }
    }

    public class Gold : Entity
    {
        public GoldType Type;
        public enum GoldType
        {
            Usual,
            Secret
        }

        public Gold(string name, GoldType type)
        {
            Name = name;
            Type = type;
            Size = StandardSize;
            Textures["usual"] = Tuple.Create(new List<Image> {
                Image.FromFile("images/Gold.png")
            }, 0.0);
            Textures["secret"] = Tuple.Create(new List<Image> {
                Image.FromFile("images/Secret.png")
            }, 0.0);
        }

        public override void TryDestroy(GameForm form)
        {
            var player = (Player)form.GameSpace.Map["Player"];
            if (player.Column == form.GameSpace.Map[Name, true].Column && player.Row == form.GameSpace.Map[Name, true].Row)
            {
                form.GameSpace.Map[player.Column, player.Row].Gold = null;
                form.Scores++;
            }
        }

        public override Image GetTexture(GameForm form)
        {
            if (Type == GoldType.Usual)
                return TextureGetter("usual");
            else
                return TextureGetter("secret");
        }
    }

    public class Ladder : Entity
    {
        public LadderType Type;

        public enum LadderType
        {
            Metal,
            Wooden,
            Ice
        }

        public Ladder(string name, LadderType type)
        {
            Permeability = true;
            Size = StandardSize;
            Name = name;
            Type = type;

            Textures["metal"] = Tuple.Create(new List<Image> {
                Image.FromFile("images/1/MetalLadder.png")
            }, 0.0);
            Textures["wooden"] = Tuple.Create(new List<Image> {
                Image.FromFile("images/2/WoodenLadder.png")
            }, 0.0);
            Textures["ice"] = Tuple.Create(new List<Image> {
                Image.FromFile("images/3/IceLadder.png")
            }, 0.0);
        }

        public override Image GetTexture(GameForm form)
        {
            if (Type == LadderType.Metal)
                return TextureGetter("metal");
            else if (Type == LadderType.Wooden)
                return TextureGetter("wooden");
            else
                return TextureGetter("ice");
        }
    }

    public class Box : Entity
    {
        public BoxType Type;

        public enum BoxType
        {
            Wooden,
            Carton,
            Ice
        }

        public Box(string name, BoxType type)
        {
            Permeability = false;
            Size = StandardSize;
            Name = name;
            Type = type;
            Textures["wooden"] = Tuple.Create(new List<Image> {
                Image.FromFile("images/1/WoodenBox.png")
            }, 0.0);
            Textures["carton"] = Tuple.Create(new List<Image> {
                Image.FromFile("images/2/CartonBox.png")
            }, 0.0);
            Textures["ice"] = Tuple.Create(new List<Image> {
                Image.FromFile("images/3/Ice.png")
            }, 0.0);
        }

        public override void TryDestroy(GameForm form)
        {
            var player = (Player)form.GameSpace.Map["Player"];

            if (player.Column == form.GameSpace.Map[Name, true].Column + 1 && player.Row == form.GameSpace.Map[Name, true].Row)
            {
                if (player.direction == Player.Direction.Left && player.actions.Contains(Player.Action.Shot))
                {
                    form.GameSpace.Map[player.Column - 1, player.Row].Block = null;
                    return;
                }
            }

            if (player.Column == form.GameSpace.Map[Name, true].Column - 1 && player.Row == form.GameSpace.Map[Name, true].Row)
            {
                if (player.direction == Player.Direction.Right && player.actions.Contains(Player.Action.Shot))
                {
                    form.GameSpace.Map[player.Column + 1, player.Row].Block = null;
                    return;
                }
            }
        }

        public override Image GetTexture(GameForm form)
        {
            if (Type == BoxType.Wooden)
                return TextureGetter("wooden");
            else if (Type == BoxType.Carton)
                return TextureGetter("carton");
            else
                return TextureGetter("ice");
        }
    }

    public class Wall : Entity
    {
        public WallType Type;

        public enum WallType
        {
            Brick,
            Sand,
            Ice
        }

        public Wall(string name, WallType type)
        {
            Permeability = false;
            Name = name;
            Size = StandardSize;
            Type = type;
            Textures["brick"] = Tuple.Create(new List<Image> {
                Image.FromFile("images/1/BrickWall.png")
            }, 0.0);
        }

        public override Image GetTexture(GameForm form)
        {
            if (Type == WallType.Brick)
                return TextureGetter("brick");
            else
                return base.GetTexture(form);
        }
    }
}
