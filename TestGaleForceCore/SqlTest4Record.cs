namespace TestGaleForceCore
{
    using System;
    using System.Linq;
    using GaleForceCore.Builders;
    public class SqlTest4Record : SqlTest3Record
    {
        public new const string TableName = "TableName4";

        [IgnoreField]
        public string StringExtra { get; set; }
    }
}
