using System;
using System.Collections.Generic;
using System.Linq;
using Zhichkin.ChangeTracking;
using Zhichkin.Metadata.Model;
using Zhichkin.Integrator.Model;
using System.Data;
using System.Data.SqlClient;

namespace Zhichkin.Translator
{
    public sealed class MessageTranslator : IMessageTranslator<ChangeTrackingRecord>
    {
        private Dictionary<string, ITranslationRule> rules = new Dictionary<string, ITranslationRule>();
        
        private readonly Subscription subscription;
        public MessageTranslator(Subscription subscription)
        {
            this.subscription = subscription;
            InitializeTranslationRules();
        }
        public ChangeTrackingRecord Translate(ChangeTrackingRecord source)
        {
            ChangeTrackingRecord target = new ChangeTrackingRecord();
            target.SYS_CHANGE_OPERATION = source.SYS_CHANGE_OPERATION;
            List<ChangeTrackingField> target_fields = new List<ChangeTrackingField>();
            ITranslationRule rule;
            foreach (ChangeTrackingField field in source.Fields)
            {
                if (rules.TryGetValue(field.Name, out rule))
                {
                    if (source.SYS_CHANGE_OPERATION == "D" && !rule.IsKey) continue;
                    rule.Apply(field, target_fields);
                }
            }
            target.Fields = target_fields.ToArray();
            return target;
        }
        private void InitializeTranslationRules()
        {
            foreach (TranslationRule rule in subscription.TranslationRules)
            {
                IList<Field> source_fields = rule.SourceProperty.Fields;
                IList<Field> target_fields = rule.TargetProperty.Fields;
                if (source_fields.Count == 1 && target_fields.Count == 1)
                {
                    CreateSimpleRule(rule, source_fields, target_fields);
                }
                else if (source_fields.Count == 1 && target_fields.Count > 1)
                {
                    CreateOneToManyRule(rule, source_fields, target_fields);
                }
                else if (source_fields.Count > 1 && target_fields.Count == 1)
                {
                    CreateManyToOneRule(rule, source_fields, target_fields);
                }
                else if (source_fields.Count > 1 && target_fields.Count > 1)
                {
                    CreateManyToManyRule(rule, source_fields, target_fields);
                }
            }
        }
        private Entity GetCorrespondingTargetEntity(InfoBase source, InfoBase target, int typeCode)
        {
            Entity result = null;
            using (SqlConnection connection = new SqlConnection(IntegratorPersistentContext.Current.ConnectionString))
            {
                using (SqlCommand command = connection.CreateCommand())
                {
                    command.CommandText = "[integrator].[get_corresponding_target_entity]";
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("source_infobase", source.Identity);
                    command.Parameters.AddWithValue("target_infobase", target.Identity);
                    command.Parameters.AddWithValue("type_code",       typeCode);
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            result = IntegratorPersistentContext.Current.Factory.New<Entity>(reader.GetGuid(0));
                        }
                    }
                }
            }
            return result;
        }
        private Entity GetCorrespondingSourceEntity(InfoBase source, InfoBase target, int typeCode)
        {
            Entity result = null;
            using (SqlConnection connection = new SqlConnection(IntegratorPersistentContext.Current.ConnectionString))
            {
                using (SqlCommand command = connection.CreateCommand())
                {
                    command.CommandText = "[integrator].[get_corresponding_source_entity]";
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("source_infobase", source.Identity);
                    command.Parameters.AddWithValue("target_infobase", target.Identity);
                    command.Parameters.AddWithValue("type_code",     typeCode);
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            result = IntegratorPersistentContext.Current.Factory.New<Entity>(reader.GetGuid(0));
                        }
                    }
                }
            }
            return result;
        }
        private Dictionary<int, int> GetPropertyTypeCodesLookup(Property property, InfoBase target)
        {
            Dictionary<int, int> lookup = new Dictionary<int, int>();
            using (SqlConnection connection = new SqlConnection(IntegratorPersistentContext.Current.ConnectionString))
            {
                using (SqlCommand command = connection.CreateCommand())
                {
                    command.CommandText = "[integrator].[get_property_type_codes_lookup]";
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("property",        property.Identity);
                    command.Parameters.AddWithValue("target_infobase", target.Identity);
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            lookup.Add(reader.GetInt32(0), reader.GetInt32(1));
                        }
                    }
                }
            }
            return lookup;
        }
        private void CreateSimpleRule(TranslationRule rule, IList<Field> source, IList<Field> target)
        {
            SimpleTranslationRule new_rule = new SimpleTranslationRule()
            {
                Name = target[0].Name,
                IsKey = rule.IsSyncKey
            };
            rules.Add(source[0].Name, new_rule);
        }
        private void CreateOneToManyRule(TranslationRule rule, IList<Field> source, IList<Field> target)
        {
            OneToManyTranslationRule new_rule = new OneToManyTranslationRule()
            {
                IsKey = rule.IsSyncKey
            };
            foreach (Field field in target)
            {
                if (field.Purpose == FieldPurpose.Locator)
                {
                    new_rule.LocatorField = field.Name;
                }
                else if (field.Purpose == FieldPurpose.TypeCode)
                {
                    new_rule.TypeCodeField = field.Name;
                    Entity corresponding_type = GetCorrespondingTargetEntity(
                        rule.Source.Namespace.InfoBase,
                        rule.Target.Namespace.InfoBase,
                        rule.SourceProperty.Relations[0].Entity.Code);
                    new_rule.TypeCode = (corresponding_type == null ? 0 : corresponding_type.Code);
                }
                else if (field.Purpose == FieldPurpose.Object)
                {
                    new_rule.Name = field.Name;
                }
                else
                {
                    continue;
                }
            }
            rules.Add(source[0].Name, new_rule);
        }
        private void CreateManyToOneRule(TranslationRule rule, IList<Field> source, IList<Field> target)
        {
            ManyToOneTranslationRule new_rule = new ManyToOneTranslationRule()
            {
                Name = target[0].Name,
                IsKey = rule.IsSyncKey
            };
            foreach (Field field in source)
            {
                rules.Add(field.Name, new_rule);
                if (field.Purpose == FieldPurpose.TypeCode)
                {
                    new_rule.TypeCodeField = field.Name;
                }
                else if (field.Purpose == FieldPurpose.Object)
                {
                    new_rule.ObjectField = field.Name;
                }
            }
            Entity corresponding_type = GetCorrespondingSourceEntity(
                rule.Source.Namespace.InfoBase,
                rule.Target.Namespace.InfoBase,
                rule.TargetProperty.Relations[0].Entity.Code);
            new_rule.TestTypeCode = (corresponding_type == null ? 0 : corresponding_type.Code);
        }
        private void CreateManyToManyRule(TranslationRule rule, IList<Field> source, IList<Field> target)
        {
            ManyToManyTranslationRule new_rule = new ManyToManyTranslationRule()
            {
                IsKey = rule.IsSyncKey
            };
            foreach (Field field in source)
            {
                rules.Add(field.Name, new_rule);
            }
            foreach (Field field in target)
            {
                new_rule.Fields[field.Purpose] = field.Name;
            }
            new_rule.TypeCodesLookup = GetPropertyTypeCodesLookup(rule.SourceProperty, rule.Target.Namespace.InfoBase);
        }
    }
}
