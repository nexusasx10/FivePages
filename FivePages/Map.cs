using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FivePages
{
    public class Cell
    {
        public Entity Block;
        public Entity Gold;

        public Cell(Cell[,] cells, int column, int row)
        {
            Column = column;
            Row = row;
            Cells = cells;
        }

        public enum Layer
        {
            Block,
            Gold
        }

        public readonly int Column;
        public readonly int Row;

        private Cell[,] Cells;

        public Cell Left
        {
            get
            {
                if (Column > 0)
                    return Cells[Column - 1, Row];
                else
                    return null;
            }
        }
        public Cell Right
        {
            get
            {
                if (Column < Cells.GetLength(0) - 1)
                    return Cells[Column + 1, Row];
                else
                    return null;
            }
        }
        public Cell Top
        {
            get
            {
                if (Row > 0)
                    return Cells[Column, Row - 1];
                else
                    return null;
            }
        }
        public Cell Bottom
        {
            get
            {
                if (Row < Cells.GetLength(1) - 1)
                    return Cells[Column, Row + 1];
                else
                    return null;
            }
        }

        public bool IsPermeable { get { return Block != null ? Block.Permeability : true; } }

        public Vector Position { get { return new Vector(Column * Entity.StandardSize.X, Row * Entity.StandardSize.Y); } }        
    }

    public class Map
    {
        public readonly int Width;
        public readonly int Height;
        public Cell[,] Cells;
        public Dictionary<string, MovableEntity> MovableEntities;

        public Map(int width, int height)
        {
            Width = width + 2;
            Height = height + 2;
            Cells = new Cell[Width, Height];
            for (var column = 0; column < Width; column++)
                for (var row = 0; row < Height; row++)
                    Cells[column, row] = new Cell(Cells, column, row);
            MovableEntities = new Dictionary<string, MovableEntity>();
        }

        public Cell this[int column, int row]
        {
            get
            {
                return Cells[column, row];
            }
        }

        public Entity this[string name]
        {
            get
            {
                return MovableEntities[name];
            }
        }

        public Cell this[string name, bool cap]
        {
            get
            {
                foreach (var cell in Cells)
                {
                    if (cell.Block != null)
                        if (cell.Block.Name == name)
                            return cell;
                    if (cell.Gold != null)
                        if (cell.Gold.Name == name)
                            return cell;
                }
                return null;
            }
        }

        public void Add(MovableEntity entity)
        {
            MovableEntities.Add(entity.Name, entity);
        }

        public void Add(Entity entity, int column, int row, Cell.Layer layer)
        {
            if (layer == Cell.Layer.Block)
                Cells[column, row].Block = entity;
            else
                Cells[column, row].Gold = entity;
        }
    }
}
