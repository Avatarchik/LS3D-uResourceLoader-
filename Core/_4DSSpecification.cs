using UnityEngine;

using System.Collections.Generic;
using System.Runtime.InteropServices;
using System;

public class _4DSSpecification
{
    public enum FrameType
    {
        FRAME_VISUAL = 1,
        FRAME_SECTOR = 5,
        FRAME_DUMMY = 6,
        FRAME_TARGET = 7,
        FRAME_JOINT = 10,
    }

    public enum VisualType
    {
        VISUAL_OBJECT = 0,
        VISUAL_SINGLEMESH = 2,
        VISUAL_SINGLEMORPH = 3,
        VISUAL_BILLBOARD = 4,
        VISUAL_MORPH = 5,
        VISUAL_MIRROR = 8
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct FILE
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public char[] fourCC;

        public short fileVersion;
        public long timeStamp;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct MATERIAL
    {
        public uint flags;

        public Vector3 ambient;
        public Vector3 diffuse;
        public Vector3 emissive;
        public float opacity;
    }

    public struct MATREFLECT
    {
        public float reflectionLevel;
        public byte reflectionTextureNameLength;
        public string reflectionTextureName;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct MATANIMATION
    {
        public uint numFrames;
        public ushort unknown;
        public uint delay;
        public uint unknown2;
        public uint unknown3;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct OBJECTINFO
    {
        public ushort parentID;

        public Vector3 positionMesh;
        public Vector3 scaleMesh;
        public Quaternion rotationMesh;
    }

    public struct SMeshInfo
    {
        public string name;
        public int[] texs;

        public int[][] triangles;
        public Vector3[] vertices;
        public Vector3[] normals;
        public Vector2[] uv;
    }

	public struct JointInfo
	{
		public GameObject boneObj;

		public Matrix4x4 transformMatrix;
		public uint boneID;
	}
}
