// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using Microsoft.Data.Entity.FunctionalTests;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Framework.DependencyInjection;
using Xunit;

namespace Microsoft.Data.Entity.Sqlite.FunctionalTests
{
    public class AutoincrementTest
    {
        private readonly DbContextOptions _options;
        private readonly IServiceProvider _provider;
        private readonly SqliteTestStore _testStore;

        [Fact]
        public void Autoincrement_prevents_reusing_rowid()
        {
            using (var context = CreateContext())
            {
                var hero = context.People.First(p => p.Id == 1);

                context.People.Remove(hero);
                context.SaveChanges();
                context.People.Add(new Person { Name = "Batman" });
                context.SaveChanges();
                var gone = context.People.FirstOrDefault(p => p.Id == 1);
                var begins = context.People.FirstOrDefault(p => p.Id == 2);

                Assert.Null(gone);
                Assert.NotNull(begins);
            }
        }

        public AutoincrementTest()
        {
            _testStore = SqliteTestStore.CreateScratch();

            var builder = new DbContextOptionsBuilder();
            builder.UseSqlite(_testStore.Connection);
            _options = builder.Options;
            _provider = new ServiceCollection()
                .AddEntityFramework()
                .AddSqlite()
                .AddDbContext<BatContext>()
                .ServiceCollection()
                .BuildServiceProvider();

            using (var context = CreateContext())
            {
                context.Database.EnsureCreated();
                context.People.Add(new Person { Name = "Bruce" });
                context.SaveChanges();
            }
        }

        private BatContext CreateContext() => new BatContext(_provider, _options);
    }

    public class BatContext : DbContext
    {
        public BatContext(IServiceProvider serviceProvider, DbContextOptions options)
            : base(serviceProvider, options)
        {
        }

        public DbSet<Person> People { get; set; }
    }

    public class Person
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
}
