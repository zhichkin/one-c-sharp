using System;
using System.Collections.Generic;
using Zhichkin.ChangeTracking;
using Zhichkin.Metadata.Model;
using M = Zhichkin.Metadata.Model;

namespace Zhichkin.Translator
{
    public class ManyToManyTranslationRule : SimpleTranslationRule
    {
        public Dictionary<FieldPurpose, string> Fields = new Dictionary<FieldPurpose, string>()
                {
                    { FieldPurpose.Locator,  string.Empty },
                    { FieldPurpose.TypeCode, string.Empty },
                    { FieldPurpose.Object,   string.Empty }
                };
        public Dictionary<FieldPurpose, object> Values = new Dictionary<FieldPurpose, object>()
                {
                    { FieldPurpose.TypeCode, string.Empty },
                    { FieldPurpose.Object,   string.Empty }
                };
        private Dictionary<FieldPurpose, bool> Flags = new Dictionary<FieldPurpose, bool>()
                {
                    { FieldPurpose.TypeCode, false },
                    { FieldPurpose.Object,   false }
                };
        public Dictionary<int, int> TypeCodesLookup = new Dictionary<int, int>();
        public override void Apply(ChangeTrackingField field, IList<ChangeTrackingField> target)
        {
            FieldPurpose purpose = M.Utilities.ParseFieldPurpose(field.Name);
            if (purpose == FieldPurpose.TypeCode || purpose == FieldPurpose.Object)
            {
                Values[purpose] = field.Value;
                Flags[purpose] = true;
            }
            else
            {
                return; // _TYPE
            }
            if (Flags[FieldPurpose.TypeCode] && Flags[FieldPurpose.Object])
            {
                if (Fields[FieldPurpose.Locator] != string.Empty)
                {
                    target.Add(new ChangeTrackingField()
                    {
                        Name = Fields[FieldPurpose.Locator],
                        Value = 0x08,
                        IsKey = field.IsKey
                    });
                }
                int type_code = Utilities.GetInt32(((byte[])Values[FieldPurpose.TypeCode]));
                if (TypeCodesLookup.TryGetValue(type_code, out type_code))
                {
                    target.Add(new ChangeTrackingField()
                    {
                        Name = Fields[FieldPurpose.Object],
                        Value = Values[FieldPurpose.Object],
                        IsKey = field.IsKey
                    });
                }
                else
                {
                    target.Add(new ChangeTrackingField()
                    {
                        Name = Fields[FieldPurpose.Object],
                        Value = Guid.Empty,
                        IsKey = field.IsKey
                    });
                }
                target.Add(new ChangeTrackingField()
                {
                    Name = Fields[FieldPurpose.TypeCode],
                    Value = type_code,
                    IsKey = field.IsKey
                });
            }
        }
    }
}
