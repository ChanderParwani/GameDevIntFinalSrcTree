﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventoryMenu : MonoBehaviour {

	//Components
	private GameManager gm;
	private InventoryTransition transition;

	private Text map_button;
	private Text file_button;
	private Text exit_button;
	
	private Text display_text;
	
	private Image select_menu;
	private Text[] select_buttons;

	private Image jill_portrait;
	private InventorySlotScript equip_slot;
	private InventorySlotScript[] slots;

	private Image[] select_cursor;
	
	//Settings
	private bool can_move;
	private float offset;
	private float text_spd;
	//private bool show_cursor;
	
	//Variables
	private float map_alpha;
	private float file_alpha;
	private float exit_alpha;

	private bool draw_ui_text;
	private float ui_text_timer;
	private string ui_text;

	private bool combine;
	private int combine_index;
	private int combine_select_index;

	private int select_menu_index;
	
	private float sin_val;
	private int select;
	private Vector2 cursor_a;
	private Vector2 cursor_b;
	

	//Initialization
	void Awake () {
		//Transition
		gm = GameManager.instance;
		transition = GetComponent<InventoryTransition>();
		createMenu();

		//Inventory Slots
		slots = new InventorySlotScript[6];
		int i = 0;
		for (int h = 0; h < 3; h++)
		{
			for (int w = 0; w < 2; w++)
			{
				slots[i] = Instantiate(Resources.Load<GameObject>("System/GUI/InventorySlot")).GetComponent<InventorySlotScript>();
				slots[i].transform.SetParent(transform);
				slots[i].transform.localPosition = new Vector3(314 + (w * 224), 176 + (h * -172), 0);
				slots[i].transform.localScale = new Vector3(2.9f, 2.9f, 1);
				i++;
			}
		}
		
		//Equip Slot
		equip_slot = Instantiate(Resources.Load<GameObject>("System/GUI/InventorySlot")).GetComponent<InventorySlotScript>();
		equip_slot.transform.SetParent(transform);
		equip_slot.transform.localPosition = new Vector3(55, -190);
		equip_slot.transform.localScale = new Vector3(2.9f, 2.9f, 1);
		
		//Select Cursor
		select_cursor = new Image[4];
		for (i = 0; i < 4; i++)
		{
			GameObject cursor_obj = new GameObject("cursor_corner" + i, typeof(RectTransform), typeof(Image));
			cursor_obj.transform.SetParent(transform);
			cursor_obj.GetComponent<RectTransform>().sizeDelta = new Vector2(10, 10);
			cursor_obj.transform.localPosition = new Vector3(0, 0, 0);
			cursor_obj.transform.localScale = new Vector3(3, 3, 1);
			select_cursor[i] = cursor_obj.GetComponent<Image>();
			select_cursor[i].sprite = Resources.Load<Sprite>("System/GUI/Cursor/sInventorySelect" + (i + 1));
		}
		
		//Settings
		can_move = true;
		offset = 15f;
		text_spd = 12f;
		
		//Variables
		sin_val = 0f;
		select = 0;

		draw_ui_text = false;
		ui_text_timer = 0;
		ui_text = "";

		select_menu_index = -1;
	}
	
	//Update Event
	void Update () {
		//Draw Sin
		sin_val += Time.deltaTime * 0.7f;
		if (sin_val >= 1)
		{
			sin_val = 0f;
		}
		float draw_sin = (Mathf.Sin(sin_val * 2 * Mathf.PI) + 1) / 2f;
		
		//Debug
		if (Input.GetKeyDown(KeyCode.K))
		{
			gm.inventory.debugScramble();
		}

		//Set Select Position
		map_alpha = 0.6f;
		file_alpha = 0.6f;
		exit_alpha = 0.6f;
		for (int i = 0; i < 6; i++)
		{
			slots[i].hover = false;
			slots[i].select = false;
		}
		select_menu.color = Color.clear;
		for (int q = 0; q < 3; q++)
		{
			select_buttons[q].color = Color.clear;
		}
		
		if (can_move)
		{
			//Exit Inventory
			if (gm.getKeyDown("inventory"))
			{
				if (select_menu_index == -1)
				{
					can_move = false;
					transition.switchTrans();
				}
				else
				{
					select_menu_index = -1;
				}
			}
			
			//Interface
			if (select == 0)
			{
				//Map
				map_alpha = 1f;
				cursor_a = new Vector2(320, 430) + new Vector2(-80, 10);
				cursor_b = new Vector2(320, 430) + new Vector2(80, -10);

				if (gm.getKeyDown("down"))
				{
					select = 4;
					setText(InventoryData.itemName(gm.inventory.inventory[select - 4]));
				}
				else if (gm.getKeyDown("right"))
				{
					select = 1;
				}
			}
			else if (select == 1)
			{
				//File
				file_alpha = 1f;
				cursor_a = new Vector2(550, 430) + new Vector2(-80, 10);
				cursor_b = new Vector2(550, 430) + new Vector2(80, -10);

				if (gm.getKeyDown("down"))
				{
					select = 2;
				}
				else if (gm.getKeyDown("left"))
				{
					select = 0;
				}
			}
			else if (select == 2)
			{
				//Exit
				exit_alpha = 1f;
				cursor_a = new Vector2(550, 340) + new Vector2(-80, 10);
				cursor_b = new Vector2(550, 340) + new Vector2(80, -10);

				if (gm.getKeyDown("down"))
				{
					select = 5;
					setText(InventoryData.itemName(gm.inventory.inventory[select - 4]));
				}
				else if (gm.getKeyDown("up"))
				{
					select = 1;
				}
				else if (gm.getKeyDown("interact"))
				{
					can_move = false;
					transition.switchTrans();
				}
			}
			else if (select == 3)
			{
				//Storage
			}
			else if (select > 3)
			{
				//Inventory
				int inven_select_num = select - 4;
				Vector2 draw_pos = slots[inven_select_num].static_position;
				draw_pos += new Vector2(5, 2);
				cursor_a = draw_pos + new Vector2(-72, 45);
				cursor_b = draw_pos + new Vector2(72, -45);

				if (select_menu_index == -1)
				{
					//Moving around Inventory Slots
					if (gm.getKeyDown("interact"))
					{
						if (gm.inventory.inventory[inven_select_num] != 0)
						{
							select_menu_index = 0;
							if (gm.inventory.inventory[inven_select_num] > 0 &&
							    gm.inventory.inventory[inven_select_num] < 9)
							{
								select_buttons[0].text = "Equip";
							}
							else
							{
								select_buttons[0].text = "Use";
							}
						}
					}
					else if (select == 4)
					{
						if (gm.getKeyDown("up"))
						{
							select = 0;
							setText("");
						}
						else if (gm.getKeyDown("down"))
						{
							select = 6;
							setText(InventoryData.itemName(gm.inventory.inventory[select - 4]));
						}
						else if (gm.getKeyDown("right"))
						{
							select = 5;
							setText(InventoryData.itemName(gm.inventory.inventory[select - 4]));
						}
					}
					else if (select == 5)
					{
						if (gm.getKeyDown("up"))
						{
							select = 2;
							setText("");
						}
						else if (gm.getKeyDown("down"))
						{
							select = 7;
							setText(InventoryData.itemName(gm.inventory.inventory[select - 4]));
						}
						else if (gm.getKeyDown("left"))
						{
							select = 4;
							setText(InventoryData.itemName(gm.inventory.inventory[select - 4]));
						}
					}
					else if (select == 6)
					{
						if (gm.getKeyDown("up"))
						{
							select = 4;
							setText(InventoryData.itemName(gm.inventory.inventory[select - 4]));
						}
						else if (gm.getKeyDown("down"))
						{
							select = 8;
							setText(InventoryData.itemName(gm.inventory.inventory[select - 4]));
						}
						else if (gm.getKeyDown("right"))
						{
							select = 7;
							setText(InventoryData.itemName(gm.inventory.inventory[select - 4]));
						}
					}
					else if (select == 7)
					{
						if (gm.getKeyDown("up"))
						{
							select = 5;
							setText(InventoryData.itemName(gm.inventory.inventory[select - 4]));
						}
						else if (gm.getKeyDown("down"))
						{
							select = 9;
							setText(InventoryData.itemName(gm.inventory.inventory[select - 4]));
						}
						else if (gm.getKeyDown("left"))
						{
							select = 6;
							setText(InventoryData.itemName(gm.inventory.inventory[select - 4]));
						}
					}
					else if (select == 8)
					{
						if (gm.getKeyDown("up"))
						{
							select = 6;
							setText(InventoryData.itemName(gm.inventory.inventory[select - 4]));
						}
						else if (gm.getKeyDown("right"))
						{
							select = 9;
							setText(InventoryData.itemName(gm.inventory.inventory[select - 4]));
						}
					}
					else if (select == 9)
					{
						if (gm.getKeyDown("up"))
						{
							select = 7;
							setText(InventoryData.itemName(gm.inventory.inventory[select - 4]));
						}
						else if (gm.getKeyDown("left"))
						{
							select = 8;
							setText(InventoryData.itemName(gm.inventory.inventory[select - 4]));
						}
					}
				}
				else if (combine)
				{
					
				}
				else
				{
					//Tab Menu Traits
					select_menu.color = Color.white;
					for (int l = 0; l < 3; l++)
					{
						select_buttons[l].color = new Color(1, 1, 1, 0.6f);
					}
					select_buttons[select_menu_index].color = new Color(1, 1, 1, 1f);
					slots[inven_select_num].select = true;
					cursor_a = new Vector2(select_buttons[select_menu_index].transform.localPosition.x, select_buttons[select_menu_index].transform.localPosition.y) + new Vector2(-80, 10);
					cursor_b = new Vector2(select_buttons[select_menu_index].transform.localPosition.x, select_buttons[select_menu_index].transform.localPosition.y) + new Vector2(80, -10);
					
					//Tab Menu
					if (gm.getKeyDown("right"))
					{
						select_menu_index = -1;
					}
					else if (select_menu_index == 0)
					{
						//Use or Equip
						if (gm.getKeyDown("interact"))
						{
							if (select_buttons[0].text == "Equip")
							{
								gm.inventory.changeEquip(inven_select_num);
								select_menu_index = -1;
							}
						}
						else if (gm.getKeyDown("down"))
						{
							select_menu_index = 1;
						}
					}
					else if (select_menu_index == 1)
					{
						//Check
						if (gm.getKeyDown("up"))
						{
							select_menu_index = 0;
						}
						else if (gm.getKeyDown("down"))
						{
							select_menu_index = 2;
						}
					}
					else if (select_menu_index == 2)
					{
						//Combine
						if (gm.getKeyDown("interact"))
						{
							combine = true;
							combine_index = inven_select_num;
							combine_select_index = inven_select_num;
						}	
						else if (gm.getKeyDown("up"))
						{
							select_menu_index = 1;
						}
					}
				}

				//Set Select
				slots[inven_select_num].hover = true;
			}
		}

		//Set Menu Traits
		drawText();
		resetInventoryValues();
		map_button.color = new Color(1, 1, 1, map_alpha);
		file_button.color = new Color(1, 1, 1, file_alpha);
		exit_button.color = new Color(1, 1, 1, exit_alpha);
		setCursor(cursor_a, cursor_b, draw_sin);
	}
	
	//Methods
	private void drawText()
	{
		if (draw_ui_text)
		{
			if (display_text.text != ui_text)
			{
				ui_text_timer += Time.deltaTime * text_spd;
				display_text.text = ui_text.Substring(0, Mathf.RoundToInt(Mathf.Clamp(ui_text_timer, 0, ui_text.Length)));
			}
		}
	}
	
	private void setText(string text)
	{
		if (text == "")
		{
			draw_ui_text = false;
			ui_text_timer = 0;
			ui_text = "";
			display_text.text = "";
		}
		else
		{
			draw_ui_text = true;
			ui_text_timer = 0;
			ui_text = text;
			display_text.text = "";
		}
	}
	
	private void resetInventoryValues()
	{
		if (gm.inventory.player_equip != -1)
		{
			equip_slot.setItemValue(gm.inventory.inventory[gm.inventory.player_equip], gm.inventory.inventory_stock[gm.inventory.player_equip]);
		}

		for (int i = 0; i < 6; i++)
		{
			slots[i].setItemValue(gm.inventory.inventory[i], gm.inventory.inventory_stock[i]);
		}
	}
	
	private void setCursor(Vector2 position_a, Vector2 position_b, float sin)
	{
		float lerp_dis = Time.deltaTime * 5f;
		select_cursor[0].transform.localPosition = Vector2.Lerp(select_cursor[0].transform.localPosition, new Vector2(position_a.x - (sin * offset), position_a.y + (sin * offset)), lerp_dis);
		select_cursor[1].transform.localPosition = Vector2.Lerp(select_cursor[1].transform.localPosition, new Vector2(position_b.x + (sin * offset), position_a.y + (sin * offset)), lerp_dis);
		select_cursor[2].transform.localPosition = Vector2.Lerp(select_cursor[2].transform.localPosition, new Vector2(position_b.x + (sin * offset), position_b.y - (sin * offset)), lerp_dis);
		select_cursor[3].transform.localPosition = Vector2.Lerp(select_cursor[3].transform.localPosition, new Vector2(position_a.x - (sin * offset), position_b.y - (sin * offset)), lerp_dis);
	}

	private void createMenu()
	{
		//Map Button
		GameObject text_obj = new GameObject("map_button", typeof(RectTransform), typeof(Text));
		text_obj.transform.SetParent(transform);
		text_obj.transform.localPosition = new Vector3(320, 430, 0);
		text_obj.transform.localScale = new Vector3(0.2f, 0.2f, 1f);
		text_obj.GetComponent<RectTransform>().sizeDelta = new Vector2(5000, 2000);
		map_button = text_obj.GetComponent<Text>();
		map_button.alignment = TextAnchor.MiddleCenter;
		map_button.font = Resources.Load<Font>("System/GUI/ResTextFont");
		map_button.fontSize = 300;
		map_button.text = "Map";
		
		//File Button
		text_obj = new GameObject("file_button", typeof(RectTransform), typeof(Text));
		text_obj.transform.SetParent(transform);
		text_obj.transform.localPosition = new Vector3(548, 430, 0);
		text_obj.transform.localScale = new Vector3(0.2f, 0.2f, 1f);
		text_obj.GetComponent<RectTransform>().sizeDelta = new Vector2(5000, 2000);
		file_button = text_obj.GetComponent<Text>();
		file_button.alignment = TextAnchor.MiddleCenter;
		file_button.font = Resources.Load<Font>("System/GUI/ResTextFont");
		file_button.fontSize = 300;
		file_button.text = "File";
		
		//Exit Button
		text_obj = new GameObject("exit_button", typeof(RectTransform), typeof(Text));
		text_obj.transform.SetParent(transform);
		text_obj.transform.localPosition = new Vector3(548, 340, 0);
		text_obj.transform.localScale = new Vector3(0.2f, 0.2f, 1f);
		text_obj.GetComponent<RectTransform>().sizeDelta = new Vector2(5000, 2000);
		exit_button = text_obj.GetComponent<Text>();
		exit_button.alignment = TextAnchor.MiddleCenter;
		exit_button.font = Resources.Load<Font>("System/GUI/ResTextFont");
		exit_button.fontSize = 300;
		exit_button.text = "Exit";
		
		//Display Text
		text_obj = new GameObject("display_text", typeof(RectTransform), typeof(Text));
		text_obj.transform.SetParent(transform);
		text_obj.transform.localPosition = new Vector3(0, -380, 0);
		text_obj.transform.localScale = new Vector3(0.2f, 0.2f, 1f);
		text_obj.GetComponent<RectTransform>().sizeDelta = new Vector2(6000, 650);
		display_text = text_obj.GetComponent<Text>();
		display_text.alignment = TextAnchor.UpperLeft;
		display_text.font = Resources.Load<Font>("System/GUI/ResTextFont");
		display_text.fontSize = 300;
		display_text.text = "";
		
		//Jill Portrait
		text_obj = new GameObject("jill_portrait", typeof(RectTransform), typeof(Image));
		text_obj.transform.SetParent(transform);
		text_obj.transform.localPosition = new Vector3(-555, -174, 0);
		text_obj.transform.localScale = new Vector3(1.2f, 1.2f, 1f);
		jill_portrait = text_obj.GetComponent<Image>();
		jill_portrait.sprite = Resources.Load<Sprite>("System/GUI/sJillPortraitLR");
		
		//Select Menu
		text_obj = new GameObject("select_menu", typeof(RectTransform), typeof(Image));
		text_obj.transform.SetParent(transform);
		text_obj.transform.localPosition = new Vector3(12, 290, 0);
		text_obj.transform.localScale = new Vector3(3f, 3f, 1f);
		text_obj.GetComponent<RectTransform>().sizeDelta = new Vector2(80, 96);
		select_menu = text_obj.GetComponent<Image>();
		select_menu.sprite = Resources.Load<Sprite>("System/GUI/sTabMenu");
		select_menu.color = Color.clear;

		select_buttons = new Text[3];
		for (int i = 0; i < 3; i++)
		{
			text_obj = new GameObject("select_button", typeof(RectTransform), typeof(Text));
			text_obj.transform.SetParent(transform);
			text_obj.transform.localPosition = new Vector3(12, 385 - (i * 95), 0);
			text_obj.transform.localScale = new Vector3(0.2f, 0.2f, 1f);
			text_obj.GetComponent<RectTransform>().sizeDelta = new Vector2(5000, 2000);
			select_buttons[i] = text_obj.GetComponent<Text>();
			select_buttons[i].alignment = TextAnchor.MiddleCenter;
			select_buttons[i].font = Resources.Load<Font>("System/GUI/ResTextFont");
			select_buttons[i].fontSize = 300;
			select_buttons[i].color = Color.clear;
		}
		select_buttons[0].text = "Use";
		select_buttons[1].text = "Check";
		select_buttons[2].text = "Combine";
	}
}

public class InventoryTransition : MonoBehaviour
{
	
	//Settings
	private bool trans;
	private bool transflip;
	private bool startflip;
	private float alpha;
	private Image black_screen;
	private Image background;
	private Image inventory_image;

	private InventoryMenu menu;
	private List<GameObject> enemies;

	void Start()
	{
		//Settings
		alpha = 0;
		GameObject black_screen_obj = new GameObject("black_screen", typeof(RectTransform));
		black_screen_obj.transform.SetParent(gameObject.transform);
		black_screen = black_screen_obj.AddComponent<Image>();
		
		Texture2D tex = Resources.Load<Texture2D>("texture2") as Texture2D;
		black_screen.sprite = Sprite.Create(tex, new Rect(64, 0, 64, 64), new Vector2(0.5f, 0.5f));
		
		trans = true;
		startflip = true;
		transflip = false;
		
		black_screen.color = new Color(0, 0, 0, 0);
		black_screen.GetComponent<RectTransform>().localPosition = new Vector3(0, 0, 0);
		black_screen.GetComponent<RectTransform>().localScale = new Vector3(10000, 10000, 1);
		
		//Set Enemies Inactive
		enemies = new List<GameObject>();
		GameObject[] enemy = GameObject.FindGameObjectsWithTag("Enemy");
		for (int i = 0; i < enemy.Length; i++)
		{
			enemies.Add(enemy[i]);
			enemy[i].GetComponent<EnemyBehaviour>().enabled = false;
		}
	}

	void Update()
	{
		if (startflip)
		{
			if (transflip)
			{
				if (trans)
				{
					alpha = Mathf.Lerp(alpha, 0, Time.deltaTime * 5f);
					if (alpha <= 0.05f)
					{
						foreach (GameObject enemy in enemies)
						{
							enemy.GetComponent<EnemyBehaviour>().enabled = true;
						}
						GameObject.FindWithTag("Player").GetComponent<PlayerBehaviour>().canmove = true;
						Destroy(gameObject);
					}
				}
				else
				{
					alpha = Mathf.Lerp(alpha, 1, Time.deltaTime * 5f);
					if (alpha >= 0.95f)
					{
						Destroy(menu);
						foreach (Transform child in transform)
						{
							if (child != black_screen.transform)
							{
								Destroy(child.gameObject);
							}
						}

						trans = true;
						alpha = 1;
					}
				}
			}
			else
			{
				if (trans)
				{
					alpha = Mathf.Lerp(alpha, 1, Time.deltaTime * 5f);
					if (alpha >= 0.95f)
					{
						GameObject background_obj = new GameObject("background", typeof(RectTransform));
						background_obj.transform.SetParent(gameObject.transform);
						background = background_obj.AddComponent<Image>();
						background.sprite = Resources.Load<Sprite>("System/GUI/sInventoryScreenBGColor");
						background.GetComponent<RectTransform>().localScale = new Vector3(50, 50, 1);
						background.GetComponent<RectTransform>().localPosition = new Vector3(0, 0, 0);
						
						GameObject inventory_obj = new GameObject("inventory_image", typeof(RectTransform));
						inventory_obj.transform.SetParent(gameObject.transform);
						inventory_image = inventory_obj.AddComponent<Image>();
						inventory_image.sprite = Resources.Load<Sprite>("System/GUI/sInventoryMenu");
						inventory_image.GetComponent<RectTransform>().sizeDelta = new Vector2(480, 360);
						inventory_image.GetComponent<RectTransform>().localScale = new Vector3(3, 3, 1);
						inventory_image.GetComponent<RectTransform>().localPosition = new Vector3(0, 0, 0);
						
						trans = false;
						alpha = 1;
						menu = gameObject.AddComponent<InventoryMenu>();
					}
				}
				else
				{
					alpha = Mathf.Lerp(alpha, 0, Time.deltaTime * 5f);
					if (alpha <= 0.05f)
					{
						startflip = false;
					}
				}
			}	
		}
		
		black_screen.color = new Color(0, 0, 0, alpha);
		black_screen.transform.SetAsLastSibling();
	}

	public void switchTrans()
	{
		transflip = true;
		startflip = true;
	}
	
}