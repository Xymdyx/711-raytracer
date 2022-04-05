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
    // CONSTANTS
    public static Point floorOrigin = new Point( -6f, 1.25f, 60.5f ); // floor origin for cp4
    public enum Axes { X, Y, Z }

    private float _x;
    private float _y;
    private float _z;

    private Point _texCoord;

    public float x { get => this._x; set => this._x = value; }
    public float y { get => this._y; set => this._y = value; }
    public float z { get => this._z; set => this._z = value; }
    public Point texCoord { get => this._texCoord; set => this._texCoord = value; }

    public static Point origin = new Point( 0, 0, 0 );
//DEFAULT CONSTRUCTOR
    public Point()
    {
        this._x = 0;
        this._y = 0;
        this._z = 0;
        this._texCoord = null; 
    }
//FULL CONSTRUCTOR
    public Point( float x, float y, float z )
    {
        this._x = x;
        this._y = y;
        this._z = z;
        this._texCoord = null;
    }

    //operator overloads + and -... All of these normalize, use methods for non-normalized
    public static Point operator +( Point p1 ) => new Point( (p1.x), (p1.y), (p1.z) );
    public static Point operator -( Point p1 ) => new Point( -(p1.x), -(p1.y), -(p1.z) );

    public static Point operator +( Point p1, Vector v1 ) => new Point( p1.x + v1.v1, p1.y + v1.v2, p1.z + v1.v3 );
    public static Point operator -( Point p1, Vector v1 ) => new Point( p1.x - v1.v1, p1.y - v1.v2, p1.z - v1.v3 );

    public static Vector operator -( Point p1, Point p2 ) => new Vector( p1.x - p2.x, p1.y - p2.y, p1.z - p2.z );

    public static Point operator *( Point p1, float k ) => new Point( p1.x * k, p1.y * k, p1.z * k );

    public static bool operator ==( Point lhs, Point rhs )
	{
        if (lhs is null)
        {
            if (rhs is null)
            {
                // null == null = true.
                return true;
            }

            // Only the left side is null.
            return false;
        }
        // Equals handles the case of null on right side.
        return lhs.Equals( rhs );
    }

    public static bool operator !=( Point lhs, Point rhs ) => !( lhs == rhs );

    public override bool Equals( object obj )
    {
        if ((obj == null) || !(this.GetType().Equals( obj.GetType() )))
            return false;

        Point p = (Point)obj;
        return ((p.x == this.x) && (p.y == this.y) && (p.z == this.z));
    }

    //METHODS
//METHODS
    public float getAxisCoord( int axis )
	{
        if (axis == 0) return this.x;
        else if (axis == 1) return this.y;
        else if (axis == 2) return this.z;

        return float.NaN;
	}

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

    //rotate a Point about the x via row-major Vector4 transformed with Mat4x4
    public void rotateX( float degrees )
    {
        Vector4 ptHmg = this.toHmgCoords();
        degrees *= (float) (Math.PI / 180f);
        float cos = (float) Math.Cos( degrees );
        float sin = (float)Math.Sin( degrees );

        Matrix4x4 rotX = new Matrix4x4
            ( 1, 0, 0, 0,
             0, cos ,-sin, 0,
             0, sin, cos, 0,
             0, 0, 0, 1 );
        Vector4 newScaledVec = Vector4.Transform( ptHmg, rotX );
        this.fromHmgCoords( newScaledVec );
    }

    //rotate a Point about the y via row-major Vector4 transformed with Mat4x4
    public void rotateY( float degrees )
    {
        Vector4 ptHmg = this.toHmgCoords();
        degrees *= (float)(Math.PI / 180f);
        float cos = (float)Math.Cos( degrees );
        float sin = (float)Math.Sin( degrees );

        Matrix4x4 rotY = new Matrix4x4
            ( cos, 0, sin, 0,
             0, 0, 0, 0,
             -sin, 0, cos, 0,
             0, 0, 0, 1 );
        Vector4 newScaledVec = Vector4.Transform( ptHmg, rotY );
        this.fromHmgCoords( newScaledVec );
    }

    //rotate a Point about the z via row-major Vector4 transformed with Mat4x4
    public void rotateZ( float degrees )
    {
        Vector4 ptHmg = this.toHmgCoords();
        degrees *= (float)(Math.PI / 180);
        float cos = (float)Math.Cos( degrees );
        float sin = (float)Math.Sin( degrees );

        Matrix4x4 rotZ = new Matrix4x4
            ( cos, -sin, 0, 0,
             sin, cos, 0, 0,
             0, 0, 0, 0,
             0, 0, 0, 1 );
        Vector4 newScaledVec = Vector4.Transform( ptHmg, rotZ );
        this.fromHmgCoords( newScaledVec );
    }

    //TODO ADD TRANSFORMATIONS SUCH AS ROTATING

    // subtract two points to get vector sans normalizing. For Moller-Trumbone ray-triangle
    // dest - origin
    public Vector ptSub( Point p2)
    {
        return new Vector( this.x - p2.x, this.y - p2.y, this.z - p2.z, false );
    }

    //displace method to prevent acne from floating point round-off
    public Point displaceMe( Vector along, float bias = 1e-6f)
	{
        Vector displaceVec = along.scale( bias );
        return this + displaceVec;
    }
	public override string ToString()
	{
		return $"Point [{this.x}, {this.y}, {this.z}]";
    }


    public Point copy()
    {
        return new Point( this.x, this.y, this.z );
    }
}