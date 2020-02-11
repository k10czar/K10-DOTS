using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

public sealed class ScreenVisibilityCheckSystem : ComponentSystem
{
	Camera _mainCamera;

	const int MAX_ELEMENTS_PER_BATCH = 128;

	NativeArray<float3> _positions;
	NativeArray<float3> _vpos;
	NativeArray<byte> _result;

	protected override void OnStartRunning()
	{
		_positions = new NativeArray<float3>(MAX_ELEMENTS_PER_BATCH, Allocator.Persistent);
		_vpos = new NativeArray<float3>(MAX_ELEMENTS_PER_BATCH, Allocator.Persistent);
		_result = new NativeArray<byte>(MAX_ELEMENTS_PER_BATCH, Allocator.Persistent);
	}

	protected override void OnStopRunning()
	{
		_positions.Dispose();
		_vpos.Dispose();
		_result.Dispose();
	}

	[BurstCompile]
	struct Job : IJobParallelFor
	{
		const float TOLERANCE = 0.5f;
		const float MIN_TOLERANCE = -MAX_TOLERANCE;
		const float MAX_TOLERANCE = 1 + TOLERANCE;

		[ReadOnly] public NativeArray<float3> positions;
		[ReadOnly] public float4x4 worldToScreenTransform;
		[ReadOnly] public float nearClip;
		[ReadOnly] public float farClip;
		public NativeArray<float3> vPos;
		public NativeArray<byte> visible;

		public void Execute(int i)
		{
			var wPos = new float4(positions[i], 1);
			wPos.y = wPos.y + .5f; // Hack to reach center of characters and only charge at oher threads
			var temp = math.mul(worldToScreenTransform, wPos);
			var vPos = new float3(temp.x / temp.w, temp.y / temp.w, temp.z);
			this.vPos[i] = vPos;

			visible[i] = (vPos.z > 0 &&
							vPos.z < farClip &&
							vPos.x > MIN_TOLERANCE &&
							vPos.x < MAX_TOLERANCE &&
							vPos.y > MIN_TOLERANCE &&
							vPos.y < MAX_TOLERANCE) ?
							byte.MaxValue : byte.MinValue;
		}
	}

	private static readonly EntitiesCollection<ScreenVisibility> _entities = new EntitiesCollection<ScreenVisibility>();

	public static bool Add(ScreenVisibility element) => _entities.Add(element);
	public static void Remove(ScreenVisibility element) => _entities.Remove(element);

	protected override void OnUpdate()
	{
		if (_mainCamera == null) _mainCamera = Camera.main;
		var camera = _mainCamera;

		var mat = camera.projectionMatrix * camera.worldToCameraMatrix;

		var entitiesCount = _entities.Count;

		for (int j = 0; j < entitiesCount; j += MAX_ELEMENTS_PER_BATCH)
		{
			var elements = Mathf.Min(entitiesCount, j + MAX_ELEMENTS_PER_BATCH) - j;

			for (int i = 0; i < elements; i++)
			{
				var e = _entities[j + i];
				_positions[i] = e.transform.position;
			}

			var job = new Job()
			{
				worldToScreenTransform = mat,
				positions = _positions,
				visible = _result,
				nearClip = camera.nearClipPlane,
				farClip = camera.farClipPlane,
				vPos = _vpos,
			};

			var handle = job.Schedule(elements, 1);
			handle.Complete();

			for (int i = 0; i < elements; i++)
			{
				_entities[j + i].SetData(_vpos[i], _result[i] != byte.MinValue);
			}
		}
	}
}