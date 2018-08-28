namespace ReliableQueue.SQLite
{
    using ionix.Migration;
    using System;
    using System.Collections.Generic;
    using System.Reflection;

    internal sealed class Migration100 : MigrationAutoGen
    {
        internal const string VersionNo = "1.0.0";

        public Migration100() :
            base(VersionNo)
        {
        }

        protected override IEnumerable<Type> GetMigrationTypes()
        {
            return Assembly.GetExecutingAssembly().GetTypes();
        }
    }
}
