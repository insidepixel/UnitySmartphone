using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Linq;

public class PhoneController : MonoBehaviour 
{
	//Various parts of the phone, references save us having to search for them later
	public GameObject phone;
	public AudioSource speaker;
	public Text dateTimeText;
	public GameObject soundMenuContent;
	public GameObject soundButton;
	public bool isActive = false;
	public bool isCalling = false;
	public int phoneMenu = 0;
    public int creditsMenu = 0;
	public GameObject[] menus;
    public GameObject[] creditsPanels;
	public Sound[] sounds;
	public Contact[] contacts;
	public GameObject phoneInput;
	public GameObject currentContact;
    public Contact selectedContact;
	public GameObject contactButton;
	public Sprite defaultContactPic;
	public Speaker[] speakers;
    public GameObject contactMenuContent;
    public GameObject phonePanel;
    public GameObject contactPanel;

	void Start() 
	{
        //Menus must be populated and then disabled before the round starts
		PopulateSoundList();
        PopulateContactList();
		foreach (GameObject go in menus)
        {
            go.SetActive(false);
        }
        ChangeMenu(0);
        contactPanel.SetActive(false);
		phone.SetActive(false);
	}
	
	void Update () 
	{
        //Replace this with Input axis later on when all controls are sorted
		if(Input.GetKeyDown("i") && !isCalling)
		{
			phone.SetActive(!isActive);
			isActive = !isActive;
		}
		DateTime dt = DateTime.Now;
		dateTimeText.text = dt.ToShortTimeString() + "\n" + dt.ToShortDateString();
	}
	
	public void ChangeMenu(int index)
	{
        //Disable menus based on button selection, all main menu buttons and back buttons have this event
        menus[phoneMenu].SetActive(false);
		menus[index].SetActive(true);
        phoneMenu = index;
	}

    public void ChangeCreditsPage(int index)
    {
        //Switch out panels for credits menu on button select
        creditsPanels[creditsMenu].SetActive(false);
        creditsPanels[index].SetActive(true);
        creditsMenu = index;
    }
	
	public void PopulateSoundList()
	{
        //Generating the sound menu content
		RectTransform rectT = soundMenuContent.GetComponent<RectTransform>();
        //If there are less than or equal to 8 sound buttons, the bottom offset is 0
        //Otherwise, the offset needs to be increased by 100 for each extra button
		float bottomOffset = sounds.Length > 8 ? (sounds.Length - 8) * 100 : 0;
		rectT.offsetMin = new Vector2(rectT.offsetMin.x, -bottomOffset);
		float btnHeight = 85F;
		float btnPadding = 15F;
		float btnYBase = -42.5F;
		for(int i = 0; i < sounds.Length; i++)
		{
            //Generate and parent soundbuttons to sound menu content gameobject
			GameObject btn = Instantiate(soundButton) as GameObject;
			btn.transform.SetParent(soundMenuContent.transform, false);
            //Calculate and apply anchoredPosition, as this is a local position relative to parent and anchor
			RectTransform bRect = btn.GetComponent<RectTransform>();
			float yPos = (i * -(btnHeight + btnPadding) + btnYBase);
			bRect.anchoredPosition = new Vector2(0, yPos);
            //Set up the actual sound handling part of the buttons
            //passing in information from the sounds array
            SoundButtonController sbc = btn.GetComponent<SoundButtonController>();
            sbc.Name = sounds[i].displayName;
            sbc.SoundFile = sounds[i].soundFile;
            sbc.Unlocked = sounds[i].unlocked;
            sbc.SetUp();
            //Finally, apply the onclick event to the button itself
            Button b = btn.GetComponent<Button>();
			b.onClick.AddListener(() => sbc.TryToPlay());
		}
	}

    public void PopulateContactList()
    {
        //Similar to sounds menu, calculations based on button size
        RectTransform rectT = contactMenuContent.GetComponent<RectTransform>();
        float bottomOffset = contacts.Length > 4 ? (contacts.Length - 4) * 200 : 0;
        rectT.offsetMin = new Vector2(rectT.offsetMin.x, -bottomOffset);
        float btnHeight = 200F;
        float btnPadding = 0F;
        float btnYBase = -100F;
        //Sort all contacts alphabetically during population
        //This prevents having to mess around with data inside the contacts array in the editor
        var sortedContacts = from s in contacts
                                    orderby s.name ascending
                                    select s;
        int count = 0;
        //Had to use foreach here, since Linq didn't want to play nice
        //count is used to track the button index for placement calculations
        foreach(Contact s in sortedContacts)
        {
            GameObject btn = Instantiate(contactButton) as GameObject;
            btn.transform.SetParent(contactMenuContent.transform, false);
            RectTransform bRect = btn.GetComponent<RectTransform>();
            float yPos = (count * -(btnHeight + btnPadding) + btnYBase);
            bRect.anchoredPosition = new Vector2(0, yPos);
            btn.transform.GetChild(1).GetComponent<Image>().sprite = s.image;
            btn.GetComponentInChildren<Text>().text = string.Format("{0}\n{1}", s.name, s.number);
            string pNumber = s.number;
            Button b = btn.GetComponent<Button>();
            b.onClick.AddListener(() => SelectContact(pNumber, b));
            count++;
        }
    }

    public void SelectContact(string number, Button b)
    {
        //Keep a reference to the most recently selected contact, searching for it by number of button
        string num = number;
        selectedContact = (from con in contacts
                            where con.number == num
                            select con).First();
    }

    public void UnlockSound(string soundName)
    {
        //Somewhat inefficient, simply finds button with name of sound passed in
        //and then unlocked it if one is found
        GameObject btn = null;
        for(int i = 0; i < sounds.Length; i++)
        {
            if(soundMenuContent.transform.GetChild(i).GetComponent<SoundButtonController>().Name == soundName)
            {
                btn = soundMenuContent.transform.GetChild(i).gameObject;
                menus[1].SetActive(true);
                btn.GetComponent<SoundButtonController>().Unlock();
                menus[1].SetActive(false);
                return;
            }
        }
    }
	
	public void PlaySpeakerSound(AudioClip clip)
	{
        //Play soundboard sound through each speaker object that is active
		foreach(Speaker s in speakers)
		{
			if(s.isActive)
			{
				AudioSource a = s.speakerObj.GetComponent<AudioSource>();
				a.Stop();
				a.PlayOneShot(clip);
			}
		}
	}

	public void PlayPhoneSound(AudioClip clip)
	{
		speaker.Stop();
		speaker.PlayOneShot(clip);
	}

	public void HandleKeyInput(int key)
	{
        //Dealing with input to the phone keypad
		Text inputField = phoneInput.GetComponent<Text>();
        //Keys that are not numbers are given key values of 10-13
        //if key is less than 10, we can just input the number
		if(key < 10)
		{
			AddToText(key.ToString());
		}
		else
		{
			switch(key)
			{
			case 10:
				AddToText("*");
				break;
			case 11:
				AddToText("#");
				break;
            //Backspace button, simply chopping off the last character and then calling DisplayContact()
			case 12:
				if(inputField.text.Length > 0)
				{
					inputField.text = inputField.text.Substring(0, inputField.text.Length-1);
					DisplayContact();
				}
				break;
			case 13:
				AttemptCall();
				break;
			}
		}
	}

    //This actually adds the input text to the phone label, checking for max length
	public void AddToText(string message)
	{
		if(phoneInput.GetComponent<Text>().text.Length <= 11)
		{
			phoneInput.GetComponent<Text>().text += message;
		}
		DisplayContact();
	}

	public void DisplayContact()
	{
        //Delete current gameobject display button if one exists
		if(currentContact != null)
		{
			GameObject.Destroy(currentContact);
            selectedContact = null;
		}
        //Just in case this was called accidentally
		if(phoneInput.GetComponent<Text>().text.Length > 0)
		{
            //Find contacts that have a number containing the current input
            //Order by ascending so we receive the first alphabetical result
			var c = from con in contacts
				    where con.number.Contains(phoneInput.GetComponent<Text>().text)
					orderby con.name ascending
					select con;
			if(c.Count() > 0)
			{
                //Make and set up contact button
				currentContact = Instantiate(contactButton) as GameObject;
				currentContact.transform.SetParent(menus[2].transform, false);
                RectTransform cRect = currentContact.GetComponent<RectTransform>();
                cRect.anchoredPosition = new Vector2(0, -400);
				currentContact.GetComponentInChildren<Text>().text = string.Format("{0}\n{1}", c.First().name, c.First().number);
				if(c.First().image != null)
				{
					currentContact.transform.GetChild(1).GetComponent<Image>().sprite = c.First().image;
				}
				else
				{
					currentContact.transform.GetChild(1).GetComponent<Image>().sprite = defaultContactPic;
				}
                //Set up selected contact for calling
                selectedContact = c.First();
			}
		}
	}

	public void UpdateSpeaker(int index)
	{
        //Turn the speaker on or off on the soundboard menu
		Speaker s = speakers[index];
		if (s.speakerObj != null) 
		{
			s.isActive = !s.isActive;
			s.button.GetComponent<Image>().color = s.isActive ? Color.green : Color.white;
		} 
		else 
		{
			s.isActive = false;
			s.button.GetComponent<Image>().color = Color.red;
		}
	}

	public void AttemptCall()
	{
        //This is where handling of hidden content unlocks will be placed
        //If no contact exists the input is checked for achievement numbers
        //Otherwise the conversation begins
		if (selectedContact == null) 
		{
			string inputNumber = phoneInput.GetComponent<Text> ().text;
			switch (inputNumber) 
			{
				case "69*":
					Debug.Log ("Test achievement");
					break;
			}
		}
		else
		{
            GetComponent<ConversationManager>().StartConversation(selectedContact);
		}
	}

    public void DisplayContactsPanel(int index)
    {
        //Switch between keypad and contacts menu
        if (index == 0)
        {
            phonePanel.SetActive(true);
            contactPanel.SetActive(false);
        } 
        else
        {
            phonePanel.SetActive(false);
            contactPanel.SetActive(true);
        }
    }

    public void TryCall()
    {
        //Start conversation with ConversationManager
        GetComponent<ConversationManager>().StartConversation(selectedContact);
    }
}

[System.Serializable]
public class Sound
{
	public string displayName;
	public AudioClip soundFile;
	public bool unlocked;
}

[System.Serializable]
public class Contact
{
	public string name;
	public string number;
	public Sprite image;
    public TextAsset conversation;
}

[System.Serializable]
public class Speaker
{
	public GameObject speakerObj;
	public bool isActive;
	public GameObject button;
}