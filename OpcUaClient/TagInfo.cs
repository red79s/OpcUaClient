
using Opc.Ua;
using System;
using System.ComponentModel;

namespace OpcUaClient
{
    public class TagInfo : INotifyPropertyChanged
    {
        public MainWindow Parent { get; set; }
        private DataValue _attributeData;
        private bool _autoUpdateActive;
        public ReferenceDescription Element { get; set; }
        public object UpdateValue { get; set; }

        public DataValue AttributeData
        {
            get { return _attributeData; }
            set
            {
                _attributeData = value;

                OnPropertyChanged("Value");
                OnPropertyChanged("EnableAutoUpdate");
                OnPropertyChanged("EnableCancelAutoUpdate");
                OnPropertyChanged("LastUpdated");
            }
        }

        public string TagName
        {
            get { return Element.DisplayName.ToString(); }
        }

        public string TagDataType
        {
            get
            {
                if (!HasValue)
                    return "Unknown";

                return GetFriendlyTypeName(DataType);
            }
        }

        public Type DataType
        {
            get
            {
                if (!HasValue)
                    return typeof (System.String);
                return AttributeData.Value.GetType();
            }
        }

        public object Value
        {
            get
            {
                if (!HasValue)
                {
                    if (AttributeData != null && !StatusCode.IsGood(AttributeData.StatusCode))
                    {
                        return "Bad value";
                    }
                    return "No value";
                }

                return AttributeData.Value;
            }
            set { UpdateValue = value; }
        }

        public bool AutoUpdateActive
        {
            get { return _autoUpdateActive; }
            set
            {
                _autoUpdateActive = value;
                OnPropertyChanged("AutoUpdateActive");
                OnPropertyChanged("EnableAutoUpdate");
                OnPropertyChanged("EnableCancelAutoUpdate");
            }
        }

        public bool EnableAutoUpdate
        {
            get
            {
                var res = !AutoUpdateActive && IsIntValue();
                return res;
            } 
        }

        public bool EnableCancelAutoUpdate
        {
            get { return AutoUpdateActive && IsIntValue(); }
        }

        public bool IsIntValue()
        {
            if (!HasValue)
                return false;
            if (DataType == typeof (Int16) || DataType == typeof (Int32) || DataType == typeof (Int64))
                return true;
            return false;
        }

        public bool IsStringValue()
        {
            if (!HasValue)
                return false;
            if (DataType == typeof(String))
                return true;
            return false;
        }

        public bool HasValue
        {
            get { return AttributeData != null; }
        }

        public string LastUpdated
        {
            get
            {
                var dt = DateTime.MinValue;
                if (HasValue)
                {
                    dt = AttributeData.SourceTimestamp;
                }

                return dt.ToString("dd/MM/yyyy HH:mm:ss:fff");
            }
        }
        public object GetUpdateValue()
        {
            if (UpdateValue == null)
                return null;

            var strValue = UpdateValue as string;
            if (strValue != null)
            {
                if (DataType == typeof (Int16))
                {
                    return Int16.Parse(strValue);
                }
                if (DataType == typeof(Int32))
                {
                    return Int32.Parse(strValue);
                }
                if (DataType == typeof (Int64))
                {
                    return Int64.Parse(strValue);
                }
                return strValue;
            }

            return UpdateValue;
        }

        public string GetFriendlyTypeName(Type type)
        {
            if (type == typeof(int))
                return "int";
            else if (type == typeof(short))
                return "short";
            else if (type == typeof(byte))
                return "byte";
            else if (type == typeof(bool))
                return "bool";
            else if (type == typeof(long))
                return "long";
            else if (type == typeof(float))
                return "float";
            else if (type == typeof(double))
                return "double";
            else if (type == typeof(decimal))
                return "decimal";
            else if (type == typeof(string))
                return "string";
            //else if (type.IsGenericType)
            //    return type.Name.Split('`')[0] + "<" + string.Join(", ", type.GetGenericArguments().Select(x => GetFriendlyName(x)).ToArray()) + ">";
            else
                return type.Name;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
