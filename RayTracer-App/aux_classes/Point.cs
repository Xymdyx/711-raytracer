/*
 author: Sam Ford (stf8464)
date started: 1/26/22
desc: class that represents a 3d point
*/

using System;
using System.Numerics;

//double -> float and Matrix4d -> System.Numerics Matrix4x4
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

        return (float) Math.Sqrt( ((p2.x - this._x) * (p2.x - this._x))
                        + ((p2.y - this._y) * (p2.y - this._y))
                        + ((p2.z - this._z) * (p2.z - this._z)));
    }

    // to a non-normalized vector
    public Vector toVec( bool normalize = false)
	{
        return new Vector( this.x, this.y, this.z, normalize );
	}

    // from Matrix4d in OpenGLDotNet to Vector3 in System.numerics
    // https://github.com/microsoft/referencesource/blob/master/System.Numerics/System/Numerics/Vector3.cs
    public Vector4 toHmgCoords()
    {
        return new Vector4( this.x, this.y, this.z, 1f ); //tested that this works fine
    }
    
    // reconvert to 3d coords after making transformations with toHmgCoords
    public void fromHmgCoords( Vector4 hmgMat )
    {
        //convert from row-major hmg mat back to a new Point
        this.x = hmgMat.X;
        this.y = hmgMat.Y;
        this.z = hmgMat.Z;
        // / hmgMat.W; //the w may not be 1 here, do we still divide by it?
        // / hmgMat.W; //the w may not be 1 here, do we still divide by it?
        /// hmgMat.W;
    }

    public bool isOrigin()
    {
        return (this.x == 0 && this.y == 0 && this.z == 0);
    }

    //translate a Point via row-major Vector4 transformed with Mat4x4
    public void translate( float x, float y, float z )
    {
        Vector4 ptMat = this.toHmgCoords( );
        Vector4 ptHmg = this.toHmgCoords();
        Matrix4x4 trans = new Matrix4x4
            ( 1, 0, 0, 0,
             0, 1, 0, 0,
             0, 0, 1, 0,
             x, y, z, 1 );
        Vector4 newTransVec = Vector4.Transform( ptHmg, trans );
        this.fromHmgCoords( newTransVec );

        return;
    }

    //scale a Point via row-major Vector4 transformed with Mat4x4
    public void scale( float x, float y, float z )
	{
        Vector4 ptHmg = this.toHmgCoords();
        Matrix4x4 scale = new Matrix4x4
            ( x, 0, 0, 0,
             0, y, 0, 0,
             0, 0, z, 0,
             0, 0, 0, 1 );
        Vector4 newScaledVec = Vector4.Transform( ptHmg, scale );
        this.fromHmgCoords( newScaledVec );
    }

    //TODO ADD TRANSFORMATIONS SUCH AS ROTATING

    // subtract two points to get vector sans normalizing. For Moller-Trumbone ray-triangle
    public Vector ptSub( Point p2)
    {
        return new Vector( this.x - p2.x, this.y - p2.y, this.z - p2.z, false );
    }

    /* Vector4 and Matrix4x4 test:
 Vector4 test1 = new Vector4( 1, 1, 1, 1 );
		Matrix4x4 mat1 = new Matrix4x4
			( 1, 2, 3, 4,
			  5, 6, 7, 8 ,
			  9, 10, 11, 12,
			  13, 14, 15, 16);
		Vector4 result1 = Vector4.Transform( test1, mat1 );
		Console.WriteLine( test1 + " vector before matrix multiply with:\n" + mat1 );
		Console.WriteLine("\n" + test1 + " vector after matrix multiply :\n" + result1 ); */
}