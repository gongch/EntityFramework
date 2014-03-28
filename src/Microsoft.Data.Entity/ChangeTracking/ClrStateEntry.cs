// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.ChangeTracking
{
    public class ClrStateEntry : StateEntry
    {
        private object _entityOrValues;

        /// <summary>
        ///     This constructor is intended only for use when creating test doubles that will override members
        ///     with mocked or faked behavior. Use of this constructor for other purposes may result in unexpected
        ///     behavior including but not limited to throwing <see cref="NullReferenceException" />.
        /// </summary>
        protected ClrStateEntry()
        {
        }

        public ClrStateEntry([NotNull] ContextConfiguration configuration, [NotNull] IEntityType entityType, [NotNull] object entity)
            : base(configuration, entityType)
        {
            Check.NotNull(entity, "entity");

            _entityOrValues = entity;
        }

        public ClrStateEntry([NotNull] ContextConfiguration configuration, [NotNull] IEntityType entityType, [NotNull] object[] valueBuffer)
            : base(configuration, entityType)
        {
            Check.NotNull(valueBuffer, "valueBuffer");

            _entityOrValues = valueBuffer;
        }

        [NotNull]
        public override object Entity
        {
            get
            {
                // TODO: Consider: will we ever allow an entity type of object[]?
                var asValues = _entityOrValues as object[];

                if (asValues != null)
                {
                    _entityOrValues = Configuration.EntityMaterializerSource.GetMaterializer(EntityType)(asValues);
                    Configuration.StateManager.EntityMaterialized(this);
                }

                return _entityOrValues;
            }
        }

        public override object GetPropertyValue(IProperty property)
        {
            Check.NotNull(property, "property");

            var asValues = _entityOrValues as object[];

            if (asValues != null)
            {
                return asValues[property.Index];
            }

            return Configuration.ClrPropertyGetterSource.GetAccessor(property).GetClrValue(_entityOrValues);
        }

        public override void SetPropertyValue(IProperty property, object value)
        {
            Check.NotNull(property, "property");

            var asValues = _entityOrValues as object[];

            if (asValues != null)
            {
                asValues[property.Index] = value;
            }
            else
            {
                Configuration.ClrPropertySetterSource.GetAccessor(property).SetClrValue(_entityOrValues, value);
            }
        }
    }
}