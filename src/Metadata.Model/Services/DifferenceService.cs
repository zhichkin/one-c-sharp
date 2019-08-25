using System;
using System.Linq;
using System.Collections.Generic;
using Zhichkin.ORM;
using System.Transactions;

namespace Zhichkin.Metadata.Model
{
    public class DifferenceService : IDifferenceService
    {
        public IDifferenceObject Compare(InfoBase target, InfoBase source)
        {
            IDifferenceObject root = new DifferenceObject(null, target, DifferenceType.None);
            CompareLists<Namespace>(root, target.Namespaces.ToList(), source.Namespaces.ToList());
            return root;
        }
        private void CompareLists<T>(IDifferenceObject parent, List<T> target_list, List<T> source_list)
            where T : IPersistent, IComparable
        {
            int target_count = target_list.Count();
            int source_count = source_list.Count();
            int target_index = 0;
            int source_index = 0;
            int compareResult;

            if (target_count == 0 && source_count == 0) return;

            target_list.Sort();
            source_list.Sort();
            
            while (target_index < target_count)
            {
                if (source_index < source_count)
                {
                    compareResult = target_list[target_index].CompareTo(source_list[source_index]);
                    if (compareResult < 0)
                    {
                        DifferenceObject difference = new DifferenceObject(parent, target_list[target_index], DifferenceType.Delete);
                        parent.Children.Add(difference);
                        SetUpdateDifferenceType(parent);
                        AddChildren(difference);
                        target_index++;
                    }
                    else if (compareResult == 0)
                    {
                        DifferenceObject difference = new DifferenceObject(parent, target_list[target_index], DifferenceType.None);
                        CompareListItems(difference, target_list[target_index], source_list[source_index]);
                        if (difference.Difference == DifferenceType.Update) parent.Children.Add(difference);
                        target_index++;
                        source_index++;
                    }
                    else
                    {
                        DifferenceObject difference = new DifferenceObject(parent, source_list[source_index], DifferenceType.Insert);
                        parent.Children.Add(difference);
                        SetUpdateDifferenceType(parent);
                        AddChildren(difference);
                        source_index++;
                    }
                }
                else
                {
                    DifferenceObject difference = new DifferenceObject(parent, target_list[target_index], DifferenceType.Delete);
                    parent.Children.Add(difference);
                    SetUpdateDifferenceType(parent);
                    AddChildren(difference);
                    target_index++;
                }
            }
            while (source_index < source_count)
            {
                DifferenceObject difference = new DifferenceObject(parent, source_list[source_index], DifferenceType.Insert);
                parent.Children.Add(difference);
                SetUpdateDifferenceType(parent);
                AddChildren(difference);
                source_index++;
            }
        }
        // Compare methods is used by Update difference
        private void CompareListItems(IDifferenceObject difference, IPersistent target, IPersistent source)
        {
            if (typeof(Namespace) == target.GetType())
            {
                CompareNamespaces(difference, (Namespace)target, (Namespace)source);
            }
            else if (typeof(Entity) == target.GetType())
            {
                CompareEntities(difference, (Entity)target, (Entity)source);
            }
            else if (typeof(Property) == target.GetType())
            {
                CompareProperties(difference, (Property)target, (Property)source);
            }
            else if (typeof(Table) == target.GetType())
            {
                CompareTables(difference, (Table)target, (Table)source);
            }
            else if (typeof(Field) == target.GetType())
            {
                CompareFields(difference, (Field)target, (Field)source);
            }
        }
        private void CompareNamespaces(IDifferenceObject difference,  Namespace target, Namespace source)
        {
            CompareLists<Namespace>(difference, target.Namespaces.ToList(), source.Namespaces.ToList());
            CompareLists<Entity>(difference, target.Entities.ToList(), source.Entities.ToList());
        }
        private void CompareEntities(IDifferenceObject difference, Entity target, Entity source)
        {
            difference.NewValues.Clear();
            if (target.Code != source.Code)
            {
                difference.NewValues.Add("Code", source.Code);
            }
            if (target.Alias != source.Alias)
            {
                difference.NewValues.Add("Alias", source.Alias);
            }
            if (difference.NewValues.Count > 0) SetUpdateDifferenceType(difference);

            CompareLists<Property>(difference, target.Properties.ToList(), source.Properties.ToList());
            CompareLists<Entity>(difference, target.NestedEntities.ToList(), source.NestedEntities.ToList());
            CompareLists<Table>(difference, target.Tables.ToList(), source.Tables.ToList());
        }
        private void CompareProperties(IDifferenceObject difference, Property target, Property source)
        {
            difference.NewValues.Clear();
            if (target.Purpose != source.Purpose)
            {
                difference.NewValues.Add("Purpose", source.Purpose);
            }
            if (target.Ordinal != source.Ordinal)
            {
                difference.NewValues.Add("Ordinal", source.Ordinal);
            }
            if (difference.NewValues.Count > 0) SetUpdateDifferenceType(difference);

            CompareLists<Relation>(difference, target.Relations.ToList(), source.Relations.ToList());
        }
        private void CompareTables(IDifferenceObject difference, Table target, Table source)
        {
            difference.NewValues.Clear();
            if (target.Purpose != source.Purpose)
            {
                difference.NewValues.Add("Purpose", source.Purpose);
            }
            if (difference.NewValues.Count > 0) SetUpdateDifferenceType(difference);

            CompareLists<Field>(difference, target.Fields.ToList(), source.Fields.ToList());
        }
        private void CompareFields(IDifferenceObject difference, Field target, Field source)
        {
            difference.NewValues.Clear();
            if (target.IsNullable != source.IsNullable)
            {
                difference.NewValues.Add("IsNullable", source.IsNullable);
            }
            if (target.IsPrimaryKey != source.IsPrimaryKey)
            {
                difference.NewValues.Add("IsPrimaryKey", source.IsPrimaryKey);
            }
            if (target.KeyOrdinal != source.KeyOrdinal)
            {
                difference.NewValues.Add("KeyOrdinal", source.KeyOrdinal);
            }
            if (target.Length != source.Length)
            {
                difference.NewValues.Add("Length", source.Length);
            }
            if (target.Precision != source.Precision)
            {
                difference.NewValues.Add("Precision", source.Precision);
            }
            if (target.Purpose != source.Purpose)
            {
                difference.NewValues.Add("Purpose", source.Purpose);
            }
            if (target.Scale != source.Scale)
            {
                difference.NewValues.Add("Scale", source.Scale);
            }
            if (target.TypeName != source.TypeName)
            {
                difference.NewValues.Add("TypeName", source.TypeName);
            }
            if (difference.NewValues.Count > 0) SetUpdateDifferenceType(difference);
        }
        private void SetUpdateDifferenceType(IDifferenceObject difference)
        {
            if (difference.Difference != DifferenceType.None) return;
            difference.Difference = DifferenceType.Update;
            IDifferenceObject parent = difference.Parent;
            while (parent != null)
            {
                parent.Difference = DifferenceType.Update;
                parent = parent.Parent;
            }
        }
        // Add children is used by Insert and Delete differences
        private void AddChildren(IDifferenceObject difference)
        {
            Type t = difference.Target.GetType();
            if (typeof(Namespace) == t)
            {
                AddNamespaceChildren(difference);
            }
            else if (typeof(Entity) == t)
            {
                AddEntityChildren(difference);
            }
            else if (typeof(Property) == t)
            {
                AddPropertyChildren(difference);
            }
            else if (typeof(Relation) == t)
            {
                SetupRelationParent(difference);
            }
            else if (typeof(Table) == t)
            {
                AddTableChildren(difference);
            }
            else if (typeof(Field) == t)
            {
                SetupFieldParent(difference);
            }
        }
        private void AddNamespaceChildren(IDifferenceObject difference)
        {
            Namespace target = (Namespace)difference.Target;
            SetupNamespaceParent(difference);
            foreach (Namespace child in target.Namespaces)
            {
                DifferenceObject diff = new DifferenceObject(difference, child, difference.Difference);
                difference.Children.Add(diff);
                AddNamespaceChildren(diff);
            }
            foreach (Entity child in target.Entities)
            {
                DifferenceObject diff = new DifferenceObject(difference, child, difference.Difference);
                difference.Children.Add(diff);
                AddEntityChildren(diff);
            }
        }
        private void AddEntityChildren(IDifferenceObject difference)
        {
            Entity target = (Entity)difference.Target;
            SetupEntityParent(difference);
            foreach (Property child in target.Properties)
            {
                DifferenceObject diff = new DifferenceObject(difference, child, difference.Difference);
                difference.Children.Add(diff);
                AddPropertyChildren(diff);
            }
            foreach (Entity child in target.NestedEntities)
            {
                DifferenceObject diff = new DifferenceObject(difference, child, difference.Difference);
                difference.Children.Add(diff);
                AddEntityChildren(diff);
            }
            foreach (Table child in target.Tables)
            {
                DifferenceObject diff = new DifferenceObject(difference, child, difference.Difference);
                difference.Children.Add(diff);
                AddTableChildren(diff);
            }
        }
        private void AddPropertyChildren(IDifferenceObject difference)
        {
            Property target = (Property)difference.Target;
            SetupPropertyParent(difference);
            foreach (Relation child in target.Relations)
            {
                DifferenceObject diff = new DifferenceObject(difference, child, difference.Difference);
                SetupRelationParent(diff);
                difference.Children.Add(diff);
            }
        }
        private void AddTableChildren(IDifferenceObject difference)
        {
            Table target = (Table)difference.Target;
            SetupTableParent(difference);
            foreach (Field child in target.Fields)
            {
                DifferenceObject diff = new DifferenceObject(difference, child, difference.Difference);
                SetupFieldParent(diff);
                difference.Children.Add(diff);
            }
        }
        // Setup parent values for Insert difference
        private void SetupNamespaceParent(IDifferenceObject difference)
        {
            if (difference.Difference == DifferenceType.Insert
                && difference.Parent.Difference == DifferenceType.Update)
            {
                Namespace target = (Namespace)difference.Target;

                if (difference.Parent.Target.GetType() == typeof(InfoBase))
                {
                    target.Owner = (InfoBase)difference.Parent.Target;
                }
                else if (difference.Parent.Target.GetType() == typeof(Namespace))
                {
                    target.Owner = ((Namespace)difference.Parent.Target).Owner;
                }
            }
        }
        private void SetupEntityParent(IDifferenceObject difference)
        {
            if (difference.Difference == DifferenceType.Insert
                && difference.Parent.Difference == DifferenceType.Update)
            {
                Entity target = (Entity)difference.Target;

                if (difference.Parent.Target.GetType() == typeof(Namespace))
                {
                    target.Namespace = (Namespace)difference.Parent.Target;
                }
                else if (difference.Parent.Target.GetType() == typeof(Entity))
                {
                    target.Owner = (Entity)difference.Parent.Target;
                }
            }
        }
        private void SetupPropertyParent(IDifferenceObject difference)
        {
            if (difference.Difference == DifferenceType.Insert
                && difference.Parent.Difference == DifferenceType.Update)
            {
                Property target = (Property)difference.Target;
                target.Entity = (Entity)difference.Parent.Target;
            }
        }
        private void SetupRelationParent(IDifferenceObject difference)
        {
            if (difference.Difference == DifferenceType.Insert)
            {
                Relation target = (Relation)difference.Target;
                target.Property = (Property)difference.Parent.Target;
                if (target.Entity.Namespace != Namespace.TypeSystem)
                {
                    Entity target_entity = target.Property.Entity.Namespace.Entities
                        .Where(e => e.Name == target.Entity.Name).FirstOrDefault();
                    if (target_entity != null)
                    {
                        // Соответствующая сущность уже есть в InfoBase
                        target.Entity = target_entity;
                    }
                }
            }
        }
        private void SetupTableParent(IDifferenceObject difference)
        {
            if (difference.Difference == DifferenceType.Insert
                && difference.Parent.Difference == DifferenceType.Update)
            {
                Table target = (Table)difference.Target;
                target.Entity = (Entity)difference.Parent.Target;
            }
        }
        private void SetupFieldParent(IDifferenceObject difference)
        {
            if (difference.Difference == DifferenceType.Insert
                && difference.Parent.Difference == DifferenceType.Update)
            {
                Field target = (Field)difference.Target;
                target.Table = (Table)difference.Parent.Target;
            }
        }

        public void Apply(IDifferenceObject differences)
        {
            if (differences.Difference == DifferenceType.None) return;
            TransactionOptions options = new TransactionOptions() { IsolationLevel = IsolationLevel.ReadCommitted };
            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, options))
            {
                this.ApplyRecursively(differences);
                scope.Complete();
            }   
        }
        public void ApplyRecursively(IDifferenceObject difference)
        {
            if (difference.Difference == DifferenceType.None) return;

            if (difference.Difference == DifferenceType.Insert
                || difference.Difference == DifferenceType.Update)
            {
                difference.Apply();
                foreach (IDifferenceObject child in difference.Children)
                {
                    this.ApplyRecursively(child);
                }
            }
            else if (difference.Difference == DifferenceType.Delete)
            {
                foreach (IDifferenceObject child in difference.Children)
                {
                    this.ApplyRecursively(child);
                }
                difference.Apply();
            }
        }
    }
}
