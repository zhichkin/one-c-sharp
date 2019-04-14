using System;
using System.Collections.Generic;
using System.Text;

namespace Zhichkin.Hermes.Model
{
    public abstract class HermesModel
    {
        public HermesModel(HermesModel consumer)
        {
            this.Consumer = consumer;
        }
        public HermesModel Consumer { get; set; }
    }
}
