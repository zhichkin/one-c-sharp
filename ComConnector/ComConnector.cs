using System;
using System.Reflection;
using System.Runtime.InteropServices;


namespace Zhichkin
{
    // Строка подключения к ИБ 1С (файловый вариант) "File=""C:\1CBase""";Usr=""login"";Pwd=""password"";";
    // Строка подключения к ИБ 1С (серверный вариант) "Srvr=""server"";Ref=""BaseName"";Usr=""login"";Pwd=""password"";";

    public sealed class ComConnector : IDisposable
    {
        private readonly string com_connector = "V83.COMConnector";
        private readonly string connection_string;
        private Type com_type;
        private object com_object;
        private object connector;

        private const string CONST_Connect = "Connect";
        private const string CONST_NewObject = "NewObject";
        private const string CONST_String = "String";
        private const string CONST_Metadata = "Metadata";
        private const string CONST_ExternalDataProcessors = "ExternalDataProcessors";
        private const string CONST_GetDBNames = "ПолучитьСтруктуруХраненияБазыДанных";

        public ComConnector(string connectionString)
        {
            this.connection_string = connectionString;
            Initialize();
        }
        private void Initialize()
        {
            com_type = Type.GetTypeFromProgID(com_connector, true);
            com_object = Activator.CreateInstance(com_type);
        }
        private object Get(string property_name)
        {
            return com_type.InvokeMember(property_name, BindingFlags.Public | BindingFlags.GetProperty, null, connector, new object[] { });
        }
        private object Call(string method_name, params object[] args)
        {
            return com_type.InvokeMember(method_name, BindingFlags.Public | BindingFlags.InvokeMethod, null, connector, args);
        }

        public void Connect()
        {
            connector = com_type.InvokeMember(CONST_Connect, BindingFlags.Public | BindingFlags.InvokeMethod, null, com_object, new object[] { connection_string });
        }
        public IComWrapper Metadata
        {
            get
            {
                return new ComWrapper(com_type, Get(CONST_Metadata));
            }
        }
        public IComWrapper NewObject(string name)
        {
            return new ComWrapper(com_type, Call(CONST_NewObject, name));
        }
        public IComWrapper CreateComponent(string fileName)
        {
            IComWrapper component = null;
            using (IComWrapper wrapper = new ComWrapper(com_type, Get(CONST_ExternalDataProcessors)))
            {
                component = new ComWrapper(com_type, wrapper.Call("Create", fileName));
            }
            return component;
        }
        public IComWrapper GetDBNames(IComWrapper metadata_object)
        {
            IComWrapper table = null;
            using (IComWrapper array = this.NewObject("Массив"))
            {
                array.Call("Добавить", metadata_object.ComObject);
                table = new ComWrapper(com_type, Call(CONST_GetDBNames, array.ComObject, true));
            }
            return table;
        }
        public string GetTypeName(object typeObject)
        {
            string typeName = string.Empty;
            using (IComWrapper typeInfo = new ComWrapper(com_type, Call("XMLТип", typeObject)))
            {
                typeName = (string)typeInfo.Get("ИмяТипа");
            }
            return typeName;
        }
        public string ToString(object value)
        {
            return (string)Call(CONST_String, value);
        }
        public void Dispose()
        {
            Marshal.ReleaseComObject(connector);
            Marshal.ReleaseComObject(com_object);
        }

        public IComWrapper NewQuery()
        {
            //IComWrapper query = connector.NewObject("Запрос");
            //query.Set("Текст", GetQueryText());
            ////query.Call("УстановитьПараметр", "Дата1", new DateTime(2010, 3, 1, 0, 0, 0));
            ////query.Call("УстановитьПараметр", "Дата2", new DateTime(2010, 10, 1, 0, 0, 0));
            //IComWrapper result = query.CallAndWrap("Выполнить");
            //if ((bool)result.Call("Пустой"))
            //{
            //    result.Dispose();
            //    query.Dispose();
            //    return null;
            //}
            //cursor = result.CallAndWrap("Выбрать");

            //result.Dispose();
            //query.Dispose();

            //return cursor;
            return null;
        }

        private void use_cursor()
        {
            //using (IComWrapper cursor = adapter.GetCursor())
            //{
            //    if (cursor != null)
            //    {
            //        while ((bool)cursor.Call("Следующий"))
            //        {
            //            using (SqlCommand command = connection.CreateCommand())
            //            {
            //                translator.Translate(cursor, command);
            //                AddParameter(command, "id_sea", SqlDbType.Int, ParameterDirection.Input, session_id);
            //                command.ExecuteNonQuery();
            //            }
            //        }
            //    }
            //}
        }
    }
}
