using System;
using System.IO;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using System.Reflection;
using System.ComponentModel;
using System.Transactions;
using System.Collections.Generic;

using Zhichkin.ORM;

namespace Zhichkin.Metadata.Model
{
    public partial class Entity
    {
        # region " Types "

        /// <summary>
        /// A null reference.
        /// </summary>
        [Description("A null reference.")]
        public static readonly Entity Empty;

        /// <summary>
        /// A general type representing any reference or value type not explicitly represented by another TypeCode.
        /// </summary>
        [Description("A general type representing any reference or value type not explicitly represented by another TypeCode.")]
        public static readonly Entity Object;

        /// <summary>
        /// A database null (column) value.
        /// </summary>
        [Description("A database null (column) value.")]
        public static readonly Entity DBNull;

        /// <summary>
        /// A simple type representing Boolean values of true or false.
        /// </summary>
        [Description("A simple type representing Boolean values of true or false.")]
        public static readonly Entity Boolean;

        /// <summary>
        /// An integral type representing unsigned 16-bit integers with values between 0 and 65535. The set of possible values for the Char type corresponds to the Unicode character set.
        /// </summary>
        [Description("An integral type representing unsigned 16-bit integers with values between 0 and 65535. The set of possible values for the Char type corresponds to the Unicode character set.")]
        public static readonly Entity Char;

        /// <summary>
        /// An integral type representing signed 8-bit integers with values between -128 and 127.
        /// </summary>
        [Description("An integral type representing signed 8-bit integers with values between -128 and 127.")]
        public static readonly Entity SByte;

        /// <summary>
        /// An integral type representing unsigned 8-bit integers with values between 0 and 255.
        /// </summary>
        [Description("An integral type representing unsigned 8-bit integers with values between 0 and 255.")]
        public static readonly Entity Byte;

        /// <summary>
        /// An integral type representing signed 16-bit integers with values between -32768 and 32767.
        /// </summary>
        [Description("An integral type representing signed 16-bit integers with values between -32768 and 32767.")]
        public static readonly Entity Int16;

        /// <summary>
        /// An integral type representing unsigned 16-bit integers with values between 0 and 65535.
        /// </summary>
        [Description("An integral type representing unsigned 16-bit integers with values between 0 and 65535.")]
        public static readonly Entity UInt16;

        /// <summary>
        /// An integral type representing signed 32-bit integers with values between -2147483648 and 2147483647.
        /// </summary>
        [Description("An integral type representing signed 32-bit integers with values between -2147483648 and 2147483647.")]
        public static readonly Entity Int32;

        /// <summary>
        /// An integral type representing unsigned 32-bit integers with values between 0 and 4294967295.
        /// </summary>
        [Description("An integral type representing unsigned 32-bit integers with values between 0 and 4294967295.")]
        public static readonly Entity UInt32;

        /// <summary>
        /// An integral type representing signed 64-bit integers with values between -9223372036854775808 and 9223372036854775807.
        /// </summary>
        [Description("An integral type representing signed 64-bit integers with values between -9223372036854775808 and 9223372036854775807.")]
        public static readonly Entity Int64;

        /// <summary>
        /// An integral type representing unsigned 64-bit integers with values between 0 and 18446744073709551615.
        /// </summary>
        [Description("An integral type representing unsigned 64-bit integers with values between 0 and 18446744073709551615.")]
        public static readonly Entity UInt64;

        /// <summary>
        /// A floating point type representing values ranging from approximately 1.5 x 10 -45 to 3.4 x 10 38 with a precision of 7 digits.
        /// </summary>
        [Description("A floating point type representing values ranging from approximately 1.5 x 10 -45 to 3.4 x 10 38 with a precision of 7 digits.")]
        public static readonly Entity Single;

        /// <summary>
        /// A floating point type representing values ranging from approximately 5.0 x 10 -324 to 1.7 x 10 308 with a precision of 15-16 digits.
        /// </summary>
        [Description("A floating point type representing values ranging from approximately 5.0 x 10 -324 to 1.7 x 10 308 with a precision of 15-16 digits.")]
        public static readonly Entity Double;

        /// <summary>
        /// A simple type representing values ranging from 1.0 x 10 -28 to approximately 7.9 x 10 28 with 28-29 significant digits.
        /// </summary>
        [Description("A simple type representing values ranging from 1.0 x 10 -28 to approximately 7.9 x 10 28 with 28-29 significant digits.")]
        public static readonly Entity Decimal;

        /// <summary>
        /// A type representing a date and time value.
        /// </summary>
        [Description("A type representing a date and time value.")]
        public static readonly Entity DateTime;

        /// <summary>
        /// A binary data type representing 16 byte array (represents a globally unique identifier).
        /// </summary>
        [Description("A binary data type representing 16 byte array (represents a globally unique identifier).")]
        public static readonly Entity GUID;

        /// <summary>
        /// A sealed class type representing Unicode character strings.
        /// </summary>
        [Description("A sealed class type representing Unicode character strings.")]
        public static readonly Entity String;

        /// <summary>
        /// Binary data represented by array of bytes.
        /// </summary>
        [Description("Binary data represented by array of bytes.")]
        public static readonly Entity Binary;

        # endregion

        static Entity()
        {
            QueryService service = new QueryService(MetadataPersistentContext.Current.ConnectionString);
            string sql = "SELECT [key], [name] FROM [entities] WHERE [namespace] = CAST(0x00000000000000000000000000000000 AS uniqueidentifier);";

            List<dynamic> list = service.Execute(sql);

            Entity type; FieldInfo field;
            foreach (dynamic item in list)
            {
                field = typeof(Entity).GetField(item.name, BindingFlags.Public | BindingFlags.Static);
                if (field == null) continue;
                
                type = new Entity(item.key, PersistentState.Virtual);
                field.SetValue(null, type);
            }
        }
    }
}