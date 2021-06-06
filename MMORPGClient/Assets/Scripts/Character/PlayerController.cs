using Assets.Scripts.PubSubEvents.MapClient;
using Common.IoC;
using Common.PublishSubscribe;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Character
{
	public class PlayerController : MonoBehaviour
	{
		private PlayerControls controls;
		private PlayerControls.MovementActions movementActions;
		private Vector2 movementVector = Vector2.zero;
		private IPubSub pubsub;
		public Rigidbody2D Rigidbody2D;
		public float Speed = 20;
		public Transform BodyRenderer;
		public CircleCollider2D FloorCollider;
		public LayerMask FloorLayer;
		private ContactFilter2D floorFilter;
		public float JumpVelocity = 10;
		public Animator Animator;

		public void Awake()
		{
			DILoader.Initialize();

			floorFilter = new ContactFilter2D();
			floorFilter.useLayerMask = true;
			floorFilter.layerMask = FloorLayer;

			controls = new PlayerControls();
			movementActions = controls.Movement;
			movementActions.Run.performed += ctx => movementVector = ctx.ReadValue<Vector2>();
			pubsub = DI.Instance.Resolve<IPubSub>();
			pubsub.Subscribe<PlayerControlEnable>(OnPlayerControlEnable, this.name);
		}

		private void OnPlayerControlEnable(PlayerControlEnable data)
		{
			if(data.Enabled)
				controls.Enable();
			else
				controls.Disable();
		}

		private void FixedUpdate()
		{
			bool isGrounded = false;
			List<Collider2D> collisionResults = new List<Collider2D>();
			if (FloorCollider.OverlapCollider(floorFilter, collisionResults) > 0)
			{
				isGrounded = true;
			}

			bool isMovingRightLeft = HandleMoving();

			HandleJump(isGrounded);

			HandleAnimation(isGrounded, isMovingRightLeft);

			PlayerState state = new PlayerState()
			{
				Animation = Animator.GetInteger(Constants.AnimationStateName),
				IsLookingRight = BodyRenderer.localScale.x > 0,
				Position = new System.Numerics.Vector2(this.transform.position.x, this.transform.position.y)
			};
			pubsub.Publish(state);
		}

		private bool HandleMoving()
		{
			float xScaleAbs = Math.Abs(BodyRenderer.localScale.x);
			if (movementVector.x < 0)
			{
				BodyRenderer.localScale = new Vector3(-xScaleAbs, BodyRenderer.localScale.y, BodyRenderer.localScale.z);
				Rigidbody2D.velocity = new Vector2(-Speed, Rigidbody2D.velocity.y);
				return true;
			}
			else if (movementVector.x > 0)
			{
				BodyRenderer.localScale = new Vector3(xScaleAbs, BodyRenderer.localScale.y, BodyRenderer.localScale.z);
				Rigidbody2D.velocity = new Vector2(Speed, Rigidbody2D.velocity.y);
				return true;
			}

			Rigidbody2D.velocity = new Vector2(0, Rigidbody2D.velocity.y);
			return false;
		}

		private void HandleJump(bool isGrounded)
		{
			if (isGrounded && movementVector.y > 0.5f)
			{
				Rigidbody2D.velocity = new Vector2(Rigidbody2D.velocity.x, JumpVelocity);
			}
		}

		private void HandleAnimation(bool isGrounded, bool isMovingRightLeft)
		{
			if (!isGrounded)
			{
				Animator.SetInteger(Constants.AnimationStateName, 2);
			}
			else if (isGrounded)
			{
				if (isMovingRightLeft)
				{
					Animator.SetInteger(Constants.AnimationStateName, 1);
				}
				else
				{
					Animator.SetInteger(Constants.AnimationStateName, 0);
				}
			}
		}
	}
}
