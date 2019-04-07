using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Zhichkin.ChangeTracking;
using Zhichkin.Integrator.Model;
using Zhichkin.Metadata.Model;

namespace Zhichkin.Integrator.Translator
{
    public sealed class MessageTranslator : IMessageTranslator<ChangeTrackingMessage>
    {
        private Dictionary<string, ITranslationRule> rules = new Dictionary<string, ITranslationRule>();

        private readonly Subscription subscription;
        public MessageTranslator(Subscription subscription)
        {
            this.subscription = subscription;
            InitializeTranslationRules();
        }
        public ChangeTrackingMessage Translate(ChangeTrackingMessage source)
        {
            this.ResetRules();
            ChangeTrackingMessage target = new ChangeTrackingMessage();
            target.SYS_CHANGE_OPERATION = source.SYS_CHANGE_OPERATION;
            List<ChangeTrackingField> target_fields = new List<ChangeTrackingField>();
            List<object> target_values = new List<object>();
            ITranslationRule rule;
            ChangeTrackingRecord record = source.Records[0];
            for (int i = 0; i < source.Fields.Length; i++)
            {
                ChangeTrackingField field = source.Fields[i];
                if (rules.TryGetValue(field.Name, out rule))
                {
                    if (source.SYS_CHANGE_OPERATION == "D" && !rule.IsSyncKey) continue;
                    rule.Apply(field, record.Values[i], target_fields, target_values);
                }
            }
            target.Fields = target_fields.ToArray();
            target.Records = new ChangeTrackingRecord[] { new ChangeTrackingRecord() { Values = target_values.ToArray() } };
            return target;
        }
        private void ResetRules()
        {
            foreach (ITranslationRule rule in rules.Values)
            {
                rule.Reset();
            }
        }
        private void InitializeTranslationRules()
        {
            foreach (TranslationRule rule in subscription.TranslationRules)
            {
                IList<Field> source_fields = rule.SourceProperty.Fields;
                IList<Field> target_fields = rule.TargetProperty.Fields;
                if (source_fields.Count == 1 && target_fields.Count == 1)
                {
                    CreateOneToOneRule(rule, source_fields, target_fields);
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
                            int key = reader.GetInt32(0);
                            if (lookup.ContainsKey(key)) continue;
                            int value = reader.GetInt32(1);
                            if (value == 0) continue;
                            lookup.Add(key, value);
                        }
                    }
                }
            }
            return lookup;
        }
        private void CreateOneToOneRule(TranslationRule rule, IList<Field> source, IList<Field> target)
        {
            OneToOneTranslationRule new_rule = new OneToOneTranslationRule()
            {
                TargetName = target[0].Name,
                IsSyncKey = rule.IsSyncKey
            };
            rules.Add(source[0].Name, new_rule);
        }
        private void CreateManyToManyRule(TranslationRule rule, IList<Field> source, IList<Field> target)
        {
            ManyToManyTranslationRule new_rule = new ManyToManyTranslationRule()
            {
                IsSyncKey = rule.IsSyncKey
            };
            foreach (Field field in source)
            {
                rules.Add(field.Name, new_rule);
            }
            foreach (Field field in target)
            {
                new_rule.TargetFields[field.Purpose] = field.Name;
            }
            new_rule.TypeCodesLookup = GetPropertyTypeCodesLookup(rule.SourceProperty, rule.Target.Namespace.InfoBase);
        }
        // TODO: 1-X and X-1 rules have to be revised !
        private void CreateOneToManyRule(TranslationRule rule, IList<Field> source, IList<Field> target)
        {
            OneToManyTranslationRule new_rule = new OneToManyTranslationRule()
            {
                IsSyncKey = rule.IsSyncKey
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
                    new_rule.TargetName = field.Name;
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
                TargetName = target[0].Name,
                IsSyncKey = rule.IsSyncKey
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
    }
}
