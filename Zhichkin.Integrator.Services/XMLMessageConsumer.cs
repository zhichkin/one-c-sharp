using System;
using System.Linq;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Text;
using System.Xml;
using Zhichkin.Integrator.Model;
using Zhichkin.Metadata.Model;

namespace Zhichkin.Integrator.Services
{
    public sealed class XMLMessageConsumer
    {
        private readonly IIntegratorService _service = new IntegratorService();

        public XMLMessageConsumer() { }

        public IDataMessage Consume(Stream stream)
        {
            IDataMessage message = new DataMessage();
            using (XmlReader reader = XmlReader.Create(stream))
            {
                while (reader.Read())
                {
                    if (reader.NodeType == XmlNodeType.Element)
                    {
                        if (reader.Name == "M") ReadDataMessage(reader, message);
                        else if (reader.Name == "E") ReadDataEntity(reader, message);
                        //else if (reader.Name == "S") ReadDataMessage(reader, message);
                        else if (reader.Name == "I") ReadDataRecord(reader, message, DataRecordType.Insert);
                        else if (reader.Name == "U") ReadDataRecord(reader, message, DataRecordType.Update);
                        else if (reader.Name == "D") ReadDataRecord(reader, message, DataRecordType.Delete);
                    }
                    //else if (reader.NodeType == XmlNodeType.Text) { }
                    //else if (reader.NodeType == XmlNodeType.EndElement) { }
                }
            }
            return message;
        }
        private void ReadDataMessage(XmlReader reader, IDataMessage message)
        {
            if (reader.HasAttributes)
            {
                while (reader.MoveToNextAttribute())
                {
                    if (reader.Name == "key") message.Key = reader.Value;
                }
            }
        }
        private void ReadDataEntity(XmlReader reader, IDataMessage message)
        {
            if (reader.HasAttributes)
            {
                IDataEntity entity = new DataEntity();
                while (reader.MoveToNextAttribute())
                {
                    if (reader.Name == "key") entity.Key = reader.Value;
                    //if (reader.Name == "table") entity.Table = reader.Value;
                    //else if (reader.Name == "keys") entity.Keys.Add(reader.Value);
                }
                message.Entities.Add(entity);
            }
        }
        private void ReadDataRecord(XmlReader reader, IDataMessage message, DataRecordType type)
        {
            if (message.Entities.Count == 0) return;
            int index = message.Entities.Count - 1; // last added entity
            IDataEntity entity = message.Entities[index];
            Entity metadata = Entity.Select(new Guid(entity.Key));
            if (reader.HasAttributes)
            {
                DataRecord record = new DataRecord(type);
                while (reader.MoveToNextAttribute())
                {
                    Field field = metadata.MainTable.Fields.Where(f => f.Name == reader.Name).FirstOrDefault();
                    if (field == null) continue;
                    record.AddColumnValue(reader.Name, Utilities.GetValueFromString(field, reader.Value));
                }
                entity.Records.Add(record);
            }
        }
    }
}
