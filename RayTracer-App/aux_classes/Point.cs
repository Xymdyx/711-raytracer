/*
 author: Sam Ford (stf8464)
date started: 1/26/22
desc: class that represents a 3d point
*/

using System;

public class Point
{
    double x;
    double y;
    double z;

    public void transform( double x, double y, double z)
    {
        this.x = x;
        this.y = y;
        this.z = z;

        return;
    }

    public double distance( Point p1, Point p2)
    {

        return Math.Sqrt( Math.Pow( (p2.x - p1.x), 2 )
                        + Math.Pow( (p2.y - p1.y), 2 )
                        + Math.Pow( (p2.z - p1.z), 2 ) );
    }
}