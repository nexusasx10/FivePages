using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FivePages
{
    public partial class GameForm : Form
    {
        public Game GameSpace;        
        private System.Windows.Forms.Timer MainTimer;
        public int Scores = 0;

        private ProgressBar loading;
        private Label statusBar;

        public GameForm()
        {
            
            Width = 420;
            Height = 675;
            Text = "Five Pages beta 0.1";
            FormBorderStyle = FormBorderStyle.FixedSingle;            

            GameSpace = new Level1("At the beginning", 30, 10);
            DoubleBuffered = true;

            MainTimer = new System.Windows.Forms.Timer();
            MainTimer.Interval = 20;
            MainTimer.Tick += TimerTick;
            MainTimer.Start();
            
            statusBar = new Label();
            statusBar.Location = new Point(0, 0);
            statusBar.Size = new Size(ClientSize.Width, 35);
            statusBar.BackColor = Color.Gray;
            statusBar.Font = new Font("Arial", 20);
            statusBar.ForeColor = Color.White;
            Controls.Add(statusBar);

            loading = new ProgressBar();
            loading.Location = new Point(40, 5);
            loading.Size = new Size(200, 25);
            Controls.Add(loading);
        }

        private void TimerTick(object sender, EventArgs e)
        {
            if (GameSpace == null)
                return;
            if ( loading.Value < 100)
            {
                loading.Value++;
                Thread.Sleep(20);
            }
            statusBar.Text = "Your scores: " + Scores;
            if (((Player)GameSpace.Map["Player"]).IsDead)
            {
                statusBar.Text = "You dead. Your scores: " + Scores;
                MainTimer.Stop();
            }
            if (((Player)GameSpace.Map["Player"]).Column == GameSpace.Map["Exit", true].Column &&
            ((Player)GameSpace.Map["Player"]).Row == GameSpace.Map["Exit", true].Row)
            {
                statusBar.Text = "You win! Your scores: " + Scores;
                MainTimer.Stop();
            }

            foreach(var entity in GameSpace.Map.MovableEntities.Select(x => x.Value))
                entity.Update(this);
            foreach (var entity in GameSpace.Map.MovableEntities.Select(x => x.Value))
                entity.TryDestroy(this);
            foreach (var entity in GameSpace.Map.Cells)
            {
                if (entity.Block != null)
                    entity.Block.TryDestroy(this);
                if (entity.Gold != null)
                    entity.Gold.TryDestroy(this);
            }
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            var player = ((Player)GameSpace.Map["Player"]);
            base.OnKeyDown(e);
            if (GameSpace.Map[player.Column, player.Row].Block is Ladder)
            {
                if (e.KeyCode == Keys.A)
                {
                    player.direction = MovableEntity.Direction.Left;
                    player.actions.Remove(Player.Action.Stand);
                    player.actions.Add(Player.Action.Climb);
                }
                if (e.KeyCode == Keys.D)
                {
                    player.direction = MovableEntity.Direction.Right;
                    player.actions.Remove(Player.Action.Stand);
                    player.actions.Add(Player.Action.Climb);
                }
                if (e.KeyCode == Keys.S)
                {
                    player.direction = MovableEntity.Direction.Down;
                    player.actions.Remove(Player.Action.Stand);
                    player.actions.Add(Player.Action.Climb);
                }
                if (e.KeyCode == Keys.W)
                {
                    player.direction = MovableEntity.Direction.Up;
                    player.actions.Remove(Player.Action.Stand);
                    player.actions.Add(Player.Action.Climb);
                }
            }
            else
            {
                if (e.KeyCode == Keys.A)
                {
                    player.direction = MovableEntity.Direction.Left;
                    player.actions.Remove(Player.Action.Stand);
                    player.actions.Add(Player.Action.Run);
                }
                if (e.KeyCode == Keys.D)
                {
                    player.direction = MovableEntity.Direction.Right;
                    player.actions.Remove(Player.Action.Stand);
                    player.actions.Add(Player.Action.Run);
                }
            }

            if (e.KeyCode == Keys.ShiftKey)
            {
                ((Player)GameSpace.Map["Player"]).actions.Add(Player.Action.Shot);
            }


            if (e.KeyCode == Keys.Space)
            {
                ((Player)GameSpace.Map["Player"]).actions.Remove(Player.Action.Stand);
                ((Player)GameSpace.Map["Player"]).actions.Add(Player.Action.Jump);
            }            
        }

        protected override void OnKeyUp(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            if (e.KeyCode == Keys.A || e.KeyCode == Keys.D)
            {
                ((Player)GameSpace.Map["Player"]).actions.Remove(Player.Action.Run);
                ((Player)GameSpace.Map["Player"]).actions.Remove(Player.Action.Climb);
                ((Player)GameSpace.Map["Player"]).actions.Add(Player.Action.Stand);
            }
            if (e.KeyCode == Keys.S || e.KeyCode == Keys.W)
            {
                ((Player)GameSpace.Map["Player"]).actions.Remove(Player.Action.Climb);
                ((Player)GameSpace.Map["Player"]).actions.Add(Player.Action.Stand);
            }
            if (e.KeyCode == Keys.Space)
            {
                ((Player)GameSpace.Map["Player"]).actions.Add(Player.Action.Stand);
                ((Player)GameSpace.Map["Player"]).actions.Remove(Player.Action.Jump);
            }
            if (e.KeyCode == Keys.ShiftKey)
            {
                ((Player)GameSpace.Map["Player"]).actions.Remove(Player.Action.Shot);
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var graphics = e.Graphics;
            Rectangle background = new Rectangle(0, 0, 1400, 700);
            graphics.FillRectangle(GameSpace.Background, background);
            graphics.DrawRectangle(new Pen(GameSpace.Background), background);
            double offset = ((Player)GameSpace.Map["Player"]).Position.X - 4.5 * Entity.StandardSize.X;
            if (offset < 40)
                offset = 40;
            if (offset > 835)
                offset = 835;

            //foreach (var cell in GameSpace.Map.Cells)
            //{
            //    Pen pen = new Pen(Color.FromArgb(255, 255, 0, 0));
            //    graphics.DrawLine(pen, (float)(cell.Position.X - offset), (float)(cell.Position.Y - 25), (float)(cell.Position.X + Entity.StandardSize.X - offset), (float)(cell.Position.Y - 25));
            //    graphics.DrawLine(pen, (float)(cell.Position.X - offset), (float)(cell.Position.Y - 25), (float)(cell.Position.X - offset), (float)(cell.Position.Y + Entity.StandardSize.Y - 25));
            //}

            foreach (var cell in GameSpace.Map.Cells)
            {
                if (cell.Gold != null)
                    graphics.DrawImage(cell.Gold.GetTexture(this), new PointF(
                        (float)(cell.Position.X - offset),
                        (float)(cell.Position.Y - 25)));
                if (cell.Block != null)
                    graphics.DrawImage(cell.Block.GetTexture(this), new PointF(
                        (float)(cell.Position.X - offset),
                        (float)(cell.Position.Y - 25)));
            }
            foreach (var entity in GameSpace.Map.MovableEntities.Select(x => x.Value))
            {
                graphics.DrawImage(entity.GetTexture(this), new PointF(
                    (float)(entity.Position.X - offset),
                    (float)(entity.Position.Y - 25)));
            }
        }
    }
}
