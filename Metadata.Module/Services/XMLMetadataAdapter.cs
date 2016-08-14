using System;
using System.IO;
using Zhichkin.Metadata.Model;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Reflection;

namespace Zhichkin.Metadata.Services
{
    public sealed class XMLMetadataAdapter : IMetadataAdapter
    {
        public void Load(string connectionString, InfoBase infoBase)
        {
            using (ComConnector connector = new ComConnector(connectionString))
            {
                connector.Connect();

                string filePath = @"C:\Users\User\Desktop\Z\1C\ЭкспортКонфигурацииXML.epf";
                using (IComWrapper component = connector.CreateComponent(filePath))
                {
                    component.Call("ЗаписатьКонфигурацию", @"C:\Users\User\Desktop\Z\1C\zhichkin.xml");
                }
            }
            GC.Collect(); // !? COM-соединение подвисает в 1С


        }
    }
}
