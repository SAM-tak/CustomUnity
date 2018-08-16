using System.Collections;
using System.Linq;

namespace CustomUnity
{
    /// <summary>
    /// move all Enumerators that given as parameter
    /// </summary>
    /// <usage>
    /// var op1 = AssetBundleManager.LoadAssetAsync<GameObject>("a", "obj");
    /// var op2 = AssetBundleManager.LoadAssetAsync<GameObject>("b", "obj");
    /// yield return new WaitForEnumerators(op1, op2);
    /// </usage>
    public class WaitForEnumerators : IEnumerator
    {
        public WaitForEnumerators(params IEnumerator[] enumrators)
        {
            this.enumrators = enumrators;
        }

        public WaitForEnumerators(IEnumerable<IEnumerator> enumrators)
        {
            this.enumrators = enumrators;
        }
        
        readonly IEnumerable<IEnumerator> enumrators;

        public object Current => null;

        public bool MoveNext()
        {
            return !enumrators.All(x => !MoveNextRecursively(x));
        }

        public void Reset()
        {
            foreach(var i in enumrators) i.Reset();
        }

        bool MoveNextRecursively(IEnumerator enumrator)
        {
            return enumerator.Current is IEnumerator && MoveNextReccursively((IEnumerator)enumerator.Current) ? true : enumerator.MoveNext();
        }
    }
}