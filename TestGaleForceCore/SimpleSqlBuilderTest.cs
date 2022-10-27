﻿namespace TestGaleForceCore
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
            var actual = new SimpleSqlBuilder<SqlTestRecord>()
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
        /// Tests Select with Where, Contains.
        /// </summary>
        [TestMethod]
        public void TestSelectWhereStringContains()
        {
            var actual = new SimpleSqlBuilder<SqlTestRecord>()
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
        public void TestSelectWhereStringContainsLocalVars()
        {
            var abcdefg = "ABCDEFG";
            var abc = "ABC";
            var actual = new SimpleSqlBuilder<SqlTestRecord>()
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
        public void TestWhereBool()
        {
            var user = "abc@def.com";
            var actual = new SimpleSqlBuilder<SqlTestRecord>()
                .From(SqlTestRecord.TableName)
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
                .From(SqlTestRecord.TableName)
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
            var user = "abc@def.com";
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
        public void TestWhereAdditive()
        {
            var user = "abc@def";
            var actual = new SimpleSqlBuilder<SqlTestRecord>()
                .From(SqlTestRecord.TableName)
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
        public void TestWhereAdditiveWithField()
        {
            var user = "abc@def";
            var actual = new SimpleSqlBuilder<SqlTestRecord>()
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
        public void TestWhere2()
        {
            var actual = new SimpleSqlBuilder<SqlTestNewRecord, SqlTestRecord, SqlTestRecord>()
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

            var actual = new SimpleSqlBuilder<SqlTestNewRecord, SqlTestRecord, SqlTestRecord>()
                .From(SqlTestRecord.TableName, "TableName2")
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
            var data = this.GetData();

            var actual = new SimpleSqlBuilder<SqlTestNewRecord, SqlTestRecord, SqlTestRecord>()
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
        public void TestSelectAndSelectAsDifferentTable()
        {
            var data = this.GetData();

            var actual = new SimpleSqlBuilder<SqlTestNewRecord, SqlTestRecord, SqlTestRecord>()
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
        public void TestSelectAndSelectAsSameTable()
        {
            var data = this.GetData();

            var actual = new SimpleSqlBuilder<SqlTestRecord, SqlTestRecord, SqlTestRecord>()
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
                .Where(a => intList.Contains(a.Int1))
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
        public void TestOuterJoinSelect()
        {
            var actual = new SimpleSqlBuilder<SqlTestNewRecord, SqlTestRecord, SqlTestRecord>()
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
        public void TestOuterJoinMultiSelect()
        {
            var actual = new SimpleSqlBuilder<SqlTestNewRecord, SqlTestRecord, SqlTestRecord>()
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

        [TestMethod]
        public void TestDeepWhere()
        {
            var emotionSet = new string[] { "ABC" };
            var from = 0;
            var toto = 10;
            var piece = "DEF";
            var actual = new SimpleSqlBuilder<SqlTestNewRecord, SqlTestRecord, SqlTestRecord>()
                    .From(SqlTestRecord.TableName, SqlTestRecord.TableName)
                .Select((e, t) => e.Int3, (e, t) => t.String2)
                .InnerJoinOn((e, t) => e.Int3 == t.Int3)
                .Where(
                    (e, t) => t.Int1 > from &&
                        t.Int1 < toto &&
                        (t.String2.Contains(piece + ":Reviewed") || t.String2.Contains(piece + "Non-Actionable")) &&
                        emotionSet.Contains(e.String1))
                .OrderByDescending(ep => ep.Int3)
                .Build();

            var expected = "SELECT TableName.Int3,TableName.String2 " +
                "FROM TableName " +
                "INNER JOIN TableName ON (TableName.Int3 = TableName.Int3) " +
                "WHERE ((((TableName.Int1 > 0) AND (TableName.Int1 < 10)) " +
                "AND (TableName.String2 LIKE '%CONCAT('DEF',':Reviewed')%' " +
                "OR TableName.String2 LIKE '%CONCAT('DEF','Non-Actionable')%')) " +
                "AND TableName.String1 IN ('ABC')) ORDER BY Int3 DESC";

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestOrderBy3() //fails at the moment - not built yet
        {
            var emotionSet = new string[] { "ABC" };
            var from = 0;
            var toto = 10;
            var piece = "DEF";
            var actual = new SimpleSqlBuilder<SqlTestNewRecord, SqlTestRecord, SqlTestRecord>()
                    .From(SqlTestRecord.TableName, SqlTestRecord.TableName)
                .Select((e, t) => e.Int3, (e, t) => t.String2)
                .InnerJoinOn((e, t) => e.Int3 == t.Int3)
                .OrderBy((e, t) => e.Int3)
                .Build();

            var expected = "SELECT TableName.Int3,TableName.String2 " +
                "FROM TableName " +
                "INNER JOIN TableName ON (TableName.Int3 = TableName.Int3) " +
                "ORDER BY TableName.Int3 DESC";

            // Assert.AreEqual(expected, actual);
        }

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
                .Update(s => s.Int2, s => s.Bool1)
                .Build(newRecord);

            var expected = "UPDATE TableName SET Int2 = 202, Bool1 = 0 WHERE Int1 = 3";

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestUpdateWithWhere()
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
                .Update(s => s.Int2, s => s.Bool1)
                .Where(s => s.String1 == "String022")
                .Build(newRecord);

            var expected = "UPDATE TableName SET Int2 = 202, Bool1 = 0 WHERE Int1 = 3 AND (String1 = 'String022')";

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestUpdateMultiple()
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

            newRecord = new SqlTestRecord
            {
                Int1 = 4,
                Int2 = 201,
                String1 = "String999",
                Bool1 = true,
                DateTime1 = dt
            };

            records.Add(newRecord);

            var actual = new SimpleSqlBuilder<SqlTestRecord>(SqlTestRecord.TableName)
                .Match(s => s.Int1)
                .Update(s => s.Int2, s => s.Bool1)
                .Where(s => s.String1 == "String022")
                .Build(records);

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

            newRecord = new SqlTestRecord
            {
                Int1 = 4,
                Int2 = 201,
                String1 = "String999",
                Bool1 = true,
                DateTime1 = dt
            };

            source.Add(newRecord);

            var target = this.GetData();
            var count = new SimpleSqlBuilder<SqlTestRecord>(SqlTestRecord.TableName)
                .Match(s => s.Int1)
                .Update(s => s.Int2, s => s.Bool1)
                .Where(s => s.String1 == "String022")
                .ExecuteUpdate(source, target);

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

            newRecord = new SqlTestRecord
            {
                Int1 = 4,
                Int2 = 201,
                String1 = "String999",
                Bool1 = true,
                DateTime1 = dt
            };

            source.Add(newRecord);

            var target = this.GetData();
            var count = new SimpleSqlBuilder<SqlTestRecord>(SqlTestRecord.TableName)
                .Insert(s => s.Int2, s => s.Bool1)
                .ExecuteInsert(source, target);

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

            newRecord = new SqlTestRecord
            {
                Int1 = 4,
                Int2 = 201,
                String1 = "String999",
                Bool1 = true,
                DateTime1 = dt
            };

            source.Add(newRecord);

            var target = this.GetData();
            var count = new SimpleSqlBuilder<SqlTestRecord>(SqlTestRecord.TableName)
                .Insert(s => s.Int2, s => s.Bool1, s => s.Int1)
                .Values(s => 2 + s.Int2, s => !s.Bool1, s => 6)
                .ExecuteInsert(source, target);

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

            newRecord = new SqlTestRecord
            {
                Int1 = 4,
                Int2 = 201,
                String1 = "String999",
                Bool1 = true,
                DateTime1 = dt
            };

            source.Add(newRecord);

            var target = this.GetData();
            var actual = new SimpleSqlBuilder<SqlTestRecord>(SqlTestRecord.TableName)
                .Insert(s => s.Int2, s => s.Bool1)
                .Build(source);

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

            newRecord = new SqlTestRecord
            {
                Int1 = 4,
                Int2 = 201,
                String1 = "String999",
                Bool1 = true,
                DateTime1 = dt
            };

            source.Add(newRecord);

            var target = this.GetData();
            var actual = new SimpleSqlBuilder<SqlTestRecord>(SqlTestRecord.TableName)
                .Insert(s => s.Int1, s => s.Int2, s => s.Bool1)
                .Values(s => s.Int1, s => 10, s => !s.Bool1)
                .Build(source);

            var expected = @"INSERT INTO TableName (Int1,Int2,Bool1) VALUES (3,10,1);
INSERT INTO TableName (Int1,Int2,Bool1) VALUES (4,10,0);
GO;";

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestMerge()
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

            newRecord = new SqlTestRecord
            {
                Int1 = 4,
                Int2 = 201,
                String1 = "String999",
                Bool1 = true,
                DateTime1 = dt
            };

            source.Add(newRecord);

            var target = this.GetData();
            var actual = new SimpleSqlBuilder<SqlTestRecord>(SqlTestRecord.TableName)
                .MergeInto(SqlTestRecord.TableName)
                .Match((s, t) => s.Int1 == t.Int1 && s.String1 == "String123")
                .Where(s => s.Int2 == 101)
                .WhenMatched(s => s.Update(s => s.Int2))
                .WhenNotMatched(s => s.Insert(s => s.Int1, s => s.Int2, s => s.String1))
                .Build(source);

            var expected = @"MERGE INTO TableName AS Target USING TableName AS Source ON ((Source.Int1 = Target.Int1) AND (Source.String1 = 'String123')) WHEN MATCHED THEN UPDATE SET Target.Int2 = Source.Int2 WHEN NOT MATCHED THEN INSERT (Int1,Int2,String1) VALUES (Source.Int1, Source.Int2, Source.String1)";

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestMergeValues()
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

            newRecord = new SqlTestRecord
            {
                Int1 = 4,
                Int2 = 201,
                String1 = "String999",
                Bool1 = true,
                DateTime1 = dt
            };

            source.Add(newRecord);

            var target = this.GetData();
            var actual = new SimpleSqlBuilder<SqlTestRecord>(SqlTestRecord.TableName)
                .MergeInto(SqlTestRecord.TableName)
                .Match((s, t) => s.Int1 == t.Int1 && s.String1 == "String123")
                .WhenMatched(s => s.Update(s => s.Int2).Values(s => 5))
                .WhenNotMatched(
                    s => s.Insert(s => s.Int1, s => s.Int2, s => s.String1).Values(s => s.Int1 + 1, s => 20, s => "X"))
                .Build(source);

            var expected = @"MERGE INTO TableName AS Target USING TableName AS Source ON ((Source.Int1 = Target.Int1) AND (Source.String1 = 'String123')) WHEN MATCHED THEN UPDATE SET Target.Int2 = 5 WHEN NOT MATCHED THEN INSERT (Int1,Int2,String1) VALUES ((Source.Int1 + 1), 20, 'X')";

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

            newRecord = new SqlTestRecord
            {
                Int1 = 4,
                Int2 = 201,
                String1 = "String999",
                Bool1 = true,
                DateTime1 = dt
            };

            source.Add(newRecord);

            var target = this.GetData();
            var actual = new SimpleSqlBuilder<SqlTestRecord>(SqlTestRecord.TableName)
                .MergeInto(SqlTestRecord.TableName)
                .Match((s, t) => s.Int1 == t.Int1 && t.String1 == "String022")
                .WhenMatched(s => s.Update(s => s.Int2).Values(s => 5))
                .WhenNotMatched(
                    s => s.Insert(s => s.Int1, s => s.Int2, s => s.String1).Values(s => s.Int1 + 1, s => 20, s => "X"))
                .ExecuteMerge(source, target);

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
                .DistinctOn(s => s.Int2)
                .Build();

            var expected = 
                @"with ctr AS (SELECT Int2, row_number() over (partition by Int2 order by Int2) as Temp from TableName) DELETE FROM TableName WHERE Temp > 1";

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestDeleteDistinctWhere()
        {
            var actual = new SimpleSqlBuilder<SqlTestRecord>(SqlTestRecord.TableName)
                .Delete()
                .DistinctOn(s => s.Int2)
                .Where(s => s.Int1 == 5)
                .Build();

            var expected = 
                @"with ctr AS (SELECT Int2, row_number() over (partition by Int2 order by Int2) as Temp from TableName) DELETE FROM TableName WHERE (Int1 = 5) AND Temp > 1";

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
                .DistinctOn(s => s.Int2)
                .ExecuteDelete(data);

            Assert.AreEqual(2, actual);
            Assert.AreEqual(3, data.Count());
            Assert.AreEqual(101, data[2].Int2);
        }

        [TestMethod]
        public void TestExecuteDeleteDistinctWhere()
        {
            var data = this.GetData();
            var actual = new SimpleSqlBuilder<SqlTestRecord>(SqlTestRecord.TableName)
                .Delete()
                .DistinctOn(s => s.Int2)
                .Where(s => s.Int1 == 5)
                .ExecuteDelete(data);

            Assert.AreEqual(1, actual);
            Assert.AreEqual(4, data.Count());
            Assert.AreEqual(4, data[3].Int1);
        }

        [TestMethod]
        public void TestInnerJoinExecute1()
        {
            var data1 = this.GetData();
            var data2 = this.GetData();

            var actual = new SimpleSqlBuilder<SqlTestNewRecord, SqlTestRecord, SqlTestRecord>()
                .From(SqlTestRecord.TableName, "TableName2")
                .SelectAs(tResult => tResult.Int3, (t1, t2) => t1.Int1 + t2.Int2)
                .InnerJoinOn(
                    (TableName, TableName2) => TableName.String1 == TableName2.String1)
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
                .Where(tResult => tResult.Int1 > 3)
                .ExecuteSelect(data1, data2)
                .ToList();

            Assert.AreEqual(2, actual.Count());
            Assert.AreEqual(4, actual[0].Int1);
            Assert.AreEqual(5, actual[1].Int1);
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
                .Where(tResult => tResult.Int1 > 3)
                .ExecuteSelect(data1, data2)
                .ToList();

            Assert.AreEqual(2, actual.Count());
            Assert.AreEqual(4, actual[0].Int1);
            Assert.AreEqual(200, actual[0].Int2);
            Assert.AreEqual(5, actual[1].Int1);
            Assert.AreEqual(200, actual[1].Int2);
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
        public const string TableName = "TableName";

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
