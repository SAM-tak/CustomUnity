using UnityEngine;

namespace CustomUnity
{
    /// <summary>
    /// Math utilities
    /// </summary>
    public static class Math
    {
        /// <summary>
        /// Wrap value for 0 to max.
        /// </summary>
        /// <param name="value">Value</param>
        /// <param name="max">Positive Maximum Value</param>
        /// <returns>Wraped Value</returns>
        public static int Wrap(int value, int max)
        {
            Debug.Assert(max > 0);
            var m = value % max;
            return m < 0 ? m + max : m;
        }

        /// <summary>
        /// Wrap value for 0 to max.
        /// </summary>
        /// <param name="value">Value</param>
        /// <param name="max">Positive Maximum Value</param>
        /// <returns>Wraped Value</returns>
        public static float Wrap(float value, float max)
        {
            Debug.Assert(max > 0);
            var m = value % max;
            return m < 0 ? m + max : m;
        }

        /// <summary>
        /// Ceiling value / alignment.
        /// </summary>
        /// <param name="value">Value</param>
        /// <param name="alignment">Positive Alignment Value</param>
        /// <returns>Wraped Value</returns>
        public static int CeilDiv(int value, int alignment)
        {
            Debug.Assert(alignment > 0);
            return value < 0 ? (value - alignment + 1) / alignment + 1 : (value + alignment - 1) / alignment;
        }

        /// <summary>
        /// Ceiling value / alignment.
        /// </summary>
        /// <param name="value">Value</param>
        /// <param name="alignment">Positive Alignment Value</param>
        /// <returns>Wraped Value</returns>
        public static long CeilDiv(long value, long alignment)
        {
            Debug.Assert(alignment > 0);
            return value < 0 ? (value - alignment + 1) / alignment + 1 : (value + alignment - 1) / alignment;
        }

        /// <summary>
        /// Ceiling value / alignment.
        /// </summary>
        /// <param name="value">Value</param>
        /// <param name="alignment">Alignment Value</param>
        /// <returns>Wraped Value</returns>
        public static uint CeilDiv(uint value, uint alignment)
        {
            return (value + alignment - 1) / alignment;
        }

        /// <summary>
        /// Ceiling value / alignment.
        /// </summary>
        /// <param name="value">Value</param>
        /// <param name="alignment">Alignment Value</param>
        /// <returns>Wraped Value</returns>
        public static ulong CeilDiv(ulong value, ulong alignment)
        {
            return (value + alignment - 1) / alignment;
        }

        /// <summary>
        /// Flooring value / alignment.
        /// </summary>
        /// <param name="value">Value</param>
        /// <param name="alignment">Positive Alignment Value</param>
        /// <returns>Wraped Value</returns>
        public static int FloorDiv(int value, int alignment)
        {
            Debug.Assert(alignment > 0);
            return value < 0 ? (value + 1) / alignment - 1 : value / alignment;
        }

        /// <summary>
        /// Flooring value / alignment.
        /// </summary>
        /// <param name="value">Value</param>
        /// <param name="alignment">Positive Alignment Value</param>
        /// <returns>Wraped Value</returns>
        public static long FloorDiv(long value, long alignment)
        {
            Debug.Assert(alignment > 0);
            return value < 0 ? (value + 1) / alignment - 1 : value / alignment;
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
            float sinp = 2.0f * (q.w * q.y - q.z * q.x);
            float yaw = Mathf.Abs(sinp) >= 1 ? (sinp < 0 ? -Mathf.PI / 2 : Mathf.PI / 2) /* use 90 degrees if out of range */ : Mathf.Asin(sinp);

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

        public static Vector4 SetX(this Vector4 self, float x)
        {
            return new Vector4(x, self.y, self.z, self.w);
        }

        public static Vector4 SetY(this Vector4 self, float y)
        {
            return new Vector4(self.x, y, self.z, self.w);
        }

        public static Vector4 SetZ(this Vector4 self, float z)
        {
            return new Vector4(self.x, self.y, z, self.w);
        }

        public static Vector4 SetW(this Vector4 self, float w)
        {
            return new Vector4(self.x, self.y, self.z, w);
        }

        public static Vector4 SetZW(this Vector4 self, float z, float w)
        {
            return new Vector4(self.x, self.y, z, w);
        }

        public static Vector4 SetZW(this Vector4 self, Vector2 zw)
        {
            return new Vector4(self.x, self.y, zw.x, zw.y);
        }

        public static Vector2 ToVector2(this Vector3 self)
        {
            return new Vector2(self.x, self.y);
        }

        public static Vector3 ToVector3(this Vector2 self, float z)
        {
            return ((Vector3)self).SetZ(z);
        }

        public static Vector4 ToVector4(this Vector2 self, float z, float w)
        {
            return ((Vector4)self).SetZW(z, w);
        }

        public static Vector4 ToVector4(this Vector2 self, Vector2 zw)
        {
            return ((Vector4)self).SetZW(zw);
        }

        public static Vector4 ToVector4(this Vector3 self, float w)
        {
            return ((Vector4)self).SetW(w);
        }

        public static Vector2 EpsilonVector2 { get; } = new Vector2(float.Epsilon, float.Epsilon);

        public static Vector3 EpsilonVector3 { get; } = new Vector3(float.Epsilon, float.Epsilon, float.Epsilon);

        public static Vector4 EpsilonVector4 { get; } = new Vector4(float.Epsilon, float.Epsilon, float.Epsilon, float.Epsilon);

        /// <summary>
        /// Make Hadamard product
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns>Hadamard product</returns>
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
        /// <returns></returns>
        public static Vector2 Reciprocal(Vector2 a)
        {
            return new Vector2(1 / a.x, 1 / a.y);
        }

        /// <summary>
        /// Make element-wise reciprocal
        /// </summary>
        /// <param name="a"></param>
        /// <returns>Hadamard product</returns>
        public static Vector3 Reciprocal(Vector3 a)
        {
            return new Vector3(1 / a.x, 1 / a.y, 1 / a.z);
        }

        /// <summary>
        /// Make element-wise reciprocal
        /// </summary>
        /// <param name="a"></param>
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

        /// <summary>
        /// Return a vactor has absolete element values of original.
        /// </summary>
        /// <param name="a">Origin</param>
        /// <returns>Absolete Vector</returns>
        public static Vector2 Abs(Vector2 a)
        {
            return new Vector2(Mathf.Abs(a.x), Mathf.Abs(a.y));
        }

        /// <summary>
        /// Return a vactor has absolete element values of original.
        /// </summary>
        /// <param name="a">Origin</param>
        /// <returns>Absolete Vector</returns>
        public static Vector3 Abs(Vector3 a)
        {
            return new Vector3(Mathf.Abs(a.x), Mathf.Abs(a.y), Mathf.Abs(a.z));
        }

        /// <summary>
        /// Return a vactor has absolete element values of original.
        /// </summary>
        /// <param name="a">Origin</param>
        /// <returns>Absolete Vector</returns>
        public static Vector4 Abs(Vector4 a)
        {
            return new Vector4(Mathf.Abs(a.x), Mathf.Abs(a.y), Mathf.Abs(a.z), Mathf.Abs(a.w));
        }

        //
        // Starting from dx/dt = wxt.
        // We treat dx and dt as infinitessimal factors of x and t, therefore fundemental mathematical operations still apply.
        // Rearranging the equation to group x, dx and t, dt.
        //
        // 1/x dx = w t dt.
        //
        // Note: We could rearrange with a on the left handside but since we want to find an equation for x(t) it is convienent to
        // seperate all constants.
        //
        // We now have the integral:
        //    Σ 1/x dx = w Σ t dt.
        //
        //    ln x = (1 / 2)w t^2 + C.
        // Where C is an integrating constant.
        // 
        //   x(t) = Aexp((1/2)w t^2).
        //   Define A = exp(C).
        //
        // Then w Σ(0->t)xt = w x t exp((1/2)w t^2)
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
                float omega = 1.0f / Mathf.Max(float.Epsilon, halfLife * halfLife);
                float a = 0.5f * omega * deltaTime * deltaTime;
                float a2 = a * a;
                return current + delta * Mathf.Clamp01(omega * deltaTime * (1.0f + a + 0.48f * a2 + 0.235f * a2 * a));
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
                float omega = 1.0f / Mathf.Max(float.Epsilon, halfLife * halfLife);
                float a = 0.5f * omega * deltaTime * deltaTime;
                float a2 = a * a;
                return current + delta * Mathf.Clamp01(omega * deltaTime * (1.0f + a + 0.48f * a2 + 0.235f * a2 * a));
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
                float omega = 1.0f / Mathf.Max(float.Epsilon, halfLife * halfLife);
                float a = 0.5f * omega * deltaTime * deltaTime;
                float a2 = a * a;
                return current + delta * Mathf.Clamp01(omega * deltaTime * (1.0f + a + 0.48f * a2 + 0.235f * a2 * a));
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
                float omega = 1.0f / Mathf.Max(float.Epsilon, halfLife * halfLife);
                float a = 0.5f * omega * deltaTime * deltaTime;
                float a2 = a * a;
                return current + delta * Mathf.Clamp01(omega * deltaTime * (1.0f + a + 0.48f * a2 + 0.235f * a2 * a));
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
        public static Quaternion RubberStep(Quaternion current, Quaternion target, float halfLife, float deltaTime)
        {
            Quaternion delta = target * Quaternion.Inverse(current);
            if(Quaternion.Angle(delta, Quaternion.identity) > float.Epsilon) {
                float omega = 1.0f / Mathf.Max(float.Epsilon, halfLife * halfLife);
                float a = 0.5f * omega * deltaTime * deltaTime;
                float a2 = a * a;
                return current * Quaternion.Slerp(Quaternion.identity, delta, Mathf.Clamp01(omega * deltaTime * (1.0f + a + 0.48f * a2 + 0.235f * a2 * a)));
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
                Vector2 omega = Reciprocal(Vector4.Max(EpsilonVector2, Times(halfLife, halfLife)));
                Vector2 a = 0.5f * deltaTime * deltaTime * omega;
                Vector2 a2 = a * a;
                return current + delta * Clamp01(omega * deltaTime * (Vector2.one + a + 0.48f * a2 + 0.235f * a2 * a));
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
                Vector3 omega = Reciprocal(Vector4.Max(EpsilonVector3, Times(halfLife, halfLife)));
                Vector3 a = 0.5f * deltaTime * deltaTime * omega;
                Vector3 a2 = Times(a, a);
                Vector3 k = Vector3.one + a + 0.48f * a2 + 0.235f * Times(a2, a);
                return current + Times(delta, Clamp01(Times(omega * deltaTime, k)));
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
                Vector4 omega = Reciprocal(Vector4.Max(EpsilonVector4, Times(halfLife, halfLife)));
                Vector4 a = 0.5f * deltaTime * deltaTime * omega;
                Vector4 a2 = Times(a, a);
                Vector4 k = Vector4.one + a + 0.48f * a2 + 0.235f * Times(a2, a);
                return current + Times(delta, Clamp01(Times(omega * deltaTime, k)));
            }
            return target;
        }

        /// <summary>
        /// ゴムひも補間（異方性半減期バージョン）
        /// </summary>
        /// <param name="current">現在値</param>
        /// <param name="target">目標値</param>
        /// <param name="increaseHalfLife">増加方向半減期</param>
        /// <param name="decreaseHalfLife">減少方向半減期</param>
        /// <param name="deltaTime">経過時間</param>
        /// <returns>次の値</returns>
        public static Vector2 RubberStep(Vector2 current, Vector2 target, Vector2 increaseHalfLife, Vector2 decreaseHalfLife, float deltaTime)
        {
            Vector2 delta = target - current;
            if(delta.sqrMagnitude > float.Epsilon) {
                Vector2 halfLife = new Vector2(delta.x >= 0 ? increaseHalfLife.x : decreaseHalfLife.x, delta.y >= 0 ? increaseHalfLife.y : decreaseHalfLife.y);
                Vector2 omega = Reciprocal(Vector4.Max(EpsilonVector2, halfLife));
                Vector2 a = 0.5f * deltaTime * deltaTime * omega;
                Vector2 a2 = a * a;
                return current + delta * Clamp01(omega * deltaTime * (Vector2.one + a + 0.48f * a2 + 0.235f * a2 * a));
            }
            return target;
        }

        /// <summary>
        /// ゴムひも補間（異方性半減期バージョン）
        /// </summary>
        /// <param name="current">現在値</param>
        /// <param name="target">目標値</param>
        /// <param name="increaseHalfLife">増加方向半減期</param>
        /// <param name="decreaseHalfLife">減少方向半減期</param>
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
                Vector3 omega = Reciprocal(Vector4.Max(EpsilonVector3, halfLife));
                Vector3 a = 0.5f * deltaTime * deltaTime * omega;
                Vector3 a2 = Times(a, a);
                Vector3 k = Vector3.one + a + 0.48f * a2 + 0.235f * Times(a2, a);
                return current + Times(delta, Clamp01(Times(omega * deltaTime, k)));
            }
            return target;
        }

        /// <summary>
        /// ゴムひも補間（異方性半減期バージョン）
        /// </summary>
        /// <param name="current">現在値</param>
        /// <param name="target">目標値</param>
        /// <param name="increaseHalfLife">増加方向半減期</param>
        /// <param name="decreaseHalfLife">減少方向半減期</param>
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
                Vector4 omega = Reciprocal(Vector4.Max(EpsilonVector4, halfLife));
                Vector4 a = 0.5f * deltaTime * deltaTime * omega;
                Vector4 a2 = Times(a, a);
                Vector4 k = Vector4.one + a + 0.48f * a2 + 0.235f * Times(a2, a);
                return current + Times(delta, Clamp01(Times(omega * deltaTime, k)));
            }
            return target;
        }

        const float DampCoeff = 80.0f;

        public static Vector3 SmoothDamp(ref Vector3 currentVelocity, Vector3 current, Vector3 target, float halfLife, float deltaTime)
        {
            var newPosition = RubberStep(current, target, halfLife, deltaTime);

            var newVelocity = newPosition - current;
            var accel = (newVelocity - currentVelocity);
            if(currentVelocity.sqrMagnitude > 0.0001f && accel.sqrMagnitude > 0.0001f) {
                accel *= Mathf.Lerp(0.1f, 1.0f, (0.5f - 0.5f * Vector3.Dot(currentVelocity.normalized, accel.normalized)) * Mathf.Clamp01(accel.sqrMagnitude / DampCoeff));
                newVelocity = currentVelocity + accel;
                newPosition = current + newVelocity * deltaTime;
            }
            else currentVelocity = Vector3.zero;

            return newPosition;
        }

        public static Vector3 SmoothDamp(ref Vector3 currentVelocity, Vector3 current, Vector3 target, Vector3 halfLife, float deltaTime)
        {
            var newPosition = RubberStep(current, target, halfLife, deltaTime);

            var newVelocity = newPosition - current;
            var accel = (newVelocity - currentVelocity);
            if(currentVelocity.sqrMagnitude > 0.0001f && accel.sqrMagnitude > 0.0001f) {
                accel *= Mathf.Lerp(0.1f, 1.0f, (0.5f - 0.5f * Vector3.Dot(currentVelocity.normalized, accel.normalized)) * Mathf.Clamp01(accel.sqrMagnitude / DampCoeff));
                newVelocity = currentVelocity + accel;
                newPosition = current + newVelocity * deltaTime;
            }
            else currentVelocity = Vector3.zero;

            return newPosition;
        }

        public static Vector3 SmoothDamp(ref Vector3 currentVelocity, Vector3 current, Vector3 target, Vector3 increaseHalfLife, Vector3 decreaseHalfLife, float deltaTime)
        {
            var newPosition = RubberStep(current, target, increaseHalfLife, decreaseHalfLife, deltaTime);

            var newVelocity = newPosition - current;
            var accel = (newVelocity - currentVelocity);
            if(currentVelocity.sqrMagnitude > 0.0001f && accel.sqrMagnitude > 0.0001f) {
                accel *= Mathf.Lerp(0.1f, 1.0f, (0.5f - 0.5f * Vector3.Dot(currentVelocity.normalized, accel.normalized)) * Mathf.Clamp01(accel.sqrMagnitude / DampCoeff));
                newVelocity = currentVelocity + accel;
                newPosition = current + newVelocity * deltaTime;
            }
            else currentVelocity = Vector3.zero;

            return newPosition;
        }

        public static bool IntersectsLineSegment(Vector2 p1s, Vector2 p1e, Vector2 p2s, Vector2 p2e, out Vector2 intersection)
        {
            var d1 = p1e - p1s;
            if(d1.sqrMagnitude > float.Epsilon) { // first line segment is not dot
                var d2 = p2e - p2s;
                if(d2.sqrMagnitude > float.Epsilon) { // second line segment is not dot
                    var d = d1.x * d2.y - d1.y * d2.x; // cross product of 2d vector
                    if(Mathf.Abs(d) > float.Epsilon) { // not parallele
                        var v = p2s - p1s;
                        var s = (v.x * d1.y - v.y * d1.x) / d;
                        if(0 <= s && s <= 1) { // cross point is in second line segment
                            var t = (v.x * d2.y - v.y * d2.x) / d;
                            if(0 <= t && t <= 1) { // cross point is in first line segment
                                intersection = p1s + d1 * t;
                                return true;
                            }
                        }
                    }
                }
            }

            intersection.x = intersection.y = float.NaN;
            return false;
        }

        static bool ContainsInOBB(this Vector2 self, Vector2 center, Vector2 size, Vector2 right, Vector2 up)
        {
            var v = self - center;
            if(Mathf.Abs(Vector2.Dot(v, right)) >= size.x) return false;
            if(Mathf.Abs(Vector2.Dot(v, up)) >= size.y) return false;
            return true;
        }

        static void UpdateMin(this Vector2 self, ref Vector2 min)
        {
            if(min.x > self.x) min.x = self.x;
            if(min.y > self.y) min.y = self.y;
        }

        static void UpdateMax(this Vector2 self, ref Vector2 max)
        {
            if(max.x < self.x) max.x = self.x;
            if(max.y < self.y) max.y = self.y;
        }

        static void UpdateMinMax(this Vector2 self, ref Vector2 min, ref Vector2 max)
        {
            self.UpdateMin(ref min);
            self.UpdateMax(ref max);
        }

        public static bool OverlapsOBB(this BoxCollider2D a, BoxCollider2D b, out Vector2 min, out Vector2 max)
        {
            min = Vector2.positiveInfinity;
            max = Vector2.negativeInfinity;

            var ap = a.transform.TransformPoint(a.offset).ToVector2();
            var adx = a.transform.TransformDirection(Vector2.right).ToVector2();
            var ady = a.transform.TransformDirection(Vector2.up).ToVector2();
            var bp = b.transform.TransformPoint(b.offset).ToVector2();
            var bdx = b.transform.TransformDirection(Vector2.right).ToVector2();
            var bdy = b.transform.TransformDirection(Vector2.up).ToVector2();
            var al = a.size * 0.5f;
            var aex = adx * al.x;
            var aey = ady * al.y;
            var bl = b.size * 0.5f;
            var bex = bdx * bl.x;
            var bey = bdy * bl.y;

            if(Mathf.Abs(Vector2.Dot(bp - ap, adx)) >= al.x + Mathf.Abs(Vector2.Dot(bex, adx)) + Mathf.Abs(Vector2.Dot(bey, adx))) return false;
            if(Mathf.Abs(Vector2.Dot(bp - ap, ady)) >= al.y + Mathf.Abs(Vector2.Dot(bex, ady)) + Mathf.Abs(Vector2.Dot(bey, ady))) return false;
            if(Mathf.Abs(Vector2.Dot(ap - bp, bdx)) >= Mathf.Abs(Vector2.Dot(aex, bdx)) + Mathf.Abs(Vector2.Dot(aey, bdx)) + bl.x) return false;
            if(Mathf.Abs(Vector2.Dot(ap - bp, bdy)) >= Mathf.Abs(Vector2.Dot(aex, bdy)) + Mathf.Abs(Vector2.Dot(aey, bdy)) + bl.y) return false;

            var ap1 = ap - aex + aey;
            var ap2 = ap + aex + aey;
            var ap3 = ap + aex - aey;
            var ap4 = ap - aex - aey;
            var bp1 = bp - bex + bey;
            var bp2 = bp + bex + bey;
            var bp3 = bp + bex - bey;
            var bp4 = bp - bex - bey;

            if(IntersectsLineSegment(ap1, ap2, bp1, bp2, out Vector2 intersection)) intersection.UpdateMinMax(ref min, ref max);
            if(IntersectsLineSegment(ap1, ap2, bp2, bp3, out intersection)) intersection.UpdateMinMax(ref min, ref max);
            if(IntersectsLineSegment(ap1, ap2, bp3, bp4, out intersection)) intersection.UpdateMinMax(ref min, ref max);
            if(IntersectsLineSegment(ap1, ap2, bp4, bp1, out intersection)) intersection.UpdateMinMax(ref min, ref max);
            if(IntersectsLineSegment(ap2, ap3, bp1, bp2, out intersection)) intersection.UpdateMinMax(ref min, ref max);
            if(IntersectsLineSegment(ap2, ap3, bp2, bp3, out intersection)) intersection.UpdateMinMax(ref min, ref max);
            if(IntersectsLineSegment(ap2, ap3, bp3, bp4, out intersection)) intersection.UpdateMinMax(ref min, ref max);
            if(IntersectsLineSegment(ap2, ap3, bp4, bp1, out intersection)) intersection.UpdateMinMax(ref min, ref max);
            if(IntersectsLineSegment(ap3, ap4, bp1, bp2, out intersection)) intersection.UpdateMinMax(ref min, ref max);
            if(IntersectsLineSegment(ap3, ap4, bp2, bp3, out intersection)) intersection.UpdateMinMax(ref min, ref max);
            if(IntersectsLineSegment(ap3, ap4, bp3, bp4, out intersection)) intersection.UpdateMinMax(ref min, ref max);
            if(IntersectsLineSegment(ap3, ap4, bp4, bp1, out intersection)) intersection.UpdateMinMax(ref min, ref max);
            if(IntersectsLineSegment(ap4, ap1, bp1, bp2, out intersection)) intersection.UpdateMinMax(ref min, ref max);
            if(IntersectsLineSegment(ap4, ap1, bp2, bp3, out intersection)) intersection.UpdateMinMax(ref min, ref max);
            if(IntersectsLineSegment(ap4, ap1, bp3, bp4, out intersection)) intersection.UpdateMinMax(ref min, ref max);
            if(IntersectsLineSegment(ap4, ap1, bp4, bp1, out intersection)) intersection.UpdateMinMax(ref min, ref max);

            if(ap1.ContainsInOBB(bp, bl, bdx, bdy)) ap1.UpdateMinMax(ref min, ref max);
            if(ap2.ContainsInOBB(bp, bl, bdx, bdy)) ap2.UpdateMinMax(ref min, ref max);
            if(ap3.ContainsInOBB(bp, bl, bdx, bdy)) ap3.UpdateMinMax(ref min, ref max);
            if(ap4.ContainsInOBB(bp, bl, bdx, bdy)) ap4.UpdateMinMax(ref min, ref max);
            if(bp1.ContainsInOBB(ap, al, adx, ady)) bp1.UpdateMinMax(ref min, ref max);
            if(bp2.ContainsInOBB(ap, al, adx, ady)) bp2.UpdateMinMax(ref min, ref max);
            if(bp3.ContainsInOBB(ap, al, adx, ady)) bp3.UpdateMinMax(ref min, ref max);
            if(bp4.ContainsInOBB(ap, al, adx, ady)) bp4.UpdateMinMax(ref min, ref max);

            return true;
        }

        public static bool OverlapsOBB(this BoxCollider2D a, BoxCollider2D b)
        {
            var ap = a.transform.TransformPoint(a.offset).ToVector2();
            var adx = a.transform.TransformDirection(Vector2.right).ToVector2();
            var ady = a.transform.TransformDirection(Vector2.up).ToVector2();
            var bp = b.transform.TransformPoint(b.offset).ToVector2();
            var bdx = b.transform.TransformDirection(Vector2.right).ToVector2();
            var bdy = b.transform.TransformDirection(Vector2.up).ToVector2();
            var al = a.size * 0.5f;
            var aex = adx * al.x;
            var aey = ady * al.y;
            var bl = b.size * 0.5f;
            var bex = bdx * bl.x;
            var bey = bdy * bl.y;

            if(Mathf.Abs(Vector2.Dot(bp - ap, adx)) >= al.x + Mathf.Abs(Vector2.Dot(bex, adx)) + Mathf.Abs(Vector2.Dot(bey, adx))) return false;
            if(Mathf.Abs(Vector2.Dot(bp - ap, ady)) >= al.y + Mathf.Abs(Vector2.Dot(bex, ady)) + Mathf.Abs(Vector2.Dot(bey, ady))) return false;
            if(Mathf.Abs(Vector2.Dot(ap - bp, bdx)) >= Mathf.Abs(Vector2.Dot(aex, bdx)) + Mathf.Abs(Vector2.Dot(aey, bdx)) + bl.x) return false;
            if(Mathf.Abs(Vector2.Dot(ap - bp, bdy)) >= Mathf.Abs(Vector2.Dot(aex, bdy)) + Mathf.Abs(Vector2.Dot(aey, bdy)) + bl.y) return false;

            return true;
        }
    }
}