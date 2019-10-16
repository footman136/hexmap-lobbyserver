using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 给定一个字符串，生成这个字符串的CRC32码🐎
/// https://www.cnblogs.com/Kconnie/p/3538194.html
/// </summary>
public class Crc32
{
    protected static long[] Crc32Table;
    //生成CRC32码表
    public static void GetCRC32Table() 
    {
        Crc32Table = new long[256];
        int i,j;
        for(i = 0;i < 256; i++) 
        {
            var Crc = (long)i;
            for (j = 8; j > 0; j--)
            {
                if ((Crc & 1) == 1)
                Crc = (Crc >> 1) ^ 0xEDB88320;
                else
                Crc >>= 1;
            }
            Crc32Table[i] = Crc;
        }
    }
    
    //获取字符串的CRC32校验值
    public static long GetCRC32(string sInputString)
    {
        //生成码表
        GetCRC32Table();
        byte[] buffer = System.Text.ASCIIEncoding.ASCII.GetBytes(sInputString);
        long value = 0xffffffff;
        int len = buffer.Length;
        for (int i = 0; i < len; i++)
        {
            value = (value >> 8) ^ Crc32Table[(value & 0xFF)^ buffer[i]];
        }
        return value ^ 0xffffffff; 
    }
}

