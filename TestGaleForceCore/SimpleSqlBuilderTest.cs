﻿namespace TestGaleForceCore
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.Linq;
    using System.Text;
    using GaleForceCore.Builders;
    using GaleForceCore.Helpers;
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
            var data = this.GetData();

            var actual = new SimpleSqlBuilder<SqlTestRecord>()
                .From(SqlTestRecord.TableName)
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
            var data = this.GetData();

            var actual = new SimpleSqlBuilder<SqlTestRecord>()
                .From(SqlTestRecord.TableName)
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
            var data = this.GetData();

            var actual = new SimpleSqlBuilder<SqlTestRecord>()
                .From(SqlTestRecord.TableName)
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
                .From(SqlTestRecord.TableName)
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
                .From(SqlTestRecord.TableName)
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
                .From(SqlTestRecord.TableName)
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
                .From(SqlTestRecord.TableName)
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
                .From(SqlTestRecord.TableName)
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
                .From(SqlTestRecord.TableName)
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
                .From(SqlTestRecord.TableName)
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
            var options = new SimpleSqlBuilderOptions { UseParameters = false };
            var actual = new SimpleSqlBuilder<SqlTestRecord>(options)
                .From(SqlTestRecord.TableName)
                .Select(r => r.Int1)
                .OrderByDescending(r => r.Int2)
                .Take(10)
                .Where(r => r.String1 == "ABC")
                .Build();

            var expected = "SELECT TOP 10 Int1 FROM TableName WHERE (String1 = 'ABC') ORDER BY Int2 DESC";

            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// Tests Select with Where.
        /// </summary>
        [TestMethod]
        public void TestSelectWhereStringParams()
        {
            var options = new SimpleSqlBuilderOptions { UseParameters = true };
            var actual = new SimpleSqlBuilder<SqlTestRecord>(options)
                .From(SqlTestRecord.TableName)
                .Select(r => r.Int1)
                .OrderByDescending(r => r.Int2)
                .Take(10)
                .Where(r => r.String1 == "ABC")
                .Build();

            var expected = "SELECT TOP 10 Int1 FROM TableName WHERE (String1 = @Param1) ORDER BY Int2 DESC";

            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// Tests Select with Where, Contains.
        /// </summary>
        [TestMethod]
        public void TestSelectWhereStringContainsStringArray()
        {
            var list = new string[] { "ABC", "DEF" };

            var options = new SimpleSqlBuilderOptions { UseParameters = false };
            var actual = new SimpleSqlBuilder<SqlTestRecord>(options)
                .From(SqlTestRecord.TableName)
                .Select(r => r.Int1)
                .OrderByDescending(r => r.Int2)
                .Take(10)
                .Where(r => list.Contains(r.String1))
                .Build();

            var expected = "SELECT TOP 10 Int1 FROM TableName WHERE String1 IN ('ABC','DEF') ORDER BY Int2 DESC";

            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// Tests Select with Where, Contains.
        /// </summary>
        [TestMethod]
        public void TestSelectWhereStringContainsIntArray()
        {
            var list = new int[] { 1, 3, 5 };

            var options = new SimpleSqlBuilderOptions { UseParameters = false };
            var actual = new SimpleSqlBuilder<SqlTestRecord>(options)
                .From(SqlTestRecord.TableName)
                .Select(r => r.Int1)
                .OrderByDescending(r => r.Int2)
                .Take(10)
                .Where(r => list.Contains(r.Int2.Value))
                .Build();

            var expected = "SELECT TOP 10 Int1 FROM TableName WHERE Int2 IN (1,3,5) ORDER BY Int2 DESC";

            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// Tests Select with Where, Contains.
        /// </summary>
        [TestMethod]
        public void TestStringEmpty()
        {
            var options = new SimpleSqlBuilderOptions { UseParameters = false };
            var actual = new SimpleSqlBuilder<SqlTestRecord>(options)
                .From(SqlTestRecord.TableName)
                .Select(r => r.Int1)
                .Where(r => r.String1 != string.Empty)
                .Build();

            var expected = "SELECT Int1 FROM TableName WHERE (String1 != '')";

            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// Tests Select with Where, Contains.
        /// </summary>
        [TestMethod]
        public void TestSelectWhereStringContains()
        {
            var options = new SimpleSqlBuilderOptions { UseParameters = false };
            var actual = new SimpleSqlBuilder<SqlTestRecord>(options)
                .From(SqlTestRecord.TableName)
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
        public void TestSelectWhereStringContainsParam()
        {
            var options = new SimpleSqlBuilderOptions { UseParameters = true };
            var actual = new SimpleSqlBuilder<SqlTestRecord>(options)
                .From(SqlTestRecord.TableName)
                .Select(r => r.Int1)
                .OrderByDescending(r => r.Int2)
                .Take(10)
                .Where(r => r.String1.Contains("ABC"))
                .Build();

            var expected = "SELECT TOP 10 Int1 FROM TableName WHERE String1 LIKE '%'+@Param1+'%' ORDER BY Int2 DESC";

            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// Tests Select with Where, Contains.
        /// </summary>
        [TestMethod]
        public void TestSelectWhereStringContainsLocalVars() //todo:params
        {
            var abcdefg = "ABCDEFG";
            var abc = "ABC";
            var options = new SimpleSqlBuilderOptions { UseParameters = false };
            var actual = new SimpleSqlBuilder<SqlTestRecord>(options)
                .From(SqlTestRecord.TableName)
                .Select(r => r.Int1)
                .OrderByDescending(r => r.Int2)
                .Take(10)
                .Where(r => abcdefg.StartsWith(abc))
                .Build();

            var expected = "SELECT TOP 10 Int1 FROM TableName WHERE 'ABCDEFG' LIKE 'ABC%' ORDER BY Int2 DESC";

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestWhereBool() //todo: params
        {
            var user = "abc@def.com";
            var options = new SimpleSqlBuilderOptions { UseParameters = false };
            var actual = new SimpleSqlBuilder<SqlTestRecord>(options)
                .From(SqlTestRecord.TableName)
                .Select(r => r.Bool1)
                .Where(r => r.String1 == user && r.Bool1)
                .Take(1)
                .Build();

            var expected = "SELECT TOP 1 Bool1 FROM TableName WHERE ((String1 = 'abc@def.com') AND Bool1 = 1)";

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestWhereBoolFalse() //todo: params
        {
            var user = "abc@def.com";
            var options = new SimpleSqlBuilderOptions { UseParameters = false };
            var actual = new SimpleSqlBuilder<SqlTestRecord>(options)
                .From(SqlTestRecord.TableName)
                .Select(r => r.Int1)
                .Where(r => r.String1 == user && !r.Bool1)
                .Take(1)
                .Build();

            var expected = "SELECT TOP 1 Int1 FROM TableName WHERE ((String1 = 'abc@def.com') AND Bool1 = 0)";

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestWhereBoolCompare() //todo: params
        {
            var user = "abc@def.com";
            var options = new SimpleSqlBuilderOptions { UseParameters = false };
            var actual = new SimpleSqlBuilder<SqlTestRecord>(options)
                .From(SqlTestRecord.TableName)
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
            var actual = new SimpleSqlBuilder<SqlTestRecord>()
                .From(SqlTestRecord.TableName)
                .Select(r => r.Int1)
                .Where(r => !r.Bool1)
                .Take(1)
                .Build();

            var expected = "SELECT TOP 1 Int1 FROM TableName WHERE Bool1 = 0";

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestWhereAdditive() //todo: params
        {
            var user = "abc@def";
            var options = new SimpleSqlBuilderOptions { UseParameters = false };
            var actual = new SimpleSqlBuilder<SqlTestRecord>(options)
                .From(SqlTestRecord.TableName)
                .Select(r => r.Int1)
                .Where(r => r.String1 == user + ".com")
                .Take(1)
                .Build();

            var expected = "SELECT TOP 1 Int1 FROM TableName WHERE (String1 = CONCAT('abc@def','.com'))";

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestWhereAdditive3() //todo: params
        {
            var user = "abc@def";
            var options = new SimpleSqlBuilderOptions { UseParameters = false };
            var actual = new SimpleSqlBuilder<SqlTestRecord>(options)
                .From(SqlTestRecord.TableName)
                .Select(r => r.Int1)
                .Where(r => r.String1 == user + "." + "com")
                .Take(1)
                .Build();

            var expected = "SELECT TOP 1 Int1 FROM TableName WHERE (String1 = CONCAT(CONCAT('abc@def','.'),'com'))";

            // todo: collect param list and make one concat stringset

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestWhereAdditiveWithField() //todo: params
        {
            var options = new SimpleSqlBuilderOptions { UseParameters = false };
            var actual = new SimpleSqlBuilder<SqlTestRecord>(options)
                .From(SqlTestRecord.TableName)
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
                .From(SqlTestRecord.TableName, "TableName2")
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
                .From(SqlTestRecord.TableName, "TableName2")
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
                .From(SqlTestRecord.TableName, "TableName2")
                .Select((t1, t2) => t1.Int1)
                .InnerJoinOn((TableName, TableName2) => TableName.String1 == TableName2.String1)
                .Where(tResult => tResult.Int3 > 0)
                .Build();

            var expected = "SELECT TableName.Int1 FROM TableName INNER JOIN TableName2 ON (TableName.String1 = TableName2.String1) WHERE (Int3 > 0)";

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestWhere2() //todo: params
        {
            var options = new SimpleSqlBuilderOptions { UseParameters = false };
            var actual = new SimpleSqlBuilder<SqlTestNewRecord, SqlTestRecord, SqlTestRecord>(options)
                .From(SqlTestRecord.TableName, "TableName2")
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
            var data = this.GetData();

            var options = new SimpleSqlBuilderOptions { UseParameters = false };
            var actual = new SimpleSqlBuilder<SqlTestNewRecord, SqlTestRecord, SqlTestRecord>(options)
                .From(SqlTestRecord.TableName, "TableName2")
                .InnerJoinOn((TableName, TableName2) => TableName.String1 == TableName2.String1)
                .Where((m, t) => m.String1 == data[0].String1)
                .Select((m, t) => m.Int1)
                .Build();

            var expected = "SELECT TableName.Int1 FROM TableName INNER JOIN TableName2 ON (TableName.String1 = TableName2.String1) WHERE (TableName.String1 = 'String123')";
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestInnerJoinStringCompareParams()
        {
            var data = this.GetData();

            var options = new SimpleSqlBuilderOptions { UseParameters = true };
            var actual = new SimpleSqlBuilder<SqlTestNewRecord, SqlTestRecord, SqlTestRecord>(options)
                .From(SqlTestRecord.TableName, "TableName2")
                .InnerJoinOn((TableName, TableName2) => TableName.String1 == TableName2.String1)
                .Where((m, t) => m.String1 == data[0].String1)
                .Select((m, t) => m.Int1)
                .Build();

            var expected = "SELECT TableName.Int1 FROM TableName INNER JOIN TableName2 ON (TableName.String1 = TableName2.String1) WHERE (TableName.String1 = @Param1)";
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestJoinTake()
        {
            var data = this.GetData();

            var options = new SimpleSqlBuilderOptions { UseParameters = false };
            var actual = new SimpleSqlBuilder<SqlTestNewRecord, SqlTestRecord, SqlTestRecord>(options)
                .From(SqlTestRecord.TableName, "TableName2")
                .Take(10)
                .InnerJoinOn((TableName, TableName2) => TableName.String1 == TableName2.String1)
                .Where((m, t) => m.String1 == data[0].String1)
                .Select((m, t) => m.Int1)
                .Build();

            var expected = "SELECT TOP 10 TableName.Int1 FROM TableName INNER JOIN TableName2 ON (TableName.String1 = TableName2.String1) WHERE (TableName.String1 = 'String123')";
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestJoinTakeParams()
        {
            var data = this.GetData();

            var options = new SimpleSqlBuilderOptions { UseParameters = true };
            var actual = new SimpleSqlBuilder<SqlTestNewRecord, SqlTestRecord, SqlTestRecord>(options)
                .From(SqlTestRecord.TableName, "TableName2")
                .Take(10)
                .InnerJoinOn((TableName, TableName2) => TableName.String1 == TableName2.String1)
                .Where((m, t) => m.String1 == data[0].String1)
                .Select((m, t) => m.Int1)
                .Build();

            var expected = "SELECT TOP 10 TableName.Int1 FROM TableName INNER JOIN TableName2 ON (TableName.String1 = TableName2.String1) WHERE (TableName.String1 = @Param1)";
            Assert.AreEqual(expected, actual);
        }

        // xxx
        [TestMethod]
        public void TestSelectAndSelectAsDifferentTable()
        {
            var data = this.GetData();

            var options = new SimpleSqlBuilderOptions { UseParameters = false };
            var actual = new SimpleSqlBuilder<SqlTestNewRecord, SqlTestRecord, SqlTestRecord>(options)
                .From(SqlTestRecord.TableName, "TableName2")
                .Select((m, t) => m.Int1)
                .SelectAs(m => m.Int3, (m, t) => m.Int2)
                .InnerJoinOn((TableName, TableName2) => TableName.String1 == TableName2.String1)
                .Where((m, t) => m.String1 == data[0].String1)
                .Build();

            var expected = "SELECT TableName.Int1,TableName.Int2 AS Int3 FROM TableName INNER JOIN TableName2 ON (TableName.String1 = TableName2.String1) WHERE (TableName.String1 = 'String123')";
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestSelectAndSelectAsDifferentTableParams()
        {
            var data = this.GetData();

            var options = new SimpleSqlBuilderOptions { UseParameters = true };
            var actual = new SimpleSqlBuilder<SqlTestNewRecord, SqlTestRecord, SqlTestRecord>(options)
                .From(SqlTestRecord.TableName, "TableName2")
                .Select((m, t) => m.Int1)
                .SelectAs(m => m.Int3, (m, t) => m.Int2)
                .InnerJoinOn((TableName, TableName2) => TableName.String1 == TableName2.String1)
                .Where((m, t) => m.String1 == data[0].String1)
                .Build();

            var expected = "SELECT TableName.Int1,TableName.Int2 AS Int3 FROM TableName INNER JOIN TableName2 ON (TableName.String1 = TableName2.String1) WHERE (TableName.String1 = @Param1)";
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestSelectAndSelectAsSameTable() //todo:params
        {
            var data = this.GetData();

            var options = new SimpleSqlBuilderOptions { UseParameters = false };
            var actual = new SimpleSqlBuilder<SqlTestRecord, SqlTestRecord, SqlTestRecord>(options)
                .From(SqlTestRecord.TableName, "TableName2")
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
            var data = this.GetData();

            var intList = new List<int>() { 1, 2, 3 };

            var actual = new SimpleSqlBuilder<SqlTestRecord>()
                .From(SqlTestRecord.TableName)
                .Select(a => a.Int1)
                .Where(a => intList.Contains((int)a.Int1))
                .Build();

            var expected = "SELECT Int1 FROM TableName WHERE Int1 IN (1,2,3)";
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestInListStrings()
        {
            var data = this.GetData();

            var stringList = new List<string>() { "a", "b", "c" };

            var actual = new SimpleSqlBuilder<SqlTestRecord>()
                .From(SqlTestRecord.TableName)
                .Select(a => a.Int1)
                .Where(a => stringList.Contains(a.String1))
                .Build();

            var expected = "SELECT Int1 FROM TableName WHERE String1 IN ('a','b','c')";
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestOuterJoinSelect() //todo: params
        {
            var options = new SimpleSqlBuilderOptions { UseParameters = false };
            var actual = new SimpleSqlBuilder<SqlTestNewRecord, SqlTestRecord, SqlTestRecord>(options)
                .From(SqlTestRecord.TableName, "TableName2")
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
        public void TestOuterJoinMultiSelect() //todo: params
        {
            var options = new SimpleSqlBuilderOptions { UseParameters = false };
            var actual = new SimpleSqlBuilder<SqlTestNewRecord, SqlTestRecord, SqlTestRecord>(options)
                .From(SqlTestRecord.TableName, "TableName2")
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
        public void TestOuterJoinMultiSelect3() //todo: params
        {
            var options = new SimpleSqlBuilderOptions { UseParameters = false };
            var actual = new SimpleSqlBuilder<SqlTestNewRecord, SqlTestRecord, SqlTestRecord, SqlTestRecord>(options)
                .From("TableName", "TableName2", "TableName3")
                .Select((t1, t2, t3) => t1.Int1, (t1, t2, t3) => t2.String1, (t1, t2, t3) => t3.Int2)
                .LeftOuterJoin12On((TableName, TableName2) => TableName.Int1 == TableName2.Int1)
                .LeftOuterJoin13On((TableName, TableName3) => TableName.Int1 == TableName3.Int1)
                .Where((t1, t2, t3) => t1.String1 != null)
                .Build();

            var expected = "SELECT TableName.Int1,TableName2.String1,TableName3.Int2 FROM TableName " +
                "LEFT OUTER JOIN TableName2 ON (TableName.Int1 = TableName2.Int1) " +
                "LEFT OUTER JOIN TableName3 ON (TableName.Int1 = TableName3.Int1) " +
                "WHERE (TableName.String1 IS NOT NULL)";

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestOuterJoinMultiSelect3RegularParamsInsteadOfTableNames() //todo: params
        {
            var options = new SimpleSqlBuilderOptions { UseParameters = false };
            var actual = new SimpleSqlBuilder<SqlTestNewRecord, SqlTestRecord, SqlTestRecord, SqlTestRecord>(options)
                .From("TableName", "TableName2", "TableName3")
                .Select((t1, t2, t3) => t1.Int1, (t1, t2, t3) => t2.String1, (t1, t2, t3) => t3.Int2)
                .LeftOuterJoin12On((t1, t2) => t1.Int1 == t2.Int1)
                .InnerJoin13On((t1, t3) => t1.Int1 == t3.Int1)
                .Where((t1, t2, t3) => t1.String1 != null)
                .Build();

            var expected = "SELECT TableName.Int1,TableName2.String1,TableName3.Int2 FROM TableName " +
                "LEFT OUTER JOIN TableName2 ON (TableName.Int1 = TableName2.Int1) " +
                "INNER JOIN TableName3 ON (TableName.Int1 = TableName3.Int1) " +
                "WHERE (TableName.String1 IS NOT NULL)";

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestDeepWhere()
        {
            var emotionSet = new string[] { "ABC" };
            var from = 0;
            var toto = 10;
            var piece = "DEF";
            var options = new SimpleSqlBuilderOptions { UseParameters = false };
            var actual = new SimpleSqlBuilder<SqlTestNewRecord, SqlTestRecord, SqlTestRecord>(options)
                    .From(SqlTestRecord.TableName, SqlTestRecord.TableName)
                .Select((e, t) => e.Int1, (e, t) => t.String1)
                .InnerJoinOn((e, t) => e.Int1 == t.Int1)
                .Where(
                    (e, t) => t.Int1 > from &&
                        t.Int1 < toto &&
                        (t.String1.Contains(piece + ":Reviewed") || t.String1.Contains(piece + "Non-Actionable")) &&
                        emotionSet.Contains(e.String1))
                .OrderByDescending((e, t) => e.Int1)
                .Build();

            var expected = "SELECT TableName.Int1,TableName__1.String1 FROM TableName " +
                "INNER JOIN TableName TableName__1 ON (TableName.Int1 = TableName__1.Int1) " +
                "WHERE ((((TableName__1.Int1 > 0) AND (TableName__1.Int1 < 10)) AND " +
                "(TableName__1.String1 LIKE '%'+CONCAT('DEF',':Reviewed')+'%' OR " +
                "TableName__1.String1 LIKE '%'+CONCAT('DEF','Non-Actionable')+'%')) " +
                "AND TableName.String1 IN ('ABC')) ORDER BY TableName.Int1 DESC";

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestDeepWhereParams()
        {
            var emotionSet = new string[] { "ABC" };
            var from = 0;
            var toto = 10;
            var piece = "DEF";
            var options = new SimpleSqlBuilderOptions { UseParameters = true };
            var actual = new SimpleSqlBuilder<SqlTestNewRecord, SqlTestRecord, SqlTestRecord>(options)
                    .From(SqlTestRecord.TableName, SqlTestRecord.TableName)
                .Select((e, t) => e.Int1, (e, t) => t.String1)
                .InnerJoinOn((e, t) => e.Int1 == t.Int1)
                .Where(
                    (e, t) => t.Int1 > from &&
                        t.Int1 < toto &&
                        (t.String1.Contains(piece + ":Reviewed") || t.String1.Contains(piece + "Non-Actionable")) &&
                        emotionSet.Contains(e.String1))
                .OrderByDescending((e, t) => e.Int1)
                .Build();

            var expected = "SELECT TableName.Int1,TableName__1.String1 FROM TableName INNER JOIN TableName TableName__1 ON (TableName.Int1 = TableName__1.Int1) WHERE ((((TableName__1.Int1 > 0) AND (TableName__1.Int1 < 10)) AND (TableName__1.String1 LIKE '%'+CONCAT(@Param1,@Param2)+'%' OR TableName__1.String1 LIKE '%'+CONCAT(@Param3,@Param4)+'%')) AND TableName.String1 IN ('ABC')) ORDER BY TableName.Int1 DESC";

            Assert.AreEqual(expected, actual);
        }

        // [TestMethod]
        // public void TestOrderBy3() //fails at the moment - not built yet
        // {
        // var emotionSet = new string[] { "ABC" };
        // var from = 0;
        // var toto = 10;
        // var piece = "DEF";
        // var actual = new SimpleSqlBuilder<SqlTestNewRecord, SqlTestRecord, SqlTestRecord>()
        // .From(SqlTestRecord.TableName, SqlTestRecord.TableName)
        // .Select((e, t) => e.Int3, (e, t) => t.String2)
        // .InnerJoinOn((e, t) => e.Int3 == t.Int3)
        // .OrderBy((e, t) => e.Int3)
        // .Build();

        // var expected = "SELECT TableName.Int3,TableName.String2 " +
        // "FROM TableName " +
        // "INNER JOIN TableName ON (TableName.Int3 = TableName.Int3) " +
        // "ORDER BY TableName.Int3 DESC";

        // // Assert.AreEqual(expected, actual);
        // }

        [TestMethod]
        public void TestSelectAll()
        {
            var actual = new SimpleSqlBuilder<SqlTestRecord>()
            .From(SqlTestRecord.TableName)
                .Where(a => a.Int1 == 1)
                .Build();

            var expected = "SELECT * FROM TableName WHERE (Int1 = 1)";
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestUpdate()
        {
            var dt = DateTime.MinValue;
            var newRecord = new SqlTestRecord
            {
                Int1 = 3,
                Int2 = 202,
                String1 = "String999",
                Bool1 = false,
                DateTime1 = dt
            };

            var actual = new SimpleSqlBuilder<SqlTestRecord>(SqlTestRecord.TableName)
                .Match(s => s.Int1)
                .Update(newRecord, s => s.Int2, s => s.Bool1)
                .Build();

            var expected = "UPDATE TableName SET Int2 = 202, Bool1 = 0 WHERE Int1 = 3";

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestUpdateBytes()
        {
            var dt = DateTime.MinValue;
            var newRecord = new SqlTest5Record
            {
                Int1 = 1,
                Data = new byte[5] { 1, 2, 3, 4, 5 } };

            var actual = new SimpleSqlBuilder<SqlTest5Record>(SqlTest5Record.TableName)
                .Update(newRecord)
                .Build();

            var expected = "UPDATE TableName SET Bool1 = 0, DateTime1 = '0001-01-01T00:00:00.0000000', DateTime2 = null, Int1 = 1, Int2 = null, Int3 = null, String1 = null, String2 = null, Bool2 = null, Int4 = null, Data = 0x0102030405";

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestUpdateWithWhere() //todo: params
        {
            var dt = DateTime.MinValue;
            var newRecord = new SqlTestRecord
            {
                Int1 = 3,
                Int2 = 202,
                String1 = "String999",
                Bool1 = false,
                DateTime1 = dt
            };

            var options = new SimpleSqlBuilderOptions { UseParameters = false };
            var actual = new SimpleSqlBuilder<SqlTestRecord>(options, SqlTestRecord.TableName)
                .Match(s => s.Int1)
                .Update(newRecord, s => s.Int2, s => s.Bool1)
                .Where(s => s.String1 == "String022")
                .Build();

            var expected = "UPDATE TableName SET Int2 = 202, Bool1 = 0 WHERE Int1 = 3 AND (String1 = 'String022')";

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestUpdateMultiple() //todo: params
        {
            var dt = DateTime.MinValue;
            var records = new List<SqlTestRecord>();
            var newRecord = new SqlTestRecord
            {
                Int1 = 3,
                Int2 = 202,
                String1 = "String999",
                Bool1 = false,
                DateTime1 = dt
            };

            records.Add(newRecord);

            newRecord = new SqlTestRecord { Int1 = 4, Int2 = 201, String1 = "String999", Bool1 = true, DateTime1 = dt };

            records.Add(newRecord);

            var options = new SimpleSqlBuilderOptions { UseParameters = false };
            var actual = new SimpleSqlBuilder<SqlTestRecord>(options, SqlTestRecord.TableName)
                .Match(s => s.Int1)
                .Update(records, s => s.Int2, s => s.Bool1)
                .Where(s => s.String1 == "String022")
                .Build();

            var expected = "UPDATE TableName SET Int2 = 202, Bool1 = 0 WHERE Int1 = 3 AND (String1 = 'String022');\r\n" +
                "UPDATE TableName SET Int2 = 201, Bool1 = 1 WHERE Int1 = 4 AND (String1 = 'String022');\r\n" +
                "GO;";

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestUpdateExecute()
        {
            var dt = DateTime.MinValue;
            var source = new List<SqlTestRecord>();
            var newRecord = new SqlTestRecord
            {
                Int1 = 3,
                Int2 = 202,
                String1 = "String999",
                Bool1 = false,
                DateTime1 = dt
            };

            source.Add(newRecord);

            newRecord = new SqlTestRecord { Int1 = 4, Int2 = 201, String1 = "String999", Bool1 = true, DateTime1 = dt };

            source.Add(newRecord);

            var target = this.GetData();
            var count = new SimpleSqlBuilder<SqlTestRecord>(SqlTestRecord.TableName)
                .Match(s => s.Int1)
                .Update(source, s => s.Int2, s => s.Bool1)
                .Where(s => s.String1 == "String022")
                .ExecuteUpdate(target);

            Assert.AreEqual(1, count);

            Assert.AreEqual(1, target[0].Int1);
            Assert.AreEqual(103, target[0].Int2);
            Assert.AreEqual("String123", target[0].String1);

            Assert.AreEqual(3, target[2].Int1);
            Assert.AreEqual(202, target[2].Int2);
            Assert.AreEqual("String022", target[2].String1);

            Assert.AreEqual(4, target[3].Int1);
            Assert.AreEqual(101, target[3].Int2);
            Assert.AreEqual("String112", target[3].String1);
        }

        [TestMethod]
        public void TestExecuteInsert1()
        {
            var dt = DateTime.MinValue;
            var source = new List<SqlTestRecord>();
            var newRecord = new SqlTestRecord
            {
                Int1 = 3,
                Int2 = 202,
                String1 = "String999",
                Bool1 = false,
                DateTime1 = dt
            };

            source.Add(newRecord);

            newRecord = new SqlTestRecord { Int1 = 4, Int2 = 201, String1 = "String999", Bool1 = true, DateTime1 = dt };

            source.Add(newRecord);

            var target = this.GetData();
            var count = new SimpleSqlBuilder<SqlTestRecord>(SqlTestRecord.TableName)
                .Insert(source, s => s.Int2, s => s.Bool1)
                .ExecuteInsert(target);

            Assert.AreEqual(2, count);
            Assert.AreEqual(7, target.Count());

            Assert.AreEqual(source[0].Int2, target[5].Int2);
            Assert.AreEqual(source[0].Bool1, target[5].Bool1);
            Assert.AreNotEqual(source[0].String1, target[5].String1);

            Assert.AreEqual(source[1].Int2, target[6].Int2);
            Assert.AreEqual(source[1].Bool1, target[6].Bool1);
            Assert.AreNotEqual(source[1].String1, target[6].String1);
        }

        [TestMethod]
        public void TestExecuteInsertValues1()
        {
            var dt = DateTime.MinValue;
            var source = new List<SqlTestRecord>();
            var newRecord = new SqlTestRecord
            {
                Int1 = 3,
                Int2 = 202,
                String1 = "String999",
                Bool1 = false,
                DateTime1 = dt
            };

            source.Add(newRecord);

            newRecord = new SqlTestRecord { Int1 = 4, Int2 = 201, String1 = "String999", Bool1 = true, DateTime1 = dt };

            source.Add(newRecord);

            var target = this.GetData();
            var count = new SimpleSqlBuilder<SqlTestRecord>(SqlTestRecord.TableName)
                .Insert(source, s => s.Int2, s => s.Bool1, s => s.Int1)
                .Values(s => 2 + s.Int2, s => !s.Bool1, s => 6)
                .ExecuteInsert(target);

            Assert.AreEqual(2, count);
            Assert.AreEqual(7, target.Count());

            Assert.AreEqual(source[0].Int2 + 2, target[5].Int2);
            Assert.AreEqual(source[0].Bool1, !target[5].Bool1);
            Assert.AreEqual(6, target[5].Int1);
            Assert.AreNotEqual(source[0].String1, target[5].String1);

            Assert.AreEqual(source[1].Int2 + 2, target[6].Int2);
            Assert.AreEqual(source[1].Bool1, !target[6].Bool1);
            Assert.AreEqual(6, target[6].Int1);
            Assert.AreNotEqual(source[1].String1, target[6].String1);
        }

        [TestMethod]
        public void TestBuildInsert1()
        {
            var dt = DateTime.MinValue;
            var source = new List<SqlTestRecord>();
            var newRecord = new SqlTestRecord
            {
                Int1 = 3,
                Int2 = 202,
                String1 = "String999",
                Bool1 = false,
                DateTime1 = dt
            };

            source.Add(newRecord);

            newRecord = new SqlTestRecord { Int1 = 4, Int2 = 201, String1 = "String999", Bool1 = true, DateTime1 = dt };

            source.Add(newRecord);

            var target = this.GetData();
            var actual = new SimpleSqlBuilder<SqlTestRecord>(SqlTestRecord.TableName)
                .Insert(source, s => s.Int2, s => s.Bool1)
                .Build();

            var expected = @"INSERT INTO TableName (Int2,Bool1) VALUES (202,0);
INSERT INTO TableName (Int2,Bool1) VALUES (201,1);
GO;";

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestBuildInsertValues1()
        {
            var dt = DateTime.MinValue;
            var source = new List<SqlTestRecord>();
            var newRecord = new SqlTestRecord
            {
                Int1 = 3,
                Int2 = 202,
                String1 = "String999",
                Bool1 = false,
                DateTime1 = dt
            };

            source.Add(newRecord);

            newRecord = new SqlTestRecord { Int1 = 4, Int2 = 201, String1 = "String999", Bool1 = true, DateTime1 = dt };

            source.Add(newRecord);

            var target = this.GetData();
            var actual = new SimpleSqlBuilder<SqlTestRecord>(SqlTestRecord.TableName)
                .Insert(source, s => s.Int1, s => s.Int2, s => s.Bool1)
                .Values(s => s.Int1, s => 10, s => !s.Bool1)
                .Build();

            var expected = @"INSERT INTO TableName (Int1,Int2,Bool1) VALUES (3,10,1);
INSERT INTO TableName (Int1,Int2,Bool1) VALUES (4,10,0);
GO;";

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestMerge()
        {
            var options = new SimpleSqlBuilderOptions { UseParameters = false };
            var target = this.GetData();
            var actual = new SimpleSqlBuilder<SqlTestRecord>(options, SqlTestRecord.TableName)
                .MergeInto(SqlTestRecord.TableName)
                .Match((s, t) => s.Int1 == t.Int1 && s.String1 == "String123")
                .Where(s => s.Int2 == 101)
                .WhenMatched(s => s.Update(s => s.Int2))
                .WhenNotMatched(s => s.Insert(s => s.Int1, s => s.Int2, s => s.String1))
                .Build();

            var expected = @"MERGE INTO TableName AS Target USING TableName AS Source ON ((Source.Int1 = Target.Int1) AND (Source.String1 = 'String123')) WHEN MATCHED THEN UPDATE SET Target.Int2 = Source.Int2 WHEN NOT MATCHED THEN INSERT (Int1,Int2,String1) VALUES (Source.Int1, Source.Int2, Source.String1);";

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestMergeParams()
        {
            var options = new SimpleSqlBuilderOptions { UseParameters = true };
            var target = this.GetData();
            var actual = new SimpleSqlBuilder<SqlTestRecord>(options, SqlTestRecord.TableName)
                .MergeInto(SqlTestRecord.TableName)
                .Match((s, t) => s.Int1 == t.Int1 && s.String1 == "String123")
                .Where(s => s.Int2 == 101)
                .WhenMatched(s => s.Update(s => s.Int2))
                .WhenNotMatched(s => s.Insert(s => s.Int1, s => s.Int2, s => s.String1))
                .Build();

            var expected = @"MERGE INTO TableName AS Target USING TableName AS Source ON ((Source.Int1 = Target.Int1) AND (Source.String1 = @Param2)) WHEN MATCHED THEN UPDATE SET Target.Int2 = Source.Int2 WHEN NOT MATCHED THEN INSERT (Int1,Int2,String1) VALUES (Source.Int1, Source.Int2, Source.String1);";

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestMergeValues()
        {
            var options = new SimpleSqlBuilderOptions { UseParameters = false };
            var target = this.GetData();
            var actual = new SimpleSqlBuilder<SqlTestRecord>(options, SqlTestRecord.TableName)
                .MergeInto(SqlTestRecord.TableName)
                .Match((s, t) => s.Int1 == t.Int1 && s.String1 == "String123")
                .WhenMatched(s => s.Update(s => s.Int2).Values(s => 5))
                .WhenNotMatched(
                    s => s.Insert(s => s.Int1, s => s.Int2, s => s.String1).Values(s => s.Int1 + 1, s => 20, s => "X"))
                .Build();

            var expected = @"MERGE INTO TableName AS Target USING TableName AS Source ON ((Source.Int1 = Target.Int1) AND (Source.String1 = 'String123')) WHEN MATCHED THEN UPDATE SET Target.Int2 = 5 WHEN NOT MATCHED THEN INSERT (Int1,Int2,String1) VALUES ((Source.Int1 + 1), 20, 'X');";

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestMergeValuesParams()
        {
            var options = new SimpleSqlBuilderOptions { UseParameters = true };
            var target = this.GetData();
            var actual = new SimpleSqlBuilder<SqlTestRecord>(options, SqlTestRecord.TableName)
                .MergeInto(SqlTestRecord.TableName)
                .Match((s, t) => s.Int1 == t.Int1 && s.String1 == "String123")
                .WhenMatched(s => s.Update(s => s.Int2).Values(s => 5))
                .WhenNotMatched(
                    s => s.Insert(s => s.Int1, s => s.Int2, s => s.String1).Values(s => s.Int1 + 1, s => 20, s => "X"))
                .Build();

            var expected = @"MERGE INTO TableName AS Target USING TableName AS Source ON ((Source.Int1 = Target.Int1) AND (Source.String1 = @Param2)) WHEN MATCHED THEN UPDATE SET Target.Int2 = 5 WHEN NOT MATCHED THEN INSERT (Int1,Int2,String1) VALUES ((Source.Int1 + 1), 20, @Param1);";

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestExecuteMerge()
        {
            var dt = DateTime.MinValue;
            var source = new List<SqlTestRecord>();
            var newRecord = new SqlTestRecord
            {
                Int1 = 3,
                Int2 = 202,
                String1 = "String999",
                Bool1 = false,
                DateTime1 = dt
            };

            source.Add(newRecord);

            newRecord = new SqlTestRecord { Int1 = 4, Int2 = 201, String1 = "String999", Bool1 = true, DateTime1 = dt };

            source.Add(newRecord);

            var target = this.GetData();
            var actual = new SimpleSqlBuilder<SqlTestRecord>(SqlTestRecord.TableName)
                .MergeInto(SqlTestRecord.TableName)
                .Match((s, t) => s.Int1 == t.Int1 && t.String1 == "String022")
                .WhenMatched(s => s.Update(s => s.Int2).Values(s => 5))
                .WhenNotMatched(
                    s => s.Insert(s => s.Int1, s => s.Int2, s => s.String1).Values(s => s.Int1 + 1, s => 20, s => "X"))
                .ExecuteMerge(target, source);

            var originalData = this.GetData();

            Assert.AreEqual(2, actual);
            Assert.AreEqual(originalData.Count() + 1, target.Count());
            Assert.AreEqual(5, target[2].Int2);
            Assert.AreEqual(20, target[5].Int2);
            Assert.AreEqual("X", target[5].String1);
            for (var i = 0; i < 5; i++)
            {
                if (i != 2)
                {
                    Assert.AreEqual(originalData[i].Int2, target[i].Int2);
                    Assert.AreEqual(originalData[i].Int1, target[i].Int1);
                }
            }
        }

        [TestMethod]
        public void TestMergeFromGeneric()
        {
            var target = this.GetData();
            target.RemoveAt(2);

            var source = this.GetData();
            for (var i = 0; i < source.Count; i++)
            {
                source[i].Int2 = 1000 + i;
            }

            var sourceIEnum = source as IEnumerable<SqlTestRecord>;

            // Assert.AreNotEqual(source[0].Int1, target[0].Int1);
            // Assert.AreNotEqual(source[1].Int1, target[1].Int1);
            // Assert.AreNotEqual(source[2].Int1, target[2].Int1);

            var count = new SimpleSqlBuilder<SqlTestRecord>(SqlTestRecord.TableName + "_temp")
                .MergeInto(SqlTestRecord.TableName)
                .Match(l => l.Int1)
                .WhenMatched(s => s.Update(l => l.Int1, l => l.String1, l => l.Int2))
                .WhenNotMatched(s => s.Insert(l => l.Int1, l => l.String1, l => l.Int2))
                .ExecuteMerge(target, sourceIEnum);

            var sortedTarget = target.OrderBy(t => t.Int1).ToList();

            Assert.AreEqual(5, target.Count);
            Assert.AreEqual(source[0].Int1, sortedTarget[0].Int1);
            Assert.AreEqual(source[1].Int1, sortedTarget[1].Int1);
            Assert.AreEqual(source[2].Int1, sortedTarget[2].Int1);
        }

        [TestMethod]
        public void TestDelete()
        {
            var actual = new SimpleSqlBuilder<SqlTestRecord>(SqlTestRecord.TableName)
                .Delete()
                .Build();

            var expected = "DELETE FROM TableName";

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestDeleteWhere()
        {
            var actual = new SimpleSqlBuilder<SqlTestRecord>(SqlTestRecord.TableName)
                .Delete()
                .Where(s => s.Int1 == 5)
                .Build();

            var expected = "DELETE FROM TableName WHERE (Int1 = 5)";

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestDeleteDistinct()
        {
            var actual = new SimpleSqlBuilder<SqlTestRecord>(SqlTestRecord.TableName)
                .Delete()
                .ExceptDistinctBy(s => s.Int2)
                .Build();

            var expected = 
                @"with TableNameCTE AS (SELECT Int2, row_number() over (partition by Int2 order by Int2) as Temp from TableName) DELETE FROM TableNameCTE where Temp > 1";

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestDeleteDistinctWhere()
        {
            var actual = new SimpleSqlBuilder<SqlTestRecord>(SqlTestRecord.TableName)
                .Delete()
                .ExceptDistinctBy(s => s.Int2)
                .Where(s => s.Int1 == 5)
                .Build();

            var expected = 
                @"with TableNameCTE AS (SELECT Int2, row_number() over (partition by Int2 order by Int2) as Temp from TableName WHERE (Int1 = 5)) DELETE FROM TableNameCTE where Temp > 1";

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestExecuteDelete1()
        {
            var data = this.GetData();
            var actual = new SimpleSqlBuilder<SqlTestRecord>(SqlTestRecord.TableName)
                .Delete()
                .ExecuteDelete(data);

            Assert.AreEqual(5, actual);
            Assert.AreEqual(0, data.Count());
        }

        [TestMethod]
        public void TestExecuteDeleteWhere()
        {
            var data = this.GetData();
            var actual = new SimpleSqlBuilder<SqlTestRecord>(SqlTestRecord.TableName)
                .Delete()
                .Where(s => s.Int1 == 5)
                .ExecuteDelete(data);

            Assert.AreEqual(1, actual);
            Assert.AreEqual(4, data.Count());
        }

        [TestMethod]
        public void TestExecuteDeleteDistinct()
        {
            var data = this.GetData();
            var actual = new SimpleSqlBuilder<SqlTestRecord>(SqlTestRecord.TableName)
                .Delete()
                .ExceptDistinctBy(s => s.Int2)
                .ExecuteDelete(data);

            Assert.AreEqual(2, actual);
            Assert.AreEqual(3, data.Count());
            Assert.AreEqual(101, data[2].Int2);
        }

        [TestMethod]
        public void TestExecuteDeleteDistinctStr()
        {
            var data = this.GetData();
            var actual = new SimpleSqlBuilder<SqlTestRecord>(SqlTestRecord.TableName)
                .Delete()
                .ExceptDistinctBy(s => s.String1)
                .ExecuteDelete(data);

            Assert.AreEqual(0, actual);
            Assert.AreEqual(5, data.Count());
        }

        [TestMethod]
        public void TestExecuteDeleteDistinctWhere()
        {
            var data = this.GetData();
            var actual = new SimpleSqlBuilder<SqlTestRecord>(SqlTestRecord.TableName)
                .Delete()
                .ExceptDistinctBy(s => s.Int2)
                .Where(s => s.Int1 == 5)
                .ExecuteDelete(data);

            Assert.AreEqual(0, actual);
            Assert.AreEqual(5, data.Count());
            Assert.AreEqual(4, data[3].Int1);
        }

        [TestMethod]
        public void TestExecuteDeleteDistinctWhereDeletable()
        {
            var data = this.GetData();
            data[3].Int1 = 5;

            var actual = new SimpleSqlBuilder<SqlTestRecord>(SqlTestRecord.TableName)
                .Delete()
                .ExceptDistinctBy(s => s.Int2)
                .Where(s => s.Int1 == 5)
                .ExecuteDelete(data);

            Assert.AreEqual(1, actual);
            Assert.AreEqual(4, data.Count());
        }

        [TestMethod]
        public void TestInnerJoinExecute1()
        {
            var data1 = this.GetData();
            var data2 = this.GetData();

            var actual = new SimpleSqlBuilder<SqlTestNewRecord, SqlTestRecord, SqlTestRecord>()
                .From(SqlTestRecord.TableName, "TableName2")
                .SelectAs(tResult => tResult.Int3, (t1, t2) => t1.Int1 + t2.Int2)
                .InnerJoinOn((TableName, TableName2) => TableName.String1 == TableName2.String1)
                .ExecuteSelect(data1, data2)
                .ToList();

            Assert.AreEqual(5, actual.Count());
            Assert.AreEqual(104, actual[0].Int3);
            Assert.AreEqual(106, actual[4].Int3);
        }

        [TestMethod]
        public void TestInnerJoinExecuteSelect()
        {
            var data1 = this.GetData();
            var data2 = this.GetData();

            var actual = new SimpleSqlBuilder<SqlTestRecord, SqlTestRecord, SqlTestRecord>()
                .From(SqlTestRecord.TableName, "TableName2")
                .Select((t1, t2) => t1.Int1)
                .InnerJoinOn((TableName, TableName2) => TableName.String1 == TableName2.String1)
                .ExecuteSelect(data1, data2)
                .ToList();

            Assert.AreEqual(5, actual.Count());
            Assert.AreEqual(1, actual[0].Int1);
            Assert.AreEqual(2, actual[1].Int1);
        }

        [TestMethod]
        public void TestInnerJoinExecuteSelectAcross()
        {
            var data1 = this.GetData();
            var data2 = new List<SqlTestRecord>() { new SqlTestRecord { Int1 = 100, Int2 = 200 } };

            var actual = new SimpleSqlBuilder<SqlTestRecord, SqlTestRecord, SqlTestRecord>()
                .From(SqlTestRecord.TableName, "TableName2")
                .Select((t1, t2) => t1.Int1, (t1, t2) => t2.Int2)
                .InnerJoinOn((TableName, TableName2) => TableName2.Int2 == 200)
                .ExecuteSelect(data1, data2)
                .ToList();

            Assert.AreEqual(5, actual.Count());
            Assert.AreEqual(1, actual[0].Int1);
            Assert.AreEqual(200, actual[0].Int2);
            Assert.AreEqual(2, actual[1].Int1);
            Assert.AreEqual(200, actual[1].Int2);
        }

        [TestMethod]
        public void TestBoolNull()
        {
            var actual = new SimpleSqlBuilder<SqlTestRecord>(SqlTestRecord.TableName)
                .Where(s => s.Bool2.Value)
                .Build();

            var expected = $"SELECT * FROM {SqlTestRecord.TableName} WHERE Bool2 = 1";
            Assert.AreEqual(expected, actual.ToString());
        }

        [TestMethod]
        public void TestSelectAllExecute()
        {
            var actual = new SimpleSqlBuilder<SqlTestRecord>(SqlTestRecord.TableName)
                .Select()
                .Execute(this.GetData())
                .ToList();

            var expected = this.GetData();
            Assert.AreEqual(expected.Count(), actual.Count());
            Assert.AreEqual(expected[0].Int1, actual[0].Int1);
            Assert.AreEqual(expected[0].Int2, actual[0].Int2);
            Assert.AreEqual(expected[0].String1, actual[0].String1);
            Assert.AreEqual(expected[0].Bool1, actual[0].Bool1);
            Assert.AreEqual(expected[0].DateTime1, actual[0].DateTime1);
        }

        [TestMethod]
        public void TestIIFLate()
        {
            var actual = new SimpleSqlBuilder<SqlTestRecord>(SqlTestRecord.TableName) 
                .Select(s => s.Int1)
                .Where(s => s.Int1 > 2 ? s.Int2 == 102 : s.Int2 == 103)
                .Build();

            var expected = "SELECT Int1 FROM TableName WHERE 1 = IIF((Int1 > 2),IIF((Int2 = 102),1,0),IIF((Int2 = 103),1,0))";
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestIIFEarly()
        {
            var value = 5;
            var actual = new SimpleSqlBuilder<SqlTestRecord>(SqlTestRecord.TableName) 
                .Select(s => s.Int1)
                .SetOption("EarlyConditionalEval", true)
                .Where(s => value == 5 ? s.Int2 == 102 : s.Int2 == 103)
                .Build();

            var expected = "SELECT Int1 FROM TableName WHERE (Int2 = 102)";
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestIIFEarlyNested()
        {
            var value = 5;
            var value2 = 10;
            var value3 = 15;
            var actual = new SimpleSqlBuilder<SqlTestRecord>(SqlTestRecord.TableName) 
                .Select(s => s.Int1)
                .SetOption("EarlyConditionalEval", true)
                .Where(
                    s => value == 5 ? value2 == 10 ? s.Int2 == 102 : s.Int2 == 103 : value3 == 15 ? s.Bool1 : !s.Bool1)
                .Build();

            var expected = "SELECT Int1 FROM TableName WHERE (Int2 = 102)";
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestIIFEarlyNestedNots()
        {
            var value = 1;
            var value2 = 10;
            var value3 = 11;
            var actual = new SimpleSqlBuilder<SqlTestRecord>(SqlTestRecord.TableName) 
                .Select(s => s.Int1)
                .SetOption("EarlyConditionalEval", true)
                .Where(
                    s => value == 5 ? value2 == 10 ? s.Int2 == 102 : s.Int2 == 103 : value3 == 15 ? s.Bool1 : !s.Bool1)
                .Build();

            var expected = "SELECT Int1 FROM TableName WHERE Bool1 = 0";
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestNot()
        {
            var actual = new SimpleSqlBuilder<SqlTestRecord>(SqlTestRecord.TableName) 
                .Select(s => s.Int1)
                .Where(s => !s.Bool1)
                .Build();

            var expected = "SELECT Int1 FROM TableName WHERE Bool1 = 0";
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestIIFLatePurposeful()
        {
            var value = 5;
            var actual = new SimpleSqlBuilder<SqlTestRecord>(SqlTestRecord.TableName) 
                .Select(s => s.Int1)
                .SetOption("EarlyConditionalEval", false)
                .Where(s => value == 5 ? s.Int2 == 102 : s.Int2 == 103)
                .Build();

            var expected = "SELECT Int1 FROM TableName WHERE 1 = IIF((5 = 5),IIF((Int2 = 102),1,0),IIF((Int2 = 103),1,0))";
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void SelectAs2WhereConditional()
        {
            var condition = true;
            var actual = new SimpleSqlBuilder<SqlTestRecord, SqlTestRecord, SqlTestRecord>()
                .From(SqlTestRecord.TableName, SqlTestRecord.TableName)
                .SelectAs(m => m.Int1, (t, a) => t.Int1 + a.Int1)
                .SelectAs(m => m.Int2, (t, a) => a.Int2)
                .InnerJoinOn((t, a) => t.Int1 == a.Int1)
                .Where((t, a) => (condition ? t.Int2 > 101 : t.Int1 > 2) && a.String1.Contains("ring"))
                .ExecuteSelect(this.GetData(), this.GetData());

            Assert.AreEqual(3, actual.Count());
        }

        [TestMethod]
        public void ThreeTest()
        {
            var actual = new SimpleSqlBuilder<SqlTestRecord, SqlTestRecord, SqlTest2Record, SqlTest3Record>()
                .From(SqlTestRecord.TableName, SqlTest2Record.TableName, SqlTest3Record.TableName)
                .Select((a, b, c) => a.Int1, (a, b, c) => b.Int2, (a, b, c) => c.String1)
                .InnerJoin12On((a, b) => a.Int1 == b.Int1)
                .LeftOuterJoin13On((a, c) => a.Int1 == c.Int1)
                .Where((a, b, c) => b.Bool1)
                .Build();

            var expected = "SELECT TableName.Int1,TableName2.Int2,TableName3.String1 FROM TableName INNER JOIN TableName2 ON (TableName.Int1 = TableName2.Int1) LEFT OUTER JOIN TableName3 ON (TableName.Int1 = TableName3.Int1) WHERE TableName2.Bool1 = 1";
            Assert.AreEqual(expected, actual.ToString());
        }

        [TestMethod]

        public void TestIntNullInt()
        {
            var value = 1;
            var actual = new SimpleSqlBuilder<SqlTestRecord, SqlTestRecord, SqlTest2Record, SqlTest3Record>()
                .From(SqlTestRecord.TableName, SqlTest2Record.TableName, SqlTest3Record.TableName)
                .LeftOuterJoin12On((a, b) => a.Int1 == b.Int1 && a.Int4 == value)
                .Build();

            var expected = "SELECT * FROM TableName LEFT OUTER JOIN TableName2 ON ((TableName.Int1 = TableName2.Int1) AND (TableName.Int4 = 1))";
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestJoin3Build()
        {
            var actual = new SimpleSqlBuilder<SqlTestRecord, SqlTestRecord, SqlTest2Record, SqlTest3Record>()
                .From("TableName", "TableName2", "TableName3")
                .Select((a, b, c) => c.Int1, (a, b, c) => a.String1, (a, b, c) => a.Int2)
                .SelectAs(pp => pp.Int3, (a, b, c) => b.Int2)
                .InnerJoin12On((a, b) => a.Int1 == b.Int1)
                .LeftOuterJoin13On((a, c) => a.Int1 == c.Int1)
                .Where((a, b, c) => a.Bool1 && b.Bool1 && c.Bool1)
                .Build();

            var expected = @"SELECT TableName3.Int1,TableName.String1,TableName.Int2,TableName2.Int2 AS Int3 FROM TableName INNER JOIN TableName2 ON (TableName.Int1 = TableName2.Int1) LEFT OUTER JOIN TableName3 ON (TableName.Int1 = TableName3.Int1) WHERE ((TableName.Bool1 = 1 AND TableName2.Bool1 = 1) AND TableName3.Bool1 = 1)";

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestJoin4Build()
        {
            var actual = new SimpleSqlBuilder<SqlTestRecord, SqlTestRecord, SqlTest2Record, SqlTest3Record, SqlTest3Record>()
                .From("TableName", "TableName2", "TableName3", "TableName4")
                .Select((a, b, c, d) => c.Int1, (a, b, c, d) => a.String1, (a, b, c, d) => a.Int2)
                .SelectAs(pp => pp.Int3, (a, b, c, d) => b.Int2)
                .InnerJoin12On((a, b) => a.Int1 == b.Int1)
                .InnerJoin14On((a, b) => a.Int2 == b.Int2)
                .LeftOuterJoin13On((a, c) => a.Int1 == c.Int1)
                .Where((a, b, c, d) => a.Bool1 && b.Bool1 && c.Bool1 && d.Bool1)
                .Build();

            var expected = @"SELECT TableName3.Int1,TableName.String1,TableName.Int2,TableName2.Int2 AS Int3 FROM TableName INNER JOIN TableName2 ON (TableName.Int1 = TableName2.Int1) INNER JOIN TableName4 ON (TableName.Int2 = TableName4.Int2) LEFT OUTER JOIN TableName3 ON (TableName.Int1 = TableName3.Int1) WHERE (((TableName.Bool1 = 1 AND TableName2.Bool1 = 1) AND TableName3.Bool1 = 1) AND TableName4.Bool1 = 1)";

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestJoin3Execute()
        {
            var actual = new SimpleSqlBuilder<SqlTestRecord, SqlTestRecord, SqlTest2Record, SqlTest3Record>()
                .From("TableName", "TableName2", "TableName3")
                .Select((a, b, c) => c.Int1, (a, b, c) => a.String1, (a, b, c) => a.Int2)
                .SelectAs(pp => pp.Int3, (a, b, c) => b.Int2)
                .InnerJoin12On((a, b) => a.Int1 == b.Int1)
                .LeftOuterJoin13On((a, c) => a.Int1 == c.Int1)
                .Where((a, b, c) => a.Bool1 && b.Bool1 && c.Bool1)
                .Execute(this.GetData(), this.GetData2(), this.GetData3())
                .ToList();

            Assert.AreEqual(1, actual.Count());
            Assert.AreEqual(1, actual[0].Int1);
        }

        [TestMethod]
        public void TestJoin4Execute()
        {
            var actual = new SimpleSqlBuilder<SqlTestRecord, SqlTestRecord, SqlTest2Record, SqlTest3Record, SqlTest3Record>()
                .From("TableName", "TableName2", "TableName3", "TableName4")
                .Select((a, b, c, d) => c.Int1, (a, b, c, d) => a.String1, (a, b, c, d) => a.Int2)
                .SelectAs(pp => pp.Int3, (a, b, c, d) => b.Int2)
                .InnerJoin12On((a, b) => a.Int1 == b.Int1)
                .InnerJoin14On((a, b) => a.Int1 == b.Int1)
                .LeftOuterJoin13On((a, c) => a.Int1 == c.Int1)
                .Where((a, b, c, d) => a.Bool1 && b.Bool1 && c.Bool1 && d.Bool1)
                .Execute(this.GetData(), this.GetData2(), this.GetData3(), this.GetData4())
                .ToList();

            Assert.AreEqual(1, actual.Count());
            Assert.AreEqual(1, actual[0].Int1);
        }

        [TestMethod]
        public void TestJoin3Execute2()
        {
            var actual = new SimpleSqlBuilder<SqlTestRecord, SqlTestRecord, SqlTest2Record, SqlTest3Record>()
                .From("TableName", "TableName2", "TableName3")
                .Select((a, b, c) => c.Int1, (a, b, c) => a.String1, (a, b, c) => a.Int2)
                .SelectAs(pp => pp.Int3, (a, b, c) => b.Int2)
                .InnerJoin12On((a, b) => a.Int1 == b.Int1)
                .LeftOuterJoin13On((a, c) => a.Int1 == c.Int1)
                .Where((a, b, c) => a.Bool1 && b.Bool1)
                .Execute(this.GetData(), this.GetData2(), this.GetData3())
                .ToList();

            Assert.AreEqual(2, actual.Count());
            Assert.AreEqual(1, actual[0].Int1);
            Assert.AreEqual(null, actual[1].Int1);
        }

        [TestMethod]
        public void TestJoin3Execute3()
        {
            var actual = new SimpleSqlBuilder<SqlTestRecord, SqlTestRecord, SqlTest2Record, SqlTest3Record>()
                .From("TableName", "TableName2", "TableName3")
                .Select((a, b, c) => c.Int1, (a, b, c) => a.String1, (a, b, c) => a.Int2)
                .SelectAs(pp => pp.Int3, (a, b, c) => b.Int2)
                .InnerJoin12On((a, b) => a.Int1 == b.Int1)
                .LeftOuterJoin13On((a, c) => a.Int1 == c.Int1)
                .Execute(this.GetData(), this.GetData2(), this.GetData3())
                .ToList();

            Assert.AreEqual(5, actual.Count());
            Assert.AreEqual(1, actual[0].Int1);
            Assert.AreEqual(null, actual[2].Int1);
            Assert.AreEqual(201, actual[3].Int3);
            Assert.AreEqual(204, actual[4].Int3);
        }

        [TestMethod]
        public void TestJoin2Execute1()
        {
            var actual = new SimpleSqlBuilder<SqlTestRecord, SqlTestRecord, SqlTest2Record>()
                .From("TableName", "TableName2")
                .Select((a, b) => a.Int1, (a, b) => b.Int2, (a, b) => a.String1)
                .SelectAs(z => z.Int3, (a, b) => b.Int1)
                .InnerJoinOn((a, b) => a.Int1 == b.Int1)
                .Execute(this.GetData(), this.GetData2())
                .ToList();

            Assert.AreEqual(5, actual.Count());
            Assert.IsTrue(actual.Select(a => (int)a.Int1).SequenceEqual(new int[] { 1, 2, 3, 4, 4 }));
            Assert.IsTrue(actual.Select(a => (int)a.Int3).SequenceEqual(new int[] { 1, 2, 3, 4, 4 }));
        }

        [TestMethod]
        public void TestJoin2Execute1Where2()
        {
            var actual = new SimpleSqlBuilder<SqlTestRecord, SqlTestRecord, SqlTest2Record>()
                .From("TableName", "TableName2")
                .Select((a, b) => a.Int1, (a, b) => b.Int2, (a, b) => a.String1)
                .SelectAs(z => z.Int3, (a, b) => b.Int1)
                .InnerJoinOn((a, b) => a.Int1 == b.Int1)
                .Where((a, b) => a.Bool1 && b.Bool1)
                .Execute(this.GetData(), this.GetData2())
                .ToList();

            Assert.AreEqual(2, actual.Count());
            Assert.IsTrue(actual.Select(a => (int)a.Int1).SequenceEqual(new int[] { 1, 3 }));
            Assert.IsTrue(actual.Select(a => (int)a.Int3).SequenceEqual(new int[] { 1, 3 }));
            Assert.IsFalse(actual[0].Bool1);
        }

        // [TestMethod]
        // public void TestJoin2Execute1Where1()
        // {
        // var actual = new SimpleSqlBuilder<SqlTestRecord, SqlTestRecord, SqlTest2Record>()
        // .From("TableName", "TableName2")
        // .Select((a, b) => a.Int1, (a, b) => b.Int2, (a, b) => a.String1)
        // .SelectAs(z => z.Int3, (a, b) => b.Int1)
        // .RightOuterJoinOn((a, b) => a.Int1 == b.Int1)
        // .Where(z => z.Int3 > 10)
        // .Execute(this.GetData(), this.GetData2())
        // .ToList();

        // Assert.AreEqual(1, actual.Count());
        // Assert.AreEqual(15, actual[0].Int3);
        // Assert.AreEqual(null, actual[0].Int1);
        // }

        [TestMethod]
        public void TestJoin2Execute2()
        {
            var actual = new SimpleSqlBuilder<SqlTestRecord, SqlTestRecord, SqlTest2Record>()
                .From("TableName", "TableName2")
                .Select((a, b) => a.Int1, (a, b) => b.Int2, (a, b) => a.String1)
                .SelectAs(z => z.Int3, (a, b) => b.Int1)
                .LeftOuterJoinOn((a, b) => a.Int1 == b.Int1)
                .Execute(this.GetData(), this.GetData2())
                .ToList();

            Assert.AreEqual(6, actual.Count());
            Assert.AreEqual(null, actual[5].Int3);
            Assert.IsTrue(actual.Select(a => (int)a.Int1).SequenceEqual(new int[] { 1, 2, 3, 4, 4, 5 }));
        }

        [TestMethod]
        public void TestJoin2Execute3()
        {
            var actual = new SimpleSqlBuilder<SqlTestRecord, SqlTestRecord, SqlTest2Record>()
                .From("TableName", "TableName2")
                .Select((a, b) => a.Int1, (a, b) => b.Int2, (a, b) => a.String1)
                .SelectAs(z => z.Int3, (a, b) => b.Int1)
                .RightOuterJoinOn((a, b) => a.Int1 == b.Int1)
                .Execute(this.GetData(), this.GetData2())
                .ToList();

            Assert.AreEqual(6, actual.Count());
            Assert.AreEqual(null, actual[5].Int1);
            Assert.IsTrue(actual.Select(a => (int)a.Int3).SequenceEqual(new int[] { 1, 2, 3, 4, 4, 15 }));
        }

        [TestMethod]
        public void TestMissingMemberException()
        {
            var valid = false;
            try
            {
                var actual = new SimpleSqlBuilder<SqlTestRecord, SqlTestRecord, SqlTest3Record>()
                .From("TableName", "TableName3")
                    .Select((a, b) => b.Int4Extra)
                    .InnerJoinOn((a, b) => a.Int1 == b.Int1)
                    .Execute(this.GetData(), this.GetData3());
            }
            catch (MissingMemberException)
            {
                // expected
                valid = true;
            }
            catch (Exception)
            {
                Assert.Fail("Unexpected/incorrect exception received");
            }

            if (!valid)
            {
                Assert.Fail("MissingMemberException not received");
            }
        }

        [TestMethod]
        public void TestIgnoreAttribute()
        {
            var data = this.GetData4();
            var actual = new SimpleSqlBuilder<SqlTest4Record>(SqlTest4Record.TableName)
                .Select()
                .Execute(data)
                .ToList();

            Assert.AreEqual(null, actual[0].StringExtra);
        }

        [TestMethod]
        public void TestMultipleWheres()
        {
            var actual = new SimpleSqlBuilder<SqlTestRecord>(SqlTestRecord.TableName)
                .Select(s => s.Int1)
                .Where(s => s.Bool1)
                .Where(s => s.Int2 % 2 == 1)
                .Build();

            var expected = "SELECT Int1 FROM TableName WHERE Bool1 = 1 AND ((Int2 % 2) = 1)";
            Assert.AreEqual(expected, actual.ToString());
        }

        [TestMethod]
        public void TestMultipleWheresExecute()
        {
            var data = this.GetData();
            var actual = new SimpleSqlBuilder<SqlTestRecord>(SqlTestRecord.TableName)
                .Select(s => s.Int1)
                .Where(s => s.Bool1)
                .Where(s => s.Int2 % 2 == 1)
                .Execute(data)
                .ToList();

            Assert.AreEqual(2, actual.Count());
            Assert.AreEqual(1, actual[0].Int1);
            Assert.AreEqual(5, actual[1].Int1);
        }

        [TestMethod]
        public void TestClearWhere()
        {
            var actual = new SimpleSqlBuilder<SqlTestRecord>(SqlTestRecord.TableName)
                .Select(s => s.Int1)
                .Where(s => s.Bool1)
                .Where(s => s.Int2 % 2 == 1)
                .ClearWhere()
                .Build();

            var expected = "SELECT Int1 FROM TableName";
            Assert.AreEqual(expected, actual.ToString());
        }

        [TestMethod]
        public void TestIfClauseTrue()
        {
            var condition = true;
            var actual = new SimpleSqlBuilder<SqlTestRecord>(SqlTestRecord.TableName)
                .Select(s => s.Int1)
                .If(condition, ssb => ssb.Where(s => s.Bool1))
                .Where(s => s.Int2 % 2 == 1)
                .Build();

            var expected = "SELECT Int1 FROM TableName WHERE Bool1 = 1 AND ((Int2 % 2) = 1)";
            Assert.AreEqual(expected, actual.ToString());
        }

        [TestMethod]
        public void TestIfClauseFalse()
        {
            var condition = false;
            var actual = new SimpleSqlBuilder<SqlTestRecord>(SqlTestRecord.TableName)
                .Select(s => s.Int1)
                .If(condition, ssb => ssb.Where(s => s.Bool1))
                .Where(s => s.Int2 % 2 == 1)
                .Build();

            var expected = "SELECT Int1 FROM TableName WHERE ((Int2 % 2) = 1)";
            Assert.AreEqual(expected, actual.ToString());
        }

        [TestMethod]
        public void TestNotContains() //todo: params
        {
            var options = new SimpleSqlBuilderOptions { UseParameters = false };
            var actual = new SimpleSqlBuilder<SqlTestRecord>(options, SqlTestRecord.TableName)
                .Select(s => s.Int1)
                .Where(s => !s.String1.Contains("11"))
                .Build();

            var expected = "SELECT Int1 FROM TableName WHERE NOT String1 LIKE '%11%'";
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestUpdateGeneral() //todo: params
        {
            var options = new SimpleSqlBuilderOptions { UseParameters = false };
            var actual = new SimpleSqlBuilder<SqlTestRecord>(options, SqlTestRecord.TableName)
                .Update(s => s.Int2)
                .Values(s => 0)
                .Where(s => !s.String1.Contains("11"))
                .Build();

            var expected = "UPDATE TableName SET TableName.Int2 = 0 WHERE NOT String1 LIKE '%11%'";
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestUpdateGeneralExecute()
        {
            var data = this.GetData();
            var actual = new SimpleSqlBuilder<SqlTestRecord>(SqlTestRecord.TableName)
                .Update(s => s.Int2)
                .Values(s => 0)
                .Where(s => !s.String1.Contains("11"))
                .ExecuteNonQuery(data);

            Assert.AreEqual(3, actual);
            Assert.AreEqual(0, data[0].Int2);
            Assert.AreEqual(102, data[1].Int2);
            Assert.AreEqual(0, data[2].Int2);
        }

        [TestMethod]
        public void TestAddSTR()
        {
            var options = new SimpleSqlBuilderOptions { UseParameters = false };
            var actual = new SimpleSqlBuilder<SqlTestRecord>(options, SqlTestRecord.TableName)
                            .Select(s => s.String1)
                .Where(s => s.String1 == "Str" + "ing" + (s.Int2 + 20))
                .Build();

            var expected = "SELECT String1 FROM TableName WHERE (String1 = ('String' + STR((Int2 + 20)) ))";
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestAddSTRParams()
        {
            var options = new SimpleSqlBuilderOptions { UseParameters = true };
            var actual = new SimpleSqlBuilder<SqlTestRecord>(options, SqlTestRecord.TableName)
                            .Select(s => s.String1)
                .Where(s => s.String1 == "Str" + "ing" + (s.Int2 + 20))
                .Build();

            var expected = "SELECT String1 FROM TableName WHERE (String1 = (@Param1 + STR((Int2 + 20)) ))";
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestParamsDeclared()
        {
            var options = new SimpleSqlBuilderOptions { UseParameters = true };
            var actual = new SimpleSqlBuilder<SqlTestRecord>(options, SqlTestRecord.TableName)
                .Select(s => s.String1)
                .Where(s => s.String1 == "String111")
                .Build(true);

            var expected = @"DECLARE @Param1 VARCHAR(9)
SET @Param1 = 'String111'

SELECT String1 FROM TableName WHERE (String1 = @Param1)";
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestParamsUndeclared()
        {
            var options = new SimpleSqlBuilderOptions { UseParameters = true };
            var actual = new SimpleSqlBuilder<SqlTestRecord>(options, SqlTestRecord.TableName)
                .Select(s => s.String1)
                .Where(s => s.String1 == "String111")
                .Build();

            var expected = "SELECT String1 FROM TableName WHERE (String1 = @Param1)";
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestInnerRefToSelf()
        {
            var options = new SimpleSqlBuilderOptions { UseParameters = false };
            var actual = new SimpleSqlBuilder<SqlTestRecord, SqlTestRecord, SqlTestRecord>(options)
                .From(SqlTestRecord.TableName, SqlTestRecord.TableName)
                .Select((s1, s2) => s1.String1)
                .InnerJoinOn((s1, s2) => s1.Int1 == s2.Int1)
                .Where(s => s.String1 == "String111")
                .Build();

            var expected = "SELECT TableName.String1 FROM TableName INNER JOIN TableName TableName__1 ON (TableName.Int1 = TableName__1.Int1) WHERE (TableName.String1 = 'String111')";
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestInnerRefToSelfParams()
        {
            var options = new SimpleSqlBuilderOptions { UseParameters = true };
            var actual = new SimpleSqlBuilder<SqlTestRecord, SqlTestRecord, SqlTestRecord>(options)
                .From(SqlTestRecord.TableName, SqlTestRecord.TableName)
                .Select((s1, s2) => s1.String1)
                .InnerJoinOn((s1, s2) => s1.Int1 == s2.Int1)
                .Where(s => s.String1 == "String111")
                .Build();

            var expected = "SELECT TableName.String1 FROM TableName INNER JOIN TableName TableName__1 ON (TableName.Int1 = TableName__1.Int1) WHERE (TableName.String1 = @Param1)";
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestAllAsFields()
        {
            var data = this.GetData();
            var actual = new SimpleSqlBuilder<SqlTestNewRecord, SqlTestRecord, SqlTestRecord>()
                            .From(SqlTestRecord.TableName, SqlTestRecord.TableName)
                .SelectAs(tu => tu.String3, (t1, t3) => t3.String1)
                .SelectAs(tu => tu.Int3, (t1, t3) => t1.Int1)
                .InnerJoinOn((t1, t3) => t1.Int1 == t3.Int1)
                .Where((t1, t3) => t1.Int1 % 2 == 1)
                .Execute(data, data);

            Assert.AreEqual(3, actual.Count());
        }

        [TestMethod]
        public void TestStringNullFn()
        {
            var actual = new SimpleSqlBuilder<SqlTestRecord>(SqlTestRecord.TableName)
                .Select(s => s.String1)
                .Where(s => !string.IsNullOrEmpty(s.String1) && !string.IsNullOrWhiteSpace(s.String1))
                .Build();

            var expected = "SELECT String1 FROM TableName WHERE (NOT ISNULL(String1, '') = '' AND NOT TRIM(ISNULL(String1, '')) = '')";
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestIndexOf()
        {
            var options = new SimpleSqlBuilderOptions { UseParameters = false };
            var actual = new SimpleSqlBuilder<SqlTestRecord>(options, SqlTestRecord.TableName)
                .Select(s => s.String1)
                .Where(s => s.String1.IndexOf("Str") == 0)
                .Build();

            var expected = "SELECT String1 FROM TableName WHERE ((CHARINDEX('Str',String1)-1) = 0)";
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestIndexOfParams()
        {
            var options = new SimpleSqlBuilderOptions { UseParameters = true };
            var actual = new SimpleSqlBuilder<SqlTestRecord>(options, SqlTestRecord.TableName)
                .Select(s => s.String1)
                .Where(s => s.String1.IndexOf("Str") == 0)
                .Build();

            var expected = "SELECT String1 FROM TableName WHERE ((CHARINDEX(@Param1,String1)-1) = 0)";
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestCharIndexOf()
        {
            var options = new SimpleSqlBuilderOptions { UseParameters = false };
            var actual = new SimpleSqlBuilder<SqlTestRecord>(options, SqlTestRecord.TableName)
                .Select(s => s.String1)
                .Where(s => s.String1.CHARINDEX("Str") == 1)
                .Build();

            var expected = "SELECT String1 FROM TableName WHERE (CHARINDEX('Str',String1) = 1)";
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestCharIndexOfParams()
        {
            var options = new SimpleSqlBuilderOptions { UseParameters = true };
            var actual = new SimpleSqlBuilder<SqlTestRecord>(options, SqlTestRecord.TableName)
                .Select(s => s.String1)
                .Where(s => s.String1.CHARINDEX("Str") == 1)
                .Build();

            var expected = "SELECT String1 FROM TableName WHERE (CHARINDEX(@Param1,String1) = 1)";
            Assert.AreEqual(expected, actual);
        }

        // here

        [TestMethod]
        public void TestStrEquals() //todo: params
        {
            var options = new SimpleSqlBuilderOptions { UseParameters = false };
            var actual = new SimpleSqlBuilder<SqlTestRecord>(options, SqlTestRecord.TableName)
                .Select(s => s.String1)
                .Where(s => s.String1.Equals("String123"))
                .Build();

            var expected = "SELECT String1 FROM TableName WHERE 'String123' = String1";
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestStrLower() //todo: params
        {
            var options = new SimpleSqlBuilderOptions { UseParameters = false };
            var actual = new SimpleSqlBuilder<SqlTestRecord>(options, SqlTestRecord.TableName)
                .Select(s => s.String1)
                .Where(s => s.String1.ToLower() == "string123")
                .Build();

            var expected = "SELECT String1 FROM TableName WHERE (LOWER(String1) = 'string123')";
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestSubstring() //todo: params
        {
            var options = new SimpleSqlBuilderOptions { UseParameters = false };
            var actual = new SimpleSqlBuilder<SqlTestRecord>(options, SqlTestRecord.TableName)
                .Select(s => s.String1)
                .Where(s => s.String1.Substring(0, 1) == "S")
                .Build();

            var expected = "SELECT String1 FROM TableName WHERE (SUBSTRING(String1,1,1) = 'S')";
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestSUBSTRING() //todo: params
        {
            var options = new SimpleSqlBuilderOptions { UseParameters = false };
            var actual = new SimpleSqlBuilder<SqlTestRecord>(options, SqlTestRecord.TableName)
                .Select(s => s.String1)
                .Where(s => s.String1.SUBSTRING(1, 1) == "S")
                .Build();

            var expected = "SELECT String1 FROM TableName WHERE (SUBSTRING(String1,1,1) = 'S')";
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestInadvertentDowncast()
        {
            var valid = false;
            try
            {
                var sql = new SimpleSqlBuilder<SqlTestRecord, SqlTestRecord, SqlTestRecord>()
                .From(SqlTestRecord.TableName, SqlTestRecord.TableName)
                    .Select((l1, l2) => l1.String1)
                    .InnerJoinOn((l1, l2) => l1.Int1 == l2.Int2)
                    .OrderByDescending(z => z.String1)
                    .Execute(this.GetData());
            }
            catch (InadvertentDowncastException)
            {
                // correct
                valid = true;
            }
            catch (Exception)
            {
                Assert.Fail("Incorrect exception");
            }

            if (!valid)
            {
                Assert.Fail("Exception not thrown");
            }
        }

        [TestMethod]
        public void TestEqualsTrue()
        {
            var actual = new SimpleSqlBuilder<SqlTestRecord>(SqlTestRecord.TableName)
                .Select(s => s.String1)
                .Where(s => s.Bool2 == true)
                .Build();

            Assert.AreEqual("SELECT String1 FROM TableName WHERE (Bool2 = 1)", actual);
        }

        [TestMethod]
        public void TestNullRecordsTemplate()
        {
            var actual = new SimpleSqlBuilder<SqlTestRecord>(SqlTestRecord.TableName)
                .Insert(s => s.Int3)
                .Values(s => 2)
                .Build();

            Assert.AreEqual("INSERT INTO TableName (Int3) VALUES (2)", actual);
        }

        [TestMethod]
        public void TestWhereOnInsertThrow()
        {
            var valid = false;
            try
            {
                var actual = new SimpleSqlBuilder<SqlTestRecord>(SqlTestRecord.TableName)
                .Insert(s => s.Int3)
                    .Values(s => 2)
                    .Where(s => s.Bool2.Value)
                    .Build();
            }
            catch (IncompatibleClauseException)
            {
                // correct
                valid = true;
            }
            catch (Exception)
            {
                Assert.Fail("Incorrect exception");
            }

            if (!valid)
            {
                Assert.Fail("Exception not thrown");
            }
        }

        [TestMethod]
        public void TestZeroRecordsEmpty()
        {
            var data = new List<SqlTestRecord>() { };
            var actual = new SimpleSqlBuilder<SqlTestRecord>(SqlTestRecord.TableName)
                .Insert(data, s => s.Int3)
                .Values(s => 2)
                .Build();

            Assert.IsTrue(string.IsNullOrEmpty(actual));
        }

        [TestMethod]
        public void InsertFromSelectAll()
        {
            var mergeInsert = new SimpleSqlBuilder<SqlTestRecord>(SqlTestRecord.TableName)
                .Insert(s => s.From("Temp" + SqlTestRecord.TableName).Select())
                .Build();

            Assert.AreEqual("INSERT INTO TableName SELECT * FROM TempTableName", mergeInsert);
        }

        [TestMethod]
        public void InsertFromSelectSelectFields()
        {
            var mergeInsert = new SimpleSqlBuilder<SqlTestRecord>(SqlTestRecord.TableName)
                .Insert(s => s.From("Temp" + SqlTestRecord.TableName).Select(s => s.String1, s => s.Bool1))
                .Build();

            Assert.AreEqual(
                "INSERT INTO TableName (String1,Bool1) SELECT String1,Bool1 FROM TempTableName",
                mergeInsert);
        }

        [TestMethod]
        public void InsertFromSelectInsertFieldsWithoutSelect()
        {
            try
            {
                var mergeInsert = new SimpleSqlBuilder<SqlTestRecord>(SqlTestRecord.TableName)
                .Insert(s => s.From("Temp" + SqlTestRecord.TableName).Select(), s => s.String1, s => s.Bool1)
                    .Build();
            }
            catch
            {
                // good
                return;
            }

            Assert.Fail("No exception for too many fields");
        }

        [TestMethod]
        public void InsertFromSelectBothFields()
        {
            var mergeInsert = new SimpleSqlBuilder<SqlTestRecord>(SqlTestRecord.TableName)
                .Insert(
                    ss => ss.From("Temp" + SqlTestRecord.TableName).Select(s => s.String1, s => s.Bool1),
                    s => s.String1,
                    s => s.Bool1)
                .Build();

            Assert.AreEqual(
                "INSERT INTO TableName (String1,Bool1) SELECT String1,Bool1 FROM TempTableName",
                mergeInsert);
        }

        [TestMethod]
        public void InsertFromSelectAllExecute()
        {
            var source = this.GetData();
            var target = new List<SqlTestRecord>();
            var sources = new Dictionary<string, SourceData>();
            sources["TempTableName"] = SourceData.Create("TempTableName", source);

            var mergeInsert = new SimpleSqlBuilder<SqlTestRecord>(SqlTestRecord.TableName)
                .Insert(s => s.From("Temp" + SqlTestRecord.TableName).Select())
                .ExecuteNonQuery(target, sources);

            Assert.AreEqual(5, mergeInsert);
            Assert.AreEqual(5, target.Count());
            Assert.AreEqual("String123", target[0].String1);
        }

        [TestMethod]
        public void ComplexBuilder1()
        {
            var options = new SimpleSqlBuilderOptions { UseParameters = false };
            var actual = new SimpleSqlBuilder<TargetRecord, SourceRecord1, SourceRecord2, SourceRecord2, SourceRecord3>(
                options)
                .From(
                    SourceRecord1.TableName,
                    SourceRecord2.TableName,
                    SourceRecord2.TableName,
                    SourceRecord3.TableName)
                .Select((s1, s2a, s2b, s3) => s1.Int1)
                .SelectAs(z => z.SameString, (s1, s2a, s2b, s3) => s3.SameString)
                .SelectAs(z => z.Int2, (s1, s2a, s2b, s3) => s2a.String2.Substring(s2a.String2.IndexOf(":") + 1, 100))
                .SelectAs(z => z.String3, (s1, s2a, s2b, s3) => s3.String3)
                .InnerJoin12On(
                    (s1, s2a) => s1.SameString == s2a.SameString.Substring(s2a.SameString.IndexOf(":") + 1, 100) &&
                        s2a.Bool2.Value)
                .InnerJoin13On((s1, s2b) => s1.Int1 == s2b.Int2)
                .InnerJoin14On((s1, s3) => s1.Int1 == s3.Int3)
                .Where((s1, s2a, s2b, s3) => s2b.String2 == "text" && !s3.String3.EndsWith(":bad"))
                .Build();

            var expected = @"SELECT Source1.Int1,Source3.SameString AS SameString,SUBSTRING(Source2.String2,((CHARINDEX(':',Source2.String2)-1) + 1)+1,100) AS Int2,Source3.String3 AS String3 FROM Source1 INNER JOIN Source2 ON ((Source1.SameString = SUBSTRING(Source2.SameString,((CHARINDEX(':',Source2.SameString)-1) + 1)+1,100)) AND .Bool2 = 1) INNER JOIN Source2 Source2__1 ON (Source1.Int1 = Source2__1.Int2) INNER JOIN Source3 ON (Source1.Int1 = Source3.Int3) WHERE ((Source2__1.String2 = 'text') AND NOT Source3.String3 LIKE '%:bad')";
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void ComplexBuilder1Param()
        {
            var options = new SimpleSqlBuilderOptions { UseParameters = true };
            var actual = new SimpleSqlBuilder<TargetRecord, SourceRecord1, SourceRecord2, SourceRecord2, SourceRecord3>(
                options)
                .From(
                    SourceRecord1.TableName,
                    SourceRecord2.TableName,
                    SourceRecord2.TableName,
                    SourceRecord3.TableName)
                .Select((s1, s2a, s2b, s3) => s1.Int1)
                .SelectAs(z => z.SameString, (s1, s2a, s2b, s3) => s3.SameString)
                .SelectAs(z => z.Int2, (s1, s2a, s2b, s3) => s2a.String2.Substring(s2a.String2.IndexOf(":") + 1, 100))
                .SelectAs(z => z.String3, (s1, s2a, s2b, s3) => s3.String3)
                .InnerJoin12On(
                    (s1, s2a) => s1.SameString == s2a.SameString.Substring(s2a.SameString.IndexOf(":") + 1, 100) &&
                        s2a.Bool2.Value)
                .InnerJoin13On((s1, s2b) => s1.Int1 == s2b.Int2)
                .InnerJoin14On((s1, s3) => s1.Int1 == s3.Int3)
                .Where((s1, s2a, s2b, s3) => s2b.String2 == "text" && !s3.String3.EndsWith(":bad"))
                .Build();

            var expected = @"SELECT Source1.Int1,Source3.SameString AS SameString,SUBSTRING(Source2.String2,((CHARINDEX(@Param1,Source2.String2)-1) + 1)+1,100) AS Int2,Source3.String3 AS String3 FROM Source1 INNER JOIN Source2 ON ((Source1.SameString = SUBSTRING(Source2.SameString,((CHARINDEX(@Param4,Source2.SameString)-1) + 1)+1,100)) AND .Bool2 = 1) INNER JOIN Source2 Source2__1 ON (Source1.Int1 = Source2__1.Int2) INNER JOIN Source3 ON (Source1.Int1 = Source3.Int3) WHERE ((Source2__1.String2 = @Param2) AND NOT Source3.String3 LIKE '%'+@Param3)";
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestBracketedFields()
        {
            var actual = new SimpleSqlBuilder<SourceRecordWithUser>(SourceRecordWithUser.TableName)
                .Select()
                .Where(m => string.IsNullOrEmpty(m.User))
                .Build();

            var expected = "SELECT * FROM TableName WHERE ISNULL([User], '') = ''";
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestBracketedFields2()
        {
            var options = new SimpleSqlBuilderOptions { UseParameters = false };
            var actual = new SimpleSqlBuilder<SourceRecordWithUser>(options, SourceRecordWithUser.TableName)
                .Select()
                .Where(m => m.User == "ABC")
                .Build();

            var expected = "SELECT * FROM TableName WHERE ([User] = 'ABC')";
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestBracketedFields2Params()
        {
            var options = new SimpleSqlBuilderOptions { UseParameters = true };
            var actual = new SimpleSqlBuilder<SourceRecordWithUser>(options, SourceRecordWithUser.TableName)
                .Select()
                .Where(m => m.User == "ABC")
                .Build();

            var expected = "SELECT * FROM TableName WHERE ([User] = @Param1)";
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestTracing()
        {
            var sb = new StringBuilder();
            var data = this.GetData();
            var actual = new SimpleSqlBuilder<SqlTestNewRecord, SqlTestRecord, SqlTestRecord>()
                .TraceTo(sb)
                .From(SqlTestRecord.TableName, SqlTestRecord.TableName)
                .SelectAs(tu => tu.String3, (t1, t3) => t3.String1)
                .SelectAs(tu => tu.Int3, (t1, t3) => t1.Int1)
                .InnerJoinOn((t1, t3) => t1.Int1 == t3.Int1)
                .Where((t1, t3) => t1.Int1 % 2 == 1)
                .Execute(data, data);

            Assert.IsTrue(sb.ToString().Length > 0);
        }

        [TestMethod]
        public void TestDistinct()
        {
            var data = this.GetData();

            var query = new SimpleSqlBuilder<SqlTestRecord>()
                .From(SqlTestRecord.TableName)
                .Select(r => r.Int2)
                .Distinct();

            var build = query.Build();
            var actual = query.Execute(data);
            Debugger.Break();

            Assert.AreEqual(3, actual.Count(), "Wrong number of records");
            Assert.AreEqual(1, actual.Where(a => a.Int2 == 101).Count(), "Wrong number of records - duplicates found");
            Assert.AreEqual(1, actual.Where(a => a.Int2 == 102).Count(), "Wrong number of records - duplicates found");
            Assert.AreEqual(1, actual.Where(a => a.Int2 == 103).Count(), "Wrong number of records - duplicates found");
        }

        [TestMethod]
        public void TestSingleQuote()
        {
            var options = new SimpleSqlBuilderOptions { UseParameters = false };
            var actual = new SimpleSqlBuilder<SqlTestRecord>(options)
                .From(SqlTestRecord.TableName)
                .Select(r => r.Int1)
                .OrderByDescending(r => r.Int2)
                .Take(10)
                .Where(r => r.String1 == "AB'C%D_E\\F")
                .Build();

            var expected = "SELECT TOP 10 Int1 FROM TableName WHERE (String1 = 'AB''C%D_E\\F') ORDER BY Int2 DESC";

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestUnderscoreForEquals()
        {
            var test = "A_Value";
            var options = new SimpleSqlBuilderOptions { UseParameters = false };
            var actual = new SimpleSqlBuilder<SqlTestRecord>(options)
                .From(SqlTestRecord.TableName)
                .Select(r => r.Int1)
                .Take(10)
                .Where(r => r.String1 == test)
                .Build();

            var expected = "SELECT TOP 10 Int1 FROM TableName WHERE (String1 = 'A_Value')";

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestUnderscoreForLike()
        {
            var test = "A_Value";
            var options = new SimpleSqlBuilderOptions { UseParameters = false };
            var actual = new SimpleSqlBuilder<SqlTestRecord>(options)
                .From(SqlTestRecord.TableName)
                .Select(r => r.Int1)
                .Take(10)
                .Where(r => r.String1.Contains(test))
                .Build();

            var expected = "SELECT TOP 10 Int1 FROM TableName WHERE String1 LIKE '%A_Value%'";

            Assert.AreEqual(expected, actual);
        }

        // [TestMethod]
        public void TestBacktick()
        {
            var dt = DateTime.MinValue;
            var newRecord = new SqlTestRecord
            {
                Int1 = 3,
                Int2 = 202,
                String1 = "This `thing` is a test",
                Bool1 = false,
                DateTime1 = dt
            };

            var actual = new SimpleSqlBuilder<SqlTestRecord>(SqlTestRecord.TableName)
                .Where(s => s.Int1 == 3)
                .Update(newRecord, s => s.String1)
                .Build();

            var expected = "UPDATE TableName SET Int2 = 202, Bool1 = 0 WHERE Int1 = 3";

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestMultiSameNames()
        {
            var build = new SimpleSqlBuilder<SqlTestRecord, SqlTestRecord, SqlTest2Record, SqlTest2Record>()
                .From(
                    SqlTestRecord.TableName,
                    SqlTest2Record.TableName,
                    SqlTest2Record.TableName)
                .InnerJoin12On((e, tx) => e.Int1 == tx.Int1)
                .InnerJoin13On((e, tz) => e.Int2 == tz.Int2)
                .Select(
                    (e, tx, tz) => e.Int1,
                    (e, tx, tz) => e.String1,
                    (e, tx, tz) => tx.String2)
                .SelectAs(r => r.DateTime1, (e, tx, tz) => tz.DateTime1)
                .Where((e, tx, tz) => e.Int2 != null);

            var query = build.Build();
            var expected = @"SELECT TableName.Int1,TableName.String1,TableName2.String2,TableName2__1.DateTime1 AS DateTime1 FROM TableName INNER JOIN TableName2 ON (TableName.Int1 = TableName2.Int1) INNER JOIN TableName2 TableName2__1 ON (TableName.Int2 = TableName2__1.Int2) WHERE (TableName.Int2 IS NOT NULL)";
            Assert.AreEqual(expected, query);
        }

        [TestMethod]
        public void TestMulti3SameNames()
        {
            var build = new SimpleSqlBuilder<SqlTestRecord, SqlTestRecord, SqlTest2Record, SqlTest2Record, SqlTest2Record>()
                .From(
                    SqlTestRecord.TableName,
                    SqlTest2Record.TableName,
                    SqlTest2Record.TableName,
                    SqlTest2Record.TableName)
                .InnerJoin12On((e, tx) => e.Int1 == tx.Int1)
                .InnerJoin13On((e, ty) => e.Int2 == ty.Int2)
                .InnerJoin14On((e, tz) => e.Int2 == tz.Int2)
                .Select(
                    (e, tx, ty, tz) => e.Int1,
                    (e, tx, ty, tz) => e.String1,
                    (e, tx, ty, tz) => tx.String2)
                .SelectAs(r => r.DateTime1, (e, tx, ty, tz) => tz.DateTime1)
                .Where((e, tx, ty, tz) => e.Int2 != null);

            var query = build.Build();
            var expected = @"SELECT TableName.Int1,TableName.String1,TableName2.String2,TableName2__2.DateTime1 AS DateTime1 FROM TableName INNER JOIN TableName2 ON (TableName.Int1 = TableName2.Int1) INNER JOIN TableName2 TableName2__1 ON (TableName.Int2 = TableName2__1.Int2) INNER JOIN TableName2 TableName2__2 ON (TableName.Int2 = TableName2__2.Int2) WHERE (TableName.Int2 IS NOT NULL)";
            Assert.AreEqual(expected, query);
        }

        public class SourceRecordWithUser
        {
            public const string TableName = "TableName";

            public int Id
            {
                get;
                set;
            }

            public string User
            {
                get;
                set;
            }
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

        private List<SqlTest2Record> GetData2()
        {
            var dt = DateTime.MaxValue;
            var data = new List<SqlTest2Record>();
            data.Add(new SqlTest2Record { Int1 = 1, Int2 = 203, String1 = "String123b", Bool1 = true, DateTime1 = dt });
            data.Add(new SqlTest2Record { Int1 = 2, Int2 = 202, String1 = "String111b", Bool1 = false, DateTime1 = dt });
            data.Add(new SqlTest2Record { Int1 = 3, Int2 = 202, String1 = "String022b", Bool1 = true, DateTime1 = dt });
            data.Add(new SqlTest2Record { Int1 = 4, Int2 = 201, String1 = "String112b", Bool1 = false, DateTime1 = dt });
            data.Add(new SqlTest2Record { Int1 = 4, Int2 = 204, String1 = "String444b", Bool1 = false, DateTime1 = dt });
            data.Add(new SqlTest2Record { Int1 = 15, Int2 = 201, String1 = "String132b", Bool1 = true, DateTime1 = dt });
            return data;
        }

        private List<SqlTest3Record> GetData3()
        {
            var dt = DateTime.MaxValue;
            var data = new List<SqlTest3Record>();
            data.Add(new SqlTest3Record { Int1 = 1, Int2 = 303, String1 = "String123c", Bool1 = true, DateTime1 = dt });
            data.Add(new SqlTest3Record { Int1 = 2, Int2 = 302, String1 = "String111c", Bool1 = false, DateTime1 = dt });
            data.Add(new SqlTest3Record { Int1 = 10, Int2 = 302, String1 = "String022c", Bool1 = true, DateTime1 = dt });
            data.Add(
                new SqlTest3Record { Int1 = 11, Int2 = 301, String1 = "String112c", Bool1 = false, DateTime1 = dt });
            data.Add(new SqlTest3Record { Int1 = 15, Int2 = 301, String1 = "String132c", Bool1 = true, DateTime1 = dt });
            return data;
        }

        private List<SqlTest4Record> GetData4()
        {
            var dt = DateTime.MaxValue;
            var data = new List<SqlTest4Record>();
            data.Add(
                new SqlTest4Record
                {
                    Int1 = 1,
                    Int2 = 303,
                    String1 = "String123c",
                    Bool1 = true,
                    DateTime1 = dt,
                    StringExtra = "not this1"
                });
            data.Add(
                new SqlTest4Record
                {
                    Int1 = 2,
                    Int2 = 302,
                    String1 = "String111c",
                    Bool1 = false,
                    DateTime1 = dt,
                    StringExtra = "not this2"
                });
            data.Add(
                new SqlTest4Record
                {
                    Int1 = 10,
                    Int2 = 302,
                    String1 = "String022c",
                    Bool1 = true,
                    DateTime1 = dt,
                    StringExtra = "not this3"
                });
            data.Add(
                new SqlTest4Record
                {
                    Int1 = 11,
                    Int2 = 301,
                    String1 = "String112c",
                    Bool1 = false,
                    DateTime1 = dt,
                    StringExtra = "not this4"
                });
            data.Add(
                new SqlTest4Record
                {
                    Int1 = 15,
                    Int2 = 301,
                    String1 = "String132c",
                    Bool1 = true,
                    DateTime1 = dt,
                    StringExtra = "not this5"
                });
            return data;
        }
    }
}
