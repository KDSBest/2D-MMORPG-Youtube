using Assets.Scripts.NPC;
using Assets.Scripts.PubSubEvents.Dialog;
using Assets.Scripts.PubSubEvents.MapClient;
using Assets.Scripts.PubSubEvents.StartUI;
using Common.GameDesign;
using Common.IoC;
using Common.Protocol.Combat;
using Common.Protocol.Map;
using Common.PublishSubscribe;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Character
{
	public class PlayerController : MonoBehaviour
	{
		public Rigidbody2D Rigidbody2D;
		public float Speed = 20;
		public Transform MirrorTransform;
		public CircleCollider2D FloorCollider;
		public LayerMask FloorLayer;
		public float JumpVelocity = 35;
		public Animator Animator;
		public float GravityDefault = 5;
		public float GravityFall = 15;
		public List<PlayerSkill> Skills = new List<PlayerSkill>() { new PlayerSkill(SkillCastType.Fireball), new PlayerSkill(SkillCastType.LightningBolt) };
		public LayerMask NPCLayerMask;
		public Vector2? ForcePosition = null;

		private ContactFilter2D floorFilter;
		private NPCController currentNPC;
		private ICurrentContext context;
		private PlayerControls controls;
		private PlayerControls.MovementActions movementActions;
		private PlayerControls.SkillsActions skillActions;
		private PlayerControls.UIsActions uiActions;
		private Vector2 movementVector = Vector2.zero;
		private IPubSub pubsub;

		public void Awake()
		{
			DILoader.Initialize();

			pubsub = DI.Instance.Resolve<IPubSub>();
			pubsub.Subscribe<PlayerControlEnable>(OnPlayerControlEnable, this.GetType().Name);
			pubsub.Subscribe<DialogDone>(OnDialogDone, this.GetType().Name);
			pubsub.Subscribe<PlayerStateMessage>(OnPlayerState, this.GetType().Name);

			context = DI.Instance.Resolve<ICurrentContext>();
			context.PlayerController = this;

			floorFilter = new ContactFilter2D();
			floorFilter.useLayerMask = true;
			floorFilter.layerMask = FloorLayer;

			controls = new PlayerControls();
			
			movementActions = controls.Movement;
			movementActions.Run.performed += ctx => movementVector = ctx.ReadValue<Vector2>();

			skillActions = controls.Skills;
			skillActions.CastQ.performed += ctx => CastSkill(0);
			skillActions.CastE.performed += ctx => CastSkill(1);
			skillActions.InteractNPC.performed += ctx => InteractNPC();

			uiActions = controls.UIs;
			uiActions.ToggleInventory.performed += ctx => pubsub.Publish<ToggleInventoryScreen>(new ToggleInventoryScreen());
		}

		private void OnPlayerState(PlayerStateMessage data)
		{
			if (!data.ForcePosition)
				return;

			ForcePosition = new Vector2(data.Position.X, data.Position.Y);
			LookDirection(data.IsLookingRight);
		}

		private void OnDialogDone(DialogDone data)
		{
			controls.Enable();
		}

		private void InteractNPC()
		{
			if(currentNPC != null)
			{
				controls.Disable();
				currentNPC.OnInteract();
			}
		}

		private void OnPlayerControlEnable(PlayerControlEnable data)
		{
			if (data.Enabled)
				controls.Enable();
			else
				controls.Disable();
		}

		private void FixedUpdate()
		{
			foreach (var skill in Skills)
			{
				skill.Update((int) (Time.fixedDeltaTime * 1000.0f));
			}

			bool isGrounded = false;
			List<Collider2D> collisionResults = new List<Collider2D>();
			if (FloorCollider.OverlapCollider(floorFilter, collisionResults) > 0)
			{
				isGrounded = true;
			}

			bool isMovingRightLeft = HandleMoving();

			HandleJump(isGrounded);

			HandleAnimation(isGrounded, isMovingRightLeft);

			HandleNPC();

			if(ForcePosition != null)
			{
				Rigidbody2D.velocity = Vector2.zero;
				Rigidbody2D.position = ForcePosition.Value;
				ForcePosition = null;
			}

			PlayerState state = new PlayerState()
			{
				Animation = Animator.GetInteger(Constants.AnimationStateName),
				IsLookingRight = MirrorTransform.localScale.x > 0,
				Position = new System.Numerics.Vector2(this.transform.position.x, this.transform.position.y)
			};
			pubsub.Publish(state);
		}

		public void SetForcePosition(Vector2 pos)
		{
			ForcePosition = pos;
		}

		private bool HandleMoving()
		{
			if (movementVector.x < 0)
			{
				LookDirection(false);
				Rigidbody2D.velocity = new Vector2(-Speed, Rigidbody2D.velocity.y);
				return true;
			}
			else if (movementVector.x > 0)
			{
				LookDirection(true);
				Rigidbody2D.velocity = new Vector2(Speed, Rigidbody2D.velocity.y);
				return true;
			}

			Rigidbody2D.velocity = new Vector2(0, Rigidbody2D.velocity.y);
			return false;
		}

		private void LookDirection(bool isRight)
		{
			float xScaleAbs = Math.Abs(MirrorTransform.localScale.x);

			if (isRight)
				MirrorTransform.localScale = new Vector3(xScaleAbs, MirrorTransform.localScale.y, MirrorTransform.localScale.z);
			else
				MirrorTransform.localScale = new Vector3(-xScaleAbs, MirrorTransform.localScale.y, MirrorTransform.localScale.z);
		}

		private void HandleJump(bool isGrounded)
		{
			if (isGrounded && movementVector.y > 0.5f)
			{
				Rigidbody2D.velocity = new Vector2(Rigidbody2D.velocity.x, JumpVelocity);
			}

			if (isGrounded || Rigidbody2D.velocity.y >= 0)
			{
				Rigidbody2D.gravityScale = GravityDefault;
			}
			else
			{
				Rigidbody2D.gravityScale = GravityFall;
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

		private void HandleNPC()
		{
			var hit = Physics2D.Raycast(this.transform.position, MirrorTransform.localScale.x > 0 ? Vector3.right : Vector3.left, 10, NPCLayerMask);
			if (hit.collider == null)
			{
				DeselectCurrentNPC();
				return;
			}

			var npcController = hit.collider.gameObject.GetComponent<NPCController>();
			if (npcController == null)
				return;

			if (npcController == currentNPC)
				return;

			DeselectCurrentNPC();
			currentNPC = npcController;
			currentNPC.OnSelected(true);

		}

		private void DeselectCurrentNPC()
		{
			if (currentNPC != null)
			{
				currentNPC.OnSelected(false);
				currentNPC = null;
			}
		}

		public void CastSkill(int index)
		{
			Skills[index].Cast(this.transform.position, MirrorTransform.localScale.x > 0 ? Vector3.right : Vector3.left);
		}
	}
}
