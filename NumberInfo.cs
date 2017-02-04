using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ReadingMNISTDatabase
{
    class NumberInfo
    {
       public double[,] mean;
     public  double[,] varience;
     public double[] numoftypes;
       public  NumberInfo()
        {
            mean = new double[10, (28 * 28)];
            varience = new double[10, (28 * 28)];
            numoftypes = new double[10];

        }
    }
}
