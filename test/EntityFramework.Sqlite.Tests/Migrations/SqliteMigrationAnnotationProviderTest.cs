// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Metadata.ModelConventions;
using Microsoft.Data.Entity.Sqlite.Metadata;
using Xunit;

namespace Microsoft.Data.Entity.Sqlite.Migrations
{
    public class SqliteMigrationAnnotationProviderTest
    {
        private readonly ModelBuilder _modelBuilder;
        private readonly SqliteMigrationAnnotationProvider _provider;

        private readonly Annotation _autoincrement = new Annotation(SqliteAnnotationNames.Prefix + SqliteAnnotationNames.Autoincrement, true);

        public SqliteMigrationAnnotationProviderTest()
        {
            _modelBuilder = new ModelBuilder(new CoreConventionSetBuilder().CreateConventionSet(), new Model());
            _provider = new SqliteMigrationAnnotationProvider(new SqliteTypeMapper());
        }

        [Theory]
        [InlineData(typeof(string), false)]
        [InlineData(typeof(Guid), false)]
        [InlineData(typeof(byte), false)]
        [InlineData(typeof(byte[]), false)]
        [InlineData(typeof(bool), false)]
        [InlineData(typeof(char), false)]
        [InlineData(typeof(int), true)]
        [InlineData(typeof(uint), true)]
        [InlineData(typeof(long), true)]
        [InlineData(typeof(ulong), true)]
        [InlineData(typeof(short), true)]
        [InlineData(typeof(ushort),true)]
        public void Adds_Autoincrement(Type keyType, bool addsAnnotation)
        {
            _modelBuilder.Entity<Entity>(b =>
                {
                    b.Property(keyType, "PK");
                    b.Key("PK");
                });
            var property = _modelBuilder.Model.GetEntityType(typeof(Entity)).GetProperty("PK");

            var annotation = _provider.For(property).ToList();
            if (addsAnnotation)
            {
                Assert.Contains(annotation, a=>a.Name==_autoincrement.Name && (bool)a.Value);
            }
            else
            {
                Assert.DoesNotContain(annotation, a=>a.Name==_autoincrement.Name && (bool)a.Value);
            }
        }

        [Theory]
        [InlineData("int", false)]
        [InlineData("integer", true)]
        [InlineData("bool", false)]
        public void Adds_Autoincrement_by_column_type(string columnType, bool addsAnnotation)
        {
            _modelBuilder.Entity<Entity>(b =>
                {
                    b.Property(e => e.StringId).SqliteColumnType(columnType);
                    b.Key(e => e.StringId);
                });
            var property = _modelBuilder.Model.GetEntityType(typeof(Entity)).GetProperty("StringId");

            var annotation = _provider.For(property);
            if (addsAnnotation)
            {
                Assert.Contains(annotation, a=>a.Name==_autoincrement.Name && (bool)a.Value);
            }
            else
            {
                Assert.DoesNotContain(annotation, a=>a.Name==_autoincrement.Name && (bool)a.Value);
            }
        }

        private class Entity
        {
            public int Id { get; set; }
            public string StringId { get; set; }
        }
    }
}
