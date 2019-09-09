using System;
using UnityEngine;

public class Player : MonoSingleton<Player>
{
    private PlayerController _controller;
    private ClimbingController _climber;
    private PlayerAnimator _animator;
    private Rigidbody _rigidbody;
    private PlayerTrigger _trigger;

    private BaseController _currentController;

    private void Start() {
        EventManager.PlayerEvent.OnControllerOverride.AddListener((NewController, Uninteruptable) => {
            if (NewController != null) {
                if (NewController != _currentController) {
                    NewController.Resume();
                    if (_currentController != null)
                        _currentController.Suspend();
                }

                _currentController = NewController;
                print("Switching PlayerController To: " + _currentController);
            }
            else
                EventManager.PlayerEvent.OnControllerOverride.Invoke(this.Controller, false);

        });

        if (EventManager.PlayerEvent.OnControllerOverride != null)
            EventManager.PlayerEvent.OnControllerOverride.Invoke(Controller, false);
    }

    private void Update() {
        if (_currentController == null)
            _currentController = Controller;

        _currentController.Step();
    }

    public PlayerController Controller {
        get {
            if (_controller == null)
                _controller = GetComponentInChildren<PlayerController>();
            return _controller;
        }
    }

    public ClimbingController Climber {
        get {
            if (_climber == null)
                _climber = GetComponentInChildren<ClimbingController>();
            return _climber;
        }
    }

    public PlayerAnimator Animator {
        get {
            if (_animator == null)
                _animator = GetComponentInChildren<PlayerAnimator>();
            return _animator;
        }
    }

    public Rigidbody Rigidbody {
        get {
            if (_rigidbody == null)
                _rigidbody = GetComponentInChildren<Rigidbody>();
            return _rigidbody;
        }
    }

    public PlayerTrigger Trigger {
        get {
            if (_trigger == null)
                _trigger = GetComponentInChildren<PlayerTrigger>();
            return _trigger;
        }
    }
}