﻿using System.Collections.Generic;
using Fizz;
using Fizz.Chat;
using Fizz.Common;
using UnityEngine;
using UnityEngine.UI;

public class StatusNode : MonoBehaviour
{
    [SerializeField] Text statusLabel;

    [SerializeField] Button happyButton;
    [SerializeField] Button sadButton;
    [SerializeField] Button angryButton;
    [SerializeField] Button boredButton;
    [SerializeField] Button deleteButton;

    [SerializeField] RectTransform actionsNode;

    private FizzChannelMessage _message;

    public void SetData(string userId, FizzChannelMessage message)
    {
        _message = message;
        if (message != null && message.Data != null)
        {
            string status = string.Empty;
            if (message.Data.TryGetValue(CustomCellSample.KEY_DATA_STATUS, out status))
            {
                statusLabel.text = status;
            }

            actionsNode.gameObject.SetActive(_message.From.Equals(userId));
        }
    }

    private void OnEnable()
    {
        happyButton.onClick.AddListener(OnHappyButtonPressed);
        sadButton.onClick.AddListener(OnSadButtonPressed);
        angryButton.onClick.AddListener(OnAngryButtonPressed);
        boredButton.onClick.AddListener(OnBoredButtonPressed);
        deleteButton.onClick.AddListener(OnDeleteButtonPressed);
    }

    private void OnDisable()
    {
        happyButton.onClick.RemoveListener(OnHappyButtonPressed);
        sadButton.onClick.RemoveListener(OnSadButtonPressed);
        angryButton.onClick.RemoveListener(OnAngryButtonPressed);
        boredButton.onClick.RemoveListener(OnBoredButtonPressed);
        deleteButton.onClick.RemoveListener(OnDeleteButtonPressed);
    }

    private void OnHappyButtonPressed()
    {
        UpdateStatus("HAPPY");
    }

    private void OnSadButtonPressed()
    {
        UpdateStatus("SAD");
    }

    private void OnAngryButtonPressed()
    {
        UpdateStatus("ANGRY");
    }

    private void OnBoredButtonPressed()
    {
        UpdateStatus("BORED");
    }

    private void OnDeleteButtonPressed()
    {
        if (_message == null)
            return;

        FizzService.Instance.DeleteMessage(_message.To, _message.Id, ex =>
        {
            if (ex == null)
            {
                FizzLogger.D("Status Deleted");
            }
        });
    }

    private void UpdateStatus(string status)
    {
        if (_message == null)
            return;

        Dictionary<string, string> data = _message.Data;
        data[CustomCellSample.KEY_DATA_STATUS] = status;
        FizzService.Instance.UpdateMessage(_message.To, _message.Id, _message.Nick, _message.Body, data, true, true, ex =>
        {
            if (ex == null)
            {
                FizzLogger.D("Status Updated");
            }
        });
    }
}
