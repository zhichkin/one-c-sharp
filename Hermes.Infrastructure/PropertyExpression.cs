using System;
using System.Collections.Generic;
using System.Text;
using Zhichkin.Metadata.Model;

namespace Zhichkin.Hermes.Model
{
    public class PropertyExpression
    {
        public PropertyExpression() { }
        public string Alias { get; set; }
        public Property Property { get; set; }
    }
}
