/*
 author: Sam Ford (stf8464)
date started: 1/26/22
desc: class that represents a 3d point
*/

using System;
using System.Numerics;

//double -> float and Matrix4d -> System.Numerics Matrix4x4
//TODO REVIEW 
public class Point
{
    private float _x;
    private float _y;
    private float _z;

    public float x { get => this._x; set => this._x = value; }
    public float y { get => this._y; set => this._y = value; }
    public float z { get => this._z; set => this._z = value; }

//DEFAULT CONSTRUCTOR
    public Point()
    {
        this._x = 0;
        this._y = 0;
        this._z = 0;
    }
//FULL CONSTRUCTOR
    public Point( float x, float y, float z )
    {
        this._x = x;
        this._y = y;
        this._z = z;
    }

//operator overloads + and -... All of these normalize, use methods for non-normalized
    public static Point operator +( Point p1 ) => new Point( (p1.x), (p1.y), (p1.z) );
    public static Point operator -( Point p1 ) => new Point( -(p1.x), -(p1.y), -(p1.z) );

    public static Point operator +( Point p1, Vector v1 ) => new Point( p1.x + v1.v1, p1.y + v1.v2, p1.z + v1.v3 );
    public static Point operator -( Point p1, Vector v1 ) => new Point( p1.x - v1.v1, p1.y - v1.v2, p1.z - v1.v3 );

    public static Vector operator -( Point p1, Point p2 ) => new Vector( p1.x - p2.x, p1.y - p2.y, p1.z - p2.z );


//METHODS

//calculate distance
    public float distance( Point p2)
    {

        return (float) Math.Sqrt( Math.Pow( (p2.x - this._x), 2 )
                        + Math.Pow( (p2.y - this._y), 2 )
                        + Math.Pow( (p2.z - this._z), 2 ) );
    }

    public Vector toVec()
	{
        return new Vector( this.x, this.y, this.z, false );
	}

    // from Matrix4d in OpenGLDotNet to Matrix4x4 in System.numerics
    // https://github.com/microsoft/referencesource/blob/master/System.Numerics/System/Numerics/Vector3.cs
    public Vector4 toHmgCoords()
    {
        return new Vector4( this.x, this.y, this.z, 1f );
    }
    public void fromHmgCoords( Vector4 hmgMat )
    {
        //convert from row-major hmg mat back to a new Point
        this.x = hmgMat.X / hmgMat.W;
        this.y = hmgMat.Y / hmgMat.W;
        this.z = hmgMat.Z/ hmgMat.W;
    }

    public bool isOrigin()
    {
        return (this.x == 0 && this.y == 0 && this.z == 0);
    }

    //TODO FIGURE OUT HOW TO DO THIS
    public void translate( float x, float y, float z )
    {
        Vector4 ptMat = this.toHmgCoords( );
        Matrix4x4 tranMat = new Matrix4x4
            ( 1, 0, 0, 0,
            0, 1, 0, 0,
            0, 0, 1, 0,
            x, y, z, 1 );
        Matrix4x4 result = tranMat;

        return;
    }

    // subtract two points to get vector sans normalizing. For Moller-Trumbone ray-triangle
    public Vector ptSub( Point p2)
    {
        return new Vector( this.x - p2.x, this.y - p2.y, this.z - p2.z, false );
    }

}