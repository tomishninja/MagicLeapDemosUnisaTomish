using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class Chunk
{
    public Mesh mesh;

    // the size should proberbly be static but we will see how it runs later on:
    public Vector3Int Size = new Vector3Int(512, 512, 512);

    //
    public Vector3 position = new Vector3(0, 0, 0);

    public readonly int[] Buffer;

    private bool isCreatingMesh = false;
    public bool IsCreatingMesh() { return isCreatingMesh; }

    // notes in tutorial was public and called ready
    public bool ReadyToDraw = false;

    // if this is set to false skip the object when rendering
    public bool isVisiable = true;

    // what ints should be shown and what ones shouldn't
    public int Tolerence = 50;

    //
    private Vector3[] vertices;

    //
    private Vector2[] uvs;

    //
    private int[] triangles;

    public readonly int amountOfVoxelsInChunk;

    private int highestColorValue = 255;

    private int lowestColorValue = 0;

    private Color32[] colors = null;

    public Chunk(Vector3 position, int[] buffer, int tolerence, Vector3Int size, bool isVisiable = true)
    {
        this.position = position;
        this.Buffer = buffer;
        this.Tolerence = tolerence;
        this.Size = size;
        this.amountOfVoxelsInChunk = this.Size.x * this.Size.y * this.Size.z;
        this.isVisiable = isVisiable;
    }

    private Chunk()
    {
        this.position = new Vector3(0, 0, 0);
        Tolerence = 0;
    }

    public System.Collections.IEnumerator StartToGenerateMesh()
    {
        ReadyToDraw = false;
        isCreatingMesh = true;
        Debug.Log("Starting to draw mesh");

        // start the engeration funciton
        Thread _thread = null; _thread = new Thread(GenerateMesh_Concurrently);
        _thread.Start();

        yield return new WaitUntil(() => !isCreatingMesh);

        if (mesh == null)
        {
            mesh = new Mesh();
        }
        else
        {
            mesh.Clear();
        }

        if (!(isVisiable == false || vertices.Length == 0))
        {
            if (vertices.Length > 65000)
            {
                mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            }

            mesh.vertices = vertices;
            mesh.uv = uvs;
            mesh.triangles = triangles;

            mesh.RecalculateNormals();

            mesh.colors32 = colors;

            ReadyToDraw = true;
            Debug.Log("ready to draw == true");
        }


    }

    private void GenerateMesh_Concurrently()
    {
        //0001 1111 //All sides and bottom example

        byte[] Faces = new byte[((int)(this.Size.x * this.Size.y * this.Size.z))];
        
        int index;

        int sizeEstimate = 0;

        // values to keep trak of various data sourses
        int vertexIndex = 0;
        int trianglesIndex = 0;

        int x=0;
        int y=0;
        int z=0;

        // generate faces array
        for (index = 0; index < Buffer.Length; index++)
        {
            if (Buffer[index] < this.Tolerence)
            {
                Faces[index] = 0;
            }
            else
            {
                // determine what faces to show

                // a chunk the size of 1 all round would enable true on all of these all the time
                if (z == 0 || z > 0 && Buffer[index - 1] < Tolerence)
                {
                    Faces[index] |= (byte)Direction.South;
                    sizeEstimate += 4;
                }

                if (z == this.Size.z - 1 || (index + 1 < Buffer.Length && Buffer[index + 1] < Tolerence))
                {
                    Faces[index] |= (byte)Direction.North;
                    sizeEstimate += 4;
                }

                if (y == 0 || Buffer[index - this.Size.z] < Tolerence)
                {
                    Faces[index] |= (byte)Direction.Down;
                    sizeEstimate += 4;
                }

                if (y == this.Size.y - 1 || index + this.Size.z > Buffer.Length || Buffer[index + this.Size.z] < Tolerence)
                {
                    Faces[index] |= (byte)Direction.Up;
                    sizeEstimate += 4;
                }

                if (x == 0 || x > 0 && Buffer[index - (this.Size.z * this.Size.y)] < Tolerence)
                {
                    Faces[index] |= (byte)Direction.West;
                    sizeEstimate += 4;
                }
            
                if (x == this.Size.x - 1 || x < this.Size.x-1 && Buffer[index + (this.Size.z * this.Size.y)] < Tolerence)
                {
                    Faces[index] |= (byte)Direction.East;
                    sizeEstimate += 4;
                }
            }

            z++;
            // check if you hit any milestones before you start the next row.
            if (z == this.Size.z)
            {
                z = 0;
                y++;

                // chek if we need to move on the the next row as well if needed
                if (y == this.Size.y)
                {
                    y = 0;
                    x++;
                }
            }
        }

        Debug.Log("faces done");


        vertices = new Vector3[sizeEstimate];
        colors = new Color32[sizeEstimate];
        uvs = new Vector2[sizeEstimate];
        // number of triangles should allways be equal so rounding isn't nessarcary
        triangles = new int[(int)(sizeEstimate * 1.5)];

        vertexIndex = 0;
        trianglesIndex = 0;

        //revert all the indexes back to zero
        x = 0;
        y = 0;
        z = 0;
        index = 0;

        // generate mesh
        while (index < Faces.Length)
        {
            if (Faces[index] != 0)
            {

                // Calculate the vertex Color
                float currentValueRaw = (Buffer[index] - 0);
                float heighestValueCalculated = this.highestColorValue - this.lowestColorValue;
                float colorValue = currentValueRaw / heighestValueCalculated;
                Color colorOfBlock = new Color(colorValue, colorValue, colorValue, colorValue);

                // create the face
                if ((Faces[index] & (byte)Direction.North) != 0)
                {
                    vertices[vertexIndex] = new Vector3(x + position.x, y + position.y, z + position.z + 1);
                    vertices[vertexIndex + 1] = new Vector3(x + position.x + 1, y + position.y, z + position.z + 1);
                    vertices[vertexIndex + 2] = new Vector3(x + position.x, y + position.y + 1, z + position.z + 1);
                    vertices[vertexIndex + 3] = new Vector3(x + position.x + 1, y + position.y + 1, z + position.z + 1);

                    triangles[trianglesIndex++] = vertexIndex + 1;
                    triangles[trianglesIndex++] = vertexIndex + 2;
                    triangles[trianglesIndex++] = vertexIndex;

                    triangles[trianglesIndex++] = vertexIndex + 1;
                    triangles[trianglesIndex++] = vertexIndex + 3;
                    triangles[trianglesIndex++] = vertexIndex + 2;
                    
                    colors[vertexIndex++] = colorOfBlock;
                    colors[vertexIndex++] = colorOfBlock;
                    colors[vertexIndex++] = colorOfBlock;
                    colors[vertexIndex++] = colorOfBlock;
                }

                if ((Faces[index] & (byte)Direction.East) != 0)
                {
                    vertices[vertexIndex] = new Vector3(x + position.x + 1, y + position.y, z + position.z);
                    vertices[vertexIndex + 1] = new Vector3(x + position.x + 1, y + position.y, z + position.z + 1);
                    vertices[vertexIndex + 2] = new Vector3(x + position.x + 1, y + position.y + 1, z + position.z);
                    vertices[vertexIndex + 3] = new Vector3(x + position.x + 1, y + position.y + 1, z + position.z + 1);

                    triangles[trianglesIndex++] = vertexIndex;
                    triangles[trianglesIndex++] = vertexIndex + 2;
                    triangles[trianglesIndex++] = vertexIndex + 1;

                    triangles[trianglesIndex++] = vertexIndex + 2;
                    triangles[trianglesIndex++] = vertexIndex + 3;
                    triangles[trianglesIndex++] = vertexIndex + 1;
                    
                    colors[vertexIndex++] = colorOfBlock;
                    colors[vertexIndex++] = colorOfBlock;
                    colors[vertexIndex++] = colorOfBlock;
                    colors[vertexIndex++] = colorOfBlock;
                }

                if ((Faces[index] & (byte)Direction.South) != 0)
                {
                    vertices[vertexIndex] = new Vector3(x + position.x, y + position.y, z + position.z);
                    vertices[vertexIndex + 1] = new Vector3(x + position.x + 1, y + position.y, z + position.z);
                    vertices[vertexIndex + 2] = new Vector3(x + position.x, y + position.y + 1, z + position.z);
                    vertices[vertexIndex + 3] = new Vector3(x + position.x + 1, y + position.y + 1, z + position.z);

                    triangles[trianglesIndex++] = vertexIndex;
                    triangles[trianglesIndex++] = vertexIndex + 2;
                    triangles[trianglesIndex++] = vertexIndex + 1;

                    triangles[trianglesIndex++] = vertexIndex + 2;
                    triangles[trianglesIndex++] = vertexIndex + 3;
                    triangles[trianglesIndex++] = vertexIndex + 1;
                    
                    colors[vertexIndex++] = colorOfBlock;
                    colors[vertexIndex++] = colorOfBlock;
                    colors[vertexIndex++] = colorOfBlock;
                    colors[vertexIndex++] = colorOfBlock;
                }

                if ((Faces[index] & (byte)Direction.West) != 0)
                {
                    vertices[vertexIndex] = new Vector3(x + position.x, y + position.y, z + position.z);
                    vertices[vertexIndex + 1] = new Vector3(x + position.x, y + position.y + 1, z + position.z);
                    vertices[vertexIndex + 2] = new Vector3(x + position.x, y + position.y, z + position.z + 1);
                    vertices[vertexIndex + 3] = new Vector3(x + position.x, y + position.y + 1, z + position.z + 1);

                    triangles[trianglesIndex++] = vertexIndex;
                    triangles[trianglesIndex++] = vertexIndex + 2;
                    triangles[trianglesIndex++] = vertexIndex + 1;

                    triangles[trianglesIndex++] = vertexIndex + 2;
                    triangles[trianglesIndex++] = vertexIndex + 3;
                    triangles[trianglesIndex++] = vertexIndex + 1;

                    colors[vertexIndex++] = colorOfBlock;
                    colors[vertexIndex++] = colorOfBlock;
                    colors[vertexIndex++] = colorOfBlock;
                    colors[vertexIndex++] = colorOfBlock;
                }

                if ((Faces[index] & (byte)Direction.Up) != 0)
                {
                    vertices[vertexIndex] = new Vector3(x + position.x, y + position.y + 1, z + position.z);
                    vertices[vertexIndex + 1] = new Vector3(x + position.x + 1, y + position.y + 1, z + position.z);
                    vertices[vertexIndex + 2] = new Vector3(x + position.x, y + position.y + 1, z + position.z + 1);
                    vertices[vertexIndex + 3] = new Vector3(x + position.x + 1, y + position.y + 1, z + position.z + 1);

                    triangles[trianglesIndex++] = vertexIndex;
                    triangles[trianglesIndex++] = vertexIndex + 2;
                    triangles[trianglesIndex++] = vertexIndex + 1;

                    triangles[trianglesIndex++] = vertexIndex + 2;
                    triangles[trianglesIndex++] = vertexIndex + 3;
                    triangles[trianglesIndex++] = vertexIndex + 1;

                    colors[vertexIndex++] = colorOfBlock;
                    colors[vertexIndex++] = colorOfBlock;
                    colors[vertexIndex++] = colorOfBlock;
                    colors[vertexIndex++] = colorOfBlock;
                }

                if ((Faces[index] & (byte)Direction.Down) != 0)
                {
                    vertices[vertexIndex] = new Vector3(x + position.x, y + position.y, z + position.z);
                    vertices[vertexIndex + 1] = new Vector3(x + position.x, y + position.y, z + position.z + 1);
                    vertices[vertexIndex + 2] = new Vector3(x + position.x + 1, y + position.y, z + position.z);
                    vertices[vertexIndex + 3] = new Vector3(x + position.x + 1, y + position.y, z + position.z + 1);

                    triangles[trianglesIndex++] = vertexIndex;
                    triangles[trianglesIndex++] = vertexIndex + 2;
                    triangles[trianglesIndex++] = vertexIndex + 1;

                    triangles[trianglesIndex++] = vertexIndex + 2;
                    triangles[trianglesIndex++] = vertexIndex + 3;
                    triangles[trianglesIndex++] = vertexIndex + 1;

                    colors[vertexIndex++] = colorOfBlock;
                    colors[vertexIndex++] = colorOfBlock;
                    colors[vertexIndex++] = colorOfBlock;
                    colors[vertexIndex++] = colorOfBlock;
                }
            }

            index++;

            z++;
            // check if you hit any milestones before you start the next row.
            if (z == this.Size.z)
            {
                z = 0;
                y++;

                // chek if we need to move on the the next row as well if needed
                if (y == this.Size.y)
                {
                    y = 0;
                    x++;
                }
            }
        }

        //Debug.Log(vertexIndex);
        //Debug.Log(triangles.Length);

        isCreatingMesh = false;

        Debug.Log("Mesh Ready");
    }


}
