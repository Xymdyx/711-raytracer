using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.IO;
using RayTracer_App.Scene_Objects;
using RayTracer_App.Illumination_Models;

//https://stackoverflow.com/questions/37411371/read-and-import-a-ply-flle-with-c-sharp

namespace RayTracer_App.aux_classes
{
    public static class PlyParser
    {
        //default file has a face list...
        const string FILENAME = @"C:\Users\Sam\Desktop\CSCLasses\GI\New folder\bunny\reconstruction\bun_zipper_res4.ply"; //treat verbatim...allows for spaces and \s without escape chars;

        const int TRIJUMP = 3;
        const int STARTTRIIDX = 2;

        public static Polygon buildTriangleFromFace( List<int> indices, List<Point> vertices)
		{
            if( indices.Count == 3)
			{
                Point p1 = vertices[indices[0]].copy();
                Point p2 = vertices[indices[1]].copy();
                Point p3 = vertices[indices[2]].copy();
                List<Point> plyVerts = new List<Point> { p1, p2, p3 };
                return new Polygon( plyVerts, Color.bunnyColor);
			}
            return null;
		}

        //parse only the vertices of a given plyyFile and return total polygons
        public static List<Polygon> parseEdgePly( Vector originVec, String fileName = FILENAME )
        {
            List<Point> data = new List<Point>();
            List<Polygon> plyTriangles = new List<Polygon>();

            StreamReader reader = new StreamReader( fileName );
            string inputLine = "";
            Boolean endHeader = false;

            int totalVertices = int.MaxValue;
            int totalFaces = int.MaxValue;

            while ( ((inputLine = reader.ReadLine()) != null) && ( (totalVertices != 0) || (totalFaces != 0) ) )
            {
                inputLine = inputLine.Trim();
                if ( inputLine.Length > 0 )
                {
                    //parse vetex data
                    if (endHeader)
                    {
                        //parse Vertices
                        if (totalVertices != 0)
                        {
                            List<float> newRow = inputLine.Split( new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries ).Select( x => float.Parse( x ) ).ToList();
                            Point vertex = new Point( newRow[0], newRow[1], newRow[2] );
                            //the ply is read as a RHS as far as I can tell, so I do negative transforms here to not have the rabbit upside down
                            vertex.scale( 2.5f, -2.5f, -2.5f );
                            vertex.translate( originVec.v1, originVec.v2, originVec.v3 );
                            data.Add( vertex );
                            totalVertices--; //we want only the vertices
                        }

                        //make the vertices into triangles
                        else if ( totalFaces != 0 )
						{
                            List<int> newTriIdx = inputLine.Split( new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries ).Select( x => int.Parse( x ) ).ToList();
                            int ptCount = newTriIdx[0];

                            List<int> triVertices = newTriIdx.GetRange( 1, ptCount ); //shallow copy of the range... this may disappear.. [sIdx, sIdx + (count -1) ] inclusive
                            Polygon newTriangle = buildTriangleFromFace( triVertices, data);
                            newTriangle.lightModel = PhongBlinn.bunnyBlinn;
                            //newTriangle.scale(1.5f, 1.5f, 1.5f );
                            plyTriangles.Add( newTriangle );
                            totalFaces--;
						}
                    }
                    else
                    {
                        if (inputLine.Contains( "end_header" ))
                        {
                            endHeader = true;
                        }

                        //how many vertex lines after header?
                        else if (inputLine.Contains( "element vertex" ))
                        {
                            String[] vertexString;
                            vertexString = inputLine.Split( new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries );
                            totalVertices = int.Parse( vertexString[vertexString.Length - 1] ); //grab the vertex integer at end
                        }
                        // how many faces?
                        else if (inputLine.Contains( "element face" ))
                        {
                            String[] faceString;
                            faceString = inputLine.Split( new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries );
                            totalFaces = int.Parse( faceString[faceString.Length - 1] ); //grab the face integer at end...
                        }
                    }
                }
            }
            /*debug.. prints correctly, VS likes to not show all outpuit...
            foreach (Point p in data)
                Console.WriteLine( p );

            Console.WriteLine();

            foreach (Polygon tri in plyTriangles)
                Console.WriteLine( tri );*/

            return plyTriangles;
        }
    }
}
