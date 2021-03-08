using System;
using System.Collections.Generic;
using UnityEditor.UI;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

///summary
///summary
public class WaveFunctionCollapse : MonoBehaviour
{
	#region Public Fields

    public List<Tile> tiles;
    private AdjacencyRules adjRules;
    public Cell[,,] outputTiles;
    private ModelProcessor modelProcessor;
    [SerializeField] 
    public Vector3Int tileSize;
    public Vector3Int gridSize;
    private FrequencyRules freqRules;
    private PriorityQueue<KeyValuePair<Cell, double>> entropyPq;
    private Stack<RemovalUpdate> tileRemovals;
    #endregion

    public class Cell
    {
	    public bool isCollapsed;
	    public int totalWeight;
	    public double weightedLog;
	    public Vector3Int coor;
	    public bool[] notPossible;
	    public float entropyNoise;
	    public List<TileEnabler> tileEnablers;
	    
		public struct TileEnabler
		{
			public int[] inDirection;

			public bool ContainsAnyZeroCount()
			{
				foreach (var i in inDirection)
				{
					if (i == 0)
					{
						return true;
					}
				}

				return false;
			}
		}
		
	    public void RemoveTile(int tileIndex, FrequencyRules frequencyRules)
	    {
		    notPossible[tileIndex] = true;
		    var relativeFrequency = frequencyRules.GetRelativeFrequency(tileIndex);
		    totalWeight -= relativeFrequency;
		    weightedLog -= relativeFrequency * Math.Log10(relativeFrequency);
	    }

	    public void InitProperties(FrequencyRules freqRules)
	    {
		    int sum = 0;
		    totalWeight = 0;
		    weightedLog = 0;
		    for (int i = 0; i < notPossible.Length; i++)
		    {
		     if (!notPossible[i])
		     {
		      var relativeFrequency = freqRules.GetRelativeFrequency(i);
		      sum += relativeFrequency;
		      weightedLog += relativeFrequency * Math.Log10(relativeFrequency);
		     }
		     
		    }
		    
		    totalWeight = sum;
	    }
	    
	    public double GetEntropy()
	    {
		    // int sum = 0;
		    //
		    // double weightedLog = 0;
		    // var notPos = cell.notPossible;
		    // for (int i = 0; i < tiles.Count; i++)
		    // {
		    //  if (!notPos[i])
		    //  {
		    //   var relativeFrequency = freqRules.GetRelativeFrequency(i);
		    //   sum += relativeFrequency;
		    //   weightedLog += relativeFrequency * Math.Log10(relativeFrequency);
		    //  }
		    //  
		    // }
		    //
		    // cell.totalWeight = sum;
		    // return Math.Log10(sum) - (weightedLog / (double) sum);
		    return Math.Log10(totalWeight) - weightedLog / totalWeight + entropyNoise;
	    }
		/*
		 * not implemented
		 */
	    public bool HasNoPossibleTiles()
	    {
		    return false;
	    }
    }

    public class RemovalUpdate
    {
	    public int tileIndex;
	    public Vector3Int coor;

	    public RemovalUpdate(int tileIndex, Vector3Int coor)
	    {
		    this.tileIndex = tileIndex;
		    this.coor = coor;
	    }
    }
    public class Comparer : IComparer<KeyValuePair<Cell, double>>
    {
	    // Compares by Height, Length, and Width.
	    public int Compare(KeyValuePair<Cell, double> x, KeyValuePair<Cell, double> y)
	    {
		    return x.Value.CompareTo(y.Value);
	    }
    }
    #region Unity Methods
 
    void Start()
    {
		BeginProcess();
    }
    
    #endregion

    public void Begin()
    {
	    BeginProcess();
    }
    public void ButtonPress(int i)
    {
	    Debug.Log("tile " + i);
    }
	
    #region Private Methods

    void BeginProcess()
    {
	    // tiles = modelProcessor.PreprocessModel(tileSize);
	    //
	    var ind = 0;
	    freqRules = new FrequencyRules(tiles.Count);

	    foreach (var t in tiles)
	    {
		    freqRules.tileFrequency[ind] = t.freq;

		    t.index = ind++;

	    }
	    adjRules = new AdjacencyRules(tiles.Count);
	    
	    entropyPq = new PriorityQueue<KeyValuePair<Cell, double>>(new Comparer()) ;

	    tileRemovals = new Stack<RemovalUpdate>();
	    CreateAdjacencyRules();
	    outputTiles = new Cell[gridSize.x, gridSize.y, gridSize.z];
	    for (int i = 0; i < gridSize.x; i++)
	    {
		    for (int j = 0; j < gridSize.y; j++)
		    {
			    for (int z = 0; z < gridSize.z; z++)
			    {
				    // Debug.Log("Creating cell " + i + " " + j + " " + z);
				    
				    var outputTile = new Cell();
				    outputTiles[i,j, z] = outputTile;
				    outputTile.notPossible = new bool[tiles.Count];
				    outputTile.tileEnablers = InitTileCounts(tiles.Count, adjRules);
				    outputTile.coor = new Vector3Int(i,j,z);
				    outputTile.InitProperties(freqRules);
				    outputTile.entropyNoise = Random.Range(0.0f, 0.0000001f);
				    entropyPq.Push(new KeyValuePair<Cell, double>(outputTile, outputTile.GetEntropy()));


			    }
		    }
	    }
	    CreateModel(gridSize.x * gridSize.y * gridSize.z);
    }

    private void CreateModel(int remainingUncolapsedCells)
    {
	    while (remainingUncolapsedCells > 0)
	    {
		    var nextCoor = ChooseNextCell();
		    CollapseCellAt(nextCoor);
		    Propogate();
		    remainingUncolapsedCells--;
	    }
    }
    #endregion
 
    #region Model Creation

    private List<Cell.TileEnabler> InitTileCounts(int numTiles, AdjacencyRules adjacencyRules)
    {
	    List<Cell.TileEnabler> te = new List<Cell.TileEnabler>();
	    for (int i = 0; i < numTiles; i++)
	    {
		    var counts = new Cell.TileEnabler();
		    counts.inDirection = new[] {0, 0, 0, 0, 0, 0};
		    foreach (var direction in (AdjacencyRules.Direction[]) Enum.GetValues(typeof(AdjacencyRules.Direction)))
		    {
			    foreach (var tile in adjacencyRules.CompatibleTiles(tiles[i], direction))
			    {
				    counts.inDirection[(int) direction]++;
			    }
			    te.Add(counts);
		    }
	    }

	    return te;

    }

    private void Propogate()
    {
	    while (tileRemovals.Count > 0)
	    {
		    var removalUpdate = tileRemovals.Pop();
		    foreach (var direction in (AdjacencyRules.Direction[]) Enum.GetValues(typeof(AdjacencyRules.Direction)))
		    {
			    Vector3Int neighbourCoor = removalUpdate.coor;
			    switch (direction)
			    {
				    case AdjacencyRules.Direction.Left:
					    neighbourCoor += Vector3Int.left;
					    break;
				    case AdjacencyRules.Direction.Right:
					    neighbourCoor += Vector3Int.right;

					    break;
				    case AdjacencyRules.Direction.Up:
					    neighbourCoor += Vector3Int.up;

					    break;
				    case AdjacencyRules.Direction.Down:
					    neighbourCoor += Vector3Int.down;

					    break;
				    case AdjacencyRules.Direction.Forward:
					    neighbourCoor += new Vector3Int(0,0,1);

					    break;
				    case AdjacencyRules.Direction.Backward:
					    neighbourCoor += new Vector3Int(0,0,-1);
					    break;
			    }

			    if (OOB(neighbourCoor.x, neighbourCoor.y, neighbourCoor.z))
			    {
				    continue;
			    }
				
			    Cell neighbour = outputTiles[neighbourCoor.x, neighbourCoor.y, neighbourCoor.z];
			    foreach (var tile in adjRules.CompatibleTiles(tiles[removalUpdate.tileIndex], direction))
			    {
				    AdjacencyRules.Direction opp = AdjacencyRules.GetOppDirection(direction);
				    var neighbourTileEnabler = neighbour.tileEnablers[tile.index];
				    if (neighbourTileEnabler.inDirection[(int) direction] == 1)
				    {
					    if (!neighbourTileEnabler.ContainsAnyZeroCount())
					    {
						    neighbour.RemoveTile(tile.index, freqRules);
					    }

					    if (neighbour.HasNoPossibleTiles())
					    {
						    Debug.LogError("Contradiction");
					    }
					    entropyPq.Push(new KeyValuePair<Cell, double>(neighbour, neighbour.GetEntropy()));
					    
					    tileRemovals.Push(new RemovalUpdate(tile.index, neighbourCoor));
				    }

				    neighbourTileEnabler.inDirection[(int) direction]--;
			    }
			    
		    }
	    }
    }

    private bool OOB(int x, int y, int z)
    {
	    return x < 0 || x >= gridSize.x || y < 0 || y >= gridSize.y || z < 0 || z >= gridSize.z;
    }

    private void CollapseCellAt(Cell nextCoor)
    {
	    var rand = Random.Range(0, nextCoor.totalWeight);
	    var tileIndex = -1;
	    for (int i = 0; i < tiles.Count; i++)
	    {
		    if (!nextCoor.notPossible[i])
		    {
			    var weight = freqRules.GetRelativeFrequency(i);
			    if (rand >= weight)
			    {
				    rand -= weight;
			    }
			    else
			    {
				    tileIndex = i;
				    break;
			    }
		    }
	    }

	    nextCoor.isCollapsed = true;
	    //create model
	    Tile tile = tiles[tileIndex];
	    var coor = nextCoor.coor;
	    Vector3 pos = new Vector3(coor.x * tileSize.x, coor.z * tileSize.y, coor.y * tileSize.z);
	    Instantiate(tile.tileObj, tile.tileObj.transform.position + pos, Quaternion.identity);
	    
	    for (int i = 0; i < tiles.Count; i++)
	    {
		    if (!nextCoor.notPossible[i])
		    {
			    if (i != tileIndex)
			    {
				    nextCoor.notPossible[i] = true;
				    tileRemovals.Push(new RemovalUpdate(i, nextCoor.coor));
			    }
			    

		    }
	    }

    }

    private Cell ChooseNextCell()
    {
	    while (entropyPq.Count > 0)
	    {
		    var cell = entropyPq.Pop().Key;
		    var coor = cell.coor;
		    if (!outputTiles[coor.x, coor.y, coor.z].isCollapsed)
		    {
			    return cell;
		    }
	    }

	    throw new NotImplementedException();
    }
    #endregion

    void CreateAdjacencyRules()
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

}