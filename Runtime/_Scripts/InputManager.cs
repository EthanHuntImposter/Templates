using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEditor.VersionControl;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

namespace Template {
    public class InputManager : MonoBehaviour {
        [HideInInspector] public static InputManager instance;

        private void Awake() {
            if (instance == null) {
                instance = this;
            }
            else {
                Destroy(gameObject);
            }
        }

        private void Start() {
            playerInput = GetComponent<PlayerInput>();
            ScriptManager.instance.InputManager = this;

            GetInputActions();
        }

        private void Update() {
            ExecuteInputs();
        }

        [Header("Controls")]
        PlayerInput playerInput;

        [SerializeField] public InputAction Move;
        [SerializeField] public InputAction Settings;
        [SerializeField] public InputAction Inventory;

        public Vector2 Walking { get; private set; }
        public bool SettingsMenu { get; private set; }
        public bool InventoryMenu { get; private set; }

        //Get reference to inputs from player input comp via string
        //***Will break if name of control changes***
        void GetInputActions() {
            Move = playerInput.actions["Move"];
            Settings = playerInput.actions["Settings"];
            Inventory = playerInput.actions["Inventory"];
        }

        //Checks input actions every frame
        void ExecuteInputs() {
            Walking = Move.ReadValue<Vector2>();
            SettingsMenu = Settings.WasPressedThisFrame();
            InventoryMenu = Inventory.WasPressedThisFrame();
        }

        //Lets player input new control and changes binding
        //No mouse to avoid lmb closing menu when clicking remap button

        //Remaps composite bindings
        public void RemapBinding(ClickEvent ev, InputAction action) {
            action.Disable();

            ShowRemapWarning();

            action.PerformInteractiveRebinding(bindingIndex)
                .WithCancelingThrough("<Keyboard>/delete")
                .WithControlsExcluding("Mouse")
                .OnCancel(op => { ShowRemapWarning(); op.Dispose(); })
                .OnComplete(op => { ShowRemapWarning(); NewRemapKey(action.bindings[bindingIndex].ToDisplayString().ToUpper()); bindingIndex = 0; op.Dispose(); })
                .Start();

            action.Enable();
            //Reset binding index to 0 for non composite controls
        }
        //Gets reference to correct index in composite controls 
        //0 refers to parent 2d vector action - counts actual control bindings from 1
        public void RemapBindingCompositeIndex(ClickEvent ev, int index) {
            bindingIndex = index;
        }
        int bindingIndex = 0;

        //Actions to affect settings ui and show that a button is being remapped
        public static event Action ShowRemapWarning;
        public static event Action<String> NewRemapKey;

        //Input action custom json save bindings
        public string InputsSave() {
            return playerInput.actions.SaveBindingOverridesAsJson();
        }

        //Input action custom json load bindings
        public void InputsLoad(string json) {
            playerInput.actions.LoadBindingOverridesFromJson(json);
        }
    }
}
