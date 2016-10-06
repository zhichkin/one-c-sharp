using System;
using System.Collections.Generic;
using Zhichkin.ChangeTracking;
using Zhichkin.Metadata.Model;
using M = Zhichkin.Metadata.Model;

namespace Zhichkin.Integrator.Translator
{
    public class ManyToManyTranslationRule : OneToOneTranslationRule
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
        public override void Apply(ChangeTrackingField sourceField, object sourceValue, IList<ChangeTrackingField> targetFields, IList<object> targetValues)
        {
            FieldPurpose purpose = M.Utilities.ParseFieldPurpose(sourceField.Name);
            if (purpose == FieldPurpose.TypeCode || purpose == FieldPurpose.Object)
            {
                Values[purpose] = sourceValue;
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
                    targetFields.Add(new ChangeTrackingField()
                    {
                        Name = Fields[FieldPurpose.Locator],
                        Type = "binary",
                        IsKey = sourceField.IsKey
                    });
                    targetValues.Add(new byte[1] { 0x08 });
                }
                int type_code = Utilities.GetInt32(((byte[])Values[FieldPurpose.TypeCode]));
                targetFields.Add(new ChangeTrackingField()
                {
                    Name = Fields[FieldPurpose.Object],
                    Type = "binary",
                    IsKey = sourceField.IsKey
                });
                if (TypeCodesLookup.TryGetValue(type_code, out type_code))
                {
                    targetValues.Add(Values[FieldPurpose.Object]);
                }
                else
                {
                    targetValues.Add(Guid.Empty.ToByteArray());
                }
                targetFields.Add(new ChangeTrackingField()
                {
                    Name = Fields[FieldPurpose.TypeCode],
                    Type = "binary",
                    IsKey = sourceField.IsKey // _TYPE
                });
                targetValues.Add(Values[FieldPurpose.TypeCode]);
            }
        }
    }
}
