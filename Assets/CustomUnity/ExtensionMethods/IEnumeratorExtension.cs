using System.Collections;

namespace CustomUnity
{
    public static class IEnumeratorExtension
    {
        /// <summary>
        /// MoveNext like Unity's coroutine
        /// </summary>
        /// <param name="enumerator"></param>
        /// <returns></returns>
        public static bool MoveNextRecursively(this IEnumerator enumerator)
        {
            return enumerator.Current is IEnumerator && MoveNextRecursively(enumerator.Current as IEnumerator) || enumerator.MoveNext();
        }
    }
}