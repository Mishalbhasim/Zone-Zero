using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json.Bson;
using TMPro;
using UnityEngine;

public class PlayerListItem : MonoBehaviour
{

    [SerializeField] private TextMeshProUGUI _nameText;
    [SerializeField] private GameObject _masterIcon;


    public void Setup(string playerName, bool isMaster)
    {
        if (_nameText != null)
            _nameText.text = playerName;

        if (_masterIcon != null)
            _masterIcon.SetActive(isMaster);
    }
}
