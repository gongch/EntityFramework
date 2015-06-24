// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Relational;
using Microsoft.Data.Entity.Relational.Migrations.Infrastructure;
using Microsoft.Data.Entity.Sqlite.Metadata;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Sqlite.Migrations
{
    public class SqliteMigrationAnnotationProvider : MigrationAnnotationProvider
    {
        private readonly IRelationalTypeMapper _typeMapper;

        public SqliteMigrationAnnotationProvider([NotNull] IRelationalTypeMapper typeMapper)
        {
            Check.NotNull(typeMapper, nameof(typeMapper));

            _typeMapper = typeMapper;
        }

        public override IEnumerable<IAnnotation> For(IKey key)
        {
            if (key.IsPrimaryKey()
                && key.Properties.Count == 1)
            {
                var clrType = key.Properties[0].ClrType;
                string columnType;
                try
                {
                    columnType = key.Properties[0].Sqlite().ColumnType ?? _typeMapper.GetDefaultMapping(key.Properties[0].ClrType).DefaultTypeName;
                }
                catch (NotSupportedException)
                {
                    columnType = null;
                }
                if (columnType != null
                    && columnType.Equals("integer", StringComparison.CurrentCultureIgnoreCase)
                    && clrType != typeof(bool)
                    && clrType != typeof(char)
                    && clrType != typeof(byte)
                    )
                {
                    yield return new Annotation(SqliteAnnotationNames.Prefix + SqliteAnnotationNames.Autoincrement, true);
                }
            }
            base.For(key);
        }

        public override IEnumerable<IAnnotation> For(IProperty property)
        {
            if (property.IsPrimaryKey()
                && property.EntityType.GetPrimaryKey().Properties.Count == 1)
            {
                yield return new Annotation(SqliteAnnotationNames.Prefix + SqliteAnnotationNames.InlinePrimaryKey, true);
            }
            base.For(property);
        }
    }
}
