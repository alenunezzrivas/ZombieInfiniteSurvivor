using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class PlayerAnimator : MonoBehaviour
{
    // reference to the animator component
    private Animator _animator;
    
    // state of the player
    private bool _moving;
    private bool _shooting;
    private bool _dribbling;
    private bool _accuaticTerrain;
    
    // states of movements
    private float _velocity = 0f;
    private float _acceleration = 0.1f;
    private float _deceleration = 0.2f;
    
    // get the animator and rigidbody components
    private void Awake()
    {
        _animator = GetComponent<Animator>();
    }

    // Input handling
    private void FixedUpdate()
    {
        // Update Inputs
        // get the horizontal and vertical input
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        
        // Update States
        // set the state "Moving" to true if the player is moving
        _moving = horizontal != 0 || vertical != 0;
        // Update Velocity
        _velocity += _moving ? _acceleration * Time.deltaTime: -_deceleration * Time.deltaTime;
        _velocity = Mathf.Clamp(_velocity, 0, 1);
        // set the state "Shooting" to true if the player is shooting
        _shooting = Input.GetKey(KeyCode.Z);
        // set the state "Dribbling" to true if the player is dribbling
        _dribbling = Input.GetKey(KeyCode.X);
        
        // set the animator parameter "Walking" to true if the player is moving
        _animator.SetBool("Walking", _moving);
        
        // set the animator parameter "Velocity" to the velocity
        _animator.SetFloat("Velocity", _velocity);
        
        // set the animator parameter "Shooting" to true if the player is shooting
        _animator.SetBool("Shooting", _shooting);
        
        // set the animator parameter "Dribbling" to true if the player is dribbling
        _animator.SetBool("Dribbling", _dribbling);
        
        // set the weight of the layer "LayerSync" in the animator to 1 if the player is on accuatic terrain
        _animator.SetLayerWeight(_animator.GetLayerIndex("LayerSync"), _accuaticTerrain ? 1 : 0);
    }

    private void Update()
    {
        // set the state "AccuaticTerrain" to true if the player is on accuatic terrain (simulate with key "C")
        if (Input.GetKeyDown(KeyCode.C))
            _accuaticTerrain = !_accuaticTerrain;
    }
}