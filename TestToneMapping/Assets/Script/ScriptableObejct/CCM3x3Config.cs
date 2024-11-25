using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;

[CreateAssetMenu(fileName = "CCM3x3Config", menuName = "ScriptableObjects/CCM3x3Config", order = 1)]
public class CCM3x3Config : ScriptableObject
{
    public double[] rowMajorData;
    
    public void LoadFromRowMajorArr(double[] arr)
    {
        rowMajorData = arr;
    }

    public Matrix<double> GetMatrix()
    {
        //MathNet have no OfRowMajor,so use columnMajor,then transpose
        Matrix<double> re = DenseMatrix.OfColumnMajor(3, 3, rowMajorData);
        return re.Transpose();
    }
}
