using UnityEngine;

public class VoxelMap : MonoBehaviour
{

    public float chunkSize = 2f;

    public int voxelResolution = 8;

    public int chunkResolutionX = 2;
    public int chunkResolutionY = 2;

    private int callbacksCount;

    public VoxelGrid voxelGridPrefab;

    private VoxelGrid[] chunks;

    private float voxelSize, halfSize;

    private VoxelStencil[] stencils = {
        new VoxelStencil(),
        new VoxelStencilCircle()
    };
    public Transform[] stencilVisualizations;

    public float maxFeatureAngle = 135f;

    public CameraControler cameraControler;
    #region GUI

    private static string[] fillTypeNames = { "Filled", "Empty" };

    private static string[] radiusNames = { "0", "1", "2", "3", "4", "5" };

    private static string[] stencilNames = { "Square", "Circle" };

    private int fillTypeIndex = 1, radiusIndex = 4, stencilIndex = 1;

    private void OnGUI()
    {
        GUILayout.BeginArea(new Rect(4f, 4f, 150f, 500f));
        GUILayout.Label("Fill Type");
        fillTypeIndex = GUILayout.SelectionGrid(fillTypeIndex, fillTypeNames, 2);
        GUILayout.Label("Radius");
        radiusIndex = GUILayout.SelectionGrid(radiusIndex, radiusNames, 6);
        GUILayout.Label("Stencil");
        stencilIndex = GUILayout.SelectionGrid(stencilIndex, stencilNames, 2);
        GUILayout.EndArea();
    }
    #endregion

    private void Awake()
    {   
        callbacksCount = 0;
        halfSize = chunkSize * 0.5f;
        chunkSize = 2;
        voxelSize = chunkSize / voxelResolution;
        int chunksCount = chunkResolutionX * chunkResolutionY;
        chunks = new VoxelGrid[chunksCount];

        cameraControler.Initialze(new Vector2(chunkResolutionX * chunkSize / 2, chunkResolutionY * chunkSize));

        for (int i = 0, y = 0; y < chunkResolutionY; y++)
        {
            for (int x = 0; x < chunkResolutionX; x++, i++)
            {
                CreateChunk(i, x, y);

            }
        }
        BoxCollider box = gameObject.AddComponent<BoxCollider>();
        box.size = new Vector3(chunkResolutionX* chunkSize, chunkResolutionY* chunkSize);
        box.center= new Vector3((chunkResolutionX * chunkSize) / 2, (chunkResolutionY * chunkSize )/2,0);

        InitCallback();

    }

    private void Update()
    {
        Transform visualization = stencilVisualizations[stencilIndex];
        RaycastHit hitInfo;
        if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hitInfo) &&
            hitInfo.collider.gameObject == gameObject)
        {
            //used for stencil visualization calculations
            Vector2 center = transform.InverseTransformPoint(hitInfo.point);
            center.x += halfSize;
            center.y += halfSize;
            //----------------
            if (Input.GetMouseButton(0))
            {
                VoxelStencil activeStencil = stencils[stencilIndex];
                activeStencil.Initialize(fillTypeIndex == 0, (radiusIndex + 0.5f) * voxelSize);
                activeStencil.SetCenter(center.x, center.y);

                EditVoxels(center, activeStencil);
            }
            //used for stencil visualization calculations
            center.x -= halfSize;
            center.y -= halfSize;
            visualization.localPosition = center;
            Vector3 tempScale = Vector3.one * ((radiusIndex + 0.5f) * voxelSize * 2f);
            visualization.localScale = new Vector3(tempScale.x,0.01f, tempScale.z);
            //----------------
            visualization.gameObject.SetActive(true);
        }
        else
        {
            visualization.gameObject.SetActive(false);
        }


       

    }

    private void EditVoxels(Vector2 center, VoxelStencil stencilToUse)
    {
       
        int xStart = (int)((stencilToUse.XStart - voxelSize) / chunkSize);
        if (xStart < 0)
        {
            xStart = 0;
        }
        int xEnd = (int)((stencilToUse.XEnd + voxelSize) / chunkSize);
        if (xEnd >= chunkResolutionX)
        {
            xEnd = chunkResolutionX - 1;
        }
        int yStart = (int)((stencilToUse.YStart - voxelSize) / chunkSize);
        if (yStart < 0)
        {
            yStart = 0;
        }
        int yEnd = (int)((stencilToUse.YEnd + voxelSize) / chunkSize);
        if (yEnd >= chunkResolutionY)
        {
            yEnd = chunkResolutionY - 1;
        }

        for (int y = yEnd; y >= yStart; y--)
        {
            int i = y * chunkResolutionX  + xEnd;
            for (int x = xEnd; x >= xStart; x--, i--)
            {
                stencilToUse.SetCenter(center.x - x * chunkSize, center.y - y * chunkSize);
                chunks[i].Apply(stencilToUse);
            }
        }
    }

    private void CreateChunk(int i, int x, int y)
    {

        VoxelGrid chunk = Instantiate(voxelGridPrefab) as VoxelGrid;
        chunk.Initialize(voxelResolution, chunkSize, maxFeatureAngle, InitCallback);
        chunk.transform.parent = transform;
        chunk.transform.localPosition = new Vector3(x * chunkSize - halfSize, y * chunkSize - halfSize);
        chunks[i] = chunk;
        if (x > 0)
        {
            chunks[i - 1].xNeighbor = chunk;
        }
        if (y > 0)
        {
            chunks[i - chunkResolutionX].yNeighbor = chunk;
            if (x > 0)
            {
                chunks[i - chunkResolutionX - 1].xyNeighbor = chunk;
            }
        }

    }

    void InitCallback()
    {
        callbacksCount++;
        if(callbacksCount == chunkResolutionX*chunkResolutionY+1)
            for (int i = 0; i < chunks.Length; i++)
            {
                chunks[i].Refresh(CutStart);
            }
    }

    void CutStart()
    {
        Vector2 center = new Vector2(chunkResolutionX*chunkSize/2,chunkResolutionY*chunkSize);

         VoxelStencil activeStencil = stencils[1];
         activeStencil.Initialize(false, (4 + 0.5f) * voxelSize);
         activeStencil.SetCenter(center.x, center.y);

        EditVoxels(center, activeStencil);

    }


}