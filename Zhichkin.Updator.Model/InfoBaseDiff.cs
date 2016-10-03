using System;
using Zhichkin.Metadata.Model;

namespace Zhichkin.Updator.Model
{
    public sealed class InfoBaseDiff : DifferenceItemBase<InfoBase>
    {
        public InfoBaseDiff(DifferenceType type, InfoBase target, InfoBase source) : base(type, target, source) { }
        protected override void Insert()
        {
            throw new NotSupportedException();
        }
        protected override void Delete()
        {
            throw new NotSupportedException();
        }
        protected override void Update()
        {
            throw new NotSupportedException();
        }
    }
}
