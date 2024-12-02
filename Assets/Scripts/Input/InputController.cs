using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace SMM.Input
{
    public class InputController : MonoBehaviour
    {
        private InputActions inputActions = null;
        private Vector2 mousePosition = Vector2.zero;


        public Vector2 MousePosition => mousePosition;

        public event Action Place;


        protected void Awake()
        {
            inputActions = new InputActions();
        }

        protected void OnEnable()
        {
            inputActions.InGame.Enable();
            inputActions.InGame.Move.performed += OnMove;
            inputActions.InGame.Place.performed += OnPlace;
        }

        protected void OnDisable()
        {
            inputActions.InGame.Move.performed -= OnMove;
            inputActions.InGame.Place.performed -= OnPlace;
            inputActions.InGame.Disable();
            inputActions.Dispose();
            inputActions = null;
        }


        private void OnMove(InputAction.CallbackContext context)
        {
            mousePosition = context.ReadValue<Vector2>();
        }

        private void OnPlace(InputAction.CallbackContext context)
        {
            Place?.Invoke();
        }
    }
}
