using UnityEngine;
using CustomUnity;

namespace YourProjectNamespace
{
    public class Collider2DTest : MonoBehaviour
    {
        void OnCollisionEnter2D(Collision2D collision)
        {
            LogInfo($"{gameObject.name} : OnCollisionEnter2D {collision.gameObject.name}".Color(Color.red));
        }

        void OnCollisionExit2D(Collision2D collision)
        {
            LogInfo($"{gameObject.name} : OnCollisionExit2D {collision.gameObject.name}".Color(Color.blue));
        }

        void OnTriggerEnter2D(Collider2D collision)
        {
            LogInfo($"{gameObject.name} : OnTriggerEnter2D {collision.gameObject.name}".Color(Color.magenta));
        }

        void OnTriggerExit2D(Collider2D collision)
        {
            LogInfo($"{gameObject.name} : OnTriggerExit2D {collision.gameObject.name}".Color(Color.cyan));
        }
    }
}
