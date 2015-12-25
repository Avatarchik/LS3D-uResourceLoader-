using UnityEngine;

using System.Collections.Generic;
using System.IO;
using System;

public class _4DSSceneLoader : _4DSSpecification
{
    private const int rootLod = 0;
    private static int numLODs = 0;

    private static BinaryReader BinaryReader;
    private static CustomReader CustomReader;

    private static FILE _4DSMainHeader = default(FILE);

    private static List<string> _4DSTextures = new List<string>();
    private static List<OBJECTINFO> _4DSObjects = new List<OBJECTINFO>();
	private static List<JointInfo> _4DSJoints = new List<JointInfo>();

    private static GameObject UniqueMesh = null;
    private static List<GameObject> UniqueMeshes = new List<GameObject>();

    private static GameObject ModelObject;

    public static void LoadModel(string modelName)
    {
        BinaryReader = new BinaryReader(File.OpenRead(WorldController.GamePath + modelName + ".4ds"));
        CustomReader = new CustomReader(BinaryReader);

        ModelObject = new GameObject(modelName);
        _4DSMainHeader = CustomReader.ReadType<FILE>(0);
        Debug.Log("4DS version: " + _4DSMainHeader.fileVersion);

        LoadMaterialsInfo(); LoadObjectsInfo();
        bool haveAnimation = BinaryReader.ReadBoolean();

        BinaryReader.BaseStream.Close();
    }

    private static void LoadMaterialsInfo()
    {
        int CountMaterials = BinaryReader.ReadUInt16();

        for (int i = 0; i < CountMaterials; i++)
        {
            MATERIAL _4DSMaterial = CustomReader.ReadType<MATERIAL>();

            string ReflectionTexture = LoadReflectMaterial(); // Load Reflection Material Info
            string DiffuseTexture = LoadDiffuseMaterial(); // Load Diffuse Material Info
            string OpacityTexture = LoadOpacityMaterial(); // Load Opacity Material Info
            LoadAnimationMaterial(); // Load Animation Material Info

            _4DSTextures.Add(DiffuseTexture);
        }
    }

    private static string LoadReflectMaterial()
    {
        long InputPositionInFile = BinaryReader.BaseStream.Position;
        float reflectionLevel = BinaryReader.ReadSingle();

        if (reflectionLevel > 0.01f && reflectionLevel < 1.01f)
        {
            byte reflectionTextureNameLength = BinaryReader.ReadByte();
            return new string(BinaryReader.ReadChars(reflectionTextureNameLength));
        }

        BinaryReader.BaseStream.Position = InputPositionInFile;
        return "Null";
    }

    private static string LoadOpacityMaterial()
    {
        long InputPositionInFile = BinaryReader.BaseStream.Position;
        byte opacityTextureNameLength = BinaryReader.ReadByte();

        if (opacityTextureNameLength > 5 && opacityTextureNameLength <= 15)
            return new string(BinaryReader.ReadChars(opacityTextureNameLength));

        BinaryReader.BaseStream.Position = InputPositionInFile;
        return "Null";
    }

    private static void LoadAnimationMaterial()
    {
        long InputPositionInFile = BinaryReader.BaseStream.Position;
        MATANIMATION mAnimation = CustomReader.ReadType<MATANIMATION>();

        if (mAnimation.delay > 120)
            BinaryReader.BaseStream.Position = InputPositionInFile;
    }

    private static string LoadDiffuseMaterial()
    {
        byte diffuseTextureNameLength = BinaryReader.ReadByte();
        return new string(BinaryReader.ReadChars(diffuseTextureNameLength));
    }

    private static void LoadObjectsInfo()
    {
        int CountObjects = BinaryReader.ReadUInt16();

        for (int i = 0; i < CountObjects; i++)
        {
            byte frameType = BinaryReader.ReadByte(), visualType = 0;
            if (frameType == (int)FrameType.FRAME_VISUAL)
            {
                visualType = BinaryReader.ReadByte();
                byte[] visualFlags = BinaryReader.ReadBytes(2);
            }

            OBJECTINFO objMainInfo = CustomReader.ReadType<OBJECTINFO>();
            byte cullingFlags = BinaryReader.ReadByte();

            byte uniqueMeshNameLength = BinaryReader.ReadByte();
            string uniqueMeshName = new string(BinaryReader.ReadChars(uniqueMeshNameLength));

            byte meshParametersLength = BinaryReader.ReadByte();
            string meshParameters = new string(BinaryReader.ReadChars(meshParametersLength));

            GameObject uniqueMesh = new GameObject(uniqueMeshName);
            uniqueMesh.transform.parent = ModelObject.transform; 
            UniqueMeshes.Add(uniqueMesh); _4DSObjects.Add(objMainInfo);

            UniqueMesh = uniqueMesh;

            if (frameType == (int)FrameType.FRAME_VISUAL)
            {
                switch (visualType)
                {
                    case (int)VisualType.VISUAL_OBJECT: LoadStandartMesh(objMainInfo, uniqueMeshName); break;
                    case (int)VisualType.VISUAL_SINGLEMESH: LoadStandartMesh(objMainInfo, uniqueMeshName); LoadSingleMesh(); break;
                    case (int)VisualType.VISUAL_SINGLEMORPH: LoadStandartMesh(objMainInfo, uniqueMeshName); LoadSingleMesh(); LoadMorph(); break;

                    case (int)VisualType.VISUAL_BILLBOARD: LoadStandartMesh(objMainInfo, uniqueMeshName); BinaryReader.BaseStream.Position += 5; break;
                    case (int)VisualType.VISUAL_MORPH: LoadStandartMesh(objMainInfo, uniqueMeshName); LoadMorph(); break;
                    case (int)VisualType.VISUAL_MIRROR: LoadMirror(); break;

                    default: Debug.Log("Unknown Visual Type. Abort..."); return;
                }
            }

            switch (frameType)
            {
                case (int)FrameType.FRAME_VISUAL: continue;
                case (int)FrameType.FRAME_SECTOR: LoadSector(); break;
                case (int)FrameType.FRAME_TARGET: LoadTarget(); break;
                case (int)FrameType.FRAME_DUMMY: LoadDummy(); break;

                case (int)FrameType.FRAME_JOINT:
					JointInfo jointInfo = new JointInfo(); jointInfo.boneObj = uniqueMesh;
					LoadJoint(out jointInfo.transformMatrix, out jointInfo.boneID); 
					_4DSJoints.Add(jointInfo); break;

                default: Debug.Log("Unknown Frame Type. Abort..."); return;
            }
        }

        ParentObjects();
    }

    private static void LoadStandartMesh(OBJECTINFO objMainInfo, string uniqueMeshName)
    {
        if (BinaryReader.ReadUInt16() == 0)
        {
            numLODs = BinaryReader.ReadByte();

            for (int i = 0; i < numLODs; i++)
            {
                float relativeDistance = BinaryReader.ReadSingle();
                int numVertices = BinaryReader.ReadUInt16();

                List<Vector3> positionOfPoint = new List<Vector3>();
                List<Vector3> normalVector = new List<Vector3>();
                List<Vector2> textureCoordinate = new List<Vector2>();

                List<int[]> meshTriangles = new List<int[]>();
                List<int> materialsId = new List<int>();

                for (int l = 0; l < numVertices; l++)
                {
                    positionOfPoint.Add(CustomReader.ReadType<Vector3>());
                    normalVector.Add(CustomReader.ReadType<Vector3>());
                    textureCoordinate.Add(CustomReader.ReadType<Vector2>());
                }

                int numFaceGroups = BinaryReader.ReadByte();

                for (int l = 0; l < numFaceGroups; l++)
                {
                    List<int> faceTriangles = new List<int>();
                    int numTriangles = BinaryReader.ReadUInt16();

                    for (int n = 0; n < numTriangles * 3; n++)
                        faceTriangles.Add(BinaryReader.ReadUInt16());

                    int materialId = BinaryReader.ReadUInt16();

                    meshTriangles.Add(faceTriangles.ToArray());
                    materialsId.Add(materialId);
                }

                if (i == rootLod)
                {
                    SMeshInfo sMeshInfo = new SMeshInfo();
                    sMeshInfo.name = uniqueMeshName;
                    sMeshInfo.texs = materialsId.ToArray();

                    sMeshInfo.triangles = meshTriangles.ToArray();
                    sMeshInfo.vertices = positionOfPoint.ToArray();
                    sMeshInfo.normals = normalVector.ToArray();
                    sMeshInfo.uv = textureCoordinate.ToArray();

                    CreateMesh(sMeshInfo, objMainInfo);
                }
            }
        }
    }

    private static void LoadSingleMesh()
    {
        for (int i = 0; i < numLODs; i++)
        {
            byte numOfJoints = BinaryReader.ReadByte();
            uint unknown = BinaryReader.ReadUInt32();

            Vector3 min = CustomReader.ReadType<Vector3>();
            Vector3 max = CustomReader.ReadType<Vector3>();

            for (int l = 0; l < numOfJoints; l++)
            {
                Matrix4x4 transformMatrix = CustomReader.ReadType<Matrix4x4>();

                uint _unknown = BinaryReader.ReadUInt32();
                uint numOfAdditionalValues = BinaryReader.ReadUInt32();
                uint boneID = BinaryReader.ReadUInt32();

                Vector3 _min = CustomReader.ReadType<Vector3>();
                Vector3 _max = CustomReader.ReadType<Vector3>();

                float[] additionalValues = CustomReader.ReadType<float>(numOfAdditionalValues);
            }
        }
    }

    private static void LoadMorph()
    {
        byte countFrames = BinaryReader.ReadByte();

        byte numLevelOfGeometryDetails = BinaryReader.ReadByte();
        byte unknown = BinaryReader.ReadByte();

        for (int i = 0; i < numLevelOfGeometryDetails; i++)
        {
            uint numVertices = BinaryReader.ReadUInt16();

            List<Vector3> morphVertices = new List<Vector3>();
            List<Vector3> morphNormals = new List<Vector3>();

            for (int l = 0; l < countFrames; l++)
            {
                for (int n = 0; n < numVertices; n++)
                {
                    morphVertices.Add(CustomReader.ReadType<Vector3>());
                    morphNormals.Add(CustomReader.ReadType<Vector3>());
                }
            }

            byte _unknown = BinaryReader.ReadByte();
            List<ushort> vertexIndices = new List<ushort>();

            if (_unknown >= 1)
                vertexIndices.AddRange(CustomReader.ReadType<ushort>(numVertices));
        }

        float[] unknownArray = CustomReader.ReadType<float>((uint)10);
    }

    private static void LoadMirror()
    {
        Vector3 min = CustomReader.ReadType<Vector3>();
        Vector3 max = CustomReader.ReadType<Vector3>();

        float[] unknown = CustomReader.ReadType<float>((uint)4);

        Matrix4x4 transformMatrix = CustomReader.ReadType<Matrix4x4>();
        Color32 color = new Color32(BinaryReader.ReadByte(), BinaryReader.ReadByte(), BinaryReader.ReadByte(), 255);

        float reflectionStrength = BinaryReader.ReadSingle();
        uint numVertices = BinaryReader.ReadUInt32();
        uint numFaces = BinaryReader.ReadUInt32();

        Vector3[] mirrorVertices = CustomReader.ReadType<Vector3>(numVertices);
        ushort[] mirrorTriangles = CustomReader.ReadType<ushort>(numFaces);
    }

    private static void LoadSector()
    {
        uint[] flags = CustomReader.ReadType<uint>((uint)2);

        uint numVertices = BinaryReader.ReadUInt32();
        uint numFaces = BinaryReader.ReadUInt32();

        Vector3[] sectorVertices = CustomReader.ReadType<Vector3>(numVertices);
        ushort[] sectorTriangles = CustomReader.ReadType<ushort>(numFaces * 3);

        Vector3 min = CustomReader.ReadType<Vector3>();
        Vector3 max = CustomReader.ReadType<Vector3>();

        byte numPortals = BinaryReader.ReadByte();

        for (int i = 0; i < numPortals; i++)
        {
            byte _numVertices = BinaryReader.ReadByte();

            Vector3 n = CustomReader.ReadType<Vector3>();
            float d = BinaryReader.ReadSingle();

            uint _flags = BinaryReader.ReadUInt32();
            float nearRange = BinaryReader.ReadSingle();
            float farRange = BinaryReader.ReadSingle();

            Vector3[] portalVertices = CustomReader.ReadType<Vector3>((uint)_numVertices);
        }
    }

    private static void LoadTarget()
    {
        ushort unknown = BinaryReader.ReadUInt16();
        byte numLinks = BinaryReader.ReadByte();

        ushort[] linksID = CustomReader.ReadType<ushort>((uint)numLinks);
    }

    private static void LoadJoint(out Matrix4x4 transformMatrix, out uint boneID)
    {
        transformMatrix = CustomReader.ReadType<Matrix4x4>();
        boneID = BinaryReader.ReadUInt32();
    }

    private static void LoadDummy()
    {
        Vector3 min = CustomReader.ReadType<Vector3>();
        Vector3 max = CustomReader.ReadType<Vector3>();
    }

    private static void CreateMesh(SMeshInfo sMeshInfo, OBJECTINFO objMainInfo)
    {
        UniqueMesh.transform.position = objMainInfo.positionMesh * WorldController.WorldScale;
        UniqueMesh.transform.localScale = objMainInfo.scaleMesh;

        Quaternion fixedRotation = new Quaternion(objMainInfo.rotationMesh.y, objMainInfo.rotationMesh.z, objMainInfo.rotationMesh.w, -objMainInfo.rotationMesh.x);
        UniqueMesh.transform.rotation = fixedRotation;

        MeshRenderer meshRenderer = UniqueMesh.AddComponent<MeshRenderer>();
        meshRenderer.materials = new Material[sMeshInfo.triangles.Length];

		MeshFilter meshFilter = UniqueMesh.AddComponent<MeshFilter>();
		meshFilter.sharedMesh = new Mesh ();

		for (int i = 0; i < sMeshInfo.vertices.Length; i++)
			sMeshInfo.vertices[i] = sMeshInfo.vertices[i] * WorldController.WorldScale;
		
		meshFilter.sharedMesh.subMeshCount = sMeshInfo.triangles.Length;
		
		meshFilter.sharedMesh.vertices = sMeshInfo.vertices;
		meshFilter.sharedMesh.normals = sMeshInfo.normals;
		meshFilter.sharedMesh.uv = sMeshInfo.uv;
		
		for (int i = 0; i < sMeshInfo.triangles.Length; i++)
		{
			meshRenderer.materials[i].shader = Shader.Find("Mobile/Diffuse");
			meshRenderer.materials[i].mainTexture = TextureLoader.LoadTexture(_4DSTextures[sMeshInfo.texs[i] - 1]);
			
			meshFilter.sharedMesh.SetTriangles(sMeshInfo.triangles[i], i);
		}
		
		meshFilter.sharedMesh.Optimize();
    }

    private static void ParentObjects()
    {
        for (int i = 0; i < _4DSObjects.Count; i++)
        {
            if (_4DSObjects[i].parentID != 0)
                UniqueMeshes[i].transform.parent = UniqueMeshes[_4DSObjects[i].parentID].transform;
        }
    }
}
