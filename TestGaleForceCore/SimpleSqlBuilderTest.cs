namespace TestGaleForceCore
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using GaleForceCore.Builders;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Defines test class TestSqlUtils.
    /// </summary>
    [TestClass]
    public class SimpleSqlBuilderTest
    {
        [TestMethod]
        public void TestExecute1()
        {
            var data = GetData();

            var actual = new SimpleSqlBuilder<SqlTestRecord>()
                .From("TableName")
                .Select(r => r.Int1, r => r.Int2)
                .Where(r => r.Int2 == 102)
                .Execute(data);

            Assert.AreEqual(2, actual.Count(), "Wrong number of records");
            Assert.IsTrue(actual.All(r => r.Int2 == 102), "Where clause failed");
            Assert.IsTrue(actual.All(r => string.IsNullOrEmpty(r.String1)), "Select clause failed");
        }

        [TestMethod]
        public void TestExecute2()
        {
            var data = GetData();

            var actual = new SimpleSqlBuilder<SqlTestRecord>()
                .From("TableName")
                .Select(r => r.Int1, r => r.Int2, r => r.String1)
                .OrderBy(r => r.String1)
                .Execute(data);

            Assert.AreEqual(5, actual.Count(), "Wrong number of records");
            Assert.AreEqual(
                "String022,String111,String112,String123,String132",
                String.Join(",", actual.Select(a => a.String1)),
                "Order failed");
        }

        [TestMethod]
        public void TestExecute3()
        {
            var data = GetData();

            var actual = new SimpleSqlBuilder<SqlTestRecord>()
                .From("TableName")
                .Select(r => r.Int1, r => r.Int2, r => r.String1)
                .OrderBy(r => r.Int2)
                .ThenByDescending(r => r.String1)
                .Take(4)
                .Execute(data);

            Assert.AreEqual(4, actual.Count(), "Wrong number of records - Take failed");
            Assert.AreEqual(
                "String132,String112,String111,String022",
                String.Join(",", actual.Select(a => a.String1)),
                "Order failed");
        }

        /// <summary>
        /// Tests Select.
        /// </summary>
        [TestMethod]
        public void TestSelect()
        {
            var actual = new SimpleSqlBuilder<SqlTestRecord>()
                .From("TableName")
                .Select(r => r.DateTime1)
                .OrderByDescending(r => r.DateTime1)
                .Take(1)
                .Build();

            var expected = "SELECT TOP 1 DateTime1 FROM TableName ORDER BY DateTime1 DESC";

            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// Tests Select with Where, Contains.
        /// </summary>
        [TestMethod]
        public void TestSelectFields()
        {
            var actual = new SimpleSqlBuilder<SqlTestRecord>()
                .From("TableName")
                .Select(r => r.Int1, r => r.Int2)
                .Build();

            var expected = "SELECT Int1,Int2 FROM TableName";

            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// Tests Select with Where.
        /// </summary>
        [TestMethod]
        public void TestSelectWhereBool()
        {
            var actual = new SimpleSqlBuilder<SqlTestRecord>()
                .From("TableName")
                .Select(r => r.Int1)
                .OrderByDescending(r => r.Int2)
                .Take(10)
                .Where(r => r.Bool1)
                .Build();

            var expected = "SELECT TOP 10 Int1 FROM TableName WHERE Bool1 = 1 ORDER BY Int2 DESC";

            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// Tests Select with Where.
        /// </summary>
        [TestMethod]
        public void TestSelectWhereDateTime()
        {
            var dt = DateTime.Parse("2022-09-10T11:12:13Z", null, DateTimeStyles.RoundtripKind);
            var actual = new SimpleSqlBuilder<SqlTestRecord>()
                .From("TableName")
                .Select(r => r.Int1)
                .OrderByDescending(r => r.Int2)
                .Take(10)
                .Where(r => r.DateTime1 < dt)
                .Build();

            var expected = "SELECT TOP 10 Int1 FROM TableName WHERE (DateTime1 < '2022-09-10T11:12:13.0000000Z') ORDER BY Int2 DESC";

            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// Tests Select with Where.
        /// </summary>
        [TestMethod]
        public void TestSelectWhereDateTime2()
        {
            DateTime? aDate = new DateTime(2022, 01, 01).AddDays(1);
            var actual = new SimpleSqlBuilder<SqlTestRecord>()
                .From("TableName")
                .Select(r => r.Int1)
                .OrderByDescending(r => r.Int2)
                .Take(10)
                .Where(r => r.DateTime2 > aDate)
                .Build();

            var expected = "SELECT TOP 10 Int1 FROM TableName WHERE (DateTime2 > '2022-01-02T00:00:00.0000000') ORDER BY Int2 DESC";

            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// Tests Select with Where.
        /// </summary>
        [TestMethod]
        public void TestSelectWhereDateTimeDirect()
        {
            var actual = new SimpleSqlBuilder<SqlTestRecord>()
                .From("TableName")
                .Select(r => r.Int1)
                .OrderByDescending(r => r.Int2)
                .Take(10)
                .Where(r => r.DateTime1 < DateTime.Parse("2022-09-10T11:12:13Z", null, DateTimeStyles.RoundtripKind))
                .Build();

            var expected = "SELECT TOP 10 Int1 FROM TableName WHERE (DateTime1 < '2022-09-10T11:12:13.0000000Z') ORDER BY Int2 DESC";

            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// Tests Select with Where.
        /// </summary>
        [TestMethod]
        public void TestSelectWhereInts()
        {
            var actual = new SimpleSqlBuilder<SqlTestRecord>()
                .From("TableName")
                .Select(r => r.Int1)
                .OrderByDescending(r => r.Int2)
                .Take(10)
                .Where(r => r.Int1 > 10 && r.Int2 < 20 && r.Int2 - r.Int1 > 2)
                .Build();

            var expected = "SELECT TOP 10 Int1 FROM TableName WHERE (((Int1 > 10) AND (Int2 < 20)) AND ((Int2 - Int1) > 2)) ORDER BY Int2 DESC";

            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// Tests Select with Where.
        /// </summary>
        [TestMethod]
        public void TestSelectWhereString()
        {
            var actual = new SimpleSqlBuilder<SqlTestRecord>()
                .From("TableName")
                .Select(r => r.Int1)
                .OrderByDescending(r => r.Int2)
                .Take(10)
                .Where(r => r.String1 == "ABC")
                .Build();

            var expected = "SELECT TOP 10 Int1 FROM TableName WHERE (String1 = 'ABC') ORDER BY Int2 DESC";

            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// Tests Select with Where, Contains.
        /// </summary>
        [TestMethod]
        public void TestSelectWhereStringContains()
        {
            var actual = new SimpleSqlBuilder<SqlTestRecord>()
                .From("TableName")
                .Select(r => r.Int1)
                .OrderByDescending(r => r.Int2)
                .Take(10)
                .Where(r => r.String1.Contains("ABC"))
                .Build();

            var expected = "SELECT TOP 10 Int1 FROM TableName WHERE String1 LIKE '%ABC%' ORDER BY Int2 DESC";

            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// Tests Select with Where, Contains.
        /// </summary>
        [TestMethod]
        public void TestSelectWhereStringContainsLocalVars()
        {
            var abcdefg = "ABCDEFG";
            var abc = "ABC";
            var actual = new SimpleSqlBuilder<SqlTestRecord>()
                .From("TableName")
                .Select(r => r.Int1)
                .OrderByDescending(r => r.Int2)
                .Take(10)
                .Where(r => abcdefg.StartsWith(abc))
                .Build();

            var expected = "SELECT TOP 10 Int1 FROM TableName WHERE 'ABCDEFG' LIKE 'ABC%' ORDER BY Int2 DESC";

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestWhereBool()
        {
            var user = "abc@def.com";
            var actual = new SimpleSqlBuilder<SqlTestRecord>()
                .From("TableName")
                .Select(r => r.Bool1)
                .Where(r => r.String1 == user && r.Bool1)
                .Take(1)
                .Build();

            var expected = "SELECT TOP 1 Bool1 FROM TableName WHERE ((String1 = 'abc@def.com') AND Bool1 = 1)";

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestWhereBoolFalse()
        {
            var user = "abc@def.com";
            var actual = new SimpleSqlBuilder<SqlTestRecord>()
                .From("TableName")
                .Select(r => r.Int1)
                .Where(r => r.String1 == user && !r.Bool1)
                .Take(1)
                .Build();

            var expected = "SELECT TOP 1 Int1 FROM TableName WHERE ((String1 = 'abc@def.com') AND Bool1 = 0)";

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestWhereBoolCompare()
        {
            var user = "abc@def.com";
            var actual = new SimpleSqlBuilder<SqlTestRecord>()
                .From("TableName")
                .Select(r => r.Int1)
                .Where(r => r.String1 == user && r.Bool1 == r.Bool1)
                .Take(1)
                .Build();

            var expected = "SELECT TOP 1 Int1 FROM TableName WHERE ((String1 = 'abc@def.com') AND (Bool1 = Bool1))";

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestWhereNotBoolCompare()
        {
            var user = "abc@def.com";
            var actual = new SimpleSqlBuilder<SqlTestRecord>()
                .From("TableName")
                .Select(r => r.Int1)
                .Where(r => !r.Bool1)
                .Take(1)
                .Build();

            var expected = "SELECT TOP 1 Int1 FROM TableName WHERE Bool1 = 0";

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestWhereAdditive()
        {
            var user = "abc@def";
            var actual = new SimpleSqlBuilder<SqlTestRecord>()
                .From("TableName")
                .Select(r => r.Int1)
                .Where(r => r.String1 == user + ".com")
                .Take(1)
                .Build();

            var expected = "SELECT TOP 1 Int1 FROM TableName WHERE (String1 = CONCAT('abc@def','.com'))";

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestWhereAdditive3()
        {
            var user = "abc@def";
            var actual = new SimpleSqlBuilder<SqlTestRecord>()
                .From("TableName")
                .Select(r => r.Int1)
                .Where(r => r.String1 == user + "." + "com")
                .Take(1)
                .Build();

            var expected = "SELECT TOP 1 Int1 FROM TableName WHERE (String1 = CONCAT(CONCAT('abc@def','.'),'com'))";

            // todo: collect param list and make one concat stringset

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestWhereAdditiveWithField()
        {
            var user = "abc@def";
            var actual = new SimpleSqlBuilder<SqlTestRecord>()
                .From("TableName")
                .Select(r => r.Int1)
                .Where(r => r.String1 == r.String2 + ".com")
                .Take(1)
                .Build();

            var expected = "SELECT TOP 1 Int1 FROM TableName WHERE (String1 = CONCAT(String2,'.com'))";

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestInnerJoin1()
        {
            var actual = new SimpleSqlBuilder<SqlTestNewRecord, SqlTestRecord, SqlTestRecord>()
                .From("TableName", "TableName2")
                .SelectAs(tResult => tResult.Int3, (t1, t2) => t1.Int1 + t2.Int2)
                .InnerJoinOn((TableName, TableName2) => TableName.String1 == TableName2.String1)
                .Build();

            var expected = "SELECT (TableName.Int1 + TableName2.Int2) AS Int3 FROM TableName INNER JOIN TableName2 ON (TableName.String1 = TableName2.String1)";

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestInnerJoinWithWhere()
        {
            var actual = new SimpleSqlBuilder<SqlTestNewRecord, SqlTestRecord, SqlTestRecord>()
                .From("TableName", "TableName2")
                .SelectAs(tResult => tResult.Int3, (t1, t2) => t1.Int1 + t2.Int2)
                .InnerJoinOn((TableName, TableName2) => TableName.String1 == TableName2.String1)
                .Where(tResult => tResult.Int3 > 0)
                .Build();

            var expected = "SELECT (TableName.Int1 + TableName2.Int2) AS Int3 FROM TableName INNER JOIN TableName2 ON (TableName.String1 = TableName2.String1) WHERE (Int3 > 0)";

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestInnerJoinSelect()
        {
            var actual = new SimpleSqlBuilder<SqlTestNewRecord, SqlTestRecord, SqlTestRecord>()
                .From("TableName", "TableName2")
                .Select((t1, t2) => t1.Int1)
                .InnerJoinOn((TableName, TableName2) => TableName.String1 == TableName2.String1)
                .Where(tResult => tResult.Int3 > 0)
                .Build();

            var expected = "SELECT TableName.Int1 FROM TableName INNER JOIN TableName2 ON (TableName.String1 = TableName2.String1) WHERE (Int3 > 0)";

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestWhere2()
        {
            var actual = new SimpleSqlBuilder<SqlTestNewRecord, SqlTestRecord, SqlTestRecord>()
                .From("TableName", "TableName2")
                .InnerJoinOn((TableName, TableName2) => TableName.String1 == TableName2.String1)
                .Where((m, t) => m.String1 == "String123")
                .Select((m, t) => m.Int1)
                .Build();

            var expected = "SELECT TableName.Int1 FROM TableName INNER JOIN TableName2 ON (TableName.String1 = TableName2.String1) WHERE (TableName.String1 = 'String123')";
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestInnerJoinStringCompare()
        {
            var data = GetData();

            var actual = new SimpleSqlBuilder<SqlTestNewRecord, SqlTestRecord, SqlTestRecord>()
                .From("TableName", "TableName2")
                .InnerJoinOn((TableName, TableName2) => TableName.String1 == TableName2.String1)
                .Where((m, t) => m.String1 == data[0].String1)
                .Select((m, t) => m.Int1)
                .Build();

            var expected = "SELECT TableName.Int1 FROM TableName INNER JOIN TableName2 ON (TableName.String1 = TableName2.String1) WHERE (TableName.String1 = 'String123')";
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestJoinTake()
        {
            var data = GetData();

            var actual = new SimpleSqlBuilder<SqlTestNewRecord, SqlTestRecord, SqlTestRecord>()
                .From("TableName", "TableName2")
                .Take(10)
                .InnerJoinOn((TableName, TableName2) => TableName.String1 == TableName2.String1)
                .Where((m, t) => m.String1 == data[0].String1)
                .Select((m, t) => m.Int1)
                .Build();

            var expected = "SELECT TOP 10 TableName.Int1 FROM TableName INNER JOIN TableName2 ON (TableName.String1 = TableName2.String1) WHERE (TableName.String1 = 'String123')";
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestSelectAndSelectAsDifferentTable()
        {
            var data = GetData();

            var actual = new SimpleSqlBuilder<SqlTestNewRecord, SqlTestRecord, SqlTestRecord>()
                .From("TableName", "TableName2")
                .Select((m, t) => m.Int1)
                .SelectAs(m => m.Int3, (m, t) => m.Int2)
                .InnerJoinOn((TableName, TableName2) => TableName.String1 == TableName2.String1)
                .Where((m, t) => m.String1 == data[0].String1)
                .Build();

            var expected = "SELECT TableName.Int1,TableName.Int2 AS Int3 FROM TableName INNER JOIN TableName2 ON (TableName.String1 = TableName2.String1) WHERE (TableName.String1 = 'String123')";
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestSelectAndSelectAsSameTable()
        {
            var data = GetData();

            var actual = new SimpleSqlBuilder<SqlTestRecord, SqlTestRecord, SqlTestRecord>()
                .From("TableName", "TableName2")
                .Select((m, t) => m.Int1)
                .SelectAs(m => m.Int3, (m, t) => m.Int2)
                .InnerJoinOn((TableName, TableName2) => TableName.String1 == TableName2.String1)
                .Where((m, t) => m.String1 == data[0].String1)
                .Build();

            var expected = "SELECT TableName.Int1,TableName.Int2 AS Int3 FROM TableName INNER JOIN TableName2 ON (TableName.String1 = TableName2.String1) WHERE (TableName.String1 = 'String123')";
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestInListInts()
        {
            var data = GetData();

            var intList = new List<int>() { 1, 2, 3 };

            var actual = new SimpleSqlBuilder<SqlTestRecord>()
                .From("TableName")
                .Select(a => a.Int1)
                .Where(a => intList.Contains(a.Int1))
                .Build();

            var expected = "SELECT Int1 FROM TableName WHERE Int1 IN (1,2,3)";
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestInListStrings()
        {
            var data = GetData();

            var stringList = new List<string>() { "a", "b", "c" };

            var actual = new SimpleSqlBuilder<SqlTestRecord>()
                .From("TableName")
                .Select(a => a.Int1)
                .Where(a => stringList.Contains(a.String1))
                .Build();

            var expected = "SELECT Int1 FROM TableName WHERE String1 IN ('a','b','c')";
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestOuterJoinSelect()
        {
            var actual = new SimpleSqlBuilder<SqlTestNewRecord, SqlTestRecord, SqlTestRecord>()
                .From("TableName", "TableName2")
                .Select((t1, t2) => t1.Int1)
                .LeftOuterJoinOn((TableName, TableName2) => TableName.Int1 == TableName2.Int1)
                .Where((t1, t2) => t1.String1 != null)
                .Build();

            var expected = "SELECT TableName.Int1 FROM TableName " +
                "LEFT OUTER JOIN TableName2 ON (TableName.Int1 = TableName2.Int1) " +
                "WHERE (TableName.String1 IS NOT NULL)";

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestOuterJoinMultiSelect()
        {
            var actual = new SimpleSqlBuilder<SqlTestNewRecord, SqlTestRecord, SqlTestRecord>()
                .From("TableName", "TableName2")
                .Select((t1, t2) => t1.Int1, (t1, t2) => t2.String1)
                .LeftOuterJoinOn((TableName, TableName2) => TableName.Int1 == TableName2.Int1)
                .Where((t1, t2) => t1.String1 != null)
                .Build();

            var expected = "SELECT TableName.Int1,TableName2.String1 FROM TableName " +
                "LEFT OUTER JOIN TableName2 ON (TableName.Int1 = TableName2.Int1) " +
                "WHERE (TableName.String1 IS NOT NULL)";

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestOuterJoinMultiSelect3()
        {
            var actual = new SimpleSqlBuilder<SqlTestNewRecord, SqlTestRecord, SqlTestRecord, SqlTestRecord>()
                .From("TableName", "TableName2", "TableName3")
                .Select((t1, t2, t3) => t1.Int1, (t1, t2, t3) => t2.String1, (t1, t2, t3) => t3.Int2)
                .LeftOuterJoinOn((TableName, TableName2, TableName3) => TableName.Int1 == TableName2.Int1)
                .LeftOuterJoinOn((TableName, TableName2, TableName3) => TableName.Int1 == TableName3.Int1)
                .Where((t1, t2, t3) => t1.String1 != null)
                .Build();

            var expected = "SELECT TableName.Int1,TableName2.String1,TableName3.Int2 FROM TableName " +
                "LEFT OUTER JOIN TableName2 ON (TableName.Int1 = TableName2.Int1) " +
                "LEFT OUTER JOIN TableName3 ON (TableName.Int1 = TableName3.Int1) " +
                "WHERE (TableName.String1 IS NOT NULL)";

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestOuterJoinMultiSelect3RegularParamsInsteadOfTableNames()
        {
            var actual = new SimpleSqlBuilder<SqlTestNewRecord, SqlTestRecord, SqlTestRecord, SqlTestRecord>()
                .From("TableName", "TableName2", "TableName3")
                .Select((t1, t2, t3) => t1.Int1, (t1, t2, t3) => t2.String1, (t1, t2, t3) => t3.Int2)
                .LeftOuterJoinOn((t1, t2, t3) => t1.Int1 == t2.Int1)
                .InnerJoinOn((t1, t2, t3) => t1.Int1 == t3.Int1)
                .Where((t1, t2, t3) => t1.String1 != null)
                .Build();

            var expected = "SELECT TableName.Int1,TableName2.String1,TableName3.Int2 FROM TableName " +
                "LEFT OUTER JOIN TableName2 ON (TableName.Int1 = TableName2.Int1) " +
                "INNER JOIN TableName3 ON (TableName.Int1 = TableName3.Int1) " +
                "WHERE (TableName.String1 IS NOT NULL)";

            Assert.AreEqual(expected, actual);
        }

        private List<SqlTestRecord> GetData()
        {
            var dt = DateTime.MaxValue;
            var data = new List<SqlTestRecord>();
            data.Add(new SqlTestRecord { Int1 = 1, Int2 = 103, String1 = "String123", Bool1 = true, DateTime1 = dt });
            data.Add(new SqlTestRecord { Int1 = 2, Int2 = 102, String1 = "String111", Bool1 = false, DateTime1 = dt });
            data.Add(new SqlTestRecord { Int1 = 3, Int2 = 102, String1 = "String022", Bool1 = true, DateTime1 = dt });
            data.Add(new SqlTestRecord { Int1 = 4, Int2 = 101, String1 = "String112", Bool1 = false, DateTime1 = dt });
            data.Add(new SqlTestRecord { Int1 = 5, Int2 = 101, String1 = "String132", Bool1 = true, DateTime1 = dt });
            return data;
        }
    }

    public class SqlTestNewRecord
    {
        public int Int3 { get; set; }
    }

    public class SqlTestRecord
    {
        public bool Bool1 { get; set; }

        public DateTime DateTime1 { get; set; }

        public DateTime? DateTime2 { get; set; }

        public int Int1 { get; set; }

        public int Int2 { get; set; }

        public int Int3 { get; set; }

        public string String1 { get; set; }

        public string String2 { get; set; }
    }
}
