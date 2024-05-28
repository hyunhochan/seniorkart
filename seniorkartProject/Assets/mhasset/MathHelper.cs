using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets
{

    public class MathHelper
    {
        public static int[] next = new int[ 3 ] { 1, 2, 0 };

        public static Quaternion CreateQuaternion( float w, Vector3 v ) => new Quaternion( v.x, v.y, v.z, w );

        public static void QuaMulScala( ref Quaternion q, float v )
        {
            q.x *= v;
            q.y *= v;
            q.z *= v;
            q.w *= v;
        }

        public static void QuaNormalize( ref Quaternion q ) => MathHelper.QuaMulScala( ref q, 1f / Mathf.Sqrt( (float)( q.w * q.w + q.x * q.x + q.y * q.y + q.z * q.z ) ) );

        public static void QuaAdd( ref Quaternion q1, Quaternion q2 )
        {
            q1.w += q2.w;
            q1.x += q2.x;
            q1.y += q2.y;
            q1.z += q2.z;
        }

        public static void QuaUnaryNegative( ref Quaternion q )
        {
            q.x *= -1f;
            q.y *= -1f;
            q.z *= -1f;
            q.w *= -1f;
        }

        public static Quaternion ToQuaternion( Vector3 left, Vector3 dir, Vector3 up )
        {
            float[] numArray1 = new float[ 4 ];
            float[,] numArray2 = new float[ 3, 3 ];
            for( int index = 0; index < 3; ++index )
            {
                numArray2[ index, 0 ] = left[ index ];
                numArray2[ index, 1 ] = up[ index ];
                numArray2[ index, 2 ] = dir[ index ];
            }
            float num1 = numArray2[ 0, 0 ] + numArray2[ 1, 1 ] + numArray2[ 2, 2 ];
            if( (double)num1 > 0.0 )
            {
                float num2 = Mathf.Sqrt( num1 + 1f );
                numArray1[ 0 ] = 0.5f * num2;
                float num3 = 0.5f / num2;
                numArray1[ 1 ] = ( numArray2[ 2, 1 ] - numArray2[ 1, 2 ] ) * num3;
                numArray1[ 2 ] = ( numArray2[ 0, 2 ] - numArray2[ 2, 0 ] ) * num3;
                numArray1[ 3 ] = ( numArray2[ 1, 0 ] - numArray2[ 0, 1 ] ) * num3;
            }
            else
            {
                int index1 = 0;
                if( (double)numArray2[ 1, 1 ] > (double)numArray2[ 0, 0 ] )
                    index1 = 1;
                if( (double)numArray2[ 2, 2 ] > (double)numArray2[ index1, index1 ] )
                    index1 = 2;
                int index2 = MathHelper.next[ index1 ];
                int index3 = MathHelper.next[ index2 ];
                float num4 = Mathf.Sqrt( (float)( (double)numArray2[ index1, index1 ] - (double)numArray2[ index2, index2 ] - (double)numArray2[ index3, index3 ] + 1.0 ) );
                numArray1[ index1 + 1 ] = 0.5f * num4;
                float num5 = 0.5f / num4;
                numArray1[ 0 ] = ( numArray2[ index3, index2 ] - numArray2[ index2, index3 ] ) * num5;
                numArray1[ index2 + 1 ] = ( numArray2[ index2, index1 ] + numArray2[ index1, index2 ] ) * num5;
                numArray1[ index3 + 1 ] = ( numArray2[ index3, index1 ] + numArray2[ index1, index3 ] ) * num5;
            }
            return new Quaternion( numArray1[ 1 ], numArray1[ 2 ], numArray1[ 3 ], numArray1[ 0 ] );
        }

        public static Quaternion ToQuaternionEx( Vector3 left, Vector3 dir, Vector3 up )
        {
            Matrix4x4 identity = Matrix4x4.identity;
            for( int index = 0; index < 3; ++index )
            {
                identity[ index, 0 ] = left[ index ];
                identity[ index, 2 ] = dir[ index ];
                identity[ index, 1 ] = up[ index ];
            }
            Quaternion quaternionEx = new Quaternion();
            quaternionEx.w = Mathf.Sqrt( Mathf.Max( 0.0f, 1f + identity[ 0, 0 ] + identity[ 1, 1 ] + identity[ 2, 2 ] ) ) / 2f;
            quaternionEx.x = Mathf.Sqrt( Mathf.Max( 0.0f, 1f + identity[ 0, 0 ] - identity[ 1, 1 ] - identity[ 2, 2 ] ) ) / 2f;
            quaternionEx.y = Mathf.Sqrt( Mathf.Max( 0.0f, 1f - identity[ 0, 0 ] + identity[ 1, 1 ] - identity[ 2, 2 ] ) ) / 2f;
            quaternionEx.z = Mathf.Sqrt( Mathf.Max( 0.0f, 1f - identity[ 0, 0 ] - identity[ 1, 1 ] + identity[ 2, 2 ] ) ) / 2f;
            quaternionEx.x *= Mathf.Sign( quaternionEx.x * ( identity[ 2, 1 ] - identity[ 1, 2 ] ) );
            quaternionEx.y *= Mathf.Sign( quaternionEx.y * ( identity[ 0, 2 ] - identity[ 2, 0 ] ) );
            quaternionEx.z *= Mathf.Sign( quaternionEx.z * ( identity[ 1, 0 ] - identity[ 0, 1 ] ) );
            return quaternionEx;
        }

        public static bool RayFaceIntersect( Vector3[] V, Vector3 S, Vector3 D )
        {
            Vector3 vector3_1 = V[ 1 ] - V[ 0 ];
            Vector3 vector3_2 = V[ 2 ] - V[ 0 ];
            Vector3 rhs1 = Vector3.Cross( D, vector3_2 );
            float num1 = Vector3.Dot( vector3_1, rhs1 );
            if( (double)num1 > -9.9999997473787516E-05 && (double)num1 < 0.0001 )
                return false;
            float num2 = 1f / num1;
            Vector3 lhs = S - V[ 0 ];
            float num3 = num2 * Vector3.Dot( lhs, rhs1 );
            if( (double)num3 < 0.0 || (double)num3 > 1.0 )
                return false;
            Vector3 rhs2 = Vector3.Cross( lhs, vector3_1 );
            float num4 = num2 * Vector3.Dot( D, rhs2 );
            if( (double)num4 < 0.0 || (double)num3 + (double)num4 > 1.0 )
                return false;
            float num5 = num2 * Vector3.Dot( vector3_2, rhs2 );
            return (double)num5 >= 0.0 && (double)num5 <= 1.0;
        }

        public static bool IsBetweenII( int value, int min, int max ) => value >= min && value <= max;

        public static bool IsBetweenII( float value, float min, float max ) => (double)value >= (double)min && (double)value <= (double)max;

        public static bool IsBetweenIE( int value, int min, int max ) => value >= min && value < max;

        public static bool IsBetweenIE( float value, float min, float max ) => (double)value >= (double)min && (double)value < (double)max;

        public static Vector3 BEZ3( float t, Vector3 a, Vector3 b, Vector3 c, Vector3 d ) => a * ( 1f - t ) * ( 1f - t ) * ( 1f - t ) + b * 3f * ( 1f - t ) * ( 1f - t ) * t + c * 3f * ( 1f - t ) * t * t + d * t * t * t;
    }
}
