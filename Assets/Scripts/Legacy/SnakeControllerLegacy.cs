using System.Collections.Generic;
using UnityEngine;

namespace Goji.Legacy
{
	public class SnakeControllerLegacy : MonoBehaviour
	{
		#region Properties
		private Vector2Int DesiredMoveDirection { get; set; }
		private Vector2Int PreviousMoveDirection { get; set; }

		private int SnakeMovementTimer { get; set; }
		private int SnakeLength { get; set; }

		private Vector2Int HeadPosition { get; set; }
		private List<Vector2Int> SegmentPositions { get; set; }
		private List<Transform> Segments { get; set; }

		public bool IsDead { get; private set; }

		private Transform Fruit { get; set; }
		#endregion

		#region Fields
		[SerializeField]
		private int defaultSnakeLength;

		[Space(20), SerializeField]
		int snakeMoveRate;

		[Space(20), SerializeField]
		Transform snakeBodySegmentPrefab;
		[SerializeField]
		Transform fruitPrefab;
		#endregion

		#region Methods
		void Start()
		{
			// Set the initial length of the snake
			SnakeLength = defaultSnakeLength;

			// Create the segment and segment position lists
			Segments = new List<Transform>();
			SegmentPositions = new List<Vector2Int>();

			// Create initial snake body segments
			for (int i = 0; i < SnakeLength; i++)
			{
				Transform segment = Instantiate(snakeBodySegmentPrefab);
				segment.name = $"Segment {i}";
				segment.SetParent(this.transform);
				Segments.Add(segment);
			}

			// Set the default movement direction
			DesiredMoveDirection = Vector2Int.right;
			PreviousMoveDirection = Vector2Int.zero;

			// Create the fruit and initialize it to a random position
			Fruit = Instantiate(fruitPrefab);
			Fruit.name = "Fruit";
			Fruit.position = (Vector3Int)GetValidFruitLocation();
		}

		void Update()
		{
			// Update the desired movement direction
			if (UnityEngine.Input.GetKeyDown(KeyCode.W))
				DesiredMoveDirection = Vector2Int.up;
			if (UnityEngine.Input.GetKeyDown(KeyCode.S))
				DesiredMoveDirection = Vector2Int.down;
			if (UnityEngine.Input.GetKeyDown(KeyCode.D))
				DesiredMoveDirection = Vector2Int.right;
			if (UnityEngine.Input.GetKeyDown(KeyCode.A))
				DesiredMoveDirection = Vector2Int.left;

			// Ignore inputs that would cause the snake to go backwards
			if (DesiredMoveDirection == PreviousMoveDirection * -1)
				DesiredMoveDirection = PreviousMoveDirection;
		}

		void FixedUpdate()
		{
			// Move the snake if the timer has elapsed
			if (SnakeMovementTimer > snakeMoveRate && !IsDead)
			{
				PerformMovement();
				CheckCollisions();
				UpdateSegments();

				// Reset movement timer
				SnakeMovementTimer = 0;
			}

			// Increment the snake movement timer
			SnakeMovementTimer++;
		}

		private void PerformMovement()
		{
			// Move the head of the snake
			HeadPosition += DesiredMoveDirection;

			// Set the previous move direction for next frame
			PreviousMoveDirection = DesiredMoveDirection;

			// Add the head position to the beginning of the segment positions list
			SegmentPositions.Insert(0, HeadPosition);

			// Remove the final position from the list if the list is longer than the snake's length
			if (SegmentPositions.Count > SnakeLength)
				SegmentPositions.RemoveAt(SegmentPositions.Count - 1);
		}

		private void CheckCollisions()
		{
			// Check if the head position is the same as the fruit
			Vector2Int fruitPosition = new Vector2Int((int)Fruit.position.x, (int)Fruit.position.y);
			if (HeadPosition == fruitPosition)
			{
				// Increase the snake length
				SnakeLength++;

				// Move the fruit
				Fruit.position = (Vector3Int)GetValidFruitLocation();
			}

			// Check if the head position is the same as any non-head segments
			for (int i = 1; i < SegmentPositions.Count; i++)
			{
				if (HeadPosition == SegmentPositions[i])
					IsDead = true;
			}

			// Check if the head has left the bounds of the map
			if (HeadPosition.x > 22 || HeadPosition.x < -22 || HeadPosition.y > 12 || HeadPosition.y < -12)
				IsDead = true;
		}

		private void UpdateSegments()
		{
			if (IsDead)
				return;

			// Add any missing segments
			while (Segments.Count < SnakeLength)
			{
				Transform segment = Instantiate(snakeBodySegmentPrefab);
				segment.name = $"Segment {Segments.Count}";
				segment.position = (Vector3Int)SegmentPositions[^1];
				segment.SetParent(this.transform);
				Segments.Add(segment);
			}

			// Set segment positions
			for (int i = 0; i < Segments.Count; i++)
			{
				int index = i < SegmentPositions.Count ? i : SegmentPositions.Count - 1;
				Segments[index].position = (Vector3Int)SegmentPositions[index];
			}
		}

		private Vector2Int GetValidFruitLocation()
		{
			Vector2Int randomPosition = Vector2Int.zero;
			bool validPosition = false;

			while (!validPosition)
			{
				randomPosition = new Vector2Int(Random.Range(-22, 22), Random.Range(-12, 12));
				validPosition = !SegmentPositions.Contains(randomPosition);
			}

			return randomPosition;
		}
		#endregion
	}
}