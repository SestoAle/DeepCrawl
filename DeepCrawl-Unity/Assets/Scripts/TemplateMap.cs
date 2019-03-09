using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Cass to manage the Template Map technique
public static class TemplateMap
{

  // List of template array. A template can be added in any time; it must be
  // an 10x10 int array with 1 = tile and 0 = column. The generation method then
  // will choose a random template from those listed below
  public static List<int[,]> templates = new List<int[,]>
  {
    new int[,] {
      { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1},
      { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1},
      { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1},
      { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1},
      { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1},
      { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1},
      { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1},
      { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1},
      { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1},
      { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1}
    },
    new int[,] {
      { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1},
      { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1},
      { 1, 1, 0, 1, 1, 1, 1, 0, 1, 1},
      { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1},
      { 1, 1, 0, 1, 1, 1, 1, 0, 1, 1},
      { 1, 1, 0, 1, 1, 1, 1, 0, 1, 1},
      { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1},
      { 1, 1, 0, 1, 1, 1, 1, 0, 1, 1},
      { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1},
      { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1}
    },
    new int[,] {
      { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1},
      { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1},
      { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1},
      { 1, 1, 0, 1, 1, 1, 1, 0, 1, 1},
      { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1},
      { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1},
      { 1, 1, 0, 1, 1, 1, 1, 0, 1, 1},
      { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1},
      { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1},
      { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1}
    },
    new int[,] {
      { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1},
      { 1, 0, 0, 1, 1, 1, 1, 1, 1, 1},
      { 1, 1, 0, 0, 1, 1, 1, 1, 1, 1},
      { 1, 1, 1, 1, 1, 1, 0, 1, 1, 1},
      { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1},
      { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1},
      { 1, 1, 1, 0, 1, 1, 1, 1, 1, 1},
      { 1, 1, 1, 1, 1, 1, 0, 0, 1, 1},
      { 1, 1, 1, 1, 1, 1, 1, 0, 0, 1},
      { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1}
    },
    new int[,] {
      { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1},
      { 1, 0, 0, 1, 1, 1, 1, 1, 1, 1},
      { 1, 1, 0, 1, 1, 1, 1, 1, 1, 1},
      { 1, 1, 1, 1, 1, 1, 0, 1, 1, 1},
      { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1},
      { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1},
      { 1, 1, 1, 0, 1, 1, 1, 1, 1, 1},
      { 1, 1, 1, 1, 1, 1, 1, 0, 1, 1},
      { 1, 1, 1, 1, 1, 1, 1, 0, 0, 1},
      { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1}
    },
    new int[,] {
      { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1},
      { 1, 0, 1, 1, 1, 1, 1, 1, 0, 1},
      { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1},
      { 1, 1, 1, 0, 1, 1, 0, 1, 1, 1},
      { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1},
      { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1},
      { 1, 1, 1, 0, 1, 1, 0, 1, 1, 1},
      { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1},
      { 1, 0, 1, 1, 1, 1, 1, 1, 0, 1},
      { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1}
    },
  };

  // List of hole template array. A hole template can be added in any time; it must be
  // an 10x10 int array with 0 = tile and -2 = no tile. The generation method then
  // will choose a random hole template from those listed below and will add it
  // to the template already chosen
  public static List<int[,]> holeTemplates = new List<int[,]>
  {
    new int[,] {
      { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
      { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
      { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
      { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
      { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
      { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
      { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
      { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
      { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
      { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
    },
    new int[,] {
      { -2, -2, -2, -2, -2, -2, -2, -2, -2, -2},
      { -2, -2, -2, -2, -2, -2, -2, -2, -2, -2},
      { -2, -2, -2, -2, -2, -2, -2, -2, -2, -2},
      { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
      { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
      { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
      { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
      { -2, -2, -2, -2, -2, -2, -2, -2, -2, -2},
      { -2, -2, -2, -2, -2, -2, -2, -2, -2, -2},
      { -2, -2, -2, -2, -2, -2, -2, -2, -2, -2},
    },
    new int[,] {
      { -2, -2, -2, -2, -2, -2, -2, -2, -2, -2},
      { -2, -2, -2, -2, -2, -2, -2, -2, -2, -2},
      { -2 , 0, 0, 0, 0, 0, 0, 0, 0, -2},
      { -2 , 0, 0, 0, 0, 0, 0, 0, 0, -2},
      { -2 , 0, 0, 0, 0, 0, 0, 0, 0, -2},
      { -2 , 0, 0, 0, 0, 0, 0, 0, 0, -2},
      { -2 , 0, 0, 0, 0, 0, 0, 0, 0, -2},
      { -2 , 0, 0, 0, 0, 0, 0, 0, 0, -2},
      { -2 , 0, 0, 0, 0, 0, 0, 0, 0, -2},
      { -2 , 0, 0, 0, 0, 0, 0, 0, 0, -2}
    },
    new int[,] {
      { -2, -2, -2, -2, 0, 0, -2, -2, -2, -2},
      { -2, 0, 0, 0, 0, 0, 0, 0, 0, -2},
      { -2, 0, 0, 0, 0, 0, 0, 0, 0, -2},
      { -2, 0, 0, 0, 0, 0, 0, 0, 0, -2},
      { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
      { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
      { -2, 0, 0, 0, 0, 0, 0, 0, 0, -2},
      { -2, 0, 0, 0, 0, 0, 0, 0, 0, -2},
      { -2, 0, 0, 0, 0, 0, 0, 0, 0, -2},
      { -2, -2, -2, -2, 0, 0, -2, -2, -2, -2},
    },
    new int[,] {
      { -2, -2, -2, -2, -2, 0, 0, 0, 0, 0},
      { -2, -2, -2, -2, -2, 0, 0, 0, 0, 0},
      { -2, -2, -2, -2, -2, 0, 0, 0, 0, 0},
      { -2, -2, -2, -2, -2, 0, 0, 0, 0, 0},
      { -2, -2, -2, -2, -2, 0, 0, 0, 0, 0},
      { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
      { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
      { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
      { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
      { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0}
    },
    new int[,] {
      { -2, -2, -2, 0, 0, 0, 0, 0, 0, 0},
      { -2, -2, -2, 0, 0, 0, 0, 0, 0, 0},
      { -2, -2, -2, 0, 0, 0, 0, 0, 0, 0},
      { -2, -2, -2, 0, 0, 0, 0, 0, 0, 0},
      { -2, -2, -2, 0, 0, 0, 0, 0, 0, 0},
      { 0, 0, 0, 0, 0, 0, 0, -2, -2, -2},
      { 0, 0, 0, 0, 0, 0, 0, -2, -2, -2},
      { 0, 0, 0, 0, 0, 0, 0, -2, -2, -2},
      { 0, 0, 0, 0, 0, 0, 0, -2, -2, -2},
      { 0, 0, 0, 0, 0, 0, 0, -2, -2, -2}
    },
    new int[,] {
      { -2, -2, -2, 0, 0, 0, 0, 0, 0, 0},
      { -2, -2, -2, 0, 0, 0, 0, 0, 0, 0},
      { -2, -2, -2, 0, 0, 0, 0, 0, 0, 0},
      { -2, -2, -2, 0, 0, 0, 0, 0, 0, 0},
      { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
      { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
      { 0, 0, 0, 0, 0, 0, 0, -2, -2, -2},
      { 0, 0, 0, 0, 0, 0, 0, -2, -2, -2},
      { 0, 0, 0, 0, 0, 0, 0, -2, -2, -2},
      { 0, 0, 0, 0, 0, 0, 0, -2, -2, -2}
    }
  };

  // Create the room template
  public static int[,] generateWallMap(int width, int height)
  {
  
    int[,] temp;
    // Get a random template
    temp = templates[Random.Range(0, templates.Count)];


    // Get a random holes map
    int[,] holeMap = holeTemplates[Random.Range(0, holeTemplates.Count)];
    // Rotate the holes map random times
    int numRotations = Random.Range(0, 4);
    for (int i = 0; i < numRotations; i++)
    {
      holeMap = rotateMatrix(holeMap);
    }
    // Add the holes map to the template
    temp = matrixSum(temp, holeMap);

    return temp;
  }

  // Generate the start room
  public static int[,] generateStartingRoom()
  {
    //return templates[0];
    return matrixSum(templates[0], holeTemplates[3]);
  }

  // Utility method: matrix sum
  public static int[,] matrixSum(int[,] mat1, int[,] mat2)
  {
    int[,] sum = new int[mat1.GetLength(0), mat1.GetLength(1)];
    for (int i = 0; i < mat1.GetLength(0); i++)
    {
      for (int j = 0; j < mat1.GetLength(1); j++)
      {
        sum[i, j] = mat1[i, j] + mat2[i, j];
      }
    }
    return sum;
  }

  // Utility method: matrix rotation
  static int[,] rotateMatrix(int[,] oldMatrix)
  {
    int[,] newMatrix = new int[oldMatrix.GetLength(1), oldMatrix.GetLength(0)];
    int newColumn, newRow = 0;
    for (int oldColumn = oldMatrix.GetLength(1) - 1; oldColumn >= 0; oldColumn--)
    {
      newColumn = 0;
      for (int oldRow = 0; oldRow < oldMatrix.GetLength(0); oldRow++)
      {
        newMatrix[newRow, newColumn] = oldMatrix[oldRow, oldColumn];
        newColumn++;
      }
      newRow++;
    }
    return newMatrix;
  }
}

