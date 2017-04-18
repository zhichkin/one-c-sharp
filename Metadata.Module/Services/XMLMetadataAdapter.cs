using System;
using System.IO;
using System.Xml;
using System.Linq;
using Zhichkin.Metadata.Model;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Configuration;
using Zhichkin.Metadata.Notifications;

namespace Zhichkin.Metadata.Services
{
    public sealed class XMLMetadataAdapter : IMetadataAdapter
    {
        private sealed class AdapterContext
        {
            public AdapterContext(InfoBase infoBase)
            {
                InfoBase = infoBase;
            }
            public InfoBase InfoBase;
            public Namespace Namespace;
            public Entity Entity;
            public Property Property;
            public Table Table;
            public Field Field;
            public Dictionary<int, Entity> TypeCodes = new Dictionary<int, Entity>();
        }

        public void Load(string filePath, InfoBase infoBase)
        {
            AdapterContext context = new AdapterContext(infoBase);

            using (XmlReader reader = XmlReader.Create(filePath))
            {
                while (reader.Read())
                {
                    if (reader.NodeType == XmlNodeType.Element)
                    {
                        if (reader.Name == "InfoBase")
                        {
                            Read_InfoBase_Element(reader, context);
                        }
                        if (reader.Name == "Types")
                        {
                            context.TypeCodes.Clear();
                            context.InfoBase.Namespaces.Clear();
                        }
                        if (reader.Name == "Type")
                        {
                            Read_Type_Element(reader, context);
                        }
                        if (reader.Name == "Namespaces")
                        {
                            // do nothing: see Types tag
                        }
                        if (reader.Name == "Namespace")
                        {
                            Read_Namespace_Element(reader, context);
                        }
                        else if (reader.Name == "Entities")
                        {
                            context.Entity.NestedEntities.Clear();
                        }
                        else if (reader.Name == "Entity")
                        {
                            Read_Entity_Element(reader, context);
                        }
                        else if (reader.Name == "Properties")
                        {
                            context.Entity.Properties.Clear();
                        }
                        else if (reader.Name == "Property")
                        {
                            Read_Property_Element(reader, context);
                        }
                        else if (reader.Name == "Tables")
                        {
                            context.Entity.Tables.Clear();
                        }
                        else if (reader.Name == "Table")
                        {
                            Read_Table_Element(reader, context);
                        }
                        else if (reader.Name == "Fields")
                        {
                            context.Table.Fields.Clear();
                        }
                        else if (reader.Name == "Field")
                        {
                            Read_Field_Element(reader, context);
                        }
                    }
                    else if (reader.NodeType == XmlNodeType.EndElement)
                    {
                        if (reader.Name == "InfoBase")
                        {
                            Close_InfoBase_Element(context);
                        }
                        else if (reader.Name == "Type")
                        {
                            context.Namespace = null;
                            context.Entity = null;
                        }
                        else if (reader.Name == "Namespace")
                        {
                            context.Namespace = null;
                        }
                        else if (reader.Name == "Entity")
                        {
                            context.Entity = context.Entity.Owner;
                        }
                        else if (reader.Name == "Property")
                        {
                            context.Property = null;
                        }
                        else if (reader.Name == "Table")
                        {
                            context.Table = null;
                        }
                        else if (reader.Name == "Field")
                        {
                            context.Field = null;
                        }
                    }
                }
            }
        }
        
        private void Read_InfoBase_Element(XmlReader reader, AdapterContext context)
        {
            context.InfoBase.Name = reader.GetAttribute("name");
            context.InfoBase.Server = reader.GetAttribute("server");
            context.InfoBase.Database = reader.GetAttribute("database");
        }
        private void Close_InfoBase_Element(AdapterContext context)
        {
            context.Namespace = null;
            context.Entity = null;
            context.Property = null;
            context.Table = null;
            context.Field = null;
        }
        private void Read_Namespace_Element(XmlReader reader, AdapterContext context)
        {
            string name = reader.GetAttribute("name");

            context.Namespace = context.InfoBase.Namespaces
                .Where((n) => n.Name == name)
                .FirstOrDefault();

            if (context.Namespace != null) return;

            context.Namespace = new Namespace()
            {
                Name = name,
                Owner = context.InfoBase
            };
            context.InfoBase.Namespaces.Add(context.Namespace);
        }
        private void Read_Type_Element(XmlReader reader, AdapterContext context)
        {
            string code = reader.GetAttribute("code");
            string name = reader.GetAttribute("name");

            string[] names = name.Split(".".ToCharArray());
            string _namespace = names[0];
            string _entity = names[1];

            context.Namespace = context.InfoBase.Namespaces
                .Where((n) => n.Name == _namespace)
                .FirstOrDefault();
            
            if (context.Namespace == null)
            {
                context.Namespace = new Namespace()
                {
                    Name = _namespace,
                    Owner = context.InfoBase
                };
                context.InfoBase.Namespaces.Add(context.Namespace);
            }

            context.Entity = new Entity()
            {
                Code = int.Parse(code),
                Name = _entity,
                Namespace = context.Namespace
            };

            context.Namespace.Entities.Add(context.Entity);
            context.TypeCodes.Add(context.Entity.Code, context.Entity);
        }
        private void Read_Entity_Element(XmlReader reader, AdapterContext context)
        {
            string code = reader.GetAttribute("code");
            string name = reader.GetAttribute("name");
            string alias = reader.GetAttribute("alias");

            if (string.IsNullOrEmpty(code) && string.IsNullOrEmpty(name)) return; // system table
            
            if (!string.IsNullOrEmpty(code)) // reference type
            {
                context.Entity = context.TypeCodes[int.Parse(code)];
                context.Entity.Alias = (alias == null ? string.Empty : alias);
                context.Namespace = context.Entity.Namespace;
                return;
            }

            // value type

            if (context.Entity == null) // independent type
            {
                context.Entity = new Entity()
                {
                    Name = name,
                    Alias = (alias == null ? string.Empty : alias),
                    Namespace = context.Namespace
                };
                context.Namespace.Entities.Add(context.Entity);
            }
            else // nested type
            {
                Entity entity = new Entity()
                {
                    Name = name,
                    Alias = (alias == null ? string.Empty : alias),
                    Namespace = context.Namespace,
                    Owner = context.Entity
                };
                context.Entity.NestedEntities.Add(entity);
                context.Entity = entity;
            }
        }
        private void Read_Property_Element(XmlReader reader, AdapterContext context)
        {
            if (context.Entity == null) return; // system table
            
            string name = reader.GetAttribute("name");
            string ordinal = reader.GetAttribute("ordinal");
            string typeCodes = reader.GetAttribute("type");
            string purpose = reader.GetAttribute("purpose");
            
            context.Property = new Property()
            {
                Name = name,
                Ordinal = int.Parse(ordinal),
                Entity = context.Entity
            };
            context.Entity.Properties.Add(context.Property);

            SetPropertyPurpose(context, purpose);
            SetPropertyTypes(context, typeCodes);
        }
        private void SetPropertyPurpose(AdapterContext context, string purpose)
        {
            context.Property.Purpose = (PropertyPurpose)Enum.Parse(typeof(PropertyPurpose), purpose);
        }
        private void SetPropertyTypes(AdapterContext context, string typeCodes)
        {
            string[] types = typeCodes.Split(",".ToCharArray());

            foreach (string type in types)
            {
                if (type == "L")
                {
                    context.Property.Relations.Add(new Relation() { Entity = Entity.Boolean, Property = context.Property });
                }
                else if (type == "N")
                {
                    context.Property.Relations.Add(new Relation() { Entity = Entity.Decimal, Property = context.Property });
                }
                else if (type == "S")
                {
                    context.Property.Relations.Add(new Relation() { Entity = Entity.String, Property = context.Property });
                }
                else if (type == "T")
                {
                    context.Property.Relations.Add(new Relation() { Entity = Entity.DateTime, Property = context.Property });
                }
                else if (type == "B")
                {
                    context.Property.Relations.Add(new Relation() { Entity = Entity.Binary, Property = context.Property });
                }
                else if (type == "GUID")
                {
                    context.Property.Relations.Add(new Relation() { Entity = Entity.GUID, Property = context.Property });
                }
                else if (type == "IO") // вид движения накопления: 0 - приход, 1 - расход
                {
                    context.Property.Relations.Add(new Relation() { Entity = Entity.Int32, Property = context.Property });
                }
                else
                {
                    int typeCode;
                    if (int.TryParse(type, out typeCode))
                    {
                        context.Property.Relations.Add(new Relation()
                        {
                            Entity = context.TypeCodes[typeCode],
                            Property = context.Property
                        });
                    }
                    else
                    {
                        // wtf ? 8\
                    }
                }
            }
        }
        private void Read_Table_Element(XmlReader reader, AdapterContext context)
        {
            string name = reader.GetAttribute("name");
            string purpose = reader.GetAttribute("purpose");

            context.Table = new Table()
            {
                Entity = context.Entity,
                Name = name,
                Purpose = (TablePurpose)Enum.Parse(typeof(TablePurpose), purpose)
            };
            context.Entity.Tables.Add(context.Table);
        }
        private void Read_Field_Element(XmlReader reader, AdapterContext context)
        {
            string name = reader.GetAttribute("name");
            string purpose = reader.GetAttribute("purpose");
            string _property = reader.GetAttribute("property");

            context.Field = new Field()
            {
                Table = context.Table,
                Name = name,
                Purpose = (FieldPurpose)Enum.Parse(typeof(FieldPurpose), purpose)
            };
            context.Table.Fields.Add(context.Field);

            if (context.Table.Purpose != TablePurpose.Main) return;

            Property property = context.Entity.Properties
                .Where((p) => p.Name == _property)
                .FirstOrDefault();

            if (property == null) // system field, which has no mapping to any one property
            {
                property = new Property()
                {
                    Name = string.IsNullOrEmpty(_property) ? name : _property,
                    Entity = context.Entity,
                    Purpose = PropertyPurpose.System
                };
                // ? надо бы разобраться какой тип данных назначать таким свойствам ...
                property.Relations.Add(new Relation() { Entity = Entity.Binary, Property = property });
                context.Entity.Properties.Add(property);
            }

            context.Field.Property = property;
            property.Fields.Add(context.Field);
        }

        public InfoBase GetMetadata(string ProgID, SQLConnectionDialogNotification settings, string tempDirectory)
        {
            Type comType = Type.GetTypeFromProgID(ProgID, true); // V83.COMConnector
            dynamic connector = Activator.CreateInstance(comType);
            string connectionString = GetConnectionString(settings);
            dynamic session = connector.Connect(connectionString);

            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ConfigurationWriter.epf");
            string temp = Path.Combine(tempDirectory, "ConfigurationWriter.epf");
            File.Copy(path, temp, true);
            dynamic processor = session.ExternalDataProcessors.Create(temp);
            string output = Path.Combine(tempDirectory, "configuration.xml");

            processor.Write(output);
            File.Delete(temp);

            Marshal.ReleaseComObject(processor); processor = null;
            Marshal.ReleaseComObject(session); session = null;
            Marshal.ReleaseComObject(connector); connector = null;

            InfoBase infoBase = new InfoBase();
            this.Load(output, infoBase);
            File.Delete(output);
            return infoBase;
        }
        private string GetConnectionString(SQLConnectionDialogNotification settings)
        {
            return string.Format("Srvr=\"{0}\";Ref=\"{1}\";Usr=\"{2}\";Pwd=\"{3}\";",
                settings.Server,
                settings.Database,
                settings.UserName,
                settings.Password);
        }
    }
}
