using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnitsAndBuilduings;
using System.Xml;
using System.Reflection;

namespace UnitDesigner
{
    static class Serializer
    {
        public static void Deserialize<T>(string xml, ref T obj) where T : new()
        {
            obj = (T)ConvertTo(xml, obj.GetType().UnderlyingSystemType);
            if (obj != null)
                return;
            obj = new T();
            FieldInfo[] props = obj.GetType().GetFields();

            for (int i = 0; i < props.Length; i++)
            {
                string xmlSub = GetXmlSubXml(xml, props[i].Name);
                object val = Activator.CreateInstance(props[i].FieldType);
                Deserialize(xmlSub, ref val);
                props[i].SetValue(obj, val);
            }
        }

        public static string GetXmlSubXml(string xml, string key)
        {
            string idStr = "<" + key + ">";
            int stInd = xml.IndexOf(idStr) + idStr.Length;
            string endId = "</" + key + ">";
            int endInd = xml.IndexOf(endId);
            return xml.Substring(stInd, endInd - stInd).TrimEnd().TrimStart();
        }

        public static object ConvertTo(string str, Type T)
        {
            if (T == typeof(int))
                return Convert.ToInt32(str);
            if (T == typeof(float))
                return Convert.ToSingle(str);
            if (T == typeof(string))
                return str;
            if (T == typeof(bool))
                return Convert.ToBoolean(str);
            return null;
        }

        /*static Type[] okTypes = new Type[3] { typeof(int), typeof(string), typeof(float) };

        public static string GetValue(string xml, string key)
        {
            string idStr = "<" + key + ">";
            int stInd = xml.IndexOf(idStr) + idStr.Length;
            int len = xml.IndexOf('<', stInd) - stInd;
            return xml.Substring(stInd, len);
        }

        public static UnitTypeInfo Desserialize(string xml)
        {
            UnitTypeInfo info = new UnitTypeInfo();
            FieldInfo[] props = info.GetType().GetFields();
            for (int i = 0; i < props.Length; i++)
            {
                if (okTypes.Contains(props[i].FieldType))
                {
                    props[i].SetValue(info, ConvertTo(GetValue(xml, props[i].Name), props[i].FieldType));
                }
            }
            return info;
        }*/

        public static string Serialize(UnitTypeInfo info)
        {
            return null;
        }
    }
}
