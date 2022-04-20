using System;
using System.Collections.Generic;
using System.Text;

// a generic maxHeap that can be used to shuffle around objects such that the highest value is at the front of an array
// with its children located at 2i + 2i+1
public class MaxHeap<T>
{
    private const bool debug = false;

    private List<T> ptrList = null; //the objects we're shuffling along with the numbers given, if any
    private double[] arr;
    private int sizeOfHeap; //sizeOfHeap corresponds to indices. 0th index is always null
    private T _prevObjRoot = default(T); //null for generics

    public List<T> objMaxMHeap { get => this.ptrList; }
    public double[] doubleMazHeap { get => this.arr; }
    public T prevObjRoot{ get => this._prevObjRoot;} //when we pop the head out
    public int heapSize { get => this.sizeOfHeap; }

    // Create a constructor  
    public MaxHeap( int size )
    {
        //We are adding size+1, because array index 0 will be blank.  
        this.arr = new double[size + 1];
        this.ptrList = new List<T>( new T[size + 1] );
        this.sizeOfHeap = 0;
    }

    public double peekTopOfHeap()
	{
        if (heapSize == 0)
            return 0;

        return this.arr[1]; //root is really at 1
	}

    //print heap size
    public int getHeapSize(bool print = false)
    {
        if( print )
            Console.WriteLine( "The size of the heap is:" + sizeOfHeap );

        return sizeOfHeap;
    }

    //returns if the heap is full
    public bool heapFull()
	{
        return (this.sizeOfHeap + 1) == this.ptrList.Count && this.arr.Length == (this.sizeOfHeap + 1);
	}

    // call this if we're also shuffling an object list
    public void swapObjs( int idx1, int idx2 )
	{
        T tmpObj = ptrList[idx1];
        ptrList[idx1] = ptrList[idx2];
        ptrList[idx2] = tmpObj;
    }

    //insert into tree
    public void InsertElementInHeap( double value, T obj )
    {

        if (sizeOfHeap < 0)
        {
            Console.WriteLine( "MAx Heap is empty" );
            return;
        }
        //Insertion of value inside the array happens at the last index of the  array, which is the heapSize. Should cover popping case
        arr[sizeOfHeap + 1] = value;
        ptrList[sizeOfHeap + 1] = obj; 
        sizeOfHeap++;
        HeapifyBottomToTop( sizeOfHeap );
        if (debug)
        {
            Console.WriteLine( "Inserted " + value + " successfully in Heap !" );
            levelOrder();
        }
    }

    //helper for inserting an element
    public void HeapifyBottomToTop( int index )
    {
        int parent = index / 2;

        // We are at root of the tree. Hence no more Heapifying is required.  
        if (index <= 1)
            return;

        // If Current value is greater than its parent, then we need to swap  
        if (arr[index] > arr[parent])
        {
            double tmp = arr[index];
            arr[index] = arr[parent];
            arr[parent] = tmp;

            //ditto for the objects we're shuffling
            if (ptrList != null)
                swapObjs( index, parent );
            //T tmpObj = ptrList[index];
            //ptrList[index] = ptrList[parent];
            //ptrList[parent] = tmpObj;
        }
        HeapifyBottomToTop( parent );
        return;
    }


    //Extract Head of Heap  
    public double extractHeadOfHeap()
    {
        if (sizeOfHeap == 0)
        {
            Console.WriteLine( "Heap is empty !" );
            return -1;
        }

        if (debug)
        {
            Console.WriteLine( "Head of the Heap is: " + arr[1] );
            Console.WriteLine( "Extracting it now..." );
        }

        double extractedValue = arr[1];
        _prevObjRoot = ptrList[1];
        arr[1] = arr[sizeOfHeap]; //Replacing with last element of the array  
        ptrList[1] = ptrList[sizeOfHeap]; //Replacing with last element of the array  

        arr[sizeOfHeap] = double.MinValue; 
        ptrList[sizeOfHeap] = default( T ); //just to highlight how we overwrote it... should never be reached since sizeOfHeap corresponds to indices
        sizeOfHeap--;
        HeapifyTopToBottom( 1 );

        if (debug)
        {
            Console.WriteLine( "Successfully extracted value from Heap." );
            levelOrder();
        }

        return extractedValue;
    }

    //helper for removing the topmost element
    public void HeapifyTopToBottom( int index )
    {
        int left = index * 2;
        int right = (index * 2) + 1;
        int largestChild = 0;

        //If there is no child of this node, then nothing to do. Just return.  
        if (sizeOfHeap < left)
        {
            return;
        }
        else if (sizeOfHeap == left)
        { //If there is only left child of this node, then do a comparison and return.  
            if (arr[index] < arr[left])
            {
                double tmp = arr[index];
                arr[index] = arr[left];
                arr[left] = tmp;

                //ditto for the objects we're shuffling
                if( ptrList != null)
                    swapObjs( index, left );

                //T tmpObj = ptrList[index];
                //ptrList[index] = ptrList[left];
                //ptrList[left] = tmpObj;
            }
            return;
        }
        else
        { //If both children are there, find smallest child
            if (arr[left] > arr[right])
                largestChild = left;
            else
                largestChild = right;

            if (arr[index] < arr[largestChild])
            { //If Parent is greater than smallest child, then swap  
                double tmp = arr[index];
                arr[index] = arr[largestChild];
                arr[largestChild] = tmp;

                //ditto for the objects we're shuffling
                if (ptrList != null)
                    swapObjs( index, largestChild );
                //T tmpObj = ptrList[index];
                //ptrList[index] = ptrList[largestChild];
                //ptrList[largestChild] = tmpObj;
            }
        }
        HeapifyTopToBottom( largestChild );
        return;
    }

    //debug message to print out levels of the heap
    public void levelOrder()
    {
        Console.WriteLine( "Printing all the elements of this Heap..." );// Printing from 1 because 0th cell is dummy  

        for (int i = 1; i <= sizeOfHeap; i++)
            Console.WriteLine( arr[i] + " " );

        Console.WriteLine( "\n" );
        return;
    }

    // free the whole heap so it's never used again
    public void deleteHeap()
	{
        if( debug )
            Console.WriteLine( "Deleting heap..." );

        this.sizeOfHeap = -1;
        this.ptrList = null;
        this.arr = null;
        this._prevObjRoot = default(T);
	}

    //testing for max heap
    public static void testHeap()
	{
        MaxHeap<int> intHeap = new MaxHeap<int>( 20 );
        for( int i = 0; i < 100; i++)
		{
            int maxInt = (int) intHeap.peekTopOfHeap();
            
            if ( intHeap.heapFull() && i > maxInt)
                maxInt = (int) intHeap.extractHeadOfHeap();
            if (!intHeap.heapFull())
                intHeap.InsertElementInHeap( i, i );
		}

        Console.WriteLine( "Final Heap :" );
        intHeap.levelOrder();
        Console.WriteLine( "Deleting heap..." );
        intHeap.deleteHeap();
	}
}

