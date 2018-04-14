using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;

namespace CustomUnity
{
    public class MathTest
    {
        [Test]
        public void MathTestSimplePasses()
        {
            // Use the Assert class to test conditions.
            Assert.AreEqual(Math.RubberStep(0, 1, 0, 1), 1);
        }

        // A UnityTest behaves like a coroutine in PlayMode
        // and allows you to yield null to skip a frame in EditMode
        [UnityTest]
        public IEnumerator MathTestWithEnumeratorPasses()
        {
            // Use the Assert class to test conditions.
            // yield to skip a frame
            yield return null;
        }
    }
}
