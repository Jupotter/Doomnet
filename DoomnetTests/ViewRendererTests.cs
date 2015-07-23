using NUnit.Framework;
using Doomnet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Doomnet.Tests
{
    [TestFixture()]
    public class ViewRendererTests
    {
        [Test()]
        public void GetSideLineTest()
        {
            var rend = new ViewRenderer(2, 2);

            Assert.That(rend.GetSideLine(2, 2, 0, 4, 4), Is.EqualTo(new Tuple<int, int>(4, 2)));
            Assert.That(rend.GetSideLine(2, 2, 90, 4, 4), Is.EqualTo(new Tuple<int, int>(2, 4)));
            Assert.That(rend.GetSideLine(2, 2, 180, 4, 4), Is.EqualTo(new Tuple<int, int>(0, 2)));
            Assert.That(rend.GetSideLine(2, 2, 270, 4, 4), Is.EqualTo(new Tuple<int, int>(2, 0)));
        }

        [Test()]
        public void GetSideLineTest2()
        {
            var rend = new ViewRenderer(2, 2);

            Assert.That(rend.GetSideLine(2, 2, 30, 4, 4), Is.EqualTo(new Tuple<int, int>(4, 3)));
            Assert.That(rend.GetSideLine(2, 2, 120, 4, 4), Is.EqualTo(new Tuple<int, int>(1, 4)));
            Assert.That(rend.GetSideLine(2, 2, 210, 4, 4), Is.EqualTo(new Tuple<int, int>(0, 1)));
            Assert.That(rend.GetSideLine(2, 2, 300, 4, 4), Is.EqualTo(new Tuple<int, int>(3, 0)));
        }

        [Test()]
        public void GetSideLineTest3()
        {
            var rend = new ViewRenderer(2, 2);

            Assert.That(rend.GetSideLine(2, 2, 60, 4, 4), Is.EqualTo(new Tuple<int, int>(4, 5)));
            Assert.That(rend.GetSideLine(2, 2, 150, 4, 4), Is.EqualTo(new Tuple<int, int>(-1, 4)));
            Assert.That(rend.GetSideLine(2, 2, 240, 4, 4), Is.EqualTo(new Tuple<int, int>(0, -1)));
            Assert.That(rend.GetSideLine(2, 2, 330, 4, 4), Is.EqualTo(new Tuple<int, int>(5, 0)));
        }

        [Test()]
        public void GetSideLineTest4()
        {
            var rend = new ViewRenderer(2, 2);

            Assert.That(rend.GetSideLine(2, 3, 45, 7, 5), Is.EqualTo(new Tuple<int, int>(7, 8)));
            Assert.That(rend.GetSideLine(2, 3, 135, 7, 5), Is.EqualTo(new Tuple<int, int>(0, 5)));
            Assert.That(rend.GetSideLine(2, 3, 225, 7, 5), Is.EqualTo(new Tuple<int, int>(-1, 0)));
            Assert.That(rend.GetSideLine(2, 3, 315, 7, 5), Is.EqualTo(new Tuple<int, int>(5, 0)));
        }
    }
}