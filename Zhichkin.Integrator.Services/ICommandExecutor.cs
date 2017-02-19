using System;
using System.Xml;
using System.Data.SqlClient;
using Zhichkin.Integrator.Model;

namespace Zhichkin.Integrator.Services
{
    public interface ICommandExecutor
    {
        void Execute(SqlCommand command);
    }

    public sealed class CommandExecutor : ICommandExecutor
    {
        public CommandExecutor() { }
        public XmlWriter Writer { set; get; }
        public AggregateItem Item { set; get; }
        public Subscription Context { set; get; }
        public Guid Aggregate { set; get; }
        public Action<Subscription, AggregateItem, XmlWriter, Guid, SqlCommand> Action { set; get; }
        public void Execute(SqlCommand command) { Action(Context, Item, Writer, Aggregate, command); }
    }
}
