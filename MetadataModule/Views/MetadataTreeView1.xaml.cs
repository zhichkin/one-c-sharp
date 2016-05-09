using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using System.Reflection.Emit;

namespace Zhichkin.Metadata.Views
{
    public class TestClass
    {
        int i;
        string s;
        bool b;
        public TestClass(int i, string s, bool b)
        {
            this.i = i;
            this.s = s;
            this.b = b;
        }
        public override string ToString()
        {
            return i.ToString() + s + b.ToString();
        }
    }

    /// <summary>
    /// Interaction logic for MetadataTreeView.xaml
    /// </summary>
    public partial class MetadataTreeView1 : UserControl
    {
        public MetadataTreeView1()
        {
            InitializeComponent();
            tbHello.Text = typeof(IDataObject).FullName;

            Func<int, string, bool, TestClass> ctor = null;

            Type[] parameters = new Type[]
                {
                    typeof(int),
                    typeof(string),
                    typeof(bool)
                };

            var ctorInfo = typeof(TestClass).GetConstructor(parameters);
            var ctorInvoker = new DynamicMethod("Create" + typeof(TestClass).Name, typeof(TestClass), parameters);

            var ilGenerator = ctorInvoker.GetILGenerator();
            // Load the int input parameter onto the stack
            ilGenerator.Emit(OpCodes.Ldarg_0);
            ilGenerator.Emit(OpCodes.Ldarg_1);
            ilGenerator.Emit(OpCodes.Ldarg_2);
            // Call the constructor
            ilGenerator.Emit(OpCodes.Newobj, ctorInfo);
            // Return the result of calling the constructor
            ilGenerator.Emit(OpCodes.Ret);

            // Create a delegate for calling the constructor
            ctor = (Func<int, string, bool, TestClass>)ctorInvoker.CreateDelegate(typeof(Func<int, string, bool, TestClass>));

            TestClass test = ctor(5, " == 5 is ", true);

            tbHello.Text = test.ToString();
        }
    }
}
