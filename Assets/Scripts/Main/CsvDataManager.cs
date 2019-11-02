using System.Collections;
using System.Collections.Generic;
using System.Data;
using UnityEngine;
using GameUtils;

public class CsvDataManager : MonoBehaviour
{
    public static CsvDataManager Instance;
    private Dictionary<string, CsvStreamReader> _liStreamReaders = new Dictionary<string, CsvStreamReader>();
    
    void Awake()
    {
        Instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    public void LoadDataAll()
    {
        string dataPath = Application.streamingAssetsPath + "/Data";
        List<string> lstDataFiles = new List<string>();
        Utils.GetDir(dataPath, "*.csv", ref lstDataFiles);
        foreach (var fullname in lstDataFiles)
        {
            
            int index = fullname.LastIndexOf('/');
            int index2 = fullname.LastIndexOf('.');
            string nakedName = fullname.Substring(index+1, index2-index-1);
            CsvStreamReader csv = new CsvStreamReader(fullname, System.Text.Encoding.UTF8);
            _liStreamReaders.Add(nakedName, csv);
        }
    }

    public CsvStreamReader GetTable(string filename)
    {
        if (_liStreamReaders.ContainsKey(filename))
        {
            return _liStreamReaders[filename];
        }

        return null;
    }
}

