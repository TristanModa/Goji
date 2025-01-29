using System.Collections.Generic;
using UnityEngine;

namespace Goji.Gameplay
{
	public class SnakeController : MonoBehaviour
	{
		#region Properties
		private Vector2Int DesiredMoveDirection { get; set; }
		private Vector2Int PreviousMoveDirection { get; set; }

		private int SnakeMovementTimer { get; set; }
		private int SnakeLength { get; set; }

		private List<Vector2Int> SegmentPositions { get; set; }
		private List<Transform> Segments { get; set; }
		private Vector2Int HeadPosition => SegmentPositions[0];

		private RectInt MapBounds => GameManager.Instance.MapBounds;

		public bool IsDead { get; private set; }

		private Transform Fruit { get; set; }
		#endregion

		#region Fields
		[SerializeField]
		private int defaultSnakeLength;

		[Space(20), SerializeField]
		Vector2Int mapSize;

		[Space(20), SerializeField]
		int snakeMoveRate;

		[Space(20), SerializeField]
		Transform snakeBodySegmentPrefab;
		[SerializeField]
		Transform fruitPrefab;
		#endregion

		#region Methods
		private void Start()
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
				SegmentPositions.Add(new Vector2Int(0, 0));
			}

			// Set the default movement direction
			DesiredMoveDirection = Vector2Int.right;
			PreviousMoveDirection = Vector2Int.zero;

			// Create the fruit and initialize it to a random position
			Fruit = Instantiate(fruitPrefab);
			Fruit.name = "Fruit";
			Fruit.position = (Vector3Int)GetValidFruitLocation();
		}

		private void Update()
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

		private void FixedUpdate()
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
			Vector2Int newHeadPosition = HeadPosition;
			newHeadPosition += DesiredMoveDirection;

			// Wrap the snake's head to the opposite side of the screen if it has left the screen bounds
			if (newHeadPosition.x < MapBounds.xMin)
				newHeadPosition.x = MapBounds.xMax;
			if (newHeadPosition.x > MapBounds.xMax)
				newHeadPosition.x = MapBounds.xMin;
			if (newHeadPosition.y < MapBounds.yMin)
				newHeadPosition.y = MapBounds.yMax;
			if (newHeadPosition.y > MapBounds.yMax)
				newHeadPosition.y = MapBounds.yMin;

			// Set the previous move direction for next frame
			PreviousMoveDirection = DesiredMoveDirection;

			// Add the head position to the beginning of the segment positions list
			SegmentPositions.Insert(0, newHeadPosition);

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
		}

		private void UpdateSegments()
		{
			//if (IsDead)
				//return;

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

			// Failsafe for when the snake takes up the entire board
			if (SnakeLength >= MapBounds.width * MapBounds.height)
				return Vector2Int.zero;

			while (!validPosition)
			{
				randomPosition = 
					new Vector2Int(
						Random.Range(MapBounds.xMin + 1, MapBounds.xMax - 1),
						Random.Range(MapBounds.yMin + 1, MapBounds.yMax - 1));
				validPosition = !SegmentPositions.Contains(randomPosition);
			}

			return randomPosition;
		}
		#endregion
	}
}