﻿using UnityEngine;

public enum Interaction { None = 0, PickUp, Cook, Talk };

public class InteractionManager : MonoBehaviour
{
	private Interaction _potentialInteraction;
	[HideInInspector] public Interaction currentInteraction;
	[SerializeField] private InputReader _inputReader = default;
	//To store the object we are currently interacting with
	private GameObject _currentInteractableObject = null;
	//Or do we want to have stg specific for every type of interaction like:
	//Item for pickup?
	//Character (or other relevant type) for talk?

	//Events for the different interaction types
	[Header("Broadcasting on")]
	[SerializeField] private GameObjectEventChannelSO _onObjectPickUp = default;
	//double check with the action name we will show on the UI (because we will not really starting cooking but showing the UI?)
	[SerializeField] private VoidEventChannelSO _onCookingStart = default;
	[SerializeField] private DialogueEventChannelSo _startTalking = default;
	//UI event
	[SerializeField] private InteractionUIEventChannelSO _toggleInteractionUI = default;
	[Header("Listening to")]
	//Check if the interaction ended 
	[SerializeField] private VoidEventChannelSO _onInteractionEnded = default;

	private void OnEnable()
	{
		_inputReader.interactEvent += OnInteractionButtonPress;
		_onInteractionEnded.OnEventRaised += OnInteractionEnd;
	}

	private void OnDisable()
	{
		_inputReader.interactEvent -= OnInteractionButtonPress;
		_onInteractionEnded.OnEventRaised -= OnInteractionEnd;
	}

	//When the interaction ends, we still want to display the interaction UI if we are still in the trigger zone
	void OnInteractionEnd()
	{
		_inputReader.EnableGameplayInput();
		switch (_potentialInteraction)
		{
			//we show it after cooking or talking, in case player want to interact again
			case Interaction.Cook:
			case Interaction.Talk:
				_toggleInteractionUI.RaiseEvent(true, _potentialInteraction);
				Debug.Log("Display interaction UI");
				break;
			default:
				break;
		}
	}

	void OnInteractionButtonPress()
	{
		switch (_potentialInteraction)
		{
			case Interaction.None:
				return;
			case Interaction.PickUp:
				if(_currentInteractableObject != null)
				{
					//raise an event with an item as parameter (to add object to inventory)
					//Item currentItem = currentInteractableObject.GetComponent<Pickable>.item;
					//_onObjectPickUp.RaiseEvent(currentItem);
					Debug.Log("PickUp event raised");
					//set current interaction for state machine
					currentInteraction = Interaction.PickUp;
				}
				//destroy the GO
				Destroy(_currentInteractableObject);
				break;
			case Interaction.Cook:
				_onCookingStart.RaiseEvent();
				Debug.Log("Cooking event raised");
				//Change the action map
				_inputReader.EnableUIInput();
				//set current interaction for state machine
				currentInteraction = Interaction.Cook;
				break;
			case Interaction.Talk:
				if (_currentInteractableObject != null)
				{
					//raise an event with an actor as parameter
					//Item currentItem = currentInteractableObject.GetComponent<Dialogue>.Actor;
					//_startTalking.RaiseEvent(currentItem);
					Debug.Log("talk event raised");
					//Change the action map
					_inputReader.EnableDialogueInput();
					//set current interaction for state machine
					currentInteraction = Interaction.Talk;
				}
				break;
			default:
				break;
		}
	}

	private void OnTriggerEnter(Collider other)
	{
		if (other.CompareTag("Pickable"))
		{
			_potentialInteraction = Interaction.PickUp;
			Debug.Log("I triggered a pickable object!");
			DisplayInteractionUI();
		}
		else if (other.CompareTag("CookingPot"))
		{
			_potentialInteraction = Interaction.Cook;
			Debug.Log("I triggered a cooking pot!");
			DisplayInteractionUI();
		}
		else if (other.CompareTag("NPC"))
		{
			_potentialInteraction = Interaction.Talk;
			Debug.Log("I triggered an NPC!");
			DisplayInteractionUI();
		}
		_currentInteractableObject = other.gameObject;
	}

	private void DisplayInteractionUI ()
	{
		//Raise event to display UI
		_toggleInteractionUI.RaiseEvent(true, _potentialInteraction);
	}

	private void OnTriggerExit(Collider other)
	{
		ResetInteraction();
	}

	private void ResetInteraction()
	{
		_potentialInteraction = Interaction.None;
		_currentInteractableObject = null;
	}
}
