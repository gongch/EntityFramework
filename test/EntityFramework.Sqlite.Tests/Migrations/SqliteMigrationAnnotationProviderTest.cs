// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
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
            var key = _modelBuilder.Model.GetEntityType(typeof(Entity)).GetPrimaryKey();

            var annotation = _provider.For(key);
            if (addsAnnotation)
            {
                Assert.Collection(annotation, a =>
                    {
                        Assert.Equal(a.Name, SqliteAnnotationNames.Prefix + SqliteAnnotationNames.Autoincrement);
                        Assert.True((bool)a.Value);
                    });
            }
            else
            {
                Assert.Collection(annotation);
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
            var key = _modelBuilder.Model.GetEntityType(typeof(Entity)).GetPrimaryKey();

            var annotation = _provider.For(key);
            if (addsAnnotation)
            {
                Assert.Collection(annotation, a =>
                {
                    Assert.Equal(a.Name, SqliteAnnotationNames.Prefix + SqliteAnnotationNames.Autoincrement);
                    Assert.True((bool)a.Value);
                });
            }
            else
            {
                Assert.Collection(annotation);
            }
        }

        [Fact]
        public void Adds_InlinePrimaryKey_annotation()
        {
            _modelBuilder.Entity<Entity>().Key(e => e.Id);
            var property = _modelBuilder.Model.GetEntityType(typeof(Entity)).GetProperty("Id");

            var annotation = _provider.For(property);

            Assert.Collection(annotation, a =>
                {
                    Assert.Equal(a.Name, SqliteAnnotationNames.Prefix + SqliteAnnotationNames.InlinePrimaryKey);
                    Assert.True((bool)a.Value);
                });
        }

        [Fact]
        public void InlinePrimaryKey_not_added()
        {
            _modelBuilder.Entity<Entity>().Key(e => new { e.Id, e.StringId });

            var property = _modelBuilder.Model.GetEntityType(typeof(Entity)).GetProperty("Id");

            var annotation = _provider.For(property);

            Assert.Collection(annotation);
        }

        private class Entity
        {
            public int Id { get; set; }
            public string StringId { get; set; }
        }
    }
}
