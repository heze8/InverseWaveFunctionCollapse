using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

///summary
///summary
public class UI : MonoBehaviour
{
 
    #region Public Fields

    public WaveFunctionCollapse wfc;
    public GameObject buttonPrefab;
    public GameObject ori;
    public float offset;
    private Vector3Int inputSize;
    #endregion
 
    #region Unity Methods
 
    void Start()
    {
	    int i = 0;
	    foreach (var t in wfc.tileSet)
	    {
		    var instantiate = Instantiate(buttonPrefab, ori.transform.position + i++ * offset * Vector3.down, Quaternion.identity);
		    instantiate.transform.SetParent(gameObject.transform);
		    int k = i;
		    instantiate.GetComponentInChildren<Button>().onClick.AddListener(()=>wfc.ButtonPress(k));
		    instantiate.GetComponentInChildren<TextMeshProUGUI>().text = i.ToString();
	    }

	    
    }

    public void InputSizeX(string size)
    {

	    inputSize.x = Int32.Parse(size);


	    wfc.UpdateSize(inputSize);
    }
    public void InputSizeY(string size)
    {

	    inputSize.y = Int32.Parse(size);


	    wfc.UpdateSize(inputSize);
    }
    public void InputSizeZ(string size)
    {

	    inputSize.z = Int32.Parse(size);


	    wfc.UpdateSize(inputSize);
    }
 
    void Update()
    {
	
    }
 
    #endregion
 
    #region Private Methods
    #endregion
}