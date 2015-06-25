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

        public override IEnumerable<IAnnotation> For(IProperty property)
        {
            var pk = property.EntityType.GetPrimaryKey()?.Properties;
            if (pk != null
                && pk.Count == 1
                && pk[0] == property)
            {
                var clrType = property.ClrType;
                string columnType;
                try
                {
                    columnType = property.Sqlite().ColumnType ?? _typeMapper.GetDefaultMapping(property.ClrType).DefaultTypeName;
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
        }
    }
}
