using UnityEngine;

namespace CustomUnity
{
    public static class Math
    {
        /// <summary>
        /// Hermite interpolation
        /// </summary>
        /// <param name="prev"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="next"></param>
        /// <param name="t"></param>
        /// <returns>interpolated position</returns>
        public static Vector2 Hermite(Vector2 prev, Vector2 start, Vector2 end, Vector2 next, float t)
        {
            float t2 = t * t;
            float t3 = t2 * t;
            float tension = 0.5f;   // 0.5 equivale a catmull-rom

            Vector2 control1 = tension * (end - prev);
            Vector2 control2 = tension * (next - start);

            float blend1 = 2 * t3 - 3 * t2 + 1;
            float blend2 = -2 * t3 + 3 * t2;
            float blend3 = t3 - 2 * t2 + t;
            float blend4 = t3 - t2;

            return blend1 * start + blend2 * end + blend3 * control1 + blend4 * control2;
        }

        /// <summary>
        /// Hermite interpolation
        /// </summary>
        /// <param name="prev"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="next"></param>
        /// <param name="t"></param>
        /// <returns>interpolated position</returns>
        public static Vector3 Hermite(Vector3 prev, Vector3 start, Vector3 end, Vector3 next, float t)
        {
            float t2 = t * t;
            float t3 = t2 * t;
            float tension = 0.5f;   // 0.5 equivale a catmull-rom

            Vector3 control1 = tension * (end - prev);
            Vector3 control2 = tension * (next - start);

            float blend1 = 2 * t3 - 3 * t2 + 1;
            float blend2 = -2 * t3 + 3 * t2;
            float blend3 = t3 - 2 * t2 + t;
            float blend4 = t3 - t2;

            return blend1 * start + blend2 * end + blend3 * control1 + blend4 * control2;
        }

        /// <summary>
        /// Hermite interpolation
        /// </summary>
        /// <param name="prev"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="next"></param>
        /// <param name="t"></param>
        /// <returns>interpolated position</returns>
        public static Vector4 Hermite(Vector4 prev, Vector4 start, Vector4 end, Vector4 next, float t)
        {
            float t2 = t * t;
            float t3 = t2 * t;
            float tension = 0.5f;   // 0.5 equivale a catmull-rom

            Vector4 control1 = tension * (end - prev);
            Vector4 control2 = tension * (next - start);

            float blend1 = 2 * t3 - 3 * t2 + 1;
            float blend2 = -2 * t3 + 3 * t2;
            float blend3 = t3 - 2 * t2 + t;
            float blend4 = t3 - t2;

            return blend1 * start + blend2 * end + blend3 * control1 + blend4 * control2;
        }

        /// <summary>
        /// Unity標準のeularは特異点を考慮していなくておかしいので自前で
        /// </summary>
        /// <param name="q">source</param>
        /// <returns>euler angles</returns>
        public static Vector3 ToEuler(Quaternion q)
        {
            // pitch (x-axis rotation)
            float sinr = 2.0f * (q.w * q.x + q.y * q.z);
            float cosr = 1.0f - 2.0f * (q.x * q.x + q.y * q.y);
            var pitch = Mathf.Atan2(sinr, cosr);

            // yaw (y-axis rotation)
            var yaw = 0.0f;
            float sinp = 2.0f * (q.w * q.y - q.z * q.x);
            if(Mathf.Abs(sinp) >= 1) yaw = sinp < 0 ? -Mathf.PI / 2 : Mathf.PI / 2; // use 90 degrees if out of range
            else yaw = Mathf.Asin(sinp);

            // roll (z-axis rotation)
            float siny = 2.0f * (q.w * q.z + q.x * q.y);
            float cosy = 1.0f - 2.0f * (q.y * q.y + q.z * q.z);
            var roll = Mathf.Atan2(siny, cosy);

            return new Vector3(pitch * Mathf.Rad2Deg, yaw * Mathf.Rad2Deg, roll * Mathf.Rad2Deg);
        }
        
        public static Vector2 SetX(this Vector2 self, float x)
        {
            return new Vector2(x, self.y);
        }

        public static Vector2 SetY(this Vector2 self, float y)
        {
            return new Vector2(self.x, y);
        }

        public static Vector3 SetX(this Vector3 self, float x)
        {
            return new Vector3(x, self.y, self.z);
        }

        public static Vector3 SetY(this Vector3 self, float y)
        {
            return new Vector3(self.x, y, self.z);
        }

        public static Vector3 SetZ(this Vector3 self, float z)
        {
            return new Vector3(self.x, self.y, z);
        }

        /// <summary>
        /// Make Hadamard product
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static Vector2 Times(Vector2 a, Vector2 b)
        {
            return new Vector2(a.x * b.x, a.y * b.y);
        }

        /// <summary>
        /// Make Hadamard product
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns>Hadamard product</returns>
        public static Vector3 Times(Vector3 a, Vector3 b)
        {
            return new Vector3(a.x * b.x, a.y * b.y, a.z * b.z);
        }

        /// <summary>
        /// Make Hadamard product
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns>Hadamard product</returns>
        public static Vector4 Times(Vector4 a, Vector4 b)
        {
            return new Vector4(a.x * b.x, a.y * b.y, a.z * b.z, a.w * b.w);
        }

        /// <summary>
        /// Make element-wise reciprocal
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static Vector2 Reciprocal(Vector2 a)
        {
            return new Vector2(1 / a.x, 1 / a.y);
        }

        /// <summary>
        /// Make element-wise reciprocal
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns>Hadamard product</returns>
        public static Vector3 Reciprocal(Vector3 a)
        {
            return new Vector3(1 / a.x, 1 / a.y, 1 / a.z);
        }

        /// <summary>
        /// Make element-wise reciprocal
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns>Hadamard product</returns>
        public static Vector4 Reciprocal(Vector4 a)
        {
            return new Vector4(1 / a.x, 1 / a.y, 1 / a.z, 1 / a.w);
        }

        /// <summary>
        /// Clamp to 0 - 1
        /// </summary>
        /// <param name="a">Targer</param>
        /// <returns>Clamped Value</returns>
        public static Vector2 Clamp01(Vector2 a)
        {
            return new Vector2(Mathf.Clamp01(a.x), Mathf.Clamp01(a.y));
        }

        /// <summary>
        /// Clamp to 0 - 1
        /// </summary>
        /// <param name="a">Targer</param>
        /// <returns>Clamped Value</returns>
        public static Vector3 Clamp01(Vector3 a)
        {
            return new Vector3(Mathf.Clamp01(a.x), Mathf.Clamp01(a.y), Mathf.Clamp01(a.z));
        }

        /// <summary>
        /// Clamp to 0 - 1
        /// </summary>
        /// <param name="a">Targer</param>
        /// <returns>Clamped Value</returns>
        public static Vector4 Clamp01(Vector4 a)
        {
            return new Vector4(Mathf.Clamp01(a.x), Mathf.Clamp01(a.y), Mathf.Clamp01(a.z), Mathf.Clamp01(a.w));
        }

        //
        // Starting from dx/dt = -axt.
        // We treat dx and dt as infinitessimal factors of x and t, therefore fundemental mathematical operations still apply.
        // Rearranging the equation to group x, dx and t, dt.
        //
        // 1/x dx = -a t dt.
        //
        // Note: We could rearrange with a on the left handside but since we want to find an equation for x(t) it is convienent to
        // seperate all constants.
        //
        // We now have the integral:
        //    Σ 1/x dx = -a Σ t dt.
        //
        //    ln x = -(1 / 2)a t^2 + C.
        // Where C is an integrating constant.
        // 
        //   x(t) = Aexp(-(1/2)a t^2).
        //   Define A = exp(C).
        //

        /// <summary>
        /// ゴムひも補間
        /// </summary>
        /// <param name="current">現在値</param>
        /// <param name="target">目標値</param>
        /// <param name="halfLife">半減期</param>
        /// <param name="deltaTime">経過時間</param>
        /// <returns>次の値</returns>
        public static float RubberStep(float current, float target, float halfLife, float deltaTime)
        {
            float delta = target - current;
            if(Mathf.Abs(delta) > float.Epsilon) {
                float rate = 1.0f / halfLife;
                float a = rate * deltaTime * deltaTime;
                float a2 = a * a;
                return current + delta * Mathf.Clamp01(rate * deltaTime * (1.0f + a + 0.5f * a2 + (1.0f / 6.0f) * a2 * a));
            }
            return target;
        }

        /// <summary>
        /// ゴムひも補間
        /// </summary>
        /// <param name="current">現在値</param>
        /// <param name="target">目標値</param>
        /// <param name="halfLife">半減期</param>
        /// <param name="deltaTime">経過時間</param>
        /// <returns>次の値</returns>
        public static Vector2 RubberStep(Vector2 current, Vector2 target, float halfLife, float deltaTime)
        {
            Vector2 delta = target - current;
            if(delta.sqrMagnitude > float.Epsilon) {
                float rate = 1.0f / halfLife;
                float a = 0.5f * rate * deltaTime * deltaTime;
                float a2 = a * a;
                return current + delta * Mathf.Clamp01(rate * deltaTime * (1.0f + a + 0.5f * a2 + (1.0f / 6.0f) * a2 * a));
            }
            return target;
        }

        /// <summary>
        /// ゴムひも補間
        /// </summary>
        /// <param name="current">現在値</param>
        /// <param name="target">目標値</param>
        /// <param name="halfLife">半減期</param>
        /// <param name="deltaTime">経過時間</param>
        /// <returns>次の値</returns>
        public static Vector3 RubberStep(Vector3 current, Vector3 target, float halfLife, float deltaTime)
        {
            Vector3 delta = target - current;
            if(delta.sqrMagnitude > float.Epsilon) {
                float rate = 1.0f / halfLife;
                float a = 0.5f * rate * deltaTime * deltaTime;
                float a2 = a * a;
                return current + delta * Mathf.Clamp01(rate * deltaTime * (1.0f + a + 0.5f * a2 + (1.0f / 6.0f) * a2 * a));
            }
            return target;
        }

        /// <summary>
        /// ゴムひも補間
        /// </summary>
        /// <param name="current">現在値</param>
        /// <param name="target">目標値</param>
        /// <param name="halfLife">半減期</param>
        /// <param name="deltaTime">経過時間</param>
        /// <returns>次の値</returns>
        public static Vector4 RubberStep(Vector4 current, Vector4 target, float halfLife, float deltaTime)
        {
            Vector4 delta = target - current;
            if(delta.sqrMagnitude > float.Epsilon) {
                float rate = 1.0f / halfLife;
                float a = 0.5f * rate * deltaTime * deltaTime;
                float a2 = a * a;
                return current + delta * Mathf.Clamp01(rate * deltaTime * (1.0f + a + 0.5f * a2 + (1.0f / 6.0f) * a2 * a));
            }
            return target;
        }

        /// <summary>
        /// ゴムひも補間（異方性半減期バージョン）
        /// </summary>
        /// <param name="current">現在値</param>
        /// <param name="target">目標値</param>
        /// <param name="halfLife">半減期</param>
        /// <param name="deltaTime">経過時間</param>
        /// <returns>次の値</returns>
        public static Vector2 RubberStep(Vector2 current, Vector2 target, Vector2 halfLife, float deltaTime)
        {
            Vector2 delta = target - current;
            if(delta.sqrMagnitude > float.Epsilon) {
                Vector2 rate = Reciprocal(halfLife);
                Vector2 a = 0.5f * rate * deltaTime * deltaTime;
                Vector2 a2 = Times(a, a);
                Vector2 k = Vector2.one + a + 0.5f * a2 + (1.0f / 6.0f) * Times(a2, a);
                return current + Times(delta, Clamp01(Times(rate * deltaTime, k)));
            }
            return target;
        }

        /// <summary>
        /// ゴムひも補間（異方性半減期バージョン）
        /// </summary>
        /// <param name="current">現在値</param>
        /// <param name="target">目標値</param>
        /// <param name="halfLife">半減期</param>
        /// <param name="deltaTime">経過時間</param>
        /// <returns>次の値</returns>
        public static Vector3 RubberStep(Vector3 current, Vector3 target, Vector3 halfLife, float deltaTime)
        {
            Vector3 delta = target - current;
            if(delta.sqrMagnitude > float.Epsilon) {
                Vector3 rate = Reciprocal(halfLife);
                Vector3 a = 0.5f * rate * deltaTime * deltaTime;
                Vector3 a2 = Times(a, a);
                Vector3 k = Vector3.one + a + 0.5f * a2 + (1.0f / 6.0f) * Times(a2, a);
                return current + Times(delta, Clamp01(Times(rate * deltaTime, k)));
            }
            return target;
        }

        /// <summary>
        /// ゴムひも補間（異方性半減期バージョン）
        /// </summary>
        /// <param name="current">現在値</param>
        /// <param name="target">目標値</param>
        /// <param name="halfLife">半減期</param>
        /// <param name="deltaTime">経過時間</param>
        /// <returns>次の値</returns>
        public static Vector4 RubberStep(Vector4 current, Vector4 target, Vector4 halfLife, float deltaTime)
        {
            Vector4 delta = target - current;
            if(delta.sqrMagnitude > float.Epsilon) {
                Vector4 rate = Reciprocal(halfLife);
                Vector4 a = 0.5f * rate * deltaTime * deltaTime;
                Vector4 a2 = Times(a, a);
                Vector4 k = Vector4.one + a + 0.5f * a2 + (1.0f / 6.0f) * Times(a2, a);
                return current + Times(delta, Clamp01(Times(rate * deltaTime, k)));
            }
            return target;
        }

        /// <summary>
        /// ゴムひも補間（異方性半減期バージョン）
        /// </summary>
        /// <param name="current">現在値</param>
        /// <param name="target">目標値</param>
        /// <param name="halfLife">半減期</param>
        /// <param name="deltaTime">経過時間</param>
        /// <returns>次の値</returns>
        public static Vector2 RubberStep(Vector2 current, Vector2 target, Vector2 increaseHalfLife, Vector2 decleaseHalfLife, float deltaTime)
        {
            Vector2 delta = target - current;
            if(delta.sqrMagnitude > float.Epsilon) {
                Vector2 halfLife = new Vector2(delta.x >= 0 ? increaseHalfLife.x : decleaseHalfLife.x, delta.y >= 0 ? increaseHalfLife.y : decleaseHalfLife.y);
                Vector2 rate = Reciprocal(halfLife);
                Vector2 a = 0.5f * rate * deltaTime * deltaTime;
                Vector2 a2 = Times(a, a);
                Vector2 k = Vector2.one + a + 0.5f * a2 + (1.0f / 6.0f) * Times(a2, a);
                return current + Times(delta, Clamp01(Times(rate * deltaTime, k)));
            }
            return target;
        }

        /// <summary>
        /// ゴムひも補間（異方性半減期バージョン）
        /// </summary>
        /// <param name="current">現在値</param>
        /// <param name="target">目標値</param>
        /// <param name="halfLife">半減期</param>
        /// <param name="deltaTime">経過時間</param>
        /// <returns>次の値</returns>
        public static Vector3 RubberStep(Vector3 current, Vector3 target, Vector3 increaseHalfLife, Vector3 decreaseHalfLife, float deltaTime)
        {
            Vector3 delta = target - current;
            if(delta.sqrMagnitude > float.Epsilon) {
                Vector3 halfLife = new Vector3(
                    delta.x >= 0 ? increaseHalfLife.x : decreaseHalfLife.x,
                    delta.y >= 0 ? increaseHalfLife.y : decreaseHalfLife.y,
                    delta.z >= 0 ? increaseHalfLife.z : decreaseHalfLife.z);
                Vector3 rate = Reciprocal(halfLife);
                Vector3 a = 0.5f * rate * deltaTime * deltaTime;
                Vector3 a2 = Times(a, a);
                Vector3 k = Vector3.one + a + 0.5f * a2 + (1.0f / 6.0f) * Times(a2, a);
                return current + Times(delta, Clamp01(Times(rate * deltaTime, k)));
            }
            return target;
        }

        /// <summary>
        /// ゴムひも補間（異方性半減期バージョン）
        /// </summary>
        /// <param name="current">現在値</param>
        /// <param name="target">目標値</param>
        /// <param name="halfLife">半減期</param>
        /// <param name="deltaTime">経過時間</param>
        /// <returns>次の値</returns>
        public static Vector4 RubberStep(Vector4 current, Vector4 target, Vector4 increaseHalfLife, Vector4 decreaseHalfLife, float deltaTime)
        {
            Vector4 delta = target - current;
            if(delta.sqrMagnitude > float.Epsilon) {
                Vector4 halfLife = new Vector4(
                    delta.x >= 0 ? increaseHalfLife.x : decreaseHalfLife.x,
                    delta.y >= 0 ? increaseHalfLife.y : decreaseHalfLife.y,
                    delta.z >= 0 ? increaseHalfLife.z : decreaseHalfLife.z,
                    delta.w >= 0 ? increaseHalfLife.w : decreaseHalfLife.w);
                Vector4 rate = Reciprocal(halfLife);
                Vector4 a = 0.5f * rate * deltaTime * deltaTime;
                Vector4 a2 = Times(a, a);
                Vector4 k = Vector4.one + a + 0.5f * a2 + (1.0f / 6.0f) * Times(a2, a);
                return current + Times(delta, Clamp01(Times(rate * deltaTime, k)));
            }
            return target;
        }
    }
}