/*
 author: Sam Ford (stf8464)
date started: 1/26/22
desc: class that represents a 3d vector
*/

using System;
//CONVERTED DOUBLE -> FLOAT!
public class Vector
{
    private static Vector ZERO_VEC = new Vector( 0, 0, 0 );

    // fields
    private float _v1;
    private float _v2;
    private float _v3;

    //properties
    public float v1 { get => this._v1; set => this._v1 = value; }
    public float v2 { get => this._v2; set => this._v2 = value; }
    public float v3 { get => this._v3; set => this._v3 = value; }

// default constructor
    public Vector()
    {
        this.v1 = 1;
        this.v2 = 1;
        this.v3 = 1;
        this.normalize();
    }

// constructor
    public Vector(float v1, float v2, float v3, bool normalize = true)
    {
        this.v1 = v1;
        this.v2 = v2;
        this.v3 = v3;
        if( normalize ) this.normalize();
    }

//operator overloads + and -
    public static Vector operator +(Vector vec1) => new Vector( (vec1.v1), (vec1.v2), (vec1.v3) );
    public static Vector operator -(Vector vec1) => new Vector(-(vec1.v1), -(vec1.v2), -(vec1.v3));

    public static Vector operator +(Vector vec1, Vector vec2) => new Vector(vec1.v1 + vec2.v1, vec1.v2 + vec2.v2, vec1.v3 + vec2.v3);
    public static Vector operator -(Vector vec1, Vector vec2) => vec1 + -vec2;

    public static bool operator ==( Vector lhs, Vector rhs ) 
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

    public static bool operator !=( Vector lhs, Vector rhs ) => !(lhs == rhs);


    // self-operations
    public Vector scale(float k)
    {
        return new Vector(k * this.v1, k * this.v2, k * this.v3, false);
    }

    public float getLen()
    {
        return (float) Math.Sqrt( (this.v1 * this.v1 ) + (this.v2 * this.v2) + (this.v3 * this.v3) );
    }

    // normalize this vector to have 1 length
    public void normalize()
    {
        float magn = getLen();
        if (magn != 0)
        { 
            this.v1 /= magn;
            this.v2 /= magn;
            this.v3 /= magn;
        }
        return;
    }

    //operators with points

    //operators with other vecs

    /*
     * return dot product scalar value
     *  vec2:  the vector on the RHS of tthe dot product (commutative)
     */
    public float dotProduct( Vector vec2 )
    {

        return ((this.v1 * vec2.v1) + (this.v2 * vec2.v2) + (this.v3 * vec2.v3));
    }


    /* 
     * return the cosine between this vector and vec2 in degrees
     * vec2: the vector on the RHS of tthe operation
     */
    public float cosineBetween(Vector vec2)
    {
        return (float) Math.Acos(this.dotProduct(vec2) /(this.getLen() * vec2.getLen()) * (180 / Math.PI));
    }


    /*
     * return the perpendicular vector betwn this vector and vec2
     * vec2: the vector on the RHS of tthe Cross product (order matters)
     */
    public Vector crossProduct(Vector vec2, bool normalize = true) //fixed issue w second term...
    {
        return new Vector((this.v2 * vec2.v3) - (this.v3 * vec2.v2),
                       (this.v3 * vec2.v1) - ( this.v1 * vec2.v3) ,
                       (this.v1 * vec2.v2) - (this.v2 * vec2.v1) , normalize);
    }

    /*
     * projection of v (vec2) onto  u (this vec)
     */
    public Vector projectOnto(Vector vec2)
    {
        float magn = this.getLen();
        return this.scale( (float) (dotProduct(vec2) / (magn * magn) ));
    }

    /*
     * vector comp of v perp. to u
     */
    public Vector perpTo(Vector vec2)
    {
        return vec2 - projectOnto(vec2);
    }

    //used to determine if two vectors are equal
    public override bool Equals( Object obj )
	{
        if ( (obj == null) || !(this.GetType().Equals( obj.GetType())) )
            return false;

        Vector v = (Vector) obj;
        return ( (v.v1 == this.v1) && (v.v2 == this.v2) && (v.v3 == this.v3) );
	}

    // vec ops without normalizing
    public Vector addVec( Vector vec2 )
    {
        return new Vector( this.v1 + vec2.v1, this.v2 + vec2.v2, this.v3 + vec2.v3, false );
    }

    // vec ops without normalizing
    public Vector subVec( Vector vec2 )
    {
        return new Vector( this.v1 - vec2.v1, this.v2 - vec2.v2, this.v3 - vec2.v3, false );
    }

    //TODO... may cause problems
    public override int GetHashCode()
    {
        int hash = (int) v1 >> 2 + (int) v2 >> 2 + (int) v3 >> 2;
        return hash;
    }

    public bool isZeroVector()
	{
        return ((0 == this.v1) && (0 == this.v2) && (0 == this.v3));
    }

    //REFLECT METHOD
    // reflect = Incoming - 2( (Incoming.dot(normal) * normal) / (normalLength^2) )
    public static Vector reflect( Vector incoming, Vector normal )
	{
        
        float inNormDp = incoming.dotProduct( normal );
       // float len = normal.getLen(); //should be 1
       // float divisor = len * len; //fixed
        Vector rightTerm = normal.scale( 2 * (inNormDp) ) ; //does not normalize here
        return rightTerm - incoming ;
	}

    //TRANSMIT METHOD
    // where ni and nt are indexes of refraction
    // trans = ((ni * (dir - normal*(d dot n))) /nt) + ( n * sqrt( 1 - ( (ni^2* (1- d dot n)^2 )/ ni^2 )
    public static Vector transmit( Vector dir, Vector normal, float ni, float nt )
	{
        //same direction if indices of refraction are the same
        if (ni == nt) 
            return dir;

        float dnDP = dir.dotProduct( normal );
        float nRat = ni / nt;

        Vector leftTerm = ( dir.subVec(normal.scale( dnDP )) ).scale(nRat);

        // the sqrt term is equal to 1 - (ni ( 1 - (d dot n) )/ nt
        float rightScale = 1 - ( nRat * (1 - dnDP) ) ;
        Vector rightTerm = normal.scale( rightScale );

        return leftTerm + rightTerm;
	}

    //FACEFORWARD method
    // for cp6. Use negative normal for calculations
    public static Vector faceForward( Vector normal, Vector traveling )
	{
        //acute angle, use regular normal
        if (normal.dotProduct( traveling ) >= 0) return normal;

        //obtuse angle, return reverse of normal
        return -normal;
	}

    //TOSTRING METHOD
    public override string ToString()
    {
        return $"Vector (u1, u2, u3) = ({this.v1}, {this.v2} , {this.v3})\n";
    }

    public Vector copy()
	{
        return new Vector( this.v1, this.v2, this.v3, false );
	}
}
