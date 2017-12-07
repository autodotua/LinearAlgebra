using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using MathNet.Numerics.LinearAlgebra.Factorization;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using System.Windows;
using System.Windows.Controls;
namespace 线性代数
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        #region 字段
        List<double[]> matrixList = new List<double[]>();//临时存放用的数组列表
        double[,] matrixA;//向量A
        double[,] matrixB;//向量B
        double[,] matrixResult;//向量运算结果
        double[] vector;//单行向量
        int rowCountA;//行数
        int columnCountA;//列数
        int rowCountB;//行数
        int columnCountB;//列数
        List<double[]> permutations = new List<double[]>();//排列数
        StringBuilder permutationsDetail = new StringBuilder();//排列数详情
        StringBuilder rowEchelonFormDetail = new StringBuilder();//行阶梯型详情
        StringBuilder reducedRowEchelonFormDetail = new StringBuilder();//行最简型详情
        StringBuilder determinantDetail = new StringBuilder();//行列式详情
        Configuration cfa = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);//配置文件
       // Matrix matrixInMathNet;//外部库
        Evd<double> evd;//外部库

        const int precision = 2;//精度：小数位数
        #endregion
        #region 初始化等
        public MainWindow()
        {
            InitializeComponent();
        }
        /// <summary>
        /// 初始化单行的向量
        /// </summary>
        private void InitializeVector()
        {
            vector = new double[columnCountA];
            for (int i = 0; i < columnCountA; i++)
            {
                vector[i] = matrixA[0, i];
            }
        }
        /// <summary>
        /// 初始化各字段和属性
        /// </summary>
        private void InitializeFields()
        {
            matrixList.Clear();
            permutationsDetail.Clear();
            rowEchelonFormDetail.Clear();
            reducedRowEchelonFormDetail.Clear();
            determinantDetail.Clear();
        }
        /// <summary>
        /// 初始化矩阵
        /// </summary>
        /// <param name="matrix"></param>
        /// <param name="txt"></param>
        /// <param name="rowCount"></param>
        /// <param name="columnCount"></param>
        /// <returns></returns>
        private MatrixType InitializeMatrix(ref double[,] matrix, ref TextBox txt, ref int rowCount, ref int columnCount)
        {
            if (txt.Text == "")
            {
                return MatrixType.Error;
            }
            try
            {
                int index = -1;
                //录入
                for (int line = 0; line < txt.LineCount; line++)
                {
                    if (txt.GetLineText(line)==Environment.NewLine|| txt.GetLineText(line) == "")
                    {
                        continue;
                    }
                    index++;
                    string[] temp/*行字符串数组*/ = txt.GetLineText(line).Split(new string[] { " ", "  ", "   ", "    ", "     ", Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
                    matrixList.Add(new double[temp.Length]);
                    int j = 0;
                    foreach (var str in temp)
                    {
                        matrixList[index][j] = double.Parse(temp[j]);
                        //因为要把分割出来的字符串转换为数字，所以只好这么来了
                        j++;
                    }
                }
               // matrixList.Remove(new double[0]);//删除空行
                columnCount = matrixList[0].Length;//这是临时的，因为还没有判断是不是矩阵，暂时指定第一行的个数位列数，供后面判断
                rowCount = matrixList.Count;
                matrix = new double[rowCount, columnCount];


                if (!MatrixListToMatrixList(ref matrix, ref columnCount))
                {
                    return MatrixType.Error;
                }
                if (columnCount == rowCount)
                {
                    if (columnCount == 1 && rowCount == 1)
                    {
                        txtType.Text = "1阶方阵";
                        return MatrixType.Error;
                    }
                    txtType.Text = columnCount + "阶方阵";
                    return MatrixType.SquareMatrix;
                }
                else
                {

                    if (columnCount == 1)
                    {
                        txtType.Text = $"{rowCount}×{columnCount}列矩阵";
                        return MatrixType.ColumnMatrix;
                    }
                    else if (rowCount == 1)
                    {
                        bool isPermutation = true;
                        InitializeVector();
                        for (int i = 0; i < columnCount; i++)
                        {
                            if (Array.IndexOf(vector, i + 1) == -1)
                            {
                                isPermutation = false;
                                break;
                            }

                        }
                        if (isPermutation)
                        {
                            txtType.Text = $"{rowCount}×{columnCount}行矩阵；排列";
                            return MatrixType.Permutation;
                        }
                        else
                        {
                            txtType.Text = $"{rowCount}×{columnCount}行矩阵";
                            return MatrixType.RowMatrix;
                        }

                    }
                    else
                    {
                        txtType.Text = $"{rowCount}×{columnCount}矩阵";
                        return MatrixType.Normal;
                    }
                }
            }
            catch
            {
                return MatrixType.Error;
            }
            finally
            {

                matrixList.Clear();
            }


        }
        /// <summary>
        /// 当源矩阵改变时
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TxtSourceMatrixTextChangedEventHandler(object sender, TextChangedEventArgs e)
        {
            MatrixProperties();
        }
        #endregion
        #region 矩阵性质
        /// <summary>
        /// 矩阵属性
        /// </summary>
        private void MatrixProperties()
        {
            InitializeFields();
            MatrixType type = InitializeMatrix(ref matrixA, ref txtSourceMatrixA, ref rowCountA, ref columnCountA);//相当于初始化
            //if (InitializeMatrix(ref matrixB, ref txtSourceMatrixB, ref rowCountB, ref columnCountB) != MatrixType.Error)
            //{
            //    if(matrixA.GetLength(0)==matrixB.GetLength(0) && matrixA.GetLength(1) == matrixB.GetLength(1))
            //    {

            //    }
            //}
                if (type == MatrixType.Error)
            {
                gbxCalculate.IsEnabled = false;
                gbxTransformation.IsEnabled = false;
                gbxProperty.IsEnabled = false;
                return;
            }
            Matrix matrixInMathNet = DenseMatrix.OfArray(matrixA);
            gbxCalculate.IsEnabled = true;
            gbxTransformation.IsEnabled = true;
            gbxProperty.IsEnabled = true;
            btnAdjointMatrix.IsEnabled = false;
            btnInverse.IsEnabled = false;
            btnPermutations.IsEnabled = false;
            txtDeterminant.IsEnabled = false;
            btnEigen.IsEnabled = false;
            txtPEigen.IsEnabled = false;
            btnInverse.IsEnabled = false;
            if (type == MatrixType.SquareMatrix)
            {
                txtDeterminant.IsEnabled = true;
                btnPermutations.IsEnabled = true;
                btnAdjointMatrix.IsEnabled = true;
                btnEigen.IsEnabled = true;
                txtPEigen.IsEnabled = true;
                evd = matrixInMathNet.Evd();
                CalculateEigen();
                if (CalculateDeterminant()!=0)
                {
                    btnInverse.IsEnabled = true;
                }
            }
            if (type == MatrixType.Permutation)
            {
                txtPermutations.IsEnabled = true;
                GetAllPermutations(0, columnCountA - 1);
                decimal n = 1;
                for (int i = 1; i <= columnCountA; i++)
                {
                    n *= i;
                }
                txtPermutations.Text = "共有" + n.ToString() + "个全排列";
            }
            else
            {
                txtPermutations.IsEnabled = false;
            }
            //由于代码移植的缘故，方法执行后源矩阵会改变，所以需要先备份。
            double[,] rawMatrix = matrixA.Clone() as double[,];
            Transpose();
            matrixA = rawMatrix.Clone() as double[,];

            RowEchelonForm();

            ReducedRowEchelonForm();
            txtRank.Text = GetRankOfMatrix().ToString();
        }
        /// <summary>
        /// 获取每一列主元前的0的数量
        /// </summary>
        /// <returns></returns>
        private List<int> GetZeroCountBeforePivot()
        {
            List<int> zeroCountBeforePivot = new List<int>();
            for (int i = 0; i < rowCountA; i++)
            {

                int zeroCount = 0;//当前已知的0的数量
                bool isNoteZeroAnyMore = false;//是否已经不是0了，目的是跳出一层循环而不用goto。
                for (int j = 0; j < columnCountA && isNoteZeroAnyMore == false; j++)
                {
                    if (matrixA[i, j] == 0)
                    {
                        zeroCount++;
                    }
                    else
                    {
                        isNoteZeroAnyMore = true;
                    }
                }
                if (zeroCount == columnCountA)
                {
                    zeroCountBeforePivot.Add(-1);
                }
                else
                {
                    zeroCountBeforePivot.Add(zeroCount);//索引就是行号，值是0的数量
                }

            }
            return zeroCountBeforePivot;
        }
        /// <summary>
        /// 获取某一行主元前0的个数，同时也是主元的序号
        /// </summary>
        /// <param name="rowIndex"></param>
        /// <returns></returns>
        private int GetZeroCountBeforePivot(int rowIndex)
        {
            //    int i = rowIndex;
            //    int zeroCount = 0;//当前已知的0的数量
            //    bool isNoteZeroAnyMore = false;//是否已经不是0了，目的是跳出一层循环而不用goto。
            //    for (int j = 0; j < columnCountA && isNoteZeroAnyMore == false; j++)
            //    {
            //        if (matrix[i, j] == 0)
            //        {
            //            zeroCount++;
            //        }
            //        else
            //        {
            //            isNoteZeroAnyMore = true;
            //        }
            //    }
            //    //zeroCountBeforePivot.Add(zeroCount);//索引就是行号，值是0的数量
            //    if (zeroCount == columnCountA)
            //    {
            //        return -1;
            //    }
            //    else
            //    {
            //        return zeroCount;
            //   }
            return GetZeroCountBeforePivot()[rowIndex];
        }
        /// <summary>
        /// 获取靠左的全0列的数量
        /// </summary>
        /// <returns></returns>
        private int GetFirstColumnWithZero()
        {
            int firstColumnWhthZero = 0;
            for (int i = 0; i < columnCountA; i++)
            {
                for (int j = 0; j < rowCountA; j++)
                {
                    if (matrixA[j, i] == 0)
                    {
                        firstColumnWhthZero = i + 1;
                    }
                    else
                    {
                        break;
                    }
                }

            }
            return firstColumnWhthZero;

        }
        /// <summary>
        /// 根据主元前的0的数量进行排序
        /// </summary>
        /// <returns></returns>
        private bool SortByZeroCountBeforePivot()
        {

            List<int> zeroCountBeforePivot = GetZeroCountBeforePivot();

            double[,] tempMatrix = new double[rowCountA, columnCountA];
            int rowIndex = 0;
            for (int i = 0; i < columnCountA; i++)//从最后往前循环，就是假设一排全是0、然后只有最后一个不是0、……
            {
                for (int j = 0; j < zeroCountBeforePivot.Count; j++)//循环一遍0的个数，如果匹配就加入matrix的下一列。这个j同时也是行的索引
                {
                    if (zeroCountBeforePivot[j] == i)//如果0的数量符合
                    {
                        for (int k = 0; k < columnCountA; k++)////循环第二维来赋值数组
                        {
                            tempMatrix[rowIndex, k] = matrixA[j, k];//将原矩阵的值复制给临时矩阵，实际上是复制整行
                        }
                        rowIndex++;
                    }
                }
            }
            if (matrixA.Equals(tempMatrix))
            {
                Debug.WriteLine("无需排序");
                return false;
            }
            else
            {
                matrixA = tempMatrix;
                return true;
            }
        }
        /// <summary>
        /// 行阶梯形
        /// </summary>
        private void RowEchelonForm()
        {
            for (int rawRow = 0; rawRow < rowCountA - 1; rawRow++)
            {
                if (SortByZeroCountBeforePivot())
                {
                    //PrintMatrix(matrixA,);
                }
                for (int targetRow = rawRow + 1; targetRow < rowCountA; targetRow++)
                {
                    if (GetZeroCountBeforePivot(targetRow) == GetZeroCountBeforePivot(rawRow))
                    {
                        int zeroColumnCount = GetFirstColumnWithZero();
                        int zeroCountBeforePivotOfRawRow = GetZeroCountBeforePivot(rawRow);
                        if (zeroCountBeforePivotOfRawRow != -1)
                        {
                            double multiple = matrixA[targetRow, zeroCountBeforePivotOfRawRow] / matrixA[rawRow, zeroCountBeforePivotOfRawRow];//目标行和用来当加数的行的主元之比
                            if (multiple.ToString() == "NaN")
                            {
                                //Debug.WriteLine(matrix[targetRow, zeroColumnCount] + "/" + matrix[rawRow, zeroColumnCount]);
                            }
                            else
                            {
                                for (int i = zeroCountBeforePivotOfRawRow; i < columnCountA; i++)//给目标行的每一个元素加上源行的multiple倍
                                {
                                    matrixA[targetRow, i] = matrixA[targetRow, i] - multiple * matrixA[rawRow, i];
                                }
                                rowEchelonFormDetail.Append('r' + (targetRow + 1).ToString() + ("-" + multiple.ToString()).Replace("--", "+") + "*r" + (rawRow + 1).ToString() + Environment.NewLine + Environment.NewLine);
                                PrintMatrix(rowEchelonFormDetail);
                            }
                        }

                        //Console.WriteLine();
                        //Console.WriteLine();
                        //Console.WriteLine("r{0}{1}{2}r{3}：", targetRow + 1, multiple > 0 ? "+" : "", multiple, rawRow + 1);
                        //Console.WriteLine();
                    }
                }
            }
            PrintMatrix(matrixA, txtRowEchelonForm);
        }
        /// <summary>
        /// 从临时的矩阵列表到二维向量
        /// </summary>
        /// <returns></returns>
        private bool MatrixListToMatrixList(ref double[,] matrix, ref int columnCount)
        {
            int rowIndex = 0;
            foreach (var array in matrixList)//循环每一个向量
            {

                if (array.Length != columnCount)//如果遇到和第一行个数不匹配的，就说明不是矩阵
                {
                    txtType.Text = "非矩阵";
                    return false;
                }

                for (int columIndex = 0; columIndex < columnCount; columIndex++)
                {
                    matrix[rowIndex, columIndex] = array[columIndex];
                }
                rowIndex++;
            }
            return true;
        }
        /// <summary>
        /// 把当前矩阵显示在指定的文本框中
        /// </summary>
        /// <param name="txt"></param>
        private void PrintMatrix(double[,] matrix, TextBox txt)
        {
            txt.Clear();
            StringBuilder sb = new StringBuilder();
            int i = 0;//行
            int j = 0;//列
            for (; i < rowCountA - 1; i++)
            {
                j = 0;
                sb.Append(Math.Round(matrix[i, j++], precision));//第一列前面不输出制表符
                for (; j < columnCountA; j++)
                {
                    sb.Append("\t" + Math.Round(matrix[i, j], precision));
                }
                sb.Append(Environment.NewLine);
            }
            //最后一行单独来
            j = 0;
            sb.Append(Math.Round(matrix[i, j++], precision));
            for (; j < columnCountA; j++)
            {
                sb.Append("\t" + Math.Round(matrix[i, j], precision));
            }
            txt.Text = sb.ToString();
        }
        /// <summary>
        /// 把当前矩阵追加到一个字符串变量中
        /// </summary>
        /// <param name="txt"></param>
        private void PrintMatrix(StringBuilder txt)
        {
            int i = 0;//行
            int j = 0;//列
            for (; i < rowCountA; i++)
            {
                j = 0;
                txt.Append(Math.Round(matrixA[i, j++], precision));//第一列前面不输出制表符
                for (; j < columnCountA; j++)
                {
                    txt.Append("\t" + Math.Round(matrixA[i, j], precision));
                }
                txt.Append(Environment.NewLine);
            }
        }
        /// <summary>
        /// 获取矩阵的秩
        /// </summary>
        /// <returns></returns>
        private int GetRankOfMatrix()
        {
            int rank = 0;
            for (int i = rowCountA - 1; i >= 0; i--)//从下到上循环行
            {
                bool isAllZero = true;
                for (int j = 0; j < columnCountA; j++)//循环列
                {
                    if (matrixA[i, j] != 0)//如果有一个不是0
                    {
                        isAllZero = false;
                        break;
                    }
                }
                if (isAllZero)
                {
                    rank++;
                }
            }
            return rowCountA - rank;
        }
        /// <summary>
        /// 行最简形
        /// </summary>
        private void ReducedRowEchelonForm()
        {
            if (!IfIsARowEchelonForm())
            {
                RowEchelonForm();
            }

            TurnPivotToOne();

            //List<int> pivotPosition = GetZeroCountBeforePivot();
            for (int rawRowIndex = rowCountA - 1; rawRowIndex >= 0; rawRowIndex--)
            {
                //if (!pivotPosition.Contains(rawRowIndex))
                //{
                //    continue;
                //}
                int columnIndexOfPivot = GetZeroCountBeforePivot(rawRowIndex);
                if (columnIndexOfPivot == -1)
                {
                    continue;
                }
                //Console.WriteLine();
                //Console.WriteLine();
                bool changed = false;
                for (int targetRowIndex = 0; targetRowIndex < rawRowIndex; targetRowIndex++)
                {
                    if (matrixA[targetRowIndex, columnIndexOfPivot] != 0)
                    {
                        changed = true;
                        double multiple = matrixA[targetRowIndex, columnIndexOfPivot];
                        for (int columnIndex = 0; columnIndex < columnCountA; columnIndex++)
                        {
                            matrixA[targetRowIndex, columnIndex] -= multiple * matrixA[rawRowIndex, columnIndex];
                        }
                        //Console.WriteLine(("r" + (targetRowIndex + 1) + "-" + multiple + "*r" + (rawRowIndex + 1)).Replace("--", "+"));
                        reducedRowEchelonFormDetail.Append(("r" + (targetRowIndex + 1) + "-" + multiple + "*r" + (rawRowIndex + 1)).Replace("--", "+") + Environment.NewLine + Environment.NewLine);
                    }
                }
                if (changed)
                {
                    //Console.WriteLine();
                    //PrintMatrix(matrixA,);
                    PrintMatrix(reducedRowEchelonFormDetail);
                }
            }
            PrintMatrix(matrixA, txtReducedRowEchelonForm);
        }
        /// <summary>
        /// 把每一行的主元变成1，并且后面的按相应倍数转换
        /// </summary>
        private void TurnPivotToOne()
        {
            //Console.WriteLine();
            //Console.WriteLine();
            for (int i = 0; i < rowCountA; i++)//循环每一行
            {
                if (GetZeroCountBeforePivot(i) == -1)
                {
                    continue;
                }
                double multiple = matrixA[i, GetZeroCountBeforePivot(i)];
                if (multiple != 1)//如果主元不是1
                {
                    for (int j = GetZeroCountBeforePivot(i); j < columnCountA; j++)//循环从主元开始的每一列
                    {
                        matrixA[i, j] /= multiple;
                    }

                    //Console.WriteLine("r{0}/{1}：", i + 1, multiple);

                }
            }
            //Console.WriteLine();
            // PrintMatrix(matrixA,);
        }
        /// <summary>
        /// 判断是否已经是行阶梯式了
        /// </summary>
        /// <returns></returns>
        private bool IfIsARowEchelonForm()
        {
            List<int> zeroCountBeforePivot = GetZeroCountBeforePivot();
            int lastRowZeroCount = -1;
            foreach (var i in zeroCountBeforePivot)
            {
                if (i <= lastRowZeroCount)
                {
                    return false;
                }
                lastRowZeroCount = i;
            }
            return true;

        }
        /// <summary>
        /// 矩阵转置
        /// </summary>
        private void Transpose()
        {
            double[,] tempMatrix = new double[columnCountA, rowCountA];
            for (int i = 0; i < rowCountA; i++)
            {
                for (int j = 0; j < columnCountA; j++)
                {
                    tempMatrix[j, i] = matrixA[i, j];
                }
            }
            matrixA = tempMatrix.Clone() as double[,];
            //矩阵转置后，行列数量发生改变，需要重置一下
            SwapRowAndColumn();
            PrintMatrix(matrixA, txtTranspose);
            SwapRowAndColumn();


        }
        /// <summary>
        /// 交换行列数量，但是并不改变矩阵
        /// </summary>
        private void SwapRowAndColumn()
        {
            int temp = columnCountA;
            columnCountA = rowCountA;
            rowCountA = temp;
        }
        /// <summary>
        /// 计算行列式
        /// </summary>
        private double CalculateDeterminant()
        {
            permutations.Clear();
            vector = new double[columnCountA];
            for (int i = 0; i < columnCountA; i++)
            {
                vector[i] = i;
            }
            GetAllPermutations(0, columnCountA- 1);
            double sum = 0;
            determinantDetail.Append("sum" + Environment.NewLine + "{" + Environment.NewLine);
            foreach (var i in permutations)
            {
                double temp = matrixA[0, (int)i[0]];
                double product = temp;
                determinantDetail.Append(temp);
                for (int j = 1; j < rowCountA; j++)
                {
                    temp = matrixA[j, (int)i[j]];
                    product *= temp;
                    determinantDetail.Append("*"+temp);
                }
                if (!GetParityOfAPermutation(i, columnCountA))
                {
                    product *= -1;
                }
                else
                {
                    determinantDetail.Append("+");
                }
                determinantDetail.Append("(=");
                determinantDetail.Append(product);
                determinantDetail.Append(")");
                determinantDetail.Append(Environment.NewLine);
                sum += product;
                // rowIndex++;
            }
            determinantDetail.Append("}="+sum);
            txtDeterminant.Text = sum.ToString();
            return sum;
        }
        private double CalculateDeterminant(double[,] matrix)
        {
            permutations.Clear();
            int rowCount = matrix.GetLength(0);
            int columnCount = matrix.GetLength(1);
            vector = new double[columnCount];
            for (int i = 0; i < columnCount; i++)
            {
                vector[i] = i;
            }
            GetAllPermutations(0, columnCount - 1);
            //File.WriteAllText("test.txt", sb.ToString());
            //int rowIndex = 0;
            double sum = 0;
            //Console.WriteLine("=");
            foreach (var i in permutations)
            {
                double temp = matrix[0, (int)i[0]];
                double product = temp;
                //Console.Write(temp);
                for (int j = 1; j < rowCount; j++)
                {
                    temp = matrix[j, (int)i[j]];
                    product *= temp;
                    // Console.Write("*"+temp);
                    // Debug.WriteLine(matrix[j, (int)i[j]]);
                }
                if (!GetParityOfAPermutation(i,columnCount))
                {
                    product *= -1;
                }
                else
                {
                    //Console.Write("+");
                }
                //Console.Write(product);
                sum += product;
                // rowIndex++;
            }
            //Console.WriteLine();
            //Console.WriteLine($"={count}");
            return sum;
        }
        /// <summary>
        /// 获取全排列
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        private void GetAllPermutations(int start, int end)
        {

            
            if (start == end)
            {
                foreach (var i in vector)
                {
                    permutationsDetail.Append(i + " ");
                }
                permutationsDetail.Append(Environment.NewLine);
                permutations.Add(vector.Clone() as double[]);
                //PrintOneDimensionalMatrix();
                // printList();

            }
            else
            {
                for (int i = start; i <= end; i++)
                {
                    Swap(ref vector[start], ref vector[i]);
                    GetAllPermutations(start + 1, end);//递归
                    Swap(ref vector[start], ref vector[i]);//数组一定要复原
                }
            }
        }
        /// <summary>
        /// 判断奇偶性
        /// </summary>
        private bool GetParityOfAPermutation()
        {
            int count = 0;
            //permutations.Clear();
            //GetAllPermutations(0, columnCountA - 1, false);
            for (int leftIndex = 0; leftIndex < columnCountA - 1; leftIndex++)//左边的数
            {
                for (int rightIndex = leftIndex + 1; rightIndex < columnCountA; rightIndex++)//右边的数
                {
                    if (vector[leftIndex] > vector[rightIndex])//如果左边大于右边
                    {
                        count++;
                        for (int i = 0; i < columnCountA; i++)//循环每一个数列中的每一个数
                        {
                            if (i == leftIndex || i == rightIndex)//如果就是目标的两个数字的一个，红色字体输出
                            {
                                Console.ForegroundColor = ConsoleColor.Red;
                                Console.Write("{0,4}", vector[i]);
                                Console.ForegroundColor = ConsoleColor.White;
                            }
                            else
                            {
                                Console.Write("{0,4}", vector[i]);
                            }
                        }
                        Console.WriteLine();
                    }
                }
            }
            Console.WriteLine();
            Console.WriteLine("共有{0}个逆序数，是{1}", count, count % 2 == 0 ? "偶排列" : "奇排列");
            return count % 2 == 0 ? true : false;
        }
        /// <summary>
        /// 指定排列判断奇偶性
        /// </summary>
        private bool GetParityOfAPermutation(double[] permutations)
        {
            return GetParityOfAPermutation(permutations, columnCountA);
        }
        /// <summary>
        /// 指定排列判断奇偶性
        /// </summary>
        /// <param name="permutations"></param>
        /// <param name="columnCount"></param>
        /// <returns></returns>
        private bool GetParityOfAPermutation(double[] permutations,int columnCount)
        {
            int count = 0;
            for (int leftIndex = 0; leftIndex < columnCount - 1; leftIndex++)//左边的数
            {
                for (int rightIndex = leftIndex + 1; rightIndex < columnCount; rightIndex++)//右边的数
                {
                    if (permutations[leftIndex] > permutations[rightIndex])//如果左边大于右边
                    {
                        count++;
                    }
                }
            }
            return count % 2 == 0 ? true : false;
        }
        /// <summary>
        /// 计算矩阵的特征值、特征向量
        /// </summary>
        private void CalculateEigen()
        {
            var eigenValues = evd.EigenValues;
            StringBuilder result = new StringBuilder();
            foreach (var i in eigenValues)
            {
                if(i.Imaginary==0)
                {
                    result.Append(Math.Round( i.Real, precision )+ ",");
                }
                else if(i.Real==0)
                {
                    result.Append(Math.Round(i.Imaginary, precision) + ",");
                }
                else
                {
                    result.Append(Math.Round(i.Real, precision) + "+"+ Math.Round(i.Imaginary, precision) + "i,");
                }
            }
            result.Remove(result.Length - 1, 1);
            txtPEigen.Text = result.Replace("+-","-").ToString();
        }

        /// <summary>
        /// 交换参数里的两个数
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        private void Swap(ref double a, ref double b)
        {
            double c = a;
            a = b;
            b = c;
        }
        #endregion

        #region 矩阵属性详情按钮点击事件
        /// <summary>
        /// 单击显示全排列详情按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnShowPermutationsDetailClickEventHandler(object sender, RoutedEventArgs e)
        {
            new ShowDetailInfo(permutationsDetail).Show();
        }
        /// <summary>
        /// 单击行阶梯形按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnRowEchelonFormClickEventHandler(object sender, RoutedEventArgs e)
        {
            new ShowDetailInfo(rowEchelonFormDetail).Show();
        }
        /// <summary>
        /// 单击行最简形按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnReducedRowEchelonFormClickEventHandler(object sender, RoutedEventArgs e)
        {
            new ShowDetailInfo(reducedRowEchelonFormDetail).Show();
        }
        /// <summary>
        /// 单击矩阵特征详情按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnEigenClickEventHandler(object sender, RoutedEventArgs e)
        {
            var formatProvider = (CultureInfo)CultureInfo.InvariantCulture.Clone();
            new ShowDetailInfo(evd.EigenVectors.ToString("#0."+new string('0',precision)+"\t", formatProvider)).Show();

        }
        /// <summary>
        /// 单击显示行列式详情按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnDeterminantClickEventHandler(object sender, RoutedEventArgs e)
        {
            new ShowDetailInfo(determinantDetail).Show();
        }
        #endregion
        #region 矩阵运算

        /// <summary>
        /// 单击矩阵加法按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnMatrixAddClickEventHandler(object sender, RoutedEventArgs e)
        {
            if (InitializeMatrix(ref matrixB, ref txtSourceMatrixB, ref rowCountB, ref columnCountB) != MatrixType.Error
                && InitializeMatrix(ref matrixA, ref txtSourceMatrixA, ref rowCountA, ref columnCountA) != MatrixType.Error)
            {
                if (rowCountA == rowCountB && columnCountA == columnCountB)
                {
                    matrixResult = matrixA.Clone() as double[,];
                    for (int rowIndex = 0; rowIndex < rowCountA; rowIndex++)
                    {
                        for (int columnIndex = 0; columnIndex < columnCountA; columnIndex++)
                        {
                            matrixResult[rowIndex, columnIndex] += matrixB[rowIndex, columnIndex];
                        }
                    }
                }
                AfterCalculate("A+B");
            }
            else
            {
                MessageBox.Show("不符合相加条件", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        /// <summary>
        /// 单击矩阵减法按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnMatrixSubClickEventHandler(object sender, RoutedEventArgs e)
        {
            if (InitializeMatrix(ref matrixB, ref txtSourceMatrixB, ref rowCountB, ref columnCountB) != MatrixType.Error
               && InitializeMatrix(ref matrixA, ref txtSourceMatrixA, ref rowCountA, ref columnCountA) != MatrixType.Error)
            {
                if (rowCountA == rowCountB && columnCountA == columnCountB)
                {
                    matrixResult = matrixA.Clone() as double[,];
                    for (int rowIndex = 0; rowIndex < rowCountA; rowIndex++)
                    {
                        for (int columnIndex = 0; columnIndex < columnCountA; columnIndex++)
                        {
                            matrixResult[rowIndex, columnIndex] -= matrixB[rowIndex, columnIndex];
                        }
                    }
                }
                AfterCalculate("A-B");
            }
            else
            {
                MessageBox.Show("不符合相减条件", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        /// <summary>
        /// 单击矩阵乘法按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnMatrixMultiClickEventHandler(object sender, RoutedEventArgs e)
        {

            if (InitializeMatrix(ref matrixB, ref txtSourceMatrixB, ref rowCountB, ref columnCountB) != MatrixType.Error
               && InitializeMatrix(ref matrixA, ref txtSourceMatrixA, ref rowCountA, ref columnCountA) != MatrixType.Error)
            {
                if ((sender as Button).Name == "btnBMultiA")
                {
                    var temp = matrixA;
                    matrixA = matrixB;
                    matrixB = temp;
                }
                if (rowCountB == columnCountA)
                {
                    matrixResult = new double[rowCountA, columnCountB];
                    //循环A的行
                    for (int ARowIndex = 0; ARowIndex < rowCountA; ARowIndex++)
                    {
                        //循环B的 列
                        for (int BColumnIndex = 0; BColumnIndex < columnCountB; BColumnIndex++)
                        {
                            double sum = 0;
                            //循环A列，同时也是B行
                            for (int AColumnBRowIndex = 0; AColumnBRowIndex < columnCountA; AColumnBRowIndex++)
                            {
                                sum += matrixA[ARowIndex, AColumnBRowIndex] * matrixB[AColumnBRowIndex, BColumnIndex];
                            }
                            matrixResult[ARowIndex, BColumnIndex] = sum;
                        }
                    }
                }
                AfterCalculate((sender as Button).Name == "btnBMultiA" ? "BA" : "AB");
            }
            else
            {
                MessageBox.Show("不符合相乘条件", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        /// <summary>
        /// 单击矩阵数乘按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnMultiClickEventHandler(object sender, RoutedEventArgs e)
        {
            try
            {
                if (InitializeMatrix(ref matrixA, ref txtSourceMatrixA, ref rowCountA, ref columnCountA) != MatrixType.Error)
                {
                    matrixResult = new double[rowCountA, columnCountA];
                    double multiple = double.Parse(txtMultiMultiple.Text);
                    for (int rowIndex = 0; rowIndex < rowCountA; rowIndex++)
                    {
                        for (int columnIndex = 0; columnIndex < columnCountA; columnIndex++)
                        {
                            matrixResult[rowIndex, columnIndex] = multiple * matrixA[rowIndex, columnIndex];
                        }
                    }
                    AfterCalculate(multiple.ToString() + "A");
                }
                else
                {
                    MessageBox.Show("不符合相乘条件", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (IndexOutOfRangeException)
            {
                MessageBox.Show("下标越界", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// 单击伴随矩阵按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnAdjointMatrixClickEventHandler(object sender, RoutedEventArgs e)
        {
            GetAdjointMatrix();
            AfterCalculate("A*");
        }
        /// <summary>
        /// 单击逆矩阵按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnInverseClickEventHandler(object sender, RoutedEventArgs e)
        {
            GetAdjointMatrix();
            double multiple = CalculateDeterminant(matrixA);
            for (int rowIndex = 0; rowIndex < rowCountA; rowIndex++)
            {
                for (int columnIndex = 0; columnIndex < columnCountA; columnIndex++)
                {
                    matrixResult[rowIndex, columnIndex] = matrixResult[rowIndex, columnIndex] / multiple;
                }
            }
            AfterCalculate("A^-1");
        }
        private void GetAdjointMatrix()
        {

            try
            {
                if (InitializeMatrix(ref matrixA, ref txtSourceMatrixA, ref rowCountA, ref columnCountA) != MatrixType.Error)
                {
                    matrixResult = new double[rowCountA, columnCountA];
                    //循环行
                    for (int outRowIndex = 0; outRowIndex < rowCountA; outRowIndex++)
                    {
                        //循环列
                        for (int outColumnIndex = 0; outColumnIndex < columnCountA; outColumnIndex++)
                        {
                            double[,] tempMatrix = new double[rowCountA - 1, columnCountA - 1];
                            int afterRow = 0;
                            //循环余子式的行
                            for (int inRowIndex = 0; inRowIndex < rowCountA; inRowIndex++)
                            {
                                if (inRowIndex == outRowIndex)
                                {
                                    if (++inRowIndex == rowCountA)
                                    {
                                        continue;
                                    }
                                    afterRow = 1;
                                }
                                int afterColumn = 0;
                                //循环余子式的列
                                for (int inColumnIndex = 0; inColumnIndex < columnCountA; inColumnIndex++)
                                {
                                    if (inColumnIndex == outColumnIndex)
                                    {
                                        if (++inColumnIndex == rowCountA)
                                        {
                                            continue;
                                        }
                                        afterColumn = 1;
                                    }
                                    tempMatrix[inRowIndex - afterRow, inColumnIndex - afterColumn] = matrixA[inRowIndex, inColumnIndex];
                                }
                            }
                            matrixResult[outColumnIndex, outRowIndex] = (((outRowIndex + outColumnIndex) % 2 == 0) ? 1 : -1) * CalculateDeterminant(tempMatrix);

                        }
                    }
                }
                else
                {
                    MessageBox.Show("不符合相乘条件", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (IndexOutOfRangeException)
            {
                MessageBox.Show("下标越界", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// 处理矩阵计算后的事
        /// </summary>
        /// <param name="text"></param>
        private void AfterCalculate(string text)
        {
            PrintMatrix(matrixResult, txtCalculateResult);
            tbkCalculateResult.Text = text;
        }

        #endregion
        #region 初等行变换
        /// <summary>
        /// 单击换行按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnRowSwitchingClickEventHandler(object sender, RoutedEventArgs e)
        {
            try
            {
                int row1 = int.Parse(txtRowSwitchingRow1.Text) - 1;
                int row2 = int.Parse(txtRowSwitchingRow2.Text) - 1;
                InitializeMatrix(ref matrixA, ref txtSourceMatrixA, ref rowCountA, ref columnCountA);
                //matrixResult = new double[rowCountA, columnCountA];
                matrixResult = matrixA.Clone() as double[,];
                for (int columnIndex = 0; columnIndex < columnCountA; columnIndex++)
                {
                    matrixResult[row1, columnIndex] = matrixA[row2, columnIndex];
                }
                for (int columnIndex = 0; columnIndex < columnCountA; columnIndex++)
                {
                    matrixResult[row2, columnIndex] = matrixA[row1, columnIndex];
                }
                AfterTransform("r" + (row1 + 1).ToString() + "←→r" + (row2 + 1).ToString());
            }


            catch (IndexOutOfRangeException)
            {
                MessageBox.Show("下标越界", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        /// <summary>
        /// 单击乘行按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnRowMultiplicationClickEventHandler(object sender, RoutedEventArgs e)
        {
            try
            {
                int multiple = int.Parse(txtRowMultiplicationMultiple.Text);
                int row = int.Parse(txtRowMultiplicationRow.Text) - 1;
                InitializeMatrix(ref matrixA, ref txtSourceMatrixA, ref rowCountA, ref columnCountA);
                matrixResult = matrixA.Clone() as double[,];
                for (int columnIndex = 0; columnIndex < columnCountA; columnIndex++)
                {
                    matrixResult[row, columnIndex] = multiple * matrixA[row, columnIndex];
                }
                AfterTransform(multiple.ToString() + "r" + (row + 1).ToString());
            }
            catch (IndexOutOfRangeException)
            {
                MessageBox.Show("下标越界", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        /// <summary>
        /// 单击加行按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnRowAdditionClickEventHandler(object sender, RoutedEventArgs e)
        {
            try
            {
                int multiple = int.Parse(txtRowAdditionMultiple.Text);
                int row1 = int.Parse(txtRowAdditionRow1.Text) - 1;
                int row2 = int.Parse(txtRowAdditionRow2.Text) - 1;
                InitializeMatrix(ref matrixA, ref txtSourceMatrixA, ref rowCountA, ref columnCountA);
                matrixResult = matrixA.Clone() as double[,];
                for (int columnIndex = 0; columnIndex < columnCountA; columnIndex++)
                {
                    matrixResult[row1, columnIndex] += multiple * matrixA[row2, columnIndex];
                }
                AfterTransform(("r" + (row1 + 1).ToString() + "+" + multiple + "r" + (row2 + 1).ToString()).Replace("+-", "-"));
            }
            catch (IndexOutOfRangeException)
            {
                MessageBox.Show("下标越界", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        /// <summary>
        /// 单击换列按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnColumnSwitchingClickEventHandler(object sender, RoutedEventArgs e)
        {
            try
            {
                int column1 = int.Parse(txtColumnSwitchingColumn1.Text) - 1;
                int column2 = int.Parse(txtColumnSwitchingColumn2.Text) - 1;
                InitializeMatrix(ref matrixA, ref txtSourceMatrixA, ref rowCountA, ref columnCountA);
                //matrixResult = new double[rowCountA, columnCountA];
                matrixResult = matrixA.Clone() as double[,];
                for (int rowIndex = 0; rowIndex < rowCountA; rowIndex++)
                {
                    matrixResult[rowIndex, column1] = matrixA[rowIndex, column2];
                }
                for (int rowIndex = 0; rowIndex < rowCountA; rowIndex++)
                {
                    matrixResult[rowIndex, column2] = matrixA[rowIndex, column1];
                }
                AfterTransform("c" + (column1 + 1).ToString() + "←→c" + (column2 + 1).ToString());
            }
            catch (IndexOutOfRangeException)
            {
                MessageBox.Show("下标越界", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        /// <summary>
        /// 单击乘列按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnColumnMultiplicationClickEventHandler(object sender, RoutedEventArgs e)
        {
            try
            {
                int multiple = int.Parse(txtColumnMultiplicationMultiple.Text);
                int column = int.Parse(txtColumnMultiplicationColumn.Text) - 1;
                InitializeMatrix(ref matrixA, ref txtSourceMatrixA, ref rowCountA, ref columnCountA);
                matrixResult = matrixA.Clone() as double[,];
                for (int rowIndex = 0; rowIndex < columnCountA; rowIndex++)
                {
                    matrixResult[rowIndex, column] = multiple * matrixA[rowIndex, column];
                }
                AfterTransform(multiple.ToString() + "c" + (column + 1).ToString());
            }
            catch (IndexOutOfRangeException)
            {
                MessageBox.Show("下标越界", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        /// <summary>
        /// 单击加列按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnColumnAdditionClickEventHandler(object sender, RoutedEventArgs e)
        {
            try
            {
                int multiple = int.Parse(txtColumnAdditionMultiple.Text);
                int column1 = int.Parse(txtColumnAdditionColumn1.Text) - 1;
                int column2 = int.Parse(txtColumnAdditionColumn2.Text) - 1;
                InitializeMatrix(ref matrixA, ref txtSourceMatrixA, ref rowCountA, ref columnCountA);
                matrixResult = matrixA.Clone() as double[,];
                for (int rowIndex = 0; rowIndex < columnCountA; rowIndex++)
                {
                    matrixResult[rowIndex, column1] += multiple * matrixA[rowIndex, column2];
                }
                AfterTransform(("c" + (column1 + 1).ToString() + "+" + multiple + "c" + (column2 + 1).ToString()).Replace("+-", "-"));
            }
            catch (IndexOutOfRangeException)
            {
                MessageBox.Show("下标越界", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// 处理初等行变换之后的事
        /// </summary>
        /// <param name="text"></param>
        private void AfterTransform(string text)
        {
            PrintMatrix(matrixResult, txtTransformationResult);
            tbkTransformationResult.Text = text;
        }
        #endregion
        #region 限制输入
        Dictionary<TextBox, string> lastString = new Dictionary<TextBox, string>();
        /// <summary>
        /// 只允许输入整数
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UniversalTxtEnterOnlyIntegerTextChangedEventHandler(object sender, TextChangedEventArgs e)
        {
            if (((TextBox)sender).Text != "")
            {
                if (((TextBox)sender).Text == "-")
                {
                    return;
                }
                double tryNum;
                try
                {
                    tryNum = double.Parse(((TextBox)sender).Text);
                    //if (tryNum != Math.Round(tryNum) || tryNum <= 0)
                    //{
                    //    ((TextBox)sender).Text = lastString[(TextBox)sender];
                    //    return;
                    //}
                    lastString[(TextBox)sender] = ((TextBox)sender).Text;
                }
                catch (Exception)
                {
                    try
                    {
                        ((TextBox)sender).Text = lastString[(TextBox)sender];
                    }
                    catch
                    {
                        ((TextBox)sender).Text = "";
                    }
                    ((TextBox)sender).SelectionStart = ((TextBox)sender).Text.Length;
                }
            }
        }
        /// <summary>
        /// 只允许输入数字
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UniversalTxtEnterOnlyNumberTextChangedEventHandler(object sender, TextChangedEventArgs e)
        {
            double tryNum;
            if (((TextBox)sender).Text == "-")
            {
                return;
            }
            if (((TextBox)sender).Text != "")
            {
                try
                {
                    tryNum = double.Parse(((TextBox)sender).Text);
                    lastString[(TextBox)sender] = ((TextBox)sender).Text;
                }
                catch (Exception)
                {
                    try
                    {
                        ((TextBox)sender).Text = lastString[(TextBox)sender];
                    }
                    catch
                    {
                        ((TextBox)sender).Text = "";
                    }
                    ((TextBox)sender).SelectionStart = ((TextBox)sender).Text.Length;
                }
            }
        }
        #endregion
        #region 配置相关
        /// <summary>
        /// 窗口启动事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void WinMainLoadedEventHandler(object sender, RoutedEventArgs e)
        {
            if (cfa.AppSettings.Settings["matrixA"] != null)
            {
                txtSourceMatrixA.Text = cfa.AppSettings.Settings["matrixA"].Value;
            }
            if (cfa.AppSettings.Settings["matrixB"] != null)
            {
                txtSourceMatrixB.Text = cfa.AppSettings.Settings["matrixB"].Value;
            }
            txtSourceMatrixA.Focus();
        }
        /// <summary>
        /// 设置单个配置
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        private void SetConfig(string name, string value)
        {
            if (cfa.AppSettings.Settings[name] == null)
            {
                cfa.AppSettings.Settings.Add(name, value);
            }
            else
            {
                cfa.AppSettings.Settings[name].Value = value;
            }
        }
        /// <summary>
        /// 关闭时保存配置
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void WindowClosingEventHandler(object sender, System.ComponentModel.CancelEventArgs e)
        {
            SetConfig("matrixA", txtSourceMatrixA.Text);
            SetConfig("matrixB", txtSourceMatrixB.Text);
            cfa.Save();
        }

        #endregion



    }
    #region 矩阵类型
    enum MatrixType
    {
        /// <summary>
        /// 不是矩阵
        /// </summary>
        Error = -1,
        /// <summary>
        /// 普通矩阵
        /// </summary>
        Normal = 0,
        /// <summary>
        /// 方阵
        /// </summary>
        SquareMatrix = 1,
        /// <summary>
        /// 排列
        /// </summary>
        Permutation = 2,
        /// <summary>
        /// 行矩阵
        /// </summary>
        RowMatrix = 3,
        /// <summary>
        /// 列矩阵
        /// </summary>
        ColumnMatrix = 4,
    }
    #endregion
}
