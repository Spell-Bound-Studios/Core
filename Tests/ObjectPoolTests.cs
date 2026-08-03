// Copyright 2026 Spellbound Studio Inc.

using NUnit.Framework;
using Spellbound.Core.ObjectPooling;

namespace Spellbound.Core.Tests {
    public class ObjectPoolTests {
        private class Item {
            public int ResetCount;
        }

        private class ItemPool : ObjectPool<Item> {
            public int Created;

            protected override Item Create() {
                Created++;

                return new Item();
            }

            protected override void Reset(Item item) => item.ResetCount++;
        }

        [Test]
        public void RentCreatesWhenEmptyAndReusesAfterReturn() {
            var pool = new ItemPool();

            var first = pool.Rent();

            Assert.AreEqual(1, pool.Created);
            Assert.AreEqual(0, pool.Available);

            pool.Return(first);

            Assert.AreEqual(1, pool.Available);
            Assert.AreEqual(1, first.ResetCount);

            var second = pool.Rent();

            Assert.AreSame(first, second);
            Assert.AreEqual(1, pool.Created);
            Assert.AreEqual(0, pool.Available);
        }
    }
}
