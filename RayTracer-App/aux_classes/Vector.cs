/*
 author: Sam Ford (stf8464)
date started: 1/26/22
desc: class that represents a 3d vector
*/

using System;
//CONVERTED DOUBLE -> FLOAT!
public class Vector
{
    public static Vector ZERO_VEC = new Vector( 0, 0, 0 );
    public static Vector UP_VEC = new Vector( 0, 1F, 0 );
    public static Vector LEFT_VEC = new Vector( 1f, 0, 0 );
    public static Vector FORWARD_VEC = new Vector( 0, 0, 1f );



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

    public static Vector operator *( Vector vec1, float k ) => new Vector( (vec1.v1 * k), (vec1.v2 * k), (vec1.v3 * k), false );

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


    // weird getters
    public float getAxisComp( int axis )
    {
        if (axis == 0) return this.v1;
        else if (axis == 1) return this.v2;
        else if (axis == 2) return this.v3;

        return float.NaN;
    }

    //weird setters.. 0 = x, 1 = 1, 2 = z
    public void setAxisComp( int axis, float val )
    {
        if (axis == 0) this.v1 = val;
        else if (axis == 1) this.v2 = val;
        else if (axis == 2) this.v3 = val;

        return;
    }

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

	///REFLECT METHOD... https://en.wikipedia.org/wiki/Phong_reflection_model
	//reflect = Incoming - 2( (Incoming.dot(normal) * normal) / (normalLength^2) ).. i do this weirdly when i is negative cam ray
    // an equivalent form is: r = i + 2(-i dot n) * n for positive cam ray
	public static Vector reflect( Vector incoming, Vector normal )
	{

		float inNormDp = incoming.dotProduct( normal );
		Vector rightTerm = normal.scale( 2f * (inNormDp) ); //does not normalize here
		return rightTerm - incoming;
	}

    // an equivalent form is: r = i + 2(-i dot n) * n for positive cam ray
    // reflect 2    
    public static Vector reflect2( Vector incoming, Vector normal )
    {

        float inNormDp = -incoming.dotProduct( normal );
        Vector rightTerm = normal.scale( 2f * (inNormDp) ); //does not normalize here
        return rightTerm + incoming;
    }

    //TRANSMIT METHOD... handles logic in method
    // where ni and nt are indexes of refraction
    //preconds: dotProduct done before this
    //https://graphics.stanford.edu/courses/cs148-10-summer/docs/2006--degreve--reflection_refraction.pdf
    public static Vector transmit( Vector dir, Vector normal, float ni, float nt )
	{
		//same direction if indices of refraction are the same
		if (ni == nt)
			return dir;

		// t = (n1/n2)i + ( (n1/n2) *cosi -  sqrt( 1- sin^2t) * n
		float cosi = -(normal.dotProduct( dir )); //this is always positive, which is what we want...
		float nRat = ni / nt;

        // cosi = -(i dot n)
        // sin^2t = (n1/n2)^2 * ( 1- cos^2 i).. TIR  when n1 > n2
        //CritAngle = arcsin (n2/n1) <=> n1 > n2

        Vector leftTerm = dir.scale( nRat );
		float sqrtTerm = (float)(1.0f - ((nRat * nRat) * (1.0f - (cosi * cosi)))); //this is sometimes negative...

        if (sqrtTerm < 0) //transmission direction doesn't exist
            return reflect2( dir, normal );
            
		float rightScale = (float)((nRat * cosi) - Math.Sqrt( sqrtTerm ));  //getting NAN here
		Vector rightTerm = normal.scale( rightScale );

		return leftTerm + rightTerm;
	}

    // scratchapixel method that handles logic for flipping normal inside of the method
    public static Vector transmit2( Vector dir, Vector normal, float ni, float nt )
    {
        //same direction if indices of refraction are the same
        if (ni == nt)
            return dir;

        // t = (n1/n2)i + ( (n1/n2) *cosi -  sqrt( 1- sin^2t) * n
        float cosi = normal.dotProduct( dir ); //this is always positive, which is what we want...
        float nRat;
        Vector n = normal;

        if (cosi < 0)
        {
            cosi = -cosi;
            nRat = ni / nt;
        }
        else
        {
            nRat = nt / ni;
            n = normal.scale( -1f );
        }

        // cosi = -(i dot n)
        // sin^2t = (n1/n2)^2 * ( 1- cos^2 i).. TIR  when n1 > n2
        Vector leftTerm = dir.scale( nRat );
        float sqrtTerm = (float)(1.0f - ((nRat * nRat) * (1.0f - (cosi * cosi)))); //this is sometimes negative...

        if (sqrtTerm < 0) //transmission direction doesn't exist
            return reflect2( dir, normal );

        float rightScale = (float)((nRat * cosi) - Math.Sqrt( sqrtTerm ));  //getting NAN here
        Vector rightTerm = n.scale( rightScale );

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

    //normal-space to space relative to normal functions...
    // try using this once we get a random unit direction from is
    //to convert back to camera space
    //https://www.gamedev.net/blogs/entry/2261086-importance-sampling/
    public static Vector findOrthoUnitVec( Vector normal )
	{
        if (normal.v1 == 0f) return LEFT_VEC;
        else return UP_VEC.crossProduct(normal);
	}

    public static Vector findBitTangent( Vector normal )
	{
        Vector tangent = findOrthoUnitVec( normal );
        return tangent.crossProduct( normal );
	}

    //convert from a local space back to the relative space of the current normal
    public static Vector normaltoSpace( Vector normal, Vector hemiUnitVec )
	{
        Vector tangent = findOrthoUnitVec( normal );
        Vector bitTangent = findBitTangent( normal );
        Vector camSpaceVec = tangent.scale(hemiUnitVec.v1) + normal.scale( hemiUnitVec.v2 ) + bitTangent.scale(hemiUnitVec.v3);

        return camSpaceVec;
	}

    //https://computergraphics.stackexchange.com/questions/10622/path-tracing-how-to-ensure-we-are-sampling-a-direction-vector-within-the-visibl
    // calculating direction wrt hemisphere around normal...
    public static Vector dirAroundNormalHemisphere( Vector normal, float theta, float phi )
	{
        Vector tangent = (normal.v1 > normal.v3) ? new Vector( -normal.v2, normal.v1, 0.0f, false ) : new Vector( 0.0f, -normal.v3, normal.v2, false );
        Vector bitTangent = normal.crossProduct( tangent );
        float sinTheta = (float)Math.Sin( theta );
        return (tangent * sinTheta * (float)Math.Cos( phi )) + (bitTangent * (float)Math.Sin( phi ) * sinTheta) + (normal * (float)Math.Cos( theta ));
        // and a bitangent vector orthogonal to both
    }
}

////// scratch REFLECT METHOD https://en.wikipedia.org/wiki/Phong_reflection_model
////// reflect = Incoming - 2( (Incoming.dot(normal) * normal) / (normalLength^2) ).. i do this weirdly
//public static Vector sReflect( Vector incoming, Vector normal )
//{

//	float inNormDp = incoming.dotProduct( normal );
//	Vector rightTerm = normal.scale( 2f * (inNormDp) ); //does not normalize here
//	return incoming - rightTerm;
//}


//scratch TRANSMIT METHOD... handles logic in method
// where ni and nt are indexes of refraction
// t = (n1/n2)i + ( (n1/n2) *cosi -  sqrt( 1- sin^2t) * n
// cosi = -(i dot n)
// sin^2t = (n1/n2)^2 * ( 1- cos^2 i).. TIR  when n1 > n2
// https://www.scratchapixel.com/code.php?id=3&origin=/lessons/3d-basic-rendering/introduction-to-ray-tracing


