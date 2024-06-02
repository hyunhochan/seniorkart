using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets
{
    public struct Matrix3
    {
        public float[,] m;

        public static Matrix3 CreateMtx() => new Matrix3()
        {
            m = new float[ 3, 3 ]
        };

        public static Matrix3 CreateMtxCol( Vector3 col0, Vector3 col1, Vector3 col2 )
        {
            Matrix3 mtxCol = new Matrix3();
            mtxCol.m = new float[ 3, 3 ];
            for( int index = 0; index < 3; ++index )
            {
                mtxCol.m[ index, 0 ] = col0[ index ];
                mtxCol.m[ index, 1 ] = col1[ index ];
                mtxCol.m[ index, 2 ] = col2[ index ];
            }
            return mtxCol;
        }

        public static Matrix3 CreateMtxIdentity() => Matrix3.CreateMtx( 1f, 0.0f, 0.0f, 0.0f, 1f, 0.0f, 0.0f, 0.0f, 1f );

        public static Matrix3 CreateMtx(
          float _11,
          float _12,
          float _13,
          float _21,
          float _22,
          float _23,
          float _31,
          float _32,
          float _33 )
        {
            Matrix3 mtx = new Matrix3();
            mtx.m = new float[ 3, 3 ];
            mtx.m[ 0, 0 ] = _11;
            mtx.m[ 0, 1 ] = _12;
            mtx.m[ 0, 2 ] = _13;
            mtx.m[ 1, 0 ] = _21;
            mtx.m[ 1, 1 ] = _22;
            mtx.m[ 1, 2 ] = _23;
            mtx.m[ 2, 0 ] = _31;
            mtx.m[ 2, 1 ] = _32;
            mtx.m[ 2, 2 ] = _33;
            return mtx;
        }

        public void SetValue( ref Matrix3 mat )
        {
            for( int index1 = 0; index1 < 3; ++index1 )
            {
                for( int index2 = 0; index2 < 3; ++index2 )
                    this.m[ index1, index2 ] = mat.m[ index1, index2 ];
            }
        }

        public Vector3 getRow( int row ) => new Vector3( this.m[ row, 0 ], this.m[ row, 1 ], this.m[ row, 2 ] );

        public Vector3 getCol( int col ) => new Vector3( this.m[ 0, col ], this.m[ 1, col ], this.m[ 2, col ] );

        public void setCol( int col, Vector3 v )
        {
            for( int index = 0; index < 3; ++index )
            {
                this.m[ index, col ] = v[ index ];
            }
        }

        public void setCol( Vector3 v1, Vector3 v2, Vector3 v3 )
        {
            this.setCol( 0, v1 );
            this.setCol( 1, v2 );
            this.setCol( 2, v3 );
        }

        public static Vector3 operator *( Matrix3 mat, Vector3 v ) => new Vector3( (float)( (double)mat.m[ 0, 0 ] * (double)v.x + (double)mat.m[ 0, 1 ] * (double)v.y + (double)mat.m[ 0, 2 ] * (double)v.z ), (float)( (double)mat.m[ 1, 0 ] * (double)v.x + (double)mat.m[ 1, 1 ] * (double)v.y + (double)mat.m[ 1, 2 ] * (double)v.z ), (float)( (double)mat.m[ 2, 0 ] * (double)v.x + (double)mat.m[ 2, 1 ] * (double)v.y + (double)mat.m[ 2, 2 ] * (double)v.z ) );

        public static Matrix3 operator *( Matrix3 mat1, Matrix3 mat2 ) => Matrix3.CreateMtx( (float)( (double)mat1.m[ 0, 0 ] * (double)mat2.m[ 0, 0 ] + (double)mat1.m[ 0, 1 ] * (double)mat2.m[ 1, 0 ] + (double)mat1.m[ 0, 2 ] * (double)mat2.m[ 2, 0 ] ), (float)( (double)mat1.m[ 0, 0 ] * (double)mat2.m[ 0, 1 ] + (double)mat1.m[ 0, 1 ] * (double)mat2.m[ 1, 1 ] + (double)mat1.m[ 0, 2 ] * (double)mat2.m[ 2, 1 ] ), (float)( (double)mat1.m[ 0, 0 ] * (double)mat2.m[ 0, 2 ] + (double)mat1.m[ 0, 1 ] * (double)mat2.m[ 1, 2 ] + (double)mat1.m[ 0, 2 ] * (double)mat2.m[ 2, 2 ] ), (float)( (double)mat1.m[ 1, 0 ] * (double)mat2.m[ 0, 0 ] + (double)mat1.m[ 1, 1 ] * (double)mat2.m[ 1, 0 ] + (double)mat1.m[ 1, 2 ] * (double)mat2.m[ 2, 0 ] ), (float)( (double)mat1.m[ 1, 0 ] * (double)mat2.m[ 0, 1 ] + (double)mat1.m[ 1, 1 ] * (double)mat2.m[ 1, 1 ] + (double)mat1.m[ 1, 2 ] * (double)mat2.m[ 2, 1 ] ), (float)( (double)mat1.m[ 1, 0 ] * (double)mat2.m[ 0, 2 ] + (double)mat1.m[ 1, 1 ] * (double)mat2.m[ 1, 2 ] + (double)mat1.m[ 1, 2 ] * (double)mat2.m[ 2, 2 ] ), (float)( (double)mat1.m[ 2, 0 ] * (double)mat2.m[ 0, 0 ] + (double)mat1.m[ 2, 1 ] * (double)mat2.m[ 1, 0 ] + (double)mat1.m[ 2, 2 ] * (double)mat2.m[ 2, 0 ] ), (float)( (double)mat1.m[ 2, 0 ] * (double)mat2.m[ 0, 1 ] + (double)mat1.m[ 2, 1 ] * (double)mat2.m[ 1, 1 ] + (double)mat1.m[ 2, 2 ] * (double)mat2.m[ 2, 1 ] ), (float)( (double)mat1.m[ 2, 0 ] * (double)mat2.m[ 0, 2 ] + (double)mat1.m[ 2, 1 ] * (double)mat2.m[ 1, 2 ] + (double)mat1.m[ 2, 2 ] * (double)mat2.m[ 2, 2 ] ) );
    }
}
