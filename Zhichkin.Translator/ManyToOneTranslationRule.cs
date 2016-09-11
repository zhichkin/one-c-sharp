using System;
using Zhichkin.ChangeTracking;
using System.Collections.Generic;

namespace Zhichkin.Translator
{
    public class ManyToOneTranslationRule : SimpleTranslationRule
    {
        public string TypeCodeField = string.Empty;
        public string ObjectField = string.Empty;
        public object Value = null;
        public int TypeCodeValue = 0;
        public int TestTypeCode = 0;
        private bool value_is_set = false;
        private bool type_code_is_set = false;
        public override void Apply(ChangeTrackingField field, IList<ChangeTrackingField> target)
        {
            if (field.Name == ObjectField)
            {
                Value = field.Value;
                value_is_set = true;
            }
            else if (field.Name == TypeCodeField)
            {
                TypeCodeValue = Utilities.GetInt32((byte[])field.Value);
                type_code_is_set = true;
            }
            else // _TYPE
            {
                return;
            }
            if (value_is_set && type_code_is_set)
            {
                if (TestTypeCode == TypeCodeValue) // TEST: byte[4] ?
                {
                    target.Add(new ChangeTrackingField()
                    {
                        Name = Name,
                        Value = Value,
                        IsKey = field.IsKey
                    });
                }
                else
                {
                    target.Add(new ChangeTrackingField()
                    {
                        Name = Name,
                        Value = Guid.Empty, // TEST: byte[16] ?
                        IsKey = field.IsKey
                    });
                }
            }
        }
    }
}