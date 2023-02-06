using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimator : MonoBehaviour
{
    Animator _animator;
    Rigidbody2D _rb;
    bool hasSpeed = false;

    // Start is called before the first frame update
    void Start()
    {
        _animator = GetComponent<Animator>();
        _rb = GetComponent<Rigidbody2D>();

        for (int i = 0; i < _animator.parameterCount; i++)
        {
            if (_animator.parameters[i].name == "speed")
            {
                hasSpeed = true;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {

        if (hasSpeed)
        {
            _animator.SetFloat("speed", _rb.velocity.magnitude);
        }
    }
}
