using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ExcelBase
{
#if UNITY_EDITOR
    //编译器下对ExcelBase实例化时的构造函数。
    public virtual void Construction() { }
#endif
    //存储到内存中的配置表字典中前 进行初始化。
    public virtual void Init() { }
}
