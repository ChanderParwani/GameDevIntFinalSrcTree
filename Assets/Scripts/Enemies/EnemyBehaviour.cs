﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class EnemyBehaviour : MonoBehaviour {

	//Components
	private Rigidbody rb;
	private Pathfinding path;
	private PlayerBehaviour player;
	private HealthScript hs;
	private EnemyAnimationScript anim;
	
	//Settings
	[SerializeField] private int health;
	
	[SerializeField] private float spd;
	[SerializeField] private float turn_spd;
	[SerializeField] private bool alert;
	[SerializeField] private float aware_radius;

	[SerializeField] private float attack_delay;
	
	[SerializeField] private bool is_a_dog;
	[SerializeField] private AudioClip dog_sfx;
	
	//Variables
	public bool dead { private get; set; }
	private bool can_move;
	private bool lock_on;

	private Vector2 velocity;

	private bool moving;
	private int path_index;
	private Vector2[] path_array;
	private Vector2 target_position;

	private bool attacking;
	private float attack_time;

	private bool play_sound;
	
	//Init Enemy
	void Awake()
	{
		gameObject.tag = "Enemy";
	}
	
	void Start () {
		//Components
		rb = gameObject.GetComponent<Rigidbody>();
		rb.constraints = RigidbodyConstraints.FreezeRotation;
		path = gameObject.AddComponent<Pathfinding>();
		player = GameObject.FindWithTag("Player").GetComponent<PlayerBehaviour>();
		anim = gameObject.GetComponent<EnemyAnimationScript>();
		anim.Play("idle");

		if (!canFindPathPlayer())
		{
			Destroy(gameObject);
		}

		hs = gameObject.AddComponent<HealthScript>();
		hs.damage(-health);
		
		//Settings
		
		//Variables
		dead = false;
		can_move = true;
		lock_on = false;

		velocity = Vector2.zero;

		path_index = 0;
		path_array = null;
		target_position = new Vector2(transform.position.x, transform.position.z);
		play_sound = false;
	}
	
	//Update Event
	void Update () {
		//Dead
		if (dead)
		{
			anim.Play("dead");
			return;
		}

		if (!play_sound)
		{
			play_sound = true;
			if (is_a_dog)
			{
				AudioSource compo = gameObject.AddComponent<AudioSource>();
				compo.loop = true;
				compo.clip = dog_sfx;
				if (alert)
				{
					compo.Play();
				}
			}
		}

		if (!player.canattack)
		{
			anim.Play("idle");
			velocity = Vector2.zero;
			return;
		}
		
		//Movement & Path Finding
		if (can_move)
		{
			//Alert Checking
			if (!alert)
			{
				//Check if player is within range of enemy seeing the player
				if (Vector3.Distance(transform.position, player.gameObject.transform.position) < aware_radius)
				{
					alert = true;
					if (is_a_dog)
					{
						AudioSource compo = GetComponent<AudioSource>();
						compo.Play();
					}
				}
			}
			else
			{
				//Set path for player
				if (!moving)
				{
					anim.Play("idle");
					if (attacking)
					{
						attack_time -= Time.deltaTime;
						if (attack_time <= 0)
						{
							if (Vector2.Distance(new Vector2(transform.position.x, transform.position.z), new Vector2(player.transform.position.x, player.transform.position.z)) <= 5f)
							{
								GameManager.instance.inventory.heal(GameManager.instance.inventory.health - 1);
								player.hurt();
							}

							attacking = false;
							setPath(player.transform.position);
						}
					}
					else
					{
						setPath(player.transform.position);
					}
				}
				else
				{
					
					anim.Play("walk");
					if (Vector2.Distance(new Vector2(transform.position.x, transform.position.z), new Vector2(player.transform.position.x, player.transform.position.z)) > 3f)
					{
						if (Vector2.Distance(target_position, new Vector2(player.transform.position.x, player.transform.position.z)) > 1f)
						{
							setPath(player.transform.position);
						}
					}
					else
					{
						moving = false;
						attacking = true;
						attack_time = attack_delay;
					}
				}
			}
			
			//PathFinding
			velocity = Vector2.zero;
			
			if (moving)
			{
				if (path_array != null)
				{
					if (path_index < path_array.Length)
					{
						//Variables
						Vector2 temp_current_position = new Vector2(transform.position.x, transform.position.z);
						Vector2 temp_target_position = path_array[path_index];
						float move_angle = pointAngle(temp_current_position, temp_target_position);
						
						//Rotate in the direction of the Target
						if (!path.checkPoints(transform.position, player.gameObject.transform.position))
						{
							move_angle = pointAngle(temp_current_position, new Vector3(player.transform.position.x, player.transform.position.z));
							if (lock_on)
							{
								transform.eulerAngles = new Vector3(transform.eulerAngles.x, move_angle, transform.eulerAngles.z);
							}
							else
							{
								if (GridBehaviour.instance.nodeFromPosition(transform.position).empty)
								{
									if (Mathf.Abs(Mathf.DeltaAngle(transform.eulerAngles.y, move_angle)) < 10)
									{
										lock_on = true;
									}

									transform.eulerAngles = new Vector3(transform.eulerAngles.x, Mathf.MoveTowardsAngle(transform.eulerAngles.y, move_angle, turn_spd), transform.eulerAngles.z);
								}
								else
								{
									if (Vector2.Distance(temp_current_position, temp_target_position) > 0.05f) {
										transform.eulerAngles = new Vector3(transform.eulerAngles.x, Mathf.MoveTowardsAngle(transform.eulerAngles.y, move_angle, turn_spd), transform.eulerAngles.z);
									}
								}
							}
						}
						else
						{
							lock_on = false;
							if (Vector2.Distance(temp_current_position, temp_target_position) > 0.05f) {
								transform.eulerAngles = new Vector3(transform.eulerAngles.x, Mathf.MoveTowardsAngle(transform.eulerAngles.y, move_angle, turn_spd), transform.eulerAngles.z);
							}
						}
						
						//Move towards Target
						float temp_angle = (-transform.eulerAngles.y + 90) * Mathf.Deg2Rad;
						float angle_spd_modify = Mathf.Pow(Mathf.Clamp(1 - (Mathf.Abs(Mathf.DeltaAngle(transform.eulerAngles.y, move_angle)) / 30), 0, 1), 1);
						velocity = new Vector2(Mathf.Cos(temp_angle), Mathf.Sin(temp_angle)) * (spd * angle_spd_modify);

						//Check if near target
						if (Vector2.Distance(temp_current_position, temp_target_position) < 0.2f)
						{
							path_index++;
							FixedUpdate();
							Update();
						}
					}
					else
					{
						moving = false;
						path_array = null;
					}
				}
			}
			
		}
		else
		{
			
		}
	}

	//Physics
	void FixedUpdate()
	{
		if (velocity != Vector2.zero)
		{
			rb.MovePosition(transform.position + new Vector3(velocity.x, 0, velocity.y));
		}
	}

	//Methods
	private bool canFindPathPlayer()
	{
		Vector3 target_pos = player.transform.position;
		target_position = new Vector2(target_pos.x, target_pos.z);
		path_array = path.findPathArray(transform.position, target_pos);

		if (path_array == null)
		{
			return false;
		}
		return true;
	}
	
	private void setPath(Vector3 target_pos)
	{
		moving = true;
		target_position = new Vector2(target_pos.x, target_pos.z);
		path_index = 0;
		path_array = path.findPathArray(transform.position, target_pos);

		if (path_array.Length > 1)
		{
			if (Vector2.Distance(path_array[0], new Vector2(transform.position.x, transform.position.z)) < 2.5f)
			{
				path_index = 1;
			}
		}
	}
	
	private float pointAngle(Vector2 pointA, Vector2 pointB)
	{
		return -((Mathf.Atan2(pointB.y - pointA.y, pointB.x - pointA.x) * 180) / Mathf.PI) + 90;
	}
	
	//Gizmos & Debug
	void OnDrawGizmos()
	{
		Gizmos.color = Color.blue;
		float first_point = 0;
		for (float i = 5; i < 360; i += 5f)
		{
			float second_point = i;
			Vector3 point_a = new Vector3(Mathf.Cos(first_point * Mathf.Deg2Rad), 0, Mathf.Sin(first_point * Mathf.Deg2Rad)) * aware_radius;
			Vector3 point_b = new Vector3(Mathf.Cos(second_point * Mathf.Deg2Rad), 0, Mathf.Sin(second_point * Mathf.Deg2Rad)) * aware_radius;
			Gizmos.DrawLine(transform.position + point_a, transform.position + point_b);
			first_point = second_point;
		}

		if (path_array != null)
		{
			for (int i = 0; i < path_array.Length; i++)
			{
				Gizmos.color = Color.magenta;
				if (path_index < i)
				{
					Gizmos.color = Color.blue;
				}

				Gizmos.DrawSphere(new Vector3(path_array[i].x, GridBehaviour.instance.transform.position.y, path_array[i].y), 0.5f);
				if (i < path_array.Length - 1)
				{
					Gizmos.DrawLine(new Vector3(path_array[i].x, GridBehaviour.instance.transform.position.y, path_array[i].y), new Vector3(path_array[i + 1].x, GridBehaviour.instance.transform.position.y, path_array[i + 1].y));
				}
			}
		}
	}
}
