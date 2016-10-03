using System;
using Zhichkin.Metadata.Model;
using Zhichkin.Metadata.Services;

namespace Zhichkin.Updator.Model
{
    public sealed class NamespaceDiff : DifferenceItemBase<Namespace>
    {
        private readonly MetadataService service = new MetadataService();
        public NamespaceDiff(DifferenceType type, Namespace target, Namespace source) : base(type, target, source) { }
        protected override void Insert()
        {
            Source.Owner = ((IDifferenceItem<InfoBase>)Owner).Target;
            service.Save(Source);
        }
        protected override void Delete()
        {
            Target.Kill();
        }
        protected override void Update()
        {
            base.Update();
        }
    }
}
