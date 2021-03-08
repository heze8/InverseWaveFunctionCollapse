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
    public float offset;
    #endregion
 
    #region Unity Methods
 
    void Start()
    {
	    int i = 0;
	    foreach (var t in wfc.tiles)
	    {
		    var instantiate = Instantiate(buttonPrefab, buttonPrefab.transform.position + i++ * offset * Vector3.down, Quaternion.identity);
		    instantiate.transform.SetParent(gameObject.transform);
		    int k = i;
		    instantiate.GetComponentInChildren<Button>().onClick.AddListener(()=>wfc.ButtonPress(k));
		    instantiate.GetComponentInChildren<TextMeshProUGUI>().text = i.ToString();
	    }

	    
    }
 
    void Update()
    {
	
    }
 
    #endregion
 
    #region Private Methods
    #endregion
}