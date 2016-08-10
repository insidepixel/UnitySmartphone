using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class SoundButtonController : MonoBehaviour 
{
    private string _name;
    private bool _unlocked;
    private AudioClip _soundFile;
	
    public void TryToPlay()
    {
        if(this.Unlocked)
            Camera.main.GetComponent<PhoneController>().PlaySpeakerSound(_soundFile);
    }

    public string Name
    {
        get { return _name; }
        set { _name = value; }
    }

    public bool Unlocked
    {
        get { return _unlocked; }
        set { _unlocked = value; }
    }

    public AudioClip SoundFile
    {
        get { return _soundFile; }
        set { _soundFile = value; }
    }

    public void SetUp()
    {
        transform.GetComponentInChildren<Text>().text = _unlocked ? _name : "? ? ? ? ? ?";
    }

    public void Unlock()
    {
        GetComponentInChildren<Text>().text = _name;
        _unlocked = true;
    }
}