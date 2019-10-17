using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// redis-cli(客户端)的命令总结：
//https://www.cnblogs.com/silent2012/p/5368925.html

public class RedisManager : MonoBehaviour
{
    public static RedisManager Instance { get; private set; }
    
    [SerializeField] private string _address = "127.0.0.1";
    [SerializeField] private int _port = 6379;

    public CSRedis.CSRedisClient CSRedis { get; private set; }

    void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("RedisManager must be Singleton! 必须是单例！！！");
        }
        Instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        CSRedis = new CSRedis.CSRedisClient("127.0.0.1:6379");
        RedisHelper.Initialization(CSRedis);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
