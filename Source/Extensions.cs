using UnityEngine;

namespace KKS
{
    public static class Extensions
    {
        public static Vector3 Multiply(this Vector3 @this, Vector3 other)
        {
            return new Vector3(@this.x * other.x, @this.y * other.y, @this.z * other.z);
        }

        public static Vector3 Divide(this Vector3 @this, Vector3 other)
        {
            return new Vector3(@this.x / other.x, @this.y / other.y, @this.z / other.z);
        }
    }
}
