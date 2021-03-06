/*  Copyright (C) 2011 Przemyslaw Szeptycki <pszeptycki@gmail.com>, Ecole Centrale de Lyon,

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/>.
*/
using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using Iridium.Numerics.LinearAlgebra;
/**************************************************************************
*
*                          ModelPreProcessing
*
* Copyright (C)         Przemyslaw Szeptycki 2007     All Rights reserved
*
***************************************************************************/

/**
*   @file       Cl3DModel.cs
*   @brief      Object describing a face (3D model)
*   @author     Przemyslaw Szeptycki <pszeptycki@gmail.com>
*   @date       26-10-2007
*
*   @history
*   @item		26-10-2007 Przemyslaw Szeptycki     created at ECL (普查迈克) (بشاماك)
*/
namespace PreprocessingFramework
{
    public class Cl3DModel
    {
        public enum eSpecificPoints
        {
            NoseTip                  = 1,
            LeftEyeLeftCorner        = 2,
            LeftEyeRightCorner       = 3,
            RightEyeLeftCorner       = 4,
            RightEyeRightCorner      = 5,
            LeftCornerOfLips        = 6,
            RightCornerOfLips       = 7,
            LeftCornerOfNose        = 8,
            RightCornerOfNose       = 9,
            UpperLip                = 10,
            BottomLip               = 11,
            LeftEyeUpperEyelid      = 12,
            LeftEyeBottomEyelid     = 13,
            RightEyeUpperEyelid     = 14,
            RightEyeBottomEyelid    = 15,
            UnspecifiedPoint        = 16,
        }

        public class Cl3DModelPointIterator : Cl3DPoint
        {
            public enum eSpecificValues
            {
                Mean_25      = 1,
                Gaussian_25  = 2,
                K1_25                  = 3,
                K2_25                  = 4,
                ShapeIndex_25          = 5,

                Mean_20      = 6,
                Gaussian_20  = 7,
                K1_20                  = 8,
                K2_20                  = 9,
                ShapeIndex_20          = 10,

                Mean_15      = 11,
                Gaussian_15  = 12,
                K1_15                  = 13,
                K2_15                  = 14,
                ShapeIndex_15          = 15,

                Mean_10      = 16,
                Gaussian_10  = 17,
                K1_10                  = 18,
                K2_10                  = 19,
                ShapeIndex_10          = 20,

                GeodesicDistanceToNoseTip                  = 21,
                GeodesicDistanceToLeftEye                  = 22,
                GeodesicDistanceToRightEye                 = 23,
                DifferenceBetween_HCurvatures_25           = 24,
                DifferenceBetween_KCurvatures_25           = 25,
                DifferenceBetween_K1Curvatures_25          = 26,
                DifferenceBetween_K2Curvatures_25          = 27,
                DifferenceBetween_ShapeIndexCurvatures_25  = 28,
                
                DifferenceDescriptorForNoseTip             = 29,
                DifferenceDescriptorForEye                 = 30,

                ConformalFactor                           = 31,
                ConnectedTrianglesArea                    = 32,

                Mean_Custom                      = 33,
                Gaussian_Custom                  = 34,
                K1_Custom                                  = 35,
                K2_Custom                                  = 36,
                ShapeIndex_Custom                          = 37,

                CurvednessIndex_25              = 38,
                CurvednessIndex_20              = 39,
                CurvednessIndex_15              = 40,
                CurvednessIndex_10              = 41,
                GeodesicDistanceFromPoint       = 42,

                K1_40 = 43,
            }

            internal Cl3DModel m_mManagedModel = null;
            internal Cl3DModelPoint m_pActualPoint = null;

            public Cl3DModelPointIterator(Cl3DModel p_mManagedModel)
            {
                m_mManagedModel = p_mManagedModel;
                m_pActualPoint = p_mManagedModel.m_pFirstPointInModel;
            }
            internal Cl3DModelPointIterator(Cl3DModel p_mManagedModel, Cl3DModelPoint p_pActualPoint)
            {
                m_mManagedModel = p_mManagedModel;
                m_pActualPoint = p_pActualPoint;
            }
            public Cl3DModel GetManagedModel()
            {
                return m_mManagedModel;
            }

            public bool MoveToPoint(uint p_uPointID)
            {
                if (m_pActualPoint == null)
                    return false;

                Cl3DModelPoint tmp;

                if (m_mManagedModel.m_dPointsInTheModel.TryGetValue(p_uPointID, out tmp))
                {
                    m_pActualPoint = tmp;
                    return true;
                }
                
                return false;
            }

            public bool MoveToNext()
            {
                if (m_pActualPoint == null)
                    return false;

                m_pActualPoint = m_pActualPoint.m_NextPoint;

                if (m_pActualPoint == null)
                    return false;
                else
                    return true;
            }
            public bool MoveToPrevious()
            {
                if (m_pActualPoint == null || m_pActualPoint.m_PrevPoint == null)
                    return false;

                m_pActualPoint = m_pActualPoint.m_PrevPoint;
                return true;
            }
            public bool IsValid()
            {
                return m_pActualPoint != null;
            }

            public bool IsLabeled(out string Label)
            {
                if (m_pActualPoint == null)
                    throw new Exception("Point is not valid, probable the 3D model has no points");

                Label = "NO";
                foreach (KeyValuePair<String, Cl3DModelPoint> LabeledPoint in m_mManagedModel.m_dSpecificPoints)
                {
                    if (LabeledPoint.Value.m_PointID == m_pActualPoint.m_PointID)
                    {
                        Label = LabeledPoint.Key;
                        return true;
                    }
                }
                return false;
            }

            public bool IsPointInNeighbors(uint p_PointID)
            {
                if (m_pActualPoint == null)
                    throw new Exception("Point is not valid, probable the 3D model has no points");

                foreach (Cl3DModelPoint point in m_pActualPoint.m_NeighborhoodsList)
                    if (point.m_PointID == p_PointID)
                        return true;

                return false;
            }

            public Cl3DModelPointIterator CopyIterator()
            {
                if (m_pActualPoint == null)
                    throw new Exception("Point is not valid, probable the 3D model has no points");

                return new Cl3DModelPointIterator(m_mManagedModel, m_pActualPoint);
            }

            public bool AlreadyVisited
            {
                get
                {
                    if (m_pActualPoint == null)
                        throw new Exception("Point is not valid, probable the 3D model has no points");

                    return m_pActualPoint.m_AlreadyVisited;
                }
                set
                {
                    if (m_pActualPoint == null)
                        throw new Exception("Point is not valid, probable the 3D model has no points");

                    m_pActualPoint.m_AlreadyVisited = value;
                }

            }

            public void AddSpecificValue(eSpecificValues p_eSPV, double p_fVal)
            {
                if (m_pActualPoint == null)
                    throw new Exception("Point is not valid, probable the 3D model has no points");

                if(m_pActualPoint.m_dPointValues.ContainsKey(p_eSPV.ToString()))
                    m_pActualPoint.m_dPointValues.Remove(p_eSPV.ToString());

                m_pActualPoint.m_dPointValues.Add(p_eSPV.ToString(), p_fVal);
            }
            public void AddSpecificValue(String p_sSPV, double p_fVal)
            {
                if (m_pActualPoint == null)
                    throw new Exception("Point is not valid, probable the 3D model has no points");

                if (m_pActualPoint.m_dPointValues.ContainsKey(p_sSPV))
                    m_pActualPoint.m_dPointValues.Remove(p_sSPV);

                m_pActualPoint.m_dPointValues.Add(p_sSPV, p_fVal);
            }
            public void RemoveSpecificValue(string p_sSPV)
            {
                if (m_pActualPoint == null)
                    throw new Exception("Point is not valid, probable the 3D model has no points");

                if (m_pActualPoint.m_dPointValues.ContainsKey(p_sSPV))
                    m_pActualPoint.m_dPointValues.Remove(p_sSPV);
            }
            public void RemoveSpecificValue(eSpecificValues p_eSPV)
            {
                RemoveSpecificValue(p_eSPV.ToString());
            }
            public bool GetSpecificValue(eSpecificValues p_eSPV, out double p_fVal)
            {
                if (m_pActualPoint == null)
                    throw new Exception("Point is not valid, probable the 3D model has no points");

                return m_pActualPoint.m_dPointValues.TryGetValue(p_eSPV.ToString(), out p_fVal);
            }
            public bool GetSpecificValue(String p_sSPV, out double p_fVal)
            {
                if (m_pActualPoint == null)
                    throw new Exception("Point is not valid, probable the 3D model has no points");

                return m_pActualPoint.m_dPointValues.TryGetValue(p_sSPV, out p_fVal);
            }
            public double GetSpecificValue(eSpecificValues p_eSPV)
            {
                return GetSpecificValue(p_eSPV.ToString());
            }
            public double GetSpecificValue(string p_eSPV)
            {
                double Value = 0;
                if (!GetSpecificValue(p_eSPV, out Value))
                    throw new Exception("Cannot get: " + p_eSPV.ToString() + " from a point no: " + this.PointID.ToString());

                return Value;
            }
            public bool IsSpecificValueCalculated(eSpecificValues p_eSPV)
            {
                if (m_pActualPoint == null)
                    throw new Exception("Point is not valid, probable the 3D model has no points");

                return m_pActualPoint.m_dPointValues.ContainsKey(p_eSPV.ToString());
            }
            public bool IsSpecificValueCalculated(String p_sSPV)
            {
                if (m_pActualPoint == null)
                    throw new Exception("Point is not valid, probable the 3D model has no points");

                return m_pActualPoint.m_dPointValues.ContainsKey(p_sSPV);
            }

            public List<string> GetListOfSpecificValues()
            {
                if (m_pActualPoint == null)
                    throw new Exception("Point is not valid, probable the 3D model has no points");

                List<String> outList = new List<String>();
                foreach(KeyValuePair<String, double> SpecificValue in m_pActualPoint.m_dPointValues)
                    outList.Add(SpecificValue.Key);
                return outList;
            }

            public float X
            {
                get
                {
                    if (m_pActualPoint == null)
                        throw new Exception("Point is not valid, probable the 3D model has no points");

                    return m_pActualPoint.m_fX;
                }
                set
                {
                    if (m_pActualPoint == null)
                        throw new Exception("Point is not valid, probable the 3D model has no points");

                    m_pActualPoint.m_fX = value;
                }
            }
            public float Y
            {
                get
                {
                    if (m_pActualPoint == null)
                        throw new Exception("Point is not valid, probable the 3D model has no points");

                    return m_pActualPoint.m_fY;
                }
                set
                {
                    if (m_pActualPoint == null)
                        throw new Exception("Point is not valid, probable the 3D model has no points");

                    m_pActualPoint.m_fY = value;
                }
            }
            public float Z
            {
                get
                {
                    if (m_pActualPoint == null)
                        throw new Exception("Point is not valid, probable the 3D model has no points");

                    return m_pActualPoint.m_fZ;
                }
                set
                {
                    if (m_pActualPoint == null)
                        throw new Exception("Point is not valid, probable the 3D model has no points");

                    m_pActualPoint.m_fZ = value;                   
                }
            }
            public int RangeImageX
            {
                get
                {
                    if (m_pActualPoint == null)
                        throw new Exception("Point is not valid, probable the 3D model has no points");

                    return m_pActualPoint.m_iRangeImageX;
                }
                set
                {
                    if (m_pActualPoint == null)
                        throw new Exception("Point is not valid, probable the 3D model has no points");

                    m_pActualPoint.m_iRangeImageX = value;
                }
            }
            public int RangeImageY
            {
                get
                {
                    if (m_pActualPoint == null)
                        throw new Exception("Point is not valid, probable the 3D model has no points");

                    return m_pActualPoint.m_iRangeImageY;
                }
                set
                {
                    if (m_pActualPoint == null)
                        throw new Exception("Point is not valid, probable the 3D model has no points");

                    m_pActualPoint.m_iRangeImageY = value;
                }
            }

            public uint PointID
            {
                get
                {
                    if (m_pActualPoint == null)
                        throw new Exception("Point is not valid, probable the 3D model has no points");

                    return m_pActualPoint.m_PointID;
                }
            }

            public Color Color
            {
                get
                {
                    if (m_pActualPoint == null)
                        throw new Exception("Point is not valid, probable the 3D model has no points");

                    return m_pActualPoint.m_Color;
                }
                set
                {
                    if (m_pActualPoint == null)
                        throw new Exception("Point is not valid, probable the 3D model has no points");

                    m_pActualPoint.m_Color = value;
                }
            }

            public int ColorR
            {
                get
                {
                    if (m_pActualPoint == null)
                        throw new Exception("Point is not valid, probable the 3D model has no points");

                    int Col = 0;
                    Col = (Col << 8) + (int)m_pActualPoint.m_Color.R;
                    return Col;
                }
            }
            public int ColorG
            {
                get
                {
                    if (m_pActualPoint == null)
                        throw new Exception("Point is not valid, probable the 3D model has no points");

                    int Col = 0;
                    Col = (Col << 8) + (int)m_pActualPoint.m_Color.G;
                    return Col;
                }
            }
            public int ColorB
            {
                get
                {
                    if (m_pActualPoint == null)
                        throw new Exception("Point is not valid, probable the 3D model has no points");

                    int Col = 0;
                    Col = (Col << 8) + (int)m_pActualPoint.m_Color.B;
                    return Col;
                }
            }

            public float U
            {
                get
                {
                    if (m_pActualPoint == null)
                        throw new Exception("Point is not valid, probable the 3D model has no points");

                    return m_pActualPoint.m_fU;
                }
                set
                {
                    if (m_pActualPoint == null)
                        throw new Exception("Point is not valid, probable the 3D model has no points");

                    m_pActualPoint.m_fU = value;
                }
            }
            public float V
            {
                get
                {
                    if (m_pActualPoint == null)
                        throw new Exception("Point is not valid, probable the 3D model has no points");

                    return m_pActualPoint.m_fV;
                }
                set
                {
                    if (m_pActualPoint == null)
                        throw new Exception("Point is not valid, probable the 3D model has no points");

                    m_pActualPoint.m_fV = value;
                }
            }

            public Vector NormalVector
            {
                get
                {
                    if (m_pActualPoint == null)
                        throw new Exception("Point is not valid, probable the 3D model has no points");

                    if (m_pActualPoint.m_NormalVector == null)
                        throw new Exception("Normal vector is not available");

                    return m_pActualPoint.m_NormalVector;
                }
                set
                {
                    if (m_pActualPoint == null)
                        throw new Exception("Point is not valid, probable the 3D model has no points");

                    m_pActualPoint.m_NormalVector = value;
                }
            }

            public static float operator -(Cl3DModelPointIterator point1, Cl3DModelPointIterator point2)
            {
                float res = (float)Math.Sqrt(Math.Pow(point1.X - point2.X, 2) + Math.Pow(point1.Y - point2.Y, 2) + Math.Pow(point1.Z - point2.Z, 2));
                return res;
            }
            /// <summary>
            ///  q[0, 0] = point.X;
            ///  q[1, 0] = point.Y;
            ///  q[2, 0] = point.Z;
            /// </summary>
            /// <param name="Matrix"></param>
            /// <param name="point"></param>
            /// <returns></returns>
            public static Matrix operator *(Matrix Matrix, Cl3DModelPointIterator point)
            {
                // TO DO correct with own multiplication
                Matrix q = new Matrix(3, 1);
                q[0, 0] = point.X;
                q[1, 0] = point.Y;
                q[2, 0] = point.Z;
                Matrix NewQ = Matrix * q;
                return NewQ;
            }

            public static Cl3DModelPointIterator operator +(Cl3DModelPointIterator point, Matrix Matrix)
            {
                point.X = (float)(point.X + Matrix[0, 0]);
                point.Y = (float)(point.Y + Matrix[1, 0]);
                point.Z = (float)(point.Z + Matrix[2, 0]);
                return point;
            }
          
            public void AddNeighbor(Cl3DModelPointIterator p_NewPointInModel)
            {
                if (m_pActualPoint == null)
                    throw new Exception("Point is not valid, probably the 3D model has no points");

                if (p_NewPointInModel == null)
                    throw new Exception("Cannot add NULL neighbor to the point");

                if (p_NewPointInModel.PointID == m_pActualPoint.m_PointID)
                    return;

                foreach (Cl3DModelPoint point in m_pActualPoint.m_NeighborhoodsList)
                    if (point.m_PointID == p_NewPointInModel.PointID)
                        return;

                m_pActualPoint.m_NeighborhoodsList.Add(p_NewPointInModel.m_pActualPoint);
                try
                {
                    p_NewPointInModel.m_pActualPoint.m_NeighborhoodsList.Add(m_pActualPoint);
                }
                catch (Exception)
                {
                    m_pActualPoint.m_NeighborhoodsList.Remove(p_NewPointInModel.m_pActualPoint);
                }
            }
            public void RemoveNeighbor(Cl3DModelPointIterator p_PointInModelToRemoveFromNeighbors)
            {
                if (m_pActualPoint == null)
                    throw new Exception("Point is not valid, probable the 3D model has no points");

                if (p_PointInModelToRemoveFromNeighbors == null)
                    throw new Exception("Cannot remove NULL point from neighbors");

                m_pActualPoint.m_NeighborhoodsList.Remove(p_PointInModelToRemoveFromNeighbors.m_pActualPoint);
                p_PointInModelToRemoveFromNeighbors.m_pActualPoint.m_NeighborhoodsList.Remove(m_pActualPoint);
            }

            public List<Cl3DModelPointIterator> GetListOfNeighbors()
            {
                if (m_pActualPoint == null)
                    throw new Exception("Point is not valid, probable the 3D model has no points");

                List<Cl3DModelPointIterator> Neighborhood = new List<Cl3DModelPointIterator>();
                foreach (Cl3DModelPoint point in m_pActualPoint.m_NeighborhoodsList)
                {
                    Neighborhood.Add(new Cl3DModelPointIterator(m_mManagedModel, point));
                }
                return Neighborhood;
            }

            public Matrix MatrixPoints
            {
                get
                {
                    if (m_pActualPoint == null)
                        throw new Exception("Point is not valid, probable the 3D model has no points");

                    Matrix q = new Matrix(3, 1);
                    q[0, 0] = m_pActualPoint.m_fX;
                    q[1, 0] = m_pActualPoint.m_fY;
                    q[2, 0] = m_pActualPoint.m_fZ;
                    return q;
                 }
                set
                {
                    if (m_pActualPoint == null)
                        throw new Exception("Point is not valid, probable the 3D model has no points");

                    if (!(value.ColumnCount == 1 && value.RowCount == 3))
                        throw new Exception("Uncorrect number of rows and colomns to set vertex values");

                    m_pActualPoint.m_fX = (float)value[0, 0];
                    m_pActualPoint.m_fY = (float)value[1, 0];
                    m_pActualPoint.m_fZ = (float)value[2, 0];
                 }
            }

            public void SetFlag(uint which, bool val)
            {
                if (m_pActualPoint == null)
                    throw new Exception("Point is not valid, probable the 3D model has no points");

                if (which < m_pActualPoint.m_Flags.Length)
                    m_pActualPoint.m_Flags[which] = val;
            }
            public bool GetFlag(uint which)
            {
                if (m_pActualPoint == null)
                    throw new Exception("Point is not valid, probable the 3D model has no points");

                if (which < m_pActualPoint.m_Flags.Length)
                    return m_pActualPoint.m_Flags[which];

                return false;
            }
        }

        internal class Cl3DModelPoint
        {
            //--------------------- MEMBERS ----------------------
            private Cl3DModel m_pMy3DModel = null;
            public float m_fX;
            public float m_fY;
            public float m_fZ;
            public int m_iRangeImageX = -1;
            public int m_iRangeImageY = -1;
            public float m_fU = 0;
            public float m_fV = 0;
            public Vector m_NormalVector = null;

            public Color m_Color = Color.White;

            public Dictionary<String, double> m_dPointValues = new Dictionary<String, double>();
            public List<Cl3DModelPoint> m_NeighborhoodsList = new List<Cl3DModelPoint>();

            public Cl3DModelPoint m_NextPoint = null;
            public Cl3DModelPoint m_PrevPoint = null;

            public readonly uint m_PointID;

            public bool m_AlreadyVisited = false;
            public bool[] m_Flags = new bool[8];

            //----------------- CONSTRUCTORS & DESTRUCTOR ---------
            public Cl3DModelPoint(float p_fX, float p_fY, float p_fZ, Cl3DModel p_p3DModel, uint p_uPointID)
            {
                m_PointID = p_uPointID;
                m_fX = p_fX;
                m_fY = p_fY;
                m_fZ = p_fZ;
                m_pMy3DModel = p_p3DModel;
            }
            public Cl3DModelPoint(float p_fX, float p_fY, float p_fZ, Cl3DModel p_p3DModel, uint p_uPointID, int p_RangeImageX, int p_RangeImageY)
            {
                m_PointID = p_uPointID;
                m_fX = p_fX;
                m_fY = p_fY;
                m_fZ = p_fZ;
                m_pMy3DModel = p_p3DModel;
                m_iRangeImageX = p_RangeImageX;
                m_iRangeImageY = p_RangeImageY;
            }
            public static float operator -(Cl3DModelPoint point1, Cl3DModelPoint point2)
            {
                float res = (float)Math.Sqrt(Math.Pow(point1.m_fX - point2.m_fX, 2) + Math.Pow(point1.m_fY - point2.m_fY, 2) + Math.Pow(point1.m_fZ - point2.m_fZ, 2));
                return res;
            }

            //----------------- PUBLIC METHODS --------------------
            public Cl3DModelPointIterator GetIterator()
            {
                return new Cl3DModelPointIterator(m_pMy3DModel, this);
            }

            public void SaveMe(BinaryWriter File)
            {
                File.Write(this.m_PointID);
                File.Write(this.m_fX);
                File.Write(this.m_fY);
                File.Write(this.m_fZ);
                File.Write(this.m_iRangeImageX);
                File.Write(this.m_iRangeImageY);
                File.Write(this.m_AlreadyVisited);
                File.Write(this.m_Color.ToArgb());
                File.Write(this.m_fU);
                File.Write(this.m_fV);

                File.Write(m_NeighborhoodsList.Count);
                foreach (Cl3DModelPoint point in m_NeighborhoodsList)
                {
                    File.Write(point.m_PointID);
                }

                File.Write(m_dPointValues.Count);
                foreach (KeyValuePair<String, double> point in m_dPointValues)
                {
                    File.Write(point.Key);
                    File.Write(point.Value);
                }
            }

            static public Cl3DModelPoint LoadPoint(BinaryReader reader, Cl3DModel p_pMy3DModel, out List<uint> p_NeighborsList)
            {
                uint PointID = reader.ReadUInt32();
                float X = reader.ReadSingle();
                float Y = reader.ReadSingle();
                float Z = reader.ReadSingle();
                int RangeX = reader.ReadInt32();
                int RangeY = reader.ReadInt32();
                bool AlreadyVisited = reader.ReadBoolean();
                int color = reader.ReadInt32();
                float U = 0;
                float V = 0;

                if (p_pMy3DModel.m_ModelVersion == 3 || p_pMy3DModel.m_ModelVersion == 4 || p_pMy3DModel.m_ModelVersion == 5)
                {
                    U = reader.ReadSingle();
                    V = reader.ReadSingle();
                }

                Cl3DModelPoint newPoint = new Cl3DModelPoint(X,Y,Z,p_pMy3DModel,PointID,RangeX,RangeY);
                newPoint.m_fU = U;
                newPoint.m_fV = V;
                newPoint.m_AlreadyVisited = AlreadyVisited;
                newPoint.m_Color = Color.FromArgb(color);

                p_NeighborsList = new List<uint>();
                int count = reader.ReadInt32();
                for (int i = 0; i < count; i++)
                {
                    p_NeighborsList.Add(reader.ReadUInt32());
                }
                count = reader.ReadInt32();
                for (int i = 0; i < count; i++)
                {
                    String SpecificPointType = "";
                    if (p_pMy3DModel.m_ModelVersion == 0)
                    {
                        int sp = reader.ReadInt32();
                        Cl3DModelPointIterator.eSpecificValues point = (Cl3DModelPointIterator.eSpecificValues)sp;
                        SpecificPointType = point.ToString();
                    }
                    else if (p_pMy3DModel.m_ModelVersion == 1 || p_pMy3DModel.m_ModelVersion == 2 || p_pMy3DModel.m_ModelVersion == 3 || p_pMy3DModel.m_ModelVersion == 4 || p_pMy3DModel.m_ModelVersion == 5)
                    {
                        SpecificPointType = reader.ReadString();
                    }
                    else
                        throw new Exception("Unsupported version of model: ver. " + p_pMy3DModel.m_ModelVersion.ToString());

                    double Value = reader.ReadDouble();
                    newPoint.m_dPointValues.Add(SpecificPointType, Value);
                }

                return newPoint;
            }
        }

        //------------------ STATIC PART --------------------
        private static List<IModelReader> sm_ListOfReaders = new List<IModelReader>();

        public static void RegisterReader(IModelReader p_pReader)
        {
            sm_ListOfReaders.Add(p_pReader);
        }
        public static List<string> sm_ListManagedFilesExtensions
        {
            get
            {
                List<string> tmp = new List<string>();
                foreach (IModelReader loader in sm_ListOfReaders)
                {
                    tmp.Add(loader.GetFileExtension());
                }
                tmp.Add("binaryModel");
                return tmp;
            }
        }
        
        //--------------------- MEMBERS ----------------------
        private readonly uint m_iSupportedFileVersion = 5;
        private string m_sExpression = "Unknown";

        private List<string> m_ListOfPreviousProcessingAlgorithms = new List<string>();
        private uint m_ModelVersion = 0;
        private string m_sModelPath = "Unknown";
        private bool m_bModelHasChanged = true;
        private string m_sModelType = "Unknown";
        private Dictionary<String, Cl3DModelPoint> m_dSpecificPoints = new Dictionary<String, Cl3DModelPoint>();
        private Cl3DModelPoint m_pFirstPointInModel = null;
        private Dictionary<uint, Cl3DModelPoint> m_dPointsInTheModel = new Dictionary<uint, Cl3DModelPoint>();
        private uint m_uPointsCount = 0;
        private uint m_NextPointId = 0;

        //----------------- CONSTRUCTORS & DESTRUCTOR ---------
        public Cl3DModel()
        {
        }

        //--------------------- PUBLIC ------------------------
        public List<string> GetListOfPreviousProcessingAlgorithms()
        {
            return m_ListOfPreviousProcessingAlgorithms;
        }

        internal void AddDoneProcessingAlgorithm(string procesingAlgorithmDescription)
        {
            m_ListOfPreviousProcessingAlgorithms.Add(procesingAlgorithmDescription);
        }

        public string ModelType
        {
            get
            {
                return m_sModelType;
            }
        }

        public string ModelExpression
        {
            get
            {
                return m_sExpression;
            }
            set
            {
                m_sExpression = value;
            }
        }

        public bool IsModelChanged
        {
           get
           {
               return m_bModelHasChanged;
           }
           set
           {
               m_bModelHasChanged = value;
           }
        }
        /// <summary>
        /// The whole file path with file name and extension
        /// </summary>
        public string ModelFilePath
        {
            get
            {
                return m_sModelPath;
            }
            set
            {
                m_sModelPath = value;
            }
        }
        /// <summary>
        /// Only the file name without extension
        /// </summary>
        public string ModelFileName
        {
            get
            {
                int indexOfDot = m_sModelPath.LastIndexOf('.');
                int indexOfBackslesh = m_sModelPath.LastIndexOf('\\');
                int indexOfSlesh = m_sModelPath.LastIndexOf('/');
                if (indexOfDot == -1)
                    return "Unknown";

                if (indexOfBackslesh == -1 && indexOfSlesh != -1)
                    return m_sModelPath.Substring(++indexOfSlesh, indexOfDot - indexOfSlesh);
                else if (indexOfBackslesh != -1 && indexOfSlesh == -1)
                    return m_sModelPath.Substring(++indexOfBackslesh, indexOfDot - indexOfBackslesh);
                else
                    return "Unknown";

            }
        }
        /// <summary>
        /// File path without file name only folder
        /// </summary>
        public string ModelFileFolder
        {
            get
            {
                int indexOfBackslesh = m_sModelPath.LastIndexOf('\\');
                int indexOfSlesh = m_sModelPath.LastIndexOf('/');

                if (indexOfBackslesh == -1 && indexOfSlesh != -1)
                    return m_sModelPath.Substring(0, ++indexOfSlesh);
                else if (indexOfBackslesh != -1 && indexOfSlesh == -1)
                    return m_sModelPath.Substring(0, ++indexOfBackslesh);
                else
                    return "Unknown";
            }
        }

        public void ResetModel()
        {
            m_ModelVersion = 0;
            m_sModelPath = "Unknown";
            m_sModelType = "Unknown";
            m_bModelHasChanged = true;
            m_dSpecificPoints.Clear();
            while (m_pFirstPointInModel != null)
            {
                m_pFirstPointInModel.m_NeighborhoodsList.Clear();
                Cl3DModelPoint next = m_pFirstPointInModel.m_NextPoint;
                if (next != null)
                    next.m_PrevPoint = null;
                m_pFirstPointInModel.m_NextPoint = null;
                m_pFirstPointInModel = next;
            }
        }

        public void ResetVisitedPoints()
        {
            Cl3DModelPoint actual;
            if (m_pFirstPointInModel == null)
                return;

            actual = m_pFirstPointInModel;
            do
            {
                actual.m_AlreadyVisited = false;
                actual = actual.m_NextPoint;
            }
            while (actual != null);
        }

        public void ResetColor(Color p_Color)
        {
            Cl3DModelPoint actual;
            if (m_pFirstPointInModel == null)
                return;

            actual = m_pFirstPointInModel;
            do
            {
                actual.m_Color = p_Color;
                actual = actual.m_NextPoint;
            }
            while (actual != null);
            m_bModelHasChanged = true;
        }

        public uint ModelPointsCount
        {
            get
            {
                return m_uPointsCount;
            }
        }

        private void LoadInternalModelType(string p_sModelPath)
        {
            using (BinaryReader reader = new BinaryReader(File.OpenRead(p_sModelPath)))
            {
                string header = reader.ReadString();
                string[] splitedHeader = header.Split(',');
                string[] splitedFileVersion = splitedHeader[0].Split(' ');
                if(splitedFileVersion[0].Equals("FileVersion:"))
                    m_ModelVersion = UInt32.Parse(splitedFileVersion[1]);

                if (m_ModelVersion > m_iSupportedFileVersion)
                    throw new Exception("Unsupported file version: " + m_ModelVersion.ToString());

                string oldPath = reader.ReadString();
                m_sModelType = reader.ReadString();
                m_NextPointId = reader.ReadUInt32();
                m_uPointsCount = reader.ReadUInt32();

                if (m_ModelVersion == 4 || m_ModelVersion == 5)
                    m_sExpression = reader.ReadString();

                if (m_ModelVersion == 5)
                {
                    int Count = reader.ReadInt32();
                    for( int i=0; i< Count; i++)
                    {
                        m_ListOfPreviousProcessingAlgorithms.Add(reader.ReadString());
                    }
                }

                Dictionary<String, uint> SpecificPointsDictionarry = new Dictionary<String, uint>();

                int count = reader.ReadInt32();
                for (int i = 0; i < count; i++)
                {
                    if (m_ModelVersion == 0 || m_ModelVersion == 1)
                    {
                        int sp = reader.ReadInt32();
                        uint PointID = reader.ReadUInt32();
                        SpecificPointsDictionarry.Add(((eSpecificPoints)sp).ToString(), PointID);
                    }
                    else if (m_ModelVersion == 2 || m_ModelVersion == 3 || m_ModelVersion == 4 || m_ModelVersion == 5)
                    {
                        string sp = reader.ReadString();
                        uint PointID = reader.ReadUInt32();

                        if (!SpecificPointsDictionarry.ContainsKey(sp))
                            SpecificPointsDictionarry.Add(sp, PointID);
                        else
                        {
                            ClInformationSender.SendInformation("Cannot add specific label " + sp + " to a point number " + PointID + ", this label already exists", ClInformationSender.eInformationType.eDebugText);
                        }
                    }
                    else
                        throw new Exception("Unsupported file model version: " + m_ModelVersion.ToString());
                }

                Cl3DModelPoint previous = null;

                m_dSpecificPoints = new Dictionary<String,Cl3DModelPoint>();
                m_dPointsInTheModel = new Dictionary<uint,Cl3DModelPoint>();

                Dictionary<uint, KeyValuePair<Cl3DModel.Cl3DModelPointIterator, List<uint>>> Points = new Dictionary<uint, KeyValuePair<Cl3DModelPointIterator, List<uint>>>();
                for (int i = 0; i < m_uPointsCount; i++)
                {
                    List<uint> NeighborsList;
                    Cl3DModelPoint actual = Cl3DModelPoint.LoadPoint(reader, this, out NeighborsList);

                    Points.Add(actual.m_PointID, new KeyValuePair<Cl3DModelPointIterator, List<uint>>(actual.GetIterator(), NeighborsList));
                    m_dPointsInTheModel.Add(actual.m_PointID, actual);

                    if (previous == null)
                    {
                        m_pFirstPointInModel = actual;
                        previous = m_pFirstPointInModel;
                    }
                    else
                    {
                        previous.m_NextPoint = actual;
                        actual.m_PrevPoint = previous;
                        previous = actual;
                    }
                }

                foreach(KeyValuePair<uint, KeyValuePair<Cl3DModel.Cl3DModelPointIterator, List<uint>>> point in Points)
                {
                    Cl3DModel.Cl3DModelPointIterator Iter = point.Value.Key;
                    List<uint> IterList = point.Value.Value;
                    foreach (uint NeighborID in IterList)
                    {
                        KeyValuePair<Cl3DModel.Cl3DModelPointIterator, List<uint>> Neighboor;
                        if (!Points.TryGetValue(NeighborID, out Neighboor))
                            throw new Exception("Cannot get point fron the dicrionary");

                        Iter.AddNeighbor(Neighboor.Key);
                    }
                }
                
                foreach(KeyValuePair<String, uint> SpecPoint in SpecificPointsDictionarry)
                {
                    string SpecPointLabel = SpecPoint.Key;
                    uint SpecPointID = SpecPoint.Value;

                    Cl3DModelPoint pt = null;
                    if (m_dPointsInTheModel.TryGetValue(SpecPointID, out pt))
                        m_dSpecificPoints.Add(SpecPointLabel, pt);
                    else
                        throw new Exception("Cannot find point with ID: " + SpecPointID + " to assign label: " + SpecPointLabel);
                }
               
                reader.Close();
            }
            ClInformationSender.SendInformation("BinaryModel ver: " + m_ModelVersion.ToString(), ClInformationSender.eInformationType.eDebugText);
        }

        public void LoadModel(string p_sModelPath)
        {
            ClInformationSender.SendInformation("Loading 3D Model (" + p_sModelPath + ")...", ClInformationSender.eInformationType.eTextInternal);

            IModelReader FileReader = null;
            int DotIndex = p_sModelPath.LastIndexOf('.');
            string extension = p_sModelPath.Substring(DotIndex + 1);
            extension = extension.ToLower();

            m_sModelPath = p_sModelPath;
            m_sModelType = extension;

            //internal file format *.binaryModel
            if (extension.CompareTo("binarymodel") == 0)
            {
                LoadInternalModelType(p_sModelPath);
            }
            else
            {
                foreach (IModelReader reader in sm_ListOfReaders)
                {
                    if (reader.GetFileExtension().ToLower() == extension)
                        FileReader = reader;
                }
                if (FileReader == null)
                    throw new Exception("Unsupported file extension: " + extension);

                FileReader.ReadModel(this, p_sModelPath);
            }
            
            ClInformationSender.SendInformation("Model: " + ModelFileName.ToUpper() + " has been loaded. Contains " + ModelPointsCount.ToString() + " points", ClInformationSender.eInformationType.eTextInternal);

            ClInformationSender.SendInformation( GetModelInfo(), ClInformationSender.eInformationType.eDebugText);

        }
        public string GetModelInfo()
        {
            string ModelInfo = "-------------MODEL INFO-------------\n";
            ModelInfo += "* Model name: " + ModelFileName + "\n";
            ModelInfo += "* BinaryModel version: " + m_ModelVersion.ToString() + "\n";
            ModelInfo += "* Expression: " + m_sExpression + "\n";
            ModelInfo += "* Specific Values:\n";
            Dictionary<string, uint> SpecificValues = new Dictionary<string, uint>();
            if (m_pFirstPointInModel != null)
            {
                Cl3DModel.Cl3DModelPointIterator iter = this.GetIterator();
                do
                {
                    foreach (string SpecValues in iter.GetListOfSpecificValues())
                    {
                        uint number = 0;
                        if (SpecificValues.TryGetValue(SpecValues, out number))
                        {
                            SpecificValues.Remove(SpecValues);
                        }

                        number++;
                        SpecificValues.Add(SpecValues, number);
                    }

                } while (iter.MoveToNext());
                foreach (KeyValuePair<string, uint> SpecValues in SpecificValues)
                    ModelInfo += "\t" + SpecValues.Key + ":\t[" + (((float)SpecValues.Value * 100) / m_uPointsCount).ToString() + "%]\t(" + SpecValues.Value.ToString() + "/" + m_uPointsCount.ToString() + ")\n";
            }
            ModelInfo += "\n* Anchor points:\n";
            foreach (KeyValuePair<string, Cl3DModelPoint> point in m_dSpecificPoints)
                ModelInfo += "\t" + point.Key + "(ID: " + point.Value.m_PointID.ToString() + ")\n";

            ModelInfo += "\n* List of the previous processing algorithms:\n";

            string PreviousAlgorithms = "No algorithms stored\n";
            if (m_ListOfPreviousProcessingAlgorithms.Count != 0)
            {
                int count = 0;
                PreviousAlgorithms = "";
                foreach (string alg in m_ListOfPreviousProcessingAlgorithms)
                {
                    PreviousAlgorithms += "  (" + count.ToString() + ") " + alg + "\n";
                    count++;
                }
            }
            ModelInfo = ModelInfo + PreviousAlgorithms + "-------------END OF THE MODEL INFO-------------";
            return ModelInfo;

        }
        /// <summary>
        /// Saves model to the internatl .binaryModel format
        /// </summary>
        /// <param name="p_sModelPath">File path</param>
        public void SaveModel(string p_sModelPath)
        {
            p_sModelPath += ".binaryModel";
            using (FileStream fs = File.OpenWrite(p_sModelPath))
            {
                BinaryWriter Writer = new BinaryWriter(fs);
                Writer.Write("FileVersion: " + m_iSupportedFileVersion + ", Przemyslaw Szeptycki 2011, pszeptycki@gmail.com\n");
                Writer.Write(m_sModelPath);
                Writer.Write(m_sModelType);
                Writer.Write(m_NextPointId);
                Writer.Write(m_uPointsCount);
                Writer.Write(m_sExpression);

                Writer.Write(m_ListOfPreviousProcessingAlgorithms.Count);
                foreach (string alg in m_ListOfPreviousProcessingAlgorithms)
                {
                    Writer.Write(alg);
                }

                Writer.Write(m_dSpecificPoints.Count);
                foreach (KeyValuePair<string, Cl3DModelPoint> SpecificPoint in m_dSpecificPoints)
                {
                    Writer.Write(SpecificPoint.Key);
                    Writer.Write(SpecificPoint.Value.m_PointID);
                }

                Cl3DModelPoint actulaPoint = m_pFirstPointInModel;
                while (actulaPoint != null)
                {
                    actulaPoint.SaveMe(Writer);
                    actulaPoint = actulaPoint.m_NextPoint;
                }
                fs.Close();
            }
        }

        /// <summary>
        /// Gets Range Bitmap from the model
        /// </summary>
        /// <param name="p_Bitmap">Output bitmap</param>
        /// <param name="p_fPower">Power to control depth</param>
        public void GetBMPImage(out Bitmap p_Bitmap, float p_fPower)
        {
            GetBMPImage(out p_Bitmap, p_fPower, 220, 60);
        }
        public void GetBMPImage(out Bitmap p_Bitmap, float p_fPower, int biggerInWidth, int biggerInHeight)
        {
            bool firstPoint = true;
            Cl3DModelPoint point = m_pFirstPointInModel;
            double MinZValue = 0;
            double MaxZValue = 0;
            int MinXValue = 0;
            int MaxXValue = 0;
            int MinYValue = 0;
            int MaxYValue = 0;

            while(point != null)
            {
                if (firstPoint)
                {
                    MinZValue = point.m_fZ;
                    MaxZValue = point.m_fZ;
                    MinXValue = point.m_iRangeImageX;
                    MaxXValue = point.m_iRangeImageX;
                    MinYValue = point.m_iRangeImageY;
                    MaxYValue = point.m_iRangeImageY;
                    firstPoint = false;
                    continue;
                }

                if (MinZValue > point.m_fZ)
                    MinZValue = point.m_fZ;
                if (MaxZValue < point.m_fZ)
                    MaxZValue = point.m_fZ;

                if (MinXValue > point.m_iRangeImageX)
                    MinXValue = point.m_iRangeImageX;
                if (MaxXValue < point.m_iRangeImageX)
                    MaxXValue = point.m_iRangeImageX;

                if (MinYValue > point.m_iRangeImageY)
                    MinYValue = point.m_iRangeImageY;
                if (MaxYValue < point.m_iRangeImageY)
                    MaxYValue = point.m_iRangeImageY;

                point = point.m_NextPoint;
            }
            
            Bitmap BaseRangeBitmap = new Bitmap((MaxXValue - MinXValue) + biggerInWidth, (MaxYValue - MinYValue) + biggerInHeight);
            point = m_pFirstPointInModel;
            while(point != null)
            {
                float proc = 0.0f;
                if (MaxZValue - MinZValue != 0)
                    proc = (float)((point.m_fZ - MinZValue) / (MaxZValue - MinZValue));

                proc = (float)Math.Pow(proc, p_fPower);
                Color colForPixel = Color.FromArgb((int)(proc * point.m_Color.R), (int)(proc * point.m_Color.G), (int)(proc * point.m_Color.B));
                BaseRangeBitmap.SetPixel(point.m_iRangeImageX - MinXValue + biggerInWidth / 2, point.m_iRangeImageY - MinYValue + biggerInHeight / 2, colForPixel);

                point = point.m_NextPoint;
            }

            int no = 0;
            foreach (KeyValuePair<string, Cl3DModelPoint> kvp in m_dSpecificPoints)
            {
                try
                {
                    Color newColor = GetColorRGB(((float)no) / (m_dSpecificPoints.Count - 1), 1f);
                    BaseRangeBitmap.SetPixel((int)kvp.Value.m_iRangeImageX - MinXValue + biggerInWidth / 2, (int)kvp.Value.m_iRangeImageY - MinYValue + biggerInHeight / 2, newColor);
                    BaseRangeBitmap.SetPixel((int)kvp.Value.m_iRangeImageX - MinXValue + 1 + biggerInWidth / 2, (int)kvp.Value.m_iRangeImageY - MinYValue + biggerInHeight / 2, newColor);
                    BaseRangeBitmap.SetPixel((int)kvp.Value.m_iRangeImageX - MinXValue + biggerInWidth / 2, (int)kvp.Value.m_iRangeImageY - MinYValue + 1 + biggerInHeight / 2, newColor);
                    BaseRangeBitmap.SetPixel((int)kvp.Value.m_iRangeImageX - MinXValue - 1 + biggerInWidth / 2, (int)kvp.Value.m_iRangeImageY - MinYValue + biggerInHeight / 2, newColor);
                    BaseRangeBitmap.SetPixel((int)kvp.Value.m_iRangeImageX - MinXValue + biggerInWidth / 2, (int)kvp.Value.m_iRangeImageY - MinYValue - 1 + biggerInHeight / 2, newColor);
                    no++;
                }
                catch (Exception) { }
            }
            Graphics g = Graphics.FromImage(BaseRangeBitmap);
            no = 0;
            foreach (KeyValuePair<string, Cl3DModelPoint> kvp in m_dSpecificPoints)
            {
                Color newColor = GetColorRGB(((float)no) / (m_dSpecificPoints.Count - 1), 1f);
                g.DrawString(kvp.Key, new Font("Arial", 7, FontStyle.Bold), new System.Drawing.SolidBrush(newColor), 0, 10 * no);
                no++;
            }
            p_Bitmap = BaseRangeBitmap;
        }
        /// <summary>
        /// Gets RGB color from percentage
        /// </summary>
        /// <param name="p_fProcent"> form 0 to 1</param>
        /// <param name="p_fPower"> 1 = diagonal</param>
        /// <returns></returns>
        private Color GetColorRGB(float p_fProcent, float p_fPower)
        {
            if (p_fProcent >= 0 && p_fProcent <= 1)
            {
                p_fProcent = (float)Math.Pow((double)p_fProcent, (double)p_fPower);
                int R = 0;
                int G = 0;
                int B = 0;
                if (p_fProcent <= 1 / 5f)
                {
                    float tmp = p_fProcent;
                    tmp /= 1 / 5f;
                    R = (int)((1f * (1 - tmp) + 0f * tmp) * 255f);
                    G = 0;
                    B = 255;

                }
                else if (p_fProcent > 1 / 5f && p_fProcent <= 2 / 5f)
                {
                    float tmp = p_fProcent;
                    tmp -= 1 / 5f;
                    tmp /= 1 / 5f;
                    R = 0;
                    G = (int)((0f * (1 - tmp) + 1f * tmp) * 255f);
                    B = 255;
                }
                else if (p_fProcent > 2 / 5f && p_fProcent <= 3 / 5f)
                {
                    float tmp = p_fProcent;
                    tmp -= 2 / 5f;
                    tmp /= 1 / 5f;
                    R = 0;
                    G = 255;
                    B = (int)((1f * (1 - tmp) + 0f * tmp) * 255f);
                }
                else if (p_fProcent > 3 / 5f && p_fProcent <= 4 / 5f)
                {
                    float tmp = p_fProcent;
                    tmp -= 3 / 5f;
                    tmp /= 1 / 5f;
                    R = (int)((0f * (1 - tmp) + 1f * tmp) * 255f); 
                    G = 255;
                    B = 0;
                }
                else
                {
                    float tmp = p_fProcent;
                    tmp -= 4 / 5f;
                    tmp /= 1 / 5f;
                    R = 255;
                    G = (int)((1f * (1 - tmp) + 0f * tmp) * 255f);
                    B = 0;
                }

                System.Diagnostics.Debug.Assert((R <= 255 && R >= 0) && (G <= 255 && G >= 0) && (B <= 255 && B >= 0), "Wrong RGB value");

                return Color.FromArgb(R, G, B);
            }
            else if (p_fProcent < 0)
            {
                return Color.White;
            }
            else
            {
                return Color.Black;
            }
        }

        //-------- MENAGE POINTS -----------
        public void AddSpecificPoint(eSpecificPoints p_eSpecificPoint, Cl3DModelPointIterator p_point)
        {
            AddSpecificPoint(p_eSpecificPoint.ToString(), p_point);
        }
        public void AddSpecificPoint(String p_eSpecificPoint, Cl3DModelPointIterator p_point)
        {
            if (p_point == null)
                throw new Exception("Cannot add null point to the model");
            if (m_dSpecificPoints.ContainsKey(p_eSpecificPoint))
                m_dSpecificPoints.Remove(p_eSpecificPoint);
            m_dSpecificPoints.Add(p_eSpecificPoint, p_point.m_pActualPoint);
            ClInformationSender.SendInformation("New " + p_eSpecificPoint + "\n X: " + p_point.X.ToString() + "\n Y: " + p_point.Y.ToString() + "\n Z: " + p_point.Z.ToString(), ClInformationSender.eInformationType.eDebugText);
        }
        public bool GetSpecificPoint(eSpecificPoints p_sSpecificPoint, ref Cl3DModelPointIterator p_point)
        {
            return GetSpecificPoint(p_sSpecificPoint.ToString(), ref p_point);
        }
        public bool GetSpecificPoint(string p_sSpecificPoint, ref Cl3DModelPointIterator p_point)
        {
            Cl3DModelPoint point = null;
            bool retVal = m_dSpecificPoints.TryGetValue(p_sSpecificPoint, out point);
            if (retVal == true)
            {
                p_point = point.GetIterator();
            }
            return retVal;
        }

        public bool IsThisPointInSpecificPoints(Cl3DModelPointIterator point, ref string p_eSpecificPoint)
        {
            foreach (KeyValuePair<string, Cl3DModelPoint> points in m_dSpecificPoints)
            {
                if (point.PointID == points.Value.m_PointID)
                {
                    p_eSpecificPoint = points.Key;
                    return true;
                }
            }

            return false;
        }

        public Cl3DModelPointIterator GetSpecificPoint(eSpecificPoints p_eSpecificPoint)
        {
            return GetSpecificPoint(p_eSpecificPoint.ToString());
        }

        public Cl3DModelPointIterator GetSpecificPoint(string p_eSpecificPoint)
        {
            Cl3DModelPointIterator tmp = null;
            if (!GetSpecificPoint(p_eSpecificPoint, ref tmp))
                throw new Exception("Cannot find: " + p_eSpecificPoint.ToString());
            else
                return tmp;
        }
        public void RemoveAllSpecificPoints()
        {
            m_dSpecificPoints.Clear();
        }
        public List<KeyValuePair<string,Cl3DModelPointIterator>> GetAllSpecificPoints()
        {
            List<KeyValuePair<string, Cl3DModelPointIterator>> outList = new List<KeyValuePair<string, Cl3DModelPointIterator>>();
            foreach (KeyValuePair<string, Cl3DModelPoint> KeyVal in m_dSpecificPoints)
            {
                outList.Add(new KeyValuePair<string, Cl3DModelPointIterator>(KeyVal.Key, new Cl3DModelPointIterator(this, KeyVal.Value)));
            }
            return outList;
        }

        public Cl3DModelPointIterator RemovePointFromModel(Cl3DModelPointIterator p_pPointToRemoveFromModel)
        {
            if (m_pFirstPointInModel == null || p_pPointToRemoveFromModel == null || p_pPointToRemoveFromModel.m_pActualPoint == null)
                return null;

            foreach (KeyValuePair<string, Cl3DModelPoint> point in m_dSpecificPoints)
            {
                if (point.Value.m_PointID == p_pPointToRemoveFromModel.PointID)
                {
                    m_dSpecificPoints.Remove(point.Key);
                    break;
                }
            }

            if (m_pFirstPointInModel.m_PointID == p_pPointToRemoveFromModel.m_pActualPoint.m_PointID)
                m_pFirstPointInModel = m_pFirstPointInModel.m_NextPoint;

            List<Cl3DModelPointIterator> listOfNeighbors = p_pPointToRemoveFromModel.GetListOfNeighbors();
            foreach (Cl3DModelPointIterator neighbor in listOfNeighbors)
                neighbor.RemoveNeighbor(p_pPointToRemoveFromModel);

            if (p_pPointToRemoveFromModel.m_pActualPoint.m_NextPoint != null)
                p_pPointToRemoveFromModel.m_pActualPoint.m_NextPoint.m_PrevPoint = p_pPointToRemoveFromModel.m_pActualPoint.m_PrevPoint;
            if (p_pPointToRemoveFromModel.m_pActualPoint.m_PrevPoint != null)
                p_pPointToRemoveFromModel.m_pActualPoint.m_PrevPoint.m_NextPoint = p_pPointToRemoveFromModel.m_pActualPoint.m_NextPoint;

            m_dPointsInTheModel.Remove(p_pPointToRemoveFromModel.PointID);
            
            Cl3DModel Model = p_pPointToRemoveFromModel.m_mManagedModel;
            p_pPointToRemoveFromModel.m_mManagedModel = null;

            Cl3DModelPoint ActualPoint = p_pPointToRemoveFromModel.m_pActualPoint;
            p_pPointToRemoveFromModel.m_pActualPoint = null;
            
            m_uPointsCount--;

            return new Cl3DModelPointIterator(Model, ActualPoint.m_NextPoint);

            
        }
        public Cl3DModelPointIterator AddPointToModel(float p_fX, float p_fY, float p_fZ, int p_iRangeImageX, int p_iRangeImageY)
        {
            return AddPointToModel(p_fX, p_fY, p_fZ, p_iRangeImageX, p_iRangeImageY, m_NextPointId++);
        }
        public Cl3DModelPointIterator AddPointToModel(float p_fX, float p_fY, float p_fZ)
        {
            if (float.IsNaN(p_fX) || float.IsNaN(p_fY) || float.IsNaN(p_fZ))
                throw new Exception("Coordinates: X or Y or Z are not number(s)");

            return AddPointToModel(p_fX, p_fY, p_fZ, m_NextPointId++);
        }
        public Cl3DModelPointIterator AddPointToModel(float p_fX, float p_fY, float p_fZ, int p_iRangeImageX, int p_iRangeImageY, uint p_PointID)
        {
            Cl3DModelPointIterator newPoint = AddPointToModel(p_fX, p_fY, p_fZ, p_PointID);
            newPoint.RangeImageX = p_iRangeImageX;
            newPoint.RangeImageY = p_iRangeImageY;

            return newPoint;
        }
        public Cl3DModelPointIterator AddPointToModel(float p_fX, float p_fY, float p_fZ, uint p_PointID)
        {
            if (m_dPointsInTheModel.ContainsKey(p_PointID))
                throw new Exception("Such point with ID: " + p_PointID + " exists in the model");

            Cl3DModelPoint newPoint = new Cl3DModelPoint(p_fX, p_fY, p_fZ, this, p_PointID);

            m_dPointsInTheModel.Add(p_PointID, newPoint);

            if (m_pFirstPointInModel == null)
            {
                m_pFirstPointInModel = newPoint;
            }
            else
            {
                newPoint.m_NextPoint = m_pFirstPointInModel;
                m_pFirstPointInModel.m_PrevPoint = newPoint;
                m_pFirstPointInModel = newPoint;
            }

            if (m_NextPointId <= p_PointID)
                m_NextPointId = p_PointID + 1;

            m_uPointsCount++;
            return newPoint.GetIterator();
        }
        
        public Cl3DModelPointIterator GetIterator()
        {
            return new Cl3DModelPointIterator(this);
        }
    }
}
