using UnityEngine;
using System.Collections;
using System.Linq;
using System;
using System.Collections.Generic;

public static class _ {
	public static bool Logging = true;
	public static void Log(object msg){
		if(Logging) Debug.Log(msg);
	}

    public static void Log(params object[] msgs)
    {
        if (Logging)
        {
            Debug.Log(string.Concat(msgs));
        }
    }

    public static void Log2(params object[] msgs)
    {
        if (!Logging) return;
        Debug.Log(String.Join(", ", msgs.Select(e =>
        {
            if (e != null) return e.ToString();
            else return "null";
        }).ToArray()));
    }
}
