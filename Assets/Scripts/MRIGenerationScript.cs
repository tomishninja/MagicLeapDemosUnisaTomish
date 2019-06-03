using UnityEngine;

public class MRIGenerationScript : MonoBehaviour
{
    public enum InputTypeE
    {
        JSON,
        PNM
    };

    public MeshFilter meshFilter;

    public MeshRenderer meshRenderer;

    public Chunk chunk;

    public InputTypeE inputType = InputTypeE.PNM;

    private int width = 0;
    private int height = 0;
    private int breadth = 0;
    
    public string filePath = "/DicomData/dcmToJSONTest.json";

    //private int[] VisableObjectsBuffer = null;

    public Material chunksMaterial;

    public int tolerance = 0;

    // Use this for initialization
    void Start()
    {
        //VisableObjectsBuffer = new int[width * height * breadth];

        // gerate a chunk(s) for the image
        // just create one new chunk to display
        chunk = new Chunk(this.transform.position, this.GetImageDataAsInts(), tolerance, new Vector3Int(width, height, breadth));
        StartCoroutine(chunk.StartToGenerateMesh());
    }

    private void Update()
    {
        if (chunk != null)
        {
            if (chunk.ReadyToDraw)
            {
                meshFilter.mesh = chunk.mesh;
                meshRenderer.material = chunksMaterial;
                chunk = null;
            }
        }
    }

    private int[] GetImageDataAsInts()
    {
        // if the image is set to a pnm input data this way
        if (InputTypeE.PNM == inputType)
        {
            PNMtoBuffer.PNMtoBufferedIntArray buffer = new PNMtoBuffer.PNMtoBufferedIntArray();
            PNMtoBuffer.PNMIntArrayObject output = buffer.Compile(this.filePath);

            this.width = output.Width;
            this.height = output.Height;

            this.breadth = 1;

            return output.Pixels;
        }
        else
        {
            //TODO
            return null;
        }
    }
}
