/*
 author: Sam Ford (stf8464)
date started: 1/26/22
desc: class that represents a 3d point
*/

using System;

public class Point
{
    private double _x;
    private double _y;
    private double _z;

    public double x { get => this._x; set => this._x = value; }
    public double y { get => this._y; set => this._y = value; }
    public double z { get => this._z; set => this._z = value; }

//DEFAULT CONSTRUCTOR
    public Point()
    {
        this._x = 0;
        this._y = 0;
        this._z = 0;
    }
//FULL CONSTRUCTOR
    public Point( double x, double y, double z )
    {
        this._x = x;
        this._y = y;
        this._z = z;
    }

//operator overloads + and -
    public static Point operator +( Point p1 ) => new Point( (p1.x), (p1.y), (p1.z) );
    public static Point operator -( Point p1 ) => new Point( -(p1.x), -(p1.y), -(p1.z) );

    public static Point operator +( Point p1, Vector v1 ) => new Point( p1.x + v1.v1, p1.y + v1.v2, p1.z + v1.v3 );
    public static Point operator -( Point p1, Vector v1 ) => new Point( p1.x - v1.v1, p1.y - v1.v2, p1.z - v1.v3 );

    public static Vector operator -( Point p1, Point p2 ) => new Vector( p1.x - p2.x, p1.y - p2.y, p1.z - p2.z );


//METHODS
    //TODO FIGURE OUT HOW TO DO THIS
    public void transform( double x, double y, double z)
    {
        this._x += x;
        this._y += y;
        this._z =+ z;

        return;
    }

//calculate distance
    public double distance( Point p2)
    {

        return Math.Sqrt( Math.Pow( (p2.x - this._x), 2 )
                        + Math.Pow( (p2.y - this._y), 2 )
                        + Math.Pow( (p2.z - this._z), 2 ) );
    }
}