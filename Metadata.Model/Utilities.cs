using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Zhichkin.Metadata.Model
{
    public static class Utilities
    {
        public static FieldPurpose ParseFieldPurpose(string fieldName)
        {
            char L = char.Parse("L");
            char N = char.Parse("N");
            char T = char.Parse("T");
            char S = char.Parse("S");
            char B = char.Parse("B");

            char test = fieldName[fieldName.Count() - 1];

            if (char.IsDigit(test)) return FieldPurpose.Value;

            if (test == L)
            {
                return FieldPurpose.Boolean;
            }
            else if (test == N)
            {
                return FieldPurpose.Number;
            }
            else if (test == T)
            {
                return FieldPurpose.DateTime;
            }
            else if (test == S)
            {
                return FieldPurpose.String;
            }
            else if (test == B)
            {
                return FieldPurpose.Binary;
            }

            string TYPE = "TYPE";
            string TRef = "TRef";
            string RRef = "RRef";

            string postfix = fieldName.Substring(fieldName.Count() - 4);

            if (postfix == TYPE)
            {
                return FieldPurpose.Locator;
            }
            else if (postfix == TRef)
            {
                return FieldPurpose.TypeCode;
            }
            else if (postfix == RRef)
            {
                return FieldPurpose.Object;
            }

            return FieldPurpose.Value;
        }

        public static string GetErrorText()
        {
            // TODO сделать сообщения об ошибках DataMapper'ов более информативными
            return string.Empty;
        }
    }
}
