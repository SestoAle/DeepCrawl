using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// Cellular Automata algorithm to create the map and walls
public class CellularAutomata
{

  float chanceToStartAlive;
  int width;
  int height;
  int numberOfStep;

  // The algorithm takes as input the prob for a cell to stay alive, the size of
  // the board, and the number of steps used in the creation of the map
  public CellularAutomata(float chanceToStartAlive, int width, int height, int numberOfStep)
  {
    this.chanceToStartAlive = chanceToStartAlive;
    this.width = width;
    this.height = height;
    this.numberOfStep = numberOfStep;
  }

  bool[,] initializeMap(bool[,] map)
  {
    for (int x = 0; x < width; x++)
    {
      for (int y = 0; y < height; y++)
      {
        if (UnityEngine.Random.Range(0f, 1.0f) < chanceToStartAlive)
        {
          map[x, y] = true;
        }
      }
    }
    return map;
  }

  bool[,] doSimulationStep(bool[,] oldMap)
  {
    bool[,] newMap = new bool[width, height];
    for (int x = 0; x < width; x++)
    {
      for (int y = 0; y < height; y++)
      {
        int nbs = countAliveNeighbours(oldMap, x, y);
        if (oldMap[x, y])
        {
          if (nbs < 2)
          {
            newMap[x, y] = false;
          }
          else
          {
            newMap[x, y] = true;
          }
        }
        else
        {
          if (nbs > 3)
          {
            newMap[x, y] = true;
          }
          else
          {
            newMap[x, y] = false;
          }
        }
      }
    }
    return newMap;
  }

  int countAliveNeighbours(bool[,] oldMap, int x, int y)
  {
    int count = 0;
    for (int i = -1; i < 2; i++)
    {
      for (int j = -1; j < 2; j++)
      {
        int neighbour_x = x + i;
        int neighbour_y = y + j;
        if (i == 0 && j == 0)
        {

        }
        else if (neighbour_x < 0 || neighbour_y < 0 || neighbour_x >= width || neighbour_y >= height)
        {

        }
        else if (oldMap[neighbour_x, neighbour_y])
        {
          count++;
        }
      }
    }
    return count;
  }

  public bool[,] generateMap()
  {
    bool[,] cellmap = new bool[width, height];
    cellmap = initializeMap(cellmap);
    for (int i = 0; i < numberOfStep; i++)
    {
      cellmap = doSimulationStep(cellmap);
    }
    return cellmap;
  }
}
