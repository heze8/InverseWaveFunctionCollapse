
    using System.Collections;
    using System.Collections.Generic;

    public class AdjacencyRules
    {
        public enum Direction
        {
            Left = 0,
            Up = 1,
            Right = 2,
            Down = 3,
            Forward = 4,
            Backward = 5
        }

        public static Direction GetOppDirection(Direction direc)
        {
            switch (direc) {
                case AdjacencyRules.Direction.Left:
                    return Direction.Right;
                break;
                case AdjacencyRules.Direction.Right:
                    return Direction.Left;

                break;
                case AdjacencyRules.Direction.Up:
                    return Direction.Down;

                break;
                case AdjacencyRules.Direction.Down:
                    return Direction.Up;

                break;
                case AdjacencyRules.Direction.Forward:
                    return Direction.Backward;

                break;
                case AdjacencyRules.Direction.Backward:
                    return Direction.Forward;
                default:
                    return Direction.Backward;

            }
        }

        public struct AllowedTiles
        {
            public List<Tile>[] tiles;
        }

        public AllowedTiles[] allowedTiles;
        //what tiles can appear next to each other

        public AdjacencyRules(int numTiles)
        {
            allowedTiles = new AllowedTiles[numTiles];
            for (int i = 0; i < numTiles; i++)
            {
                var allowedTile = new AllowedTiles();
                allowedTile.tiles = new List<Tile>[6];
                for (int j = 0; j < 6; j++)
                {
                    allowedTile.tiles[j] = new List<Tile>();
                }

                allowedTiles[i] = allowedTile;
            }
        }
        
        public void Allow(Tile tile, Tile tile1, Direction direction)
        {
            allowedTiles[tile.index].tiles[(int) direction].Add(tile1);
            
            allowedTiles[tile1.index].tiles[(int) GetOppDirection(direction)].Add(tile);

        }

        public List<Tile> CompatibleTiles(Tile tile, Direction direction)
        {
            return allowedTiles[tile.index].tiles[(int) direction];
        }
    }
