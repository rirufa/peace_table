using NUnit.Framework;
using FooProject;

namespace TestProject1
{
    public class Tests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void ColletionTest()
        {
            PeaceTable table = new PeaceTable();

            table.Add("world");
            table.Add("!");
            Assert.AreEqual(table, "world!");
            table.Insert(0, "hello ");
            table.Insert(6, "dog ");
            table.Insert(10, "cat ");
            table.Insert(10, "rat ");
            table.Add("!!");
            Assert.AreEqual(table, "hello dog rat cat world!!!");
            table.Remove(7, 7);
            Assert.AreEqual(table, "hello dcat world!!!");

            table.Clear();
            Assert.AreEqual(table, "");

            table.Add("hello");
            table.Add("!");
            table.Insert(5, " world");
            Assert.AreEqual(table, "hello world!");
            table.Insert(11, " C#");
            Assert.AreEqual(table, "hello world C#!");
            table.Add("!!");
            table.Insert(14, " and C++/CLI");
            Assert.AreEqual(table, "hello world C# and C++/CLI!!!");
            table.Remove(5, 19);
            Assert.AreEqual(table, "hello!!!");
        }

        [Test]
        public void IndexerTest()
        {
            PeaceTable table = new PeaceTable();
            table.Add("world");
            table.Add("!");
            Assert.AreEqual(table[1],'o');
            table.Insert(0, "hello ");
            Assert.AreEqual(table[7], 'o');
            Assert.AreEqual(table[0], 'h');
            Assert.AreEqual(table[6], 'w');
        }

        [Test]
        public void ToStringTest()
        {
            PeaceTable table = new PeaceTable();
            table.Add("world");
            table.Add("!");
            Assert.AreEqual(table.ToString(0,5) , "world");
            table.Insert(0, "hello ");
            Assert.AreEqual(table.ToString(0,11), "hello world");
        }
    }
}