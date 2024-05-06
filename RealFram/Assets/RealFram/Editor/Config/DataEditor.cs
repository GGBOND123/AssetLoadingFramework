using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.Reflection;
using System.IO;
using System.Xml;
using OfficeOpenXml;
using System.ComponentModel;

public class DataEditor
{
    public static string XmlPath = RealConfig.GetRealFram().m_XmlPath;
    public static string BinaryPath = RealConfig.GetRealFram().m_BinaryPath;
    public static string ScriptsPath = RealConfig.GetRealFram().m_ScriptsPath;
    public static string ExcelPath = Application.dataPath + "/../Data/Excel/";
    public static string RegPath = Application.dataPath + "/../Data/Reg/";

    
    [MenuItem("Assets/类转xml")]
    //通过选中文件的名字，找到对应的类名
    public static void AssetsClassToXml()
    {
        UnityEngine.Object[] objs = Selection.objects;
        for (int i = 0; i < objs.Length; i++)
        {
            EditorUtility.DisplayProgressBar("文件下的类转成xml", "正在扫描" + objs[i].name + "... ...", 1.0f / objs.Length * i);
            ClassToXml(objs[i].name);
        }
        AssetDatabase.Refresh();
        EditorUtility.ClearProgressBar();
    }

    [MenuItem("Assets/Xml转Binary")]
    public static void AssetsXmlToBinary()
    {
        UnityEngine.Object[] objs = Selection.objects;
        for (int i = 0; i < objs.Length; i++)
        {
            EditorUtility.DisplayProgressBar("文件下的xml转成二进制", "正在扫描" + objs[i].name + "... ...", 1.0f / objs.Length * i);
            XmlToBinary(objs[i].name);
        }
        AssetDatabase.Refresh();
        EditorUtility.ClearProgressBar();
    }

    [MenuItem("Assets/Xml转Excel")]
    public static void AssetsXmlToExcel()
    {
        UnityEngine.Object[] objs = Selection.objects;
        for (int i = 0; i < objs.Length; i++)
        {
            EditorUtility.DisplayProgressBar("文件下的xml转成Excel", "正在扫描" + objs[i].name + "... ...", 1.0f / objs.Length * i);
            XmlToExcel(objs[i].name);
        }
        AssetDatabase.Refresh();
        EditorUtility.ClearProgressBar();
    }

    [MenuItem("Tools/Xml/Xml转成二进制")]
    public static void AllXmlToBinary()
    {
        string path = Application.dataPath.Replace("Assets", "") + XmlPath;
        string[] filesPath = Directory.GetFiles(path, "*.*", SearchOption.AllDirectories);
        for (int i = 0; i < filesPath.Length; i++)
        {
            EditorUtility.DisplayProgressBar("查找文件夹下面的Xml", "正在扫描" + filesPath[i] + "... ...", 1.0f / filesPath.Length * i);
            //只处理.XML文件
            if (filesPath[i].EndsWith(".xml"))
            {
                string tempPath = filesPath[i].Substring(filesPath[i].LastIndexOf("/") + 1);
                tempPath = tempPath.Replace(".xml", "");
                XmlToBinary(tempPath);
            }
        }
        AssetDatabase.Refresh();
        EditorUtility.ClearProgressBar();
    }

    [MenuItem("Tools/Xml/Excel转Xml")]
    public static void AllExcelToXml()
    {
        string[] filePaths = Directory.GetFiles(RegPath, "*", SearchOption.AllDirectories);
        for (int i = 0; i < filePaths.Length; i++)
        {
            if (!filePaths[i].EndsWith(".xml"))
                continue;
            EditorUtility.DisplayProgressBar("查找文件夹下的类","正在扫描路径" + filePaths[i] + "... ...", 1.0f / filePaths.Length * i);
            string path = filePaths[i].Substring(filePaths[i].LastIndexOf("/") + 1);
            ExcelToXml(path.Replace(".xml", ""));
        }

        AssetDatabase.Refresh();
        EditorUtility.ClearProgressBar();
    }

    [MenuItem("Tools/测试/测试读取xml")]
    public static void TestReadXml()
    {
        string xmlPath = Application.dataPath + "/../Data/Reg/MonsterData.xml";
        XmlReader reader = null;
        try
        {
            XmlDocument xml = new XmlDocument();
            reader = XmlReader.Create(xmlPath);
            xml.Load(reader);
            //查询已知相对路径的节点
            XmlNode xn = xml.SelectSingleNode("data");
            XmlElement xe = (XmlElement)xn;
            //根据对应
            string className = xe.GetAttribute("name");
            string xmlName = xe.GetAttribute("to");
            string excelName = xe.GetAttribute("from");
            reader.Close();
            Debug.LogError(className + "  " + xmlName + "  " + excelName);
            //当前节点的子节点
            foreach (XmlNode node in xe.ChildNodes)
            {
                XmlElement tempXe = (XmlElement)node;
                string name = tempXe.GetAttribute("name");
                string type = tempXe.GetAttribute("type");
                Debug.LogError(name + "  " + type);
                XmlNode listNode = tempXe.FirstChild;
                XmlElement listElement = (XmlElement)listNode;
                string listName = listElement.GetAttribute("name");
                string sheetName = listElement.GetAttribute("sheetname");
                string mainKey = listElement.GetAttribute("mainKey");
                Debug.LogError("list: " + listName + "  " + sheetName + "  " + mainKey);
                foreach (XmlNode nd in listElement.ChildNodes)
                {
                    XmlElement txe = (XmlElement)nd;
                    Debug.LogError(txe.GetAttribute("name") + "  " + txe.GetAttribute("col") + "  " + txe.GetAttribute("type"));
                }
            }
        }
        catch (Exception e)
        {
            if (reader != null)
                reader.Close();
            Debug.LogError(e);
        }
    }

    [MenuItem("Tools/测试/测试写入Excel")]
    public static void TestWriteExcel()
    {
        string xlsxPath = Application.dataPath + "/../Data/Excel/G怪物.xlsx";
        FileInfo xlsxFile = new FileInfo(xlsxPath);           //也可以用流
        if (xlsxFile.Exists)
        {
            xlsxFile.Delete();
            xlsxFile = new FileInfo(xlsxPath);
        }
        using (ExcelPackage package = new ExcelPackage(xlsxFile))
        {
            //Workbook：工作簿
            //Worksheets：返回一 个 Sheets 集合。可以以此添加表格。
            ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("怪物配置");
            //worksheet.DefaultColWidth = 10;//sheet页面默认行宽度
            //worksheet.DefaultRowHeight = 30;//sheet页面默认列高度
            //worksheet.Cells.Style.WrapText = true;//设置所有单元格的自动换行
            //worksheet.InsertColumn();//插入行，从某一行开始插入多少行
            //worksheet.InsertRow();//插入列，从某一列开始插入多少列
            //worksheet.DeleteColumn();//删除行，从某一行开始删除多少行
            //worksheet.DeleteRow();//删除列，从某一列开始删除多少列
            //worksheet.Column(1).Width = 10;//设定第几行宽度
            //worksheet.Row(1).Height = 30;//设定第几列高度
            //worksheet.Column(1).Hidden = true;//设定第几行隐藏
            //worksheet.Row(1).Hidden = true;//设定第几列隐藏
            //worksheet.Column(1).Style.Locked = true;//设定第几行锁定
            //worksheet.Row(1).Style.Locked = true;//设定第几列锁定
            //worksheet.Cells.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;//设定所有单元格对齐方式

            worksheet.Cells.AutoFitColumns();
            //第一行第一列
            ExcelRange range = worksheet.Cells[1, 1];
            range.Value = " 测试sadddddddddddddddddddddddddddddddddasda";
            range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.None;
            //range.Style.Fill.BackgroundColor.SetColor();//设置单元格内背景颜色
            //range.Style.Font.Color.SetColor();//设置单元格内字体颜色
            range.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;//对齐方式 
            //自适应宽度。 打开表格时，Text值长度将决定框的长度。
            range.AutoFitColumns();
            //自动换行。  如果不开，将不读取 / n换行符。  如果开了，当跳转框宽度时，Text值宽度不够时会自动换行。
            range.Style.WrapText = true;    
            //存入表中
            package.Save();
        }
    }

    [MenuItem("Tools/测试/测试已有类进行反射")]
    public static void TestReflection1()
    {
        TestInfo testInfo = new TestInfo()
        {
            Id = 2,
            Name = "测试反射",
            IsA = false,
            AllStrList = new List<string>(),
            AllTestInfoList = new List<TestInfoTwo>(),
        };
        testInfo.AllStrList.Add("测试1111");   testInfo.AllStrList.Add("测试2222");  testInfo.AllStrList.Add("测试3333");
        for (int i = 0; i < 3; i++)
        {
            TestInfoTwo test = new TestInfoTwo();
            test.Id = i + 1;
            test.Name = i + "name";
            testInfo.AllTestInfoList.Add(test);
        }; 
        GetMemberValue(testInfo, "Name");
        //普通数组
        //object list = GetMemberValue(testInfo, "AllStrList");
        //int listCount = System.Convert.ToInt32(list.GetType().InvokeMember("get_Count", BindingFlags.Default | BindingFlags.InvokeMethod, null, list, new object[] { }));

        //for(int i = 0; i < listCount; i++)
        //{
        //    object item = list.GetType().InvokeMember("get_Item", BindingFlags.Default | BindingFlags.InvokeMethod, null, list, new object[] { i });
        //    Debug.LogError(item);
        //}

        //普通数组中是一个类
        object list = GetMemberValue(testInfo, "AllTestInfoList");
        //InvokeMember是C#中 反射动态调用 对象/类 的方法/字段/属性 时需要用到的方法。
        //通常使用InvokeMember方法时，参数如下：
        //string name: 方法 / 字段的名称
        //BindingFlags invokeAttr: 枚举类BindingFlags代表需要动态调用的是哪种方法 / 字段，枚举值有Static、Public、NonPublic、InvokeMethod、GetField、SetField、GetProperty、SetProperty等多种枚举值（字段是Field，属性是Property：包含get / set）
        //object target: 如果动态调用的是对象，那么需要传对象实例（否则null）
        //object[] args: 如果动态调用的方法需要传参，那么需要传args
        int listCount = System.Convert.ToInt32(list.GetType().InvokeMember("get_Count", BindingFlags.Default | BindingFlags.InvokeMethod, null, list, new object[] { }));
        for (int i = 0; i < listCount; i++)
        {   //
            object item = list.GetType().InvokeMember("get_Item", BindingFlags.Default | BindingFlags.InvokeMethod, null, list, new object[] { i });

            object id = GetMemberValue(item, "Id");
            object name = GetMemberValue(item, "Name");
            Debug.LogError(id + " " + name);
        }
    }

    [MenuItem("Tools/测试/测试已有数据进行反射")]
    public static void TestReflection2()
    {
        //成员变量
        object obj = CreateClass("TestInfo");
        PropertyInfo info = obj.GetType().GetProperty("Id");
        SetValue(info, obj, "21", "int");
        PropertyInfo nameInfo = obj.GetType().GetProperty("Name");
        SetValue(nameInfo, obj, "aqweddad", "string");
        PropertyInfo isInfo = obj.GetType().GetProperty("IsA");
        SetValue(isInfo, obj, "true", "bool");
        PropertyInfo heighInfo = obj.GetType().GetProperty("Heigh");
        SetValue(heighInfo, obj, "51.4", "float");
        PropertyInfo enumInfo = obj.GetType().GetProperty("TestType");
        SetValue(enumInfo, obj, "VAR1", "enum");

        //数组
        Type type = typeof(string);
        object list = CreateList(type);
        for (int i = 0; i < 3; i++)
        {
            object addItem = "测试填数据" + i;
            //调用Add添加数据。
            list.GetType().InvokeMember("Add", BindingFlags.Default | BindingFlags.InvokeMethod, null, list, new object[] { addItem });//调用list的add方法添加数据
        }
        obj.GetType().GetProperty("AllStrList").SetValue(obj, list);

        //数组中是类
        object twoList = CreateList(typeof(TestInfoTwo));
        for (int i = 0; i < 3; i++)
        {
            object addItem = CreateClass("TestInfoTwo");
            PropertyInfo itemIdInfo = addItem.GetType().GetProperty("Id");
            SetValue(itemIdInfo, addItem, "152" + i, "int");
            PropertyInfo itemNameInfo = addItem.GetType().GetProperty("Name");
            SetValue(itemNameInfo, addItem, "测试类" + i, "string");
            twoList.GetType().InvokeMember("Add", BindingFlags.Default | BindingFlags.InvokeMethod, null, twoList, new object[] { addItem });
        }
        obj.GetType().GetProperty("AllTestInfoList").SetValue(obj, twoList);

        TestInfo testInfo = (obj as TestInfo);
        //foreach (string str in testInfo.AllStrList)
        //{
        //    Debug.LogError(str);
        //}

        foreach (TestInfoTwo test in testInfo.AllTestInfoList)
        {
            Debug.LogError(test.Id + " " + test.Name);
        }
    }

    private static void ExcelToXml(string name)
    {
        string className = "";
        string xmlName = "";
        string excelName = "";
        //第一步，读取Reg文件，确定类的结构
        Dictionary<string, SheetClass> allSheetClassDic = ReadReg(name, ref excelName, ref xmlName, ref className);

        //第二步，读取excel里面的数据
        string excelPath = ExcelPath + excelName;
        Dictionary<string, SheetData> sheetDataDic = new Dictionary<string, SheetData>();
        try
        {
            using (FileStream stream = new FileStream(excelPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                using (ExcelPackage package = new ExcelPackage(stream))
                {
                    ExcelWorksheets worksheetArray = package.Workbook.Worksheets;
                    for (int i = 0; i < worksheetArray.Count; i++)
                    {
                        SheetData sheetData = new SheetData();
                        ExcelWorksheet worksheet = worksheetArray[i + 1];
                        SheetClass sheetClass = allSheetClassDic[worksheet.Name];
                        int colCount = worksheet.Dimension.End.Column;
                        int rowCount = worksheet.Dimension.End.Row;

                        for (int n = 0; n < sheetClass.VarList.Count; n++)
                        {
                            sheetData.AllName.Add(sheetClass.VarList[n].Name);
                            sheetData.AllType.Add(sheetClass.VarList[n].Type);
                        }

                        for (int m = 1; m < rowCount; m++)
                        {
                            RowData rowData = new RowData();
                            int n = 0;
                            if (string.IsNullOrEmpty(sheetClass.SplitStr) && sheetClass.ParentVar != null
                                && !string.IsNullOrEmpty(sheetClass.ParentVar.Foregin))
                            {
                                rowData.ParnetVlue = worksheet.Cells[m + 1, 1].Value.ToString().Trim();
                                n = 1;
                            }
                            for (; n < colCount; n++)
                            {
                                ExcelRange range = worksheet.Cells[m + 1, n + 1];
                                string value = "";
                                if (range.Value != null)
                                {
                                    value = range.Value.ToString().Trim();
                                }
                                string colValue = worksheet.Cells[1, n + 1].Value.ToString().Trim();
                                rowData.RowDataDic.Add(GetNameFormCol(sheetClass.VarList, colValue), value);
                            }

                            sheetData.AllData.Add(rowData);
                        }
                        sheetDataDic.Add(worksheet.Name, sheetData);
                    }
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError(e);
            return;
        }

        //根据类的结构，创建类，并且给每个变量赋值（从excel里读出来的值）
        object objClass = CreateClass(className);

        List<string> outKeyList = new List<string>();
        foreach (string str in allSheetClassDic.Keys)
        {
            SheetClass sheetClass = allSheetClassDic[str];
            if (sheetClass.Depth == 1)
            {
                outKeyList.Add(str);
            }
        }

        for (int i = 0; i < outKeyList.Count; i++)
        {
            ReadDataToClass(objClass, allSheetClassDic[outKeyList[i]], sheetDataDic[outKeyList[i]], allSheetClassDic, sheetDataDic, null);
        }

        BinarySerializeOpt.Xmlserialize(XmlPath + xmlName, objClass);
        //BinarySerializeOpt.BinarySerilize(BinaryPath + className + ".bytes", objClass);
        Debug.Log(excelName + "表导入unity完成！");
        AssetDatabase.Refresh();
    }

    private static void ReadDataToClass(object objClass, SheetClass sheetClass, SheetData sheetData, Dictionary<string, SheetClass> allSheetClassDic, Dictionary<string, SheetData> sheetDataDic, object keyValue)
    {
        object item = CreateClass(sheetClass.Name);//只是为了得到变量类型
        object list = CreateList(item.GetType());
        for (int i = 0; i < sheetData.AllData.Count; i++)
        {
            if (keyValue != null &&!string.IsNullOrEmpty(sheetData.AllData[i].ParnetVlue))
            {
                if (sheetData.AllData[i].ParnetVlue != keyValue.ToString())
                    continue;
            }
            object addItem = CreateClass(sheetClass.Name);
            for (int j = 0; j < sheetClass.VarList.Count; j++)
            {
                VarClass varClass = sheetClass.VarList[j];
                if (varClass.Type == "list" && string.IsNullOrEmpty(varClass.SplitStr))
                {
                    ReadDataToClass(addItem, allSheetClassDic[varClass.ListSheetName], sheetDataDic[varClass.ListSheetName], allSheetClassDic, sheetDataDic, GetMemberValue(addItem, sheetClass.MainKey));
                }
                else if (varClass.Type == "list")
                {
                    string value = sheetData.AllData[i].RowDataDic[sheetData.AllName[j]];
                    SetSplitClass(addItem, allSheetClassDic[varClass.ListSheetName], value);
                }
                else if (varClass.Type == "listStr" || varClass.Type == "listFloat" || varClass.Type == "listInt" || varClass.Type == "listBool")
                {
                    string value = sheetData.AllData[i].RowDataDic[sheetData.AllName[j]];
                    SetSplitBaseClass(addItem, varClass, value);
                }
                else
                {
                    string value = sheetData.AllData[i].RowDataDic[sheetData.AllName[j]];
                    if (string.IsNullOrEmpty(value) && !string.IsNullOrEmpty(varClass.DeafultValue))
                    {
                        value = varClass.DeafultValue;
                    }
                    if (string.IsNullOrEmpty(value))
                    {
                        Debug.LogError("表格中有空数据，或者Reg文件未配置defaultValue！" + sheetData.AllName[j]);
                        continue;
                    }
                    SetValue(addItem.GetType().GetProperty(sheetData.AllName[j]), addItem, value, sheetData.AllType[j]);
                }
            }
            list.GetType().InvokeMember("Add", BindingFlags.Default | BindingFlags.InvokeMethod, null, list, new object[] { addItem });
        }
        objClass.GetType().GetProperty(sheetClass.ParentVar.Name).SetValue(objClass, list);
    }

    /// <summary>
    /// 自定义类List赋值
    /// </summary>
    /// <param name="objClass"></param>
    /// <param name="sheetClass"></param>
    /// <param name="value"></param>
    private static void SetSplitClass(object objClass, SheetClass sheetClass, string value)
    {
        object item = CreateClass(sheetClass.Name);
        object list = CreateList(item.GetType());
        if (string.IsNullOrEmpty(value))
        {
            Debug.Log("excel里面自定义list的列里有空值！" + sheetClass.Name);
            return;
        }
        else
        {
            string splitStr = sheetClass.ParentVar.SplitStr.Replace("\\n", "\n").Replace("\\r", "\r");
            string[] rowArray = value.Split(new string[] { splitStr }, StringSplitOptions.None);
            for (int i = 0; i < rowArray.Length; i++)
            {
                object addItem = CreateClass(sheetClass.Name);
                string[] valueList = rowArray[i].Trim().Split(new string[] { sheetClass.SplitStr }, StringSplitOptions.None);
                for (int j = 0; j < valueList.Length; j++)
                {
                    SetValue(addItem.GetType().GetProperty(sheetClass.VarList[j].Name), addItem, valueList[j].Trim(), sheetClass.VarList[j].Type);
                }
                list.GetType().InvokeMember("Add", BindingFlags.Default | BindingFlags.InvokeMethod, null, list, new object[] { addItem });
            }

        }
        objClass.GetType().GetProperty(sheetClass.ParentVar.Name).SetValue(objClass, list);
    }

    /// <summary>
    /// 基础List赋值
    /// </summary>
    /// <param name="objClass"></param>
    /// <param name="varClass"></param>
    /// <param name="value"></param>
    private static void SetSplitBaseClass(object objClass, VarClass varClass, string value)
    {
        Type type = null;
        if (varClass.Type == "listStr")
        {
            type = typeof(string);
        }
        else if (varClass.Type == "listFloat")
        {
            type = typeof(float);
        }
        else if (varClass.Type == "listInt")
        {
            type = typeof(int);
        }
        else if (varClass.Type == "listBool")
        {
            type = typeof(bool);
        }
        object list = CreateList(type);
        string[] rowArray = value.Split(new string[] { varClass.SplitStr }, StringSplitOptions.None);
        for (int i = 0; i < rowArray.Length; i++)
        {
            object addItem = rowArray[i].Trim();
            try
            {
                list.GetType().InvokeMember("Add", BindingFlags.Default | BindingFlags.InvokeMethod, null, list, new object[] { addItem });
            }
            catch
            {
                Debug.Log(varClass.ListSheetName + "  里 " + varClass.Name + "  列表添加失败！具体数值是：" + addItem);
            }
        }
        objClass.GetType().GetProperty(varClass.Name).SetValue(objClass, list);
    }

    /// <summary>
    /// 根据列名获取变量名
    /// </summary>
    /// <param name="varlist"></param>
    /// <param name="col"></param>
    /// <returns></returns>
    private static string GetNameFormCol(List<VarClass> varlist, string col)
    {
        foreach (VarClass varClass in varlist)
        {
            if (varClass.Col == col)
                return varClass.Name;
        }
        return null;
    }




   
    private static void XmlToExcel(string name)
    {
        string className = "";
        string xmlName = "";
        string excelName = "";
        //获取所有 类名-SheetClass 键值对,类名对应<variable>中的name
        Dictionary<string, SheetClass> allSheetClassDic = ReadReg(name, ref excelName, ref xmlName, ref className);

        //计算类中有几个list，然后将他们依次塞入数据。
        List<SheetClass> outSheetList = new List<SheetClass>();
        foreach (SheetClass sheetClass in allSheetClassDic.Values)
        {
            if (sheetClass.Depth == 1)
                outSheetList.Add(sheetClass);
        }

        //通过Reg里面的XML文件名,获取其所有 List-SheetData 键值对，包括List中的List 
        Dictionary<string, SheetData> sheetDataDic = new Dictionary<string, SheetData>();
        object data = GetObjFormXml(className);
        for (int i = 0; i < outSheetList.Count; i++)
            ReadData(data, outSheetList[i], allSheetClassDic, sheetDataDic, "");



        //开始写入Excel
        string xlsxPath = ExcelPath + excelName;
        if (FileIsUsed(xlsxPath))
        {
            Debug.LogError("文件被占用，无法修改");
            return;
        }

        try
        {
            FileInfo xlsxFile = new FileInfo(xlsxPath);
            if (xlsxFile.Exists)
            {
                xlsxFile.Delete();
                xlsxFile = new FileInfo(xlsxPath);
            }
            using (ExcelPackage package = new ExcelPackage(xlsxFile))
            {
                //对于每个sheet 开始写入excel
                foreach (string str in sheetDataDic.Keys)
                {
                    ExcelWorksheet worksheet = package.Workbook.Worksheets.Add(str);
                    SheetData sheetData = sheetDataDic[str];
                    //所有列名塞入
                    for (int i = 0; i < sheetData.AllName.Count; i++)
                    {
                        ExcelRange range = worksheet.Cells[1, i + 1];
                        range.Value = sheetData.AllName[i];
                        range.AutoFitColumns();
                    }
                    //对于每一行数据塞入
                    for (int i = 0; i < sheetData.AllData.Count; i++)
                    {
                        RowData rowData = sheetData.AllData[i];
                        for (int j = 0; j < sheetData.AllData[i].RowDataDic.Count; j++)
                        {
                            ExcelRange range = worksheet.Cells[i + 2, j + 1];
                            string vaule = rowData.RowDataDic[sheetData.AllName[j]];
                            range.Value = vaule;
                            range.AutoFitColumns();
                            //如果包含换行符才进行换行.
                            if (vaule.Contains("\n") || vaule.Contains("\r\n"))
                                range.Style.WrapText = true;
                        }
                    }
                    worksheet.Cells.AutoFitColumns();
                }
                package.Save();
            }
        }
        catch(Exception e)
        {
            Debug.LogError(e);
            return;
        }
        Debug.Log("生成"+xlsxPath+"成功！！！");
    }

    /// <summary>
    /// 通过Reg里面的XML文件名,获取其所有 List-SheetClass 键值对，包括List中的List 
    /// </summary>
    private static Dictionary<string, SheetClass> ReadReg(string name, ref string excelName, ref string xmlName, ref string className)
    {
        string regPath = RegPath + name + ".xml";
        if (!File.Exists(regPath))
        {
            Debug.LogError("此数据不存在配置变化xml：" + name);
        }
        XmlDocument xml = new XmlDocument();
        XmlReader reader = XmlReader.Create(regPath);
        // 忽略xml里面的注释
        XmlReaderSettings settings = new XmlReaderSettings();
        settings.IgnoreComments = true;
        xml.Load(reader);
        //查询已知相对路径的节点
        XmlNode xn = xml.SelectSingleNode("data");
        XmlElement xe = (XmlElement)xn;
        className = xe.GetAttribute("name");
        xmlName = xe.GetAttribute("to");
        excelName = xe.GetAttribute("from");

        //储存所有变量的表sheet
        Dictionary<string, SheetClass> allSheetClassDic = new Dictionary<string, SheetClass>();
        ReadXmlNode(xe, allSheetClassDic, 0);
        reader.Close();
        return allSheetClassDic;
    }

    /// <summary>
    /// 反序列化xml到类
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    private static object GetObjFormXml(string name)
    {
        Type type = null;
        foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
        {
            Type tempType = asm.GetType(name);
            if (tempType != null)
            {
                type = tempType;
                break;
            }
        }
        if (type != null)
        {
            string xmlPath = XmlPath + name + ".xml";
            return BinarySerializeOpt.XmlDeserialize(xmlPath, type);
        }

        return null;
    }

    /// <summary>
    /// 递归读取类里面当前List的数据 。
    /// </summary>
    private static void ReadData(object data, SheetClass sheetClass, Dictionary<string, SheetClass> allSheetClassDic, Dictionary<string, SheetData> sheetDataDic, string mainKey)
    {
        List<VarClass> varList = sheetClass.VarList;
        VarClass varClass = sheetClass.ParentVar;     //<list>外的<variable>含有数组的变量名
        //通过反射，获得类中当前数组。
        object dataList = GetMemberValue(data, varClass.Name);
        //数组长度，即表格中有多少行
        int listCount = System.Convert.ToInt32(dataList.GetType().InvokeMember("get_Count", BindingFlags.Default | BindingFlags.InvokeMethod, null, dataList, new object[] { }));

        SheetData sheetData = new SheetData();

        //表明是个list ,并且新增一行.
        if (!string.IsNullOrEmpty(varClass.Foregin))
        {
            sheetData.AllName.Add(varClass.Foregin);
            sheetData.AllType.Add(varClass.Type);
        }

        //添加当前sheet对应的行和类型，即List<T>中，T有多少变量
        for (int i = 0; i < varList.Count; i++)
        {
            if (!string.IsNullOrEmpty(varList[i].Col))
            {
                sheetData.AllName.Add(varList[i].Col);
                sheetData.AllType.Add(varList[i].Type);
            }
        }

        string tempKey = mainKey;
        //遍历行数进行赋值
        for (int i = 0; i < listCount; i++)
        {
            //第i个数组的T值  
            object item = dataList.GetType().InvokeMember("get_Item", BindingFlags.Default | BindingFlags.InvokeMethod, null, dataList, new object[] { i });

            RowData rowData = new RowData();
            if (!string.IsNullOrEmpty(varClass.Foregin) && !string.IsNullOrEmpty(tempKey))
            {
                rowData.RowDataDic.Add(varClass.Foregin, tempKey);
            }

            if (!string.IsNullOrEmpty(sheetClass.MainKey))
            {
                mainKey = GetMemberValue(item, sheetClass.MainKey).ToString();
            }
            //遍历列数,第j个共有成员进行处理.
            for (int j = 0; j < varList.Count; j++)
            {
                if (varList[j].Type == "list" && string.IsNullOrEmpty(varList[j].SplitStr))
                {
                    SheetClass tempSheetClass = allSheetClassDic[varList[j].ListSheetName];
                    ReadData(item, tempSheetClass, allSheetClassDic, sheetDataDic, mainKey);
                }
                else if (varList[j].Type == "list")
                {
                    SheetClass tempSheetClass = allSheetClassDic[varList[j].ListSheetName];
                    string value = GetSplitStrList(item, varList[j], tempSheetClass);
                    rowData.RowDataDic.Add(varList[j].Col, value);
                }
                else if (varList[j].Type == "listStr" || varList[j].Type == "listFloat" || varList[j].Type == "listInt" || varList[j].Type == "listBool")
                {   //将所有值都加到一个字符串中,用分号隔开
                    string value = GetSpliteBaseList(item, varList[j]);
                    rowData.RowDataDic.Add(varList[j].Col, value);
                }
                else
                {   //每一行数据中  类名-数值 存入字典.
                    object value = GetMemberValue(item, varList[j].Name);
                    if (varList != null)
                        rowData.RowDataDic.Add(varList[j].Col, value.ToString());
                    else 
                        Debug.LogError(varList[j].Name + "反射出来为空！");
                }
            }

            //RowData数据塞完,放进 string-SheetData字典中. 表明每一
            string key = varClass.ListSheetName;
            if (sheetDataDic.ContainsKey(key))
                sheetDataDic[key].AllData.Add(rowData);
            else
            {
                sheetData.AllData.Add(rowData);
                sheetDataDic.Add(key, sheetData);
            }
        }
    }

    /// <summary>
    /// 获取本身是一个类的列表，但是数据比较少；（没办法确定父级结构的）
    /// </summary>
    /// <returns></returns>
    private static string GetSplitStrList(object data, VarClass varClass, SheetClass sheetClass)
    {
        string split = varClass.SplitStr;
        string classSplit = sheetClass.SplitStr;
        string str = "";
        if (string.IsNullOrEmpty(split) || string.IsNullOrEmpty(classSplit))
        {
            Debug.LogError("类的列类分隔符或变量分隔符为空！！！");
            return str;
        }
        //获取名为varClass.Name的List<T>变量
        object dataList = GetMemberValue(data, varClass.Name);
        int listCount = System.Convert.ToInt32(dataList.GetType().InvokeMember("get_Count", BindingFlags.Default | BindingFlags.InvokeMethod, null, dataList, new object[] { }));
        for (int i = 0; i < listCount; i++)
        {
            //当前List中的第i个T类实例
            object item = dataList.GetType().InvokeMember("get_Item", BindingFlags.Default | BindingFlags.InvokeMethod, null, dataList, new object[] { i });
            //遍历该T对象的所有共有成员.
            for (int j = 0; j < sheetClass.VarList.Count; j++)
            {
                object value = GetMemberValue(item, sheetClass.VarList[j].Name);
                str += value.ToString();
                if (j != sheetClass.VarList.Count - 1)
                    str += classSplit.Replace("\\n", "\n").Replace("\\r", "\r");
            }
            if (i != listCount - 1)
                str += split.Replace("\\n", "\n").Replace("\\r", "\r");
        }
        return str;
    }

    /// <summary>
    /// 获取基础List里面的所有值,通过分隔符拼凑出来. 在塞入Excel前的中间类数据中被调用.
    /// </summary>
    /// <returns></returns>
    private static string GetSpliteBaseList(object data, VarClass varClass)
    {
        string str = "";
        if (string.IsNullOrEmpty(varClass.SplitStr))
        {
            Debug.LogError("基础List的分隔符为空！");
            return str;
        }
        object dataList = GetMemberValue(data, varClass.Name);
        int listCount = System.Convert.ToInt32(dataList.GetType().InvokeMember("get_Count", BindingFlags.Default | BindingFlags.InvokeMethod, null, dataList, new object[] { }));

        for (int i = 0; i < listCount; i++)
        {
            object item = dataList.GetType().InvokeMember("get_Item", BindingFlags.Default | BindingFlags.InvokeMethod, null, dataList, new object[] { i });
            str += item.ToString();
            if (i != listCount - 1)
            {
                str += varClass.SplitStr.Replace("\\n", "\n").Replace("\\r", "\r");
            }
        }
        return str;
    }

    /// <summary>
    /// 递归读取配置结构
    /// </summary>
    private static void ReadXmlNode(XmlElement xmlElement, Dictionary<string, SheetClass> allSheetClassDic, int depth)
    {
        depth++;
        foreach (XmlNode node in xmlElement.ChildNodes)
        {
            //sheet父节点值，为<variable>。
            XmlElement xe = (XmlElement)node;
            //如果是List，才需要递归。
            if (xe.GetAttribute("type") == "list")
            {
                //操作当前sheet表的父节点<variable>
                VarClass parentVar = new VarClass()
                {
                    Name = xe.GetAttribute("name"),
                    Type = xe.GetAttribute("type"),
                    Col  = xe.GetAttribute("col"),
                    DeafultValue = xe.GetAttribute("defaultValue"),
                    Foregin = xe.GetAttribute("foregin"),
                    SplitStr = xe.GetAttribute("split"),
                };
                if (parentVar.Type == "list")
                {
                    parentVar.ListName = ((XmlElement)xe.FirstChild).GetAttribute("name");
                    parentVar.ListSheetName = ((XmlElement)xe.FirstChild).GetAttribute("sheetname");
                }

                //获取当前sheet表<list>类
                XmlElement listEle = (XmlElement)node.FirstChild;
                SheetClass sheetClass = new SheetClass()
                {
                    Name = listEle.GetAttribute("name"),
                    SheetName = listEle.GetAttribute("sheetname"),
                    SplitStr = listEle.GetAttribute("split"),
                    MainKey = listEle.GetAttribute("mainKey"),
                    ParentVar = parentVar,
                    Depth = depth,
                };

                //表名存在，才有这张表
                if (!string.IsNullOrEmpty(sheetClass.SheetName))
                {
                    if (!allSheetClassDic.ContainsKey(sheetClass.SheetName))
                    {
                        //获取当前Sheet表中所有<variable>的列项。即list中所有变量，那怕是list中装有list类型，也会将内部的list（对应sheet）记录到该字典中。
                        foreach (XmlNode insideNode in listEle.ChildNodes)
                        {
                            XmlElement insideXe = (XmlElement)insideNode;

                            VarClass varClass = new VarClass()
                            {
                                Name = insideXe.GetAttribute("name"),
                                Type = insideXe.GetAttribute("type"),
                                Col = insideXe.GetAttribute("col"),
                                DeafultValue = insideXe.GetAttribute("defaultValue"),
                                Foregin = insideXe.GetAttribute("foregin"),
                                SplitStr = insideXe.GetAttribute("split"),
                            };
                            if (varClass.Type == "list")
                            {
                                varClass.ListName = ((XmlElement)insideXe.FirstChild).GetAttribute("name");
                                varClass.ListSheetName = ((XmlElement)insideXe.FirstChild).GetAttribute("sheetname");
                            }

                            sheetClass.VarList.Add(varClass);
                        }
                        allSheetClassDic.Add(sheetClass.SheetName, sheetClass);
                    }
                }
                //list节点传入，然后调用这一层的childnode依次递归。
                ReadXmlNode(listEle, allSheetClassDic, depth);
            }
        }
    }


    /// <summary>
    /// 判断文件是否被占用
    /// </summary>
    private static bool FileIsUsed(string path)
    {
        bool result = false;

        if (!File.Exists(path))
            result = false;
        else
        {
            FileStream fileStream = null;
            //打开如果出异常了,就是被占用了.
            try
            {
                fileStream = File.Open(path, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
                result = false;
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                result = true;
            }
            finally
            {
                if (fileStream != null)
                    fileStream.Close();
            }
        }

        return result;
    }

    /// <summary>
    /// 反射new一個list
    /// </summary>
    private static object CreateList(Type type)
    {
        Type listType = typeof(List<>);
        //确定list<>里面T的类型。  参数表示<>内的类型种类，如果是字典，就会是长度为2的数组。
        Type specType = listType.MakeGenericType(new System.Type[] { type });
        //new出来这个list
        return Activator.CreateInstance(specType, new object[] { });
    }

    /// <summary>
    /// 反射变量赋值
    /// </summary>
    private static void SetValue(PropertyInfo info, object var, string value, string type)
    {
        object val = (object)value;
        if (type == "int")
            val = System.Convert.ToInt32(val);
        else if (type == "bool")
            val = System.Convert.ToBoolean(val);
        else if (type == "float")
            val = System.Convert.ToSingle(val); //转为float
        else if (type == "enum")   //枚举类型通过反射给类成员设值
            //返回指定组件类型的类型转换器,并进行转换。  并从String转换为枚举
            val = TypeDescriptor.GetConverter(info.PropertyType).ConvertFromInvariantString(val.ToString());
        info.SetValue(var, val);
    }

    /// <summary>
    /// 反射类里面的变量的具体数值
    /// </summary>
    private static object GetMemberValue(object obj, string memeberName, BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance) 
        //BindingFlags.Instance：代表返回的是一个实例
    {
        Type type = obj.GetType();
        //通过当前类型Type可以获取当前Type指定成员，比如搜索具有指定名称或者指定隐私关系的公共成员。
        MemberInfo[] members = type.GetMember(memeberName, bindingFlags);

        //当前类型没有对应变量，则可能是基类变量，所以往基类去找对应变量。
        //while (members == null || members.Length == 0)
        //{
        //    type = type.BaseType;   //当前 Type 直接继承类
        //    if (type == null)
        //        return;

        //    members = type.GetMember("Name",  BindingFlags.Public | BindingFlags.Default);
        //}

        //测试第一个Member是哪种成员类型。
        switch (members[0].MemberType)   
        {
            //MemberTypes.Field：字段就是变量，包含静态或者只读等
            //GetField()：Get an array of FieldInfo objects，并返回一个FieldInfo[]。    
            //单个FieldInfo通过GetValue获取他的值。
            case MemberTypes.Field:
                return type.GetField(memeberName, bindingFlags).GetValue(obj);
            case MemberTypes.Property:
                return type.GetProperty(memeberName, bindingFlags).GetValue(obj);
            default:
                return null;
        }
    }

    /// <summary>
    /// 反射创建类的实例
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    private static object CreateClass(string name)
    {
        object obj = null;
        Type type = null;
        //遍历程序集
        foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
        {
            Type tempType = asm.GetType(name);
            if (tempType != null)
            {
                type = tempType;
                break;
            }
        }
        if (type != null)
        {
            obj = Activator.CreateInstance(type);
        }
        return obj;
    }

    /// <summary>
    /// xml转二进制
    /// </summary>
    /// <param name="name"></param>
    private static void XmlToBinary(string name)
    {
        if (string.IsNullOrEmpty(name))
            return;

        try
        {
            Type type = null;
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                Type tempType = asm.GetType(name);
                if (tempType != null)
                {
                    type = tempType;
                    break;
                }
            }
            if (type != null)
            {
                string xmlPath = XmlPath + name + ".xml";
                string binaryPath = BinaryPath + name + ".bytes";
                object obj = BinarySerializeOpt.XmlDeserialize(xmlPath, type);
                BinarySerializeOpt.BinarySerilize(binaryPath, obj);
                Debug.Log(name + "xml转二进制成功，二进制路径为:" + binaryPath);
            }
        }
        catch
        {
            Debug.LogError(name + "xml转二进制失败！");
        }
    }

    /// <summary>
    /// 实际的类转XML
    /// </summary>
    /// <param name="name"></param>
    private static void ClassToXml(string name)
    {
        if (string.IsNullOrEmpty(name))
            return;

        try
        {
            Type type = null;
            //找到当前应用程序域，从其一堆程序集中通过反射找到对应的类 并构造。
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                Type tempType = asm.GetType(name);
                if (tempType != null)
                {
                    type = tempType;
                    break;
                }
            }
            if (type != null)
            {
                var temp = Activator.CreateInstance(type);
                if (temp is ExcelBase)
                {
                    (temp as ExcelBase).Construction();
                }
                string xmlPath = XmlPath + name + ".xml";
                BinarySerializeOpt.Xmlserialize(xmlPath, temp);
                Debug.Log(name + "类转xml成功，xml路径为:" + xmlPath);
            }
        }
        catch
        {
            Debug.LogError(name + "类转xml失败！");
        }
    }
}

// XML中<list>标签 
public class SheetClass
{
    //所属父级Var变量
    public VarClass ParentVar { get; set; }
    //深度
    public int Depth { get; set; }
    //类名
    public string Name { get; set; }
    //类对应的sheet名
    public string SheetName { get; set; }
    //主键
    public string MainKey { get; set; }
    /// <summary>
    /// 类分隔符,用于将List<类>中的数据写到一列中时用到.
    /// </summary>
    public string SplitStr { get; set; }
    //所包含的变量
    public List<VarClass> VarList = new List<VarClass>();
}



// XML中<variable>标签  每个list中的元素
public class VarClass
{
    //原类里面变量的名称
    public string Name { get; set; }
    //变量类型
    public string Type { get; set; }
    //变量对应的Excel里的列名
    public string Col { get; set; }
    //变量的默认值
    public string DeafultValue { get; set; }
    //变量是list的话，外联部分列
    public string Foregin { get; set; }
    //分隔符
    public string SplitStr { get; set; }
    //如果自己是List，对应的list类名
    public string ListName { get; set; }
    //如果自己是list,对应的sheet名
    public string ListSheetName {get;set;}
}

public class SheetData
{
    //多少列 
    public List<string> AllName = new List<string>();
    public List<string> AllType = new List<string>();
    //多少行数据
    public List<RowData> AllData = new List<RowData>();
}
/// <summary>
/// 每一行的字典类值
/// </summary>
public class RowData
{
    public string ParnetVlue = "";
    //key为excel中每一列最上面的string，里面应该存储了所有最上面的string键值对。
    public Dictionary<string, string> RowDataDic = new Dictionary<string, string>();
}


public enum TestEnum
{
    None = 0,
    VAR1 = 1,
    TEST2 = 2,
}

public class TestInfo
{
    public int Id { get; set; }
    public string Name { get; set; }
    public bool IsA { get; set; }

    public float Heigh { get; set; }

    public TestEnum TestType { get; set; }

    public List<string> AllStrList { get; set; }
    public List<TestInfoTwo> AllTestInfoList { get; set; }
}

public class TestInfoTwo
{
    public int Id { get; set; }
    public string Name { get; set; }
}
