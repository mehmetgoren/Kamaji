﻿namespace Kamaji.Data.Mongo
{
    using ionix.Data.Mongo;
    using ionix.Data.Mongo.Migration;
    using System;
    using System.Linq;
    using System.Reflection;
    using System.Text;

    public abstract class MigrationBase : Migration
    {
        protected static string GetDefaultGenerateMigrationScript(MigrationBase migration)
        {
            if (null == migration)
                throw new ArgumentNullException(nameof(migration));

            var libAssembly = migration.GetMigrationsAssembly();

            StringBuilder sb = new StringBuilder();
            var types = libAssembly.GetTypes()
                .ToList();
            types.ForEach(type => sb.Append(migration.GetModelCreateScript(type)));
            return sb.ToString();
        }

        protected MigrationBase(MigrationVersion version)
            : base(version)
        {
        }


        public virtual Assembly GetMigrationsAssembly()
        {
            return this.GetType().GetTypeInfo().Assembly;
        }


        public sealed override void Update()
        {
            if (String.IsNullOrEmpty(this.Script))
            {
                throw new InvalidOperationException("MigrationBase.GenerateMigrationScript() method should not returns null or empty script");
            }

            MongoAdmin.ExecuteScript(this.Database, this.Script);
        }

        private string _script;
        public sealed override string Script
        {
            get
            {
                if (_script == null)
                {
                    _script = GenerateMigrationScript();
                }
                return _script;
            }
        }
        protected bool AlsoCheckMigrationVersion { get; set; } = true;

        protected virtual StringBuilder GetModelCreateScript(Type type)
        {
            StringBuilder sb = new StringBuilder();

            var typeInfo = type.GetTypeInfo();
            bool flag = !this.AlsoCheckMigrationVersion;
            if (!flag)
            {
                var migrationVersionAttr = typeInfo.GetCustomAttribute<MigrationVersionAttribute>();
                if (null != migrationVersionAttr && !String.IsNullOrEmpty(migrationVersionAttr.MigrationVersion))
                {
                    string migrationVersion = migrationVersionAttr.MigrationVersion;
                    flag = !String.IsNullOrEmpty(migrationVersion) && new MigrationVersion(migrationVersion) == this.Version;
                }
            }
            if (flag)
            {
                var collAttr = typeInfo.GetCustomAttribute<MongoCollectionAttribute>();
                if (null != collAttr)
                {
                    string script = collAttr.Script(type);
                    sb.Append(script).Append("; ");
                }

                //default index
                var indexAttrList = typeInfo.GetCustomAttributes<MongoIndexAttribute>();
                if (null != indexAttrList)
                {
                    foreach (var script in indexAttrList.Scripts(type))
                    {

                        sb.Append(script).Append("; ");
                    }
                }

                //text Index
                var textIndexAttr = typeInfo.GetCustomAttribute<MongoTextIndexAttribute>();
                if (null != textIndexAttr)
                {
                    string script = textIndexAttr.Script(type);
                    sb.Append(script).Append("; ");
                }

            }

            return sb;
        }

        //Template Method PAttern
        public abstract string GenerateMigrationScript();
    }

    public class Migration100 : MigrationBase
    {
        public const string VersionNo = "1.0.0";

        public Migration100()
            : base(VersionNo)
        {
        }

        public override string GenerateMigrationScript()
        {
            var libAssembly = GetMigrationsAssembly();

            StringBuilder sb = new StringBuilder();
            libAssembly.GetTypes() .ToList().ForEach(type => sb.Append(GetModelCreateScript(type)));
            return sb.ToString();
        }
    }
}
