using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;


public class ModelProcessor
{
    private AdjacencyRules adjRules;
    private Dictionary<int, int> newTileToInput;
    private Dictionary<int, int> inputToNewTile;

    /*
     * every tile-sized square of pixels in the output image appears somewhere in the input image
    the relative frequencies of tile-sized squares of pixels in the output image is roughly the same as in the input image
     */
    public List<Tile> PreprocessModel(KeyValuePair<int, int>[,,] input, List<Tile> tileset, Vector3Int inputSize)
    {
        if (input == null || input.Length <= 0)
        {
            adjRules = new AdjacencyRules(tileset.Count);

            CreateAdjacencyRules(tileset);
            return tileset;
        }
        newTileToInput = new Dictionary<int, int>();
        inputToNewTile = new Dictionary<int, int>();
        Dictionary<KeyValuePair<int,int>, int> frequencyCount = new Dictionary<KeyValuePair<int,int>, int>();
        HashSet<int> exist = new HashSet<int>();
        
        //create tiles
        var tiles = new List<Tile>();
        
        for (int x = 0; x < inputSize.x; x++)
        {
            for (int y = 0; y < inputSize.y; y++)
            {
                for (int z = 0; z < inputSize.y; z++)
                {
                    var inputTile = input[x, y, z];
                    if (inputTile.Key == -1 || inputTile.Value == -1) continue;
                    
                    if(frequencyCount.ContainsKey(inputTile))
                    {
                        frequencyCount[inputTile]++;
                    } else
                    {
                        frequencyCount.Add(inputTile, 1);
                    }
                }
            }
        }
        
        foreach (var newTiles in frequencyCount)
        {
            var inputT = newTiles.Key.Key;
            if (!exist.Contains(inputT))
            {
                exist.Add(inputT);
                inputToNewTile[inputT] = tiles.Count;
                for (int i = 0; i < 4; i++)
                {
                    Tile newTile = ScriptableObject.CreateInstance<Tile>();
                    newTile.index = tiles.Count;
                    newTileToInput[newTile.index] = inputT;
                    newTile.tileObj = tileset[inputT].tileObj;
                    newTile.direction = i;

                    if (newTiles.Key.Value == i)
                        newTile.freq = newTiles.Value;
                    else 
                        newTile.freq = 1;
                    
                    tiles.Add(newTile);
                }
                
            }
            else
            {
                Tile newTile = tiles[inputToNewTile[inputT] + newTiles.Value];
                newTile.freq = newTiles.Value;
            }
        }
        adjRules = new AdjacencyRules(tiles.Count);
        CreateAdjacencyRules(tiles, input, inputSize);

        return tiles;
    }
    
    void CreateAdjacencyRules(List<Tile> tiles, KeyValuePair<int, int>[,,] input, Vector3Int inputSize)
    {
        foreach (var a in tiles)
        {
            foreach (var b in tiles)
            {
                foreach (var direction in (AdjacencyRules.Direction[]) Enum.GetValues(typeof(AdjacencyRules.Direction)))
                {
                    if (IsCompatible(a, b, direction, input, inputSize))
                    {
                        adjRules.Allow(a, b, direction);
                    }
				    
                }
            }
        }
    }
    void CreateAdjacencyRules(List<Tile> tiles)
    {
        foreach (var a in tiles)
        {
            foreach (var b in tiles)
            {
                foreach (var direction in (AdjacencyRules.Direction[]) Enum.GetValues(typeof(AdjacencyRules.Direction)))
                {
                    if (IsCompatible(a, b, direction))
                    {
                        adjRules.Allow(a, b, direction);
                    }
				    
                }
            }
        }
    }

    private bool IsCompatible(Tile tile, Tile tile1, AdjacencyRules.Direction direction)
    {
        return Random.value < 0.5f;
    }
    private bool IsCompatible(Tile tile, Tile tile1, AdjacencyRules.Direction direction,
        KeyValuePair<int, int>[,,] input, Vector3Int inputSize)
    {
        int inputT1 = newTileToInput[tile.index];
        int inputT2 = newTileToInput[tile1.index];
        // z is up in input
        for (int x = 0; x < inputSize.x; x++)
        {
            for (int y = 0; y < inputSize.y; y++)
            {
                for (int z = 0; z < inputSize.y; z++)
                {
                    var inputTile = input[x, y, z];
                    if (inputTile.Key == -1 || inputTile.Value == -1) continue;

                    if (inputTile.Key == inputT1)
                    {
                        var direc = GetDirectionToLook(direction, inputTile.Value);
                        var newPos = new Vector3Int(x,y,z) + direc;

                        if (OOB(newPos.x, newPos.y, newPos.z, inputSize)) continue;
                        if (input[newPos.x, newPos.y, newPos.z].Key == inputT2)
                        {
                            return true;
                        }
                    }
                }
            }
        }

        return false;
    }
    private bool OOB(int x, int y, int z, Vector3Int gridSize)
    {
        return x < 0 || x >= gridSize.x || y < 0 || y >= gridSize.z || z < 0 || z >= gridSize.y;
    }

    private Vector3Int GetDirectionToLook(AdjacencyRules.Direction direction, int inputTileValue)
    {
        Vector3Int direc;
        switch (direction) {
            case AdjacencyRules.Direction.Left:
                direc=new Vector3Int(-1,0,0);
                break;
            case AdjacencyRules.Direction.Right:
                direc= new Vector3Int(1,0,0);

                break;
            case AdjacencyRules.Direction.Up:
                direc= new Vector3Int(0,1,0);

                break;
            case AdjacencyRules.Direction.Down:
                direc= new Vector3Int(0,-1,0);

                break;
            case AdjacencyRules.Direction.Forward:
                direc= new Vector3Int(0,0,1);

                break;
            case AdjacencyRules.Direction.Backward:
                direc= new Vector3Int(0,0,-1);
                break;
            default:
                direc= new Vector3Int(1,0,0);
                break;

        }

        for (int i = 0; i < inputTileValue; i++)
        {
            direc =  new Vector3Int(-direc.y, direc.x, direc.z);
        }

        return direc;
    }

    public AdjacencyRules GetAdjacencyRule()
    {
        return adjRules;
    }
    
    
}
