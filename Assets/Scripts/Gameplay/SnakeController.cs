using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace Goji.Gameplay
{
	public class SnakeController : MonoBehaviour
	{
		#region Properties
		private Vector2Int DesiredMoveDirection { get; set; }

		private int SnakeMovementTimer { get; set; }
		private int SnakeLength { get; set; }

		/// <summary>
		/// The snake's speed in tiles per second
		/// </summary>
		private float SnakeSpeed => 1 / Time.fixedDeltaTime / (snakeMoveRate + 1);

		private List<Vector2Int> SegmentPositions { get; set; }
		private List<Transform> Segments { get; set; }
		private Vector2Int HeadPosition => SegmentPositions[0];
		private Vector2Int PreviousTailPosition { get; set; }

		private Transform FrontSmoothingSegment { get; set; }
		private Transform BackSmoothingSegment { get; set; }
		private Transform SnakeArrow { get; set; }

		private RectInt MapBounds => GameManager.Instance.MapBounds;

		public bool IsDead { get; private set; }

		private Transform Fruit { get; set; }
		private Vector2Int FruitPosition { get; set; }
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
		[SerializeField]
		Transform snakeArrowPrefab;
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

			// Create smoothing segments
			FrontSmoothingSegment = Instantiate(snakeBodySegmentPrefab);
			FrontSmoothingSegment.name = "Front Smoothing Segment";
			FrontSmoothingSegment.parent = this.transform;
			BackSmoothingSegment = Instantiate(snakeBodySegmentPrefab);
			BackSmoothingSegment.name = "Back Smoothing Segment";
			BackSmoothingSegment.parent = this.transform;

			// Create arrow indicator
			SnakeArrow = Instantiate(snakeArrowPrefab);
			SnakeArrow.name = "Snake Arrow";
			SnakeArrow.parent = FrontSmoothingSegment.transform;

			// Set the default movement direction
			DesiredMoveDirection = Vector2Int.right;

			// Create the fruit and initialize it to a random position
			Fruit = Instantiate(fruitPrefab);
			Fruit.name = "Fruit";
			FruitPosition = GetValidFruitLocation();
			Fruit.position = (Vector3Int)FruitPosition;
		}

		private void Update()
		{
			// Update the snake arrow
			UpdateSnakeArrow();

			// Update the desired movement direction
			//if (UnityEngine.Input.GetKeyDown(KeyCode.W))
			//	DesiredMoveDirection = Vector2Int.up;
			//if (UnityEngine.Input.GetKeyDown(KeyCode.S))
			//	DesiredMoveDirection = Vector2Int.down;
			//if (UnityEngine.Input.GetKeyDown(KeyCode.D))
			//	DesiredMoveDirection = Vector2Int.right;
			//if (UnityEngine.Input.GetKeyDown(KeyCode.A))
			//	DesiredMoveDirection = Vector2Int.left;

			// Update smoothing segment positions
			FrontSmoothingSegment.position = 
				Vector2.MoveTowards(
					FrontSmoothingSegment.position, 
					HeadPosition + DesiredMoveDirection, 
					SnakeSpeed * Time.deltaTime);

			BackSmoothingSegment.position =
				Vector2.MoveTowards(
					BackSmoothingSegment.position,
					SegmentPositions[SegmentPositions.Count - 1],
					SnakeSpeed * Time.deltaTime);

			// Update the fruit's position
			Fruit.position = Vector2.Lerp(Fruit.position, FruitPosition, 0.25f);
		}

		private void FixedUpdate()
		{
			if (IsDead)
				return;

			// Move the snake if the timer has elapsed
			if (SnakeMovementTimer > snakeMoveRate && !IsDead)
			{
				PerformMovement();
				CheckCollisions();
				UpdateSegments();

				// Reset movement timer
				SnakeMovementTimer = 0;

				// Determine next movement direction
				DesiredMoveDirection = GetNextMovementDirection();
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

			// Add the head position to the beginning of the segment positions list
			SegmentPositions.Insert(0, newHeadPosition);

			// Get the previous tail position
			PreviousTailPosition = SegmentPositions[SegmentPositions.Count - 1];

			// Remove the final position from the list if the list is longer than the snake's length
			if (SegmentPositions.Count > SnakeLength)
				SegmentPositions.RemoveAt(SegmentPositions.Count - 1);
		}

		private void CheckCollisions()
		{
			// Check if the head position is the same as the fruit
			Vector2Int fruitPosition = FruitPosition;
			if (HeadPosition == fruitPosition)
			{
				// Increase the snake length
				SnakeLength++;

				// Move the fruit
				FruitPosition = GetValidFruitLocation();

				// Play Sound Effect
				AudioManager.PlaySFX("Collect");
			}

			// Check if the head position is the same as any non-head segments
			for (int i = 1; i < SegmentPositions.Count; i++)
			{
				if (HeadPosition == SegmentPositions[i])
				{
					IsDead = true;
					AudioManager.PlaySFX("Death");
				}
			}
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

			// Set smoothing segment positions
			FrontSmoothingSegment.position = (Vector3Int)HeadPosition;
			BackSmoothingSegment.position = (Vector3Int)PreviousTailPosition;
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

		private Vector2Int GetNextMovementDirection() 
		{
			Vector2Int[] directions = { Vector2Int.down, Vector2Int.up, Vector2Int.left, Vector2Int.right };

			Vector2Int bestMove = Vector2Int.zero;
			float shortestDistance = float.MaxValue;

			foreach (Vector2Int dir in directions) 
			{
				Vector2Int newPos = HeadPosition + dir;

				if (SegmentPositions.Contains(newPos))
					continue;

				// Calculate the distance to the fruit in toroidal space
				float dx = Mathf.Abs(newPos.x - FruitPosition.x);
				float dy = Mathf.Abs(newPos.y - FruitPosition.y);

				if (dx > MapBounds.width * 0.5f)
					dx = MapBounds.width - dx;

				if (dy > MapBounds.height * 0.5f)
					dy = MapBounds.height - dy;

				float distance = Mathf.Sqrt(dx * dx + dy * dy);

				if (distance < shortestDistance) 
				{
					shortestDistance = distance;
					bestMove = dir;
				}
			}

			return bestMove;
		}

		private void UpdateSnakeArrow()
		{
			if (DesiredMoveDirection == Vector2Int.up)
				SnakeArrow.rotation = Quaternion.Euler(0, 0, 0);
			else if (DesiredMoveDirection == Vector2Int.right)
				SnakeArrow.rotation = Quaternion.Euler(0, 0, 270);
			else if (DesiredMoveDirection == Vector2Int.down)
				SnakeArrow.rotation = Quaternion.Euler(0, 0, 180);
			else
				SnakeArrow.rotation = Quaternion.Euler(0, 0, 90);
		}
		#endregion
	}
}